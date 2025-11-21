using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Web.ViewModels.Store
{
    public class CheckoutViewModel
    {
        public CartViewModel Cart { get; set; } = new();
        public CheckoutAddressViewModel ShippingAddress { get; set; } = new();
        public CheckoutAddressViewModel? BillingAddress { get; set; }
        public bool UseSameAddressForBilling { get; set; } = true;
        public string PaymentMethod { get; set; } = "CashOnDelivery"; // ✅ CHANGED: Default to cash
        public string? DeliveryNotes { get; set; }
    }

    public class CheckoutAddressViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Street address is required")]
        public string? Street { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string? City { get; set; }

        public string? District { get; set; }
        public string? PostalCode { get; set; }
        public string? AdditionalInfo { get; set; }
    }

    public class ProcessPaymentRequest
    {
        [Required]
        public string PaymentMethod { get; set; } = "CashOnDelivery"; // ✅ CHANGED: Default to cash only

        [Required(ErrorMessage = "Shipping address is required")]
        public CheckoutAddressViewModel ShippingAddress { get; set; } = new(); // ✅ FIXED: Not nullable, initialized
        
        public CheckoutAddressViewModel? BillingAddress { get; set; }
        public string? DeliveryNotes { get; set; }

        // Payment gateway data (not used for cash)
        public string? PaymentToken { get; set; }
        public string? CardLastFourDigits { get; set; }
    }
}
