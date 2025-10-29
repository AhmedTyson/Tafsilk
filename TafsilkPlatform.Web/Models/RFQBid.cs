using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Web.Models
{
    public class RFQBid
    {
        public Guid Id { get; set; }
        public Guid RFQId { get; set; }
        public Guid TailorId { get; set; }
        public decimal BidAmount { get; set; }
        public DateTime EstimatedDelivery { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("RFQId")]
        public virtual RFQ RFQ { get; set; } = null!;
        [ForeignKey("TailorId")]
        public virtual TailorProfile Tailor { get; set; } = null!;
    }
}

