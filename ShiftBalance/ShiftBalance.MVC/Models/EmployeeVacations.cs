using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftBalance.MVC.Models
{
    [Table("EmployeeVacations")]
    public class EmployeeVacations
    {
        [Key]
        public int ID { get; set; }
        
        [ForeignKey("Employee")]
        public int IdEmployee { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public Employee Employee { get; set; }
    }
}
