using api_cart_service.Data;
using api_cart_service.Models;
using Microsoft.EntityFrameworkCore;

namespace api_cart_service.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly CartDbContext _context;

        public CartRepository(CartDbContext context)
        {
            _context = context;
        }

        public async Task<Cart> GetCartByIdAsync(int id)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Cart> GetCartByCustomerIdAsync(int customerId)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.Status == "Active");
        }

        public async Task<IEnumerable<Cart>> GetAllCartsAsync()
        {
            return await _context.Carts
                .Include(c => c.Items)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cart>> GetActiveCartsAsync()
        {
            return await _context.Carts
                .Include(c => c.Items)
                .Where(c => c.Status == "Active")
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cart>> GetAbandonedCartsAsync()
        {
            return await _context.Carts
                .Include(c => c.Items)
                .Where(c => c.Status == "Abandoned")
                .OrderByDescending(c => c.AbandonedAt)
                .ToListAsync();
        }

        public async Task<Cart> CreateCartAsync(Cart cart)
        {
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        public async Task UpdateCartAsync(Cart cart)
        {
            cart.UpdatedAt = DateTime.UtcNow;
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCartAsync(int id)
        {
            var cart = await GetCartByIdAsync(id);
            if (cart != null)
            {
                _context.Carts.Remove(cart);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> CartExistsAsync(int id)
        {
            return await _context.Carts.AnyAsync(c => c.Id == id);
        }

        public async Task<CartItem> AddItemToCartAsync(CartItem cartItem)
        {
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();
            return cartItem;
        }

        public async Task<CartItem> UpdateCartItemAsync(CartItem cartItem)
        {
            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync();
            return cartItem;
        }

        public async Task RemoveCartItemAsync(int cartItemId)
        {
            var cartItem = await GetCartItemAsync(cartItemId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<CartItem> GetCartItemAsync(int cartItemId)
        {
            return await _context.CartItems.FirstOrDefaultAsync(ci => ci.Id == cartItemId);
        }

        public async Task<IEnumerable<CartItem>> GetCartItemsAsync(int cartId)
        {
            return await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .OrderByDescending(ci => ci.AddedAt)
                .ToListAsync();
        }

        public async Task<bool> CartItemExistsAsync(int cartItemId)
        {
            return await _context.CartItems.AnyAsync(ci => ci.Id == cartItemId);
        }

        public async Task ClearCartAsync(int cartId)
        {
            var items = await GetCartItemsAsync(cartId);
            foreach (var item in items)
            {
                _context.CartItems.Remove(item);
            }
            await _context.SaveChangesAsync();
        }
    }
}
