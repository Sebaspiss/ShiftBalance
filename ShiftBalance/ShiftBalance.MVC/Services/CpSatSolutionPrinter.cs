using Google.OrTools.Sat;
using ShiftBalance.MVC.Excel;
using ShiftBalance.MVC.Models;

namespace ShiftBalance.MVC.Services
{
    public class CpSatSolutionPrinter : CpSolverSolutionCallback
    {
        private List<Employee> _workers;
        private Dictionary<int, DateTime> _calendar;
        private int[] _allDipendenti;
        private int[] _allDays;
        private int[] _allShifts;
        private int[,] _allHolidays;
        private Dictionary<(int, int, int), BoolVar> _shifts;

        public CpSatSolutionPrinter(List<Employee> workers, Dictionary<int, DateTime> calendar, int[] allDipendenti, int[] allDays, int[] allShifts, Dictionary<(int, int, int), BoolVar> shifts, int[,] employeeHolidays)
        {
            _workers = workers;
            _calendar = calendar;
            _allDipendenti = allDipendenti;
            _allDays = allDays;
            _allShifts = allShifts;
            _shifts = shifts;
            _allHolidays = employeeHolidays;
        }

        // Genera l'excel con i giorni come colonne e dipendenti come righe
        public override void OnSolutionCallback()
        {
            ShiftMatrix openings = new(_allDipendenti.Length, _allDays.Length);
            ShiftMatrix closeings = new(_allDipendenti.Length, _allDays.Length);
            ShiftMatrix availability = new(_allDipendenti.Length, _allDays.Length);
            ShiftMatrix holidays = new(_allDipendenti.Length, _allDays.Length);
            PopulateMatrixes(openings, closeings, availability);
            PopulateHolidays(holidays);

            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ShiftBalance", $"plan_CpSat_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
            ShiftWorksheet worksheet = new(_workers, openings, closeings, availability, _calendar,holidays);

            worksheet.Generate(filePath);
            StopSearch();

            ExcelProcess.OpenFile(filePath);
        }

        private void PopulateHolidays(ShiftMatrix holidays)
        {
            for (int i = 0; i < _allDipendenti.Length; i++)
            {
                for (int j = 0; j < _allDays.Length; j++)
                {
                    holidays.Matrix[i,j] = _allHolidays[i,j];
                }
            }
        }

        private void PopulateMatrixes(ShiftMatrix openings, ShiftMatrix closeings, ShiftMatrix availability)
        {
            for (int i = 0; i < _allDipendenti.Length; i++)
            {
                for (int j = 0; j < _allDays.Length; j++)
                {
                    // Reperibilità?
                    if (IsAvailabilityOrNationalHoliday(j))
                    {
                        openings.Matrix[i, j] = 0;
                        closeings.Matrix[i, j] = 0;
                        availability.Matrix[i, j] = (Value(_shifts[(i + 1, j + 1, 1)]) > 0 || Value(_shifts[(i + 1, j + 1, 2)]) > 0) ? 1 : 0;
                    }
                    else
                    {
                        availability.Matrix[i, j] = 0;
                        openings.Matrix[i, j] = (Value(_shifts[(i + 1, j + 1, 1)]) > 0) ? 1 : 0;
                        closeings.Matrix[i, j] = (Value(_shifts[(i + 1, j + 1, 2)]) > 0) ? 1 : 0;
                    }
                }
            }
        }

        private bool IsAvailabilityOrNationalHoliday(int dayNumber)
        {
            var date = _calendar[dayNumber];

            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday || CalendarFunctions.IsNationalHoliday(date))
            {
                return true;
            }
            return false;
        }      
    }
}
