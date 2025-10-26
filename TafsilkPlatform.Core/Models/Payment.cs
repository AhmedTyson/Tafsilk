using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TafsilkPlatform.Core.Models
{
    public class Payment
    {
        [Key]
        public Guid PaymentId { get; set; }

        [ForeignKey("Order")]
        public Guid OrderId { get; set; }
        public Order Order { get; set; }

        [ForeignKey("Customer")]
        public Guid CustomerId { get; set; }
        public CustomerProfile Customer { get; set; }

        [ForeignKey("Tailor")]
        public Guid TailorId { get; set; }
        public TailorProfile Tailor { get; set; }

        public decimal Amount { get; set; }
        public Enums.PaymentType PaymentType { get; set; }
        public Enums.PaymentStatus PaymentStatus { get; set; }
        public Enums.TransactionType TransactionType { get; set; }
        public DateTimeOffset PaidAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
