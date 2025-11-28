using TafsilkPlatform.DataAccess.Repository;
using TafsilkPlatform.Models.Models;
using TafsilkPlatform.Models.ViewModels;
using TafsilkPlatform.Web.Common;

namespace TafsilkPlatform.Web.Services;

public interface ITailorRegistrationService
{
    Task<Result<TailorProfile>> CompleteProfileAsync(CompleteTailorProfileRequest request);
    Task<Result<bool>> HasCompletedProfileAsync(Guid userId);
    Task<Result<string>> SavePortfolioImagesAsync(IEnumerable<IFormFile> images, Guid tailorId);
    Task<Result<string>> SaveIdDocumentAsync(IFormFile document, Guid tailorId);
}

public class TailorRegistrationService : ITailorRegistrationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TailorRegistrationService> _logger;
    private readonly IDateTimeService _dateTime;
    private readonly IWebHostEnvironment _environment;
    private readonly IValidationService _validationService;

    private const int MaxPortfolioImages = 10;
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
    private static readonly string[] AllowedImageTypes = { "image/jpeg", "image/jpg", "image/png", "image/webp" };

    public TailorRegistrationService(
        IUnitOfWork unitOfWork,
        ILogger<TailorRegistrationService> logger,
        IDateTimeService dateTime,
     IWebHostEnvironment environment,
      IValidationService validationService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _dateTime = dateTime;
        _environment = environment;
        _validationService = validationService;
    }

    public async Task<Result<TailorProfile>> CompleteProfileAsync(CompleteTailorProfileRequest request)
    {
        try
        {
            _logger.LogInformation("[TailorRegistration] Starting profile completion for user: {UserId}", request.UserId);

            // 1. FluentValidation - Comprehensive validation
            var validationResult = await _validationService.ValidateCompleteTailorProfileAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("[TailorRegistration] Validation failed: {Errors}", errors);
                return Result<TailorProfile>.Failure(validationResult.Errors.First().ErrorMessage);
            }

            // 2. Validate user exists and is a tailor
            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
            if (user == null || user.Role?.Name?.ToLower() != "tailor")
            {
                _logger.LogWarning("[TailorRegistration] Invalid user or role: {UserId}", request.UserId);
                return Result<TailorProfile>.Failure("Invalid account");
            }

            // 3. Check for duplicate submission (ONE-TIME submission)
            var existingProfile = await _unitOfWork.Tailors.GetByUserIdAsync(request.UserId);
            if (existingProfile != null)
            {
                _logger.LogWarning("[TailorRegistration] Duplicate submission attempt: {UserId}", request.UserId);
                return Result<TailorProfile>.Failure("Documents already submitted. Cannot submit again.");
            }

            // 4. Validate documents (additional check)
            var documentValidation = ValidateDocuments(request);
            if (!documentValidation.IsSuccess)
            {
                return Result<TailorProfile>.Failure(documentValidation.Error!);
            }

            // 5. Create profile entity
            var profile = new TailorProfile
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                FullName = request.FullName ?? user.Email,
                ShopName = request.WorkshopName,
                Address = request.Address,
                City = request.City,
                Bio = request.Description,
                ExperienceYears = request.ExperienceYears,
                IsVerified = false, // Awaiting admin approval
                CreatedAt = _dateTime.Now
            };

            _logger.LogInformation("[TailorRegistration] Creating profile entity: {ProfileId}", profile.Id);

            // 6. Save ID document to database (as binary data)
            if (request.IdDocument != null)
            {
                var idDocResult = await SaveIdDocumentToDatabaseAsync(request.IdDocument, profile);
                if (!idDocResult.IsSuccess)
                {
                    _logger.LogError("[TailorRegistration] Failed to save ID document: {Error}", idDocResult.Error);
                    return Result<TailorProfile>.Failure(idDocResult.Error!);
                }
            }

            // 7. Save profile to database
            await _unitOfWork.Tailors.AddAsync(profile);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("[TailorRegistration] Profile saved to database: {ProfileId}", profile.Id);

            // 8. Save portfolio images to file system
            if (request.PortfolioImages != null && request.PortfolioImages.Any())
            {
                var portfolioResult = await SavePortfolioImagesAsync(request.PortfolioImages, profile.Id);
                if (!portfolioResult.IsSuccess)
                {
                    _logger.LogWarning("[TailorRegistration] Portfolio upload failed: {Error}", portfolioResult.Error);
                    // Don't fail the entire operation, just log the warning
                }
            }

            // 9. Keep user INACTIVE until admin approves (important for workflow)
            user.IsActive = false;
            user.UpdatedAt = _dateTime.Now;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("[TailorRegistration] Profile created successfully. User remains inactive pending admin approval: {UserId}", request.UserId);

            return Result<TailorProfile>.Success(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TailorRegistration] Error completing profile: {UserId}", request.UserId);
            return Result<TailorProfile>.Failure("Error saving data. Please try again.");
        }
    }

    public async Task<Result<bool>> HasCompletedProfileAsync(Guid userId)
    {
        try
        {
            var profile = await _unitOfWork.Tailors.GetByUserIdAsync(userId);
            var hasProfile = profile != null;

            _logger.LogInformation("[TailorRegistration] Profile check for user {UserId}: {HasProfile}", userId, hasProfile);

            return Result<bool>.Success(hasProfile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TailorRegistration] Error checking profile: {UserId}", userId);
            return Result<bool>.Failure("An error occurred");
        }
    }

    public async Task<Result<string>> SavePortfolioImagesAsync(IEnumerable<IFormFile> images, Guid tailorId)
    {
        try
        {
            var portfolioFolder = Path.Combine(_environment.WebRootPath, "uploads", "portfolio", tailorId.ToString());
            Directory.CreateDirectory(portfolioFolder);

            int savedCount = 0;
            foreach (var image in images.Take(MaxPortfolioImages))
            {
                if (!IsValidImage(image))
                {
                    _logger.LogWarning("[TailorRegistration] Invalid image skipped: {FileName}", image.FileName);
                    continue;
                }

                var fileName = $"portfolio_{_dateTime.Now.Ticks}_{savedCount}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine(portfolioFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                {
                    await image.CopyToAsync(stream);
                }

                var relativeUrl = $"/uploads/portfolio/{tailorId}/{fileName}";
                var portfolioImage = new PortfolioImage
                {
                    PortfolioImageId = Guid.NewGuid(),
                    TailorId = tailorId,
                    ImageUrl = relativeUrl,
                    IsBeforeAfter = false,
                    UploadedAt = _dateTime.Now,
                    IsDeleted = false
                };

                await _unitOfWork.Context.Set<PortfolioImage>().AddAsync(portfolioImage);
                savedCount++;
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("[TailorRegistration] Saved {Count} portfolio images for tailor: {TailorId}", savedCount, tailorId);
            return Result<string>.Success($"{savedCount} images saved");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TailorRegistration] Error saving portfolio images: {TailorId}", tailorId);
            return Result<string>.Failure("Error saving images");
        }
    }

    public async Task<Result<string>> SaveIdDocumentAsync(IFormFile document, Guid tailorId)
    {
        try
        {
            if (!IsValidImage(document))
            {
                return Result<string>.Failure("Invalid ID image");
            }

            using var memoryStream = new MemoryStream();
            await document.CopyToAsync(memoryStream);
            var data = memoryStream.ToArray();

            _logger.LogInformation("[TailorRegistration] ID document saved for tailor: {TailorId}, Size: {Size} bytes",
        tailorId, data.Length);

            return Result<string>.Success("ID saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TailorRegistration] Error saving ID document: {TailorId}", tailorId);
            return Result<string>.Failure("Error saving ID");
        }
    }

    private Result<bool> ValidateDocuments(CompleteTailorProfileRequest request)
    {
        if (request.IdDocument == null || request.IdDocument.Length == 0)
        {
            return Result<bool>.Failure("ID image is required");
        }

        if (!IsValidImage(request.IdDocument))
        {
            return Result<bool>.Failure("ID file type not supported. Please upload JPG or PNG");
        }

        if ((request.PortfolioImages == null || !request.PortfolioImages.Any()) &&
            (request.WorkSamples == null || !request.WorkSamples.Any()))
        {
            return Result<bool>.Failure("At least 3 portfolio images are required");
        }

        var portfolioImages = request.PortfolioImages ?? request.WorkSamples ?? new List<IFormFile>();
        if (portfolioImages.Count < 3)
        {
            return Result<bool>.Failure("At least 3 portfolio images are required");
        }

        if (portfolioImages.Count > MaxPortfolioImages)
        {
            return Result<bool>.Failure($"Maximum {MaxPortfolioImages} images allowed");
        }

        return Result<bool>.Success(true);
    }

    private async Task<Result<bool>> SaveIdDocumentToDatabaseAsync(IFormFile document, TailorProfile profile)
    {
        try
        {
            using var memoryStream = new MemoryStream();
            await document.CopyToAsync(memoryStream);
            profile.ProfilePictureData = memoryStream.ToArray();
            profile.ProfilePictureContentType = document.ContentType;

            _logger.LogInformation("[TailorRegistration] ID document saved to profile (binary): {Size} bytes", profile.ProfilePictureData.Length);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TailorRegistration] Error saving ID to database");
            return Result<bool>.Failure("Error saving ID");
        }
    }

    private bool IsValidImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;

        if (file.Length > MaxFileSize)
        {
            _logger.LogWarning("[TailorRegistration] File too large: {Size} bytes", file.Length);
            return false;
        }

        if (!AllowedImageTypes.Contains(file.ContentType.ToLower()))
        {
            _logger.LogWarning("[TailorRegistration] Invalid content type: {Type}", file.ContentType);
            return false;
        }

        return true;
    }
}
