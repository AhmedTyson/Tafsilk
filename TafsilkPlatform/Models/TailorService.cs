using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TafsilkPlatform.Models
{
    public class TailorService
    {
        public int TailorServiceId { get; set; }
        public int TailorId { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public int EstimatedDuration { get; set; } 

        public TailorProfile Tailor { get; set; }
    }
}
