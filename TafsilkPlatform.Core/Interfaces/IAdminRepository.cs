using TafsilkPlatform.Core.Models;

namespace TafsilkPlatform.Core.Interfaces
{
    public interface IAdminRepository : IRepository<Admin>
    {
        Task<Admin?> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Admin>> GetAdminsByPermissionAsync(string permission);
        Task<bool> UpdateAdminPermissionsAsync(Guid adminId, string permissions);
    }
}
