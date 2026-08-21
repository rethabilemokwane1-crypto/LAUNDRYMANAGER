using LaundryManager.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
namespace LaundryManager.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        // Notice the <Machine> and <FaultReport> indicators below:
        public DbSet<Machine> Machines { get; set; }
        public DbSet<FaultReport> FaultReports { get; set; }
        // FIX: Moved inside the class braces and added <User>
        public DbSet<User> Users { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<PushSubscription> PushSubscriptions { get; set; }
    }
}