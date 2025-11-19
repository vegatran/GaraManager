namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// ✅ 4.3.2.3: DTO cho Accounts Payable (Công nợ Phải Trả)
    /// </summary>
    public class AccountsPayableDto
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Loại: "PurchaseOrder"
        /// </summary>
        public string Type { get; set; } = string.Empty;
        
        /// <summary>
        /// ID của PurchaseOrder
        /// </summary>
        public int ReferenceId { get; set; }
        
        /// <summary>
        /// Số đơn hàng (OrderNumber)
        /// </summary>
        public string ReferenceNumber { get; set; } = string.Empty;
        
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string? SupplierPhone { get; set; }
        public string? SupplierEmail { get; set; }
        
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        
        public string PaymentStatus { get; set; } = string.Empty; // "Unpaid", "Partial"
        
        public DateTime? OrderDate { get; set; } // Ngày đặt hàng
        public DateTime? ReceivedDate { get; set; } // Ngày nhận hàng
        public DateTime? DueDate { get; set; } // Ngày đến hạn thanh toán (calculated from PaymentTerms)
        
        /// <summary>
        /// Số ngày quá hạn (Overdue Days)
        /// </summary>
        public int OverdueDays { get; set; }
        
        public string? PaymentTerms { get; set; } // "Net 30", "Net 60", etc.
        
        /// <summary>
        /// ✅ 4.3.2.3: Số ngày credit trực tiếp (nếu có). Ưu tiên dùng để tính DueDate và OverdueDays
        /// </summary>
        public int? CreditDays { get; set; }
        
        public DateTime? LastPaymentDate { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// ✅ 4.3.2.3: DTO cho Accounts Payable Summary
    /// </summary>
    public class AccountsPayableSummaryDto
    {
        public decimal TotalPayable { get; set; }
        public decimal OverduePayable { get; set; }
        public decimal Overdue30Days { get; set; }
        public decimal Overdue60Days { get; set; }
        public decimal Overdue90Days { get; set; }
        public int TotalCount { get; set; }
        public int OverdueCount { get; set; }
        public List<SupplierPayableDto> BySupplier { get; set; } = new();
    }

    /// <summary>
    /// ✅ 4.3.2.3: DTO cho công nợ theo nhà cung cấp
    /// </summary>
    public class SupplierPayableDto
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string? SupplierPhone { get; set; }
        public decimal TotalPayable { get; set; }
        public int PurchaseOrderCount { get; set; }
        public int OverdueCount { get; set; }
    }
}

