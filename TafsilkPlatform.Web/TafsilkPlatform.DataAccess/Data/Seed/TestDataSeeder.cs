using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.DataAccess.Data;
using TafsilkPlatform.Models.Models;
using TafsilkPlatform.Utility.Security;

namespace TafsilkPlatform.DataAccess.Data.Seed
{
    /// <summary>
    /// Seeds test data for customer journey workflow testing
    /// </summary>
    public static class TestDataSeeder
 {
        public static async Task SeedTestDataAsync(ApplicationDbContext context)
        {
            // Skip if test data already exists
      if (await context.Users.AnyAsync(u => u.Email.Contains("@tafsilk.test")))
            {
    Console.WriteLine("✅ Test data already exists. Skipping seeding.");
   return;
    }

            Console.WriteLine("========================================");
        Console.WriteLine("SEEDING TEST DATA");
            Console.WriteLine("========================================");

        // Get roles
     var customerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer");
            var tailorRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Tailor");

    if (customerRole == null || tailorRole == null)
 {
    Console.WriteLine("❌ Roles not found. Please run database migrations first.");
      return;
        }

            // Password: Test@123
            var passwordHash = PasswordHasher.Hash("Test@123");

       // Create test customers
         var customers = new List<User>
          {
        new User
         {
       Id = Guid.NewGuid(),
       Email = "ahmed.hassan@tafsilk.test",
          PhoneNumber = "+201012345671",
    PasswordHash = passwordHash,
     RoleId = customerRole.Id,
        IsActive = true,
   EmailVerified = true,
          CreatedAt = DateTime.UtcNow
     },
    new User
     {
            Id = Guid.NewGuid(),
    Email = "fatima.ali@tafsilk.test",
         PhoneNumber = "+201012345672",
     PasswordHash = passwordHash,
   RoleId = customerRole.Id,
          IsActive = true,
             EmailVerified = true,
  CreatedAt = DateTime.UtcNow
        },
      new User
         {
     Id = Guid.NewGuid(),
        Email = "mohamed.salem@tafsilk.test",
          PhoneNumber = "+201012345673",
        PasswordHash = passwordHash,
         RoleId = customerRole.Id,
     IsActive = true,
           EmailVerified = true,
              CreatedAt = DateTime.UtcNow
      }
       };

     context.Users.AddRange(customers);
            await context.SaveChangesAsync();
          Console.WriteLine($"✅ Created {customers.Count} test customers");

            // Create customer profiles
var customerProfiles = new List<CustomerProfile>();
         foreach (var customer in customers)
            {
    var profile = new CustomerProfile
            {
    Id = Guid.NewGuid(),
          UserId = customer.Id,
      FullName = customer.Email.Split('@')[0].Replace('.', ' '),
       City = "Cairo",
   CreatedAt = DateTime.UtcNow
        };
                customerProfiles.Add(profile);
       context.CustomerProfiles.Add(profile);
            }
       await context.SaveChangesAsync();
    Console.WriteLine($"✅ Created {customerProfiles.Count} customer profiles");

// Create customer loyalty records
            foreach (var profile in customerProfiles)
            {
         var loyalty = new CustomerLoyalty
         {
   Id = Guid.NewGuid(),
       CustomerId = profile.Id,
     Points = 100,
                LifetimePoints = 300,
        Tier = "Bronze",
                    TotalOrders = 1,
       ReferralsCount = 0,
           ReferralCode = $"REF{profile.Id.ToString().Substring(0, 8).ToUpper()}",
          CreatedAt = DateTime.UtcNow
    };
              context.CustomerLoyalties.Add(loyalty);
            }
       await context.SaveChangesAsync();
    Console.WriteLine($"✅ Created {customerProfiles.Count} loyalty records");

     // Create saved measurements
            var firstCustomer = customerProfiles[0];
     var measurement = new CustomerMeasurement
          {
            Id = Guid.NewGuid(),
    CustomerId = firstCustomer.Id,
 Name = "My Default Thobe",
          GarmentType = "Thobe",
Chest = 42,
      Waist = 34,
    Hips = 38,
      ThobeLength = 150,
         IsDefault = true,
    CreatedAt = DateTime.UtcNow
       };
         context.CustomerMeasurements.Add(measurement);
      await context.SaveChangesAsync();
      Console.WriteLine("✅ Created saved measurements");

        // Create test tailors
var tailors = new List<User>
       {
      new User
      {
           Id = Guid.NewGuid(),
        Email = "master.tailor@tafsilk.test",
      PhoneNumber = "+201112345671",
     PasswordHash = passwordHash,
            RoleId = tailorRole.Id,
       IsActive = true,
               EmailVerified = true,
             CreatedAt = DateTime.UtcNow
    },
new User
        {
    Id = Guid.NewGuid(),
         Email = "wedding.specialist@tafsilk.test",
        PhoneNumber = "+201112345672",
    PasswordHash = passwordHash,
      RoleId = tailorRole.Id,
             IsActive = true,
    EmailVerified = true,
           CreatedAt = DateTime.UtcNow
                }
   };

            context.Users.AddRange(tailors);
         await context.SaveChangesAsync();
         Console.WriteLine($"✅ Created {tailors.Count} test tailors");

   // Create tailor profiles
            var tailorProfiles = new List<TailorProfile>
            {
     new TailorProfile
        {
     Id = Guid.NewGuid(),
     UserId = tailors[0].Id,
    FullName = "Master Ibrahim",
   ShopName = "Al-Fakhama Tailoring",
           City = "Cairo",
      District = "Nasr City",
   Address = "15 Makram Ebeid St, Nasr City",
  Bio = "Premium custom tailoring with 20 years experience",
        Specialization = "Men's Suits & Thobes",
 ExperienceYears = 20,
 IsVerified = true,
         AverageRating = 4.8m,
           CreatedAt = DateTime.UtcNow,
         Latitude = 30.0444m,
         Longitude = 31.2357m
    },
            new TailorProfile
       {
         Id = Guid.NewGuid(),
         UserId = tailors[1].Id,
  FullName = "Madame Laila",
    ShopName = "Elegance Bridal Studio",
               City = "Alexandria",
             District = "Smouha",
          Address = "45 El-Horreya Road, Smouha",
  Bio = "Specialized in wedding dresses and evening gowns",
        Specialization = "Wedding & Evening Dresses",
            ExperienceYears = 15,
      IsVerified = true,
      AverageRating = 4.9m,
      CreatedAt = DateTime.UtcNow,
    Latitude = 31.2001m,
      Longitude = 29.9187m
      }
            };

            context.TailorProfiles.AddRange(tailorProfiles);
       await context.SaveChangesAsync();
    Console.WriteLine($"✅ Created {tailorProfiles.Count} tailor profiles");

            // Create tailor services
   var services = new List<TailorService>
       {
         new TailorService
         {
     TailorServiceId = Guid.NewGuid(),
         TailorId = tailorProfiles[0].Id,
         ServiceName = "Custom Business Suit",
      Description = "3-piece custom business suit with premium fabric",
   BasePrice = 2500.00m,
       EstimatedDuration = 14,
      IsDeleted = false
                },
     new TailorService
        {
            TailorServiceId = Guid.NewGuid(),
               TailorId = tailorProfiles[0].Id,
                    ServiceName = "Traditional Thobe",
        Description = "Classic Saudi/Emirati style thobe",
        BasePrice = 800.00m,
       EstimatedDuration = 7,
   IsDeleted = false
      },
    new TailorService
    {
               TailorServiceId = Guid.NewGuid(),
    TailorId = tailorProfiles[1].Id,
              ServiceName = "Wedding Dress",
         Description = "Custom wedding dress with full embellishments",
      BasePrice = 8000.00m,
           EstimatedDuration = 45,
              IsDeleted = false
     },
       new TailorService
      {
            TailorServiceId = Guid.NewGuid(),
       TailorId = tailorProfiles[1].Id,
   ServiceName = "Evening Gown",
         Description = "Elegant evening gown for special occasions",
     BasePrice = 3500.00m,
      EstimatedDuration = 14,
       IsDeleted = false
  }
       };

   context.TailorServices.AddRange(services);
       await context.SaveChangesAsync();
     Console.WriteLine($"✅ Created {services.Count} tailor services");

      // Create test orders (covering all statuses)
       var orders = new List<Order>
 {
   // Pending (awaiting quote)
     new Order
   {
             OrderId = Guid.NewGuid(),
      CustomerId = customerProfiles[0].Id,
   TailorId = tailorProfiles[0].Id,
         Description = "Custom business suit for job interview",
        OrderType = "Custom Business Suit",
      Status = OrderStatus.Pending,
   TotalPrice = 2500.00,
  CreatedAt = DateTimeOffset.UtcNow,
  DueDate = DateTimeOffset.UtcNow.AddDays(14),
 RequiresDeposit = true,
      FulfillmentMethod = "Pickup",
          MeasurementsJson = "{\"chest\":42,\"waist\":34}",
       Customer = customerProfiles[0],
          Tailor = tailorProfiles[0]
  },
 // Confirmed
   new Order
           {
   OrderId = Guid.NewGuid(),
CustomerId = customerProfiles[1].Id,
    TailorId = tailorProfiles[1].Id,
  Description = "Wedding dress with lace details",
     OrderType = "Wedding Dress",
     Status = OrderStatus.Confirmed,
    TotalPrice = 8500.00,
        TailorQuote = 8500.00,
  TailorQuoteNotes = "Includes premium French lace",
    QuoteProvidedAt = DateTimeOffset.UtcNow.AddHours(-2),
     RequiresDeposit = true,
     DepositAmount = 4250.00,
        DepositPaid = true,
          DepositPaidAt = DateTimeOffset.UtcNow.AddHours(-1),
   CreatedAt = DateTimeOffset.UtcNow.AddDays(-2),
   DueDate = DateTimeOffset.UtcNow.AddDays(28),
 FulfillmentMethod = "Delivery",
           Customer = customerProfiles[1],
    Tailor = tailorProfiles[1]
         },
  // Delivered (completed)
         new Order
                {
   OrderId = Guid.NewGuid(),
  CustomerId = customerProfiles[2].Id,
    TailorId = tailorProfiles[0].Id,
             Description = "Traditional white thobe",
OrderType = "Traditional Thobe",
          Status = OrderStatus.Delivered,
TotalPrice = 800.00,
  CreatedAt = DateTimeOffset.UtcNow.AddDays(-20),
  DueDate = DateTimeOffset.UtcNow.AddDays(-5),
FulfillmentMethod = "Pickup",
    Customer = customerProfiles[2],
     Tailor = tailorProfiles[0]
                }
    };

         context.Orders.AddRange(orders);
      await context.SaveChangesAsync();
         Console.WriteLine($"✅ Created {orders.Count} test orders");

      Console.WriteLine("");
      Console.WriteLine("========================================");
            Console.WriteLine("TEST DATA SEEDING COMPLETE!");
Console.WriteLine("========================================");
 Console.WriteLine("");
            Console.WriteLine("Test Credentials:");
            Console.WriteLine("-----------------");
    Console.WriteLine("Password for all accounts: Test@123");
    Console.WriteLine("");
    Console.WriteLine("Customers:");
   Console.WriteLine("  - ahmed.hassan@tafsilk.test");
     Console.WriteLine("  - fatima.ali@tafsilk.test");
   Console.WriteLine("  - mohamed.salem@tafsilk.test");
    Console.WriteLine("");
          Console.WriteLine("Tailors:");
    Console.WriteLine("  - master.tailor@tafsilk.test (Master Ibrahim)");
       Console.WriteLine("  - wedding.specialist@tafsilk.test (Madame Laila)");
    Console.WriteLine("");
        }
    }
}
