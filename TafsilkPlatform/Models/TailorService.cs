using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TafsilkPlatform.Models
{
    public class TailorService
    {
        public Guid TailorServiceId { get; set; }
        public Guid TailorId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public int EstimatedDuration { get; set; }
        public TailorProfile? Tailor { get; set; } // Made nullable to fix CS8618
        public bool IsDeleted { get; set; } // Add this property to fix CS1061
    }
}
