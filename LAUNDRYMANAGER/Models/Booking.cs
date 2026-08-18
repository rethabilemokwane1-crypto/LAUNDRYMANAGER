namespace LaundryManager.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int MachineId { get; set; }
        public string StudentEmail { get; set; } = "";
        public DateTime SlotStart { get; set; }
        public DateTime SlotEnd { get; set; }
    }
}