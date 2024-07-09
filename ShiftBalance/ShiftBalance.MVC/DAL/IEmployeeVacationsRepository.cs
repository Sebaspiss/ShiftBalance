using ShiftBalance.MVC.Models;

namespace ShiftBalance.MVC.DAL
{
    public interface IEmployeeVacationsRepository : IDisposable
    {
        IEnumerable<EmployeeVacations> GetVacations();
        IEnumerable<EmployeeVacations> GetVacations(int employeeID);

        void Insert(EmployeeVacations vacations);
        void Delete(int id);
        void Update(EmployeeVacations vacations);
        void Save();
    }
}
