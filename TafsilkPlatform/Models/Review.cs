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
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; } // <-- Add this property to fix CS1061
        public Order Order { get; set; }
        public TailorProfile Tailor { get; set; }
        public CustomerProfile Customer { get; set; }
        public ICollection<RatingDimension> RatingDimensions { get; set; }
    }
}
