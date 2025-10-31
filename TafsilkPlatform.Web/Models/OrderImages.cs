using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TafsilkPlatform.Web.Models
{
    public class OrderImages
    {
        [Key]
        public Guid OrderImageId { get; set; }

        public Guid OrderId { get; set; }

        // use PascalCase navigation name so EF conventions map correctly
        public required Order Order { get; set; }

        public required string ImgUrl { get; set; }
        public required string UploadedId { get; set; }
        public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
