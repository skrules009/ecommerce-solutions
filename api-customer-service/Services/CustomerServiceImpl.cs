using api_customer_service.DTOs;
using api_customer_service.Models;
using api_customer_service.Repositories;
using Serilog;

namespace api_customer_service.Services
{
    public class CustomerServiceImpl : ICustomerService
    {
        private readonly ICustomerRepository _repository;

        public CustomerServiceImpl(ICustomerRepository repository)
        {
            _repository = repository;
        }

        public async Task<CustomerDto> GetCustomerByIdAsync(int id)
        {
            Log.Information("Getting customer by ID: {CustomerId}", id);
            var customer = await _repository.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                Log.Warning("Customer not found with ID: {CustomerId}", id);
                return null;
            }
            return MapToDto(customer);
        }

        public async Task<CustomerDto> GetCustomerByEmailAsync(string email)
        {
            Log.Information("Getting customer by email: {Email}", email);
            var customer = await _repository.GetCustomerByEmailAsync(email);
            return customer != null ? MapToDto(customer) : null;
        }

        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            Log.Information("Getting all customers");
            var customers = await _repository.GetAllCustomersAsync();
            return customers.Select(MapToDto).ToList();
        }

        public async Task<IEnumerable<CustomerDto>> GetActiveCustomersAsync()
        {
            Log.Information("Getting active customers");
            var customers = await _repository.GetActiveCustomersAsync();
            return customers.Select(MapToDto).ToList();
        }

        public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createCustomerDto)
        {
            Log.Information("Creating new customer: {Email}", createCustomerDto.Email);

            // Validate email doesn't already exist
            if (await _repository.EmailExistsAsync(createCustomerDto.Email))
            {
                throw new ArgumentException($"Customer with email {createCustomerDto.Email} already exists");
            }

            var customer = new Customer
            {
                FirstName = createCustomerDto.FirstName,
                LastName = createCustomerDto.LastName,
                Email = createCustomerDto.Email.ToLower(),
                Phone = createCustomerDto.Phone,
                Address = createCustomerDto.Address,
                City = createCustomerDto.City,
                State = createCustomerDto.State,
                PostalCode = createCustomerDto.PostalCode,
                Country = createCustomerDto.Country,
                Notes = createCustomerDto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            var createdCustomer = await _repository.CreateCustomerAsync(customer);
            Log.Information("Customer created successfully: {CustomerId}", createdCustomer.Id);

            return MapToDto(createdCustomer);
        }

        public async Task UpdateCustomerAsync(int id, CreateCustomerDto updateCustomerDto)
        {
            Log.Information("Updating customer: {CustomerId}", id);

            var customer = await _repository.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                throw new KeyNotFoundException($"Customer not found with ID: {id}");
            }

            // Check if email is changing and if new email already exists
            if (customer.Email != updateCustomerDto.Email &&
                await _repository.EmailExistsAsync(updateCustomerDto.Email))
            {
                throw new ArgumentException($"Email {updateCustomerDto.Email} is already in use");
            }

            customer.FirstName = updateCustomerDto.FirstName;
            customer.LastName = updateCustomerDto.LastName;
            customer.Email = updateCustomerDto.Email.ToLower();
            customer.Phone = updateCustomerDto.Phone;
            customer.Address = updateCustomerDto.Address;
            customer.City = updateCustomerDto.City;
            customer.State = updateCustomerDto.State;
            customer.PostalCode = updateCustomerDto.PostalCode;
            customer.Country = updateCustomerDto.Country;
            customer.Notes = updateCustomerDto.Notes;

            await _repository.UpdateCustomerAsync(customer);
            Log.Information("Customer updated successfully: {CustomerId}", id);
        }

        public async Task DeleteCustomerAsync(int id)
        {
            Log.Information("Deleting customer: {CustomerId}", id);

            var exists = await _repository.CustomerExistsAsync(id);
            if (!exists)
            {
                throw new KeyNotFoundException($"Customer not found with ID: {id}");
            }

            await _repository.DeleteCustomerAsync(id);
            Log.Information("Customer deleted successfully: {CustomerId}", id);
        }

        private CustomerDto MapToDto(Customer customer)
        {
            return new CustomerDto
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                City = customer.City,
                State = customer.State,
                PostalCode = customer.PostalCode,
                Country = customer.Country,
                IsActive = customer.IsActive,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt,
                Notes = customer.Notes
            };
        }
    }
}
