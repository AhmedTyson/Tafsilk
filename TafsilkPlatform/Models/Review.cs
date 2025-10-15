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

        public Order Order { get; set; }
        public Tailor Tailor { get; set; }
        public Customer Customer { get; set; }
        public ICollection<RatingDimension> RatingDimensions { get; set; } = new List<RatingDimension>();
    }
}
