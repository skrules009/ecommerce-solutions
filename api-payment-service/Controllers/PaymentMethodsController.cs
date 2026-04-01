using api_payment_service.DTOs;
using api_payment_service.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace api_payment_service.Controllers
{
    [ApiController]
    [Route("api/payment-methods")]
    public class PaymentMethodsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentMethodsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        /// Get payment methods for customer
        /// </summary>
        [HttpGet("customer/{customerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> GetPaymentMethods(int customerId)
        {
            Log.Information("GET: Payment methods for customer {CustomerId} requested", customerId);
            var methods = await _paymentService.GetPaymentMethodsAsync(customerId);
            return Ok(methods);
        }

        /// <summary>
        /// Get default payment method for customer
        /// </summary>
        [HttpGet("customer/{customerId}/default")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentMethodDto>> GetDefaultPaymentMethod(int customerId)
        {
            Log.Information("GET: Default payment method for customer {CustomerId} requested", customerId);
            var method = await _paymentService.GetDefaultPaymentMethodAsync(customerId);
            if (method == null)
                return NotFound(new { message = $"No default payment method found for customer {customerId}" });

            return Ok(method);
        }

        /// <summary>
        /// Add payment method
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaymentMethodDto>> AddPaymentMethod([FromBody] CreatePaymentMethodDto createPaymentMethodDto)
        {
            // ✅ CUSTOM VALIDATION - Don't use ModelState.IsValid
            if (string.IsNullOrWhiteSpace(createPaymentMethodDto.MethodType))
            {
                return BadRequest(new { message = "MethodType is required" });
            }

            // Validate based on payment method type
            switch (createPaymentMethodDto.MethodType.ToLower())
            {
                case "creditcard":
                case "debitcard":
                    // For cards, we need card details
                    if (string.IsNullOrWhiteSpace(createPaymentMethodDto.CardNumber))
                        return BadRequest(new { message = "CardNumber is required for card payments" });
                    if (string.IsNullOrWhiteSpace(createPaymentMethodDto.CardholderName))
                        return BadRequest(new { message = "CardholderName is required for card payments" });
                    if (string.IsNullOrWhiteSpace(createPaymentMethodDto.ExpiryMonth))
                        return BadRequest(new { message = "ExpiryMonth is required for card payments" });
                    if (string.IsNullOrWhiteSpace(createPaymentMethodDto.ExpiryYear))
                        return BadRequest(new { message = "ExpiryYear is required for card payments" });
                    break;

                case "paypal":
                    // For PayPal, we need email
                    if (string.IsNullOrWhiteSpace(createPaymentMethodDto.PayPalEmail))
                        return BadRequest(new { message = "PayPalEmail is required for PayPal payments" });
                    break;

                case "bankaccount":
                    // For bank account, we need account details
                    if (string.IsNullOrWhiteSpace(createPaymentMethodDto.BankAccountNumber))
                        return BadRequest(new { message = "BankAccountNumber is required for bank account payments" });
                    if (string.IsNullOrWhiteSpace(createPaymentMethodDto.BankRoutingNumber))
                        return BadRequest(new { message = "BankRoutingNumber is required for bank account payments" });
                    if (string.IsNullOrWhiteSpace(createPaymentMethodDto.BankName))
                        return BadRequest(new { message = "BankName is required for bank account payments" });
                    break;

                default:
                    return BadRequest(new { message = "Invalid MethodType. Allowed values: CreditCard, DebitCard, PayPal, BankAccount" });
            }

            Log.Information("POST: Adding payment method for customer {CustomerId}", createPaymentMethodDto.CustomerId);

            try
            {
                var method = await _paymentService.AddPaymentMethodAsync(createPaymentMethodDto);
                return CreatedAtAction(nameof(GetPaymentMethods),
                    new { customerId = method.CustomerId }, method);
            }
            catch (ArgumentException ex)
            {
                Log.Warning("Validation error adding payment method: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update payment method
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePaymentMethod(int id, [FromBody] CreatePaymentMethodDto updatePaymentMethodDto)
        {
            Log.Information("PUT: Updating payment method {PaymentMethodId}", id);

            try
            {
                await _paymentService.UpdatePaymentMethodAsync(id, updatePaymentMethodDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                Log.Warning("Payment method not found: {Error}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                Log.Warning("Validation error updating payment method: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete payment method
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePaymentMethod(int id)
        {
            Log.Information("DELETE: Deleting payment method {PaymentMethodId}", id);

            try
            {
                await _paymentService.DeletePaymentMethodAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                Log.Warning("Payment method not found: {Error}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Set default payment method
        /// </summary>
        [HttpPut("{paymentMethodId}/set-default")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetDefaultPaymentMethod(int paymentMethodId, [FromQuery] int customerId)
        {
            Log.Information("PUT: Setting default payment method {PaymentMethodId} for customer {CustomerId}",
                paymentMethodId, customerId);

            try
            {
                await _paymentService.SetDefaultPaymentMethodAsync(customerId, paymentMethodId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                Log.Warning("Payment method not found: {Error}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
