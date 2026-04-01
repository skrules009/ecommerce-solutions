namespace api_inventory_service.DTOs
{
    public class CreateInventoryDto
    {
        public int ProductId { get; set; }
        public string ProductSku { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public int ReorderLevel { get; set; }
        public int ReorderQuantity { get; set; }
        public string Warehouse { get; set; }
        public string Location { get; set; }
    }
}
