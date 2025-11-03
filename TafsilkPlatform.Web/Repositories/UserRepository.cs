using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.Data;
using System.Linq.Expressions;

namespace TafsilkPlatform.Web.Repositories;

public class UserRepository : EfRepository<User>, IUserRepository
{
    // Compiled queries for better performance
    private static readonly Func<AppDbContext, string, Task<User?>> _getByEmailCompiledQuery =
        EF.CompileAsyncQuery((AppDbContext db, string email) =>
            db.Users
                .AsNoTracking()
  .FirstOrDefault(u => u.Email == email));

    private static readonly Func<AppDbContext, string, Task<User?>> _getByPhoneCompiledQuery =
        EF.CompileAsyncQuery((AppDbContext db, string phone) =>
            db.Users
    .AsNoTracking()
                .FirstOrDefault(u => u.PhoneNumber == phone));

    private static readonly Func<AppDbContext, string, Task<bool>> _isEmailUniqueCompiledQuery =
        EF.CompileAsyncQuery((AppDbContext db, string email) =>
       !db.Users.Any(u => u.Email == email));

    private static readonly Func<AppDbContext, string, Task<bool>> _isPhoneUniqueCompiledQuery =
        EF.CompileAsyncQuery((AppDbContext db, string phone) =>
          !db.Users.Any(u => u.PhoneNumber == phone));

    public UserRepository(AppDbContext db) : base(db) { }

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
  .Include(u => u.CustomerProfile)
     .Include(u => u.TailorProfile)
            .Include(u => u.CorporateAccount)
            .FirstOrDefaultAsync(u => u.Id == id);
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
