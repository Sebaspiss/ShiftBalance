namespace ShiftBalance.MVC.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public EmployeeProfile Profile { get; set; }
        public DateTime DateOfHiring { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int ShiftAverage { get; set; }
    }
}
