namespace GarageManagementSystem.Core.Interfaces
{
    /// <summary>
    /// ✅ 4.3.3.1: Service tính toán Báo cáo Lợi nhuận (Profit Report)
    /// </summary>
    public interface IProfitReportService
    {
        /// <summary>
        /// Tính toán Báo cáo Kết quả Kinh doanh (Income Statement)
        /// </summary>
        /// <param name="fromDate">Ngày bắt đầu</param>
        /// <param name="toDate">Ngày kết thúc</param>
        /// <param name="serviceOrderStatus">Trạng thái ServiceOrder để lọc (null = tất cả)</param>
        /// <returns>IncomeStatementResult chứa Revenue, COGS, Expenses, và Profit</returns>
        Task<IncomeStatementResult> GetIncomeStatementAsync(
            DateTime fromDate,
            DateTime toDate,
            string? serviceOrderStatus = null);
    }

    /// <summary>
    /// ✅ 4.3.3.1: Kết quả Báo cáo Kết quả Kinh doanh (Income Statement)
    /// </summary>
    public class IncomeStatementResult
    {
        /// <summary>
        /// Doanh thu (Revenue)
        /// </summary>
        public RevenueResult Revenue { get; set; } = new();

        /// <summary>
        /// Giá vốn hàng bán (Cost of Goods Sold)
        /// </summary>
        public COGSResult COGS { get; set; } = new();

        /// <summary>
        /// Chi phí (Expenses)
        /// </summary>
        public ExpensesResult Expenses { get; set; } = new();

        /// <summary>
        /// Lợi nhuận (Profit)
        /// </summary>
        public ProfitResult Profit { get; set; } = new();
    }

    /// <summary>
    /// Doanh thu
    /// </summary>
    public class RevenueResult
    {
        /// <summary>
        /// Doanh thu dịch vụ (từ ServiceOrder/PaymentTransaction)
        /// </summary>
        public decimal ServiceRevenue { get; set; }

        /// <summary>
        /// Doanh thu bán phụ tùng (từ ServiceOrderParts hoặc Parts Sale)
        /// </summary>
        public decimal PartsSale { get; set; }

        /// <summary>
        /// Tổng doanh thu
        /// </summary>
        public decimal TotalRevenue { get; set; }
    }

    /// <summary>
    /// Giá vốn hàng bán
    /// </summary>
    public class COGSResult
    {
        /// <summary>
        /// Tổng giá vốn (từ ServiceOrder.TotalCOGS)
        /// </summary>
        public decimal TotalCOGS { get; set; }

        /// <summary>
        /// Ghi chú: COGS tính từ ServiceOrders
        /// </summary>
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Chi phí
    /// </summary>
    public class ExpensesResult
    {
        /// <summary>
        /// Chi phí nhân công (Labor Cost) - từ ServiceOrder.TotalActualHours × đơn giá
        /// </summary>
        public decimal LaborCost { get; set; }

        /// <summary>
        /// Chi phí vận hành (Operating Cost) - từ FinancialTransaction (Expense) - Parts Purchase
        /// </summary>
        public decimal OperatingCost { get; set; }

        /// <summary>
        /// Chi phí mua phụ tùng (Parts Purchase) - từ FinancialTransaction (Expense) Category = "Parts Purchase"
        /// </summary>
        public decimal PartsPurchase { get; set; }

        /// <summary>
        /// Tổng chi phí
        /// </summary>
        public decimal TotalExpenses { get; set; }
    }

    /// <summary>
    /// Lợi nhuận
    /// </summary>
    public class ProfitResult
    {
        /// <summary>
        /// Lợi nhuận gộp (Gross Profit) = Revenue - COGS
        /// </summary>
        public decimal GrossProfit { get; set; }

        /// <summary>
        /// Tỷ suất lợi nhuận gộp (%) = (Gross Profit / Revenue) × 100
        /// </summary>
        public decimal GrossProfitMargin { get; set; }

        /// <summary>
        /// Lợi nhuận ròng (Net Profit) = Gross Profit - Labor Cost - Operating Cost
        /// </summary>
        public decimal NetProfit { get; set; }

        /// <summary>
        /// Tỷ suất lợi nhuận ròng (%) = (Net Profit / Revenue) × 100
        /// </summary>
        public decimal NetProfitMargin { get; set; }
    }
}

