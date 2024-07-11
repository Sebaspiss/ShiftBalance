using Microsoft.EntityFrameworkCore;
using ShiftBalance.MVC.Models;

namespace ShiftBalance.MVC.DAL
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Vacations)
                .WithOne(e => e.Employee)
                .HasForeignKey(e => e.IdEmployee)
                .HasPrincipalKey(e => e.Id)
                .IsRequired();
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeVacations> EmployeesVacations { get; set; }
    }
}
