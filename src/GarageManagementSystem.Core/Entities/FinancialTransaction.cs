using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// FinancialTransaction - Giao dịch tài chính (Thu/Chi)
    /// </summary>
    public class FinancialTransaction : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string TransactionNumber { get; set; } = string.Empty; // "FIN-2024-001"
        
        [Required]
        [StringLength(20)]
        public string TransactionType { get; set; } = string.Empty; // "Income", "Expense", "Transfer"
        
        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // "Service Revenue", "Parts Sale", "Labor Cost", "Parts Purchase"
        
        [StringLength(100)]
        public string? SubCategory { get; set; } // "Oil Change", "Brake Repair", "Salary", "Rent"
        
        [Required]
        public decimal Amount { get; set; } // Số tiền
        
        [StringLength(3)]
        public string Currency { get; set; } = "VND"; // Đơn vị tiền tệ
        
        [Required]
        public DateTime TransactionDate { get; set; } = DateTime.Now; // Ngày giao dịch
        
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty; // "Cash", "Bank Transfer", "Credit Card", "Check"
        
        [StringLength(100)]
        public string? ReferenceNumber { get; set; } // Số tham chiếu (hóa đơn, phiếu thu...)
        
        [StringLength(500)]
        public string? Description { get; set; } // Mô tả giao dịch
        
        [StringLength(100)]
        public string? RelatedEntity { get; set; } // "ServiceOrder", "PaymentTransaction", "StockTransaction"
        
        public int? RelatedEntityId { get; set; } // ID của entity liên quan
        
        // Thông tin người thực hiện
        public int? EmployeeId { get; set; } // Nhân viên thực hiện giao dịch
        
        [StringLength(100)]
        public string? ApprovedBy { get; set; } // Người phê duyệt
        
        public DateTime? ApprovedDate { get; set; } // Ngày phê duyệt
        
        [StringLength(500)]
        public string? Notes { get; set; } // Ghi chú
        
        public bool IsApproved { get; set; } = false; // Đã được phê duyệt
        
        public bool IsReconciled { get; set; } = false; // Đã đối soát
        
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Completed, Cancelled
        
        // Navigation properties
        public virtual Employee? Employee { get; set; }
        public virtual ICollection<FinancialTransactionAttachment> Attachments { get; set; } = new List<FinancialTransactionAttachment>();
    }
}
