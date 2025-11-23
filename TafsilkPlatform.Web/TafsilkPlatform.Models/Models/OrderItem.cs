using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Models.Models
{
    public class OrderItem
    {
        [Key]
        public Guid OrderItemId { get; set; }

        [Required]
        [MaxLength(255)]
        public required string Description { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        // ✅ NEW: Product reference for store purchases
        public Guid? ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [MaxLength(50)]
        public string? SelectedSize { get; set; }

        [MaxLength(50)]
        public string? SelectedColor { get; set; }

        [MaxLength(500)]
        public string? SpecialInstructions { get; set; }

        // Order relation
        [Required]
        [ForeignKey("Order")]
        public Guid OrderId { get; set; }

        public required Order Order { get; set; }
    }
}
