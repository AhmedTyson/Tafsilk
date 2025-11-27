using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Models.Models
{
    /// <summary>
    /// Represents a customer's shopping cart
    /// </summary>
    public class ShoppingCart
    {
        [Key]
        public Guid CartId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public required CustomerProfile Customer { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }

        // Cart expiration (auto-cleanup after 30 days of inactivity)
        public DateTimeOffset? ExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<CartItem> Items { get; set; } = [];

        // Calculated properties
        [NotMapped]
        public decimal SubTotal => Items?.Sum(i => i.TotalPrice) ?? 0;

        [NotMapped]
        public int TotalItems => Items?.Sum(i => i.Quantity) ?? 0;
    }
}
