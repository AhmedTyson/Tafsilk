using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Drawing.Imaging;
using TafsilkPlatform.Models.Models;

namespace TafsilkPlatform.DataAccess.Data.Seed
{
    public static class PortfolioSeeder
    {
        public static void Seed(ApplicationDbContext db, ILogger logger)
        {
            // Check if we already have portfolio items
            if (db.PortfolioImages.Any())
            {
                logger.LogInformation("Portfolio images already seeded, skipping PortfolioSeeder");
                return;
            }

            logger.LogInformation("🌱 Starting PortfolioSeeder...");

            // Get verified tailors
            var tailors = db.TailorProfiles
                .Where(t => t.IsVerified)
                .OrderBy(t => t.CreatedAt)
                .Take(10)
                .ToList();

            if (!tailors.Any())
            {
                logger.LogError("No verified tailors found. Run TailorSeeder first.");
                return;
            }

            var portfolioImages = new List<PortfolioImage>();
            var random = new Random();

            // Categories for portfolio items
            var categories = new[]
            {
                "Wedding Dress", "Business Suit", "Evening Gown", "Traditional Wear",
                "Casual Wear", "Formal Wear", "Children's Wear", "Abaya",
                "Galabeya", "Alterations", "Custom Design"
            };

            // Sample titles and descriptions for each category
            var itemData = new Dictionary<string, (string[] Titles, string[] Descriptions)>
            {
                ["Wedding Dress"] = (
                    new[] { "Elegant Bridal Gown", "Vintage Wedding Dress", "Modern Mermaid Dress", "Classic A-Line Gown" },
                    new[] { "Stunning white gown with intricate lace details", "Timeless vintage-inspired wedding dress", "Sleek mermaid silhouette with beaded bodice", "Traditional A-line dress with cathedral train" }
                ),
                ["Business Suit"] = (
                    new[] { "Executive Three-Piece Suit", "Modern Slim Fit Suit", "Classic Pinstripe Suit", "Navy Blue Business Suit" },
                    new[] { "Premium wool suit for corporate executives", "Contemporary slim-fit design in charcoal gray", "Traditional pinstripe with classic cut", "Versatile navy suit for all occasions" }
                ),
                ["Evening Gown"] = (
                    new[] { "Sequined Evening Dress", "Silk Evening Gown", "Off-Shoulder Gown", "Velvet Ball Gown" },
                    new[] { "Sparkly sequined dress perfect for galas", "Luxurious silk gown in emerald green", "Elegant off-shoulder design with train", "Rich velvet ball gown for special events" }
                ),
                ["Traditional Wear"] = (
                    new[] { "Embroidered Thobe", "Traditional Galaby", "Cultural Dress", "Heritage Attire" },
                    new[] { "Hand-embroidered thobe with golden threads", "Authentic Egyptian galabeya with intricate patterns", "Traditional dress preserving cultural heritage", "Heritage-inspired formal attire" }
                ),
                ["Abaya"] = (
                    new[] { "Modern Black Abaya", "Embellished Abaya", "Casual Abaya", "Formal Abaya" },
                    new[] { "Contemporary black abaya with subtle details", "Elegant abaya with delicate embellishments", "Comfortable everyday abaya design", "Formal abaya for special occasions" }
                ),
                ["Galabeya"] = (
                    new[] { "Premium White Galabeya", "Formal Galabeya", "Summer Galabeya", "Ceremonial Galabeya" },
                    new[] { "High-quality white galabeya for formal events", "Traditional formal design with modern touches", "Lightweight summer galabeya in linen", "Special ceremonial galabeya with embroidery" }
                ),
                ["Alterations"] = (
                    new[] { "Dress Hemming", "Suit Tailoring", "Resizing Service", "Custom Fitting" },
                    new[] { "Professional dress hemming and adjustments", "Expert suit alterations for perfect fit", "Resizing service for all garment types", "Custom fitting and alterations" }
                ),
                ["Custom Design"] = (
                    new[] { "Bespoke Creation", "Designer Original", "Made-to-Measure", "Exclusive Design" },
                    new[] { "One-of-a-kind bespoke garment", "Original designer creation", "Made-to-measure luxury piece", "Exclusive custom-designed attire" }
                )
            };

            int imageCount = 0;
            foreach (var tailor in tailors)
            {
                // Create 3-5 portfolio items per tailor
                int itemsPerTailor = random.Next(3, 6);

                for (int i = 0; i < itemsPerTailor; i++)
                {
                    // Select a random category
                    var category = categories[random.Next(categories.Length)];

                    // Get sample data for this category, with fallback
                    var (titles, descriptions) = itemData.ContainsKey(category)
                        ? itemData[category]
                        : (new[] { $"{category} Design", $"Custom {category}", $"Premium {category}" },
                           new[] { $"Beautiful {category.ToLower()} design", $"High-quality {category.ToLower()}", $"Premium {category.ToLower()} creation" });

                    var titleIndex = random.Next(titles.Length);
                    var descriptionIndex = random.Next(descriptions.Length);

                    var portfolioImage = new PortfolioImage
                    {
                        PortfolioImageId = Guid.NewGuid(),
                        TailorId = tailor.Id,
                        Title = titles[titleIndex],
                        Category = category,
                        Description = descriptions[descriptionIndex],
                        DisplayOrder = i + 1,
                        IsFeatured = i == 0, // Make first item featured
                        EstimatedPrice = GeneratePrice(category, random),
                        UploadedAt = DateTime.UtcNow.AddDays(-random.Next(1, 90)),
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 90)),
                        IsDeleted = false
                    };

                    // Generate placeholder image
                    try
                    {
                        portfolioImage.ImageData = GeneratePortfolioImage(portfolioImage.Title, category, imageCount);
                        portfolioImage.ContentType = "image/png";
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Failed to generate portfolio image for {portfolioImage.Title}");
                        // Continue without image data - it's optional
                    }

                    portfolioImages.Add(portfolioImage);
                    imageCount++;
                }
            }

