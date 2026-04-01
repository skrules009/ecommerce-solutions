using api_inventory_service.DTOs;
using api_inventory_service.Models;
using api_inventory_service.Repositories;
using Serilog;

namespace api_inventory_service.Services
{
    public class InventoryServiceImpl : IInventoryService
    {
        private readonly IInventoryRepository _repository;

        public InventoryServiceImpl(IInventoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<InventoryDto> GetInventoryByIdAsync(int id)
        {
            Log.Information("Getting inventory by ID: {InventoryId}", id);
            var inventory = await _repository.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                Log.Warning("Inventory not found with ID: {InventoryId}", id);
                return null;
            }
            return MapToDto(inventory);
        }

        public async Task<InventoryDto> GetInventoryByProductIdAsync(int productId)
        {
            Log.Information("Getting inventory for product: {ProductId}", productId);
            var inventory = await _repository.GetInventoryByProductIdAsync(productId);
            return inventory != null ? MapToDto(inventory) : null;
        }

        public async Task<InventoryDto> GetInventoryBySkuAsync(string sku)
        {
            Log.Information("Getting inventory by SKU: {Sku}", sku);
            var inventory = await _repository.GetInventoryBySkuAsync(sku);
            return inventory != null ? MapToDto(inventory) : null;
        }

        public async Task<IEnumerable<InventoryDto>> GetAllInventoriesAsync()
        {
            Log.Information("Getting all inventories");
            var inventories = await _repository.GetAllInventoriesAsync();
            return inventories.Select(MapToDto).ToList();
        }

        public async Task<IEnumerable<InventoryDto>> GetLowStockInventoriesAsync()
        {
            Log.Information("Getting low stock inventories");
            var inventories = await _repository.GetLowStockInventoriesAsync();
            return inventories.Select(MapToDto).ToList();
        }

        public async Task<IEnumerable<InventoryDto>> GetInventoriesByWarehouseAsync(string warehouse)
        {
            Log.Information("Getting inventories for warehouse: {Warehouse}", warehouse);
            var inventories = await _repository.GetInventoriesByWarehouseAsync(warehouse);
            return inventories.Select(MapToDto).ToList();
        }

        public async Task<InventoryDto> CreateInventoryAsync(CreateInventoryDto createInventoryDto)
        {
            Log.Information("Creating new inventory for product: {ProductSku}", createInventoryDto.ProductSku);

            // Check if product already has inventory
            if (await _repository.ProductInventoryExistsAsync(createInventoryDto.ProductId))
            {
                throw new ArgumentException($"Inventory already exists for product ID: {createInventoryDto.ProductId}");
            }

            if (createInventoryDto.ReorderLevel < 0 || createInventoryDto.ReorderQuantity < 0)
            {
                throw new ArgumentException("Reorder level and quantity must be non-negative");
            }

            var inventory = new Inventory
            {
                ProductId = createInventoryDto.ProductId,
                ProductSku = createInventoryDto.ProductSku.ToUpper(),
                ProductName = createInventoryDto.ProductName,
                Quantity = createInventoryDto.Quantity,
                ReorderLevel = createInventoryDto.ReorderLevel,
                ReorderQuantity = createInventoryDto.ReorderQuantity,
                Warehouse = createInventoryDto.Warehouse,
                Location = createInventoryDto.Location,
                LastRestockedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            var createdInventory = await _repository.CreateInventoryAsync(inventory);
            Log.Information("Inventory created successfully: {InventoryId}", createdInventory.Id);

            return MapToDto(createdInventory);
        }

        public async Task UpdateInventoryAsync(int id, CreateInventoryDto updateInventoryDto)
        {
            Log.Information("Updating inventory: {InventoryId}", id);

            var inventory = await _repository.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                throw new KeyNotFoundException($"Inventory not found with ID: {id}");
            }

            inventory.ProductName = updateInventoryDto.ProductName;
            inventory.ReorderLevel = updateInventoryDto.ReorderLevel;
            inventory.ReorderQuantity = updateInventoryDto.ReorderQuantity;
            inventory.Warehouse = updateInventoryDto.Warehouse;
            inventory.Location = updateInventoryDto.Location;

            await _repository.UpdateInventoryAsync(inventory);
            Log.Information("Inventory updated successfully: {InventoryId}", id);
        }

        public async Task DeleteInventoryAsync(int id)
        {
            Log.Information("Deleting inventory: {InventoryId}", id);

            var exists = await _repository.InventoryExistsAsync(id);
            if (!exists)
            {
                throw new KeyNotFoundException($"Inventory not found with ID: {id}");
            }

            await _repository.DeleteInventoryAsync(id);
            Log.Information("Inventory deleted successfully: {InventoryId}", id);
        }

