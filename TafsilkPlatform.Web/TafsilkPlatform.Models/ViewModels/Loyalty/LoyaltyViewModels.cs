using TafsilkPlatform.Models.Models;
using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Models.ViewModels.Loyalty
{
    /// <summary>
  /// Customer loyalty dashboard view model
    /// </summary>
    public class LoyaltyDashboardViewModel
    {
        public int CurrentPoints { get; set; }
        public int LifetimePoints { get; set; }
        public string Tier { get; set; } = "Bronze"; // Bronze, Silver, Gold, Platinum
        public int TotalOrders { get; set; }
        public int ReferralsCount { get; set; }
        public string? ReferralCode { get; set; }
        public int PointsToNextTier { get; set; }
        public string? NextTier { get; set; }
        
        // Recent transactions
        public List<LoyaltyTransactionViewModel> RecentTransactions { get; set; } = new();
        
   // Rewards catalog
        public List<RewardItemViewModel> AvailableRewards { get; set; } = new();
        
        // Tier benefits
  public TierBenefitsViewModel CurrentTierBenefits { get; set; } = new();
  }
    
    public class LoyaltyTransactionViewModel
    {
        public Guid Id { get; set; }
     public int Points { get; set; }
      public string Type { get; set; } = null!; // "Earned", "Redeemed", "Expired", "Bonus"
    public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
 
    public class RewardItemViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
   public int PointsRequired { get; set; }
        public string Type { get; set; } = null!; // "Discount", "FreeService", "VoucherCode"
        public decimal? DiscountValue { get; set; }
   public bool IsAvailable { get; set; }
    }
    
    public class TierBenefitsViewModel
    {
        public string TierName { get; set; } = null!;
        public decimal DiscountPercentage { get; set; }
        public bool PrioritySupport { get; set; }
  public bool FreeDelivery { get; set; }
        public int BonusPointsMultiplier { get; set; } = 1;
        public List<string> SpecialBenefits { get; set; } = new();
    }
    
    /// <summary>
    /// Redeem reward request
    /// </summary>
    public class RedeemRewardRequest
    {
        [Required]
        public Guid RewardId { get; set; }
        
        public Guid? OrderId { get; set; } // If applying to specific order
    }
}
