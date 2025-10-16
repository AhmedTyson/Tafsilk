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
        public string Title { get; set; }
        public string Content { get; set; }
        public string AudienceType { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool IsDeleted { get; set; }
    }
}
