namespace api_cart_service.DTOs
{
    public class CartDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public decimal TotalPrice { get; set; }
        public int TotalItems { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? AbandonedAt { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
    }
}
