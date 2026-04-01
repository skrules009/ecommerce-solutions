using api_payment_service.Data;
using api_payment_service.Models;
using Microsoft.EntityFrameworkCore;

namespace api_payment_service.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentDbContext _context;

        public PaymentRepository(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<Payment> GetPaymentByIdAsync(int id)
        {
            return await _context.Payments
                .Include(p => p.Refunds)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Payment> GetPaymentByTransactionIdAsync(string transactionId)
        {
            return await _context.Payments
                .Include(p => p.Refunds)
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId);
        }

        public async Task<Payment> GetPaymentByOrderIdAsync(int orderId)
        {
            return await _context.Payments
                .Include(p => p.Refunds)
                .FirstOrDefaultAsync(p => p.OrderId == orderId);
        }

        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
        {
            return await _context.Payments
                .Include(p => p.Refunds)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(string status)
        {
            return await _context.Payments
                .Include(p => p.Refunds)
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Payments
                .Include(p => p.Refunds)
                .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Payment> CreatePaymentAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task UpdatePaymentAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePaymentAsync(int id)
        {
            var payment = await GetPaymentByIdAsync(id);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> PaymentExistsAsync(int id)
        {
            return await _context.Payments.AnyAsync(p => p.Id == id);
        }

        // Refunds
        public async Task<PaymentRefund> AddRefundAsync(PaymentRefund refund)
        {
            _context.PaymentRefunds.Add(refund);
            await _context.SaveChangesAsync();
            return refund;
        }

        public async Task<PaymentRefund> GetRefundByIdAsync(int id)
        {
            return await _context.PaymentRefunds.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<PaymentRefund>> GetRefundsByPaymentIdAsync(int paymentId)
        {
            return await _context.PaymentRefunds
                .Where(r => r.PaymentId == paymentId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateRefundAsync(PaymentRefund refund)
        {
            _context.PaymentRefunds.Update(refund);
            await _context.SaveChangesAsync();
        }

        // Payment Methods
        public async Task<PaymentMethod> GetPaymentMethodByIdAsync(int id)
        {
            return await _context.PaymentMethods.FirstOrDefaultAsync(pm => pm.Id == id);
        }

        public async Task<IEnumerable<PaymentMethod>> GetPaymentMethodsByCustomerAsync(int customerId)
        {
            return await _context.PaymentMethods
                .Where(pm => pm.CustomerId == customerId && pm.IsActive)
                .OrderByDescending(pm => pm.IsDefault)
                .ToListAsync();
        }

        public async Task<PaymentMethod> GetDefaultPaymentMethodAsync(int customerId)
        {
            return await _context.PaymentMethods
                .FirstOrDefaultAsync(pm => pm.CustomerId == customerId && pm.IsDefault && pm.IsActive);
        }

        public async Task<PaymentMethod> CreatePaymentMethodAsync(PaymentMethod paymentMethod)
        {
            _context.PaymentMethods.Add(paymentMethod);
            await _context.SaveChangesAsync();
            return paymentMethod;
        }

        public async Task UpdatePaymentMethodAsync(PaymentMethod paymentMethod)
        {
            paymentMethod.UpdatedAt = DateTime.UtcNow;
            _context.PaymentMethods.Update(paymentMethod);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePaymentMethodAsync(int id)
        {
            var paymentMethod = await GetPaymentMethodByIdAsync(id);
            if (paymentMethod != null)
            {
                _context.PaymentMethods.Remove(paymentMethod);
                await _context.SaveChangesAsync();
            }
        }
    }
}
