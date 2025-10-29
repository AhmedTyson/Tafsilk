using System;

namespace TafsilkPlatform.Web.Models
{
    public class RevenueReport
    {
        public Guid TailorId { get; set; }
        public DateTime Month { get; set; }
        public decimal TotalRevenue { get; set; }
        public int CompletedOrders { get; set; }
        public DateTime GeneratedAt { get; set; }
        public TailorProfile? Tailor { get; set; }
        public bool IsDeleted { get; set; }
    }
}
