using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Drawing.Imaging;
using TafsilkPlatform.Models.Models;

namespace TafsilkPlatform.DataAccess.Data.Seed
{
    public static class ProductSeeder
    {
        public static async Task SeedProductsAsync(ApplicationDbContext context)
        {
            // Check if products already exist
            var existingProducts = await context.Products.ToListAsync();
            if (existingProducts.Any())
            {
                // Clear existing products - need to handle foreign key constraints
                // First remove cart items and order items that reference products
                var productIds = existingProducts.Select(p => p.ProductId).ToList();
                var cartItems = await context.CartItems.Where(ci => productIds.Contains(ci.ProductId)).ToListAsync();
                var orderItems = await context.OrderItems.Where(oi => oi.ProductId.HasValue && productIds.Contains(oi.ProductId.Value)).ToListAsync();

                context.CartItems.RemoveRange(cartItems);
                context.OrderItems.RemoveRange(orderItems);
                context.Products.RemoveRange(existingProducts);
                await context.SaveChangesAsync();

                Console.WriteLine($"✅ Cleared {existingProducts.Count} existing products, {cartItems.Count} cart items, {orderItems.Count} order items");
            }

            var tailors = await context.TailorProfiles.ToListAsync();
            if (!tailors.Any())
            {
                Console.WriteLine("⚠️ No tailors found. Please seed tailors first.");
                return;
            }

            var allProducts = new List<Product>();
            var random = new Random();

            foreach (var tailor in tailors)
            {
                // Generate 3-8 products per tailor
                int productCount = random.Next(3, 9);

                for (int i = 0; i < productCount; i++)
                {
                    var template = GetRandomProductTemplate(random);

                    // Customize template for this tailor
                    var product = new Product
                    {
                        Name = template.Name, // Could append tailor name if desired
                        Description = template.Description,
                        Price = template.Price * (decimal)(0.8 + (random.NextDouble() * 0.4)), // Vary price +/- 20%
                        Category = template.Category,
                        SubCategory = template.SubCategory,
                        Size = template.Size,
                        Color = template.Color,
                        Material = template.Material,
                        Brand = tailor.ShopName ?? "Tafsilk Collection",
                        StockQuantity = random.Next(5, 50),
                        IsAvailable = true,
                        IsFeatured = random.NextDouble() > 0.8, // 20% chance of being featured
                        Slug = $"{template.Slug}-{Guid.NewGuid().ToString().Substring(0, 8)}", // Ensure unique slug
                        MetaTitle = template.MetaTitle,
                        MetaDescription = template.MetaDescription,
                        TailorId = tailor.Id,
                        AverageRating = Math.Round(3.5 + (random.NextDouble() * 1.5), 1),
                        ReviewCount = random.Next(5, 50),
                        SalesCount = random.Next(10, 200),
                        ViewCount = random.Next(100, 1000),
                        PrimaryImageData = GenerateProductImage(template.Name, template.Color)
                    };

                    if (random.NextDouble() > 0.7)
                    {
                        product.DiscountedPrice = product.Price * 0.85m; // 15% discount
                    }

                    allProducts.Add(product);
                }
            }

            await context.Products.AddRangeAsync(allProducts);
            await context.SaveChangesAsync();

            Console.WriteLine($"✅ Seeded {allProducts.Count} products for {tailors.Count} tailors successfully!");
        }

        private static byte[] GenerateProductImage(string text, string? colorName)
        {
            try
            {
                int width = 400;
                int height = 500; // Portrait aspect ratio for products

                using (Bitmap bitmap = new Bitmap(width, height))
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    // Background color based on product color name (simplified)
                    Color bgColor = GetColorFromString(colorName);

                    // Fill background
                    using (Brush brush = new SolidBrush(bgColor))
                    {
                        graphics.FillRectangle(brush, 0, 0, width, height);
                    }

                    // Add some texture/pattern
                    using (Pen pen = new Pen(Color.FromArgb(30, Color.White), 2))
                    {
                        graphics.DrawLine(pen, 0, 0, width, height);
                        graphics.DrawLine(pen, width, 0, 0, height);
                    }

                    // Draw text
                    string displayText = text.Length > 20 ? text.Substring(0, 20) + "..." : text;
                    using (Font font = new Font(FontFamily.GenericSansSerif, 24, FontStyle.Bold))
                    using (Brush textBrush = new SolidBrush(Color.White))
                    {
                        SizeF textSize = graphics.MeasureString(displayText, font);
                        graphics.DrawString(displayText, font, textBrush, (width - textSize.Width) / 2, (height - textSize.Height) / 2);
                    }

                    using (MemoryStream ms = new MemoryStream())
                    {
                        bitmap.Save(ms, ImageFormat.Png);
                        return ms.ToArray();
                    }
                }
            }
            catch
            {
                return Array.Empty<byte>();
            }
        }

        private static Color GetColorFromString(string? colorName)
        {
            if (string.IsNullOrEmpty(colorName)) return Color.Gray;

            string lower = colorName.ToLower();
            if (lower.Contains("white")) return Color.LightGray;
            if (lower.Contains("black")) return Color.FromArgb(40, 40, 40);
            if (lower.Contains("blue")) return Color.SteelBlue;
            if (lower.Contains("red")) return Color.IndianRed;
            if (lower.Contains("green")) return Color.SeaGreen;
            if (lower.Contains("brown")) return Color.SaddleBrown;
            if (lower.Contains("cream")) return Color.Beige;
            if (lower.Contains("grey") || lower.Contains("gray")) return Color.DimGray;
            if (lower.Contains("burgundy")) return Color.Maroon;

            return Color.SlateGray;
        }

