using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TafsilkPlatform.Models
{
    public class PortfolioImage
    {
        public Guid PortfolioImageId { get; set; }
        public Guid TailorId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsBeforeAfter { get; set; }
        public DateTime UploadedAt { get; set; }
        public TailorProfile? Tailor { get; set; } // Made nullable to fix CS8618
        public bool IsDeleted { get; set; } // Add this property to fix CS1061
    }
}
