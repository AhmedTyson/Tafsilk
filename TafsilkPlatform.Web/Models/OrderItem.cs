using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Web.Models
{
    public class OrderItem
    {
        [Key]
        public Guid OrderItemId { get; set; }

        public Guid OrderId { get; set; }

        // use PascalCase navigation name so EF conventions map correctly
        public required Order Order { get; set; }

        public required string ItemName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }
}
