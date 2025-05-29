// File: Models/TimelineEvent.cs

using System;

namespace DreamDayWeddingPlanner.Models
{
    public class TimelineEvent
    {
        public int EventId { get; set; }
        public int WeddingId { get; set; }
        public string EventTitle { get; set; }
        public string EventDescription { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
