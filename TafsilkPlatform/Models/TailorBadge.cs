using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TafsilkPlatform.Models
{
    public class TailorBadge
    {
        public int TailorBadgeId { get; set; }
        public int TailorId { get; set; }
        public string BadgeName { get; set; }
        public DateTime EarnedAt { get; set; }
        public string Description { get; set; }
    }
}
