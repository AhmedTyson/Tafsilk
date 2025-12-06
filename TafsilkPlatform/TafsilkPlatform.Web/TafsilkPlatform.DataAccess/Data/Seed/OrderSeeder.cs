using Microsoft.Extensions.Logging;
using TafsilkPlatform.Models.Models;

namespace TafsilkPlatform.DataAccess.Data.Seed
{
    public static class OrderSeeder
    {
        public static void Seed(ApplicationDbContext db, ILogger logger)
        {
            // 1. Ensure Test Customer
            var customerUser = db.Users.FirstOrDefault(u => u.Email == "customer@test.com");
            if (customerUser == null)
            {
                logger.LogWarning("Test customer user not found. Skipping order seeding.");
                return;
            }

            var customerProfile = db.CustomerProfiles.FirstOrDefault(c => c.UserId == customerUser.Id);
            if (customerProfile == null)
            {
                customerProfile = new CustomerProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = customerUser.Id,
                    FullName = "Test Customer",
                    Gender = "Male",
                    City = "Cairo",
                    CreatedAt = DateTime.UtcNow
                };
                db.CustomerProfiles.Add(customerProfile);
                db.SaveChanges();
            }

            // 2. Ensure Test Tailors
            var tailor1 = EnsureTailor(db, "tailor1@test.com", "Tailor One", "Silk Masters");
            var tailor2 = EnsureTailor(db, "tailor2@test.com", "Tailor Two", "Cotton Kings");

            // 3. Ensure Products
            var product1 = EnsureProduct(db, tailor1, "Silk Scarf", 500m);
            var product2 = EnsureProduct(db, tailor2, "Cotton Shirt", 350m);

            db.SaveChanges();

            // 4. Create Multi-Tailor Order
            if (!db.Orders.Any(o => o.Description == "Multi-Tailor Test Order"))
            {
                var order = new Order
                {
                    OrderId = Guid.NewGuid(),
                    OrderNumber = "ORD-TEST-001",
                    Description = "Multi-Tailor Test Order",
                    OrderType = "Store Purchase",
                    Status = OrderStatus.Processing,
                    CreatedAt = DateTimeOffset.UtcNow,
                    TotalPrice = 850, // 500 + 350
                    CustomerId = customerProfile.Id,
                    Customer = customerProfile,
                    TailorId = tailor1.Id, // Main tailor (system requirement)
                    Tailor = tailor1
                };

                // Add items
                var item1 = new OrderItem
                {
                    OrderItemId = Guid.NewGuid(),
                    Description = product1.Name,
                    Quantity = 1,
                    UnitPrice = product1.Price,
                    Total = product1.Price,
                    ProductId = product1.ProductId,
                    Product = product1,
                    SelectedSize = "M",
                    SelectedColor = "Red",
                    Order = order,
                    OrderId = order.OrderId
                };

                var item2 = new OrderItem
                {
                    OrderItemId = Guid.NewGuid(),
                    Description = product2.Name,
                    Quantity = 1,
                    UnitPrice = product2.Price,
                    Total = product2.Price,
                    ProductId = product2.ProductId,
                    Product = product2,
                    SelectedSize = "L",
                    SelectedColor = "Blue",
                    Order = order,
                    OrderId = order.OrderId
                };

                order.Items = new List<OrderItem> { item1, item2 };

                // Add fake payment
                var payment = new Payment
                {
                    PaymentId = Guid.NewGuid(),
                    Amount = 850m,
                    PaidAt = DateTime.UtcNow,
                    Provider = "Stripe",
                    PaymentStatus = Enums.PaymentStatus.Completed,
                    PaymentType = Enums.PaymentType.Card,
                    ProviderTransactionId = "tx_test_123",
                    Order = order,
                    OrderId = order.OrderId,
                    Customer = customerProfile,
                    CustomerId = customerProfile.Id,
                    Tailor = tailor1,
                    TailorId = tailor1.Id
                };

                order.Payments = new List<Payment> { payment };

                db.Orders.Add(order);
                db.SaveChanges();
                logger.LogInformation("✅ Seeded Multi-Tailor Order: ORD-TEST-001");
            }
        }

        private static TailorProfile EnsureTailor(ApplicationDbContext db, string email, string name, string shop)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                // Create dummy user if not exists (simplified)
                user = new User { Id = Guid.NewGuid(), Email = email, PasswordHash = "hash", RoleId = Guid.Empty, IsActive = true, CreatedAt = DateTime.UtcNow };
                db.Users.Add(user);
            }

            var profile = db.TailorProfiles.FirstOrDefault(t => t.UserId == user.Id);
            if (profile == null)
            {
                profile = new TailorProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    FullName = name,
                    ShopName = shop,
                    City = "Cairo",
                    Address = "123 St",
                    IsVerified = true,
                    CreatedAt = DateTime.UtcNow
                };
                db.TailorProfiles.Add(profile);
            }
            return profile;
        }

        private static Product EnsureProduct(ApplicationDbContext db, TailorProfile tailor, string name, decimal price)
        {
            var product = db.Products.FirstOrDefault(p => p.Name == name && p.TailorId == tailor.Id);
            if (product == null)
            {
                product = new Product
                {
                    ProductId = Guid.NewGuid(),
                    Name = name,
                    Description = $"High quality {name}",
                    Price = price,
                    TailorId = tailor.Id,
                    Tailor = tailor,
                    Category = "Men",
                    CreatedAt = DateTime.UtcNow,
                    IsAvailable = true
                };
                db.Products.Add(product);
            }
            return product;
        }
    }
}
