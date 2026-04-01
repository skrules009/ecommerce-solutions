namespace api_payment_service.DTOs
{
    public class CreatePaymentMethodDto
    {
        // ✅ REQUIRED
        public int CustomerId { get; set; }
        public string MethodType { get; set; } // CreditCard, DebitCard, PayPal, BankAccount

        // ✅ OPTIONAL - Only for CreditCard/DebitCard
        public string? CardNumber { get; set; }
        public string CardholderName { get; set; }
        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }
        public string? CVV { get; set; } // ✅ MADE OPTIONAL

        // ✅ OPTIONAL - Only for PayPal
        public string? PayPalEmail { get; set; } // ✅ MADE OPTIONAL

        // ✅ OPTIONAL - Only for BankAccount
        public string? BankAccountNumber { get; set; } // ✅ MADE OPTIONAL
        public string? BankRoutingNumber { get; set; } // ✅ MADE OPTIONAL
        public string? BankName { get; set; } // ✅ MADE OPTIONAL
        public bool IsDefault { get; set; } = false;
    }
}
