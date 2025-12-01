using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Drawing.Imaging;
using TafsilkPlatform.Models.Models;

namespace TafsilkPlatform.DataAccess.Data.Seed
{
    public static class TailorSeeder
    {
        public static void Seed(ApplicationDbContext db, ILogger logger)
        {
            // Check if we already have enough tailor profiles (keep admin, add 9 more for total of 10)
            if (db.TailorProfiles.Count() >= 10)
            {
                logger.LogInformation("Tailor profiles already seeded (10+), skipping TailorSeeder");
                return;
            }

            logger.LogInformation("🌱 Starting TailorSeeder (Egypt Localization)...");

            // Get tailor role users
            var tailorRole = db.Roles.FirstOrDefault(r => r.Name == "Tailor");
            if (tailorRole == null)
            {
                logger.LogError("Tailor role not found. Cannot seed tailor profiles.");
                return;
            }

            var tailorUsers = db.Users.Where(u => u.RoleId == tailorRole.Id && u.Email.Contains("tailor")).ToList();
            if (!tailorUsers.Any())
            {
                logger.LogError("No tailor users found. Run UserSeeder first.");
                return;
            }

            var tailorProfiles = new List<TailorProfile>();

            // Tailor shop data (Egyptian)
            var shopData = new[]
            {
                new { ShopName = "Al-Ahram Tailors", City = "Cairo", Specialization = "Traditional Egyptian Galabeya", District = "Zamalek", Experience = 15, Bio = "Master tailor specializing in premium traditional Egyptian galabeyas with over 15 years of experience." },
                new { ShopName = "Nile Fashion House", City = "Alexandria", Specialization = "Women's Abayas & Dresses", District = "Sidi Gaber", Experience = 12, Bio = "Elegant and modern abaya designs with traditional touches. Custom fitting available." },
                new { ShopName = "Sphinx Suits", City = "Giza", Specialization = "Business Suits & Formal Wear", District = "Dokki", Experience = 10, Bio = "Professional tailoring for modern business attire. European and traditional styles." },
                new { ShopName = "Luxor Linens", City = "Luxor", Specialization = "Wedding Dresses & Formal Gowns", District = "East Bank", Experience = 8, Bio = "Creating dream wedding dresses and formal gowns for special occasions." },
                new { ShopName = "Aswan Authentic", City = "Aswan", Specialization = "Men's Traditional & Casual Wear", District = "Corniche", Experience = 20, Bio = "Nubian heritage meets modern tailoring. Authentic craftsmanship." },
                new { ShopName = "Red Sea Couture", City = "Hurghada", Specialization = "Custom Evening Wear", District = "El Mamsha", Experience = 7, Bio = "Haute couture evening wear and party dresses. Each piece is a work of art." },
                new { ShopName = "Sharm Style", City = "Sharm El Sheikh", Specialization = "Luxury Fashion", District = "Naama Bay", Experience = 9, Bio = "Premium tailoring services for discerning clients. Only the finest fabrics." },
                new { ShopName = "Canal Tailors", City = "Port Said", Specialization = "Contemporary Fashion", District = "El Sharq", Experience = 6, Bio = "Fresh, contemporary designs blending tradition with modern fashion trends." },
                new { ShopName = "Suez Stitch", City = "Suez", Specialization = "Traditional Wear", District = "Arbaeen", Experience = 18, Bio = "Specializing in traditional attire with authentic craftsmanship." },
                new { ShopName = "Delta Designs", City = "Mansoura", Specialization = "Children's Designer Wear", District = "El Mashaya", Experience = 5, Bio = "Adorable and comfortable designer clothing for children. Every child deserves to look their best." }
            };

            var addresses = new[]
            {
                "26th of July Street, Building 10", "Corniche Road, Suite 5", "Tahrir Street, Floor 2",
                "Karnak Temple Road, Shop 3", "Abtal El Tahrir Street, Unit 8", "Sheraton Road, Building 12",
                "Peace Road, Complex A", "El Gomhouria Street", "El Geish Street, Plaza 5",
                "El Gomhouria Street, Shop 15"
            };

            var businessHours = new[]
            {
                "Sat-Thu: 9 AM - 9 PM, Fri: 4 PM - 10 PM",
                "Sat-Thu: 10 AM - 10 PM, Fri: Closed",
                "Daily: 8 AM - 8 PM",
                "Sat-Thu: 9 AM - 8 PM, Fri: 3 PM - 9 PM",
                "Daily: 10 AM - 11 PM",
                "Sat-Wed: 9 AM - 7 PM, Thu-Fri: 2 PM - 10 PM",
                "Sat-Thu: 11 AM - 9 PM, Fri: 5 PM - 11 PM",
                "Daily: 9 AM - 9 PM",
                "Sat-Thu: 8 AM - 6 PM, Fri: Closed",
                "Sat-Thu: 10 AM - 8 PM, Fri: 4 PM - 10 PM"
            };

            var pricingRanges = new[] { "$$$ - Premium", "$$ - Moderate", "$$$ - High-End", "$$$$ - Luxury", "$$ - Affordable", "$$$ - Premium", "$$$$ - Luxury", "$$ - Moderate", "$$$ - Premium", "$$ - Moderate" };

            // Create tailor profiles
            for (int i = 0; i < tailorUsers.Count && i < shopData.Length; i++)
            {
                var user = tailorUsers[i];
                var shop = shopData[i];

                // Generate coordinates
                var coordinates = GetCityCoordinates(shop.City);

                var tailorProfile = new TailorProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    FullName = db.CustomerProfiles.FirstOrDefault(cp => cp.UserId == user.Id)?.FullName ?? $"Tailor {i + 1}",
                    ShopName = shop.ShopName,
                    ShopDescription = $"Welcome to {shop.ShopName} - your destination for {shop.Specialization.ToLower()}.",
                    Specialization = shop.Specialization,
                    Address = addresses[i],
                    City = shop.City,
                    District = shop.District,
                    Latitude = coordinates.Latitude,
                    Longitude = coordinates.Longitude,
                    ExperienceYears = shop.Experience,
                    PricingRange = pricingRanges[i],
                    Bio = shop.Bio,
                    BusinessHours = businessHours[i],
                    IsVerified = i < 8, // Verify first 8 tailors
                    VerifiedAt = i < 8 ? DateTime.UtcNow.AddDays(-30 + i) : null,
                    AverageRating = 4.0m + (i * 0.1m), // Ratings from 4.0 to 4.9
                    CreatedAt = user.CreatedAt,
                    FacebookUrl = $"https://facebook.com/{shop.ShopName.Replace(" ", "").Replace("'", "").ToLower()}",
                    InstagramUrl = $"https://instagram.com/{shop.ShopName.Replace(" ", "").Replace("'", "").ToLower()}",
                    TwitterUrl = i % 2 == 0 ? $"https://twitter.com/{shop.ShopName.Replace(" ", "").Replace("'", "").ToLower()}" : null,
                    WebsiteUrl = i % 3 == 0 ? $"https://www.{shop.ShopName.Replace(" ", "").Replace("'", "").ToLower()}.eg" : null,
                    ProfilePictureContentType = "image/png"
                };

                try
                {
                    tailorProfile.ProfilePictureData = GeneratePlaceholderImage(shop.ShopName, i);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Failed to generate placeholder image for {shop.ShopName}");
                    // Continue without image
                }

                tailorProfiles.Add(tailorProfile);
            }

