using Google.OrTools.Sat;
using ShiftBalance.MVC.Models;

namespace ShiftBalance.MVC.Services
{
    public class CpSatSolver
    {
        // Input data
        private readonly List<Employee> _workers;
        private readonly DateTime _startShift;
        private readonly DateTime _endShift;

        // Constants
        private const int ShiftsPerDay = 2;

        public CpSatSolver(List<Employee> workers, DateTime startShift, DateTime endShift)
        {
            _workers = workers;
            _startShift = startShift;
            _endShift = endShift;
        }

        public CpSolverStatus Solve()
        {
            // DATA
            int numberOfWorkers = _workers.Count;
            int numberOfDays = (int)_endShift.Subtract(_startShift).TotalDays + 1;

            Dictionary<int, DateTime> calendar = GetCalendarMap(numberOfDays);

            int[] allWorkers = GetWorkersInSchedule();
            int[] allDays = GetDaysInSchedule();
            int[] allShifts = GetShiftsPerDay();

            // medie turni
            int[] previousShiftAverageOpening = GetOpeningsAverage();
            int[] previousShiftAverageClosing = GetClosingsAverage();
            int[] previousShiftAverageHoliday = GetAvailabilityAverage();

            // Vacanze
            int[,] vacations = GetVacations(numberOfWorkers, numberOfDays);

            // #### MODELLO ####
            CpModel model = new();
            model.Model.Variables.Capacity = numberOfWorkers * numberOfDays * ShiftsPerDay;

            // variabili
            Dictionary<(int, int, int), BoolVar> shifts = new(numberOfWorkers * numberOfDays * ShiftsPerDay);
            foreach (int n in allWorkers)
            {
                foreach (int d in allDays)
                {
                    foreach (int s in allShifts)
                    {
                        shifts.Add((n, d, s), model.NewBoolVar($"shifts_n{n}d{d}s{s}"));
                    }
                }
            }

            // Vincoli: ogni turno deve avere esattamente un dipendente
            List<ILiteral> literals = new();
            foreach (int d in allDays)
            {
                foreach (int s in allShifts)
                {
                    literals.Clear();
                    foreach (int n in allWorkers)
                    {
                        literals.Add(shifts[(n, d, s)]);
                    }
                    model.AddExactlyOne(literals);
                }
            }

            // Vincoli: ogni dipendente può lavorare al massimo un turno al giorno
            foreach (int n in allWorkers)
            {
                foreach (int d in allDays)
                {
                    literals.Clear();
                    foreach (int s in allShifts)
                    {
                        literals.Add(shifts[(n, d, s)]);
                    }
                    model.AddAtMostOne(literals);
                }
            }

            // Vincoli: dipendenti non possono lavorare durante le ferie
            for (int n = 0; n < allWorkers.Length; n++)
            {
                for (int d = 0; d < allDays.Length; d++)
                {
                    if (vacations[n, d] == 1)
                    {
                        foreach (int s in allShifts)
                        {
                            if (n < allWorkers.Length && d < allDays.Length)
                            {
                                model.Add(shifts[(n + 1, d + 1, s)] == 0);
                            }
                        }
                    }
                }
            }

            //// Vincoli: un dipendente non può lavorare dopo una reperibilità festiva
            for (int n = 0; n < numberOfWorkers; n++)
            {
                for (int d = 0; d < numberOfDays; d++)
                {
                    // giorno reperibilità con controllo bounds
                    if (IsAvailability(calendar[d]) && (d + 2 < numberOfDays))
                    {
                        // variabile ausiliaria per stabilire se lavora o meno il weekend
                        BoolVar worksWeekend = model.NewBoolVar($"works_weekend_n{n}_d{d}");

                        // Constraints
                        model.AddMaxEquality(worksWeekend,
                            [
                                shifts[(n + 1, d + 1, 1)], shifts[(n + 1, d + 1, 2)],  // Sabato
                                shifts[(n + 1, d + 2, 1)], shifts[(n + 1, d + 2, 2)]   // Domenica
                            ]
                        );
                        model.Add(shifts[(n + 1, d + 3, 1)] + shifts[(n + 1, d + 3, 2)] <= (1 - worksWeekend)
                        );
                    }
                }
            }

            int avgOpening = (int)Math.Round(previousShiftAverageOpening.Average() * 0.85, MidpointRounding.AwayFromZero);
            int avgClosing = (int)Math.Round(previousShiftAverageClosing.Average() * 0.85, MidpointRounding.AwayFromZero);
            int avgHoliday = (int)Math.Round(previousShiftAverageHoliday.Average() * 0.85,MidpointRounding.AwayFromZero);

            List<Google.OrTools.Sat.LinearExpr> penalties = new List<Google.OrTools.Sat.LinearExpr>();

            foreach (int n in Enumerable.Range(1, numberOfWorkers))
            {
                Google.OrTools.Sat.LinearExpr openingShifts = Google.OrTools.Sat.LinearExpr.Sum(
                    Enumerable.Range(1, numberOfDays).Select(d => shifts[(n, d, 1)])
                );
                Google.OrTools.Sat.LinearExpr closingShifts = Google.OrTools.Sat.LinearExpr.Sum(
                    Enumerable.Range(1, numberOfDays).Select(d => shifts[(n, d, 2)])
                );
                Google.OrTools.Sat.LinearExpr holidayShifts = Google.OrTools.Sat.LinearExpr.Sum(
                    Enumerable.Range(1, numberOfDays).Where(d => IsAvailability(calendar[d - 1])).SelectMany(d => allShifts.Select(s => shifts[(n, d, s)]))
                );

                // Creare variabili per rappresentare il deficit rispetto ai target
                int targetOpening = Math.Max(0, avgOpening+1 - previousShiftAverageOpening[n - 1]);
                int targetClosing = Math.Max(0, avgClosing+1 - previousShiftAverageClosing[n - 1]);
                int targetHoliday = Math.Max(0, avgHoliday+2 - previousShiftAverageHoliday[n - 1]);

                IntVar openingDeficit = model.NewIntVar(0, targetOpening, $"openingDeficit{n}");
                IntVar closingDeficit = model.NewIntVar(0, targetClosing, $"closingDeficit{n}");
                IntVar holidayDeficit = model.NewIntVar(0, targetHoliday, $"holidayDeficit{n}");

                // Aggiungere vincoli che collegano le variabili di deficit con i turni effettivi
                model.Add(openingDeficit > targetOpening - openingShifts);
                model.Add(closingDeficit > targetClosing - closingShifts);
                model.Add(holidayDeficit > targetHoliday - holidayShifts);

                // Aggiungere le variabili di deficit alle penalità
                penalties.Add(openingDeficit);
                penalties.Add(closingDeficit);
                penalties.Add(holidayDeficit);
            }

            // Minimizzare la somma delle penalità
            Google.OrTools.Sat.LinearExpr totalPenalty = Google.OrTools.Sat.LinearExpr.Sum(penalties);
            model.Minimize(totalPenalty);

            //Soluzione
            CpSolver solver = new CpSolver();
            solver.StringParameters = "linearization_level:2";

            CpSatSolutionPrinter cb = new CpSatSolutionPrinter(_workers, calendar, allWorkers, allDays, allShifts, shifts, vacations);
            CpSolverStatus status = solver.Solve(model, cb);

            return status;
        }

