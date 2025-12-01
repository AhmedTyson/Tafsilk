using Microsoft.Extensions.Logging;
using TafsilkPlatform.Models.Models;
using TafsilkPlatform.Utility.Security;

namespace TafsilkPlatform.DataAccess.Data.Seed
{
    public static class UserSeeder
    {
        public static void Seed(ApplicationDbContext db, ILogger logger)
        {
            logger.LogInformation("🌱 Starting UserSeeder (Egypt Localization)...");

            // Get roles
            var customerRole = db.Roles.FirstOrDefault(r => r.Name == "Customer");
            var tailorRole = db.Roles.FirstOrDefault(r => r.Name == "Tailor");

            if (customerRole == null || tailorRole == null)
            {
                logger.LogError("Required roles not found. Cannot seed users.");
                return;
            }

            var users = new List<User>();
            var customerProfiles = new List<CustomerProfile>();

            // Egyptian cities
            var cities = new[] { "Cairo", "Alexandria", "Giza", "Luxor", "Aswan", "Hurghada", "Sharm El Sheikh", "Port Said", "Suez", "Mansoura" };
            var genders = new[] { "Male", "Female" };

            // Create 10 Tailor Users (profiles will be created by TailorSeeder)
            var tailorNames = new[]
            {
                "Mohamed Hassan", "Fatima Youssef", "Ahmed Ali", "Aisha Mohamed",
                "Mahmoud Ibrahim", "Nora Ahmed", "Omar Khaled", "Sarah Mahmoud",
                "Hassan Mostafa", "Layla Said"
            };

            for (int i = 0; i < 10; i++)
            {
                // Check if user already exists
                var email = $"tailor{i + 1}@tafsilk.com";
                if (db.Users.Any(u => u.Email == email)) continue;

                var tailorUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    PasswordHash = PasswordHasher.Hash("Password@123"),
                    RoleId = tailorRole.Id,
                    IsActive = true,
                    EmailVerified = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-90 + (i * 5)),
                    LastLoginAt = DateTime.UtcNow.AddDays(-i)
                };
                users.Add(tailorUser);

                // Create customer profile for tailors too (they can also be customers)
                var tailorCustomerProfile = new CustomerProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = tailorUser.Id,
                    FullName = tailorNames[i],
                    Gender = i % 2 == 0 ? "Male" : "Female",
                    City = cities[i],
                    Bio = $"Professional tailor and customer on Tafsilk platform",
                    DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-30 - i)),
                    CreatedAt = tailorUser.CreatedAt
                };
                customerProfiles.Add(tailorCustomerProfile);
            }

            // Create 19 Customer Users (excluding admin)
            var customerFirstNames = new[]
            {
                "Youssef", "Mariam", "Karim", "Salma", "Moustafa", "Hana", "Tarek", "Nour",
                "Hussein", "Rania", "Sherif", "Dina", "Khaled", "Heba", "Amr", "Yasmine",
                "Hazem", "Farida", "Ziad"
            };

            for (int i = 0; i < 19; i++)
            {
                // Check if user already exists
                var email = $"customer{i + 1}@tafsilk.com";
                if (db.Users.Any(u => u.Email == email)) continue;

                var customerUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    PasswordHash = PasswordHasher.Hash("Password@123"),
                    RoleId = customerRole.Id,
                    IsActive = true,
                    EmailVerified = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-60 + (i * 3)),
                    LastLoginAt = DateTime.UtcNow.AddDays(-i / 2)
                };
                users.Add(customerUser);

                var customerProfile = new CustomerProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = customerUser.Id,
                    FullName = customerFirstNames[i] + " " + (i % 2 == 0 ? "Ahmed" : "Mohamed"),
                    Gender = genders[i % 2],
                    City = cities[i % cities.Length],
                    Bio = i % 3 == 0 ? $"Fashion enthusiast from {cities[i % cities.Length]}" : null,
                    DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-25 - (i % 30))),
                    CreatedAt = customerUser.CreatedAt
                };
                customerProfiles.Add(customerProfile);
            }

            // Add all users and profiles
            if (users.Any())
            {
                db.Users.AddRange(users);
                db.SaveChanges();
                logger.LogInformation($"✅ Created {users.Count} users");
            }

            if (customerProfiles.Any())
            {
                db.CustomerProfiles.AddRange(customerProfiles);
                db.SaveChanges();
                logger.LogInformation($"✅ Created {customerProfiles.Count} customer profiles");
            }

            logger.LogInformation("🌱 UserSeeder completed successfully");
        }
    }
}
