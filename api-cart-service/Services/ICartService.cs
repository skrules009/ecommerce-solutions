using api_cart_service.DTOs;

namespace api_cart_service.Services
{
    public interface ICartService
    {
        Task<CartDto> GetCartByIdAsync(int id);
        Task<CartDto> GetCartByCustomerIdAsync(int customerId);
        Task<IEnumerable<CartDto>> GetAllCartsAsync();
        Task<IEnumerable<CartDto>> GetActiveCartsAsync();
        Task<IEnumerable<CartDto>> GetAbandonedCartsAsync();
        Task<CartDto> CreateCartAsync(CreateCartDto createCartDto);
        Task<CartDto> AddItemToCartAsync(int cartId, CreateCartItemDto createCartItemDto);
        Task<CartDto> UpdateCartItemAsync(int cartId, int cartItemId, UpdateCartItemDto updateCartItemDto);
        Task<CartDto> RemoveItemFromCartAsync(int cartId, int cartItemId);
        Task<CartDto> ClearCartAsync(int cartId);
        Task DeleteCartAsync(int id);
    }
}
