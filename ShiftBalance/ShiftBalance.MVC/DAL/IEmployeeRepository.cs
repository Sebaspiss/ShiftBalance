using ShiftBalance.MVC.Models;

namespace ShiftBalance.MVC.DAL
{
    public interface IEmployeeRepository : IDisposable
    {
        IEnumerable<Employee> GetEmployees();
        Employee GetEmployee(int employeeID);
    }
}
