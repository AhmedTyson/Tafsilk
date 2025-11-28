using FluentValidation;
using FluentValidation.Results;
using TafsilkPlatform.Models.ViewModels;

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
              .NotEmpty().WithMessage("Workshop name is required")
   .MinimumLength(3).WithMessage("Workshop name must be at least 3 characters")
             .MaximumLength(100).WithMessage("Workshop name cannot exceed 100 characters")
 .Matches(@"^[\u0600-\u06FFa-zA-Z0-9\s\-&.]+$").WithMessage("Workshop name contains invalid characters");

            // Workshop Type Validation
            RuleFor(x => x.WorkshopType)
            .NotEmpty().WithMessage("Workshop type is required")
                  .Must(type => new[] { "tailoring", "design", "embroidery", "repair", "other" }.Contains(type))
            .WithMessage("Invalid workshop type");

            // Phone Number Validation (Egyptian format)
            RuleFor(x => x.PhoneNumber)
                    .NotEmpty().WithMessage("Phone number is required")
       .Matches(@"^01[0-2,5]\d{8}$").WithMessage("Invalid Egyptian phone number (e.g., 01012345678)");

            // Address Validation
            RuleFor(x => x.Address)
    .NotEmpty().WithMessage("Address is required")
    .MinimumLength(10).WithMessage("Address must be at least 10 characters")
      .MaximumLength(255).WithMessage("Address cannot exceed 255 characters");

            // City Validation
            RuleFor(x => x.City)
          .NotEmpty().WithMessage("City is required")
           .MaximumLength(50).WithMessage("City cannot exceed 50 characters");

            // Description Validation
            RuleFor(x => x.Description)
           .NotEmpty().WithMessage("Workshop description is required")
                  .MinimumLength(20).WithMessage("Description must be at least 20 characters")
         .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

            // Experience Years Validation
            RuleFor(x => x.ExperienceYears)
               .GreaterThanOrEqualTo(0).When(x => x.ExperienceYears.HasValue)
        .WithMessage("Experience years cannot be negative")
                     .LessThanOrEqualTo(60).When(x => x.ExperienceYears.HasValue)
                  .WithMessage("Experience years cannot exceed 60 years");

            // ID Document Validation
            RuleFor(x => x.IdDocument)
                  .NotNull().WithMessage("ID document is required")
            .Must(file => file != null && file.Length > 0)
                   .WithMessage("ID document is required")
           .Must(file => file == null || file.Length <= 10 * 1024 * 1024)
                     .WithMessage("ID file size must not exceed 10 MB")
         .Must(file => file == null || IsValidImageFile(file))
            .WithMessage("ID file type not supported. Please upload JPG, PNG, or PDF");

            // Portfolio Images Validation
            RuleFor(x => x.PortfolioImages)
               .NotNull().WithMessage("Portfolio images are required")
            .Must(images => images != null && images.Count >= 3)
    .WithMessage("At least 3 portfolio images are required")
        .Must(images => images == null || images.Count <= 10)
          .WithMessage("Maximum 10 images allowed")
        .Must(images => images == null || images.All(img => img.Length <= 5 * 1024 * 1024))
      .WithMessage("Each image size must not exceed 5 MB")
       .Must(images => images == null || images.All(IsValidImageFile))
          .WithMessage("All images must be in JPG, PNG, or WEBP format");

            // Terms Agreement Validation
            RuleFor(x => x.AgreeToTerms)
           .Equal(true).WithMessage("You must agree to the terms and conditions");

            // UserId Validation
            RuleFor(x => x.UserId)
                        .NotEmpty().WithMessage("User ID is required");
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
           .NotEmpty().WithMessage("Full name is required")
               .MinimumLength(3).WithMessage("Name must be at least 3 characters")
             .MaximumLength(100).WithMessage("Name cannot exceed 100 characters")
        .Matches(@"^[\u0600-\u06FFa-zA-Z\s]+$").WithMessage("Name can only contain Arabic or English characters");

            RuleFor(x => x.Preferences)
         .MaximumLength(500).WithMessage("Preferences cannot exceed 500 characters");

            RuleFor(x => x.DateOfBirth)
           .LessThan(DateTime.Now.AddYears(-13))
      .When(x => x.DateOfBirth.HasValue)
           .WithMessage("Must be at least 13 years old");

            RuleFor(x => x.Bio)
   .MaximumLength(500).WithMessage("Bio cannot exceed 500 characters");
        }
    }

    public class TailorProfileValidator : AbstractValidator<UpdateTailorProfileRequest>
    {
        public TailorProfileValidator()
        {
            RuleFor(x => x.ShopName)
           .NotEmpty().WithMessage("Shop name is required")
                  .MinimumLength(3).WithMessage("Shop name must be at least 3 characters")
                   .MaximumLength(100).WithMessage("Shop name cannot exceed 100 characters");

            RuleFor(x => x.Bio)
            .NotEmpty().WithMessage("Bio is required")
                 .MinimumLength(10).WithMessage("Bio must be at least 10 characters")
                         .MaximumLength(500).WithMessage("Bio cannot exceed 500 characters");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^01[0-2,5]\d{8}$").WithMessage("Invalid Egyptian phone number (e.g., 01012345678)");

            RuleFor(x => x.Address)
                 .NotEmpty().WithMessage("Address is required")
                 .MinimumLength(10).WithMessage("Address must be at least 10 characters")
                   .MaximumLength(255).WithMessage("Address cannot exceed 255 characters");

            RuleFor(x => x.City)
    .NotEmpty().WithMessage("City is required")
           .MaximumLength(50).WithMessage("City cannot exceed 50 characters");

            RuleFor(x => x.ExperienceYears)
       .GreaterThanOrEqualTo(0).When(x => x.ExperienceYears.HasValue)
         .WithMessage("Experience years cannot be negative")
        .LessThanOrEqualTo(60).When(x => x.ExperienceYears.HasValue)
                .WithMessage("Experience years cannot exceed 60 years");

            RuleFor(x => x.SkillLevel)
                .Must(x => string.IsNullOrEmpty(x) || new[] { "Beginner", "Intermediate", "Advanced", "Expert" }.Contains(x))
        .When(x => !string.IsNullOrEmpty(x.SkillLevel))
             .WithMessage("Invalid skill level");
        }
    }

    public class AddressValidator : AbstractValidator<AddAddressRequest>
    {
        public AddressValidator()
        {
            RuleFor(x => x.Label)
    .NotEmpty().WithMessage("Address label is required")
                .MaximumLength(50).WithMessage("Label cannot exceed 50 characters");

            RuleFor(x => x.StreetAddress)
         .NotEmpty().WithMessage("Address is required")
     .MinimumLength(5).WithMessage("Address must be at least 5 characters")
         .MaximumLength(255).WithMessage("Address cannot exceed 255 characters");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required")
       .MaximumLength(50).WithMessage("City cannot exceed 50 characters");

            RuleFor(x => x.District)
                        .NotEmpty().WithMessage("District is required")
                 .MaximumLength(50).WithMessage("District cannot exceed 50 characters");

            RuleFor(x => x.PostalCode)
            .MaximumLength(10).WithMessage("Postal code cannot exceed 10 characters")
                   .Matches(@"^\d{5}$").When(x => !string.IsNullOrEmpty(x.PostalCode))
            .WithMessage("Postal code must be 5 digits");

            RuleFor(x => x.Latitude)
              .InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue)
            .WithMessage("Latitude must be between -90 and 90");

            RuleFor(x => x.Longitude)
           .InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue)
    .WithMessage("Longitude must be between -180 and 180");

            RuleFor(x => x.AdditionalNotes)
      .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters");
        }
    }

    public class ServiceValidator : AbstractValidator<AddServiceRequest>
    {
        public ServiceValidator()
        {
            RuleFor(x => x.ServiceName)
            .NotEmpty().WithMessage("Service name is required")
           .MinimumLength(3).WithMessage("Service name must be at least 3 characters")
                .MaximumLength(100).WithMessage("Service name cannot exceed 100 characters");

            RuleFor(x => x.Description)
     .NotEmpty().WithMessage("Description is required")
    .MinimumLength(10).WithMessage("Description must be at least 10 characters")
     .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.BasePrice)
               .GreaterThan(0).WithMessage("Price must be greater than zero")
                 .LessThanOrEqualTo(100000).WithMessage("Price cannot exceed 100,000 EGP");

            RuleFor(x => x.EstimatedDuration)
       .GreaterThan(0).WithMessage("Duration must be at least 1 day")
       .LessThanOrEqualTo(365).WithMessage("Duration cannot exceed 365 days");

            RuleFor(x => x.ServiceType)
      .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.ServiceType))
        .WithMessage("Service type cannot exceed 50 characters");
        }
    }
}
