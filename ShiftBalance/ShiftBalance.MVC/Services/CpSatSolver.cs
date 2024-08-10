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

        // Media dei turni precedentemente lavorati per ogni dipendente
        private int[] GetShiftsAverage()
        {
            int[] averages = new int[_workers.Count];

            for (int i = 0; i < averages.Length; i++)
            {
                averages[i] = _workers[i].ShiftAverage;
            }
            return averages;
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

        // Definizione dei giorni festivi (1 indica festivo, 0 indica giorno lavorativo)
        private int[] GetAvailability(int numberOfDays)
        {
            int[] availability = new int[numberOfDays];

            for (int d = 0; d < numberOfDays; d++)
            {
                var date = _startShift.AddDays(d);

                if (IsAvailability(date))
                {
                    availability[d] = 1;
                }
            }
            return availability;
        }

        public CpSolverStatus Solve()
        {
            //DATA
            int numberOfWorkers = _workers.Count;
            int numberOfDays = (int)_endShift.Subtract(_startShift).TotalDays + 1;
            Dictionary<int, DateTime> calendar = GetCalendarMap(numberOfDays);

            int[] allWorkers = GetWorkersInSchedule();
            int[] allDays = GetDaysInSchedule();
            int[] allShifts = GetShiftsPerDay();

            //MODELLO
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

            // medie turni
            int[] previousShiftAverageOpening = GetOpeningsAverage();
            int[] previousShiftAverageClosing = GetClosingsAverage();
            int[] previousShiftAverageHoliday = GetAvailabilityAverage();

            // Vacanze
            int[,] vacations = GetVacations(numberOfWorkers, numberOfDays);
            int[] availability = GetAvailability(numberOfDays);


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
                    // Check if the day is Saturday or Sunday and ensure it doesn't go out of bounds
                    if ((IsAvailability(calendar[d])) && (d + 2 < numberOfDays))
                    {
                        // Auxiliary variable to check if the worker works on Saturday or Sunday
                        BoolVar worksWeekend = model.NewBoolVar($"works_weekend_n{n}_d{d}");

                        // Constraints to set the value of worksWeekend
                        model.AddMaxEquality(
                            worksWeekend,
                            new IntVar[] {
                    shifts[(n + 1, d + 1, 1)], shifts[(n + 1, d + 1, 2)],  // Shifts on Saturday
                    shifts[(n + 1, d + 2, 1)], shifts[(n + 1, d + 2, 2)]   // Shifts on Sunday
                            }
                        );

                        // Constraint to ensure no work on Monday if worked on the weekend
                        model.Add(shifts[(n + 1, d + 3, 1)] + shifts[(n + 1, d + 3, 2)] <= (1 - worksWeekend)
                        );
                    }
                }
            }

            // Calculate normalization factors (maximum number of shifts any worker has had)
            int maxOpeningShifts = previousShiftAverageOpening.Max();
            int maxClosingShifts = previousShiftAverageClosing.Max();
            int maxHolidayShifts = previousShiftAverageHoliday.Max();

            // FUNZIONE OBIETTIVO
            List<Google.OrTools.Sat.LinearExpr> objectiveTerms = new List<Google.OrTools.Sat.LinearExpr>();

            foreach (int n in Enumerable.Range(1, numberOfWorkers))
            {
                // Subtract the worker's shift average from the max to give more shifts to those with less
                int openingWeight = maxOpeningShifts - previousShiftAverageOpening[n - 1];
                int closingWeight = maxClosingShifts - previousShiftAverageClosing[n - 1];
                int holidayWeight = maxHolidayShifts - previousShiftAverageHoliday[n - 1];

                foreach (int d in Enumerable.Range(1, numberOfDays))
                {
                    // Add weighted terms to the objective function
                    objectiveTerms.Add(shifts[(n, d, 1)] * openingWeight);
                    objectiveTerms.Add(shifts[(n, d, 2)] * closingWeight);

                    // Handle holiday shifts, if the day is a holiday
                    if (IsAvailability(calendar[d - 1]))
                    {
                        foreach (int s in allShifts)  // Assuming allShifts includes holidays at specific indices
                        {
                            objectiveTerms.Add(shifts[(n, d, s)] * holidayWeight);
                        }
                    }
                }
            }
            // Somma tutte le espressioni lineari per costruire l'obiettivo finale
            Google.OrTools.Sat.LinearExpr objective = Google.OrTools.Sat.LinearExpr.Sum(objectiveTerms);

            // Aggiungi la funzione obiettivo al modello
            model.Minimize(objective);

            //Soluzione
            CpSolver solver = new CpSolver();
            solver.StringParameters += "linearization_level:2";

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
    }
}
