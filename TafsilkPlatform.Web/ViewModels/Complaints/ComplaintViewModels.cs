using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Web.ViewModels.Complaints
{
    /// <summary>
    /// Submit new complaint view model
    /// </summary>
    public class SubmitComplaintViewModel
    {
        [Required(ErrorMessage = "Order ID is required")]
    public Guid OrderId { get; set; }
        
   public string? OrderNumber { get; set; }
  public string? TailorName { get; set; }

        [Required(ErrorMessage = "Subject is required")]
   [MaxLength(100)]
        [Display(Name = "Complaint Subject")]
        public string Subject { get; set; } = null!;
   
        [Required(ErrorMessage = "Description is required")]
        [MaxLength(2000)]
   [Display(Name = "Describe the issue")]
   public string Description { get; set; } = null!;
  
        [Required(ErrorMessage = "Complaint type is required")]
        [Display(Name = "Issue Type")]
 public string ComplaintType { get; set; } = "Other";
        // Options: "Quality", "Delay", "Communication", "Pricing", "Other"
  
        [Display(Name = "Desired Resolution")]
        public string? DesiredResolution { get; set; }
        // Options: "Refund", "Rework", "PartialRefund", "Apology"
  
      [Display(Name = "Upload Evidence (Photos)")]
        [MaxLength(5, ErrorMessage = "Maximum 5 photos allowed")]
        public List<IFormFile>? EvidencePhotos { get; set; }
    }
    
    /// <summary>
    /// Complaint details view model
  /// </summary>
 public class ComplaintDetailsViewModel
    {
    public Guid Id { get; set; }
  public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = null!;
 public string Subject { get; set; } = null!;
      public string Description { get; set; } = null!;
      public string ComplaintType { get; set; } = null!;
        public string? DesiredResolution { get; set; }
     public string Status { get; set; } = null!;
        public string Priority { get; set; } = null!;
  
        // Customer info
        public string CustomerName { get; set; } = null!;
        
     // Tailor info
 public Guid TailorId { get; set; }
        public string TailorName { get; set; } = null!;
        
    // Attachments
 public List<ComplaintAttachmentViewModel> Attachments { get; set; } = new();
    
        // Admin response
        public string? AdminResponse { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        
  // Status display
        public string StatusBadgeClass => Status switch
        {
       "Open" => "badge-warning",
       "UnderReview" => "badge-info",
        "Resolved" => "badge-success",
   "Rejected" => "badge-danger",
            "Escalated" => "badge-dark",
         _ => "badge-secondary"
      };
    }
    
    public class ComplaintAttachmentViewModel
    {
 public Guid Id { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public DateTime UploadedAt { get; set; }
    }
  
  /// <summary>
/// Customer complaints list
    /// </summary>
    public class CustomerComplaintsViewModel
    {
        public List<ComplaintSummaryViewModel> Complaints { get; set; } = new();
    public int OpenCount { get; set; }
        public int ResolvedCount { get; set; }
        public int TotalCount { get; set; }
    }
 
    public class ComplaintSummaryViewModel
    {
        public Guid Id { get; set; }
        public string Subject { get; set; } = null!;
        public string ComplaintType { get; set; } = null!;
        public string Status { get; set; } = null!;
   public string OrderNumber { get; set; } = null!;
     public string TailorName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    }
    
    /// <summary>
    /// Admin: Update complaint status
  /// </summary>
    public class UpdateComplaintStatusViewModel
    {
      [Required]
        public Guid ComplaintId { get; set; }
        
        [Required]
  public string NewStatus { get; set; } = null!;

        [MaxLength(2000)]
  [Display(Name = "Admin Response")]
        public string? AdminResponse { get; set; }
    }
}
