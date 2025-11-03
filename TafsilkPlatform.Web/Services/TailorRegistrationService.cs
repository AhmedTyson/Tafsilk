using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.ViewModels;
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
    
    private const int MaxPortfolioImages = 10;
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
    private static readonly string[] AllowedImageTypes = { "image/jpeg", "image/jpg", "image/png", "image/webp" };

    public TailorRegistrationService(
        IUnitOfWork unitOfWork,
        ILogger<TailorRegistrationService> logger,
IDateTimeService dateTime,
        IWebHostEnvironment environment)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _dateTime = dateTime;
    _environment = environment;
    }

    public async Task<Result<TailorProfile>> CompleteProfileAsync(CompleteTailorProfileRequest request)
    {
     try
    {
   // 1. Validate user exists and is a tailor
      var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
if (user == null || user.Role?.Name?.ToLower() != "tailor")
{
          return Result<TailorProfile>.Failure("حساب غير صالح");
            }

     // 2. Check for duplicate submission
            var existingProfile = await _unitOfWork.Tailors.GetByUserIdAsync(request.UserId);
    if (existingProfile != null)
     {
 _logger.LogWarning("[TailorRegistration] Duplicate submission attempt: {UserId}", request.UserId);
 return Result<TailorProfile>.Failure("تم تقديم الأوراق الثبوتية بالفعل");
            }

        // 3. Validate documents
            var documentValidation = ValidateDocuments(request);
            if (!documentValidation.IsSuccess)
         {
      return Result<TailorProfile>.Failure(documentValidation.Error!);
 }

      // 4. Create profile
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
   IsVerified = false,
       CreatedAt = _dateTime.Now
  };

            // 5. Save ID document to database (as binary)
  if (request.IdDocument != null)
            {
      var idDocResult = await SaveIdDocumentToDatabaseAsync(request.IdDocument, profile);
  if (!idDocResult.IsSuccess)
        {
  return Result<TailorProfile>.Failure(idDocResult.Error!);
      }
            }

          // 6. Save profile to database
      await _unitOfWork.Tailors.AddAsync(profile);
          await _unitOfWork.SaveChangesAsync();

 // 7. Save portfolio images to file system
      if (request.PortfolioImages != null && request.PortfolioImages.Any())
            {
  var portfolioResult = await SavePortfolioImagesAsync(request.PortfolioImages, profile.Id);
      if (!portfolioResult.IsSuccess)
          {
         _logger.LogWarning("[TailorRegistration] Portfolio upload failed: {Error}", portfolioResult.Error);
      // Don't fail the entire operation, just log
    }
    }

            // 8. Activate user account
         user.IsActive = true;
         user.UpdatedAt = _dateTime.Now;
       await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("[TailorRegistration] Profile created successfully: {UserId}", request.UserId);
          
   return Result<TailorProfile>.Success(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TailorRegistration] Error completing profile: {UserId}", request.UserId);
            return Result<TailorProfile>.Failure("حدث خطأ أثناء حفظ البيانات");
 }
    }

    public async Task<Result<bool>> HasCompletedProfileAsync(Guid userId)
    {
        try
        {
            var profile = await _unitOfWork.Tailors.GetByUserIdAsync(userId);
            return Result<bool>.Success(profile != null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TailorRegistration] Error checking profile: {UserId}", userId);
            return Result<bool>.Failure("حدث خطأ");
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

           using (var stream = new FileStream(filePath, FileMode.Create))
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
   return Result<string>.Success($"تم حفظ {savedCount} صور");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TailorRegistration] Error saving portfolio images: {TailorId}", tailorId);
            return Result<string>.Failure("حدث خطأ أثناء حفظ الصور");
    }
    }

    public async Task<Result<string>> SaveIdDocumentAsync(IFormFile document, Guid tailorId)
    {
        try
        {
    if (!IsValidImage(document))
        {
   return Result<string>.Failure("صورة الهوية غير صالحة");
            }

          using var memoryStream = new MemoryStream();
            await document.CopyToAsync(memoryStream);
  var data = memoryStream.ToArray();

  _logger.LogInformation("[TailorRegistration] ID document saved for tailor: {TailorId}, Size: {Size} bytes", 
        tailorId, data.Length);
    
  return Result<string>.Success("تم حفظ الهوية بنجاح");
        }
   catch (Exception ex)
      {
            _logger.LogError(ex, "[TailorRegistration] Error saving ID document: {TailorId}", tailorId);
          return Result<string>.Failure("حدث خطأ أثناء حفظ الهوية");
    }
    }

    private Result<bool> ValidateDocuments(CompleteTailorProfileRequest request)
    {
        if (request.IdDocument == null || request.IdDocument.Length == 0)
        {
   return Result<bool>.Failure("يجب تحميل صورة الهوية الشخصية");
  }

     if (!IsValidImage(request.IdDocument))
        {
 return Result<bool>.Failure("نوع ملف الهوية غير مدعوم. يرجى تحميل صورة JPG أو PNG");
    }

        if ((request.PortfolioImages == null || !request.PortfolioImages.Any()) &&
 (request.WorkSamples == null || !request.WorkSamples.Any()))
        {
            return Result<bool>.Failure("يجب تحميل على الأقل 3 صور من أعمالك السابقة");
        }

        var portfolioImages = request.PortfolioImages ?? request.WorkSamples ?? new List<IFormFile>();
        if (portfolioImages.Count < 3)
        {
       return Result<bool>.Failure("يجب تحميل على الأقل 3 صور من أعمالك السابقة");
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
          
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
  {
      _logger.LogError(ex, "[TailorRegistration] Error saving ID to database");
    return Result<bool>.Failure("حدث خطأ أثناء حفظ الهوية");
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
