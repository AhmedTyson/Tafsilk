using TafsilkPlatform.Core.Models;

namespace TafsilkPlatform.Core.Interfaces
{
    public interface IGeoSearchService
    {
        Task<IEnumerable<TailorProfile>> FindTailorsNearbyAsync(decimal latitude, decimal longitude, double radiusKm);
        Task<double> CalculateDistanceAsync(decimal lat1, decimal lon1, decimal lat2, decimal lon2);
        Task<IEnumerable<TailorProfile>> FindTailorsInCityAsync(string city);
    }
}
