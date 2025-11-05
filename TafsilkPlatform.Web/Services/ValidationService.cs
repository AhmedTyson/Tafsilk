using FluentValidation;
using FluentValidation.Results;
using TafsilkPlatform.Web.ViewModels;

namespace TafsilkPlatform.Web.Services
{
    public class ValidationService : IValidationService
    {
        private readonly ILogger<ValidationService> _logger;

        public ValidationService(ILogger<ValidationService> logger)
        {
            _logger = logger;
        }

        public async Task<ValidationResult> ValidateCustomerProfileAsync(UpdateCustomerProfileRequest request)
        {
            _logger.LogInformation("[ValidationService] Validating customer profile");
            var validator = new CustomerProfileValidator();
            return await validator.ValidateAsync(request);
        }

        public async Task<ValidationResult> ValidateTailorProfileAsync(UpdateTailorProfileRequest request)
        {
            _logger.LogInformation("[ValidationService] Validating tailor profile");
            var validator = new TailorProfileValidator();
            return await validator.ValidateAsync(request);
        }

        public async Task<ValidationResult> ValidateAddressAsync(AddAddressRequest request)
        {
            _logger.LogInformation("[ValidationService] Validating address");
            var validator = new AddressValidator();
            return await validator.ValidateAsync(request);
        }

        public async Task<ValidationResult> ValidateServiceAsync(AddServiceRequest request)
        {
            _logger.LogInformation("[ValidationService] Validating service");
            var validator = new ServiceValidator();
            return await validator.ValidateAsync(request);
        }

        /// <summary>
        /// Validates complete tailor profile request during registration
        /// </summary>
        public async Task<ValidationResult> ValidateCompleteTailorProfileAsync(CompleteTailorProfileRequest request)
        {
            _logger.LogInformation("[ValidationService] Validating complete tailor profile for user: {UserId}", request.UserId);
            var validator = new CompleteTailorProfileValidator();
            return await validator.ValidateAsync(request);
        }
    }

    // ==================== VALIDATORS ====================

    /// <summary>
    /// Validator for CompleteTailorProfileRequest - used during initial tailor registration
    /// </summary>
    public class CompleteTailorProfileValidator : AbstractValidator<CompleteTailorProfileRequest>
    {
        public CompleteTailorProfileValidator()
        {
            // Workshop Name Validation
            RuleFor(x => x.WorkshopName)
              .NotEmpty().WithMessage("اسم الورشة مطلوب")
   .MinimumLength(3).WithMessage("اسم الورشة يجب أن يكون 3 أحرف على الأقل")
             .MaximumLength(100).WithMessage("اسم الورشة لا يمكن أن يتجاوز 100 حرف")
 .Matches(@"^[\u0600-\u06FFa-zA-Z0-9\s\-&.]+$").WithMessage("اسم الورشة يحتوي على أحرف غير مسموح بها");

            // Workshop Type Validation
            RuleFor(x => x.WorkshopType)
            .NotEmpty().WithMessage("نوع الورشة مطلوب")
                  .Must(type => new[] { "tailoring", "design", "embroidery", "repair", "other" }.Contains(type))
            .WithMessage("نوع الورشة غير صالح");

            // Phone Number Validation (Egyptian format)
            RuleFor(x => x.PhoneNumber)
                    .NotEmpty().WithMessage("رقم الهاتف مطلوب")
       .Matches(@"^01[0-2,5]\d{8}$").WithMessage("رقم هاتف مصري غير صحيح (مثال: 01012345678)");

            // Address Validation
            RuleFor(x => x.Address)
    .NotEmpty().WithMessage("العنوان مطلوب")
    .MinimumLength(10).WithMessage("العنوان يجب أن يكون 10 أحرف على الأقل")
      .MaximumLength(255).WithMessage("العنوان لا يمكن أن يتجاوز 255 حرف");

            // City Validation
            RuleFor(x => x.City)
          .NotEmpty().WithMessage("المدينة مطلوبة")
           .MaximumLength(50).WithMessage("المدينة لا يمكن أن تتجاوز 50 حرف");

            // Description Validation
            RuleFor(x => x.Description)
           .NotEmpty().WithMessage("وصف الورشة مطلوب")
                  .MinimumLength(20).WithMessage("الوصف يجب أن يكون 20 حرف على الأقل")
         .MaximumLength(1000).WithMessage("الوصف لا يمكن أن يتجاوز 1000 حرف");

            // Experience Years Validation
            RuleFor(x => x.ExperienceYears)
               .GreaterThanOrEqualTo(0).When(x => x.ExperienceYears.HasValue)
        .WithMessage("سنوات الخبرة لا يمكن أن تكون سالبة")
                     .LessThanOrEqualTo(60).When(x => x.ExperienceYears.HasValue)
                  .WithMessage("سنوات الخبرة لا يمكن أن تتجاوز 60 عاماً");

            // ID Document Validation
            RuleFor(x => x.IdDocument)
                  .NotNull().WithMessage("يجب تحميل صورة الهوية الشخصية")
            .Must(file => file != null && file.Length > 0)
                   .WithMessage("يجب تحميل صورة الهوية الشخصية")
           .Must(file => file == null || file.Length <= 10 * 1024 * 1024)
                     .WithMessage("حجم ملف الهوية يجب ألا يتجاوز 10 ميجابايت")
         .Must(file => file == null || IsValidImageFile(file))
            .WithMessage("نوع ملف الهوية غير مدعوم. يرجى تحميل صورة JPG أو PNG أو PDF");

            // Portfolio Images Validation
            RuleFor(x => x.PortfolioImages)
               .NotNull().WithMessage("يجب تحميل صور من معرض الأعمال")
            .Must(images => images != null && images.Count >= 3)
    .WithMessage("يجب تحميل على الأقل 3 صور من معرض الأعمال")
        .Must(images => images == null || images.Count <= 10)
          .WithMessage("يمكن تحميل 10 صور كحد أقصى")
        .Must(images => images == null || images.All(img => img.Length <= 5 * 1024 * 1024))
      .WithMessage("حجم كل صورة يجب ألا يتجاوز 5 ميجابايت")
       .Must(images => images == null || images.All(IsValidImageFile))
          .WithMessage("جميع الصور يجب أن تكون بصيغة JPG أو PNG أو WEBP");

            // Terms Agreement Validation
            RuleFor(x => x.AgreeToTerms)
           .Equal(true).WithMessage("يجب الموافقة على الشروط والأحكام");

            // UserId Validation
            RuleFor(x => x.UserId)
                        .NotEmpty().WithMessage("معرف المستخدم مطلوب");
        }

