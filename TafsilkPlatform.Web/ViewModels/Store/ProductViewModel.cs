using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Web.ViewModels.Store
{
    public class ProductViewModel
    {
 public Guid ProductId { get; set; }
        public required string Name { get; set; }
   public required string Description { get; set; }
     public decimal Price { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public string? Category { get; set; }
        public string? SubCategory { get; set; }
      public string? Size { get; set; }
        public string? Color { get; set; }
        public string? Material { get; set; }
    public string? Brand { get; set; }
   public int StockQuantity { get; set; }
  public bool IsAvailable { get; set; }
        public bool IsFeatured { get; set; }
        public double AverageRating { get; set; }
 public int ReviewCount { get; set; }
        public string? PrimaryImageBase64 { get; set; }
      public List<string> AdditionalImages { get; set; } = new();
        public string? TailorName { get; set; }
        public Guid? TailorId { get; set; }
    }

    public class ProductListViewModel
    {
        public List<ProductViewModel> Products { get; set; } = new();
      public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? Category { get; set; }
        public string? SearchQuery { get; set; }
 public string? SortBy { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => PageNumber > 1;
 public bool HasNext => PageNumber < TotalPages;
    }

    public class AddToCartRequest
    {
        [Required]
    public Guid ProductId { get; set; }

        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; } = 1;

      public string? SelectedSize { get; set; }
        public string? SelectedColor { get; set; }
        public string? SpecialInstructions { get; set; }
    }
}
