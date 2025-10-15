using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TafsilkPlatform.Models
{
    public class RevenueReport
    {
        public int TailorId { get; set; }
        public DateTime Month { get; set; } 
        public decimal TotalRevenue { get; set; }
        public int CompletedOrders { get; set; }
        public DateTime GeneratedAt { get; set; }

        public TailorProfile Tailor { get; set; }
    }
}
