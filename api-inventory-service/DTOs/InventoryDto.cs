namespace api_inventory_service.DTOs
{
    public class InventoryDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductSku { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public int ReorderLevel { get; set; }
        public int ReorderQuantity { get; set; }
        public string Warehouse { get; set; }
        public string Location { get; set; }
        public DateTime LastRestockedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool NeedsReorder => Quantity <= ReorderLevel;
        public List<InventoryTransactionDto> Transactions { get; set; } = new();
    }
}