        private static ProductTemplate GetRandomProductTemplate(Random random)
        {
            var templates = new List<ProductTemplate>
            {
                // Traditional Egyptian Wear
                new() { Name = "Men's Galabeya", Description = "Traditional Egyptian galabeya made from breathable cotton.", Price = 180.00m, Category = "Traditional", SubCategory = "Men's", Size = "L", Color = "White", Material = "Cotton", Slug = "mens-galabeya" },
                new() { Name = "Embroidered Galabeya", Description = "Elegant embroidered galabeya perfect for special occasions.", Price = 320.00m, Category = "Traditional", SubCategory = "Men's", Size = "L", Color = "Beige", Material = "Linen Blend", Slug = "embroidered-galabeya" },
                
                // Suits
                new() { Name = "Business Suit", Description = "Professional 2-piece suit perfect for office wear.", Price = 1200.00m, Category = "Suit", SubCategory = "Men's", Size = "M", Color = "Charcoal", Material = "Wool Blend", Slug = "business-suit" },
                new() { Name = "Wedding Suit", Description = "Luxurious 3-piece suit for weddings and formal events.", Price = 1800.00m, Category = "Suit", SubCategory = "Men's", Size = "L", Color = "Navy", Material = "Premium Wool", Slug = "wedding-suit" },
                new() { Name = "Casual Blazer", Description = "Smart-casual blazer for everyday elegance.", Price = 650.00m, Category = "Casual", SubCategory = "Men's", Size = "M", Color = "Gray", Material = "Cotton Blend", Slug = "casual-blazer" },
                
                // Dresses
                new() { Name = "Evening Dress", Description = "Elegant evening dress for special occasions.", Price = 850.00m, Category = "Dress", SubCategory = "Women's", Size = "M", Color = "Burgundy", Material = "Silk Blend", Slug = "evening-dress" },
                new() { Name = "Cocktail Dress", Description = "Stylish cocktail dress perfect for parties.", Price = 620.00m, Category = "Dress", SubCategory = "Women's", Size = "S", Color = "Black", Material = "Chiffon", Slug = "cocktail-dress" },
                new() { Name = "Casual Summer Dress", Description = "Light and comfortable summer dress.", Price = 380.00m, Category = "Dress", SubCategory = "Women's", Size = "M", Color = "Floral", Material = "Cotton", Slug = "summer-dress" },
                new() { Name = "Maxi Dress", Description = "Flowing maxi dress for elegant everyday wear.", Price = 450.00m, Category = "Dress", SubCategory = "Women's", Size = "L", Color = "Navy Blue", Material = "Jersey", Slug = "maxi-dress" },
                
                // Casual Wear
                new() { Name = "Casual Shirt", Description = "Comfortable casual shirt for everyday wear.", Price = 220.00m, Category = "Casual", SubCategory = "Men's", Size = "L", Color = "Blue", Material = "Cotton", Slug = "casual-shirt" },
                new() { Name = "Linen Pants", Description = "Breathable linen pants perfect for summer.", Price = 280.00m, Category = "Casual", SubCategory = "Men's", Size = "M", Color = "Beige", Material = "Linen", Slug = "linen-pants" },
                new() { Name = "Polo Shirt", Description = "Classic polo shirt in premium cotton.", Price = 180.00m, Category = "Casual", SubCategory = "Men's", Size = "M", Color = "White", Material = "Pique Cotton", Slug = "polo-shirt" },
                
                // Women's Traditional & Modern
                new() { Name = "Modern Kaftan", Description = "Contemporary kaftan with elegant design.", Price = 520.00m, Category = "Traditional", SubCategory = "Women's", Size = "One Size", Color = "Turquoise", Material = "Silk Blend", Slug = "modern-kaftan" },
                new() { Name = "Embroidered Tunic", Description = "Beautiful embroidered tunic for modest fashion.", Price = 340.00m, Category = "Casual", SubCategory = "Women's", Size = "M", Color = "Cream", Material = "Cotton", Slug = "embroidered-tunic" },
                
                // Accessories
                new() { Name = "Leather Belt", Description = "Genuine leather belt in classic design.", Price = 120.00m, Category = "Accessories", SubCategory = "Unisex", Size = "Adjustable", Color = "Brown", Material = "Leather", Slug = "leather-belt" },
                new() { Name = "Silk Scarf", Description = "Elegant silk scarf with traditional patterns.", Price = 95.00m, Category = "Accessories", SubCategory = "Women's", Size = "Standard", Color = "Multi", Material = "Silk", Slug = "silk-scarf" }
            };


            return templates[random.Next(templates.Count)];
        }

        private class ProductTemplate
        {
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public decimal Price { get; set; }
            public string Category { get; set; } = "";
            public string SubCategory { get; set; } = "";
            public string Size { get; set; } = "";
            public string Color { get; set; } = "";
            public string Material { get; set; } = "";
            public string Slug { get; set; } = "";
            public string MetaTitle => $"{Name} - Tafsilk";
            public string MetaDescription => Description;
        }
    }
}
