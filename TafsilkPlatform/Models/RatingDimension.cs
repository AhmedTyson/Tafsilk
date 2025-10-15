using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TafsilkPlatform.Models
{
    public class RatingDimension
    {
        public Guid RatingDimensionId { get; set; }
        public Guid ReviewId { get; set; }

        public string DimensionName { get; set; }
        public int Score  { get; set; }

        public Review Review { get; set; }

    }
}
