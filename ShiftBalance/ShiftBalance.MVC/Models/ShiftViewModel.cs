namespace ShiftBalance.MVC.Models
{
    public class ShiftViewModel
    {
        public List<Employee> Employees { get; set; }

        public ShiftViewModel()
        {
            Employees = [];
        }
    }
}
