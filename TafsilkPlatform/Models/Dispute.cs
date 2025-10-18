using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Models
{
    public class Dispute
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid OpenedByUserId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public Guid? ResolvedByAdminId { get; set; }
        public string? ResolutionDetails { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;
        [ForeignKey("OpenedByUserId")]
        public virtual User OpenedByUser { get; set; } = null!;
        [ForeignKey("ResolvedByAdminId")]
        public virtual User? ResolvedByAdmin { get; set; }
    }
}