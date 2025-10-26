using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Core.Interfaces;
using TafsilkPlatform.Core.Models;
using TafsilkPlatform.Infrastructure.Persistence;

namespace TafsilkPlatform.Infrastructure.Repositories;

public class UserRepository : EfRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext db) : base(db) { }

    public Task<User?> GetByEmailAsync(string email)
        => _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);

    public Task<User?> GetByPhoneAsync(string phoneNumber)
        => _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

    public Task<bool> IsEmailUniqueAsync(string email)
        => _db.Users.AllAsync(u => u.Email != email);

    public Task<bool> IsPhoneUniqueAsync(string phoneNumber)
        => _db.Users.AllAsync(u => u.PhoneNumber != phoneNumber);

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
        => await _db.Users.Include(u => u.Role).Where(u => u.Role.Name == role).AsNoTracking().ToListAsync();

    public Task<User?> GetUserWithProfileAsync(Guid id)
        => _db.Users
            .Include(u => u.CustomerProfile)
            .Include(u => u.TailorProfile)
            .Include(u => u.CorporateAccount)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

    public async Task UpdateUserStatusAsync(Guid userId, bool isActive)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return;
        user.IsActive = isActive;
        _db.Users.Update(user);
    }
}
