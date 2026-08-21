using LaundryManager.Data;
using Microsoft.EntityFrameworkCore;

namespace LaundryManager.Services
{
    public class BookingReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly HashSet<int> _alreadyNotified = new();

        public BookingReminderService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var pushService = scope.ServiceProvider.GetRequiredService<PushNotificationService>();

                    var now = DateTime.Now;
                    var reminderWindowStart = now.AddMinutes(4);
                    var reminderWindowEnd = now.AddMinutes(5);

                    // Find bookings starting in roughly 10-15 minutes that we haven't already pinged
                    var upcoming = await context.Bookings
                        .Where(b => b.SlotStart >= reminderWindowStart && b.SlotStart <= reminderWindowEnd)
                        .ToListAsync(stoppingToken);

                    foreach (var booking in upcoming)
                    {
                        if (_alreadyNotified.Contains(booking.Id)) continue;

                        var machine = await context.Machines.FirstOrDefaultAsync(m => m.Id == booking.MachineId, stoppingToken);
                        var machineName = machine?.Name ?? "Your machine";

                        await pushService.SendNotificationAsync(
                            booking.StudentEmail,
                            "Booking Reminder",
                            $"{machineName} is reserved for you at {booking.SlotStart:HH:mm} — coming up soon!",
                            "/Home/Index"
                        );

                        _alreadyNotified.Add(booking.Id);
                    }
                }

                // Check every 2 minutes
                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            }
        }
    }
}