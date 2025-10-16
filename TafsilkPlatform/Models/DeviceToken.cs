using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TafsilkPlatform.Models
{
    public class DeviceToken
    {
        public Guid DeviceTokenId { get; set; }
        public Guid UserId { get; set; }
        public string? Devicetoken { get; set; }
        public string Platform { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        public User? User { get; set; }
        public bool IsDeleted { get; set; }
    }
}
