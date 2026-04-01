namespace api_inventory_service.DTOs
{
    public class InventoryTransactionDto
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public string TransactionType { get; set; }
        public int Quantity { get; set; }
        public string Reference { get; set; }
        public string Notes { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
