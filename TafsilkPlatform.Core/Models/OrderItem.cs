using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TafsilkPlatform.Core.Models
{
    public class OrderItem
    {
        [Key]
        public Guid OrderItemId { get; set; }
        [ForeignKey("order")]
        public Guid OrderId { get; set; }
        public Order order { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }
}
