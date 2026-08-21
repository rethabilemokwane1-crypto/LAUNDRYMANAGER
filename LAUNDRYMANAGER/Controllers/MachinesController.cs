using Microsoft.AspNetCore.Mvc;
using LaundryManager.Models;
using LaundryManager.Data;
using System.Linq;

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

        // POST: /Machines/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var machine = _context.Machines.FirstOrDefault(m => m.Id == id);
            if (machine != null)
            {
                _context.Machines.Remove(machine);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // POST: /Machines/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(int id, MachineStatus status)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var machine = _context.Machines.FirstOrDefault(m => m.Id == id);
            if (machine != null)
            {
                machine.Status = status;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}