namespace api_payment_service.DTOs
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public string TransactionId { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public string CardLastFour { get; set; }
        public string CardBrand { get; set; }
        public string PaymentGateway { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string FailureReason { get; set; }
        public string BillingEmail { get; set; }
        public string BillingCity { get; set; }
        public string BillingCountry { get; set; }
        public List<PaymentRefundDto> Refunds { get; set; } = new();
    }
}
