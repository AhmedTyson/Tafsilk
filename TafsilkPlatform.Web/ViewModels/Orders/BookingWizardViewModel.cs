using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Web.ViewModels.Orders
{
    /// <summary>
    /// Enhanced 6-step booking wizard aligned with customer journey workflow
    /// </summary>
    public class BookingWizardViewModel
    {
        // Step 1: Service Selection
        public Guid TailorId { get; set; }
        public string? TailorName { get; set; }
        public Guid? SelectedServiceId { get; set; }
        public string? ServiceType { get; set; }
        public decimal? ServicePrice { get; set; }
        
  // Step 2: Upload Images (Reference photos or garment images)
        [Display(Name = "Reference Images")]
  [MaxLength(10, ErrorMessage = "Maximum 10 images allowed")]
    public List<IFormFile>? ReferenceImages { get; set; }
    
        // Step 3: Enter Measurements
        [Display(Name = "Use Saved Measurements")]
        public Guid? SavedMeasurementId { get; set; }
        
        [Display(Name = "Manual Measurements")]
     public MeasurementInputViewModel? ManualMeasurements { get; set; }
        
 [Display(Name = "Save these measurements for future use")]
        public bool SaveMeasurements { get; set; }
        
        [MaxLength(100)]
 [Display(Name = "Measurement Set Name")]
        public string? MeasurementSetName { get; set; }
 
    // Step 4: Select Date & Time
        [Required(ErrorMessage = "Preferred date is required")]
        [Display(Name = "Preferred Appointment Date")]
    [DataType(DataType.Date)]
        public DateTime? PreferredDate { get; set; }
        
   [Display(Name = "Preferred Time Slot")]
        public string? PreferredTimeSlot { get; set; } // "Morning", "Afternoon", "Evening"
        
    [Required(ErrorMessage = "Due date is required")]
        [Display(Name = "Expected Completion Date")]
      [DataType(DataType.Date)]
  public DateTime? DueDate { get; set; }
        
        // Step 5: Review Price Estimate
        [Display(Name = "Base Price")]
      public decimal BasePrice { get; set; }
    
        [Display(Name = "Additional Charges")]
    public decimal AdditionalCharges { get; set; }
   
        [Display(Name = "Subtotal")]
   public decimal Subtotal => BasePrice + AdditionalCharges;
 
        [Display(Name = "VAT (15%)")]
        public decimal VAT => Subtotal * 0.15m;
     
        [Display(Name = "Total Price")]
public decimal TotalPrice => Subtotal + VAT;
        
   [Display(Name = "Deposit Required")]
        public bool RequiresDeposit { get; set; }
        
        [Display(Name = "Deposit Amount (50%)")]
 public decimal DepositAmount => RequiresDeposit ? TotalPrice * 0.5m : 0;
        
        [Display(Name = "Remaining Balance")]
        public decimal RemainingBalance => TotalPrice - DepositAmount;
    
      // Step 6: Confirm Booking
        [Required(ErrorMessage = "You must agree to the terms and conditions")]
        [Display(Name = "I agree to the terms and conditions")]
public bool AgreeToTerms { get; set; }
 
  [MaxLength(1000)]
      [Display(Name = "Additional Notes")]
        public string? AdditionalNotes { get; set; }
        
  [Display(Name = "Fulfillment Method")]
        public string FulfillmentMethod { get; set; } = "Pickup"; // "Pickup" or "Delivery"
    
        [MaxLength(500)]
        [Display(Name = "Delivery Address")]
        public string? DeliveryAddress { get; set; }
        
 // Available data for display
     public List<ServiceOptionViewModel>? AvailableServices { get; set; }
     public List<SavedMeasurementViewModel>? SavedMeasurements { get; set; }
      public List<string>? AvailableTimeSlots { get; set; }
        
        // Current step tracking
        public int CurrentStep { get; set; } = 1;
    }
    
    public class MeasurementInputViewModel
    {
  [Display(Name = "Garment Type")]
  public string? GarmentType { get; set; }
     
        [Display(Name = "Chest (cm)")]
        [Range(0, 200)]
        public decimal? Chest { get; set; }
   
        [Display(Name = "Waist (cm)")]
        [Range(0, 200)]
   public decimal? Waist { get; set; }
        
   [Display(Name = "Hips (cm)")]
        [Range(0, 200)]
        public decimal? Hips { get; set; }

        [Display(Name = "Shoulder Width (cm)")]
        [Range(0, 100)]
        public decimal? ShoulderWidth { get; set; }
      
     [Display(Name = "Sleeve Length (cm)")]
        [Range(0, 150)]
    public decimal? SleeveLength { get; set; }

        [Display(Name = "Inseam Length (cm)")]
        [Range(0, 150)]
        public decimal? InseamLength { get; set; }
        
        [Display(Name = "Neck Circumference (cm)")]
   [Range(0, 100)]
        public decimal? NeckCircumference { get; set; }
        
        [Display(Name = "Thobe Length (cm)")]
        [Range(0, 200)]
  public decimal? ThobeLength { get; set; }
        
        [Display(Name = "Abaya Length (cm)")]
        [Range(0, 200)]
        public decimal? AbayaLength { get; set; }
        
        [MaxLength(500)]
      [Display(Name = "Additional Notes")]
        public string? Notes { get; set; }
    }
    
    public class SavedMeasurementViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? GarmentType { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
