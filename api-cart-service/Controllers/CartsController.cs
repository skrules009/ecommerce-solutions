using api_cart_service.DTOs;
using api_cart_service.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace api_cart_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartsController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartsController(ICartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>
        /// Get all carts
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CartDto>>> GetAllCarts()
        {
            Log.Information("GET: All carts requested");
            var carts = await _cartService.GetAllCartsAsync();
            return Ok(carts);
        }

        /// <summary>
        /// Get active carts
        /// </summary>
        [HttpGet("active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CartDto>>> GetActiveCarts()
        {
            Log.Information("GET: Active carts requested");
            var carts = await _cartService.GetActiveCartsAsync();
            return Ok(carts);
        }

        /// <summary>
        /// Get abandoned carts
        /// </summary>
        [HttpGet("abandoned")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CartDto>>> GetAbandonedCarts()
        {
            Log.Information("GET: Abandoned carts requested");
            var carts = await _cartService.GetAbandonedCartsAsync();
            return Ok(carts);
        }

        /// <summary>
        /// Get cart by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CartDto>> GetCartById(int id)
        {
            Log.Information("GET: Cart {CartId} requested", id);
            var cart = await _cartService.GetCartByIdAsync(id);
            if (cart == null)
                return NotFound(new { message = $"Cart with ID {id} not found" });

            return Ok(cart);
        }

        /// <summary>
        /// Get cart by customer ID
        /// </summary>
        [HttpGet("customer/{customerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CartDto>> GetCartByCustomerId(int customerId)
        {
            Log.Information("GET: Cart for customer {CustomerId} requested", customerId);
            var cart = await _cartService.GetCartByCustomerIdAsync(customerId);
            if (cart == null)
                return NotFound(new { message = $"No active cart found for customer {customerId}" });

            return Ok(cart);
        }

        /// <summary>
        /// Create new cart
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CartDto>> CreateCart([FromBody] CreateCartDto createCartDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Log.Information("POST: New cart creation requested for customer {CustomerId}", createCartDto.CustomerId);

            try
            {
                var cart = await _cartService.CreateCartAsync(createCartDto);
                return CreatedAtAction(nameof(GetCartById), new { id = cart.Id }, cart);
            }
            catch (ArgumentException ex)
            {
                Log.Warning("Validation error creating cart: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Add item to cart
        /// </summary>
        [HttpPost("{cartId}/items")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CartDto>> AddItemToCart(int cartId, [FromBody] CreateCartItemDto createCartItemDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Log.Information("POST: Adding item to cart {CartId}", cartId);

            try
            {
                var cart = await _cartService.AddItemToCartAsync(cartId, createCartItemDto);
                return Ok(cart);
            }
            catch (KeyNotFoundException ex)
            {
                Log.Warning("Cart not found: {Error}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                Log.Warning("Validation error adding item: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update cart item
        /// </summary>
        [HttpPut("{cartId}/items/{cartItemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CartDto>> UpdateCartItem(int cartId, int cartItemId, [FromBody] UpdateCartItemDto updateCartItemDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Log.Information("PUT: Updating item {CartItemId} in cart {CartId}", cartItemId, cartId);

            try
            {
                var cart = await _cartService.UpdateCartItemAsync(cartId, cartItemId, updateCartItemDto);
                return Ok(cart);
            }
            catch (KeyNotFoundException ex)
            {
                Log.Warning("Item or cart not found: {Error}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                Log.Warning("Validation error updating item: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Remove item from cart
        /// </summary>
        [HttpDelete("{cartId}/items/{cartItemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CartDto>> RemoveItemFromCart(int cartId, int cartItemId)
        {
            Log.Information("DELETE: Removing item {CartItemId} from cart {CartId}", cartItemId, cartId);

            try
            {
                var cart = await _cartService.RemoveItemFromCartAsync(cartId, cartItemId);
                return Ok(cart);
            }
            catch (KeyNotFoundException ex)
            {
                Log.Warning("Item or cart not found: {Error}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Clear all items from cart
        /// </summary>
        [HttpDelete("{cartId}/clear")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CartDto>> ClearCart(int cartId)
        {
            Log.Information("DELETE: Clearing cart {CartId}", cartId);

            try
            {
                var cart = await _cartService.ClearCartAsync(cartId);
                return Ok(cart);
            }
            catch (KeyNotFoundException ex)
            {
                Log.Warning("Cart not found: {Error}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete cart
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCart(int id)
        {
            Log.Information("DELETE: Cart {CartId} deletion requested", id);

            try
            {
                await _cartService.DeleteCartAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                Log.Warning("Cart not found for deletion: {Error}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
