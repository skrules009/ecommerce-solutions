namespace api_payment_service.DTOs
{
    public class PaymentMethodDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string MethodType { get; set; }
        public string CardNumber { get; set; }
        public string CardholderName { get; set; }
        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }
        public string CardBrand { get; set; }
        public string PayPalEmail { get; set; }
        public string BankName { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
