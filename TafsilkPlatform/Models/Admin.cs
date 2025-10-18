using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Models
{
    public class Admin
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Permissions { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}


