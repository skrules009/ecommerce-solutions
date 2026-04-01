using api_payment_service.DTOs;
using api_payment_service.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace api_payment_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        /// Get all payments
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAllPayments()
        {
            Log.Information("GET: All payments requested");
            var payments = await _paymentService.GetAllPaymentsAsync();
            return Ok(payments);
        }

        /// <summary>
        /// Get payments by status
        /// </summary>
        [HttpGet("status/{status}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPaymentsByStatus(string status)
        {
            Log.Information("GET: Payments with status {Status} requested", status);
            var payments = await _paymentService.GetPaymentsByStatusAsync(status);
            return Ok(payments);
        }

        /// <summary>
        /// Get payments by date range
        /// </summary>
        [HttpGet("date-range")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPaymentsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            Log.Information("GET: Payments between {StartDate} and {EndDate}", startDate, endDate);
            var payments = await _paymentService.GetPaymentsByDateRangeAsync(startDate, endDate);
            return Ok(payments);
        }

        /// <summary>
        /// Get payment by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentDto>> GetPaymentById(int id)
        {
            Log.Information("GET: Payment {PaymentId} requested", id);
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
                return NotFound(new { message = $"Payment with ID {id} not found" });

            return Ok(payment);
        }

        /// <summary>
        /// Get payment by transaction ID
        /// </summary>
        [HttpGet("transaction/{transactionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentDto>> GetPaymentByTransactionId(string transactionId)
        {
            Log.Information("GET: Payment with transaction ID {TransactionId} requested", transactionId);
            var payment = await _paymentService.GetPaymentByTransactionIdAsync(transactionId);
            if (payment == null)
                return NotFound(new { message = $"Payment with transaction ID {transactionId} not found" });

            return Ok(payment);
        }

        /// <summary>
        /// Get payment by order ID
        /// </summary>
        [HttpGet("order/{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentDto>> GetPaymentByOrderId(int orderId)
        {
            Log.Information("GET: Payment for order {OrderId} requested", orderId);
            var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
            if (payment == null)
                return NotFound(new { message = $"Payment not found for order {orderId}" });

            return Ok(payment);
        }

        /// <summary>
        /// Process payment
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaymentDto>> ProcessPayment([FromBody] CreatePaymentDto createPaymentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Log.Information("POST: Processing payment for order {OrderId}: {Amount}",
                createPaymentDto.OrderId, createPaymentDto.Amount);

            try
            {
                var payment = await _paymentService.ProcessPaymentAsync(createPaymentDto);
                return CreatedAtAction(nameof(GetPaymentById), new { id = payment.Id }, payment);
            }
            catch (ArgumentException ex)
            {
                Log.Warning("Validation error processing payment: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Complete payment
        /// </summary>
        [HttpPut("{id}/complete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentDto>> CompletePayment(int id)
        {
            Log.Information("PUT: Completing payment {PaymentId}", id);

            try
            {
                var payment = await _paymentService.CompletePaymentAsync(id);
                return Ok(payment);
            }
            catch (KeyNotFoundException ex)
            {
                Log.Warning("Payment not found: {Error}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Fail payment
        /// </summary>
        [HttpPut("{id}/fail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaymentDto>> FailPayment(int id, [FromQuery] string failureReason)
        {
            Log.Information("PUT: Failing payment {PaymentId}: {Reason}", id, failureReason);

            try
            {
                var payment = await _paymentService.FailPaymentAsync(id, failureReason);
                return Ok(payment);
            }
            catch (KeyNotFoundException ex)
            {
                Log.Warning("Payment not found: {Error}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Refund payment
        /// </summary>
        [HttpPost("{id}/refund")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentRefundDto>> RefundPayment(int id, [FromBody] CreateRefundDto createRefundDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Log.Information("POST: Refunding payment {PaymentId}: {Amount}", id, createRefundDto.Amount);

            try
            {
                createRefundDto.PaymentId = id;
                var refund = await _paymentService.RefundPaymentAsync(createRefundDto);
                return CreatedAtAction(nameof(GetPaymentById), new { id = id }, refund);
            }
            catch (KeyNotFoundException ex)
            {
                Log.Warning("Payment not found: {Error}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                Log.Warning("Validation error refunding payment: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get refunds for payment
        /// </summary>
        [HttpGet("{id}/refunds")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PaymentRefundDto>>> GetRefunds(int id)
        {
            Log.Information("GET: Refunds for payment {PaymentId} requested", id);
            var refunds = await _paymentService.GetRefundsAsync(id);
            return Ok(refunds);
        }
    }
}
