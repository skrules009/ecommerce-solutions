using api_cart_service.DTOs;
using api_cart_service.Models;
using api_cart_service.Repositories;
using Serilog;

namespace api_cart_service.Services
{
    public class CartServiceImpl : ICartService
    {
        private readonly ICartRepository _repository;

        public CartServiceImpl(ICartRepository repository)
        {
            _repository = repository;
        }

        public async Task<CartDto> GetCartByIdAsync(int id)
        {
            Log.Information("Getting cart by ID: {CartId}", id);
            var cart = await _repository.GetCartByIdAsync(id);
            if (cart == null)
            {
                Log.Warning("Cart not found with ID: {CartId}", id);
                return null;
            }
            return MapToDto(cart);
        }

        public async Task<CartDto> GetCartByCustomerIdAsync(int customerId)
        {
            Log.Information("Getting cart for customer: {CustomerId}", customerId);
            var cart = await _repository.GetCartByCustomerIdAsync(customerId);
            return cart != null ? MapToDto(cart) : null;
        }

        public async Task<IEnumerable<CartDto>> GetAllCartsAsync()
        {
            Log.Information("Getting all carts");
            var carts = await _repository.GetAllCartsAsync();
            return carts.Select(MapToDto).ToList();
        }

        public async Task<IEnumerable<CartDto>> GetActiveCartsAsync()
        {
            Log.Information("Getting active carts");
            var carts = await _repository.GetActiveCartsAsync();
            return carts.Select(MapToDto).ToList();
        }

        public async Task<IEnumerable<CartDto>> GetAbandonedCartsAsync()
        {
            Log.Information("Getting abandoned carts");
            var carts = await _repository.GetAbandonedCartsAsync();
            return carts.Select(MapToDto).ToList();
        }

        public async Task<CartDto> CreateCartAsync(CreateCartDto createCartDto)
        {
            Log.Information("Creating new cart for customer: {CustomerId}", createCartDto.CustomerId);

            // Check if customer already has active cart
            var existingCart = await _repository.GetCartByCustomerIdAsync(createCartDto.CustomerId);
            if (existingCart != null)
            {
                Log.Information("Customer already has active cart: {CartId}", existingCart.Id);
                return MapToDto(existingCart);
            }

            var cart = new Cart
            {
                CustomerId = createCartDto.CustomerId,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            var createdCart = await _repository.CreateCartAsync(cart);
            Log.Information("Cart created successfully: {CartId}", createdCart.Id);

            return MapToDto(createdCart);
        }

        public async Task<CartDto> AddItemToCartAsync(int cartId, CreateCartItemDto createCartItemDto)
        {
            Log.Information("Adding item to cart {CartId}: ProductId {ProductId}", cartId, createCartItemDto.ProductId);

            var cart = await _repository.GetCartByIdAsync(cartId);
            if (cart == null)
            {
                throw new KeyNotFoundException($"Cart not found with ID: {cartId}");
            }

            if (createCartItemDto.Quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than 0");
            }

            var cartItem = new CartItem
            {
                CartId = cartId,
                ProductId = createCartItemDto.ProductId,
                ProductName = createCartItemDto.ProductName,
                ProductSku = createCartItemDto.ProductSku,
                UnitPrice = createCartItemDto.UnitPrice,
                Quantity = createCartItemDto.Quantity,
                TotalPrice = createCartItemDto.UnitPrice * createCartItemDto.Quantity,
                AddedAt = DateTime.UtcNow
            };

            await _repository.AddItemToCartAsync(cartItem);

            // Update cart totals
            cart.TotalItems = cart.Items.Count + 1;
            cart.TotalPrice = cart.Items.Sum(i => i.TotalPrice) + cartItem.TotalPrice;
            await _repository.UpdateCartAsync(cart);

            Log.Information("Item added to cart {CartId}", cartId);

            return await GetCartByIdAsync(cartId);
        }

        public async Task<CartDto> UpdateCartItemAsync(int cartId, int cartItemId, UpdateCartItemDto updateCartItemDto)
        {
            Log.Information("Updating item {CartItemId} in cart {CartId}", cartItemId, cartId);

            var cartItem = await _repository.GetCartItemAsync(cartItemId);
            if (cartItem == null || cartItem.CartId != cartId)
            {
                throw new KeyNotFoundException($"Cart item not found");
            }

            if (updateCartItemDto.Quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than 0");
            }

            cartItem.Quantity = updateCartItemDto.Quantity;
            cartItem.UnitPrice = updateCartItemDto.UnitPrice;
            cartItem.TotalPrice = updateCartItemDto.UnitPrice * updateCartItemDto.Quantity;

            await _repository.UpdateCartItemAsync(cartItem);

            // Update cart totals
            var cart = await _repository.GetCartByIdAsync(cartId);
            cart.TotalPrice = cart.Items.Sum(i => i.TotalPrice);
            await _repository.UpdateCartAsync(cart);

            Log.Information("Cart item updated: {CartItemId}", cartItemId);

            return await GetCartByIdAsync(cartId);
        }

        public async Task<CartDto> RemoveItemFromCartAsync(int cartId, int cartItemId)
        {
            Log.Information("Removing item {CartItemId} from cart {CartId}", cartItemId, cartId);

            var cartItem = await _repository.GetCartItemAsync(cartItemId);
            if (cartItem == null || cartItem.CartId != cartId)
            {
                throw new KeyNotFoundException($"Cart item not found");
            }

            await _repository.RemoveCartItemAsync(cartItemId);

            // Update cart totals
            var cart = await _repository.GetCartByIdAsync(cartId);
            var items = await _repository.GetCartItemsAsync(cartId);
            cart.TotalItems = items.Count();
            cart.TotalPrice = items.Sum(i => i.TotalPrice);
            await _repository.UpdateCartAsync(cart);

            Log.Information("Item removed from cart {CartId}", cartId);

            return await GetCartByIdAsync(cartId);
        }

        public async Task<CartDto> ClearCartAsync(int cartId)
        {
            Log.Information("Clearing cart: {CartId}", cartId);

            var cart = await _repository.GetCartByIdAsync(cartId);
            if (cart == null)
            {
                throw new KeyNotFoundException($"Cart not found with ID: {cartId}");
            }

            await _repository.ClearCartAsync(cartId);

            cart.TotalItems = 0;
            cart.TotalPrice = 0;
            await _repository.UpdateCartAsync(cart);

            Log.Information("Cart cleared: {CartId}", cartId);

            return await GetCartByIdAsync(cartId);
        }

        public async Task DeleteCartAsync(int id)
        {
            Log.Information("Deleting cart: {CartId}", id);

            var exists = await _repository.CartExistsAsync(id);
            if (!exists)
            {
                throw new KeyNotFoundException($"Cart not found with ID: {id}");
            }

            await _repository.DeleteCartAsync(id);
            Log.Information("Cart deleted successfully: {CartId}", id);
        }

        private CartDto MapToDto(Cart cart)
        {
            return new CartDto
            {
                Id = cart.Id,
                CustomerId = cart.CustomerId,
                TotalPrice = cart.TotalPrice,
                TotalItems = cart.TotalItems,
                Status = cart.Status,
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt,
                AbandonedAt = cart.AbandonedAt,
                Items = cart.Items?.Select(i => new CartItemDto
                {
                    Id = i.Id,
                    CartId = i.CartId,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    ProductSku = i.ProductSku,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    TotalPrice = i.TotalPrice,
                    AddedAt = i.AddedAt
                }).ToList() ?? new()
            };
        }
    }
}
