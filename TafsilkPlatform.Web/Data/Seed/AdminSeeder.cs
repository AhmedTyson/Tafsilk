using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.Security;

namespace TafsilkPlatform.Web.Data.Seed
{
    public static class AdminSeeder
    {
        // Accept IConfiguration and ILogger so sensitive values can be provided via user-secrets / environment
        public static void Seed(AppDbContext db, IConfiguration config, ILogger logger)
        {
            // ✅ Ensure all roles exist
            var adminRole = EnsureRole(db, "Admin", "Administrator with full access", 100);
            var customerRole = EnsureRole(db, "Customer", "Customer role", 10);
            var tailorRole = EnsureRole(db, "Tailor", "Tailor/Service Provider role", 20);

            db.SaveChanges();

            // Read admin credentials from configuration
            var adminEmail = config["Admin:Email"] ?? "admin@tafsilk.local";
            var adminPassword = config["Admin:Password"] ?? "Admin@123!";
            
            // ✅ Create tester account (admin with all roles)
            var testerEmail = config["Tester:Email"] ?? "tester@tafsilk.local";
            var testerPassword = config["Tester:Password"] ?? "Tester@123!";

            if (string.IsNullOrWhiteSpace(adminEmail) || adminEmail == "admin@tafsilk.local")
 {
       logger.LogWarning("Admin email not configured. Using default 'admin@tafsilk.local'. Set 'Admin:Email' in user-secrets.");
            }

            if (string.IsNullOrWhiteSpace(adminPassword) || adminPassword == "Admin@123!")
      {
  logger.LogWarning("Admin password not configured. Using default. Set 'Admin:Password' in user-secrets and change after first login.");
         }

         // ✅ Ensure admin user exists
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

          // ✅ NEW: Create tester/admin user for testing all pages
         var testerUser = db.Users.FirstOrDefault(u => u.Email == testerEmail);
       if (testerUser == null)
   {
      var testerId = Guid.NewGuid();
                testerUser = new User
         {
           Id = testerId,
  Email = testerEmail,
             PasswordHash = PasswordHasher.Hash(testerPassword),
          RoleId = adminRole.Id, // Admin role
 IsActive = true,
          CreatedAt = DateTime.UtcNow,
     EmailVerified = true
            };
        db.Users.Add(testerUser);

            // ✅ Create CustomerProfile for tester
 var customerProfile = new CustomerProfile
 {
              Id = Guid.NewGuid(),
   UserId = testerId,
       FullName = "Tester Account",
          Gender = "Other",
   City = "Test City",
                Bio = "Test account with full platform access",
         CreatedAt = DateTime.UtcNow
      };
       db.CustomerProfiles.Add(customerProfile);

   // ✅ Create TailorProfile for tester
        var tailorProfile = new TailorProfile
     {
       Id = Guid.NewGuid(),
            UserId = testerId,
    FullName = "Tester Tailor",
         ShopName = "Test Tailor Shop",
     Address = "Test Address, Test City",
          City = "Test City",
                Bio = "Test tailor profile for testing purposes",
    Specialization = "تفصيل عام",
   IsVerified = true, // Auto-verify for testing
ExperienceYears = 5,
          AverageRating = 4.5m,
    CreatedAt = DateTime.UtcNow
              };
       db.TailorProfiles.Add(tailorProfile);

                logger.LogInformation("✅ Tester account created: {Email} (can access all pages)", testerEmail);
            }

            // Set admin role permissions
      if (string.IsNullOrEmpty(adminRole.Permissions))
         {
          adminRole.Permissions = "{\"CanVerifyTailors\":true,\"CanManageUsers\":true,\"CanViewReports\":true,\"CanManageOrders\":true,\"CanResolveDisputes\":true,\"CanManageRefunds\":true,\"CanSendNotifications\":true,\"CanViewAuditLogs\":true,\"CanManageRoles\":true,\"CanAccessAllPages\":true}";
       adminRole.Priority = 100;
       logger.LogInformation("✅ Admin role permissions configured");
       }

  db.SaveChanges();
            
            logger.LogInformation("✅ Seeding completed:");
            logger.LogInformation(" - Admin: {AdminEmail}", adminEmail);
   logger.LogInformation("   - Tester: {TesterEmail} (Password: {TesterPassword})", testerEmail, testerPassword);
       logger.LogInformation("   - Tester has both Customer and Tailor profiles for full testing");
      }

        private static Role EnsureRole(AppDbContext db, string roleName, string description, int priority)
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
