using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Models.Models
{
    /// <summary>
    /// Represents an item in the shopping cart
    /// </summary>
    public class CartItem
    {
        [Key]
 public Guid CartItemId { get; set; } = Guid.NewGuid();

  [Required]
        public Guid CartId { get; set; }

     [ForeignKey("CartId")]
  public required ShoppingCart Cart { get; set; }

        [Required]
 public Guid ProductId { get; set; }

        [ForeignKey("ProductId")]
        public required Product Product { get; set; }

        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; } = 1;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
  public decimal UnitPrice { get; set; }

        [MaxLength(50)]
        public string? SelectedSize { get; set; }

  [MaxLength(50)]
    public string? SelectedColor { get; set; }

        [MaxLength(500)]
        public string? SpecialInstructions { get; set; }

        public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }

        // Calculated property
        [NotMapped]
public decimal TotalPrice => UnitPrice * Quantity;
    }
}