            db.TailorProfiles.AddRange(tailorProfiles);
            db.SaveChanges();

            logger.LogInformation($"✅ Created {tailorProfiles.Count} tailor profiles");
            logger.LogInformation("🌱 TailorSeeder completed successfully");
        }

        private static (decimal Latitude, decimal Longitude) GetCityCoordinates(string city)
        {
            // Approximate coordinates for Egyptian cities
            return city switch
            {
                "Cairo" => (30.0444m, 31.2357m),
                "Alexandria" => (31.2001m, 29.9187m),
                "Giza" => (30.0131m, 31.2089m),
                "Luxor" => (25.6872m, 32.6396m),
                "Aswan" => (24.0889m, 32.8998m),
                "Hurghada" => (27.2579m, 33.8116m),
                "Sharm El Sheikh" => (27.9158m, 34.3299m),
                "Port Said" => (31.2653m, 32.3019m),
                "Suez" => (29.9668m, 32.5498m),
                "Mansoura" => (31.0409m, 31.3785m),
                _ => (30.0444m, 31.2357m)
            };
        }

        private static byte[] GeneratePlaceholderImage(string shopName, int index)
        {
            // Create a simple colored square with initials
            using var bitmap = new Bitmap(200, 200);
            using var graphics = Graphics.FromImage(bitmap);

            // Color palette for different tailors
            var colors = new[]
            {
                Color.FromArgb(99, 110, 250),   // Blue
                Color.FromArgb(239, 68, 68),    // Red
                Color.FromArgb(16, 185, 129),   // Green
                Color.FromArgb(245, 158, 11),   // Orange
                Color.FromArgb(139, 92, 246),   // Purple
                Color.FromArgb(236, 72, 153),   // Pink
                Color.FromArgb(14, 165, 233),   // Cyan
                Color.FromArgb(251, 146, 60),   // Amber
                Color.FromArgb(20, 184, 166),   // Teal
                Color.FromArgb(168, 85, 247)    // Violet
            };

            var backgroundColor = colors[index % colors.Length];
            graphics.Clear(backgroundColor);

            // Get initials from shop name
            var initials = GetInitials(shopName);

            // Draw initials
            using var font = new Font("Arial", 72, FontStyle.Bold);
            using var brush = new SolidBrush(Color.White);
            var stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            graphics.DrawString(initials, font, brush, new RectangleF(0, 0, 200, 200), stringFormat);

            // Save to byte array
            using var stream = new System.IO.MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            return stream.ToArray();
        }

        private static string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "T";

            var words = name.Split(new[] { ' ', '-', '\'' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0) return "T";
            if (words.Length == 1) return words[0].Substring(0, Math.Min(2, words[0].Length)).ToUpper();

            return (words[0][0].ToString() + words[1][0].ToString()).ToUpper();
        }
    }
}
