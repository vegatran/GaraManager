using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(IUnitOfWork unitOfWork, ILogger<AnalyticsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Dashboard overview statistics
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var today = DateTime.Now.Date;
                var thisMonth = new DateTime(today.Year, today.Month, 1);
                var lastMonth = thisMonth.AddMonths(-1);

                // Get all data
                var customers = await _unitOfWork.Customers.GetAllAsync();
                var vehicles = await _unitOfWork.Vehicles.GetAllAsync();
                var serviceOrders = await _unitOfWork.ServiceOrders.GetAllAsync();
                var payments = await _unitOfWork.Payments.GetAllAsync();
                var appointments = await _unitOfWork.Appointments.GetAllAsync();

                var stats = new
                {
                    // Today's stats
                    Today = new
                    {
                        Appointments = appointments.Count(a => a.ScheduledDateTime.Date == today),
                        ActiveOrders = serviceOrders.Count(o => o.OrderDate.Date == today),
                        Revenue = payments.Where(p => p.PaymentDate.Date == today && p.Status == "Completed").Sum(p => p.Amount)
                    },

                    // This month
                    ThisMonth = new
                    {
                        NewCustomers = customers.Count(c => c.CreatedDate >= thisMonth),
                        NewOrders = serviceOrders.Count(o => o.OrderDate >= thisMonth),
                        CompletedOrders = serviceOrders.Count(o => o.CompletedDate >= thisMonth),
                        Revenue = payments.Where(p => p.PaymentDate >= thisMonth && p.Status == "Completed").Sum(p => p.Amount),
                        AvgOrderValue = serviceOrders.Where(o => o.OrderDate >= thisMonth && o.FinalAmount > 0).Any() ?
                            serviceOrders.Where(o => o.OrderDate >= thisMonth).Average(o => o.FinalAmount) : 0
                    },

                    // Last month comparison
                    LastMonth = new
                    {
                        Revenue = payments.Where(p => p.PaymentDate >= lastMonth && p.PaymentDate < thisMonth && p.Status == "Completed").Sum(p => p.Amount),
                        Orders = serviceOrders.Count(o => o.OrderDate >= lastMonth && o.OrderDate < thisMonth)
                    },

                    // Overall totals
                    Total = new
                    {
                        Customers = customers.Count(),
                        Vehicles = vehicles.Count(),
                        ActiveOrders = serviceOrders.Count(o => o.Status == "InProgress" || o.Status == "Pending"),
                        PendingPayments = serviceOrders.Sum(o => o.AmountRemaining)
                    },

                    // Status breakdown
                    OrderStatus = new
                    {
                        Pending = serviceOrders.Count(o => o.Status == "Pending"),
                        InProgress = serviceOrders.Count(o => o.Status == "InProgress"),
                        Completed = serviceOrders.Count(o => o.Status == "Completed"),
                        Cancelled = serviceOrders.Count(o => o.Status == "Cancelled")
                    },

                    // Payment status
                    PaymentStatus = new
                    {
                        Paid = serviceOrders.Count(o => o.PaymentStatus == "Paid"),
                        Partial = serviceOrders.Count(o => o.PaymentStatus == "Partial"),
                        Unpaid = serviceOrders.Count(o => o.PaymentStatus == "Unpaid")
                    }
                };

                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thống kê dashboard" });
            }
        }

        /// <summary>
        /// Parts turnover rate analysis
        /// </summary>
        [HttpGet("parts-turnover")]
        public async Task<IActionResult> GetPartsTurnoverRate([FromQuery] int days = 90)
        {
            try
            {
                var fromDate = DateTime.Now.AddDays(-days);
                var parts = await _unitOfWork.Parts.GetAllAsync();
                var stockTransactions = await _unitOfWork.StockTransactions.GetAllAsync();

                var turnoverAnalysis = parts.Select(p =>
                {
                    var outTransactions = stockTransactions.Where(st => 
                        st.PartId == p.Id && 
                        st.TransactionType.ToString() == "Out" &&
                        st.TransactionDate >= fromDate).Sum(st => st.Quantity);

                        var avgStock = p.QuantityInStock > 0 ? p.QuantityInStock : 1;
                        var turnoverRate = avgStock > 0 ? (decimal)outTransactions / avgStock : 0;
                        var turnoverDays = turnoverRate > 0 ? days / (double)turnoverRate : 0;

                        return new
                        {
                            p.Id,
                            p.PartName,
                            p.PartNumber,
                            CurrentStock = p.QuantityInStock,
                            QuantitySold = outTransactions,
                            TurnoverRate = Math.Round(turnoverRate, 2),
                            AvgDaysToSell = Math.Round(turnoverDays, 1),
                            Category = turnoverDays == 0 ? "No Sales" :
                                      turnoverDays <= 30 ? "Fast Moving" :
                                      turnoverDays <= 60 ? "Medium Moving" : "Slow Moving",
                            ValueInStock = p.QuantityInStock * p.CostPrice
                        };
                }).OrderByDescending(p => p.TurnoverRate).ToList();

                var summary = new
                {
                    FastMoving = turnoverAnalysis.Count(p => p.Category == "Fast Moving"),
                    MediumMoving = turnoverAnalysis.Count(p => p.Category == "Medium Moving"),
                    SlowMoving = turnoverAnalysis.Count(p => p.Category == "Slow Moving"),
                    NoSales = turnoverAnalysis.Count(p => p.Category == "No Sales"),
                    Data = turnoverAnalysis
                };

                return Ok(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing parts turnover");
                return StatusCode(500, new { success = false, message = "Lỗi khi phân tích vòng quay phụ tùng" });
            }
        }

        /// <summary>
        /// Service completion time analysis
        /// </summary>
        [HttpGet("completion-time")]
        public async Task<IActionResult> GetCompletionTimeAnalysis()
        {
            try
            {
                var orders = await _unitOfWork.ServiceOrders.GetAllAsync();
                var completedOrders = orders.Where(o => o.CompletedDate.HasValue && o.StartDate.HasValue).ToList();

                var analysis = completedOrders.Select(o => new
                {
                    o.OrderNumber,
                    StartDate = o.StartDate,
                    CompletedDate = o.CompletedDate,
                    CompletionTimeHours = o.StartDate.HasValue && o.CompletedDate.HasValue ? 
                        (o.CompletedDate.Value - o.StartDate.Value).TotalHours : 0,
                    o.Status,
                    VehicleType = o.Vehicle?.VehicleType
                }).ToList();

                var summary = new
                {
                    AvgCompletionTimeHours = analysis.Any() ? Math.Round(analysis.Average(a => a.CompletionTimeHours), 2) : 0,
                    FastestCompletionHours = analysis.Any() ? Math.Round(analysis.Min(a => a.CompletionTimeHours), 2) : 0,
                    SlowestCompletionHours = analysis.Any() ? Math.Round(analysis.Max(a => a.CompletionTimeHours), 2) : 0,
                    
                    ByVehicleType = analysis.GroupBy(a => a.VehicleType)
                        .Select(g => new
                        {
                            VehicleType = g.Key,
                            Count = g.Count(),
                            AvgHours = Math.Round(g.Average(a => a.CompletionTimeHours), 2)
                        }).ToList()
                };

                return Ok(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing completion time");
                return StatusCode(500, new { success = false, message = "Lỗi khi phân tích thời gian hoàn thành" });
            }
        }

        /// <summary>
        /// Payment method distribution
        /// </summary>
        [HttpGet("payment-methods")]
        public async Task<IActionResult> GetPaymentMethodStats(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var from = fromDate ?? DateTime.Now.AddMonths(-1);
                var to = toDate ?? DateTime.Now;

                var payments = await _unitOfWork.Payments.GetAllAsync();
                var periodPayments = payments.Where(p => 
                    p.PaymentDate >= from && 
                    p.PaymentDate <= to && 
                    p.Status == "Completed").ToList();

                var distribution = periodPayments
                    .GroupBy(p => p.PaymentMethod)
                    .Select(g => new
                    {
                        PaymentMethod = g.Key,
                        Count = g.Count(),
                        TotalAmount = g.Sum(p => p.Amount),
                        Percentage = periodPayments.Any() ? Math.Round((double)g.Count() / periodPayments.Count * 100, 2) : 0,
                        AvgAmount = g.Average(p => p.Amount)
                    })
                    .OrderByDescending(x => x.TotalAmount)
                    .ToList();

                return Ok(new { success = true, data = distribution });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment method stats");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thống kê phương thức thanh toán" });
            }
        }
    }
}

