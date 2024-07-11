using ShiftBalance.MVC.DAL;
using ShiftBalance.MVC.Models;

namespace ShiftBalance.MVC.Services
{
    public class EmployeeService
    {
        private readonly EmployeeRepository _employeeRepo;
        private readonly EmployeeVacationsRepository _employeeVacationsRepo;

        public EmployeeService(EmployeeRepository employeeRepo,EmployeeVacationsRepository employeeVacationsRepo)
        {
            _employeeRepo = employeeRepo;
            _employeeVacationsRepo = employeeVacationsRepo;
        }

        public List<Employee> GetEmployees()
        {
            return _employeeRepo.GetEmployees().ToList();
        }

        public List<EmployeeVacations> GetVacations()
        {
            return _employeeVacationsRepo.GetVacations().ToList();
        }
    }
}
