using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// InsuranceClaimDocument - Tài liệu đính kèm claim bảo hiểm
    /// </summary>
    public class InsuranceClaimDocument : BaseEntity
    {
        public int InsuranceClaimId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string DocumentName { get; set; } = string.Empty; // Tên tài liệu
        
        [StringLength(50)]
        public string DocumentType { get; set; } = string.Empty; // Loại tài liệu: Photo, Police Report, Estimate, Invoice, Receipt
        
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty; // Đường dẫn file
        
        [StringLength(100)]
        public string? FileName { get; set; } // Tên file gốc
        
        [StringLength(20)]
        public string? FileExtension { get; set; } // Phần mở rộng file
        
        public long? FileSize { get; set; } // Kích thước file (bytes)
        
        [StringLength(500)]
        public string? Description { get; set; } // Mô tả tài liệu
        
        public DateTime UploadDate { get; set; } = DateTime.Now; // Ngày upload
        
        public DateTime? UploadedAt { get; set; } // Alias for UploadDate
        
        public int? UploadedBy { get; set; } // Người upload (Employee ID)
        
        [StringLength(100)]
        public string? UploadedByName { get; set; } // Tên người upload
        
        public bool IsRequired { get; set; } = false; // Tài liệu bắt buộc
        
        public bool IsVerified { get; set; } = false; // Đã xác minh
        
        public DateTime? VerifiedDate { get; set; } // Ngày xác minh
        
        [StringLength(100)]
        public string? VerifiedBy { get; set; } // Người xác minh
        
        [StringLength(500)]
        public string? VerificationNotes { get; set; } // Ghi chú xác minh
        
        // Navigation properties
        public virtual InsuranceClaim InsuranceClaim { get; set; } = null!;
    }
}
