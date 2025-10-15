using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TafsilkPlatform.Models
{
    public class UserActivityLog
    {
        public int UserActivityLogId { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; }
        public string EntityType { get; set; }
        public int? EntityId { get; set; } 
        public DateTime CreatedAt { get; set; }
        public string IpAddress { get; set; }

        public User User { get; set; }
    }
}
