namespace ShiftBalance.MVC.Models.ViewModels
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
