using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Web.Models
{
    public class Contract
    {
        public Guid Id { get; set; }
        public Guid RFQId { get; set; }
        public Guid TailorId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string ContractStatus { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("RFQId")]
        public virtual RFQ RFQ { get; set; } = null!;
        [ForeignKey("TailorId")]
        public virtual TailorProfile Tailor { get; set; } = null!;
    }
}

