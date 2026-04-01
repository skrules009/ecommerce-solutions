namespace api_cart_service.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public decimal TotalPrice { get; set; }
        public int TotalItems { get; set; }
        public string Status { get; set; } = "Active"; // Active, Abandoned, Converted
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? AbandonedAt { get; set; }

        // Navigation property
        public virtual List<CartItem> Items { get; set; } = new();
    }
}
