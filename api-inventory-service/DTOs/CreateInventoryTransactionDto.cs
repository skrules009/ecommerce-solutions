namespace api_inventory_service.DTOs
{
    public class CreateInventoryTransactionDto
    {
        public int InventoryId { get; set; }
        public string TransactionType { get; set; } // In, Out, Adjustment, Return
        public int Quantity { get; set; }
        public string Reference { get; set; }
        public string Notes { get; set; }
        public string CreatedBy { get; set; }
    }
}
