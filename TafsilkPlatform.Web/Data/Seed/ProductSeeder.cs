using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Models;

namespace TafsilkPlatform.Web.Data.Seed
{
    public static class ProductSeeder
    {
     public static async Task SeedProductsAsync(AppDbContext context)
        {
      if (await context.Products.AnyAsync())
     {
     return; // Already seeded
            }

  // Get a system tailor or create one
  var systemTailor = await context.TailorProfiles.FirstOrDefaultAsync();
         if (systemTailor == null)
         {
         Console.WriteLine("⚠️ No tailor found. Please create at least one tailor before seeding products.");
      return;
    }

            var products = new List<Product>
{
          // Traditional Thobes
       new Product
     {
        Name = "Classic White Thobe",
     Description = "Traditional Saudi white thobe made from premium cotton fabric. Perfect for daily wear and special occasions.",
        Price = 250.00m,
        Category = "Thobe",
         SubCategory = "Men's",
        Size = "L",
        Color = "White",
  Material = "100% Cotton",
    Brand = "Tafsilk Premium",
    StockQuantity = 50,
    IsAvailable = true,
   IsFeatured = true,
       Slug = "classic-white-thobe",
        MetaTitle = "Classic White Thobe - Premium Saudi Traditional Wear",
              MetaDescription = "Buy authentic Saudi white thobe online. Made from premium cotton for comfort and style.",
      TailorId = systemTailor.Id
      },
          new Product
        {
        Name = "Embroidered Thobe - Black",
       Description = "Elegant black thobe with intricate embroidery on collar and cuffs. Premium quality fabric with modern cut.",
                  Price = 450.00m,
     DiscountedPrice = 380.00m,
          Category = "Thobe",
               SubCategory = "Men's",
  Size = "XL",
               Color = "Black",
        Material = "Premium Polyester Blend",
   Brand = "Tafsilk Luxury",
         StockQuantity = 30,
            IsAvailable = true,
         IsFeatured = true,
      Slug = "embroidered-thobe-black",
   MetaTitle = "Black Embroidered Thobe - Luxury Traditional Wear",
  TailorId = systemTailor.Id
  },
                
        // Abayas
     new Product
       {
      Name = "Classic Black Abaya",
       Description = "Elegant and modest black abaya perfect for everyday wear. Made from breathable fabric for comfort.",
     Price = 180.00m,
    Category = "Abaya",
          SubCategory = "Women's",
         Size = "M",
       Color = "Black",
    Material = "Crepe Fabric",
    Brand = "Tafsilk Modest",
         StockQuantity = 75,
          IsAvailable = true,
            IsFeatured = true,
          Slug = "classic-black-abaya",
 MetaTitle = "Classic Black Abaya - Modest Fashion",
        TailorId = systemTailor.Id
    },
   new Product
       {
  Name = "Embellished Abaya - Navy Blue",
  Description = "Beautiful navy blue abaya with delicate embellishments and pearl details. Perfect for special occasions.",
   Price = 550.00m,
         DiscountedPrice = 475.00m,
   Category = "Abaya",
            SubCategory = "Women's",
   Size = "L",
   Color = "Navy Blue",
               Material = "Premium Nida Fabric",
              Brand = "Tafsilk Luxury",
                StockQuantity = 25,
           IsAvailable = true,
          IsFeatured = true,
    Slug = "embellished-abaya-navy",
              TailorId = systemTailor.Id
          },
    
                // Suits
             new Product
                {
  Name = "Business Suit - Charcoal Grey",
     Description = "Professional 2-piece business suit in charcoal grey. Tailored fit with modern styling.",
         Price = 1200.00m,
Category = "Suit",
           SubCategory = "Men's",
   Size = "L",
           Color = "Charcoal Grey",
     Material = "Wool Blend",
     Brand = "Tafsilk Executive",
    StockQuantity = 20,
     IsAvailable = true,
               IsFeatured = false,
      Slug = "business-suit-charcoal",
      TailorId = systemTailor.Id
                },
         new Product
                {
      Name = "Wedding Suit - Navy",
        Description = "Luxurious 3-piece navy suit perfect for weddings and formal events. Includes jacket, waistcoat, and trousers.",
   Price = 1800.00m,
        DiscountedPrice = 1550.00m,
    Category = "Suit",
     SubCategory = "Men's",
           Size = "M",
      Color = "Navy Blue",
         Material = "Premium Wool",
          Brand = "Tafsilk Luxury",
 StockQuantity = 15,
         IsAvailable = true,
     IsFeatured = true,
         Slug = "wedding-suit-navy",
        TailorId = systemTailor.Id
        },
     
    // Traditional Wear
new Product
     {
          Name = "Children's Thobe - White",
            Description = "Adorable white thobe for boys. Comfortable and perfect for Eid and special occasions.",
  Price = 120.00m,
   Category = "Traditional",
    SubCategory = "Children's",
   Size = "S",
         Color = "White",
   Material = "Soft Cotton",
      Brand = "Tafsilk Kids",
              StockQuantity = 60,
           IsAvailable = true,
          IsFeatured = false,
       Slug = "childrens-thobe-white",
   TailorId = systemTailor.Id
             },
   new Product
    {
        Name = "Traditional Saudi Bisht - Brown",
          Description = "Authentic Saudi bisht in rich brown color with golden trim. Worn over thobe for special occasions.",
          Price = 850.00m,
        Category = "Traditional",
       SubCategory = "Men's",
          Color = "Brown",
        Material = "Fine Wool",
           Brand = "Tafsilk Heritage",
         StockQuantity = 12,
         IsAvailable = true,
     IsFeatured = true,
            Slug = "traditional-bisht-brown",
  TailorId = systemTailor.Id
     },
         
    // Accessories
              new Product
        {
Name = "Premium Shemagh - Red & White",
       Description = "Traditional Saudi shemagh in red and white pattern. High-quality cotton fabric.",
             Price = 65.00m,
    Category = "Accessories",
        Color = "Red & White",
          Material = "100% Cotton",
             Brand = "Tafsilk Accessories",
              StockQuantity = 100,
     IsAvailable = true,
             IsFeatured = false,
   Slug = "shemagh-red-white",
                    TailorId = systemTailor.Id
   },
     new Product
    {
              Name = "Leather Belt - Black",
  Description = "Genuine leather belt in classic black. Perfect complement to thobes and suits.",
Price = 95.00m,
          Category = "Accessories",
      Color = "Black",
 Material = "Genuine Leather",
 Brand = "Tafsilk Accessories",
 StockQuantity = 80,
      IsAvailable = true,
         IsFeatured = false,
           Slug = "leather-belt-black",
   TailorId = systemTailor.Id
       },
        
            // Special Edition
   new Product
      {
       Name = "Ramadan Special Thobe - Cream",
  Description = "Limited edition Ramadan thobe in elegant cream color. Features special embroidery and premium fabric.",
   Price = 380.00m,
       DiscountedPrice = 320.00m,
   Category = "Thobe",
           SubCategory = "Men's",
    Size = "L",
     Color = "Cream",
       Material = "Premium Cotton Blend",
 Brand = "Tafsilk Special Edition",
        StockQuantity = 5,
          IsAvailable = true,
          IsFeatured = true,
           Slug = "ramadan-thobe-cream",
     TailorId = systemTailor.Id
     },
       new Product
 {
         Name = "Designer Abaya - Burgundy",
   Description = "Contemporary designer abaya in rich burgundy color. Modern cut with elegant draping.",
    Price = 680.00m,
        Category = "Abaya",
         SubCategory = "Women's",
          Size = "M",
   Color = "Burgundy",
         Material = "Designer Crepe",
    Brand = "Tafsilk Designer",
           StockQuantity = 18,
             IsAvailable = true,
          IsFeatured = true,
      Slug = "designer-abaya-burgundy",
   TailorId = systemTailor.Id
         }
            };

        // Set random ratings for variety
            var random = new Random();
         foreach (var product in products)
         {
      product.AverageRating = Math.Round(3.5 + (random.NextDouble() * 1.5), 1); // 3.5 - 5.0
    product.ReviewCount = random.Next(5, 50);
      product.SalesCount = random.Next(10, 200);
     product.ViewCount = random.Next(100, 1000);
        }

    await context.Products.AddRangeAsync(products);
          await context.SaveChangesAsync();

            Console.WriteLine($"✅ Seeded {products.Count} products successfully!");
   }
    }
}
