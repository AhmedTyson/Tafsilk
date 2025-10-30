using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TafsilkPlatform.Web.Models
{
    public class OrderImages
    {
        [Key]
        public Guid OrderImageId { get; set; }
        [ForeignKey("order")]
        public Guid OrderId { get; set; }
        public required Order order { get; set; }
        public required string ImgUrl { get; set; }
        public required string UploadedId { get; set; }
        public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
