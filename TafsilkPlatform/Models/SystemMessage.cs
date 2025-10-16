using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TafsilkPlatform.Models
{
    public class SystemMessage
    {
        public Guid SystemMessageId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string AudienceType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public bool IsDeleted { get; set; }
    }
}
