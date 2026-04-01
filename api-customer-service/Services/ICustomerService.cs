using api_customer_service.DTOs;

namespace api_customer_service.Services
{
    public interface ICustomerService
    {
        Task<CustomerDto> GetCustomerByIdAsync(int id);
        Task<CustomerDto> GetCustomerByEmailAsync(string email);
        Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
        Task<IEnumerable<CustomerDto>> GetActiveCustomersAsync();
        Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createCustomerDto);
        Task UpdateCustomerAsync(int id, CreateCustomerDto updateCustomerDto);
        Task DeleteCustomerAsync(int id);
    }
}
