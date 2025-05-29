namespace DreamDayWeddingPlanner.Models
{
    public class CartItem
    {
        public Vendor Vendor { get; set; }
        public int Quantity { get; set; } = 1; // You can allow multiple quantities later
    }

}