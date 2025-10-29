using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TafsilkPlatform.Web.Models
{
    public class OrderItem
    {
        [Key]
        public Guid OrderItemId { get; set; }
        [ForeignKey("order")]
        public Guid OrderId { get; set; }
<<<<<<< Updated upstream
        public Order order { get; set; }
        public string ItemName { get; set; }
=======
        public required Order order { get; set; }
        public required string ItemName { get; set; }
>>>>>>> Stashed changes
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }
}
