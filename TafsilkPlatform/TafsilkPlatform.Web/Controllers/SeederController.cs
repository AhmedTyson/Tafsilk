using Microsoft.AspNetCore.Mvc;
using TafsilkPlatform.DataAccess.Data;
using TafsilkPlatform.DataAccess.Data.Seed;

namespace TafsilkPlatform.Web.Controllers
{
    public class SeederController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<SeederController> _logger;
        private readonly IConfiguration _config;

        public SeederController(ApplicationDbContext db, ILogger<SeederController> logger, IConfiguration config)
        {
            _db = db;
            _logger = logger;
            _config = config;
        }

        public async Task<IActionResult> Seed()
        {
            try
            {
                _logger.LogInformation("🚀 Manually starting seeding process...");

                // Seed Admin
                var adminLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("AdminSeeder");
                AdminSeeder.Seed(_db, _config, adminLogger);
                _logger.LogInformation("✅ AdminSeeder completed");

                // Seed Users
                var userLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("UserSeeder");
                UserSeeder.Seed(_db, userLogger);
                _logger.LogInformation("✅ UserSeeder completed");

                // Seed Tailors
                var tailorLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("TailorSeeder");
                TailorSeeder.Seed(_db, tailorLogger);
                _logger.LogInformation("✅ TailorSeeder completed");

                // Seed Products
                await ProductSeeder.SeedProductsAsync(_db);
                _logger.LogInformation("✅ ProductSeeder completed");

                return Content("Seeding completed successfully! Check logs for details.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Seeding failed");
                return Content($"Seeding failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
