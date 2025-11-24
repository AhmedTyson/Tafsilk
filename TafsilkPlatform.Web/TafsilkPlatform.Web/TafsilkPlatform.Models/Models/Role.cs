using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Models.Models
{
    [Table("Roles")]
    public partial class Role
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Role name is required")]
        [StringLength(50, ErrorMessage = "Role name cannot exceed 50 characters")]
        [Display(Name = "Role Name")]
        public string Name { get; set; } = null!;

        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        /// <summary>
        /// JSON string of permissions for this role
        /// Example: {"CanVerifyTailors": true, "CanManageUsers": true, "CanViewReports": true}
        /// Replaces the Admin table's Permissions field
        /// </summary>
        [MaxLength(2000)]
        public string? Permissions { get; set; }

        /// <summary>
        /// Priority level for role hierarchy (higher = more privileges)
        /// Admin = 100, Tailor = 50, Customer = 10, etc.
        /// </summary>
        public int Priority { get; set; } = 0;

        [Display(Name = "Created Date")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();

        /// <summary>
        /// Helper to check if this role is an admin role
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public bool IsAdminRole => Name?.Equals("Admin", StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
