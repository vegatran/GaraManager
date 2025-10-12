using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// FinancialTransactionAttachment - Tài liệu đính kèm giao dịch tài chính
    /// </summary>
    public class FinancialTransactionAttachment : BaseEntity
    {
        public int FinancialTransactionId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FileName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? FileType { get; set; } // "Invoice", "Receipt", "Contract", "Other"
        
        public long FileSize { get; set; }
        
        [StringLength(100)]
        public string? MimeType { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public DateTime UploadedAt { get; set; } = DateTime.Now;
        
        [StringLength(100)]
        public string? UploadedBy { get; set; }
        
        // Navigation property
        public virtual FinancialTransaction FinancialTransaction { get; set; } = null!;
    }
}

