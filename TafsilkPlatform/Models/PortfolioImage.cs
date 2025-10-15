using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TafsilkPlatform.Models
{
    public class PortfolioImage
    {
        public int PortfolioImageId { get; set; }
        public int TailorId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsBeforeAfter { get; set; }
        public DateTime UploadedAt { get; set; }
        public TailorProfile Tailor { get; set; }
    }
}
