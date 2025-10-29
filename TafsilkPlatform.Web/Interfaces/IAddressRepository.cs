using TafsilkPlatform.Web.Models;

namespace TafsilkPlatform.Web.Interfaces
{
    public interface IAddressRepository : IRepository<UserAddress>
    {
        Task<IEnumerable<UserAddress>> GetByUserIdAsync(Guid userId);
        Task<UserAddress?> GetDefaultAddressAsync(Guid userId);
        Task<bool> SetDefaultAddressAsync(Guid addressId, Guid userId);
        Task<bool> RemoveAddressAsync(Guid addressId);
    }
}
