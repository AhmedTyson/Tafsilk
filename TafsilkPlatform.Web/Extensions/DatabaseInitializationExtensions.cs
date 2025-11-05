using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Data;

namespace TafsilkPlatform.Web.Extensions;

/// <summary>
/// Database initialization extension methods
/// </summary>
public static class DatabaseInitializationExtensions
{
    /// <summary>
    /// Initializes the database with schema and seed data
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Starting database initialization...");

            // Always use migrations - create database if it doesn't exist
            await db.Database.MigrateAsync();
            logger.LogInformation("✓ Database migrations applied successfully");

            // Seed initial data
            logger.LogInformation("Seeding initial data...");
            TafsilkPlatform.Web.Data.Seed.AdminSeeder.Seed(db, configuration, logger);
            logger.LogInformation("✓ Initial data seeded successfully");

            // TODO: Fix DatabaseSeeder property names to match actual models
            // Temporarily commented out until models are fixed
            // await TafsilkPlatform.Web.Data.Seed.DatabaseSeeder.SeedTestDataAsync(db, logger);

            // Apply performance indexes
            await ApplyPerformanceIndexesAsync(db, logger);

            logger.LogInformation("✓ Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Database initialization failed");
            throw;
        }
    }

    private static async Task ApplyPerformanceIndexesAsync(AppDbContext db, ILogger logger)
    {
        try
        {
            logger.LogInformation("Applying performance indexes...");

            var indexes = new List<string>
            {
                // Index 1: Email Verification Token
                @"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_EmailVerificationToken' AND object_id = OBJECT_ID('Users'))
      CREATE NONCLUSTERED INDEX [IX_Users_EmailVerificationToken] ON [Users]([EmailVerificationToken]) WHERE [EmailVerificationToken] IS NOT NULL;",

                // Index 2: Active Users Filter
                @"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_IsActive_IsDeleted' AND object_id = OBJECT_ID('Users'))
  CREATE NONCLUSTERED INDEX [IX_Users_IsActive_IsDeleted] ON [Users]([IsActive], [IsDeleted]) INCLUDE ([Email], [RoleId]);",

                // Index 3: Tailor Verification
                @"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TailorProfiles_UserId_IsVerified' AND object_id = OBJECT_ID('TailorProfiles'))
       CREATE NONCLUSTERED INDEX [IX_TailorProfiles_UserId_IsVerified] ON [TailorProfiles]([UserId], [IsVerified]);",

                // Index 4: Customer Orders
                @"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Orders_CustomerId_Status' AND object_id = OBJECT_ID('Orders'))
  CREATE NONCLUSTERED INDEX [IX_Orders_CustomerId_Status] ON [Orders]([CustomerId], [Status]) INCLUDE ([CreatedAt], [TotalPrice]);",

                // Index 5: Tailor Orders
                @"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Orders_TailorId_Status' AND object_id = OBJECT_ID('Orders'))
          CREATE NONCLUSTERED INDEX [IX_Orders_TailorId_Status] ON [Orders]([TailorId], [Status]) INCLUDE ([CreatedAt], [TotalPrice]);",

                // Index 6: Notifications
                @"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Notifications_UserId_IsRead' AND object_id = OBJECT_ID('Notifications'))
CREATE NONCLUSTERED INDEX [IX_Notifications_UserId_IsRead] ON [Notifications]([UserId], [IsRead]) WHERE [IsDeleted] = 0;",

                // Index 7: Reviews
                @"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Reviews_TailorId_CreatedAt' AND object_id = OBJECT_ID('Reviews'))
CREATE NONCLUSTERED INDEX [IX_Reviews_TailorId_CreatedAt] ON [Reviews]([TailorId], [CreatedAt] DESC) INCLUDE ([Rating]) WHERE [IsDeleted] = 0;",

                // Index 8: Refresh Tokens
                @"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RefreshTokens_UserId_ExpiresAt' AND object_id = OBJECT_ID('RefreshTokens'))
 CREATE NONCLUSTERED INDEX [IX_RefreshTokens_UserId_ExpiresAt] ON [RefreshTokens]([UserId], [ExpiresAt] DESC) WHERE [RevokedAt] IS NULL;"
            };

            int indexesCreated = 0;
            foreach (var indexSql in indexes)
            {
                try
                {
                    await db.Database.ExecuteSqlRawAsync(indexSql);
                    indexesCreated++;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to create index. It may already exist or table may not exist yet.");
                }
            }

            logger.LogInformation($"✓ Applied {indexesCreated} performance indexes");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to apply some performance indexes. This is not critical.");
        }
    }
}
