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
            var selected = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(employees);
            var employeeList = new List<Employee>();
            var vacations = _employeeService.GetVacations();

            foreach (var jsonString in selected)
            {
                var employee = Newtonsoft.Json.JsonConvert.DeserializeObject<Employee>(jsonString);
                employee.Vacations = vacations.Where(x=> x.IdEmployee == employee.Id).ToList();
                employeeList.Add(employee);
            }

            MatrixSolver solver = new(employeeList, fromDate, toDate);
            solver.Solve();

            return Ok();
        }

        public IActionResult GenerateCpSatShift(string employees, DateTime fromDate, DateTime toDate)
        {
            var selected = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(employees);
            var employeeList = new List<Employee>();
            var vacations = _employeeService.GetVacations();

            foreach (var jsonString in selected)
            {
                var employee = Newtonsoft.Json.JsonConvert.DeserializeObject<Employee>(jsonString);
                employee.Vacations = vacations.Where(x => x.IdEmployee == employee.Id).ToList();
                employeeList.Add(employee);
            }

            CpSatSolver solver = new(employeeList, fromDate, toDate);

            if (solver.Solve() != Google.OrTools.Sat.CpSolverStatus.Feasible)
            {
                return NotFound();
            }
            return Ok();
        }
    }
}
