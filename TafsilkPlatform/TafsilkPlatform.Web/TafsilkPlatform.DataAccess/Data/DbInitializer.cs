using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TafsilkPlatform.Models.Models;

namespace TafsilkPlatform.DataAccess.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider, bool isDevelopment)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>(); // Using DbContext logger as a proxy or we could create a named one

            try
            {
                var db = services.GetRequiredService<ApplicationDbContext>();
                var providerName = db.Database.ProviderName ?? string.Empty;

                // Apply migrations and seed minimal data
                // If using SQLite in development, prefer EnsureCreated to avoid running SQL Server-specific migrations
                if (db.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true)
                {
                    try
                    {
                        await db.Database.EnsureCreatedAsync();
                        logger.LogInformation("✓ SQLite database ensured/created successfully");

                        if (!await db.Roles.AnyAsync())
                        {
                            db.Roles.AddRange(
                                new Role { Id = Guid.NewGuid(), Name = "Admin", Description = "Administrator", CreatedAt = DateTime.UtcNow },
                                new Role { Id = Guid.NewGuid(), Name = "Tailor", Description = "Tailor role", CreatedAt = DateTime.UtcNow },
                                new Role { Id = Guid.NewGuid(), Name = "Customer", Description = "Customer role", CreatedAt = DateTime.UtcNow }
                            );
                            await db.SaveChangesAsync();
                            logger.LogInformation("✓ Seeded default roles into SQLite database");

                            // ✅ Seed admin user
                            var adminConfig = services.GetRequiredService<IConfiguration>();
                            // We need a logger for the seeder, can use the same one or a generic one
                            var adminLogger = services.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeeder"); 
                            // Note: Program might not be accessible here if it's in Web project. 
                            // Better to use ILogger<object> or just pass the current logger if compatible.
                            // However, AdminSeeder.Seed expects ILogger.
                            
                            TafsilkPlatform.DataAccess.Data.Seed.AdminSeeder.Seed(db, adminConfig, adminLogger);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to ensure/create SQLite database during startup");
                        if (!isDevelopment) throw;
                    }
                }
                else
                {
                    // For SQL Server and other providers, attempt to apply migrations
                    try
                    {
                        await db.Database.MigrateAsync();
                        logger.LogInformation("✓ Database migrations applied successfully");

                        if (!await db.Roles.AnyAsync())
                        {
                            db.Roles.AddRange(
                                new Role { Id = Guid.NewGuid(), Name = "Admin", Description = "Administrator", CreatedAt = DateTime.UtcNow },
                                new Role { Id = Guid.NewGuid(), Name = "Tailor", Description = "Tailor role", CreatedAt = DateTime.UtcNow },
                                new Role { Id = Guid.NewGuid(), Name = "Customer", Description = "Customer role", CreatedAt = DateTime.UtcNow }
                            );
                            await db.SaveChangesAsync();
                            logger.LogInformation("✓ Seeded default roles into database");
                        }

                        // ✅ Seed admin user (Run every time to ensure admin exists/updates)
                        var adminConfig = services.GetRequiredService<IConfiguration>();
                        // Using a generic logger since Program is in Web
                        var adminLogger = services.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeeder");
                        TafsilkPlatform.DataAccess.Data.Seed.AdminSeeder.Seed(db, adminConfig, adminLogger);
                    }
                    catch (Exception ex)
                    {
                        logger.LogCritical(ex, "❌ Applying migrations failed. Please ensure the database is available and migrations are compatible.");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                if (isDevelopment)
                {
                    logger.LogWarning(ex, "Database initialization failed during startup but continuing because environment is Development");
                }
                else
                {
                    logger.LogCritical(ex, "❌ Cannot initialize database. Application will stop.");
                    throw;
                }
            }
        }
    }
}
