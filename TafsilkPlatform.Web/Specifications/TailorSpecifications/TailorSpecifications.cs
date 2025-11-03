using TafsilkPlatform.Web.Models;

namespace TafsilkPlatform.Web.Specifications.TailorSpecifications;

/// <summary>
/// Specification for verified tailors with their services and portfolio
/// </summary>
public class VerifiedTailorsWithDetailsSpecification : Specification<TailorProfile>
{
    public VerifiedTailorsWithDetailsSpecification(string? city = null, int? minExperience = null)
        : base(BuildCriteria(city, minExperience))
    {
        AddInclude(t => t.User);
   AddInclude(t => t.TailorServices);
        AddInclude(t => t.PortfolioImages);
AddInclude(t => t.Reviews);
        
        ApplyOrderByDescending(t => t.AverageRating);
    }

    private static System.Linq.Expressions.Expression<Func<TailorProfile, bool>> BuildCriteria(string? city, int? minExperience)
    {
        return t => t.IsVerified && 
            (city == null || t.City == city) &&
           (minExperience == null || t.ExperienceYears >= minExperience);
    }
}

/// <summary>
/// Specification for paginated tailor search
/// </summary>
public class TailorSearchSpecification : Specification<TailorProfile>
{
    public TailorSearchSpecification(
        string? searchTerm = null,
        string? city = null,
    string? specialization = null,
  decimal? minRating = null,
        int pageNumber = 1,
        int pageSize = 20)
        : base(BuildSearchCriteria(searchTerm, city, specialization, minRating))
    {
  AddInclude(t => t.User);
 AddInclude("TailorServices");
        
        ApplyOrderByDescending(t => t.AverageRating);
        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
    }

    private static System.Linq.Expressions.Expression<Func<TailorProfile, bool>> BuildSearchCriteria(
        string? searchTerm,
    string? city,
 string? specialization,
        decimal? minRating)
    {
        return t => t.IsVerified &&
       (searchTerm == null || 
       t.ShopName.Contains(searchTerm) || 
         t.FullName!.Contains(searchTerm) ||
        t.Bio!.Contains(searchTerm)) &&
        (city == null || t.City == city) &&
           (specialization == null || t.Specialization == specialization) &&
      (minRating == null || t.AverageRating >= minRating);
    }
}

/// <summary>
/// Specification for nearby tailors
/// </summary>
public class NearbyTailorsSpecification : Specification<TailorProfile>
{
    public NearbyTailorsSpecification(decimal latitude, decimal longitude, int radiusKm = 10)
        : base(t => t.IsVerified && 
                 t.Latitude != null && 
                    t.Longitude != null)
    {
     AddInclude(t => t.User);
  ApplyOrderBy(t => t.AverageRating);
    }
}

/// <summary>
/// Specification for tailors pending verification
/// </summary>
public class PendingVerificationTailorsSpecification : Specification<TailorProfile>
{
    public PendingVerificationTailorsSpecification()
        : base(t => !t.IsVerified && t.User.IsActive == false)
    {
AddInclude(t => t.User);
    AddInclude(t => t.PortfolioImages);
        ApplyOrderBy(t => t.CreatedAt);
    }
}

/// <summary>
/// Specification for top-rated tailors
/// </summary>
public class TopRatedTailorsSpecification : Specification<TailorProfile>
{
    public TopRatedTailorsSpecification(int take = 10, string? city = null)
        : base(t => t.IsVerified && 
        t.AverageRating >= 4.0m &&
         (city == null || t.City == city))
    {
     AddInclude(t => t.User);
  AddInclude(t => t.Reviews);
   ApplyOrderByDescending(t => t.AverageRating);
        ApplyPaging(0, take);
    }
}
