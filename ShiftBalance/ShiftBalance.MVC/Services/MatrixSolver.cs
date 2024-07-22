using ShiftBalance.MVC.Models;

namespace ShiftBalance.MVC.Services
{
    public class MatrixSolver
    {
        // Input
        private readonly List<Employee> _workers;
        private readonly DateTime _startDate;
        private readonly DateTime _endDate;

        private Dictionary<int, DateTime> _calendarMap;

        // Output
        private ShiftMatrix _availability;
        private ShiftMatrix _openings;
        private ShiftMatrix _closings;
        private ShiftMatrix _holidays;

        // Property
        public ShiftMatrix Availability { get => _availability; }
        public ShiftMatrix Openings { get => _openings; }
        public ShiftMatrix Closings { get => _closings; }

        public MatrixSolver(List<Employee> workers, DateTime startShift, DateTime endShift)
        {
            _workers = workers;
            _startDate = startShift;
            _endDate = endShift;
            InitializeMatrixes();
            InitializeCalendarMap();
        }

        // Genero le matrici di dimensioni => lavoratori x giorni
        private void InitializeMatrixes()
        {
            int numberOfWorkers = _workers.Count;
            int numberOfDays = GetDaysInSchedule();

            _availability = new(numberOfWorkers, numberOfDays);
            _openings = new(numberOfWorkers, numberOfDays);
            _closings = new(numberOfWorkers, numberOfDays);
            _holidays = new(numberOfWorkers, numberOfDays);
        }

        // Inizializzo la mappa giorno/data
        private void InitializeCalendarMap()
        {
            int numberOfDays = GetDaysInSchedule();
            int day = 0;

            _calendarMap = [];

            while (_calendarMap.Count < numberOfDays)
            {
                _calendarMap.Add(day, _startDate.AddDays(day));
                day++;
            }
        }

        public void Generate()
        {
            // Compilo la matrice delle vacanze/festivi in modo da consultarla in seguito
            SetHolidays();

            // Compilare le reperibilità festive
            SetAvailabilty();

            // Compilare le aperture e chiusure
            SetWorkingDays();

            // Gestire i conflitti
            SolveConflicts();
        }

        // Setta le reperibilità festive
        private void SetAvailabilty()
        {
            int indexSeniorAvailable = _workers.Where(x => x.Profile == EmployeeProfile.Senior).Count();
            int indexJuniorAvailable = _workers.Where(x => x.Profile == EmployeeProfile.Junior).Count();

            for (int i = 0; i < _availability.NumberOfDays; i++)
            {
                if (IsAvailability(i))
                {
                    // Verifica quali worker inserire
                }
                else
                {
                    for (int j = 0; j < _availability.NumberOfEmployees; j++)
                    {
                        _availability.Matrix[j, i] = 0;
                    }
                }
            }
        }

        // Setta le ferie e i superfestivi
        private void SetHolidays()
        {
            for (int i = 0; i < _holidays.NumberOfDays; i++)
            {
                DateTime day = _calendarMap[i];

                // il giorno dell'anno è un superfestivo assegnato o vacanza
                for (int j = 0; j < _holidays.NumberOfEmployees; j++)
                {
                    foreach (var vacation in _workers[j].Vacations)
                    {
                        if (day == vacation.StartDate || (day > vacation.StartDate && day < vacation.EndDate))
                        {
                            _holidays.Matrix[j, i] = 1;
                        }
                        else
                        {
                            _holidays.Matrix[j, i] = 0;
                        }
                    }
                }
            }
        }

        private void SetWorkingDays()
        {
            int openIndex = 0;
            int closeIndex = 1;

            for (int i = 0; i < _openings.NumberOfDays; i++)
            {
                if (!IsAvailability(i))
                {
                    for (int j = 0; j < _openings.NumberOfEmployees; i++)
                    {
                        if (j == openIndex)
                            _openings.Matrix[j, i] = 1;
                        else
                            _openings.Matrix[j, i] = 0;

                        if (j == closeIndex)
                            _closings.Matrix[j, i] = 1;
                        else
                            _closings.Matrix[j, i] = 0;
                    }

                    // INDICE DI ASSEGNAZIONE APERTURA
                    if (openIndex == _openings.NumberOfEmployees - 1)
                        openIndex = 0;
                    else
                        openIndex++;

                    // INDICE DI ASSEGNAZIONE CHIUSURA
                    if (closeIndex == _closings.NumberOfEmployees - 1)
                        closeIndex = 0;
                    else
                        closeIndex++;
                }
                else
                {
                    for (int j = 0; j < _openings.NumberOfEmployees; i++)
                    {
                        _openings.Matrix[j, i] = 0;
                        _closings.Matrix[j, i] = 0;
                    }
                }
            }
        }

        private void SolveConflicts()
        {
            for (int i = 0; i < _availability.NumberOfDays; i++)
            {
                for (int j = 0; j < _availability.NumberOfEmployees; j++)
                {
                    if (_availability.Matrix[j, i] == 1 && (_openings.Matrix[j, i] == 1 || _closings.Matrix[j, i] == 1))
                    {
                        // Esegui lo swap
                    }
                }
            }
        }

        private int GetDaysInSchedule()
        {
            return (int)_endDate.Subtract(_startDate).TotalDays;
        }

        private bool IsAvailability(int dayNumber)
        {
            var date = _calendarMap[dayNumber];

            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                return true;
            }
            return false;
        }
    }
}
