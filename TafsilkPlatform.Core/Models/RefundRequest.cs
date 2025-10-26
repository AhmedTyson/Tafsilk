using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Core.Models
{
    public class RefundRequest
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid RequestedBy { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ProcessedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;
        [ForeignKey("RequestedBy")]
        public virtual User RequestedByUser { get; set; } = null!;
    }
}
