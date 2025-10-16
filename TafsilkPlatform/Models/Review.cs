using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TafsilkPlatform.Models
{
    public class Review
    {
        public Guid ReviewId { get; set; }
        public Guid OrderId { get; set; }
        public Guid TailorId { get; set; }
        public Guid CustomerId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; } // Made nullable to fix CS8618
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; } // <-- Add this property to fix CS1061
        public Order? Order { get; set; } // Made nullable to fix CS8618
        public TailorProfile? Tailor { get; set; } // Made nullable to fix CS8618
        public CustomerProfile? Customer { get; set; } // Made nullable to fix CS8618
        public ICollection<RatingDimension> RatingDimensions { get; set; } = new List<RatingDimension>();
    }
}
