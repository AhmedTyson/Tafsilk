using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TafsilkPlatform.Core.Models
{
    public class Quote
    {
        [Key]
        public Guid QuoteId { get; set; }
        [ForeignKey("order")]
        public Guid OrderId { get; set; }
        public Order order { get; set; }
        [ForeignKey("Tailor")]
        public Guid TailorId { get; set; }
        public TailorProfile Tailor { get; set; }
        public decimal ProposedPrice { get; set; }
        public int EstimatedDays { get; set; }
        public string Message { get; set; }


    }
}
