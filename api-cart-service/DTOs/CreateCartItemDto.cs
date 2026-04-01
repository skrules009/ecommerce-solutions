namespace api_cart_service.DTOs
{
    public class CreateCartItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSku { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}
