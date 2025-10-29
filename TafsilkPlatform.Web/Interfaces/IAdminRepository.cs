using TafsilkPlatform.Web.Models;

namespace TafsilkPlatform.Web.Interfaces
{
    public interface IAdminRepository : IRepository<Admin>
    {
        Task<Admin?> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Admin>> GetAdminsByPermissionAsync(string permission);
        Task<bool> UpdateAdminPermissionsAsync(Guid adminId, string permissions);
    }
}
