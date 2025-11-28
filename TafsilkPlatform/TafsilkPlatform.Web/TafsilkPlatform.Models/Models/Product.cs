using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Models.Models
{
    /// <summary>
    /// Represents a ready-made product/item available in the store
    /// </summary>
    public class Product
    {
        [Key]
        public Guid ProductId { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(200)]
        public required string Name { get; set; }

        [Required]
        [MaxLength(2000)]
        public required string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountedPrice { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Category { get; set; } // e.g., "Thobe", "Abaya", "Suit", "Traditional"

        [MaxLength(50)]
        public string? SubCategory { get; set; } // e.g., "Men's", "Women's", "Children's"

        [MaxLength(50)]
        public string? Size { get; set; } // S, M, L, XL, etc.

        [MaxLength(50)]
        public string? Color { get; set; }

        [MaxLength(100)]
        public string? Material { get; set; }

        [MaxLength(100)]
        public string? Brand { get; set; }

        [Required]
        public int StockQuantity { get; set; } = 0;

        public bool IsAvailable { get; set; } = true;

        public bool IsFeatured { get; set; } = false;

        public int ViewCount { get; set; } = 0;

        public int SalesCount { get; set; } = 0;

        public double AverageRating { get; set; } = 0.0;

        public int ReviewCount { get; set; } = 0;

        // Images stored as byte arrays
        public byte[]? PrimaryImageData { get; set; }

        [MaxLength(100)]
        public string? PrimaryImageContentType { get; set; }

        // New: filesystem-backed product image URL (stored under wwwroot/Attachments/product)
        [MaxLength(500)]
        public string? PrimaryImageUrl { get; set; }

        // Additional images stored as JSON array of base64 strings
        [MaxLength(4000)]
        public string? AdditionalImagesJson { get; set; }

        // SEO and metadata
        [MaxLength(200)]
        public string? MetaTitle { get; set; }

        [MaxLength(500)]
        public string? MetaDescription { get; set; }

        [MaxLength(200)]
        public string? Slug { get; set; }

        // Tailor who created this product (optional - for tailors selling ready-made items)
        public Guid? TailorId { get; set; }
        [ForeignKey("TailorId")]
        public virtual TailorProfile? Tailor { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual ICollection<CartItem> CartItems { get; set; } = [];

        public virtual ICollection<OrderItem> OrderItems { get; set; } = [];
    }
}
