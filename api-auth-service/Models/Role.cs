namespace api_auth_service.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;  // "User" or "Admin"
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Navigation property
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
