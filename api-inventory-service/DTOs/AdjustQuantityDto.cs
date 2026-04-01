namespace api_inventory_service.DTOs
{
    public class AdjustQuantityDto
    {
        public int Quantity { get; set; }
        public string Reference { get; set; }
        public string Notes { get; set; }
        public string CreatedBy { get; set; }
    }
}
