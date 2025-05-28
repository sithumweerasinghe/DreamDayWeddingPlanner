namespace DreamDayWeddingPlanner.Models
{
    public class Checklist
    {
        public int ChecklistId { get; set; }
        public int WeddingId { get; set; }
        public string Task { get; set; }
        public bool IsCompleted { get; set; }
    }
}
