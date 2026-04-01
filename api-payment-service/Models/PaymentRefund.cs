namespace api_payment_service.Models
{
    public class PaymentRefund
    {
        public int Id { get; set; }
        public int PaymentId { get; set; }
        public string RefundId { get; set; } // Unique refund identifier
        public decimal Amount { get; set; }
        public string Reason { get; set; } // CustomerRequest, Duplicate, Fraud, Other
        public string Status { get; set; } = "Pending"; // Pending, Processing, Completed, Failed
        public string GatewayRefundId { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string ProcessedBy { get; set; }

        // Navigation property
        public virtual Payment Payment { get; set; }
    }
}
