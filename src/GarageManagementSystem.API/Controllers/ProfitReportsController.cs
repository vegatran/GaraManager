using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarageManagementSystem.API.Controllers
{
    /// <summary>
    /// ✅ 4.3.3.1: Controller quản lý Báo cáo Lợi nhuận (Profit Report)
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProfitReportsController : ControllerBase
    {
        private readonly IProfitReportService _profitReportService;
        private readonly ILogger<ProfitReportsController> _logger;

        public ProfitReportsController(
            IProfitReportService profitReportService,
            ILogger<ProfitReportsController> logger)
        {
            _profitReportService = profitReportService;
            _logger = logger;
        }

        /// <summary>
        /// ✅ 4.3.3.1: Lấy Báo cáo Kết quả Kinh doanh (Income Statement)
        /// </summary>
        /// <param name="fromDate">Ngày bắt đầu (yyyy-MM-dd)</param>
        /// <param name="toDate">Ngày kết thúc (yyyy-MM-dd)</param>
        /// <param name="serviceOrderStatus">Trạng thái ServiceOrder để lọc (null = tất cả)</param>
        /// <returns>IncomeStatementDto chứa Revenue, COGS, Expenses, và Profit</returns>
        [HttpGet("income-statement")]
        public async Task<ActionResult<ApiResponse<IncomeStatementDto>>> GetIncomeStatement(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? serviceOrderStatus = null)
        {
            try
            {
                // ✅ Validate parameters
                var today = DateTime.Now.Date;
                
                if (!fromDate.HasValue)
                {
                    // Default: đầu tháng hiện tại
                    fromDate = new DateTime(today.Year, today.Month, 1);
                }
                
                if (!toDate.HasValue)
                {
                    // Default: hôm nay
                    toDate = today;
                }

                // ✅ Validate date range
                if (fromDate.Value > toDate.Value)
                {
                    return BadRequest(ApiResponse<IncomeStatementDto>.ErrorResult("Ngày bắt đầu không được lớn hơn ngày kết thúc"));
                }

                // ✅ Normalize dates: fromDate = start of day, toDate = end of day
                var fromDateNormalized = fromDate.Value.Date;
                var toDateNormalized = toDate.Value.Date.AddDays(1).AddTicks(-1); // End of day

                // ✅ Calculate income statement
                var incomeStatementResult = await _profitReportService.GetIncomeStatementAsync(
                    fromDateNormalized,
                    toDateNormalized,
                    serviceOrderStatus);

                // ✅ Map từ Core.Interfaces.IncomeStatementResult sang Shared.DTOs.IncomeStatementDto
                var incomeStatement = new IncomeStatementDto
                {
                    Revenue = new RevenueDto
                    {
                        ServiceRevenue = incomeStatementResult.Revenue.ServiceRevenue,
                        PartsSale = incomeStatementResult.Revenue.PartsSale,
                        TotalRevenue = incomeStatementResult.Revenue.TotalRevenue
                    },
                    COGS = new COGSDto
                    {
                        TotalCOGS = incomeStatementResult.COGS.TotalCOGS,
                        Notes = incomeStatementResult.COGS.Notes
                    },
                    Expenses = new ExpensesDto
                    {
                        LaborCost = incomeStatementResult.Expenses.LaborCost,
                        OperatingCost = incomeStatementResult.Expenses.OperatingCost,
                        PartsPurchase = incomeStatementResult.Expenses.PartsPurchase,
                        TotalExpenses = incomeStatementResult.Expenses.TotalExpenses
                    },
                    Profit = new ProfitDto
                    {
                        GrossProfit = incomeStatementResult.Profit.GrossProfit,
                        GrossProfitMargin = incomeStatementResult.Profit.GrossProfitMargin,
                        NetProfit = incomeStatementResult.Profit.NetProfit,
                        NetProfitMargin = incomeStatementResult.Profit.NetProfitMargin
                    }
                };

                return Ok(ApiResponse<IncomeStatementDto>.SuccessResult(incomeStatement, "Lấy báo cáo kết quả kinh doanh thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy Income Statement từ {fromDate:yyyy-MM-dd} đến {toDate:yyyy-MM-dd}");
                return StatusCode(500, ApiResponse<IncomeStatementDto>.ErrorResult("Lỗi khi lấy báo cáo kết quả kinh doanh", ex.Message));
            }
        }
    }
}

