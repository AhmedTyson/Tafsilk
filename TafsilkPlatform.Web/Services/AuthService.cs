using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.ViewModels;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Security;

namespace TafsilkPlatform.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;

        public AuthService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<(bool Succeeded, string? Error, User? User)> RegisterAsync(RegisterRequest request)
        {
            if (await _db.Users.AnyAsync(u => u.Email == request.Email))
                return (false, "Email already taken", null);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = PasswordHasher.Hash(request.Password),
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false,
                RoleId = await EnsureRoleAsync(request.Role)
            };

            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();

            switch (request.Role)
            {
                case RegistrationRole.Customer:
                    await _db.CustomerProfiles.AddAsync(new CustomerProfile
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        FullName = request.FullName ?? string.Empty,
                        CreatedAt = DateTime.UtcNow
                    });
                    break;
                case RegistrationRole.Tailor:
                    await _db.TailorProfiles.AddAsync(new TailorProfile
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        // store the provided full name as display name for tailor
                        FullName = request.FullName,
                        ShopName = request.ShopName ?? string.Empty,
                        Address = request.Address ?? string.Empty,
                        CreatedAt = DateTime.UtcNow,
                        IsVerified = false
                    });
                    break;
                case RegistrationRole.Corporate:
                    await _db.CorporateAccounts.AddAsync(new CorporateAccount
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        CompanyName = request.CompanyName ?? string.Empty,
                        // store the provided full name as contact person if none provided
                        ContactPerson = request.ContactPerson ?? request.FullName ?? string.Empty,
                        CreatedAt = DateTime.UtcNow,
                        IsApproved = false
                    });
                    break;
            }

            await _db.SaveChangesAsync();
            return (true, null, user);
        }

        public async Task<(bool Succeeded, string? Error, User? User)> ValidateUserAsync(string email, string password)
        {
            // Load Role for role-based claims/redirects
            var user = await _db.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user is null) return (false, "Invalid credentials", null);
            if (!PasswordHasher.Verify(user.PasswordHash, password)) return (false, "Invalid credentials", null);
            if (!user.IsActive || user.IsDeleted) return (false, "User is inactive", null);
            return (true, null, user);
        }

        private async Task<Guid> EnsureRoleAsync(RegistrationRole desired)
        {
            var name = desired switch
            {
                RegistrationRole.Customer => "Customer",
                RegistrationRole.Tailor => "Tailor",
                RegistrationRole.Corporate => "Corporate",
                _ => "Customer"
            };

            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == name);
            if (role is not null) return role.Id;

            role = new Role { Id = Guid.NewGuid(), Name = name, CreatedAt = DateTime.UtcNow };
            await _db.Roles.AddAsync(role);
            await _db.SaveChangesAsync();
            return role.Id;
        }
    }
}
