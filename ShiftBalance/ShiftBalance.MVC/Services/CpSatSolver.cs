using Google.OrTools.Sat;
using ShiftBalance.MVC.Models;
using System;

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
            var numberOfDays = _endShift.Subtract(_startShift).TotalDays;

            return Enumerable.Range(1, (int)numberOfDays).ToArray();
        }

        private int[] GetWorkersInSchedule()
        {
            return Enumerable.Range(1, _workers.Count).ToArray();
        }

        // Media dei turni precedentemente lavorati per ogni dipendente
        private int[] GetShiftsAverage()
        {
            int[] averages = new int[_workers.Count - 1];
            
            for(int i = 0; i < averages.Length; i++) 
            {
                averages[i] = _workers[i].ShiftAverage;
            }
            return averages;
        }

        // Disponibilità
        private int[,] GetVacations(int numberOfWorkers,int numberOfDays)
        {
          int[,] vacations = new int[numberOfWorkers, numberOfDays];

            // Dipendente 1 in ferie dal 10 al 15
            for (int d = 9; d <= 14; d++)
            {
                vacations[1, d] = 1;
            }
            return vacations;
        }

        // Definizione dei giorni festivi (1 indica festivo, 0 indica giorno lavorativo)
        private int[] GetHolidays(int numberOfDays)
        {
           int[] holidays = new int[numberOfDays];

            for (int d = 0; d < numberOfDays; d++)
            {
                var date = new DateTime(_startShift.Year, _startShift.Month, _startShift.Day).AddDays(d);

                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    holidays[d] = 1;
                }
            }
            // Aggiunta di festività speciali
            holidays[1] = 1; // 1 Aprile 2024 - Pasquetta
            holidays[25] = 1; // 25 Aprile 2024 - Festa della Liberazione
            holidays[30] = 1; // 1 Maggio 2024 - Festa dei Lavoratori

            return holidays;
        }

        public void Solve()
        {
            //DATA
            int numberOfWorkers = _workers.Count;
            int numberOfDays = (int)_endShift.Subtract(_startShift).TotalDays;

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

            int[] previousShiftsAverage = GetShiftsAverage();
            int[,] vacations = GetVacations(numberOfWorkers, numberOfDays);   
            int[] holidays = GetHolidays(numberOfDays);


            // Vincoli: ogni turno deve avere esattamente un dipendente
            List<ILiteral> literals = new();
            foreach (int d in allDays)
            {
                foreach (int s in allShifts)
                {
                    foreach (int n in allWorkers)
                    {
                        literals.Add(shifts[(n, d, s)]);
                    }
                    model.AddExactlyOne(literals);
                    literals.Clear();
                }
            }

            // Vincoli: ogni dipendente può lavorare al massimo un turno al giorno
            foreach (int n in allWorkers)
            {
                foreach (int d in allDays)
                {
                    foreach (int s in allShifts)
                    {
                        literals.Add(shifts[(n, d, s)]);
                    }
                    model.AddAtMostOne(literals);
                    literals.Clear();
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
                            model.Add(shifts[(n + 1, d + 1, s)] == 0);
                        }
                    }
                }
            }

            // Distribuisci equamente
            // Try to distribute the shifts evenly, so that each dipendente works
            // minTurniPerDipendente shifts. If this is not possible, because the total
            // number of shifts is not divisible by the number of dipendenti, some dipendenti will
            // be assigned one more shift.
            int minTurniPerDipendente = (ShiftsPerDay * numberOfDays) / numberOfWorkers;
            int maxTurniPerDipendente;
            if ((ShiftsPerDay * numberOfDays) % numberOfWorkers == 0)
            {
                maxTurniPerDipendente = minTurniPerDipendente;
            }
            else
            {
                maxTurniPerDipendente = minTurniPerDipendente + 1;
            }

            List<IntVar> shiftsWorked = new();
            foreach (int n in allWorkers)
            {
                foreach (int d in allDays)
                {
                    foreach (int s in allShifts)
                    {
                        shiftsWorked.Add(shifts[(n, d, s)]);
                    }
                }
                model.AddLinearConstraint(Google.OrTools.Sat.LinearExpr.Sum(shiftsWorked), minTurniPerDipendente, maxTurniPerDipendente);
                shiftsWorked.Clear();
            }

            // Vincoli: un dipendente non può lavorare dopo una reperibilità festiva
            for (int n = 0; n < numberOfWorkers; n++)
            {
                for (int d = 0; d < numberOfDays - 1; d++)
                {
                    if (holidays[d] == 1)
                    {
                        foreach (int s in allShifts)
                        {
                            model.Add(shifts[(n + 1, d + 1, s)] + shifts[(n + 1, d + 2, s)] <= 1);
                        }
                    }
                }
            }

            // Funzione obiettivo: minimizzare l'assegnazione di turni in base alla media dei turni precedenti
            Google.OrTools.Sat.LinearExpr objective = Google.OrTools.Sat.LinearExpr.Sum(Enumerable.Range(1, numberOfWorkers).SelectMany(n =>
                                                Enumerable.Range(1, numberOfDays).SelectMany(d =>
                                                Enumerable.Range(1, ShiftsPerDay).Select(s =>
                                                shifts[(n, d, s)] * previousShiftsAverage[n - 1]))));

            model.Minimize(objective);

            //Soluzione
            CpSolver solver = new CpSolver();
            solver.StringParameters += "linearization_level:2";

            CpSatSolutionPrinter cb = new CpSatSolutionPrinter(_workers, GetCalendarMap(allDays.Length), allWorkers, allDays, allShifts, shifts);
            CpSolverStatus status = solver.Solve(model, cb);

        }

        // Inizializzo la mappa giorno/data
        private Dictionary<int,DateTime> GetCalendarMap(int numberOfDays)
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
    }
}
