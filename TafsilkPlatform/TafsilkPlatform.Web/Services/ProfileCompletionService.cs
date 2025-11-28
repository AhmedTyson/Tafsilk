using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.DataAccess.Data;

namespace TafsilkPlatform.Web.Services;

/// <summary>
/// Service for calculating profile completion percentage
/// </summary>
public interface IProfileCompletionService
{
    Task<ProfileCompletionResult> GetCustomerCompletionAsync(Guid userId);
    Task<ProfileCompletionResult> GetTailorCompletionAsync(Guid userId);
    // Task<ProfileCompletionResult> GetCorporateCompletionAsync(Guid userId);
}

/// <summary>
/// Result object for profile completion analysis
/// </summary>
public class ProfileCompletionResult
{
    public int CompletionPercentage { get; set; }
    public List<ProfileCompletionItem> Items { get; set; } = new();
    public List<string> MissingFields { get; set; } = new();
    public bool IsComplete => CompletionPercentage >= 100;
}

/// <summary>
/// Individual completion item
/// </summary>
public class ProfileCompletionItem
{
    public string Label { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int Weight { get; set; }
    public string? ActionUrl { get; set; }
    public string? Icon { get; set; }
}

/// <summary>
/// Implementation of profile completion service
/// </summary>
public class ProfileCompletionService : IProfileCompletionService
{
    private readonly ApplicationDbContext _db;

    public ProfileCompletionService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ProfileCompletionResult> GetCustomerCompletionAsync(Guid userId)
    {
        var customer = await _db.CustomerProfiles
      .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        var user = await _db.Users
            .Include(u => u.UserAddresses)
      .FirstOrDefaultAsync(u => u.Id == userId);

        if (customer == null || user == null)
        {
            return new ProfileCompletionResult();
        }

        var items = new List<ProfileCompletionItem>
        {
       new ProfileCompletionItem
    {
           Label = "Full Name",
          IsCompleted = !string.IsNullOrWhiteSpace(customer.FullName),
     Weight = 20,
       Icon = "fa-user"
   },
      new ProfileCompletionItem
            {
        Label = "Phone Number",
    IsCompleted = !string.IsNullOrWhiteSpace(user.PhoneNumber),
      Weight = 15,
                Icon = "fa-phone"
       },
    new ProfileCompletionItem
          {
         Label = "Email Verified",
         IsCompleted = user.EmailVerified,
         Weight = 15,
 ActionUrl = !user.EmailVerified ? "/Account/ResendVerificationEmail" : null,
  Icon = "fa-envelope-open-text"
  },
     // Profile picture item removed
     // new ProfileCompletionItem
     // {
     //     Label = "Profile Picture",
     //     IsCompleted = customer.ProfilePictureData != null && customer.ProfilePictureData.Length > 0,
     //     Weight = 10,
     //     ActionUrl = "/Profiles/CustomerProfile",
     //     Icon = "fa-camera"
     // },
  new ProfileCompletionItem
        {
 Label = "Delivery Address",
     IsCompleted = user.UserAddresses.Any(),
      Weight = 20,
     ActionUrl = "/Profiles/ManageAddresses",
            Icon = "fa-map-marker-alt"
     },
     new ProfileCompletionItem
    {
         Label = "Date of Birth",
       IsCompleted = customer.DateOfBirth.HasValue,
         Weight = 10,
     ActionUrl = "/Profiles/CustomerProfile",
  Icon = "fa-birthday-cake"
},
new ProfileCompletionItem
            {
   Label = "Gender",
           IsCompleted = !string.IsNullOrWhiteSpace(customer.Gender),
      Weight = 10,
             ActionUrl = "/Profiles/CustomerProfile",
         Icon = "fa-venus-mars"
       }
        };

        var completedWeight = items.Where(i => i.IsCompleted).Sum(i => i.Weight);
        var totalWeight = items.Sum(i => i.Weight);
        var percentage = (int)((double)completedWeight / totalWeight * 100);

        var missingFields = items.Where(i => !i.IsCompleted).Select(i => i.Label).ToList();

        return new ProfileCompletionResult
        {
            CompletionPercentage = percentage,
            Items = items,
            MissingFields = missingFields
        };
    }