        public async Task<int> GetQuantityAsync(int inventoryId)
        {
            Log.Information("Getting quantity for inventory: {InventoryId}", inventoryId);
            return await _repository.GetQuantityAsync(inventoryId);
        }

        public async Task AdjustQuantityAsync(int inventoryId, AdjustQuantityDto adjustDto)
        {
            Log.Information("Adjusting quantity for inventory {InventoryId}: {Quantity}", inventoryId, adjustDto.Quantity);

            var inventory = await _repository.GetInventoryByIdAsync(inventoryId);
            if (inventory == null)
            {
                throw new KeyNotFoundException($"Inventory not found with ID: {inventoryId}");
            }

            var oldQuantity = inventory.Quantity;
            inventory.Quantity = adjustDto.Quantity;
            inventory.LastRestockedAt = DateTime.UtcNow;

            await _repository.UpdateInventoryAsync(inventory);

            // Add transaction record
            var transaction = new InventoryTransaction
            {
                InventoryId = inventoryId,
                TransactionType = "Adjustment",
                Quantity = adjustDto.Quantity - oldQuantity,
                Reference = adjustDto.Reference,
                Notes = adjustDto.Notes,
                CreatedBy = adjustDto.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddTransactionAsync(transaction);
            Log.Information("Quantity adjusted for inventory {InventoryId}: {OldQuantity} -> {NewQuantity}",
                inventoryId, oldQuantity, adjustDto.Quantity);
        }

        public async Task<InventoryTransactionDto> AddTransactionAsync(CreateInventoryTransactionDto createTransactionDto)
        {
            Log.Information("Adding transaction for inventory {InventoryId}", createTransactionDto.InventoryId);

            var inventory = await _repository.GetInventoryByIdAsync(createTransactionDto.InventoryId);
            if (inventory == null)
            {
                throw new KeyNotFoundException($"Inventory not found with ID: {createTransactionDto.InventoryId}");
            }

            // Update inventory quantity based on transaction type
            if (createTransactionDto.TransactionType == "In")
            {
                inventory.Quantity += createTransactionDto.Quantity;
                inventory.LastRestockedAt = DateTime.UtcNow;
            }
            else if (createTransactionDto.TransactionType == "Out")
            {
                if (inventory.Quantity < createTransactionDto.Quantity)
                {
                    throw new ArgumentException("Insufficient inventory for this transaction");
                }
                inventory.Quantity -= createTransactionDto.Quantity;
            }
            else if (createTransactionDto.TransactionType == "Return")
            {
                inventory.Quantity += createTransactionDto.Quantity;
            }

            await _repository.UpdateInventoryAsync(inventory);

            var transaction = new InventoryTransaction
            {
                InventoryId = createTransactionDto.InventoryId,
                TransactionType = createTransactionDto.TransactionType,
                Quantity = createTransactionDto.Quantity,
                Reference = createTransactionDto.Reference,
                Notes = createTransactionDto.Notes,
                CreatedBy = createTransactionDto.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            var createdTransaction = await _repository.AddTransactionAsync(transaction);
            Log.Information("Transaction added: {TransactionId}", createdTransaction.Id);

            return MapTransactionToDto(createdTransaction);
        }

        public async Task<IEnumerable<InventoryTransactionDto>> GetTransactionsAsync(int inventoryId)
        {
            Log.Information("Getting transactions for inventory {InventoryId}", inventoryId);
            var transactions = await _repository.GetTransactionsAsync(inventoryId);
            return transactions.Select(MapTransactionToDto).ToList();
        }

        private InventoryDto MapToDto(Inventory inventory)
        {
            return new InventoryDto
            {
                Id = inventory.Id,
                ProductId = inventory.ProductId,
                ProductSku = inventory.ProductSku,
                ProductName = inventory.ProductName,
                Quantity = inventory.Quantity,
                ReorderLevel = inventory.ReorderLevel,
                ReorderQuantity = inventory.ReorderQuantity,
                Warehouse = inventory.Warehouse,
                Location = inventory.Location,
                LastRestockedAt = inventory.LastRestockedAt,
                CreatedAt = inventory.CreatedAt,
                UpdatedAt = inventory.UpdatedAt,
                Transactions = inventory.Transactions?.Select(MapTransactionToDto).ToList() ?? new()
            };
        }

        private InventoryTransactionDto MapTransactionToDto(InventoryTransaction transaction)
        {
            return new InventoryTransactionDto
            {
                Id = transaction.Id,
                InventoryId = transaction.InventoryId,
                TransactionType = transaction.TransactionType,
                Quantity = transaction.Quantity,
                Reference = transaction.Reference,
                Notes = transaction.Notes,
                CreatedBy = transaction.CreatedBy,
                CreatedAt = transaction.CreatedAt
            };
        }
    }
}