        // Inizializzo la mappa giorno/data
        private Dictionary<int, DateTime> GetCalendarMap(int numberOfDays)
        {
            int day = 0;
            Dictionary<int, DateTime> calendarMap = [];

            while (calendarMap.Count < numberOfDays)
            {
                calendarMap.Add(day, _startShift.AddDays(day));
                day++;
            }
            return calendarMap;
        }

        // è un giorno di reperibilità?
        private static bool IsAvailability(DateTime day)
        {
            if (day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static int[] GetShiftsPerDay()
        {
            return Enumerable.Range(1, 2).ToArray();
        }

        private int[] GetDaysInSchedule()
        {
            var numberOfDays = _endShift.Subtract(_startShift).TotalDays + 1;

            return Enumerable.Range(1, (int)numberOfDays).ToArray();
        }

        private int[] GetWorkersInSchedule()
        {
            return Enumerable.Range(1, _workers.Count).ToArray();
        }

        // Media delle aperture
        private int[] GetOpeningsAverage()
        {
            int[] averages = new int[_workers.Count];

            for (int i = 0; i < averages.Length; i++)
            {
                averages[i] = _workers[i].Openings;
            }
            return averages;
        }

        // media delle chiusure
        private int[] GetClosingsAverage()
        {
            int[] averages = new int[_workers.Count];

            for (int i = 0; i < averages.Length; i++)
            {
                averages[i] = _workers[i].Closings;
            }
            return averages;
        }

        // Media reperibilità
        private int[] GetAvailabilityAverage()
        {
            int[] averages = new int[_workers.Count];

            for (int i = 0; i < averages.Length; i++)
            {
                averages[i] = _workers[i].Availability;
            }
            return averages;
        }


        // Setta le ferie e i superfestivi
        private int[,] GetVacations(int numberOfWorkers, int numberOfDays)
        {
            int[,] vacations = new int[numberOfWorkers, numberOfDays];
            List<Employee> employeesWithVacations = _workers.Where(x => x.Vacations != null).ToList();

            for (int i = 0; i < numberOfDays; i++)
            {
                DateTime day = _startShift.AddDays(i);

                // il giorno dell'anno è un superfestivo assegnato o vacanza
                for (int j = 0; j < numberOfWorkers; j++)
                {
                    if (employeesWithVacations.Exists(x => x.Id == j + 1))
                    {
                        Employee emp = employeesWithVacations.Where(y => y.Id == j + 1).First();

                        if (emp.Vacations.Any(x => x.StartDate == day || (day > x.StartDate && day < x.EndDate)))
                        {
                            vacations[j, i] = 1;
                        }
                        else
                        {
                            vacations[j, i] = 0;
                        }
                    }
                }
            }
            return vacations;
        }
    }
}
