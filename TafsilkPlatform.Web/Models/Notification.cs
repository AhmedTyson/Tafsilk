using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Web.Models
{
    /// <summary>
    /// Unified notification system supporting both personal and system-wide messages
    /// Replaces both Notification and SystemMessage tables
    /// </summary>
    public class Notification
    {
        [Key]
        public Guid NotificationId { get; set; }

        /// <summary>
        /// User ID for personal notifications. NULL for system-wide broadcasts
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Target audience for system messages: "All", "Customers", "Tailors", "Corporate"
        /// NULL for personal notifications
        /// </summary>
        [MaxLength(50)]
        public string? AudienceType { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Notification type: "Info", "Success", "Warning", "Error", "System"
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Read status (only applies to personal notifications)
        /// </summary>
        public bool IsRead { get; set; }

        public DateTime SentAt { get; set; }

        /// <summary>
        /// Expiration time for time-sensitive announcements
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        public bool IsDeleted { get; set; }

        // Navigation property
        public User? User { get; set; }

        /// <summary>
        /// Helper property to determine if this is a system-wide message
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public bool IsSystemMessage => !UserId.HasValue && !string.IsNullOrEmpty(AudienceType);

        /// <summary>
        /// Helper property to check if notification is still valid
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    }
}
