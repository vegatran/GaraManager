using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    public class CustomerFeedbackAttachment : BaseEntity
    {
        public int CustomerFeedbackId { get; set; }

        [Required]
        [StringLength(200)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ContentType { get; set; }

        public long FileSize { get; set; }

        public virtual CustomerFeedback CustomerFeedback { get; set; } = null!;
    }
}

