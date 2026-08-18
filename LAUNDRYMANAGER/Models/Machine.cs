namespace LaundryManager.Models
{
    public class Machine
    {
        public bool IsBooked { get; set; } = false;
        public string? BookedBy { get; set; }
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public bool IsWorking { get; set; } = true;
        public DateTime? BookedAt { get; set; }

        // NEW: works out how long this type of machine's cycle takes
        public static int GetAverageDurationMinutes(string type)
        {
            return type == "Dryer" ? 40 : 60; // 1 minute for testing
        }

        // NEW: works out when the current cycle should finish
        public DateTime? GetEstimatedFinishTime()
        {
            if (BookedAt == null) return null;
            int duration = GetAverageDurationMinutes(Type);
            return BookedAt.Value.AddMinutes(duration);
        }
    }
}