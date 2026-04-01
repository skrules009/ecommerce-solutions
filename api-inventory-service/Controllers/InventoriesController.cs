using api_inventory_service.DTOs;
using api_inventory_service.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace api_inventory_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoriesController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoriesController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        /// <summary>
        /// Get all inventories
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InventoryDto>>> GetAllInventories()
        {
            Log.Information("GET: All inventories requested");
            var inventories = await _inventoryService.GetAllInventoriesAsync();
            return Ok(inventories);
        }

        /// <summary>
        /// Get low stock inventories
        /// </summary>
        [HttpGet("low-stock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InventoryDto>>> GetLowStockInventories()
        {
            Log.Information("GET: Low stock inventories requested");
            var inventories = await _inventoryService.GetLowStockInventoriesAsync();
            return Ok(inventories);
        }

        /// <summary>
        /// Get inventories by warehouse
        /// </summary>
        [HttpGet("warehouse/{warehouse}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InventoryDto>>> GetInventoriesByWarehouse(string warehouse)
        {
            Log.Information("GET: Inventories for warehouse {Warehouse} requested", warehouse);
            var inventories = await _inventoryService.GetInventoriesByWarehouseAsync(warehouse);
            return Ok(inventories);
        }

        /// <summary>
        /// Get inventory by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InventoryDto>> GetInventoryById(int id)
        {
            Log.Information("GET: Inventory {InventoryId} requested", id);
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
                return NotFound(new { message = $"Inventory with ID {id} not found" });

            return Ok(inventory);
        }

        /// <summary>
        /// Get inventory by Product ID
        /// </summary>
        [HttpGet("product/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InventoryDto>> GetInventoryByProductId(int productId)
        {
            Log.Information("GET: Inventory for product {ProductId} requested", productId);
            var inventory = await _inventoryService.GetInventoryByProductIdAsync(productId);
            if (inventory == null)
                return NotFound(new { message = $"Inventory not found for product {productId}" });

            return Ok(inventory);
        }

        /// <summary>
        /// Get inventory by SKU
        /// </summary>
        [HttpGet("sku/{sku}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InventoryDto>> GetInventoryBySku(string sku)
        {
            Log.Information("GET: Inventory with SKU {Sku} requested", sku);
            var inventory = await _inventoryService.GetInventoryBySkuAsync(sku);
            if (inventory == null)
                return NotFound(new { message = $"Inventory not found with SKU {sku}" });

            return Ok(inventory);
        }

        /// <summary>
        /// Create new inventory
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<InventoryDto>> CreateInventory([FromBody] CreateInventoryDto createInventoryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Log.Information("POST: New inventory creation requested for SKU {Sku}", createInventoryDto.ProductSku);

            try
            {
                var inventory = await _inventoryService.CreateInventoryAsync(createInventoryDto);
                return CreatedAtAction(nameof(GetInventoryById), new { id = inventory.Id }, inventory);
            }
            catch (ArgumentException ex)
            {
                Log.Warning("Validation error creating inventory: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update inventory
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateInventory(int id, [FromBody] CreateInventoryDto updateInventoryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Log.Information("PUT: Inventory {InventoryId} update requested", id);

            try
            {
                await _inventoryService.UpdateInventoryAsync(id, updateInventoryDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                Log.Warning("Inventory not found for update: {Error}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete inventory
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteInventory(int id)
        {
            Log.Information("DELETE: Inventory {InventoryId} deletion requested", id);

            try
            {
                await _inventoryService.DeleteInventoryAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                Log.Warning("Inventory not found for deletion: {Error}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get inventory quantity
        /// </summary>
        [HttpGet("{id}/quantity")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<object>> GetQuantity(int id)
        {
            Log.Information("GET: Quantity for inventory {InventoryId} requested", id);

            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
                return NotFound(new { message = $"Inventory with ID {id} not found" });

            var quantity = await _inventoryService.GetQuantityAsync(id);
            return Ok(new { inventoryId = id, quantity });
        }

        /// <summary>
        /// Adjust inventory quantity
        /// </summary>
        [HttpPut("{id}/adjust")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AdjustQuantity(int id, [FromBody] AdjustQuantityDto adjustDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Log.Information("PUT: Adjusting quantity for inventory {InventoryId}", id);

            try
            {
                await _inventoryService.AdjustQuantityAsync(id, adjustDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                Log.Warning("Inventory not found: {Error}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                Log.Warning("Validation error adjusting quantity: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Add transaction
        /// </summary>
        [HttpPost("{id}/transactions")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<InventoryTransactionDto>> AddTransaction(int id, [FromBody] CreateInventoryTransactionDto createTransactionDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Log.Information("POST: Adding transaction for inventory {InventoryId}", id);

            try
            {
                createTransactionDto.InventoryId = id;
                var transaction = await _inventoryService.AddTransactionAsync(createTransactionDto);
                return CreatedAtAction(nameof(GetInventoryById), new { id = id }, transaction);
            }
            catch (KeyNotFoundException ex)
            {
                Log.Warning("Inventory not found: {Error}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                Log.Warning("Validation error adding transaction: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get transactions for inventory
        /// </summary>
        [HttpGet("{id}/transactions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InventoryTransactionDto>>> GetTransactions(int id)
        {
            Log.Information("GET: Transactions for inventory {InventoryId} requested", id);
            var transactions = await _inventoryService.GetTransactionsAsync(id);
            return Ok(transactions);
        }
    }
}
