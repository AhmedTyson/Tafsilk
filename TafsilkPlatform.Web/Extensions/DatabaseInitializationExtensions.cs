using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

                // Index 4: Corporate Approval
                @"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CorporateAccounts_UserId_IsApproved' AND object_id = OBJECT_ID('CorporateAccounts'))
    CREATE NONCLUSTERED INDEX [IX_CorporateAccounts_UserId_IsApproved] ON [CorporateAccounts]([UserId], [IsApproved]);",

                // Index 5: Customer Orders
                @"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Orders_CustomerId_Status' AND object_id = OBJECT_ID('Orders'))
  CREATE NONCLUSTERED INDEX [IX_Orders_CustomerId_Status] ON [Orders]([CustomerId], [Status]) INCLUDE ([CreatedAt], [TotalPrice]);",

                // Index 6: Tailor Orders
                @"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Orders_TailorId_Status' AND object_id = OBJECT_ID('Orders'))
          CREATE NONCLUSTERED INDEX [IX_Orders_TailorId_Status] ON [Orders]([TailorId], [Status]) INCLUDE ([CreatedAt], [TotalPrice]);",

                // Index 7: Notifications
                @"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Notifications_UserId_IsRead' AND object_id = OBJECT_ID('Notifications'))
CREATE NONCLUSTERED INDEX [IX_Notifications_UserId_IsRead] ON [Notifications]([UserId], [IsRead]) WHERE [IsDeleted] = 0;",

                // Index 8: Reviews
                @"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Reviews_TailorId_CreatedAt' AND object_id = OBJECT_ID('Reviews'))
CREATE NONCLUSTERED INDEX [IX_Reviews_TailorId_CreatedAt] ON [Reviews]([TailorId], [CreatedAt] DESC) INCLUDE ([Rating]) WHERE [IsDeleted] = 0;",

                // Index 9: Refresh Tokens
                @"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RefreshTokens_UserId_ExpiresAt' AND object_id = OBJECT_ID('RefreshTokens'))
 CREATE NONCLUSTERED INDEX [IX_RefreshTokens_UserId_ExpiresAt] ON [RefreshTokens]([UserId], [ExpiresAt] DESC) WHERE [RevokedAt] IS NULL;",

                // Index 10: Activity Logs
                @"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ActivityLogs_UserId_CreatedAt' AND object_id = OBJECT_ID('ActivityLogs'))
    CREATE NONCLUSTERED INDEX [IX_ActivityLogs_UserId_CreatedAt] ON [ActivityLogs]([UserId], [CreatedAt] DESC);"
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
