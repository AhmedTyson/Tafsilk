using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TafsilkPlatform.Core.Models
{
    public class TailorPerformanceView
    {
        public Guid TailorId { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalOrders { get; set; }
        public decimal Revenue { get; set; }
        public TailorProfile? Tailor { get; set; }
    }
}
