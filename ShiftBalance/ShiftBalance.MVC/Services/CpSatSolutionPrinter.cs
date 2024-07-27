using Google.OrTools.Sat;
using ShiftBalance.MVC.Excel;
using ShiftBalance.MVC.Models;
using System.Diagnostics;

namespace ShiftBalance.MVC.Services
{
    public class CpSatSolutionPrinter : CpSolverSolutionCallback
    {
        private List<Employee> _workers;
        private Dictionary<int, DateTime> _calendar;
        private int[] _allDipendenti;
        private int[] _allDays;
        private int[] _allShifts;
        private Dictionary<(int, int, int), BoolVar> _shifts;

        public CpSatSolutionPrinter(List<Employee> workers, Dictionary<int, DateTime> calendar, int[] allDipendenti, int[] allDays, int[] allShifts, Dictionary<(int, int, int), BoolVar> shifts)
        {
            _workers = workers;
            _calendar = calendar;
            _allDipendenti = allDipendenti;
            _allDays = allDays;
            _allShifts = allShifts;
            _shifts = shifts;
        }

        // Genera l'excel con i giorni come colonne e dipendenti come righe
        public void OnSolutionCallback(bool pippo)
        {
            ShiftMatrix openings = new(_allDipendenti.Length, _allDays.Length);
            ShiftMatrix closeings = new(_allDipendenti.Length, _allDays.Length);
            ShiftMatrix availability = new(_allDipendenti.Length, _allDays.Length);
            PopulateMatrixes(openings, closeings, availability);

            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ShiftBalance", $"plan_CpSat_{DateTime.UtcNow:yyyyMMddddHHmmss}");
            ShiftWorksheet worksheet = new(_workers, openings, closeings, availability, _calendar);

            worksheet.Generate(filePath);
            Process.Start(filePath);
        }

        private void PopulateMatrixes(ShiftMatrix openings, ShiftMatrix closeings, ShiftMatrix availability)
        {
            for (int i = 0; i < _allDipendenti.Length; i++)
            {
                for (int j = 0; j < _allDays.Length; j++)
                {
                    // Reperibilità?
                    if (IsAvailability(j))
                    {
                        openings.Matrix[i, j] = 0;
                        closeings.Matrix[i, j] = 0;
                        availability.Matrix[i, j] = (Value(_shifts[(i, j, 1)]) > 0 || Value(_shifts[(i, j, 2)]) > 0) ? 1 : 0;
                    }
                    else
                    {
                        availability.Matrix[i, j] = 0;
                        openings.Matrix[i, j] = (Value(_shifts[(i, j, 1)]) > 0) ? 1 : 0;
                        closeings.Matrix[i, j] = (Value(_shifts[(i, j, 2)]) > 0) ? 1 : 0;
                    }
                }
            }
        }

        public override void OnSolutionCallback()
        {
            foreach (int d in _allDays)
            {
                Console.WriteLine($"Day {d}");
                foreach (int n in _allDipendenti)
                {
                    foreach (int s in _allShifts)
                    {
                        if (Value(_shifts[(n, d, s)]) == 1L)
                        {
                            Console.WriteLine($"  Dipendente {n} work shift {s}");
                        }
                    }
                }
            }
            StopSearch();
        }

        private bool IsAvailability(int dayNumber)
        {
            var date = _calendar[dayNumber];

            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                return true;
            }
            return false;
        }
    }
}
