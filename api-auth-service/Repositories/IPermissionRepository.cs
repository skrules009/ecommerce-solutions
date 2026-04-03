using api_auth_service.Models;

namespace api_auth_service.Repositories
{
    public interface IPermissionRepository
    {
        Task<Permission?> GetByIdAsync(int id);
        Task<Permission?> GetByNameAsync(string name);
        Task<IEnumerable<Permission>> GetByRoleIdAsync(int roleId);
        Task<IEnumerable<Permission>> GetAllAsync();
        Task<Permission> CreateAsync(Permission permission);
        Task<Permission> UpdateAsync(Permission permission);
        Task<bool> DeleteAsync(int id);
    }
}
