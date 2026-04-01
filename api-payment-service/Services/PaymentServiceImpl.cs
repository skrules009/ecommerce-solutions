using api_payment_service.DTOs;
using api_payment_service.Models;
using api_payment_service.Repositories;
using Serilog;

namespace api_payment_service.Services
{
    public class PaymentServiceImpl : IPaymentService
    {
        private readonly IPaymentRepository _repository;

        public PaymentServiceImpl(IPaymentRepository repository)
        {
            _repository = repository;
        }

        public async Task<PaymentDto> GetPaymentByIdAsync(int id)
        {
            Log.Information("Getting payment by ID: {PaymentId}", id);
            var payment = await _repository.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                Log.Warning("Payment not found with ID: {PaymentId}", id);
                return null;
            }
            return MapToDto(payment);
        }

        public async Task<PaymentDto> GetPaymentByTransactionIdAsync(string transactionId)
        {
            Log.Information("Getting payment by transaction ID: {TransactionId}", transactionId);
            var payment = await _repository.GetPaymentByTransactionIdAsync(transactionId);
            return payment != null ? MapToDto(payment) : null;
        }

        public async Task<PaymentDto> GetPaymentByOrderIdAsync(int orderId)
        {
            Log.Information("Getting payment for order: {OrderId}", orderId);
            var payment = await _repository.GetPaymentByOrderIdAsync(orderId);
            return payment != null ? MapToDto(payment) : null;
        }

        public async Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync()
        {
            Log.Information("Getting all payments");
            var payments = await _repository.GetAllPaymentsAsync();
            return payments.Select(MapToDto).ToList();
        }

        public async Task<IEnumerable<PaymentDto>> GetPaymentsByStatusAsync(string status)
        {
            Log.Information("Getting payments by status: {Status}", status);
            var payments = await _repository.GetPaymentsByStatusAsync(status);
            return payments.Select(MapToDto).ToList();
        }