            db.PortfolioImages.AddRange(portfolioImages);
            db.SaveChanges();

            logger.LogInformation($"✅ Created {portfolioImages.Count} portfolio images for {tailors.Count} tailors");
            logger.LogInformation("🌱 PortfolioSeeder completed successfully");
        }

        private static decimal GeneratePrice(string category, Random random)
        {
            // Generate realistic prices based on category
            return category switch
            {
                "Wedding Dress" => random.Next(5000, 15000),
                "Evening Gown" => random.Next(2000, 8000),
                "Business Suit" => random.Next(1500, 5000),
                "Traditional Wear" => random.Next(800, 3000),
                "Abaya" => random.Next(500, 2500),
                "Galabeya" => random.Next(400, 1500),
                "Alterations" => random.Next(100, 500),
                "Custom Design" => random.Next(3000, 10000),
                "Formal Wear" => random.Next(1000, 4000),
                "Children's Wear" => random.Next(300, 1200),
                "Casual Wear" => random.Next(400, 1500),
                _ => random.Next(500, 2000)
            };
        }

        private static byte[] GeneratePortfolioImage(string title, string category, int index)
        {
            // Create a placeholder image with category and title
            using var bitmap = new Bitmap(400, 300);
            using var graphics = Graphics.FromImage(bitmap);

            // Color palette based on category
            var backgroundColor = GetCategoryColor(category, index);
            graphics.Clear(backgroundColor);

            // Add border
            using var borderPen = new Pen(Color.FromArgb(200, 200, 200), 2);
            graphics.DrawRectangle(borderPen, 1, 1, 398, 298);

            // Draw category label at top
            using var categoryFont = new Font("Arial", 14, FontStyle.Bold);
            using var categoryBrush = new SolidBrush(Color.White);
            var categoryFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            graphics.DrawString(category, categoryFont, categoryBrush, new RectangleF(0, 20, 400, 40), categoryFormat);

            // Draw title in center
            using var titleFont = new Font("Arial", 12, FontStyle.Regular);
            using var titleBrush = new SolidBrush(Color.White);
            var titleFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            // Wrap text if too long
            var wrappedTitle = WrapText(title, 35);
            graphics.DrawString(wrappedTitle, titleFont, titleBrush, new RectangleF(20, 100, 360, 100), titleFormat);

            // Add decorative element
            using var decorPen = new Pen(Color.FromArgb(100, 255, 255, 255), 1);
            graphics.DrawLine(decorPen, 100, 80, 300, 80);
            graphics.DrawLine(decorPen, 100, 220, 300, 220);

            // Save to byte array
            using var stream = new System.IO.MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            return stream.ToArray();
        }

        private static Color GetCategoryColor(string category, int index)
        {
            // Colors based on category
            return category switch
            {
                "Wedding Dress" => Color.FromArgb(255, 192, 203), // Pink
                "Evening Gown" => Color.FromArgb(75, 0, 130), // Dark purple
                "Business Suit" => Color.FromArgb(25, 25, 112), // Midnight blue
                "Traditional Wear" => Color.FromArgb(184, 134, 11), // Dark goldenrod
                "Abaya" => Color.FromArgb(0, 0, 0), // Black
                "Galabeya" => Color.FromArgb(255, 255, 255).Mix(Color.FromArgb(210, 180, 140), 0.3f), // Tan
                "Alterations" => Color.FromArgb(128, 128, 128), // Gray
                "Custom Design" => Color.FromArgb(218, 165, 32), // Goldenrod
                "Formal Wear" => Color.FromArgb(0, 0, 128), // Navy
                "Children's Wear" => Color.FromArgb(255, 182, 193), // Light pink
                "Casual Wear" => Color.FromArgb(70, 130, 180), // Steel blue
                _ => Color.FromArgb((index * 37) % 256, (index * 79) % 256, (index * 113) % 256)
            };
        }

        private static string WrapText(string text, int maxLength)
        {
            if (text.Length <= maxLength) return text;

            var words = text.Split(' ');
            var lines = new List<string>();
            var currentLine = "";

            foreach (var word in words)
            {
                if ((currentLine + " " + word).Trim().Length <= maxLength)
                {
                    currentLine = (currentLine + " " + word).Trim();
                }
                else
                {
                    if (!string.IsNullOrEmpty(currentLine))
                        lines.Add(currentLine);
                    currentLine = word;
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
                lines.Add(currentLine);

            return string.Join(Environment.NewLine, lines);
        }
    }

    // Extension method for color mixing
    public static class ColorExtensions
    {
        public static Color Mix(this Color color1, Color color2, float ratio)
        {
            ratio = Math.Clamp(ratio, 0f, 1f);
            return Color.FromArgb(
                (int)(color1.R * (1 - ratio) + color2.R * ratio),
                (int)(color1.G * (1 - ratio) + color2.G * ratio),
                (int)(color1.B * (1 - ratio) + color2.B * ratio)
            );
        }
    }
}
