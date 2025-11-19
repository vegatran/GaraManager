namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// ✅ 4.3.1.9: DTO cho Financial Transaction Attachment
    /// </summary>
    public class FinancialTransactionAttachmentDto
    {
        public int Id { get; set; }
        public int FinancialTransactionId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? FileType { get; set; } // "Invoice", "Receipt", "Contract", "Other"
        public long FileSize { get; set; }
        public string? MimeType { get; set; }
        public string? Description { get; set; }
        public DateTime UploadedAt { get; set; }
        public string? UploadedBy { get; set; }
    }

    /// <summary>
    /// ✅ 4.3.1.9: DTO để tạo Financial Transaction Attachment mới
    /// </summary>
    public class CreateFinancialTransactionAttachmentDto
    {
        public int FinancialTransactionId { get; set; }
        public string? FileType { get; set; } // "Invoice", "Receipt", "Contract", "Other"
        public string? Description { get; set; }
    }
}

