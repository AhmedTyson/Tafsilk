using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TafsilkPlatform.Web.Models
{
    public class TailorBadge
    {
        public Guid TailorBadgeId { get; set; }
        public Guid TailorId { get; set; }
        public string BadgeName { get; set; } = string.Empty;
        public DateTime EarnedAt { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } // Add this property to fix CS1061
    }
}
