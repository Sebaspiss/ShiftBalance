namespace ShiftBalance.MVC.Models
{
    public class WorkerViewModel
    {
        public List<Employee> Employees { get; set; }
        public List<EmployeeVacations> EmployeesVacations { get; set; }

        public WorkerViewModel()
        {
            Employees = [];
            EmployeesVacations = [];
        }
    }
}
