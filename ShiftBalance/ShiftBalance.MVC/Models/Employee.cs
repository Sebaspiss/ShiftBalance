using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftBalance.MVC.Models
{
    [Table("Employee")]
    public class Employee
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public EmployeeProfile Profile { get; set; }
        public DateTime DateOfHiring { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int ShiftAverage { get; set; }

        public ICollection<EmployeeVacations> Vacations { get; set; }
    }
}
