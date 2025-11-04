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
        public required string Description { get; set; } // ✅ FIXED: Corrected typo from "Discription"
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? DueDate { get; set; }
        public double TotalPrice { get; set; }
        public required string OrderType { get; set; }
        public OrderStatus Status { get; set; }

        [ForeignKey("Customer")]
        public Guid CustomerId { get; set; }
        public required CustomerProfile Customer { get; set; }
        [ForeignKey("Tailor")]
        public Guid TailorId { get; set; }
        public required TailorProfile Tailor { get; set; }

        public ICollection<OrderImages> orderImages { get; set; } = new List<OrderImages>();
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
