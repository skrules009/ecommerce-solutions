namespace api_auth_service.Models
{
    public class RolePermission
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int PermissionId { get; set; }

        // Navigation properties
        public Role? Role { get; set; }
        public Permission? Permission { get; set; }
    }
}
