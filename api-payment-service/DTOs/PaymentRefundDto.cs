namespace api_payment_service.DTOs
{
    public class PaymentRefundDto
    {
        public int Id { get; set; }
        public int PaymentId { get; set; }
        public string RefundId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string ProcessedBy { get; set; }
    }
}
