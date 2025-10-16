using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TafsilkPlatform.Models
{
    public class ErrorLog
    {
        public Guid ErrorLogId { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string Severity { get; set; } 
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
