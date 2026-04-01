namespace api_payment_service.DTOs
{
    public class CreateRefundDto
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } // CustomerRequest, Duplicate, Fraud, Other
        public string Notes { get; set; }
        public string ProcessedBy { get; set; }
    }
}
