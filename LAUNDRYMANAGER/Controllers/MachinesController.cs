using Microsoft.AspNetCore.Mvc;
using LaundryManager.Models;
using LaundryManager.Data;

namespace LaundryManager.Controllers
{
    public class MachinesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MachinesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Reusable check so we don't repeat this in every action
        private bool IsAdmin()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            return !string.IsNullOrEmpty(userEmail) && userEmail == "000000000@mywsu.ac.za";
        }

        // GET: /Machines
        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var machines = _context.Machines.ToList();
            return View(machines);
        }

        // GET: /Machines/Create
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            return View();
        }

        // POST: /Machines/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Machine machine)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                _context.Machines.Add(machine);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(machine);
        }
    }
}