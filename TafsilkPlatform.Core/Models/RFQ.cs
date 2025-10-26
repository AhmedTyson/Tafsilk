using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Core.Models
{
    public class RFQ
    {
        public Guid Id { get; set; }
        public Guid CorporateAccountId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Budget { get; set; }
        public DateTime Deadline { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CorporateAccountId")]
        public virtual CorporateAccount CorporateAccount { get; set; } = null!;
        public virtual ICollection<RFQBid> Bids { get; set; } = new List<RFQBid>();
    }
}