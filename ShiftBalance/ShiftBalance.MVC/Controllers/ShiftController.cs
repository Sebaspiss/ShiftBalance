using Microsoft.AspNetCore.Mvc;
using ShiftBalance.MVC.Models.ViewModels;
using ShiftBalance.MVC.Services;

namespace ShiftBalance.MVC.Controllers
{
    public class ShiftController : Controller
    {
        private readonly EmployeeService _employeeService;

        public ShiftController(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public IActionResult Index()
        {
            ShiftViewModel model = new()
            {
                Employees = _employeeService.GetEmployees()
            };
            return View(model);
        }
    }
}
