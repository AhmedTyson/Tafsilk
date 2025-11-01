using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TafsilkPlatform.Web.Models
{
   
    public class UserActivityLog
    {
        public Guid UserActivityLogId { get; set; }
        public Guid UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int? EntityId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string? Details { get; set; }  // Add this property if missing

        public User? User { get; set; }

        public bool IsDeleted { get; set; }
    }
}
