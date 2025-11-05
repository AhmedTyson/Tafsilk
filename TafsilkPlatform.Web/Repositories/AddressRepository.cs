using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Models;

namespace TafsilkPlatform.Web.Repositories;

public class AddressRepository : EfRepository<UserAddress>, IAddressRepository
{
    public AddressRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<UserAddress>> GetByUserIdAsync(Guid userId)
        => await _db.UserAddresses.Where(a => a.UserId == userId).AsNoTracking().ToListAsync();

    public Task<UserAddress?> GetDefaultAddressAsync(Guid userId)
        => _db.UserAddresses.AsNoTracking().FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault);

    public async Task<bool> SetDefaultAddressAsync(Guid addressId, Guid userId)
    {
        var userAddresses = await _db.UserAddresses.Where(a => a.UserId == userId).ToListAsync();
        if (userAddresses.Count == 0) return false;
        foreach (var a in userAddresses) a.IsDefault = a.Id == addressId;
        _db.UserAddresses.UpdateRange(userAddresses);
        return true;
    }

    public async Task<bool> RemoveAddressAsync(Guid addressId)
    {
        var entity = await _db.UserAddresses.FindAsync(addressId);
        if (entity is null) return false;
        _db.UserAddresses.Remove(entity);
        return true;
    }
}
