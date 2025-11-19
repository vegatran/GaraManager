namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// DTO cho Sổ Quỹ Tiền Mặt/Ngân Hàng
    /// </summary>
    public class PaymentMethodRegisterDto
    {
        /// <summary>
        /// Phương thức thanh toán: "Cash", "Bank Transfer", "Credit Card", etc.
        /// </summary>
        public string PaymentMethod { get; set; } = string.Empty;
        
        /// <summary>
        /// Ngày bắt đầu kỳ (để tính số dư đầu kỳ)
        /// </summary>
        public DateTime? FromDate { get; set; }
        
        /// <summary>
        /// Ngày kết thúc kỳ
        /// </summary>
        public DateTime? ToDate { get; set; }
        
        /// <summary>
        /// Số dư đầu kỳ (tính từ tất cả giao dịch trước FromDate)
        /// </summary>
        public decimal OpeningBalance { get; set; }
        
        /// <summary>
        /// Tổng thu trong kỳ (Income transactions)
        /// </summary>
        public decimal TotalIncome { get; set; }
        
        /// <summary>
        /// Tổng chi trong kỳ (Expense transactions)
        /// </summary>
        public decimal TotalExpense { get; set; }
        
        /// <summary>
        /// Số dư cuối kỳ = OpeningBalance + TotalIncome - TotalExpense
        /// </summary>
        public decimal ClosingBalance { get; set; }
        
        /// <summary>
        /// Tổng số giao dịch trong kỳ
        /// </summary>
        public int TransactionCount { get; set; }
        
        /// <summary>
        /// Số giao dịch thu
        /// </summary>
        public int IncomeCount { get; set; }
        
        /// <summary>
        /// Số giao dịch chi
        /// </summary>
        public int ExpenseCount { get; set; }
        
        /// <summary>
        /// Danh sách giao dịch chi tiết trong kỳ
        /// </summary>
        public List<FinancialTransactionDto> Transactions { get; set; } = new();
    }
}

