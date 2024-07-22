namespace ShiftBalance.MVC.Models.ViewModels
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
