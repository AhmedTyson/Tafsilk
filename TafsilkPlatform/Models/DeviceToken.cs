using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TafsilkPlatform.Models
{
    internal class DeviceToken
    {
        public int DeviceTokenId { get; set; }
        public int UserId { get; set; }
        public string DeviceToken { get; set; }
        public string Platform { get; set; }
        public DateTime RegisteredAt { get; set; }

        public User User { get; set; }
    }
}