    public async Task<ProfileCompletionResult> GetTailorCompletionAsync(Guid userId)
    {
        var tailor = await _db.TailorProfiles
            .Include(t => t.User)
            .Include(t => t.TailorServices)
   .Include(t => t.PortfolioImages)
        .FirstOrDefaultAsync(t => t.UserId == userId);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (tailor == null || user == null)
        {
            return new ProfileCompletionResult();
        }

        var items = new List<ProfileCompletionItem>
        {
      new ProfileCompletionItem
     {
     Label = "Full Name",
         IsCompleted = !string.IsNullOrWhiteSpace(tailor.FullName),
      Weight = 10,
                Icon = "fa-user"
     },
            new ProfileCompletionItem
            {
    Label = "Workshop Name",
                IsCompleted = !string.IsNullOrWhiteSpace(tailor.ShopName),
          Weight = 10,
 ActionUrl = "/Profiles/EditTailorProfile",
             Icon = "fa-store"
   },
            new ProfileCompletionItem
        {
                Label = "Phone Number",
                IsCompleted = !string.IsNullOrWhiteSpace(user.PhoneNumber),
      Weight = 10,
   ActionUrl = "/Profiles/EditTailorProfile",
    Icon = "fa-phone"
     },
    new ProfileCompletionItem
            {
    Label = "Email Verified",
        IsCompleted = user.EmailVerified,
    Weight = 10,
        ActionUrl = !user.EmailVerified ? "/Account/ResendVerificationEmail" : null,
           Icon = "fa-envelope-open-text"
    },
            new ProfileCompletionItem
     {
     Label = "Address and City",
     IsCompleted = !string.IsNullOrWhiteSpace(tailor.Address) && !string.IsNullOrWhiteSpace(tailor.City),
                Weight = 10,
    ActionUrl = "/Profiles/EditTailorProfile",
         Icon = "fa-map-marker-alt"
            },
        new ProfileCompletionItem
          {
          Label = "Workshop Description",
         IsCompleted = !string.IsNullOrWhiteSpace(tailor.ShopDescription),
    Weight = 5,
      ActionUrl = "/Profiles/EditTailorProfile",
                Icon = "fa-align-right"
       },
            new ProfileCompletionItem
        {
   Label = "Bio",
          IsCompleted = !string.IsNullOrWhiteSpace(tailor.Bio),
           Weight = 5,
                ActionUrl = "/Profiles/EditTailorProfile",
         Icon = "fa-info-circle"
 },
            new ProfileCompletionItem
    {
            Label = "Profile Picture",
       IsCompleted = tailor.ProfilePictureData != null && tailor.ProfilePictureData.Length > 0,
      Weight = 5,
     ActionUrl = "/Profiles/EditTailorProfile",
   Icon = "fa-camera"
 },
   new ProfileCompletionItem
   {
Label = "Services Offered",
            IsCompleted = tailor.TailorServices.Any(s => !s.IsDeleted),
                Weight = 20,
                ActionUrl = "/TailorManagement/ManageServices",
  Icon = "fa-briefcase"
     },
         new ProfileCompletionItem
      {
   Label = "Portfolio",
             IsCompleted = tailor.PortfolioImages.Count(p => !p.IsDeleted) >= 3,
    Weight = 15,
         ActionUrl = "/TailorManagement/ManagePortfolio",
     Icon = "fa-images"
            }
        };

        var completedWeight = items.Where(i => i.IsCompleted).Sum(i => i.Weight);
        var totalWeight = items.Sum(i => i.Weight);
        var percentage = (int)((double)completedWeight / totalWeight * 100);

        var missingFields = items.Where(i => !i.IsCompleted).Select(i => i.Label).ToList();

        return new ProfileCompletionResult
        {
            CompletionPercentage = percentage,
            Items = items,
            MissingFields = missingFields
        };
    }

    // REMOVED: GetCorporateCompletionAsync - Corporate feature removed
    public async Task<ProfileCompletionResult> GetCorporateCompletionAsync(Guid userId)
    {
        // Corporate feature has been removed
        // Return empty result
        await Task.CompletedTask;
        return new ProfileCompletionResult
        {
            CompletionPercentage = 0,
            Items = new List<ProfileCompletionItem>(),
            MissingFields = new List<string>()
        };
    }
}
