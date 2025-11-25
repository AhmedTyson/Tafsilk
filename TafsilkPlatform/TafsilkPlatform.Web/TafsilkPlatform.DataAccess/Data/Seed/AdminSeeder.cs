using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TafsilkPlatform.Models.Models;
using TafsilkPlatform.Utility.Security;

namespace TafsilkPlatform.DataAccess.Data.Seed
{
    public static class AdminSeeder
    {
        // Accept IConfiguration and ILogger so sensitive values can be provided via user-secrets / environment
        public static void Seed(ApplicationDbContext db, IConfiguration config, ILogger logger)
        {
            // ✅ Ensure all roles exist
            var adminRole = EnsureRole(db, "Admin", "Administrator with full access", 100);
            var customerRole = EnsureRole(db, "Customer", "Customer role", 10);
            var tailorRole = EnsureRole(db, "Tailor", "Tailor/Service Provider role", 20);

            db.SaveChanges();

            // Read admin credentials from configuration
            var adminEmail = config["Admin:Email"] ?? "admin@tafsilk.local";
            var adminPassword = config["Admin:Password"] ?? "Admin@123!";

            if (string.IsNullOrWhiteSpace(adminEmail) || adminEmail == "admin@tafsilk.local")
            {
                logger.LogWarning("Admin email not configured. Using default 'admin@tafsilk.local'. Set 'Admin:Email' in user-secrets.");
            }

            if (string.IsNullOrWhiteSpace(adminPassword) || adminPassword == "Admin@123!")
            {
                logger.LogWarning("Admin password not configured. Using default. Set 'Admin:Password' in user-secrets and change after first login.");
            }

            // ✅ Ensure admin user exists and can log in directly
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
                    EmailVerified = true
                };
                db.Users.Add(adminUser);
                logger.LogInformation("✅ Admin user created: {Email}", adminEmail);
            }
            else
            {
                // Ensure admin has correct role, active state and verified email so they can login
                adminUser.RoleId = adminRole.Id;
                adminUser.IsActive = true;
                adminUser.EmailVerified = true;
                // If password not set or empty, set it to configured adminPassword
                if (string.IsNullOrEmpty(adminUser.PasswordHash))
                {
                    adminUser.PasswordHash = PasswordHasher.Hash(adminPassword);
                }
                logger.LogInformation("✅ Admin user ensured: {Email}", adminEmail);
            }

            // Ensure admin has customer profile so pages that require a customer profile work
            var adminCustomerProfile = db.CustomerProfiles.FirstOrDefault(p => p.UserId == adminUser.Id);
            if (adminCustomerProfile == null)
            {
                adminCustomerProfile = new CustomerProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = adminUser.Id,
                    FullName = "Administrator",
                    Gender = "Other",
                    City = "Admin City",
                    CreatedAt = DateTime.UtcNow
                };
                db.CustomerProfiles.Add(adminCustomerProfile);
                logger.LogInformation("✅ Customer profile created for admin user: {Email}", adminEmail);
            }

            // Ensure admin has a tailor profile so pages that require tailor profile work
            var adminTailorProfile = db.TailorProfiles.FirstOrDefault(t => t.UserId == adminUser.Id);
            if (adminTailorProfile == null)
            {
                adminTailorProfile = new TailorProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = adminUser.Id,
                    FullName = "Admin Tailor",
                    ShopName = "Admin Tailor Shop",
                    City = "Admin City",
                    Address = "Admin Address",
                    Bio = "Administrator tailor profile for platform-wide access",
                    Specialization = "General",
                    IsVerified = true,
                    ExperienceYears = 0,
                    AverageRating = 5.0m,
                    CreatedAt = DateTime.UtcNow
                };
                db.TailorProfiles.Add(adminTailorProfile);
                logger.LogInformation("✅ Tailor profile created for admin user: {Email}", adminEmail);
            }

            // Set admin role permissions - ensure CanAccessAllPages and audit/payment permissions are present
            // If permissions already exist, attempt to merge required keys; otherwise set default full permission set
            var requiredPermissions = new Dictionary<string, object>
            {
                { "CanVerifyTailors", true },
                { "CanManageUsers", true },
                { "CanViewReports", true },
                { "CanManageOrders", true },
                { "CanResolveDisputes", true },
                { "CanManageRefunds", true },
                { "CanSendNotifications", true },
                { "CanViewAuditLogs", true },
                { "CanViewCustomerAudit", true },
                { "CanViewTailorAudit", true },
                { "CanManageRoles", true },
                { "CanAccessAllPages", true },
                { "CanViewPayments", true },
                { "CanViewTotalPayments", true }
            };

            try
            {
                if (string.IsNullOrWhiteSpace(adminRole.Permissions))
                {
                    adminRole.Permissions = System.Text.Json.JsonSerializer.Serialize(requiredPermissions);
                }
                else
                {
                    // Merge existing JSON with required keys (existing keys preserved)
                    var existing = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(adminRole.Permissions) ?? new Dictionary<string, object>();
                    foreach (var kv in requiredPermissions)
                    {
                        if (!existing.ContainsKey(kv.Key)) existing[kv.Key] = kv.Value;
                    }
                    adminRole.Permissions = System.Text.Json.JsonSerializer.Serialize(existing);
                }
                adminRole.Priority = Math.Max(adminRole.Priority, 100);
                logger.LogInformation("✅ Admin role permissions ensured and updated");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update admin role permissions; setting default permission JSON");
                adminRole.Permissions = System.Text.Json.JsonSerializer.Serialize(requiredPermissions);
            }

            db.SaveChanges();

            logger.LogInformation("✅ Seeding completed:");
            logger.LogInformation(" - Admin: {AdminEmail}", adminEmail);
            logger.LogInformation("   - Admin has customer and tailor profiles and required audit/payment permissions");
        }

        private static Role EnsureRole(ApplicationDbContext db, string roleName, string description, int priority)
        {
            var role = db.Roles.FirstOrDefault(r => r.Name == roleName);
            if (role == null)
            {
                role = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = roleName,
                    Description = description,
                    Priority = priority,
                    CreatedAt = DateTime.UtcNow
                };
                db.Roles.Add(role);
            }
            return role;
        }
    }
}
