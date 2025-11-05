using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Web.Models
{
    /// <summary>
    /// Represents a portfolio image for a tailor's work showcase
    /// </summary>
    public class PortfolioImage
    {
        [Key]
        public Guid PortfolioImageId { get; set; }

        [Required]
        public Guid TailorId { get; set; }

        [StringLength(100)]
        public string? Title { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Image URL (if stored in file system or cloud)
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Image binary data (if stored in database)
        /// </summary>
        public byte[]? ImageData { get; set; }

        /// <summary>
        /// Content type (e.g., "image/jpeg", "image/png")
        /// </summary>
        [StringLength(50)]
        public string? ContentType { get; set; }

        /// <summary>
        /// Estimated price for this type of work
        /// </summary>
        public decimal? EstimatedPrice { get; set; }

        /// <summary>
        /// Whether this is a before/after comparison image
        /// </summary>
        public bool IsBeforeAfter { get; set; }

        /// <summary>
        /// Whether this is a featured/highlighted image
        /// </summary>
        public bool IsFeatured { get; set; }

        /// <summary>
        /// Display order for sorting
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// When the image was uploaded
        /// </summary>
        public DateTime UploadedAt { get; set; }

        /// <summary>
        /// When the record was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; }

        // Navigation property
        public TailorProfile? Tailor { get; set; }
    }
}
