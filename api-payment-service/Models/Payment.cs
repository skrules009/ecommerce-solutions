namespace api_payment_service.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public string TransactionId { get; set; } // Unique transaction identifier
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Status { get; set; } = "Pending"; // Pending, Processing, Completed, Failed, Refunded
        public string PaymentMethod { get; set; } // CreditCard, DebitCard, PayPal, BankTransfer, etc.
        public string CardLastFour { get; set; } // Last 4 digits of card (if applicable)
        public string CardBrand { get; set; } // Visa, MasterCard, Amex, etc.
        public string PaymentGateway { get; set; } // Stripe, PayPal, Square, etc.
        public string GatewayTransactionId { get; set; } // Transaction ID from payment gateway
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string FailureReason { get; set; }
        public string BillingEmail { get; set; }
        public string BillingPhone { get; set; }
        public string BillingAddress { get; set; }
        public string BillingCity { get; set; }
        public string BillingState { get; set; }
        public string BillingZipCode { get; set; }
        public string BillingCountry { get; set; }

        // Navigation property
        public virtual List<PaymentRefund> Refunds { get; set; } = new();
    }
}
