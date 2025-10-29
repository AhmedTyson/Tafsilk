using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TafsilkPlatform.Web.Models
{
    public class Order
    {
        [Key]
        [Required]
        public Guid OrderId { get; set; }
<<<<<<< Updated upstream
        public string Discription { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? DueDate { get; set; }
        public double TotalPrice { get; set; }
        public string OrderType { get; set; }
=======
        public required string Discription { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? DueDate { get; set; }
        public double TotalPrice { get; set; }
        public required string OrderType { get; set; }
>>>>>>> Stashed changes
        public OrderStatus Status { get; set; }

        [ForeignKey("Customer")]
        public Guid CustomerId { get; set; }
<<<<<<< Updated upstream
        public CustomerProfile Customer { get; set; }
        [ForeignKey("Tailor")]
        public Guid TailorId { get; set; }
        public TailorProfile Tailor { get; set; }
=======
        public required CustomerProfile Customer { get; set; }
        [ForeignKey("Tailor")]
        public Guid TailorId { get; set; }
        public required TailorProfile Tailor { get; set; }
>>>>>>> Stashed changes

        public ICollection<OrderImages> orderImages { get; set; } = new List<OrderImages>();
        public ICollection<Quote> quote { get; set; } = new List<Quote>();
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
