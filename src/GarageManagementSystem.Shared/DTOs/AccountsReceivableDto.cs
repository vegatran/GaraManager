namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// ✅ 4.3.2.1: DTO cho Accounts Receivable (Công nợ Phải Thu)
    /// </summary>
    public class AccountsReceivableDto
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Loại: "Invoice" hoặc "ServiceOrder"
        /// </summary>
        public string Type { get; set; } = string.Empty;
        
        /// <summary>
        /// ID của Invoice hoặc ServiceOrder
        /// </summary>
        public int ReferenceId { get; set; }
        
        /// <summary>
        /// Số hóa đơn hoặc Order Number
        /// </summary>
        public string ReferenceNumber { get; set; } = string.Empty;
        
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }
        
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        
        public string PaymentStatus { get; set; } = string.Empty; // "Unpaid", "Partial"
        
        public DateTime? IssuedDate { get; set; } // Ngày phát hành/OrderDate
        public DateTime? DueDate { get; set; } // Ngày đến hạn (calculated)
        
        /// <summary>
        /// Số ngày quá hạn (Overdue Days)
        /// </summary>
        public int OverdueDays { get; set; }
        
        public DateTime? LastPaymentDate { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// ✅ 4.3.2.1: DTO cho Accounts Receivable Summary
    /// </summary>
    public class AccountsReceivableSummaryDto
    {
        public decimal TotalReceivable { get; set; }
        public decimal OverdueReceivable { get; set; }
        public decimal Overdue30Days { get; set; }
        public decimal Overdue60Days { get; set; }
        public decimal Overdue90Days { get; set; }
        public int TotalCount { get; set; }
        public int OverdueCount { get; set; }
        public List<CustomerReceivableDto> ByCustomer { get; set; } = new();
    }

    /// <summary>
    /// ✅ 4.3.2.1: DTO cho công nợ theo khách hàng
    /// </summary>
    public class CustomerReceivableDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public decimal TotalReceivable { get; set; }
        public int InvoiceCount { get; set; }
        public int OverdueCount { get; set; }
    }
}

