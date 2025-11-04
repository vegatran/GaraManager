using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Additional Issue Photo - Ảnh chụp phát sinh trong quá trình sửa chữa
    /// </summary>
    public class AdditionalIssuePhoto : BaseEntity
    {
        [Required]
        public int AdditionalIssueId { get; set; }

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [StringLength(200)]
        public string? FileName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public int? DisplayOrder { get; set; }

        // Navigation properties
        public virtual AdditionalIssue AdditionalIssue { get; set; } = null!;
    }
}

