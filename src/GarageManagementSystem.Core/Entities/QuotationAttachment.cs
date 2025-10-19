using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Quotation Attachment - File đính kèm báo giá
    /// </summary>
    public class QuotationAttachment : BaseEntity
    {
        public int ServiceQuotationId { get; set; }
        
        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? FileType { get; set; } // MIME type
        
        public long FileSize { get; set; } // File size in bytes
        
        [StringLength(50)]
        public string AttachmentType { get; set; } = "General"; // InsuranceApproval, CustomerDocument, etc.
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(1000)]
        public string? Notes { get; set; }
        
        public bool IsInsuranceDocument { get; set; } = false; // Có phải tài liệu bảo hiểm không
        
        public DateTime UploadedDate { get; set; } = DateTime.Now;
        
        public int? UploadedById { get; set; } // Employee who uploaded
        
        // Navigation properties
        public virtual ServiceQuotation ServiceQuotation { get; set; } = null!;
        public virtual Employee? UploadedBy { get; set; }
    }
}
