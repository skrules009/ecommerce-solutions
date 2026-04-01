using api_cart_service.Models;

namespace api_cart_service.Repositories
{
    public interface ICartRepository
    {
        Task<Cart> GetCartByIdAsync(int id);
        Task<Cart> GetCartByCustomerIdAsync(int customerId);
        Task<IEnumerable<Cart>> GetAllCartsAsync();
        Task<IEnumerable<Cart>> GetActiveCartsAsync();
        Task<IEnumerable<Cart>> GetAbandonedCartsAsync();
        Task<Cart> CreateCartAsync(Cart cart);
        Task UpdateCartAsync(Cart cart);
        Task DeleteCartAsync(int id);
        Task<bool> CartExistsAsync(int id);
        Task<CartItem> AddItemToCartAsync(CartItem cartItem);
        Task<CartItem> UpdateCartItemAsync(CartItem cartItem);
        Task RemoveCartItemAsync(int cartItemId);
        Task<CartItem> GetCartItemAsync(int cartItemId);
        Task<IEnumerable<CartItem>> GetCartItemsAsync(int cartId);
        Task<bool> CartItemExistsAsync(int cartItemId);
        Task ClearCartAsync(int cartId);
    }
}
