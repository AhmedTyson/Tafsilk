using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Models.Models
{
    /// <summary>
    /// Stores customer body measurements for faster rebooking
    /// Part of the loyalty and convenience features
    /// </summary>
    [Table("CustomerMeasurements")]
    public class CustomerMeasurement
    {
      [Key]
    public Guid Id { get; set; }

 [Required]
        public Guid CustomerId { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Measurement Set Name")]
        public string Name { get; set; } = null!; // e.g., "My Thobe", "Wedding Suit", "Standard Dress"

        [MaxLength(50)]
        public string? GarmentType { get; set; } // e.g., "Thobe", "Suit", "Dress", "Abaya"

    // Common measurements (stored as JSON or individual fields)
        public decimal? Chest { get; set; }
   public decimal? Waist { get; set; }
        public decimal? Hips { get; set; }
        public decimal? ShoulderWidth { get; set; }
     public decimal? SleeveLength { get; set; }
public decimal? InseamLength { get; set; }
        public decimal? OutseamLength { get; set; }
    public decimal? NeckCircumference { get; set; }
        public decimal? ArmLength { get; set; }
        public decimal? ThighCircumference { get; set; }

  // For traditional garments (Thobe, Abaya, etc.)
   public decimal? ThobeLength { get; set; }
        public decimal? AbayaLength { get; set; }

        // Additional custom measurements (JSON format)
        [MaxLength(2000)]
        public string? CustomMeasurementsJson { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsDefault { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
     public DateTime? UpdatedAt { get; set; }

// Navigation properties
  [ForeignKey("CustomerId")]
        public virtual CustomerProfile Customer { get; set; } = null!;
    }
}
