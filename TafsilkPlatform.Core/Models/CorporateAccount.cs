using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Core.Models
{
    [Table("CorporateAccounts")]
    public partial class CorporateAccount
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Company name is required")]
        [StringLength(255, ErrorMessage = "Company name cannot exceed 255 characters")]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = null!;

        [Required(ErrorMessage = "Contact person is required")]
        [StringLength(255, ErrorMessage = "Contact person name cannot exceed 255 characters")]
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; } = null!;

        [StringLength(100, ErrorMessage = "Industry cannot exceed 100 characters")]
        public string? Industry { get; set; }

        [StringLength(100, ErrorMessage = "Tax number cannot exceed 100 characters")]
        [Display(Name = "Tax Number")]
        public string? TaxNumber { get; set; }

        [Display(Name = "Approval Status")]
        public bool IsApproved { get; set; } = false;

        [Display(Name = "Created Date")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Updated Date")]
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}