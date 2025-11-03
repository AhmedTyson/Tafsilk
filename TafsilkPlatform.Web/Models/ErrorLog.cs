using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TafsilkPlatform.Web.Models
{
    /// <summary>
    /// ⚠️ DEPRECATED: This table stores error logs in the database which impacts performance.
    /// 
    /// RECOMMENDED: Use Serilog with external sinks instead:
    /// - File logging (with rolling)
    /// - Application Insights
    /// - Seq, Elasticsearch, etc.
    /// 
    /// Benefits:
    /// - Better performance (no DB writes on errors)
    /// - Built-in log rotation
    /// - Better querying capabilities
    /// - Industry standard approach
    /// 
    /// This table will be removed in a future version.
    /// </summary>
    [Obsolete("Use Serilog with external logging sinks instead of database logging. This will be removed in future versions.")]
    public class ErrorLog
    {
        [Key]
        public Guid ErrorLogId { get; set; }
        
        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;
   
        [MaxLength]
        public string StackTrace { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string Severity { get; set; } = string.Empty;
  
        public DateTime CreatedAt { get; set; }
   
        public bool IsDeleted { get; set; }
    }
}
