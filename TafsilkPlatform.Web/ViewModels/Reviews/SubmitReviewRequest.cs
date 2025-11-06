using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Web.ViewModels.Reviews;

/// <summary>
/// ViewModel for submitting a review for a completed order
/// </summary>
public class SubmitReviewRequest
{
    [Required(ErrorMessage = "معرف الطلب مطلوب")]
    public Guid OrderId { get; set; }

    [Required(ErrorMessage = "معرف الخياط مطلوب")]
  public Guid TailorId { get; set; }

    [Required(ErrorMessage = "التقييم الإجمالي مطلوب")]
    [Range(1, 5, ErrorMessage = "التقييم يجب أن يكون بين 1 و 5")]
    [Display(Name = "التقييم الإجمالي")]
    public int Rating { get; set; }

    [StringLength(1000, ErrorMessage = "التعليق يجب أن لا يتجاوز 1000 حرف")]
    [Display(Name = "التعليق")]
    public string? Comment { get; set; }

    // Detailed ratings (optional but recommended)
    [Range(1, 5, ErrorMessage = "التقييم يجب أن يكون بين 1 و 5")]
    [Display(Name = "تقييم الجودة")]
    public int? QualityRating { get; set; }

    [Range(1, 5, ErrorMessage = "التقييم يجب أن يكون بين 1 و 5")]
    [Display(Name = "تقييم التواصل")]
    public int? CommunicationRating { get; set; }

    [Range(1, 5, ErrorMessage = "التقييم يجب أن يكون بين 1 و 5")]
    [Display(Name = "تقييم الالتزام بالمواعيد")]
    public int? TimelinessRating { get; set; }

    [Range(1, 5, ErrorMessage = "التقييم يجب أن يكون بين 1 و 5")]
    [Display(Name = "تقييم الاحترافية")]
    public int? ProfessionalismRating { get; set; }
}
