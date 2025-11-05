using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.Security;

namespace TafsilkPlatform.Web.Data.Seed
{
    public static class AdminSeeder
    {
        // Accept IConfiguration and ILogger so sensitive values can be provided via user-secrets / environment
        public static void Seed(AppDbContext db, IConfiguration config, ILogger logger)
        {
            // ensure roles
            var adminRole = db.Roles.FirstOrDefault(r => r.Name == "Admin");
            if (adminRole == null)
            {
                adminRole = new Role { Id = Guid.NewGuid(), Name = "Admin", Description = "Administrator", CreatedAt = DateTime.UtcNow };
                db.Roles.Add(adminRole);
            }

            var userRole = db.Roles.FirstOrDefault(r => r.Name == "Customer");
            if (userRole == null)
            {
                userRole = new Role { Id = Guid.NewGuid(), Name = "Customer", Description = "Customer role", CreatedAt = DateTime.UtcNow };
                db.Roles.Add(userRole);
            }

            db.SaveChanges();

            // Read admin credentials from configuration (user-secrets or env recommended)
            var adminEmail = config["Admin:Email"] ?? string.Empty;
            var adminPassword = config["Admin:Password"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(adminEmail))
            {
                adminEmail = "admin@tafsilk.local"; // fallback default
                logger.LogWarning("Admin email was not configured. Falling back to default 'admin@tafsilk.local'. Set 'Admin:Email' using user-secrets or environment variables to protect it.");
            }

            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                adminPassword = "ChangeMe!123"; // fallback default
                logger.LogWarning("Admin password was not configured. A default password will be used. Set 'Admin:Password' using user-secrets or environment variables and change the password after first login.");
            }

            // ensure admin user exists
            var adminUser = db.Users.FirstOrDefault(u => u.Email == adminEmail);
            if (adminUser == null)
            {
                adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = adminEmail,
                    PasswordHash = PasswordHasher.Hash(adminPassword),
                    RoleId = adminRole.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    EmailVerified = true // Auto-verify admin email
                };
                db.Users.Add(adminUser);
            }

            // Set admin role permissions (replaces Admin table)
            if (string.IsNullOrEmpty(adminRole.Permissions))
            {
                adminRole.Permissions = "{\"CanVerifyTailors\":true,\"CanManageUsers\":true,\"CanViewReports\":true,\"CanManageOrders\":true,\"CanResolveDisputes\":true,\"CanManageRefunds\":true,\"CanSendNotifications\":true,\"CanViewAuditLogs\":true,\"CanManageRoles\":true}";
                adminRole.Priority = 100;
                logger.LogInformation("Admin role permissions configured");
            }

            db.SaveChanges();
            logger.LogInformation("Admin seeding completed. Email: {Email}", adminEmail);
        }
    }
}
