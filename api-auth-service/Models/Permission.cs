namespace api_auth_service.Models
{
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;  // e.g., "read:products", "write:orders"
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Navigation property
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
