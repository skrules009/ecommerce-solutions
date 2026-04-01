using api_customer_service.DTOs;
using api_customer_service.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace api_customer_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        /// <summary>
        /// Get all customers
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAllCustomers()
        {
            Log.Information("GET: All customers requested");
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
        }

        /// <summary>
        /// Get active customers only
        /// </summary>
        [HttpGet("active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetActiveCustomers()
        {
            Log.Information("GET: Active customers requested");
            var customers = await _customerService.GetActiveCustomersAsync();
            return Ok(customers);
        }

        /// <summary>
        /// Get customer by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerDto>> GetCustomerById(int id)
        {
            Log.Information("GET: Customer {CustomerId} requested", id);
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
                return NotFound(new { message = $"Customer with ID {id} not found" });

            return Ok(customer);
        }

        /// <summary>
        /// Get customer by email
        /// </summary>
        [HttpGet("by-email/{email}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerDto>> GetCustomerByEmail(string email)
        {
            Log.Information("GET: Customer {Email} requested", email);
            var customer = await _customerService.GetCustomerByEmailAsync(email);
            if (customer == null)
                return NotFound(new { message = $"Customer with email {email} not found" });

            return Ok(customer);
        }

        /// <summary>
        /// Create new customer
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CreateCustomerDto createCustomerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Log.Information("POST: New customer creation requested for email {Email}", createCustomerDto.Email);

            try
            {
                var customer = await _customerService.CreateCustomerAsync(createCustomerDto);
                return CreatedAtAction(nameof(GetCustomerById), new { id = customer.Id }, customer);
            }
            catch (ArgumentException ex)
            {
                Log.Warning("Validation error creating customer: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update existing customer
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CreateCustomerDto updateCustomerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Log.Information("PUT: Customer {CustomerId} update requested", id);

            try
            {
                await _customerService.UpdateCustomerAsync(id, updateCustomerDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                Log.Warning("Customer not found for update: {Error}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                Log.Warning("Validation error updating customer: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete customer
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            Log.Information("DELETE: Customer {CustomerId} deletion requested", id);

            try
            {
                await _customerService.DeleteCustomerAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                Log.Warning("Customer not found for deletion: {Error}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
