using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Web.Models
{
    public class OrderImages
    {
        [Key]
        public Guid OrderImageId { get; set; }

        [ForeignKey("Order")]
        public Guid OrderId { get; set; }
        public required Order Order { get; set; }

        // Binary image storage (for new uploads)
        public byte[]? ImageData { get; set; }
        public string? ContentType { get; set; }

        // URL storage (for legacy/external storage)
        public required string ImgUrl { get; set; }
        public required string UploadedId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
