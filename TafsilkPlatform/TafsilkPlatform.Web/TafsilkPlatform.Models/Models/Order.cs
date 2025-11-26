using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace TafsilkPlatform.Models.Models
{
    public class Order
    {
        [Key]
        [Required]
        public Guid OrderId { get; set; }
        public required string Description { get; set; } // ✅ FIXED: Corrected typo from "Discription"
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? DueDate { get; set; }
        public double TotalPrice { get; set; }
        public required string OrderType { get; set; }
        public OrderStatus Status { get; set; }

        // ✅ NEW: Commission Tracking
        public double CommissionAmount { get; set; }
        public double CommissionRate { get; set; } = 0.10; // Default 10%

        // ✅ NEW: Quote/pricing from tailor
        public double? TailorQuote { get; set; }
        public string? TailorQuoteNotes { get; set; }
        public DateTimeOffset? QuoteProvidedAt { get; set; }

        // ✅ NEW: Deposit payment tracking
        public bool RequiresDeposit { get; set; } = false;
        public double? DepositAmount { get; set; }
        public bool DepositPaid { get; set; } = false;
        public DateTimeOffset? DepositPaidAt { get; set; }

        // ✅ NEW: Measurements stored with order
        public string? MeasurementsJson { get; set; }

        // ✅ NEW: Pickup/delivery preferences
        [MaxLength(20)]
        public string? FulfillmentMethod { get; set; } // "Pickup", "Delivery"
        public string? DeliveryAddress { get; set; }

        [ForeignKey("Customer")]
        public Guid CustomerId { get; set; }
        public required CustomerProfile Customer { get; set; }
        [ForeignKey("Tailor")]
        public Guid TailorId { get; set; }
        public required TailorProfile Tailor { get; set; }

        public ICollection<OrderImages> orderImages { get; set; } = new List<OrderImages>();
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        // ✅ NEW: Complaints
        public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    }
}
