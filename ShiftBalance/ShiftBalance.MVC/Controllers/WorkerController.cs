using Microsoft.AspNetCore.Mvc;

namespace ShiftBalance.MVC.Controllers
{
    public class WorkerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
