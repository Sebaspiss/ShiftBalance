using ShiftBalance.MVC.Excel;
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

        public void Solve()
        {
            // Compilo la matrice delle vacanze/festivi in modo da consultarla in seguito
            SetHolidays();

            // Compilare le reperibilità festive
            SetAvailabilty();

            // Compilare le aperture e chiusure
            SetWorkingDays();

            // Gestire i conflitti
            SolveConflicts();

            // Esporta in Excel
            Excel.ShiftWorksheet worksheet = new(_workers, _openings, _closings, _availability, _calendarMap, _holidays);

            string folderName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ShiftBalance");
            Directory.CreateDirectory(folderName);

            string fileFullname = Path.Combine(folderName, $"plan_Matrix_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
            worksheet.Generate(fileFullname);

            ExcelProcess.OpenFile(fileFullname);
        }

        // Setta le reperibilità festive
        private void SetAvailabilty()
        {
            int seniorsNumber = _workers.Where(x => x.Profile == EmployeeProfile.Senior).Count();
            int juniorsNumber = _workers.Where(x => x.Profile == EmployeeProfile.Junior).Count();

            IEnumerable<int> seniorIndexes = Enumerable.Range(0, seniorsNumber);
            IEnumerable<int> juniorIndexes = Enumerable.Range(seniorsNumber, _workers.Count - seniorsNumber);

            int indexSeniorAvailable = seniorIndexes.Last();
            int indexJuniorAvailable = juniorIndexes.Last();

            // la reperibilità vale per sabato e domenica, si scala weekend per weekend oppure se è un superfestivo
            int availabilityDaysCount = 0;

            for (int i = 0; i < _availability.NumberOfDays; i++)
            {
                if (IsAvailability(i))
                {
                    // Uno junior e un senior a rotazione
                    _availability.Matrix[indexSeniorAvailable, i] = 1;
                    _availability.Matrix[indexJuniorAvailable, i] = 1;
                    availabilityDaysCount++;

                    if (availabilityDaysCount > 1 || CalendarFunctions.IsNationalHoliday(_calendarMap[i]) || (availabilityDaysCount == 1 && _calendarMap[i].DayOfWeek == DayOfWeek.Sunday))
                    {
                        indexSeniorAvailable--;
                        indexJuniorAvailable--;
                        availabilityDaysCount = 0;
                    }
                    if (indexSeniorAvailable < seniorIndexes.First())
                    {
                        indexSeniorAvailable = seniorIndexes.Last();
                    }
                    if (indexJuniorAvailable < juniorIndexes.First())
                    {
                        indexJuniorAvailable = juniorIndexes.Last();
                    }
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
            List<Employee> employeesWithVacations = _workers.Where(x => x.Vacations != null).ToList();

            for (int i = 0; i < _holidays.NumberOfDays; i++)
            {
                DateTime day = _calendarMap[i];

                // il giorno dell'anno è un superfestivo assegnato o vacanza
                for (int j = 0; j < _holidays.NumberOfEmployees; j++)
                {
                    if (employeesWithVacations.Exists(x => x.Id == j + 1))
                    {
                        Employee emp = employeesWithVacations.Where(y => y.Id == j + 1).First();

                        if (emp.Vacations.Any(x => x.StartDate == day || (day > x.StartDate && day < x.EndDate)))
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

        // Setta aperture e chiusure
        private void SetWorkingDays()
        {
            int openIndex = 0;
            int closeIndex = _closings.NumberOfEmployees - 1;

            for (int i = 0; i < _openings.NumberOfDays; i++)
            {
                if (!IsAvailability(i))
                {
                    for (int j = 0; j < _openings.NumberOfEmployees; j++)
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
                    for (int j = 0; j < _openings.NumberOfEmployees; j++)
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
                    if (i + 1 < _availability.NumberOfDays)
                    {
                        // Gestione lavoro dopo reperibilità
                        if (_availability.Matrix[j, i] == 1)
                        {
                            if (_openings.Matrix[j, i + 1] == 1)
                            {
                                Swap(j, i + 1, ShiftType.Opening);
                            }
                            else if (_closings.Matrix[j, i + 1] == 1)
                            {
                                Swap(j, i + 1, ShiftType.Closing);
                            }
                        }

                        // Gestione ferie/servizi/disponibilità
                        if (_holidays.Matrix[j, i] == 1)
                        {
                            if (_openings.Matrix[j, i] == 1)
                            {
                                SwapTurn(j, i, ShiftType.Opening);
                            }
                            else if (_closings.Matrix[j, i] == 1)
                            {
                                SwapTurn(j, i, ShiftType.Closing);
                            }
                            else if (_availability.Matrix[j,i] == 1)
                            {
                                SwapTurn(j,i, ShiftType.Availability);
                            }
                        }
                    }
                }
            }
        }

        private void SwapTurn(int worker, int day, ShiftType shiftType)
        {
            var employees = _workers.Where(x => x.Profile == _workers[worker].Profile).OrderBy(y => y.ShiftAverage).ToList();
            int swapIndex = 0;
            int swapWorker = employees[swapIndex].Id - 1;

            if (swapWorker == worker)
            {
                swapIndex = 1;
                swapWorker = employees[swapIndex].Id - 1;
            }

            if (shiftType == ShiftType.Opening)
            {
                _openings.Matrix[worker, day] = 0;
                _openings.Matrix[swapWorker, day] = 1;
            }
            else if (shiftType == ShiftType.Closing)
            {
                _closings.Matrix[worker, day] = 0;
                _closings.Matrix[swapWorker, day] = 1;
            }
            else if (shiftType == ShiftType.Availability)
            {
                _availability.Matrix[worker, day] = 0;
                _availability.Matrix[swapWorker, day] = 1;
            }
        }

        //spostare il turno al mercoledì (oppure ad altri giorni nel caso non fossero presenti turnisti disponibili);
        //si esegue poi uno scambio con il primo turnista disponibile alla riga successiva procedendo verso il basso
        //(se ci troviamo sull’ultima riga, si prosegue partendo dalla prima riga in alto);
        //se il successivo turnista, risultasse in ferie, si passa al prossimo fino a trovare il primo disponibile
        private void Swap(int worker, int violationDay, ShiftType type)
        {
            bool found = false;
            int swapDay = violationDay + 2;

            if (!(swapDay > GetDaysInSchedule()))
            {
                int swapWorker = worker + 1;
                int swapWorkerNext;
                do
                {
                    swapWorker++;

                    if (swapWorker > _closings.NumberOfEmployees - 1)
                    {
                        swapWorker = 0;
                    }
                    swapWorkerNext = swapWorker + 1;

                    if (swapWorkerNext > _closings.NumberOfEmployees - 1)
                    {
                        swapWorkerNext = 0;
                    }
                    // Tolgo il turno al lavoratore in violazione e lo do a quello di mercoledì
                    // Il lavoratore in violazione lavorerà mercoledì
                    if (type == ShiftType.Opening && _closings.Matrix[swapWorker,violationDay] == 0 && _holidays.Matrix[swapWorker, violationDay] == 0 && _holidays.Matrix[swapWorker, violationDay + 1] == 0)
                    {
                        // Tolgo a workerIndex - violationDayIndex
                        _openings.Matrix[worker, violationDay] = 0;
                        _closings.Matrix[worker, violationDay + 1] = 0;
                        // Assegno a swapWorkerIndex - violationDayIndex
                        _openings.Matrix[swapWorker, violationDay] = 1;
                        _closings.Matrix[swapWorker, violationDay + 1] = 1;
                        // Assegno a workerIndex - swapDayIndex
                        _openings.Matrix[worker, swapDay] = 1;
                        _closings.Matrix[worker, swapDay + 1] = 1;
                        // Tolgo a swapWorkerIndex - swapDayIndex
                        _openings.Matrix[swapWorker, swapDay] = 0;
                        _closings.Matrix[swapWorker, swapDay + 1] = 0;
                        found = true;
                    }
                    else if (type == ShiftType.Closing && _openings.Matrix[swapWorker, violationDay] == 0 && _holidays.Matrix[swapWorker, violationDay] == 0 && _holidays.Matrix[swapWorker, violationDay + 1] == 0)
                    {
                        // Tolgo a workerIndex - violationDayIndex
                        _closings.Matrix[worker, violationDay] = 0;
                        if (violationDay - 3 >= 0)
                            _openings.Matrix[worker, violationDay - 3] = 0;

                        // Assegno a swapWorkerIndex - violationDayIndex
                        _closings.Matrix[swapWorker, violationDay] = 1;
                        if (violationDay - 3 >= 0)
                            _openings.Matrix[swapWorker, violationDay - 3] = 1;

                        // Assegno a workerIndex - swapDayIndex
                        _closings.Matrix[worker, swapDay] = 1;
                        _openings.Matrix[worker, swapDay - 1] = 1;

                        // Tolgo a swapWorkerIndex - swapDayIndex
                        _openings.Matrix[swapWorker, swapDay - 1] = 0;
                        _closings.Matrix[swapWorker, swapDay] = 0;
                        _openings.Matrix[swapWorkerNext, swapDay - 1] = 0;
                        _closings.Matrix[swapWorkerNext, swapDay] = 0;
                        found = true;
                    }
                } while (!found);
            }
        }

        private int GetDaysInSchedule()
        {
            return (int)_endDate.Subtract(_startDate).TotalDays + 1;
        }

        private bool IsAvailability(int dayNumber)
        {
            var date = _calendarMap[dayNumber];

            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday || CalendarFunctions.IsNationalHoliday(date))
            {
                return true;
            }
            return false;
        }
    }
}
