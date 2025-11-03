using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TafsilkPlatform.Web.Models
{
    /// <summary>
    /// Unified activity log for all user and admin actions
    /// Replaces both UserActivityLog and AuditLog tables
    /// Renamed from UserActivityLog to ActivityLog for clarity
    /// </summary>
    public class ActivityLog
    {
      [Key]
        public Guid Id { get; set; }
        
        [Required]
  public Guid UserId { get; set; }
    
        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string EntityType { get; set; } = string.Empty;
  
        public Guid? EntityId { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; }
        
        [MaxLength(45)]
        public string? IpAddress { get; set; }
        
        [MaxLength(1000)]
    public string? Details { get; set; }
     
        /// <summary>
        /// Flag to distinguish admin actions from regular user actions
        /// </summary>
        public bool IsAdminAction { get; set; }
        
        public bool IsDeleted { get; set; }
        
     // Navigation property
        public User? User { get; set; }
    }
    
    /// <summary>
    /// Backward compatibility - keeps existing code working
    /// </summary>
    [Obsolete("Use ActivityLog instead. This class will be removed in a future version.")]
    public class UserActivityLog : ActivityLog
 {
        public Guid UserActivityLogId
 {
            get => Id;
            set => Id = value;
     }
    }
}
