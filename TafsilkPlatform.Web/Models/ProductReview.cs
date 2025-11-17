using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Web.Models
{
    /// <summary>
    /// Customer review for a product
    /// </summary>
    public class ProductReview
    {
     [Key]
        public Guid ReviewId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ProductId { get; set; }

[ForeignKey("ProductId")]
        public required Product Product { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [ForeignKey("CustomerId")]
   public required CustomerProfile Customer { get; set; }

        public Guid? OrderId { get; set; }

        [ForeignKey("OrderId")]
      public virtual Order? Order { get; set; }

     [Required]
     [Range(1, 5)]
        public int Rating { get; set; }

      [MaxLength(100)]
        public string? Title { get; set; }

     [MaxLength(1000)]
    public string? Comment { get; set; }

     public bool IsVerifiedPurchase { get; set; } = false;

      public bool IsApproved { get; set; } = false;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        
   public int HelpfulCount { get; set; } = 0;
    }
}
