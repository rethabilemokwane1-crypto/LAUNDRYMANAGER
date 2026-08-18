using Microsoft.AspNetCore.Mvc;
using LaundryManager.Data;
using LaundryManager.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace LaundryManager.Controllers

{

    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Connect the controller to our SQL database context
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }
        private void ActivateDueBookings()
        {
            var now = DateTime.Now;

            // Find bookings whose reserved window has started but not yet ended
            var dueBookings = _context.Bookings
                .Where(b => b.SlotStart <= now && b.SlotEnd > now)
                .ToList();

            foreach (var booking in dueBookings)
            {
                var machine = _context.Machines.FirstOrDefault(m => m.Id == booking.MachineId);

                // Only take over the machine if it's free, or already assigned to this same student
                if (machine != null && machine.IsWorking &&
                    (!machine.IsBooked || machine.BookedBy == booking.StudentEmail))
                {
                    machine.IsBooked = true;
                    machine.BookedBy = booking.StudentEmail;
                    machine.BookedAt = booking.SlotStart;
                }
            }

            // Remove bookings whose window has already ended - no longer needed
            var expiredBookings = _context.Bookings.Where(b => b.SlotEnd <= now).ToList();
            _context.Bookings.RemoveRange(expiredBookings);

            _context.SaveChanges();
        }
        [HttpGet]
        public IActionResult Landing()
        {
            // If they're already logged in, skip the landing page and go straight to their dashboard
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (!string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Index");
            }

            return View();
        }

        public IActionResult Index()
        {
            ActivateDueBookings();

            var studentEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(studentEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!_context.Machines.Any())
            {
                _context.Machines.AddRange(
                    new Machine { Name = "Washing Machine A", Type = "Washer", IsWorking = true },
                    new Machine { Name = "Washing Machine B", Type = "Washer", IsWorking = true },
                    new Machine { Name = "Washing Machine C", Type = "Washer", IsWorking = false },
                    new Machine { Name = "Tumble Dryer 1", Type = "Dryer", IsWorking = true },
                    new Machine { Name = "Tumble Dryer 2", Type = "Dryer", IsWorking = false }
                );
                _context.SaveChanges();
            }

            var machines = _context.Machines.ToList();

            var now = DateTime.Now;
            var upcomingBookings = _context.Bookings
                .Where(b => b.SlotStart > now && b.SlotStart.Date == DateTime.Today)
                .OrderBy(b => b.SlotStart)
                .ToList()
                .GroupBy(b => b.MachineId)
                .ToDictionary(g => g.Key, g => g.ToList());

            ViewBag.UpcomingBookings = upcomingBookings;
            ViewBag.StudentEmail = studentEmail;

            return View(machines);
        }
        

        public IActionResult Privacy()
        {
            return View();
        }

        // 1. SHOW THE FAULT REPORT FORM (GET)
        [HttpGet]
        public IActionResult ReportFault(int machineId)
        {
            // Security Check: Ensure user is logged in
            var studentEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(studentEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            // Find the specific machine they clicked on
            var machine = _context.Machines.FirstOrDefault(m => m.Id == machineId);
            if (machine == null)
            {
                return NotFound();
            }

            // Pass details to the view layout safely
            ViewBag.StudentEmail = studentEmail;
            ViewBag.MachineName = machine.Name;
            ViewBag.MachineId = machineId;

            return View();
        }

        // 2. PROCESS THE SUBMITTED FAULT (POST)
        [HttpPost]
        public IActionResult ReportFault(int machineId, string description)
        {
            var studentEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(studentEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                ModelState.AddModelError("", "Please provide a brief description of the issue.");

                // Reload machine details if they submitted an empty text box
                var machine = _context.Machines.FirstOrDefault(m => m.Id == machineId);
                ViewBag.StudentEmail = studentEmail;
                ViewBag.MachineName = machine?.Name ?? "Machine";
                ViewBag.MachineId = machineId;
                return View();
            }

            // Create a record entry matching your FaultReport schema
            var report = new FaultReport
            {
                MachineId = machineId,
                Description = description
                // Note: If your FaultReport model has properties like 'ReportedBy' or 'DateReported', 
                // you can easily assign them here like: ReportedBy = studentEmail
            };

            _context.FaultReports.Add(report);

            // Explicitly flip the machine status to out of order if it isn't already
            var machineToUpdate = _context.Machines.FirstOrDefault(m => m.Id == machineId);
            if (machineToUpdate != null)
            {
                machineToUpdate.IsWorking = false;
            }

            _context.SaveChanges(); // Push changes to the physical SQL database file

            // Send the student back to the updated dashboard
            return RedirectToAction("Index");
        }

        // 1. BOOK A MACHINE (POST)
        [HttpPost]
        public IActionResult BookMachine(int machineId)
        {
            var studentEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(studentEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var machine = _context.Machines.FirstOrDefault(m => m.Id == machineId);
            if (machine != null && machine.IsWorking && !machine.IsBooked)
            {
                machine.IsBooked = true;
                machine.BookedBy = studentEmail;
                machine.BookedAt = DateTime.Now;  // NEW
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }



        // 2. RELEASE / FINISH USING A MACHINE (POST)
        [HttpPost]
        public IActionResult ReleaseMachine(int machineId)
        {
            var studentEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(studentEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var machine = _context.Machines.FirstOrDefault(m => m.Id == machineId);

            if (machine != null && machine.IsBooked && machine.BookedBy == studentEmail)
            {
                machine.IsBooked = false;
                machine.BookedBy = null;
                machine.BookedAt = null;

                // NEW: also remove any currently-active booking for this machine,
                // so ActivateDueBookings() doesn't immediately re-lock it
                var now = DateTime.Now;
                var activeBooking = _context.Bookings.FirstOrDefault(b =>
                    b.MachineId == machineId && b.SlotStart <= now && b.SlotEnd > now);

                if (activeBooking != null)
                {
                    _context.Bookings.Remove(activeBooking);
                }

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult AutoReleaseMachine(int machineId)
        {
            var studentEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(studentEmail))
            {
                return Unauthorized();
            }

            var machine = _context.Machines.FirstOrDefault(m => m.Id == machineId);
            if (machine != null && machine.IsBooked && machine.BookedBy == studentEmail)
            {
                machine.IsBooked = false;
                machine.BookedBy = null;
                machine.BookedAt = null;

                var now = DateTime.Now;
                var activeBooking = _context.Bookings.FirstOrDefault(b =>
                    b.MachineId == machineId && b.SlotStart <= now && b.SlotEnd > now);

                if (activeBooking != null)
                {
                    _context.Bookings.Remove(activeBooking);
                }

                _context.SaveChanges();
            }

            return Ok();
        }

        // GET: /Home/AdminDashboard
        public IActionResult AdminDashboard()
        {
            // Security Guard: Ensure only the dedicated admin pattern can view this page
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail) || userEmail != "000000000@mywsu.ac.za")
            {
                return RedirectToAction("Login", "Account");
            }

            // Pull all fault reports from the database to display to maintenance
            var reports = _context.FaultReports.OrderByDescending(r => r.Id).ToList();
            return View(reports);
        }

        // POST: /Home/ResolveFault
        [HttpPost]
        public IActionResult ResolveFault(int machineId, int reportId)
        {
            // 1. Flip the machine state back to functional (IsWorking = true)
            var machine = _context.Machines.FirstOrDefault(m => m.Id == machineId);
            if (machine != null)
            {
                machine.IsWorking = true;
                machine.IsBooked = false; // Ensure it's fully cleared
            }

            // 2. Remove the fault report from the system (or mark it resolved)
            var report = _context.FaultReports.FirstOrDefault(r => r.Id == reportId);
            if (report != null)
            {
                _context.FaultReports.Remove(report);
            }

            _context.SaveChanges();
            return RedirectToAction("AdminDashboard");
        }

        // Paste this right below your ResolveFault method
        [HttpPost]
        public IActionResult MarkMachineAsFaulty(int machineId)
        {
            var machine = _context.Machines.FirstOrDefault(m => m.Id == machineId);

            if (machine != null)
            {
                machine.IsWorking = false;
                machine.IsBooked = false; // Kick out any current booking since it's broken
                _context.SaveChanges();
            }

            // Redirects to your feedback page, passing the machine ID along
            return RedirectToAction("StudentFeedback", new { machineId = machineId });
        }
        [HttpPost]
        public IActionResult StudentFeedback(int machineId, string description)
        {
            // Step 3 Safety Net: If the student left the description blank, give it default text
            if (string.IsNullOrEmpty(description))
            {
                description = "Machine reported as faulty (No additional details provided).";
            }

            // Create the new fault report record
            var newReport = new FaultReport
            {
                MachineId = machineId,
                Description = description,
                DateReported = DateTime.Now,
                IsResolved = false
            };

            // Save it to your database table
            _context.FaultReports.Add(newReport);
            _context.SaveChanges();

            // Take the student back to the main student dashboard
            return RedirectToAction("Index");
        }
    }
}
    