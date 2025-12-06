using TafsilkPlatform.Models.Models;

namespace TafsilkPlatform.Models.ViewModels;

public class HomeViewModel
{
    public List<TailorProfile> FeaturedTailors { get; set; } = new();
    public int TrustedTailorsCount { get; set; }
    public int CompletedOrdersCount { get; set; }
    public double AveragePlatformRating { get; set; }
    public List<Product> TrendingProducts { get; set; } = new();
}
