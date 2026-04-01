using api_payment_service.Models;

namespace api_payment_service.Repositories
{
    public interface IPaymentRepository
    {
        Task<Payment> GetPaymentByIdAsync(int id);
        Task<Payment> GetPaymentByTransactionIdAsync(string transactionId);
        Task<Payment> GetPaymentByOrderIdAsync(int orderId);
        Task<IEnumerable<Payment>> GetAllPaymentsAsync();
        Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(string status);
        Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Payment> CreatePaymentAsync(Payment payment);
        Task UpdatePaymentAsync(Payment payment);
        Task DeletePaymentAsync(int id);
        Task<bool> PaymentExistsAsync(int id);

        // Refunds
        Task<PaymentRefund> AddRefundAsync(PaymentRefund refund);
        Task<PaymentRefund> GetRefundByIdAsync(int id);
        Task<IEnumerable<PaymentRefund>> GetRefundsByPaymentIdAsync(int paymentId);
        Task UpdateRefundAsync(PaymentRefund refund);

        // Payment Methods
        Task<PaymentMethod> GetPaymentMethodByIdAsync(int id);
        Task<IEnumerable<PaymentMethod>> GetPaymentMethodsByCustomerAsync(int customerId);
        Task<PaymentMethod> GetDefaultPaymentMethodAsync(int customerId);
        Task<PaymentMethod> CreatePaymentMethodAsync(PaymentMethod paymentMethod);
        Task UpdatePaymentMethodAsync(PaymentMethod paymentMethod);
        Task DeletePaymentMethodAsync(int id);
    }
}
