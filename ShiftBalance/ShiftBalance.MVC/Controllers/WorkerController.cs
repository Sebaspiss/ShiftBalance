using Microsoft.AspNetCore.Mvc;
using ShiftBalance.MVC.Models;
using ShiftBalance.MVC.Services;

namespace ShiftBalance.MVC.Controllers
{
    public class WorkerController : Controller
    {
        private readonly EmployeeService _employeeService;

        public WorkerController(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public IActionResult Index()
        {
            WorkerViewModel model = new()
            {
                Employees = _employeeService.GetEmployees(),
                EmployeesVacations = _employeeService.GetVacations()
            };

            return View(model);
        }
    }
}
