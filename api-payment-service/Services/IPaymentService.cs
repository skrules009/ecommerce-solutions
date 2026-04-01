using api_payment_service.DTOs;

namespace api_payment_service.Services
{
    public interface IPaymentService
    {
        Task<PaymentDto> GetPaymentByIdAsync(int id);
        Task<PaymentDto> GetPaymentByTransactionIdAsync(string transactionId);
        Task<PaymentDto> GetPaymentByOrderIdAsync(int orderId);
        Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync();
        Task<IEnumerable<PaymentDto>> GetPaymentsByStatusAsync(string status);
        Task<IEnumerable<PaymentDto>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<PaymentDto> ProcessPaymentAsync(CreatePaymentDto createPaymentDto);
        Task<PaymentDto> CompletePaymentAsync(int paymentId);
        Task<PaymentDto> FailPaymentAsync(int paymentId, string failureReason);
        Task<PaymentRefundDto> RefundPaymentAsync(CreateRefundDto createRefundDto);
        Task<IEnumerable<PaymentRefundDto>> GetRefundsAsync(int paymentId);

        // Payment Methods
        Task<PaymentMethodDto> AddPaymentMethodAsync(CreatePaymentMethodDto createPaymentMethodDto);
        Task<IEnumerable<PaymentMethodDto>> GetPaymentMethodsAsync(int customerId);
        Task<PaymentMethodDto> GetDefaultPaymentMethodAsync(int customerId);
        Task UpdatePaymentMethodAsync(int id, CreatePaymentMethodDto updatePaymentMethodDto);
        Task DeletePaymentMethodAsync(int id);
        Task SetDefaultPaymentMethodAsync(int customerId, int paymentMethodId);
    }
}
