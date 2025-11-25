using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Models.Models
{
    /// <summary>
/// Customer complaint/dispute system for issue resolution
    /// Part of support workflow
    /// </summary>
    [Table("Complaints")]
    public class Complaint
    {
     [Key]
        public Guid Id { get; set; }

     [Required]
        public Guid OrderId { get; set; }

   [Required]
        public Guid CustomerId { get; set; }

    [Required]
        public Guid TailorId { get; set; }

 [Required]
        [MaxLength(100)]
        [Display(Name = "Complaint Subject")]
        public string Subject { get; set; } = null!;

      [Required]
        [MaxLength(2000)]
     [Display(Name = "Complaint Description")]
   public string Description { get; set; } = null!;

/// <summary>
/// Type of complaint (Quality, Delay, Communication, Pricing, Other)
    /// </summary>
  [Required]
     [MaxLength(50)]
public string ComplaintType { get; set; } = "Other";

      /// <summary>
  /// Desired resolution (Refund, Rework, PartialRefund, Apology)
    /// </summary>
        [MaxLength(50)]
   public string? DesiredResolution { get; set; }

        /// <summary>
   /// Current status (Open, UnderReview, Resolved, Rejected, Escalated)
      /// </summary>
     [Required]
   [MaxLength(50)]
public string Status { get; set; } = "Open";

        /// <summary>
        /// Priority level (Low, Medium, High, Critical)
   /// </summary>
  [MaxLength(20)]
        public string Priority { get; set; } = "Medium";

    // Evidence/attachments
        public virtual ICollection<ComplaintAttachment> Attachments { get; set; } = new List<ComplaintAttachment>();

        // Admin/support response
        [MaxLength(2000)]
    public string? AdminResponse { get; set; }

        public Guid? ResolvedBy { get; set; } // Admin/Support user ID

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
public DateTime? UpdatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }

        // Navigation properties
        [ForeignKey("OrderId")]
  public virtual Order Order { get; set; } = null!;

    [ForeignKey("CustomerId")]
        public virtual CustomerProfile Customer { get; set; } = null!;

        [ForeignKey("TailorId")]
        public virtual TailorProfile Tailor { get; set; } = null!;
    }

    /// <summary>
    /// Complaint evidence attachments (photos of poor quality work, etc.)
    /// </summary>
    [Table("ComplaintAttachments")]
    public class ComplaintAttachment
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ComplaintId { get; set; }

 [MaxLength]
        public byte[]? FileData { get; set; }

  [MaxLength(100)]
    public string? ContentType { get; set; }

        [MaxLength(255)]
public string? FileName { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
    [ForeignKey("ComplaintId")]
        public virtual Complaint Complaint { get; set; } = null!;
    }
}
