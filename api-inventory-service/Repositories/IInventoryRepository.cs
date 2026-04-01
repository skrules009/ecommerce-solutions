using api_inventory_service.Models;

namespace api_inventory_service.Repositories
{
    public interface IInventoryRepository
    {
        Task<Inventory> GetInventoryByIdAsync(int id);
        Task<Inventory> GetInventoryByProductIdAsync(int productId);
        Task<Inventory> GetInventoryBySkuAsync(string sku);
        Task<IEnumerable<Inventory>> GetAllInventoriesAsync();
        Task<IEnumerable<Inventory>> GetLowStockInventoriesAsync();
        Task<IEnumerable<Inventory>> GetInventoriesByWarehouseAsync(string warehouse);
        Task<Inventory> CreateInventoryAsync(Inventory inventory);
        Task UpdateInventoryAsync(Inventory inventory);
        Task DeleteInventoryAsync(int id);
        Task<bool> InventoryExistsAsync(int id);
        Task<bool> ProductInventoryExistsAsync(int productId);
        Task<int> GetQuantityAsync(int inventoryId);
        Task UpdateQuantityAsync(int inventoryId, int newQuantity);
        Task<InventoryTransaction> AddTransactionAsync(InventoryTransaction transaction);
        Task<IEnumerable<InventoryTransaction>> GetTransactionsAsync(int inventoryId);
        Task<IEnumerable<InventoryTransaction>> GetTransactionsByDateAsync(DateTime startDate, DateTime endDate);
    }
}
