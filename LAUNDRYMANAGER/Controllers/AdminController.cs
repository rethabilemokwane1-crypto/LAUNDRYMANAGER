using Microsoft.AspNetCore.Mvc;

namespace LaundryManager.Controllers
{
    public class AdminController : Controller
    {
        // Reusable check so only the designated admin account can view this page
        private bool IsAdmin()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            return !string.IsNullOrEmpty(userEmail) && userEmail == "000000000@mywsu.ac.za";
        }

        // GET: /Admin
        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View();
        }
    }
}