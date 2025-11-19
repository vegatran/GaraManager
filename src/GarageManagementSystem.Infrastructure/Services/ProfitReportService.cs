using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GarageManagementSystem.Infrastructure.Services
{
    /// <summary>
    /// ✅ 4.3.3.1: Service tính toán Báo cáo Lợi nhuận (Profit Report)
    /// </summary>
    public class ProfitReportService : IProfitReportService
    {
        private readonly GarageDbContext _context;
        private readonly ILogger<ProfitReportService> _logger;

        public ProfitReportService(
            GarageDbContext context,
            ILogger<ProfitReportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// ✅ 4.3.3.1: Tính toán Báo cáo Kết quả Kinh doanh (Income Statement)
        /// </summary>
        public async Task<IncomeStatementResult> GetIncomeStatementAsync(
            DateTime fromDate,
            DateTime toDate,
            string? serviceOrderStatus = null)
        {
            try
            {
                // ✅ OPTIMIZED: Execute queries in parallel để tăng performance
                var revenueTask = CalculateRevenueAsync(fromDate, toDate);
                var cogsTask = CalculateCOGSAsync(fromDate, toDate, serviceOrderStatus);
                var expensesTask = CalculateExpensesAsync(fromDate, toDate);

                await Task.WhenAll(revenueTask, cogsTask, expensesTask);

                var revenue = await revenueTask;
                var cogs = await cogsTask;
                var expenses = await expensesTask;

                // Tính lợi nhuận
                var grossProfit = revenue.TotalRevenue - cogs.TotalCOGS;
                var grossProfitMargin = revenue.TotalRevenue > 0 
                    ? (grossProfit / revenue.TotalRevenue) * 100 
                    : 0;

                var netProfit = grossProfit - expenses.LaborCost - expenses.OperatingCost;
                var netProfitMargin = revenue.TotalRevenue > 0 
                    ? (netProfit / revenue.TotalRevenue) * 100 
                    : 0;

                return new IncomeStatementResult
                {
                    Revenue = revenue,
                    COGS = cogs,
                    Expenses = expenses,
                    Profit = new ProfitResult
                    {
                        GrossProfit = grossProfit,
                        GrossProfitMargin = grossProfitMargin,
                        NetProfit = netProfit,
                        NetProfitMargin = netProfitMargin
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi tính toán Income Statement từ {fromDate:yyyy-MM-dd} đến {toDate:yyyy-MM-dd}");
                throw;
            }
        }

        /// <summary>
        /// Tính doanh thu (Revenue) từ FinancialTransaction (Income)
        /// </summary>
        private async Task<RevenueResult> CalculateRevenueAsync(DateTime fromDate, DateTime toDate)
        {
            // ✅ OPTIMIZED: Query ở DB level với AsNoTracking và SumAsync để tránh load vào memory
            var baseQuery = _context.FinancialTransactions
                .AsNoTracking()
                .Where(ft => !ft.IsDeleted
                    && ft.TransactionType == "Income"
                    && ft.TransactionDate >= fromDate
                    && ft.TransactionDate <= toDate
                    && ft.Status == "Completed"); // Chỉ lấy transactions đã completed

            // ✅ OPTIMIZED: Tính ServiceRevenue và PartsSale ở DB level (parallel)
            var serviceRevenueTask = baseQuery
                .Where(ft => ft.Category == "Service Revenue" || ft.Category == "Insurance Revenue")
                .SumAsync(ft => (decimal?)ft.Amount);

            var partsSaleTask = baseQuery
                .Where(ft => ft.Category == "Parts Sale")
                .SumAsync(ft => (decimal?)ft.Amount);

            await Task.WhenAll(serviceRevenueTask, partsSaleTask);

            var serviceRevenue = await serviceRevenueTask ?? 0;
            var partsSale = await partsSaleTask ?? 0;

            return new RevenueResult
            {
                ServiceRevenue = serviceRevenue,
                PartsSale = partsSale,
                TotalRevenue = serviceRevenue + partsSale
            };
        }

        /// <summary>
        /// Tính giá vốn hàng bán (COGS) từ ServiceOrder.TotalCOGS
        /// </summary>
        private async Task<COGSResult> CalculateCOGSAsync(DateTime fromDate, DateTime toDate, string? serviceOrderStatus)
        {
            // ✅ OPTIMIZED: Query ở DB level với AsNoTracking
            var query = _context.ServiceOrders
                .AsNoTracking()
                .Where(so => !so.IsDeleted
                    && so.OrderDate >= fromDate
                    && so.OrderDate <= toDate
                    && so.TotalCOGS > 0); // Chỉ lấy ServiceOrder đã có COGS

            // Filter theo status nếu có
            if (!string.IsNullOrEmpty(serviceOrderStatus))
            {
                query = query.Where(so => so.Status == serviceOrderStatus);
            }

            var totalCOGS = await query
                .SumAsync(so => so.TotalCOGS);

            return new COGSResult
            {
                TotalCOGS = totalCOGS,
                Notes = "COGS tính từ ServiceOrder.TotalCOGS (đã tính từ vật tư xuất kho)"
            };
        }

        /// <summary>
        /// Tính chi phí (Expenses) từ FinancialTransaction (Expense) và ServiceOrder (Labor Cost)
        /// </summary>
        private async Task<ExpensesResult> CalculateExpensesAsync(DateTime fromDate, DateTime toDate)
        {
            // ✅ OPTIMIZED: Query ở DB level với AsNoTracking và SumAsync để tránh load vào memory
            var expenseBaseQuery = _context.FinancialTransactions
                .AsNoTracking()
                .Where(ft => !ft.IsDeleted
                    && ft.TransactionType == "Expense"
                    && ft.TransactionDate >= fromDate
                    && ft.TransactionDate <= toDate
                    && ft.Status == "Completed"); // Chỉ lấy transactions đã completed

            // ✅ OPTIMIZED: Tính PartsPurchase và OperatingCost ở DB level (parallel)
            var partsPurchaseTask = expenseBaseQuery
                .Where(ft => ft.Category == "Parts Purchase")
                .SumAsync(ft => (decimal?)ft.Amount);

            var operatingCostTask = expenseBaseQuery
                .Where(ft => ft.Category != "Parts Purchase")
                .SumAsync(ft => (decimal?)ft.Amount);

            // ✅ OPTIMIZED: Tính TotalActualHours ở DB level
            var totalActualHoursTask = _context.ServiceOrders
                .AsNoTracking()
                .Where(so => !so.IsDeleted
                    && so.OrderDate >= fromDate
                    && so.OrderDate <= toDate
                    && so.TotalActualHours.HasValue
                    && so.TotalActualHours.Value > 0)
                .SumAsync(so => (decimal?)so.TotalActualHours);

            // ✅ OPTIMIZED: Lấy ServiceOrderIds để query ServiceOrderFees (chỉ cần IDs)
            var serviceOrderIdsTask = _context.ServiceOrders
                .AsNoTracking()
                .Where(so => !so.IsDeleted
                    && so.OrderDate >= fromDate
                    && so.OrderDate <= toDate
                    && so.TotalActualHours.HasValue
                    && so.TotalActualHours.Value > 0)
                .Select(so => so.Id)
                .ToListAsync();

            await Task.WhenAll(partsPurchaseTask, operatingCostTask, totalActualHoursTask, serviceOrderIdsTask);

            var partsPurchase = await partsPurchaseTask ?? 0;
            var operatingCost = await operatingCostTask ?? 0;
            var totalActualHours = await totalActualHoursTask ?? 0;
            var serviceOrderIds = await serviceOrderIdsTask;

            // Chi phí nhân công (Labor Cost)
            // Note: Cần đơn giá công. Tạm thời dùng giá mặc định hoặc tính từ ServiceOrderFee
            // TODO: Cần config đơn giá công hoặc lấy từ ServiceOrderFee
            var defaultLaborRate = 50000m; // 50,000 VNĐ/giờ (mặc định)
            
            // ✅ OPTIMIZED: Tính Labor Cost từ ServiceOrderFee nếu có ServiceOrders
            decimal laborCost;
            if (serviceOrderIds.Any())
            {
                var laborCostFromFeesTask = _context.ServiceOrderFees
                    .AsNoTracking()
                    .Include(f => f.ServiceFeeType) // ✅ Include ServiceFeeType để check Name
                    .Where(f => serviceOrderIds.Contains(f.ServiceOrderId)
                        && f.ServiceFeeType != null
                        && f.ServiceFeeType.Name.Contains("Labor", StringComparison.OrdinalIgnoreCase))
                    .SumAsync(f => (decimal?)f.Amount);

                var laborCostFromFees = await laborCostFromFeesTask ?? 0;

                // ✅ SỬA: Nếu có labor cost từ fees, dùng nó; nếu không, dùng default rate
                laborCost = laborCostFromFees > 0 
                    ? laborCostFromFees 
                    : totalActualHours * defaultLaborRate;
            }
            else
            {
                laborCost = totalActualHours * defaultLaborRate;
            }

            return new ExpensesResult
            {
                LaborCost = laborCost,
                OperatingCost = operatingCost,
                PartsPurchase = partsPurchase,
                TotalExpenses = laborCost + operatingCost + partsPurchase
            };
        }
    }
}