        private bool IsValidImageFile(IFormFile? file)
        {
            if (file == null) return false;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf", ".doc", ".docx" };
            var allowedContentTypes = new[] {
    "image/jpeg", "image/jpg", "image/png", "image/webp",
  "application/pdf", "application/msword",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return allowedExtensions.Contains(extension) &&
             allowedContentTypes.Contains(file.ContentType.ToLowerInvariant());
        }
    }

    public class CustomerProfileValidator : AbstractValidator<UpdateCustomerProfileRequest>
    {
        public CustomerProfileValidator()
        {
            RuleFor(x => x.FullName)
           .NotEmpty().WithMessage("الاسم الكامل مطلوب")
               .MinimumLength(3).WithMessage("الاسم يجب أن يكون 3 أحرف على الأقل")
             .MaximumLength(100).WithMessage("الاسم لا يمكن أن يتجاوز 100 حرف")
        .Matches(@"^[\u0600-\u06FFa-zA-Z\s]+$").WithMessage("الاسم يمكن أن يحتوي على أحرف عربية أو إنجليزية فقط");

            RuleFor(x => x.PhoneNumber)
      .NotEmpty().WithMessage("رقم الهاتف مطلوب")
     .Matches(@"^01[0-2,5]\d{8}$").WithMessage("رقم هاتف مصري غير صحيح (مثال: 01012345678)");

            RuleFor(x => x.Gender)
  .NotEmpty().WithMessage("الجنس مطلوب")
   .Must(x => x == "Male" || x == "Female" || x == "ذكر" || x == "أنثى")
              .WithMessage("يجب اختيار ذكر أو أنثى");

            RuleFor(x => x.City)
                 .NotEmpty().WithMessage("المدينة مطلوبة")
             .MaximumLength(50).WithMessage("المدينة لا يمكن أن تتجاوز 50 حرف");

            RuleFor(x => x.Preferences)
         .MaximumLength(500).WithMessage("التفضيلات لا يمكن أن تتجاوز 500 حرف");

            RuleFor(x => x.DateOfBirth)
           .LessThan(DateTime.Now.AddYears(-13))
      .When(x => x.DateOfBirth.HasValue)
           .WithMessage("يجب أن يكون العمر 13 عاماً على الأقل");

            RuleFor(x => x.Bio)
   .MaximumLength(500).WithMessage("النبذة لا يمكن أن تتجاوز 500 حرف");
        }
    }

