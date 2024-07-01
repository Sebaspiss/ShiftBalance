using Google.OrTools.LinearSolver;
using Microsoft.AspNetCore.Mvc;

namespace ShiftBalance.MVC.Controllers
{
    public class SolverController : Controller
    {
        public IActionResult Index()
        {
            int numDipendenti = 10;
            int numGiorniMarzo = 31;
            int numGiorniAprile = 30;
            int numGiorniMaggio = 31;
            int numGiorniTotali = numGiorniMarzo + numGiorniAprile + numGiorniMaggio;

            // Definizione dei giorni festivi e delle disponibilità (ferie) come matrici binarie
            int[,] disponibilita = new int[numDipendenti, numGiorniTotali];
            int[] festivi = new int[numGiorniTotali];

            // Inizializzazione delle matrici (esempio)
            for (int i = 0; i < numDipendenti; i++)
            {
                for (int j = 0; j < numGiorniTotali; j++)
                {
                    disponibilita[i, j] = 1; // Tutti disponibili inizialmente
                }
            }

            // Esempio di ferie (dipendente 0 in ferie dal 10 al 15 Aprile)
            for (int j = 10 + numGiorniMarzo; j <= 15 + numGiorniMarzo; j++)
            {
                disponibilita[0, j] = 0;
            }

            // Inizializzazione dei giorni festivi (esempio)
            for (int j = 0; j < numGiorniTotali; j++)
            {
                // Inizializzazione per sabati e domeniche come festivi
                var date = new DateTime(2024, 3, 1).AddDays(j);
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    festivi[j] = 1;
                }
            }

            // Aggiunta di festività speciali (esempio)
            festivi[31 + 0] = 1;  // 1 Aprile 2024 - Pasquetta
            festivi[31 + 24] = 1; // 25 Aprile 2024 - Festa della Liberazione
            festivi[31 + 30 + 0] = 1; // 1 Maggio 2024 - Festa dei Lavoratori

            // Creazione del solver
            Solver solver = Solver.CreateSolver("SCIP");

            // Definizione delle variabili
            Variable[,] W = new Variable[numDipendenti, numGiorniTotali];
            for (int i = 0; i < numDipendenti; i++)
            {
                for (int j = 0; j < numGiorniTotali; j++)
                {
                    W[i, j] = solver.MakeIntVar(0, 1, $"W_{i}_{j}");
                }
            }

            // Definizione della funzione obiettivo
            Objective objective = solver.Objective();
            for (int i = 0; i < numDipendenti; i++)
            {
                int mediaTurni = 1 + i;
                for (int j = 0; j < numGiorniTotali; j++)
                {
                    objective.SetCoefficient(W[i, j], mediaTurni);
                }
            }
            objective.SetMinimization();

            // Definizione dei vincoli
            for (int j = 0; j < numGiorniTotali; j++)
            {
                // Ogni giorno devono esserci esattamente due dipendenti in turno
                Constraint singleDay = solver.MakeConstraint(2, 2, $"c1_{j}");
                for (int i = 0; i < numDipendenti; i++)
                {
                    singleDay.SetCoefficient(W[i, j], 1);
                }
            }

            for (int i = 0; i < numDipendenti; i++)
            {
                for (int j = 0; j < numGiorniTotali; j++)
                {
                    // Un dipendente non può lavorare se non è disponibile
                    solver.Add(W[i, j] <= disponibilita[i, j]);

                    // Un dipendente che lavora un giorno festivo non può lavorare il giorno successivo
                    if (j < numGiorniTotali - 1)
                    {
                        solver.Add(W[i, j] + W[i, j + 1] <= 1 - festivi[j]);
                    }
                }
            }

            // Risoluzione del problema
            Solver.ResultStatus resultStatus = solver.Solve();

            // Verifica della soluzione
            if (resultStatus == Solver.ResultStatus.OPTIMAL)
            {
                Console.WriteLine("Soluzione ottimale trovata!");
                for (int i = 0; i < numDipendenti; i++)
                {
                    for (int j = 0; j < numGiorniTotali; j++)
                    {
                        int giorno = j + 1;
                        string mese = giorno <= numGiorniMarzo ? "Marzo" : giorno <= numGiorniMarzo + numGiorniAprile ? "Aprile" : "Maggio";
                        int giornoMese = giorno <= numGiorniMarzo ? giorno : giorno <= numGiorniMarzo + numGiorniAprile ? giorno - numGiorniMarzo : giorno - numGiorniMarzo - numGiorniAprile;
                        Console.WriteLine($"Dipendente {i + 1}, {mese} {giornoMese}: {W[i, j].SolutionValue()}");
                    }
                }
            }
            else
            {
                Console.WriteLine("Non è stata trovata una soluzione ottimale.");
            }
            return Ok();
        }
    }
}