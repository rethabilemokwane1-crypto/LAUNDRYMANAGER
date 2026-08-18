using Microsoft.AspNetCore.Mvc;

namespace LaundryManager.Controllers
{
    public class AdminController : Controller
    {
        // This handles loading the main Admin page
        public IActionResult Index()
        {
            return View();
        }
    }
}