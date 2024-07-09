using ShiftBalance.MVC.Models;

namespace ShiftBalance.MVC.DAL
{
    public class EmployeeVacationsRepository : IEmployeeVacationsRepository
    {
        private readonly DataContext _context;

        public EmployeeVacationsRepository(DataContext context)
        {
            _context = context;
        }

        public IEnumerable<EmployeeVacations> GetVacations()
        {
            return _context.EmployeesVacations.ToList();
        }

        public IEnumerable<EmployeeVacations> GetVacations(int employeeID)
        {
            return _context.EmployeesVacations.Where(x => x.IdEmployee == employeeID).ToList();
        }

        public void Insert(EmployeeVacations vacations)
        {
            _context.EmployeesVacations.Add(vacations);
        }

        public void Update(EmployeeVacations vacations)
        {
            _context.Entry(vacations).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        }

        public void Delete(int id)
        {
            EmployeeVacations? vacations = _context.EmployeesVacations.Find(id);
            
            if (vacations != null)
            {
                _context.EmployeesVacations.Remove(vacations);
            }
        }

        public void Save()
        {
           _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
