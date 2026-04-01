namespace api_payment_service.DTOs
{
    public class CreatePaymentDto
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string PaymentMethod { get; set; } // CreditCard, DebitCard, PayPal, etc.
        public int? PaymentMethodId { get; set; } // If using saved payment method
        public string Description { get; set; }

        // Card details (if paying with new card)
        public string CardNumber { get; set; }
        public string CardholderName { get; set; }
        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }
        public string CVV { get; set; }

        // Billing address
        public string BillingEmail { get; set; }
        public string BillingPhone { get; set; }
        public string BillingAddress { get; set; }
        public string BillingCity { get; set; }
        public string BillingState { get; set; }
        public string BillingZipCode { get; set; }
        public string BillingCountry { get; set; }
    }
}
