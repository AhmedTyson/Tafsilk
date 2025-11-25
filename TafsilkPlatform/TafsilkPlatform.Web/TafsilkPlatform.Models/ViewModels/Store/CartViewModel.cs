namespace TafsilkPlatform.Models.ViewModels.Store
{
    public class CartViewModel
    {
        public Guid CartId { get; set; }
        public List<CartItemViewModel> Items { get; set; } = new();
        public decimal SubTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Tax { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Total { get; set; }
        public int TotalItems { get; set; }
        public string? PromoCode { get; set; }
    }

    public class CartItemViewModel
    {
        public Guid CartItemId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageBase64 { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string? SelectedSize { get; set; }
        public string? SelectedColor { get; set; }
        public string? SpecialInstructions { get; set; }
        public int StockAvailable { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class UpdateCartItemRequest
    {
        public Guid CartItemId { get; set; }
        public int Quantity { get; set; }
    }
}
