using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TafsilkPlatform.Web.ViewModels.Corporate;

/// <summary>
/// ViewModel for editing corporate profile
/// Aligned with customer profile structure
/// </summary>
public class EditCorporateProfileViewModel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    #region Company Information

    [Required(ErrorMessage = "اسم الشركة مطلوب")]
    [StringLength(255, ErrorMessage = "اسم الشركة لا يمكن أن يتجاوز 255 حرفاً")]
    [Display(Name = "اسم الشركة")]
    public string CompanyName { get; set; } = string.Empty;

    [Required(ErrorMessage = "اسم الشخص المسؤول مطلوب")]
    [StringLength(255, ErrorMessage = "اسم الشخص المسؤول لا يمكن أن يتجاوز 255 حرفاً")]
    [Display(Name = "الشخص المسؤول")]
    public string ContactPerson { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "المجال لا يمكن أن يتجاوز 100 حرف")]
    [Display(Name = "المجال / الصناعة")]
  public string? Industry { get; set; }

    [StringLength(100, ErrorMessage = "الرقم الضريبي لا يمكن أن يتجاوز 100 حرف")]
    [Display(Name = "الرقم الضريبي")]
    public string? TaxNumber { get; set; }

    [StringLength(1000, ErrorMessage = "نبذة عن الشركة لا يمكن أن تتجاوز 1000 حرف")]
    [Display(Name = "نبذة عن الشركة")]
public string? Bio { get; set; }

    #endregion

    #region Contact Information

    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [Phone(ErrorMessage = "رقم الهاتف غير صالح")]
    [Display(Name = "رقم الهاتف")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "البريد الإلكتروني")]
    public string? Email { get; set; }

    #endregion

    #region Profile Picture

    [Display(Name = "شعار الشركة")]
    public IFormFile? ProfilePicture { get; set; }

    public byte[]? CurrentProfilePictureData { get; set; }
    public string? CurrentProfilePictureContentType { get; set; }

#endregion

    #region Statistics (Read-only)

    [Display(Name = "إجمالي الطلبات")]
    public int TotalOrders { get; set; }

    [Display(Name = "الطلبات النشطة")]
    public int ActiveOrders { get; set; }

    [Display(Name = "الطلبات المكتملة")]
    public int CompletedOrders { get; set; }

    [Display(Name = "إجمالي المبلغ المنفق")]
    public decimal TotalSpent { get; set; }

    [Display(Name = "حالة الموافقة")]
    public bool IsApproved { get; set; }

    [Display(Name = "تاريخ الموافقة")]
    public DateTime? ApprovedAt { get; set; }

    #endregion
}