        public async Task<IEnumerable<PaymentDto>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            Log.Information("Getting payments between {StartDate} and {EndDate}", startDate, endDate);
            var payments = await _repository.GetPaymentsByDateRangeAsync(startDate, endDate);
            return payments.Select(MapToDto).ToList();
        }

        public async Task<PaymentDto> ProcessPaymentAsync(CreatePaymentDto createPaymentDto)
        {
            Log.Information("Processing payment for order {OrderId}: {Amount} {Currency}",
                createPaymentDto.OrderId, createPaymentDto.Amount, createPaymentDto.Currency);

            // Validate payment details
            if (createPaymentDto.Amount <= 0)
            {
                throw new ArgumentException("Payment amount must be greater than 0");
            }

            if (string.IsNullOrWhiteSpace(createPaymentDto.PaymentMethod))
            {
                throw new ArgumentException("Payment method is required");
            }

            // Create unique transaction ID
            string transactionId = $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";

            var payment = new Payment
            {
                TransactionId = transactionId,
                OrderId = createPaymentDto.OrderId,
                Amount = createPaymentDto.Amount,
                Currency = createPaymentDto.Currency,
                PaymentMethod = createPaymentDto.PaymentMethod,
                Description = createPaymentDto.Description,
                Status = "Processing",
                PaymentGateway = "Stripe", // Default to Stripe (can be configurable)
                BillingEmail = createPaymentDto.BillingEmail,
                BillingPhone = createPaymentDto.BillingPhone,
                BillingAddress = createPaymentDto.BillingAddress,
                BillingCity = createPaymentDto.BillingCity,
                BillingState = createPaymentDto.BillingState,
                BillingZipCode = createPaymentDto.BillingZipCode,
                BillingCountry = createPaymentDto.BillingCountry,
                ProcessedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            // Mask card number for security
            if (!string.IsNullOrWhiteSpace(createPaymentDto.CardNumber) &&
                createPaymentDto.CardNumber.Length >= 4)
            {
                payment.CardLastFour = createPaymentDto.CardNumber.Substring(
                    createPaymentDto.CardNumber.Length - 4);
            }

            // In a real implementation, you would call the payment gateway API here
            // For now, we'll just create the payment record
            var createdPayment = await _repository.CreatePaymentAsync(payment);

            Log.Information("Payment created: {TransactionId}", transactionId);

            return MapToDto(createdPayment);
        }

        public async Task<PaymentDto> CompletePaymentAsync(int paymentId)
        {
            Log.Information("Completing payment: {PaymentId}", paymentId);

            var payment = await _repository.GetPaymentByIdAsync(paymentId);
            if (payment == null)
            {
                throw new KeyNotFoundException($"Payment not found with ID: {paymentId}");
            }

            if (payment.Status == "Completed")
            {
                Log.Warning("Payment already completed: {PaymentId}", paymentId);
                return MapToDto(payment);
            }

            payment.Status = "Completed";
            payment.CompletedAt = DateTime.UtcNow;

            await _repository.UpdatePaymentAsync(payment);
            Log.Information("Payment completed: {PaymentId}", paymentId);

            return MapToDto(payment);
        }

        public async Task<PaymentDto> FailPaymentAsync(int paymentId, string failureReason)
        {
            Log.Information("Failing payment {PaymentId}: {Reason}", paymentId, failureReason);

            var payment = await _repository.GetPaymentByIdAsync(paymentId);
            if (payment == null)
            {
                throw new KeyNotFoundException($"Payment not found with ID: {paymentId}");
            }

            payment.Status = "Failed";
            payment.FailureReason = failureReason;
            payment.ProcessedAt = DateTime.UtcNow;

            await _repository.UpdatePaymentAsync(payment);
            Log.Information("Payment failed: {PaymentId}", paymentId);

            return MapToDto(payment);
        }

        public async Task<PaymentRefundDto> RefundPaymentAsync(CreateRefundDto createRefundDto)
        {
            Log.Information("Processing refund for payment {PaymentId}: {Amount}",
                createRefundDto.PaymentId, createRefundDto.Amount);

            var payment = await _repository.GetPaymentByIdAsync(createRefundDto.PaymentId);
            if (payment == null)
            {
                throw new KeyNotFoundException($"Payment not found with ID: {createRefundDto.PaymentId}");
            }

            if (payment.Status != "Completed")
            {
                throw new InvalidOperationException("Only completed payments can be refunded");
            }

            // Calculate total refunded amount
            var existingRefunds = await _repository.GetRefundsByPaymentIdAsync(payment.Id);
            decimal totalRefunded = existingRefunds.Sum(r => r.Amount);

            if (totalRefunded + createRefundDto.Amount > payment.Amount)
            {
                throw new ArgumentException("Refund amount exceeds original payment amount");
            }

            string refundId = $"REF-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";

            var refund = new PaymentRefund
            {
                PaymentId = createRefundDto.PaymentId,
                RefundId = refundId,
                Amount = createRefundDto.Amount,
                Reason = createRefundDto.Reason,
                Status = "Processing",
                Notes = createRefundDto.Notes,
                ProcessedBy = createRefundDto.ProcessedBy,
                CreatedAt = DateTime.UtcNow
            };

            var createdRefund = await _repository.AddRefundAsync(refund);

            // Mark payment as refunded if fully refunded
            if (totalRefunded + createRefundDto.Amount == payment.Amount)
            {
                payment.Status = "Refunded";
                await _repository.UpdatePaymentAsync(payment);
            }

            Log.Information("Refund created: {RefundId}", refundId);

            return MapRefundToDto(createdRefund);
        }

        public async Task<IEnumerable<PaymentRefundDto>> GetRefundsAsync(int paymentId)
        {
            Log.Information("Getting refunds for payment {PaymentId}", paymentId);
            var refunds = await _repository.GetRefundsByPaymentIdAsync(paymentId);
            return refunds.Select(MapRefundToDto).ToList();
        }

        // ==================== PAYMENT METHODS ====================
        public async Task<PaymentMethodDto> AddPaymentMethodAsync(CreatePaymentMethodDto createPaymentMethodDto)
        {
            Log.Information("Adding payment method for customer {CustomerId}", createPaymentMethodDto.CustomerId);

            if (string.IsNullOrWhiteSpace(createPaymentMethodDto.MethodType))
            {
                throw new ArgumentException("Payment method type is required");
            }

            // If this is the first payment method, make it default
            var existingMethods = await _repository.GetPaymentMethodsByCustomerAsync(createPaymentMethodDto.CustomerId);
            if (!existingMethods.Any())
            {
                createPaymentMethodDto.IsDefault = true;
            }

            // If setting as default, unset other defaults
            if (createPaymentMethodDto.IsDefault)
            {
                var currentDefault = await _repository.GetDefaultPaymentMethodAsync(createPaymentMethodDto.CustomerId);
                if (currentDefault != null)
                {
                    currentDefault.IsDefault = false;
                    await _repository.UpdatePaymentMethodAsync(currentDefault);
                }
            }

            // ✅ COMPLETE MAPPING - Only set values if they're provided
            var paymentMethod = new PaymentMethod
            {
                CustomerId = createPaymentMethodDto.CustomerId,
                MethodType = createPaymentMethodDto.MethodType,

                // Card Details - Only if provided
                CardNumber = !string.IsNullOrWhiteSpace(createPaymentMethodDto.CardNumber)
                    ? MaskCardNumber(createPaymentMethodDto.CardNumber)
                    : null,
                CardholderName = createPaymentMethodDto.CardholderName,
                ExpiryMonth = createPaymentMethodDto.ExpiryMonth,
                ExpiryYear = createPaymentMethodDto.ExpiryYear,
                CardBrand = !string.IsNullOrWhiteSpace(createPaymentMethodDto.CardNumber)
                    ? DetectCardBrand(createPaymentMethodDto.CardNumber)
                    : "Unknown",

                // PayPal - Only if provided
                PayPalEmail = createPaymentMethodDto.PayPalEmail,

                // Bank Account - Only if provided
                BankAccountNumber = !string.IsNullOrWhiteSpace(createPaymentMethodDto.BankAccountNumber)
                    ? MaskAccountNumber(createPaymentMethodDto.BankAccountNumber)
                    : null,
                BankRoutingNumber = createPaymentMethodDto.BankRoutingNumber,
                BankName = createPaymentMethodDto.BankName,

                // Status & Gateway
                IsDefault = createPaymentMethodDto.IsDefault,
                IsActive = true,
                GatewayPaymentMethodId = Guid.NewGuid().ToString(),

                // Timestamps
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            var createdMethod = await _repository.CreatePaymentMethodAsync(paymentMethod);
            Log.Information("Payment method added: {PaymentMethodId}", createdMethod.Id);

            return MapPaymentMethodToDto(createdMethod);
        }

        public async Task<IEnumerable<PaymentMethodDto>> GetPaymentMethodsAsync(int customerId)
        {
            Log.Information("Getting payment methods for customer {CustomerId}", customerId);
            var methods = await _repository.GetPaymentMethodsByCustomerAsync(customerId);
            return methods.Select(MapPaymentMethodToDto).ToList();
        }

        public async Task<PaymentMethodDto> GetDefaultPaymentMethodAsync(int customerId)
        {
            Log.Information("Getting default payment method for customer {CustomerId}", customerId);
            var method = await _repository.GetDefaultPaymentMethodAsync(customerId);
            return method != null ? MapPaymentMethodToDto(method) : null;
        }

        public async Task UpdatePaymentMethodAsync(int id, CreatePaymentMethodDto updatePaymentMethodDto)
        {
            Log.Information("Updating payment method: {PaymentMethodId}", id);

            var paymentMethod = await _repository.GetPaymentMethodByIdAsync(id);
            if (paymentMethod == null)
            {
                throw new KeyNotFoundException($"Payment method not found with ID: {id}");
            }

            paymentMethod.CardholderName = updatePaymentMethodDto.CardholderName;
            paymentMethod.ExpiryMonth = updatePaymentMethodDto.ExpiryMonth;
            paymentMethod.ExpiryYear = updatePaymentMethodDto.ExpiryYear;

            if (updatePaymentMethodDto.IsDefault && !paymentMethod.IsDefault)
            {
                var currentDefault = await _repository.GetDefaultPaymentMethodAsync(paymentMethod.CustomerId);
                if (currentDefault != null)
                {
                    currentDefault.IsDefault = false;
                    await _repository.UpdatePaymentMethodAsync(currentDefault);
                }
                paymentMethod.IsDefault = true;
            }

            await _repository.UpdatePaymentMethodAsync(paymentMethod);
            Log.Information("Payment method updated: {PaymentMethodId}", id);
        }

        public async Task DeletePaymentMethodAsync(int id)
        {
            Log.Information("Deleting payment method: {PaymentMethodId}", id);

            var paymentMethod = await _repository.GetPaymentMethodByIdAsync(id);
            if (paymentMethod == null)
            {
                throw new KeyNotFoundException($"Payment method not found with ID: {id}");
            }

            // Don't actually delete, just deactivate
            paymentMethod.IsActive = false;
            await _repository.UpdatePaymentMethodAsync(paymentMethod);
            Log.Information("Payment method deactivated: {PaymentMethodId}", id);
        }

        public async Task SetDefaultPaymentMethodAsync(int customerId, int paymentMethodId)
        {
            Log.Information("Setting default payment method {PaymentMethodId} for customer {CustomerId}",
                paymentMethodId, customerId);

            var paymentMethod = await _repository.GetPaymentMethodByIdAsync(paymentMethodId);
            if (paymentMethod == null || paymentMethod.CustomerId != customerId)
            {
                throw new KeyNotFoundException("Payment method not found");
            }

            var currentDefault = await _repository.GetDefaultPaymentMethodAsync(customerId);
            if (currentDefault != null)
            {
                currentDefault.IsDefault = false;
                await _repository.UpdatePaymentMethodAsync(currentDefault);
            }

            paymentMethod.IsDefault = true;
            await _repository.UpdatePaymentMethodAsync(paymentMethod);
            Log.Information("Default payment method set: {PaymentMethodId}", paymentMethodId);
        }

        // ==================== MAPPING METHODS ====================
        private PaymentDto MapToDto(Payment payment)
        {
            return new PaymentDto
            {
                Id = payment.Id,
                TransactionId = payment.TransactionId,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                Currency = payment.Currency,
                Status = payment.Status,
                PaymentMethod = payment.PaymentMethod,
                CardLastFour = payment.CardLastFour,
                CardBrand = payment.CardBrand,
                PaymentGateway = payment.PaymentGateway,
                Description = payment.Description,
                CreatedAt = payment.CreatedAt,
                ProcessedAt = payment.ProcessedAt,
                CompletedAt = payment.CompletedAt,
                FailureReason = payment.FailureReason,
                BillingEmail = payment.BillingEmail,
                BillingCity = payment.BillingCity,
                BillingCountry = payment.BillingCountry,
                Refunds = payment.Refunds?.Select(MapRefundToDto).ToList() ?? new()
            };
        }

        private PaymentRefundDto MapRefundToDto(PaymentRefund refund)
        {
            return new PaymentRefundDto
            {
                Id = refund.Id,
                PaymentId = refund.PaymentId,
                RefundId = refund.RefundId,
                Amount = refund.Amount,
                Reason = refund.Reason,
                Status = refund.Status,
                Notes = refund.Notes,
                CreatedAt = refund.CreatedAt,
                ProcessedAt = refund.ProcessedAt,
                CompletedAt = refund.CompletedAt,
                ProcessedBy = refund.ProcessedBy
            };
        }

        private PaymentMethodDto MapPaymentMethodToDto(PaymentMethod paymentMethod)
        {
            return new PaymentMethodDto
            {
                Id = paymentMethod.Id,
                CustomerId = paymentMethod.CustomerId,
                MethodType = paymentMethod.MethodType,
                CardNumber = paymentMethod.CardNumber,
                CardholderName = paymentMethod.CardholderName,
                ExpiryMonth = paymentMethod.ExpiryMonth,
                ExpiryYear = paymentMethod.ExpiryYear,
                CardBrand = paymentMethod.CardBrand,
                PayPalEmail = paymentMethod.PayPalEmail,
                BankName = paymentMethod.BankName,
                IsDefault = paymentMethod.IsDefault,
                IsActive = paymentMethod.IsActive,
                CreatedAt = paymentMethod.CreatedAt,
                UpdatedAt = paymentMethod.UpdatedAt
            };
        }

        // ==================== HELPER METHODS ====================
        private string DetectCardBrand(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return "Unknown";

            string number = cardNumber.Replace(" ", "").Replace("-", "");

            if (number.StartsWith("4"))
                return "Visa";
            else if (number.StartsWith("5"))
                return "MasterCard";
            else if (number.StartsWith("3"))
                return "Amex";
            else if (number.StartsWith("6"))
                return "Discover";
            else if (number.StartsWith("35"))
                return "JCB";
            else if (number.StartsWith("30"))
                return "Diners";
            else
                return "Unknown";
        }

        private string MaskCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 4)
                return cardNumber;

            return $"****-****-****-{cardNumber.Substring(cardNumber.Length - 4)}";
        }

        private string MaskAccountNumber(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber) || accountNumber.Length < 4)
                return accountNumber;

            return $"***-{accountNumber.Substring(accountNumber.Length - 4)}";
        }
    }
}
