using Microsoft.EntityFrameworkCore;
using ShiftBalance.MVC.Models;

namespace ShiftBalance.MVC.DAL
{
    public class DataContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public DataContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeVacations> EmployeesVacations { get; set; }
    }
}
