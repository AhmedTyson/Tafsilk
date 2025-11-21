using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Web.Models
{
    public class Payment
    {
        [Key]
        public Guid PaymentId { get; set; }

        [ForeignKey("Order")]
        public Guid OrderId { get; set; }
        public required Order Order { get; set; }

        [ForeignKey("Customer")]
        public Guid CustomerId { get; set; }
        public required CustomerProfile Customer { get; set; }

        [ForeignKey("Tailor")]
        public Guid TailorId { get; set; }
        public required TailorProfile Tailor { get; set; }

        public decimal Amount { get; set; }
        public Enums.PaymentType PaymentType { get; set; }
        public Enums.PaymentStatus PaymentStatus { get; set; }
        public Enums.TransactionType TransactionType { get; set; }
        public DateTimeOffset PaidAt { get; set; } = DateTimeOffset.UtcNow;

        // ✅ NEW: Stripe integration fields
        /// <summary>
        /// Stripe PaymentIntent ID or other provider transaction ID
        /// </summary>
        [MaxLength(255)]
        public string? ProviderTransactionId { get; set; }

        /// <summary>
        /// Stripe Customer ID (for saved payment methods)
        /// </summary>
        [MaxLength(255)]
        public string? ProviderCustomerId { get; set; }

        /// <summary>
        /// Payment provider name (Stripe, PayPal, etc.)
        /// </summary>
        [MaxLength(50)]
        public string? Provider { get; set; } = "Internal"; // Internal, Stripe, PayPal

        /// <summary>
        /// Provider-specific metadata (JSON)
        /// </summary>
        public string? ProviderMetadata { get; set; }

        /// <summary>
        /// Last 4 digits of card (for display purposes)
        /// </summary>
        [MaxLength(4)]
        public string? CardLast4 { get; set; }

        /// <summary>
        /// Card brand (Visa, MasterCard, Amex, etc.)
        /// </summary>
        [MaxLength(20)]
        public string? CardBrand { get; set; }

        /// <summary>
        /// Currency code (SAR, USD, EUR, etc.)
        /// </summary>
        [MaxLength(3)]
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Whether 3D Secure was used
        /// </summary>
        public bool? ThreeDSecureUsed { get; set; }

        /// <summary>
        /// Payment failure reason (if failed)
        /// </summary>
        [MaxLength(500)]
        public string? FailureReason { get; set; }

        /// <summary>
        /// Refunded amount (if any)
        /// </summary>
        public decimal? RefundedAmount { get; set; }

        /// <summary>
        /// Date of last refund
        /// </summary>
        public DateTimeOffset? RefundedAt { get; set; }

        /// <summary>
        /// Additional notes
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        /// <summary>
        /// Created date
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Last updated date
        /// </summary>
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
