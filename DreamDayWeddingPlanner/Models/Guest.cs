namespace DreamDayWeddingPlanner.Models
{
    public class Guest
    {
        public int GuestId { get; set; }
        public int WeddingId { get; set; }
        public string GuestName { get; set; }
        public string Email { get; set; }
        public string RsvpStatus { get; set; }
        public string MealPreference { get; set; }
        public string SeatNumber { get; set; }
    }
}
