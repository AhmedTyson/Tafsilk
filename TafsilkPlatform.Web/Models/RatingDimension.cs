using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TafsilkPlatform.Web.Models
{
    public class RatingDimension
    {
        public Guid RatingDimensionId { get; set; }
        public Guid ReviewId { get; set; }
        public string DimensionName { get; set; } = string.Empty;
        public int Score { get; set; }
        public Review? Review { get; set; } // Made nullable to fix CS8618
        public bool IsDeleted { get; set; } // Add this property to fix CS1061
    }
}