    public class TailorProfileValidator : AbstractValidator<UpdateTailorProfileRequest>
    {
        public TailorProfileValidator()
        {
            RuleFor(x => x.ShopName)
           .NotEmpty().WithMessage("اسم المحل مطلوب")
                  .MinimumLength(3).WithMessage("اسم المحل يجب أن يكون 3 أحرف على الأقل")
                   .MaximumLength(100).WithMessage("اسم المحل لا يمكن أن يتجاوز 100 حرف");

            RuleFor(x => x.Bio)
            .NotEmpty().WithMessage("النبذة مطلوبة")
                 .MinimumLength(10).WithMessage("النبذة يجب أن تكون 10 أحرف على الأقل")
                         .MaximumLength(500).WithMessage("النبذة لا يمكن أن تتجاوز 500 حرف");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("رقم الهاتف مطلوب")
                .Matches(@"^01[0-2,5]\d{8}$").WithMessage("رقم هاتف مصري غير صحيح (مثال: 01012345678)");

            RuleFor(x => x.Address)
                 .NotEmpty().WithMessage("العنوان مطلوب")
                 .MinimumLength(10).WithMessage("العنوان يجب أن يكون 10 أحرف على الأقل")
                   .MaximumLength(255).WithMessage("العنوان لا يمكن أن يتجاوز 255 حرف");

            RuleFor(x => x.City)
    .NotEmpty().WithMessage("المدينة مطلوبة")
           .MaximumLength(50).WithMessage("المدينة لا يمكن أن تتجاوز 50 حرف");

            RuleFor(x => x.ExperienceYears)
       .GreaterThanOrEqualTo(0).When(x => x.ExperienceYears.HasValue)
         .WithMessage("سنوات الخبرة لا يمكن أن تكون سالبة")
        .LessThanOrEqualTo(60).When(x => x.ExperienceYears.HasValue)
                .WithMessage("سنوات الخبرة لا يمكن أن تتجاوز 60 عاماً");

            RuleFor(x => x.SkillLevel)
                .Must(x => string.IsNullOrEmpty(x) || new[] { "Beginner", "Intermediate", "Advanced", "Expert", "مبتدئ", "متوسط", "متقدم", "خبير" }.Contains(x))
        .When(x => !string.IsNullOrEmpty(x.SkillLevel))
             .WithMessage("مستوى المهارة غير صحيح");
        }
    }

    public class AddressValidator : AbstractValidator<AddAddressRequest>
    {
        public AddressValidator()
        {
            RuleFor(x => x.Label)
    .NotEmpty().WithMessage("تسمية العنوان مطلوبة")
                .MaximumLength(50).WithMessage("التسمية لا يمكن أن تتجاوز 50 حرف");

            RuleFor(x => x.StreetAddress)
         .NotEmpty().WithMessage("العنوان مطلوب")
     .MinimumLength(5).WithMessage("العنوان يجب أن يكون 5 أحرف على الأقل")
         .MaximumLength(255).WithMessage("العنوان لا يمكن أن يتجاوز 255 حرف");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("المدينة مطلوبة")
       .MaximumLength(50).WithMessage("المدينة لا يمكن أن تتجاوز 50 حرف");

            RuleFor(x => x.District)
                        .NotEmpty().WithMessage("الحي مطلوب")
                 .MaximumLength(50).WithMessage("الحي لا يمكن أن يتجاوز 50 حرف");

            RuleFor(x => x.PostalCode)
            .MaximumLength(10).WithMessage("الرمز البريدي لا يمكن أن يتجاوز 10 أحرف")
                   .Matches(@"^\d{5}$").When(x => !string.IsNullOrEmpty(x.PostalCode))
            .WithMessage("الرمز البريدي يجب أن يكون 5 أرقام");

            RuleFor(x => x.Latitude)
              .InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue)
            .WithMessage("خط العرض يجب أن يكون بين -90 و 90");

            RuleFor(x => x.Longitude)
           .InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue)
    .WithMessage("خط الطول يجب أن يكون بين -180 و 180");

            RuleFor(x => x.AdditionalNotes)
      .MaximumLength(500).WithMessage("الملاحظات لا يمكن أن تتجاوز 500 حرف");
        }
    }

    public class ServiceValidator : AbstractValidator<AddServiceRequest>
    {
        public ServiceValidator()
        {
            RuleFor(x => x.ServiceName)
            .NotEmpty().WithMessage("اسم الخدمة مطلوب")
           .MinimumLength(3).WithMessage("اسم الخدمة يجب أن يكون 3 أحرف على الأقل")
                .MaximumLength(100).WithMessage("اسم الخدمة لا يمكن أن يتجاوز 100 حرف");

            RuleFor(x => x.Description)
     .NotEmpty().WithMessage("الوصف مطلوب")
    .MinimumLength(10).WithMessage("الوصف يجب أن يكون 10 أحرف على الأقل")
     .MaximumLength(500).WithMessage("الوصف لا يمكن أن يتجاوز 500 حرف");

            RuleFor(x => x.BasePrice)
               .GreaterThan(0).WithMessage("السعر يجب أن يكون أكبر من صفر")
                 .LessThanOrEqualTo(100000).WithMessage("السعر لا يمكن أن يتجاوز 100,000 جنيه");

            RuleFor(x => x.EstimatedDuration)
       .GreaterThan(0).WithMessage("المدة يجب أن تكون يوم واحد على الأقل")
       .LessThanOrEqualTo(365).WithMessage("المدة لا يمكن أن تتجاوز 365 يوم");

            RuleFor(x => x.ServiceType)
      .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.ServiceType))
        .WithMessage("نوع الخدمة لا يمكن أن يتجاوز 50 حرف");
        }
    }
}
