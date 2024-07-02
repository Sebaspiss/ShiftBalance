using Google.OrTools.Sat;
using Microsoft.AspNetCore.Mvc;
using ShiftBalance.MVC.Services;

namespace ShiftBalance.MVC.Controllers
{
    public class CpSatSolverController : Controller
    {
        public IActionResult Index()
        {
            //DATA
            int dipendenti = 13;
            int giorni = 92;
            int turni = 2;

            int[] allDipendenti = Enumerable.Range(1, dipendenti).ToArray();
            int[] allGiorni = Enumerable.Range(1, giorni).ToArray();
            int[] allTurni = Enumerable.Range(1, turni).ToArray();

            //MODELLO
            CpModel model = new();
            model.Model.Variables.Capacity = dipendenti * giorni * turni;

            // variabili
            Dictionary<(int, int, int), BoolVar> shifts = new(dipendenti * giorni * turni);
            foreach (int n in allDipendenti)
            {
                foreach (int d in allGiorni)
                {
                    foreach (int s in allTurni)
                    {
                        shifts.Add((n, d, s), model.NewBoolVar($"shifts_n{n}d{d}s{s}"));
                    }
                }
            }

            // Media dei turni precedentemente lavorati per ogni dipendente
            int[] mediaTurniPrecedenti = { 26, 29, 29, 27, 32, 28, 22, 31, 28, 23, 27, 23, 24 };

            // Disponibilità
            int[,] ferie = new int[dipendenti, giorni];
            // Dipendente 1 in ferie dal 10 al 15
            for (int d = 9; d <= 14; d++)
            {
                ferie[1, d] = 1;
            }

            // Definizione dei giorni festivi (1 indica festivo, 0 indica giorno lavorativo)
            int[] festivi = new int[giorni];
            for (int d = 0; d < giorni; d++)
            {
                var date = new DateTime(2024, 3, 1).AddDays(d);
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    festivi[d] = 1;
                }
            }
            // Aggiunta di festività speciali
            festivi[1] = 1; // 1 Aprile 2024 - Pasquetta
            festivi[25] = 1; // 25 Aprile 2024 - Festa della Liberazione
            festivi[30] = 1; // 1 Maggio 2024 - Festa dei Lavoratori


            // Vincoli: ogni turno deve avere esattamente un dipendente
            List<ILiteral> literals = new();
            foreach (int d in allGiorni)
            {
                foreach (int s in allTurni)
                {
                    foreach (int n in allDipendenti)
                    {
                        literals.Add(shifts[(n, d, s)]);
                    }
                    model.AddExactlyOne(literals);
                    literals.Clear();
                }
            }

            // Vincoli: ogni dipendente può lavorare al massimo un turno al giorno
            foreach (int n in allDipendenti)
            {
                foreach (int d in allGiorni)
                {
                    foreach (int s in allTurni)
                    {
                        literals.Add(shifts[(n, d, s)]);
                    }
                    model.AddAtMostOne(literals);
                    literals.Clear();
                }
            }

            // Vincoli: dipendenti non possono lavorare durante le ferie
            for (int n = 0; n < allDipendenti.Length; n++)
            {
                for (int d = 0; d < allGiorni.Length; d++)
                {
                    if (ferie[n, d] == 1)
                    {
                        foreach (int s in allTurni)
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
            int minTurniPerDipendente = (turni * giorni) / dipendenti;
            int maxTurniPerDipendente;
            if ((turni * giorni) % dipendenti == 0)
            {
                maxTurniPerDipendente = minTurniPerDipendente;
            }
            else
            {
                maxTurniPerDipendente = minTurniPerDipendente + 1;
            }

            List<IntVar> shiftsWorked = new();
            foreach (int n in allDipendenti)
            {
                foreach (int d in allGiorni)
                {
                    foreach (int s in allTurni)
                    {
                        shiftsWorked.Add(shifts[(n, d, s)]);
                    }
                }
                model.AddLinearConstraint(LinearExpr.Sum(shiftsWorked), minTurniPerDipendente, maxTurniPerDipendente);
                shiftsWorked.Clear();
            }

            // Vincoli: un dipendente non può lavorare dopo una reperibilità festiva
            for (int n = 0; n < dipendenti; n++)
            {
                for (int d = 0; d < giorni - 1; d++)
                {
                    if (festivi[d] == 1)
                    {
                        foreach (int s in allTurni)
                        {
                            model.Add(shifts[(n + 1, d + 1, s)] + shifts[(n + 1, d + 2, s)] <= 1);
                        }
                    }
                }
            }

            // Funzione obiettivo: minimizzare l'assegnazione di turni in base alla media dei turni precedenti
            LinearExpr objective = LinearExpr.Sum(Enumerable.Range(1, dipendenti).SelectMany(n =>
                                                Enumerable.Range(1, giorni).SelectMany(d => 
                                                Enumerable.Range(1, turni).Select(s => 
                                                shifts[(n, d, s)] * mediaTurniPrecedenti[n - 1]))));

            model.Minimize(objective);

            //Soluzione
            CpSolver solver = new CpSolver();
           solver.StringParameters += "linearization_level:2";

            SolutionPrinter cb = new SolutionPrinter(allDipendenti, allGiorni, allTurni, shifts);
            CpSolverStatus status = solver.Solve(model, cb);

            Console.WriteLine($"Solve status: {status}");

            return View();
        }
    }
}
