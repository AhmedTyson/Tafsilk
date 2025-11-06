using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Web.Models
{
    /// <summary>
    /// Customer loyalty and rewards system
  /// Tracks points, level, and redemption history
    /// </summary>
    [Table("CustomerLoyalty")]
    public class CustomerLoyalty
    {
        [Key]
  public Guid Id { get; set; }

 [Required]
        public Guid CustomerId { get; set; }

        /// <summary>
 /// Current loyalty points balance
/// </summary>
    public int Points { get; set; } = 0;

        /// <summary>
  /// Total lifetime points earned
/// </summary>
        public int LifetimePoints { get; set; } = 0;

    /// <summary>
   /// Loyalty tier/level (Bronze, Silver, Gold, Platinum)
        /// </summary>
        [MaxLength(50)]
        public string Tier { get; set; } = "Bronze";

        /// <summary>
   /// Number of completed orders
        /// </summary>
        public int TotalOrders { get; set; } = 0;

/// <summary>
   /// Number of referrals made
/// </summary>
    public int ReferralsCount { get; set; } = 0;

        /// <summary>
 /// Referral code for this customer
  /// </summary>
        [MaxLength(20)]
        public string? ReferralCode { get; set; }

      /// <summary>
/// Who referred this customer (referrer's customer ID)
    /// </summary>
  public Guid? ReferredBy { get; set; }

     public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

  // Navigation properties
        [ForeignKey("CustomerId")]
 public virtual CustomerProfile Customer { get; set; } = null!;

        public virtual ICollection<LoyaltyTransaction> Transactions { get; set; } = new List<LoyaltyTransaction>();
    }

    /// <summary>
    /// Individual loyalty point transactions (earning and redemption)
    /// </summary>
    [Table("LoyaltyTransactions")]
    public class LoyaltyTransaction
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid CustomerLoyaltyId { get; set; }

 [Required]
    public int Points { get; set; } // Positive for earning, negative for redemption

        [Required]
        [MaxLength(20)]
    public string Type { get; set; } = null!; // "Earned", "Redeemed", "Expired", "Bonus"

        [MaxLength(200)]
public string? Description { get; set; }

        public Guid? RelatedOrderId { get; set; } // If points earned/redeemed from an order

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("CustomerLoyaltyId")]
        public virtual CustomerLoyalty CustomerLoyalty { get; set; } = null!;
    }
}
