using api_inventory_service.Data;
using api_inventory_service.Models;
using Microsoft.EntityFrameworkCore;

namespace api_inventory_service.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly InventoryDbContext _context;

        public InventoryRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<Inventory> GetInventoryByIdAsync(int id)
        {
            return await _context.Inventories
                .Include(i => i.Transactions)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Inventory> GetInventoryByProductIdAsync(int productId)
        {
            return await _context.Inventories
                .Include(i => i.Transactions)
                .FirstOrDefaultAsync(i => i.ProductId == productId);
        }

        public async Task<Inventory> GetInventoryBySkuAsync(string sku)
        {
            return await _context.Inventories
                .Include(i => i.Transactions)
                .FirstOrDefaultAsync(i => i.ProductSku.ToLower() == sku.ToLower());
        }

        public async Task<IEnumerable<Inventory>> GetAllInventoriesAsync()
        {
            return await _context.Inventories
                .Include(i => i.Transactions)
                .OrderBy(i => i.ProductName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inventory>> GetLowStockInventoriesAsync()
        {
            return await _context.Inventories
                .Include(i => i.Transactions)
                .Where(i => i.Quantity <= i.ReorderLevel)
                .OrderBy(i => i.Quantity)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inventory>> GetInventoriesByWarehouseAsync(string warehouse)
        {
            return await _context.Inventories
                .Include(i => i.Transactions)
                .Where(i => i.Warehouse.ToLower() == warehouse.ToLower())
                .OrderBy(i => i.Location)
                .ToListAsync();
        }

        public async Task<Inventory> CreateInventoryAsync(Inventory inventory)
        {
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();
            return inventory;
        }

        public async Task UpdateInventoryAsync(Inventory inventory)
        {
            inventory.UpdatedAt = DateTime.UtcNow;
            _context.Inventories.Update(inventory);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteInventoryAsync(int id)
        {
            var inventory = await GetInventoryByIdAsync(id);
            if (inventory != null)
            {
                _context.Inventories.Remove(inventory);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> InventoryExistsAsync(int id)
        {
            return await _context.Inventories.AnyAsync(i => i.Id == id);
        }

        public async Task<bool> ProductInventoryExistsAsync(int productId)
        {
            return await _context.Inventories.AnyAsync(i => i.ProductId == productId);
        }

        public async Task<int> GetQuantityAsync(int inventoryId)
        {
            var inventory = await GetInventoryByIdAsync(inventoryId);
            return inventory?.Quantity ?? 0;
        }

        public async Task UpdateQuantityAsync(int inventoryId, int newQuantity)
        {
            var inventory = await GetInventoryByIdAsync(inventoryId);
            if (inventory != null)
            {
                inventory.Quantity = newQuantity;
                inventory.UpdatedAt = DateTime.UtcNow;
                await UpdateInventoryAsync(inventory);
            }
        }

        public async Task<InventoryTransaction> AddTransactionAsync(InventoryTransaction transaction)
        {
            _context.InventoryTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<IEnumerable<InventoryTransaction>> GetTransactionsAsync(int inventoryId)
        {
            return await _context.InventoryTransactions
                .Where(it => it.InventoryId == inventoryId)
                .OrderByDescending(it => it.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryTransaction>> GetTransactionsByDateAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.InventoryTransactions
                .Where(it => it.CreatedAt >= startDate && it.CreatedAt <= endDate)
                .OrderByDescending(it => it.CreatedAt)
                .ToListAsync();
        }
    }
}
