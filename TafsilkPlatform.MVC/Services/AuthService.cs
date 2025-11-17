using System.Security.Cryptography;
using System.Text;
using TafsilkPlatform.MVC.Models;

namespace TafsilkPlatform.MVC.Services
{
    public interface IAuthService
    {
        Task<User?> AuthenticateAsync(string email, string password);
        Task<(bool Success, string? ErrorMessage)> RegisterAsync(RegisterViewModel model);
        User? GetUserByEmail(string email);
    }

    /// <summary>
 /// Authentication service with REAL password validation and hashing
    /// </summary>
    public class AuthService : IAuthService
    {
        // In-memory user storage (simulates database)
        private static readonly List<User> _users = new()
    {
            new User
  {
   Id = Guid.NewGuid(),
  Email = "customer@test.com",
           PasswordHash = HashPassword("123456"),
    FullName = "أحمد محمد",
      Role = "Customer",
             CreatedAt = DateTime.UtcNow
        },
      new User
            {
         Id = Guid.NewGuid(),
   Email = "tailor@test.com",
      PasswordHash = HashPassword("123456"),
     FullName = "محمد الخياط",
        Role = "Tailor",
       CreatedAt = DateTime.UtcNow
        },
 new User
 {
Id = Guid.NewGuid(),
                Email = "admin@test.com",
       PasswordHash = HashPassword("admin123"),
    FullName = "مدير النظام",
  Role = "Admin",
         CreatedAt = DateTime.UtcNow
          }
        };

     public async Task<User?> AuthenticateAsync(string email, string password)
        {
     await Task.CompletedTask; // Simulate async operation

   var user = _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
      
     if (user == null)
                return null;

   // REAL password verification
   var hashedPassword = HashPassword(password);
            if (user.PasswordHash != hashedPassword)
         return null;

            user.LastLoginAt = DateTime.UtcNow;
            return user;
    }

        public async Task<(bool Success, string? ErrorMessage)> RegisterAsync(RegisterViewModel model)
        {
            await Task.CompletedTask; // Simulate async operation

            // Check if email already exists
 if (_users.Any(u => u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase)))
            {
 return (false, "البريد الإلكتروني مستخدم بالفعل");
     }

            var newUser = new User
            {
        Id = Guid.NewGuid(),
    Email = model.Email,
   PasswordHash = HashPassword(model.Password),
      FullName = model.FullName,
        Role = model.Role,
     CreatedAt = DateTime.UtcNow
  };

 _users.Add(newUser);
     return (true, null);
    }

        public User? GetUserByEmail(string email)
      {
         return _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        // REAL password hashing using SHA256
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
       var hash = sha256.ComputeHash(bytes);
  return Convert.ToBase64String(hash);
     }
    }
}
