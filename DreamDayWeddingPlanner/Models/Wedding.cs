using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DreamDayWeddingPlanner.Models
{
    public class Wedding
    {
        public int WeddingId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public DateTime WeddingDate { get; set; }
        public string Location { get; set; }
        public string CoupleUsername { get; set; } 

    }
}