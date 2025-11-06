using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Data.Seed;

namespace TafsilkPlatform.Web.Controllers
{
    /// <summary>
 /// Development-only controller for seeding test data
    /// </summary>
  [ApiController]
    [Route("api/[controller]")]
    public class DevDataController : ControllerBase
  {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<DevDataController> _logger;

        public DevDataController(
        AppDbContext context,
            IWebHostEnvironment env,
            ILogger<DevDataController> logger)
        {
_context = context;
  _env = env;
 _logger = logger;
        }

      /// <summary>
     /// Seeds test data for customer journey workflow testing
        /// Only available in Development environment
        /// </summary>
        /// <returns></returns>
        [HttpPost("seed-test-data")]
      [AllowAnonymous]
 public async Task<IActionResult> SeedTestData()
        {
          // Only allow in Development environment
        if (!_env.IsDevelopment())
            {
       return StatusCode(403, new { error = "This endpoint is only available in Development environment" });
   }

  try
       {
      _logger.LogInformation("Starting test data seeding...");
       await TestDataSeeder.SeedTestDataAsync(_context);
          _logger.LogInformation("Test data seeding completed successfully");

       return Ok(new
   {
        success = true,
  message = "Test data seeded successfully!",
                testCredentials = new
  {
       password = "Test@123",
        customers = new[]
             {
        "ahmed.hassan@tafsilk.test",
      "fatima.ali@tafsilk.test",
        "mohamed.salem@tafsilk.test"
         },
 tailors = new[]
    {
          "master.tailor@tafsilk.test",
   "wedding.specialist@tafsilk.test"
    }
           }
        });
      }
       catch (Exception ex)
            {
        _logger.LogError(ex, "Error seeding test data");
   return StatusCode(500, new { error = "Failed to seed test data", details = ex.Message });
  }
        }

  /// <summary>
        /// Clears all test data
        /// Only available in Development environment
     /// </summary>
        [HttpDelete("clear-test-data")]
    [AllowAnonymous]
 public async Task<IActionResult> ClearTestData()
        {
            // Only allow in Development environment
            if (!_env.IsDevelopment())
  {
                return StatusCode(403, new { error = "This endpoint is only available in Development environment" });
        }

    try
       {
         _logger.LogInformation("Clearing test data...");

     // Delete in correct order to avoid foreign key constraints
           var testUsers = _context.Users.Where(u => u.Email.Contains("@tafsilk.test"));
  
 foreach (var user in testUsers)
    {
          // Delete related data first
 var customerProfile = _context.CustomerProfiles.FirstOrDefault(c => c.UserId == user.Id);
         if (customerProfile != null)
          {
            var loyalty = _context.CustomerLoyalties.FirstOrDefault(l => l.CustomerId == customerProfile.Id);
      if (loyalty != null)
      {
     var transactions = _context.LoyaltyTransactions.Where(t => t.CustomerLoyaltyId == loyalty.Id);
     _context.LoyaltyTransactions.RemoveRange(transactions);
          }
      if (loyalty != null) _context.CustomerLoyalties.Remove(loyalty);

            var measurements = _context.CustomerMeasurements.Where(m => m.CustomerId == customerProfile.Id);
   _context.CustomerMeasurements.RemoveRange(measurements);

   var orders = _context.Orders.Where(o => o.CustomerId == customerProfile.Id);
           foreach (var order in orders)
  {
  var payments = _context.Payment.Where(p => p.OrderId == order.OrderId);
     _context.Payment.RemoveRange(payments);
               }
       _context.Orders.RemoveRange(orders);

_context.CustomerProfiles.Remove(customerProfile);
             }

    var tailorProfile = _context.TailorProfiles.FirstOrDefault(t => t.UserId == user.Id);
         if (tailorProfile != null)
 {
   var services = _context.TailorServices.Where(s => s.TailorId == tailorProfile.Id);
      _context.TailorServices.RemoveRange(services);

      var portfolio = _context.PortfolioImages.Where(p => p.TailorId == tailorProfile.Id);
        _context.PortfolioImages.RemoveRange(portfolio);

      var orders = _context.Orders.Where(o => o.TailorId == tailorProfile.Id);
         _context.Orders.RemoveRange(orders);

  _context.TailorProfiles.Remove(tailorProfile);
      }

            _context.Users.Remove(user);
   }

          await _context.SaveChangesAsync();
     _logger.LogInformation("Test data cleared successfully");

          return Ok(new { success = true, message = "Test data cleared successfully" });
 }
            catch (Exception ex)
            {
   _logger.LogError(ex, "Error clearing test data");
    return StatusCode(500, new { error = "Failed to clear test data", details = ex.Message });
         }
        }
    }
}
