using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Core.Models
{
    public class BannedUser
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime BannedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }       
}