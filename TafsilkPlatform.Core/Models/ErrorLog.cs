using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TafsilkPlatform.Core.Models
{
    public class ErrorLog
    {
        public Guid ErrorLogId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string StackTrace { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
