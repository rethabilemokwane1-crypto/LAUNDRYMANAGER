namespace LaundryManager.Models
{
    public class Machine
    {
        public bool IsBooked { get; set; } = false;
        public string? BookedBy { get; set; }
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";

        // Replaces IsWorking. Defaults to Working when a machine is created.
        public MachineStatus Status { get; set; } = MachineStatus.Working;

        public DateTime? BookedAt { get; set; }

        public static int GetAverageDurationMinutes(string type)
        {
            return type == "Dryer" ? 40 : 60;
        }

        public DateTime? GetEstimatedFinishTime()
        {
            if (BookedAt == null) return null;
            int duration = GetAverageDurationMinutes(Type);
            return BookedAt.Value.AddMinutes(duration);
        }
    }
}