namespace api_cart_service.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSku { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual Cart Cart { get; set; }
    }
}
