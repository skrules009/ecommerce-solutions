namespace api_payment_service.Models
{
    public class PaymentMethod
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string MethodType { get; set; } // CreditCard, DebitCard, PayPal, BankAccount
        public string CardNumber { get; set; } // Last 4 digits only for PCI compliance
        public string CardholderName { get; set; }
        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }
        public string CardBrand { get; set; } = "Unknown"; // ✅ DEFAULT VALUE
        public string PayPalEmail { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankRoutingNumber { get; set; }
        public string BankName { get; set; }
        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public string GatewayPaymentMethodId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
