using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Core.Interfaces;
using TafsilkPlatform.Core.Models;
using TafsilkPlatform.Infrastructure.Persistence;


namespace TafsilkPlatform.Infrastructure.Repositories;

public class AdminRepository : EfRepository<Admin>, IAdminRepository
{
    public AdminRepository(AppDbContext db) : base(db) { }

    public Task<Admin?> GetByUserIdAsync(Guid userId)
        => _db.Admins.AsNoTracking().FirstOrDefaultAsync(a => a.UserId == userId);

    public async Task<IEnumerable<Admin>> GetAdminsByPermissionAsync(string permission)
        => await _db.Admins.Where(a => a.Permissions.Contains(permission)).AsNoTracking().ToListAsync();

    public async Task<bool> UpdateAdminPermissionsAsync(Guid adminId, string permissions)
    {
        var entity = await _db.Admins.FindAsync(adminId);
        if (entity is null) return false;
        entity.Permissions = permissions;
        _db.Admins.Update(entity);
        return true;
    }
}
