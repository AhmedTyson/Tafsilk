using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Web.Models
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public Guid AdminId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string AffectedEntity { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        [ForeignKey("AdminId")]
        public Admin? Admin { get; set; }
    }
}


