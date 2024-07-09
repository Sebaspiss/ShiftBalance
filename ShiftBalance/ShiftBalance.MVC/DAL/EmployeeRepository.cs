using ShiftBalance.MVC.Models;

namespace ShiftBalance.MVC.DAL
{
    public class EmployeeRepository : IEmployeeRepository
    { 
        private readonly DataContext _context;

        public EmployeeRepository(DataContext context)
        {
            _context = context;
        }

        public Employee GetEmployee(int employeeID)
        {
          return _context.Employees.Find(employeeID);
        }

        public IEnumerable<Employee> GetEmployees()
        {
            return _context.Employees.ToList();
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
