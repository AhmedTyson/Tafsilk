using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TafsilkPlatform.Web.ViewModels.Tailor;

/// <summary>
/// ViewModel for editing tailor profile
/// </summary>
public class EditTailorProfileViewModel
{
    public Guid TailorId { get; set; }
    public Guid UserId { get; set; }

    // Personal Information
    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    [StringLength(100, ErrorMessage = "الاسم الكامل لا يمكن أن يتجاوز 100 حرف")]
    [Display(Name = "الاسم الكامل")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [Phone(ErrorMessage = "رقم الهاتف غير صالح")]
    [Display(Name = "رقم الهاتف")]
    public string PhoneNumber { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صالح")]
    [Display(Name = "البريد الإلكتروني")]
    public string? Email { get; set; }

    // Shop Details
    [Required(ErrorMessage = "اسم الورشة مطلوب")]
    [StringLength(100, ErrorMessage = "اسم الورشة لا يمكن أن يتجاوز 100 حرف")]
  [Display(Name = "اسم الورشة")]
    public string ShopName { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "الوصف لا يمكن أن يتجاوز 500 حرف")]
    [Display(Name = "وصف الورشة")]
    public string? ShopDescription { get; set; }

    [StringLength(200, ErrorMessage = "التخصص لا يمكن أن يتجاوز 200 حرف")]
    [Display(Name = "التخصص")]
    public string? Specialization { get; set; }

    [Range(0, 100, ErrorMessage = "سنوات الخبرة يجب أن تكون بين 0 و 100")]
    [Display(Name = "سنوات الخبرة")]
    public int? YearsOfExperience { get; set; }

    // Location
    [StringLength(100, ErrorMessage = "المدينة لا يمكن أن تتجاوز 100 حرف")]
  [Display(Name = "المدينة")]
    public string? City { get; set; }

    [StringLength(100, ErrorMessage = "المنطقة لا يمكن أن تتجاوز 100 حرف")]
    [Display(Name = "المنطقة")]
    public string? District { get; set; }

    [StringLength(500, ErrorMessage = "العنوان لا يمكن أن يتجاوز 500 حرف")]
    [Display(Name = "العنوان التفصيلي")]
    public string? Address { get; set; }

    [Display(Name = "خط الطول")]
 public decimal? Longitude { get; set; }

    [Display(Name = "خط العرض")]
    public decimal? Latitude { get; set; }

    // Bio
    [StringLength(1000, ErrorMessage = "النبذة التعريفية لا يمكن أن تتجاوز 1000 حرف")]
    [Display(Name = "النبذة التعريفية")]
    public string? Bio { get; set; }

    // Profile Picture
    [Display(Name = "صورة الملف الشخصي")]
public IFormFile? ProfilePicture { get; set; }

    public string? CurrentProfilePictureUrl { get; set; }
    public byte[]? CurrentProfilePictureData { get; set; }
    public string? CurrentProfilePictureContentType { get; set; }

    // Business Hours
    [Display(Name = "ساعات العمل")]
  public string? BusinessHours { get; set; }

    // Social Media
    [Url(ErrorMessage = "رابط فيسبوك غير صالح")]
    [Display(Name = "رابط فيسبوك")]
    public string? FacebookUrl { get; set; }

    [Url(ErrorMessage = "رابط إنستغرام غير صالح")]
    [Display(Name = "رابط إنستغرام")]
    public string? InstagramUrl { get; set; }

    [Url(ErrorMessage = "رابط تويتر غير صالح")]
    [Display(Name = "رابط تويتر")]
    public string? TwitterUrl { get; set; }

    [Url(ErrorMessage = "رابط الموقع الإلكتروني غير صالح")]
    [Display(Name = "الموقع الإلكتروني")]
    public string? WebsiteUrl { get; set; }

    // Statistics (Read-only)
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int PortfolioCount { get; set; }
    public int ServiceCount { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
}

/// <summary>
/// Available cities for selection
/// </summary>
public static class EgyptCities
{
    public static List<string> GetAll()
    {
   return new List<string>
      {
            "القاهرة",
            "الجيزة",
            "الإسكندرية",
            "الشرقية",
         "الدقهلية",
            "القليوبية",
            "البحيرة",
            "المنوفية",
            "الغربية",
    "كفر الشيخ",
        "دمياط",
            "بورسعيد",
    "الإسماعيلية",
    "السويس",
            "شمال سيناء",
            "جنوب سيناء",
  "البحر الأحمر",
        "الفيوم",
       "بني سويف",
            "المنيا",
       "أسيوط",
    "سوهاج",
     "قنا",
            "الأقصر",
      "أسوان",
            "الوادي الجديد",
     "مطروح"
        };
    }
}

/// <summary>
/// Specialization options
/// </summary>
public static class TailorSpecializations
{
    public static List<string> GetAll()
    {
        return new List<string>
        {
            "بدل رجالية",
  "فساتين سهرة",
            "فساتين زفاف",
            "ملابس أطفال",
         "تعديلات ملابس",
            "ملابس تقليدية",
       "ملابس رياضية",
            "أزياء خاصة",
            "جميع الأنواع"
      };
  }
}
