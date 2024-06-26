namespace ShiftBalance.MVC.Models
{
    public class ShiftMatrix
    {
        private int _numberOfEmployees;
        private int _numberOfDays;
        private int[,] _matrix;

        public int NumberOfEmployees { get => _numberOfEmployees; }
        public int NumberOfDays { get => _numberOfDays; }
        public int[,] Matrix { get => _matrix; }

        public ShiftMatrix(int employeesNumber, int daysNumber)
        {
            _numberOfEmployees = employeesNumber;
            _numberOfDays = daysNumber;
            _matrix = new int[employeesNumber,daysNumber];
        }
    }
}
