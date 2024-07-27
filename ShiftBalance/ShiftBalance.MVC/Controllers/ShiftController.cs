using Microsoft.AspNetCore.Mvc;
using ShiftBalance.MVC.Models;
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

        public IActionResult GenerateMatrixShift(string employees, DateTime fromDate, DateTime toDate)
        {
            employees = employees.Replace("\\","").Trim();
            Employee[] selected = System.Text.Json.JsonSerializer.Deserialize<Employee[]>(employees);

            MatrixSolver solver = new(selected.ToList(), fromDate, toDate);
            solver.Solve();

            return Ok();
        }

        public IActionResult GenerateCpSatShift(string employees, DateTime fromDate, DateTime toDate)
        {
            var selected = System.Text.Json.JsonSerializer.Deserialize<List<Employee>>(employees);

            CpSatSolver solver = new(selected, fromDate, toDate);
            solver.Solve();

            return Ok();
        }
    }
}
