using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.DataAccess.Data;
using TafsilkPlatform.DataAccess.Repository;
using TafsilkPlatform.Models.Models;

namespace TafsilkPlatform.DataAccess.Repository;

public class UserRepository : EfRepository<User>, IUserRepository
{
    // Compiled queries for better performance
    private static readonly Func<ApplicationDbContext, string, Task<User?>> _getByEmailCompiledQuery =
        EF.CompileAsyncQuery((ApplicationDbContext db, string email) =>
            db.Users
                .AsNoTracking()
  .FirstOrDefault(u => u.Email == email));

    private static readonly Func<ApplicationDbContext, string, Task<User?>> _getByPhoneCompiledQuery =
        EF.CompileAsyncQuery((ApplicationDbContext db, string phone) =>
            db.Users
    .AsNoTracking()
                .FirstOrDefault(u => u.PhoneNumber == phone));

    private static readonly Func<ApplicationDbContext, string, Task<bool>> _isEmailUniqueCompiledQuery =
        EF.CompileAsyncQuery((ApplicationDbContext db, string email) =>
       !db.Users.Any(u => u.Email == email));

    private static readonly Func<ApplicationDbContext, string, Task<bool>> _isPhoneUniqueCompiledQuery =
        EF.CompileAsyncQuery((ApplicationDbContext db, string phone) =>
          !db.Users.Any(u => u.PhoneNumber == phone));

    public UserRepository(ApplicationDbContext db) : base(db) { }

    public Task<User?> GetByEmailAsync(string email)
  => _getByEmailCompiledQuery(_db, email);

    public Task<User?> GetByPhoneAsync(string phoneNumber)
        => _getByPhoneCompiledQuery(_db, phoneNumber);

    public Task<bool> IsEmailUniqueAsync(string email)
        => _isEmailUniqueCompiledQuery(_db, email);

    public Task<bool> IsPhoneUniqueAsync(string phoneNumber)
        => _isPhoneUniqueCompiledQuery(_db, phoneNumber);

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
    {
        // Optimized: Only include Role, not all profiles
        return await _db.Users
            .AsNoTracking()
   .Include(u => u.Role)
       .Where(u => u.Role.Name == role)
            .ToListAsync();
    }

    public async Task<User?> GetUserWithProfileAsync(Guid id)
    {
        // Use AsSplitQuery to avoid cartesian explosion with multiple includes
        return await _db.Users
            .AsNoTracking()
            .AsSplitQuery()
            .Include(u => u.Role)              // ✅ CRITICAL: Include Role!
            .Include(u => u.CustomerProfile)
     .Include(u => u.TailorProfile)
      .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
    }

    public async Task UpdateUserStatusAsync(Guid userId, bool isActive)
    {
        // More efficient update without loading the entire entity
        await _db.Users
         .Where(u => u.Id == userId)
         .ExecuteUpdateAsync(setters => setters
      .SetProperty(u => u.IsActive, isActive)
    .SetProperty(u => u.UpdatedAt, DateTime.UtcNow));
    }
}
