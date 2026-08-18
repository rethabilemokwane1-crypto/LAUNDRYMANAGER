using Microsoft.AspNetCore.Mvc;
using LaundryManager.Data;
using LaundryManager.Models;
using System.Linq;

namespace LaundryManager.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Bookings/Slots?machineId=3
        public IActionResult Slots(int machineId)
        {
            var studentEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(studentEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var machine = _context.Machines.FirstOrDefault(m => m.Id == machineId);
            if (machine == null)
            {
                return NotFound();
            }

            int durationMinutes = Machine.GetAverageDurationMinutes(machine.Type);

            // Generate slots from now until 22:00 today, in cycle-length chunks
            var slots = new List<(DateTime Start, DateTime End, bool IsTaken)>();
            var slotStart = DateTime.Now.AddMinutes(5); // small buffer so a slot isn't "in the past" by the time they click
            var dayEnd = DateTime.Today.AddHours(22);

            var todaysBookings = _context.Bookings
                .Where(b => b.MachineId == machineId && b.SlotStart.Date == DateTime.Today)
                .ToList();

            while (slotStart.AddMinutes(durationMinutes) <= dayEnd)
            {
                var slotEnd = slotStart.AddMinutes(durationMinutes);
                bool isTaken = todaysBookings.Any(b => b.SlotStart < slotEnd && slotStart < b.SlotEnd);

                slots.Add((slotStart, slotEnd, isTaken));
                slotStart = slotEnd;
            }

            ViewBag.Machine = machine;
            return View(slots);
        }

        // POST: /Bookings/Reserve
        [HttpPost]
        public IActionResult Reserve(int machineId, DateTime slotStart, DateTime slotEnd)
        {
            var studentEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(studentEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            bool alreadyTaken = _context.Bookings.Any(b =>
                b.MachineId == machineId && b.SlotStart < slotEnd && slotStart < b.SlotEnd);

            if (!alreadyTaken)
            {
                var booking = new Booking
                {
                    MachineId = machineId,
                    StudentEmail = studentEmail,
                    SlotStart = slotStart,
                    SlotEnd = slotEnd
                };
                _context.Bookings.Add(booking);
                _context.SaveChanges();

                var machine = _context.Machines.FirstOrDefault(m => m.Id == machineId);
                TempData["BookingConfirmation"] =
                    $"You've booked {machine?.Name} for {slotStart:HH:mm} to {slotEnd:HH:mm}.";
            }

            return RedirectToAction("Slots", new { machineId });
        }
    }
}
        