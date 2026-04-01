namespace api_inventory_service.Models
{
    public class InventoryTransaction
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public string TransactionType { get; set; } // In, Out, Adjustment, Return
        public int Quantity { get; set; }
        public string Reference { get; set; } // Order#, PO#, etc.
        public string Notes { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual Inventory Inventory { get; set; }
    }
}
