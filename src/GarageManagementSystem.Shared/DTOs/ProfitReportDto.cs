namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// ✅ 4.3.3.1: DTO cho Báo cáo Lợi nhuận (Profit Report)
    /// </summary>
    public class IncomeStatementDto
    {
        /// <summary>
        /// Doanh thu (Revenue)
        /// </summary>
        public RevenueDto Revenue { get; set; } = new();

        /// <summary>
        /// Giá vốn hàng bán (Cost of Goods Sold)
        /// </summary>
        public COGSDto COGS { get; set; } = new();

        /// <summary>
        /// Chi phí (Expenses)
        /// </summary>
        public ExpensesDto Expenses { get; set; } = new();

        /// <summary>
        /// Lợi nhuận (Profit)
        /// </summary>
        public ProfitDto Profit { get; set; } = new();
    }

    /// <summary>
    /// Doanh thu
    /// </summary>
    public class RevenueDto
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
    public class COGSDto
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
    public class ExpensesDto
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
    public class ProfitDto
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

