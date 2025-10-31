using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TafsilkPlatform.Web.Models
{
    public class TailorService
    {
        public Guid TailorServiceId { get; set; }
        public Guid TailorId { get; set; }

        // Navigation property name Tailor (PascalCase)
        public TailorProfile? Tailor { get; set; }

        public string ServiceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public int EstimatedDuration { get; set; }
        public bool IsDeleted { get; set; }
    }
}
