using api_inventory_service.DTOs;

namespace api_inventory_service.Services
{
    public interface IInventoryService
    {
        Task<InventoryDto> GetInventoryByIdAsync(int id);
        Task<InventoryDto> GetInventoryByProductIdAsync(int productId);
        Task<InventoryDto> GetInventoryBySkuAsync(string sku);
        Task<IEnumerable<InventoryDto>> GetAllInventoriesAsync();
        Task<IEnumerable<InventoryDto>> GetLowStockInventoriesAsync();
        Task<IEnumerable<InventoryDto>> GetInventoriesByWarehouseAsync(string warehouse);
        Task<InventoryDto> CreateInventoryAsync(CreateInventoryDto createInventoryDto);
        Task UpdateInventoryAsync(int id, CreateInventoryDto updateInventoryDto);
        Task DeleteInventoryAsync(int id);
        Task<int> GetQuantityAsync(int inventoryId);
        Task AdjustQuantityAsync(int inventoryId, AdjustQuantityDto adjustDto);
        Task<InventoryTransactionDto> AddTransactionAsync(CreateInventoryTransactionDto createTransactionDto);
        Task<IEnumerable<InventoryTransactionDto>> GetTransactionsAsync(int inventoryId);
    }
}
