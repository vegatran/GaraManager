using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IUnitOfWork unitOfWork, ILogger<ReportsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Báo cáo doanh thu theo khoảng thời gian
        /// </summary>
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueReport(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string groupBy = "day") // day, week, month, year
        {
            try
            {
                var from = fromDate ?? DateTime.Now.AddMonths(-1);
                var to = toDate ?? DateTime.Now;

                // Get all completed service orders
                var orders = await _unitOfWork.ServiceOrders.GetAllAsync();
                var completedOrders = orders.Where(o => 
                    o.CompletedDate.HasValue && 
                    o.CompletedDate >= from && 
                    o.CompletedDate <= to).ToList();

                // Get all payments
                var payments = await _unitOfWork.Payments.GetAllAsync();
                var periodPayments = payments.Where(p => 
                    p.PaymentDate >= from && 
                    p.PaymentDate <= to && 
                    p.Status == "Completed").ToList();

                // Group by period
                var revenueData = groupBy.ToLower() switch
                {
                    "day" => periodPayments.GroupBy(p => p.PaymentDate.Date)
                        .Select(g => new
                        {
                            Period = g.Key.ToString("yyyy-MM-dd"),
                            Revenue = g.Sum(p => p.Amount),
                            OrderCount = g.Count(),
                            AvgOrderValue = g.Any() ? g.Average(p => p.Amount) : 0
                        }).OrderBy(x => x.Period).ToList(),
                    
                    "week" => periodPayments.GroupBy(p => new { 
                            Year = p.PaymentDate.Year, 
                            Week = System.Globalization.ISOWeek.GetWeekOfYear(p.PaymentDate) 
                        })
                        .Select(g => new
                        {
                            Period = $"{g.Key.Year}-W{g.Key.Week:D2}",
                            Revenue = g.Sum(p => p.Amount),
                            OrderCount = g.Count(),
                            AvgOrderValue = g.Any() ? g.Average(p => p.Amount) : 0
                        }).OrderBy(x => x.Period).ToList(),
                    
                    "month" => periodPayments.GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                        .Select(g => new
                        {
                            Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                            Revenue = g.Sum(p => p.Amount),
                            OrderCount = g.Count(),
                            AvgOrderValue = g.Any() ? g.Average(p => p.Amount) : 0
                        }).OrderBy(x => x.Period).ToList(),
                    
                    _ => periodPayments.GroupBy(p => p.PaymentDate.Year)
                        .Select(g => new
                        {
                            Period = g.Key.ToString(),
                            Revenue = g.Sum(p => p.Amount),
                            OrderCount = g.Count(),
                            AvgOrderValue = g.Any() ? g.Average(p => p.Amount) : 0
                        }).OrderBy(x => x.Period).ToList()
                };

                var summary = new
                {
                    TotalRevenue = periodPayments.Sum(p => p.Amount),
                    TotalOrders = completedOrders.Count,
                    TotalPayments = periodPayments.Count,
                    AvgOrderValue = completedOrders.Any() ? completedOrders.Average(o => o.FinalAmount) : 0,
                    PeriodFrom = from,
                    PeriodTo = to,
                    Data = revenueData
                };

                return Ok(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating revenue report");
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo báo cáo doanh thu" });
            }
        }

        /// <summary>
        /// Báo cáo lợi nhuận (revenue - costs)
        /// </summary>
        [HttpGet("profit")]
        public async Task<IActionResult> GetProfitReport(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var from = fromDate ?? DateTime.Now.AddMonths(-1);
                var to = toDate ?? DateTime.Now;

                // Revenue from payments
                var payments = await _unitOfWork.Payments.GetAllAsync();
                var revenue = payments.Where(p => 
                    p.PaymentDate >= from && 
                    p.PaymentDate <= to && 
                    p.Status == "Completed").Sum(p => p.Amount);

                // Costs from stock transactions (parts purchased)
                var stockTransactions = await _unitOfWork.StockTransactions.GetAllAsync();
                var partsCost = stockTransactions.Where(st => 
                    st.TransactionDate >= from && 
                    st.TransactionDate <= to && 
                    st.TransactionType.ToString() == "In").Sum(st => st.TotalCost);

                // Labor costs (simplified - would need employee salaries)
                var serviceOrders = await _unitOfWork.ServiceOrders.GetAllAsync();
                var laborRevenue = serviceOrders.Where(o => 
                    o.CompletedDate.HasValue && 
                    o.CompletedDate >= from && 
                    o.CompletedDate <= to).Sum(o => o.ServiceTotal);

                var profit = revenue - partsCost;
                var profitMargin = revenue > 0 ? (profit / revenue) * 100 : 0;

                var report = new
                {
                    PeriodFrom = from,
                    PeriodTo = to,
                    TotalRevenue = revenue,
                    PartsCost = partsCost,
                    GrossProfit = profit,
                    ProfitMargin = Math.Round(profitMargin, 2),
                    LaborRevenue = laborRevenue,
                    PartsRevenue = revenue - laborRevenue
                };

                return Ok(new { success = true, data = report });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating profit report");
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo báo cáo lợi nhuận" });
            }
        }

        /// <summary>
        /// Top customers by revenue
        /// </summary>
        [HttpGet("top-customers")]
        public async Task<IActionResult> GetTopCustomers(
            [FromQuery] int top = 10,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var from = fromDate ?? DateTime.Now.AddMonths(-6);
                var to = toDate ?? DateTime.Now;

                var payments = await _unitOfWork.Payments.GetAllAsync();
                var periodPayments = payments.Where(p => 
                    p.PaymentDate >= from && 
                    p.PaymentDate <= to && 
                    p.Status == "Completed").ToList();

                var topCustomers = periodPayments
                    .GroupBy(p => p.CustomerId)
                    .Select(g => new
                    {
                        CustomerId = g.Key,
                        CustomerName = g.First().CustomerName,
                        TotalSpent = g.Sum(p => p.Amount),
                        OrderCount = g.Count(),
                        AvgOrderValue = g.Average(p => p.Amount),
                        LastVisit = g.Max(p => p.PaymentDate)
                    })
                    .OrderByDescending(c => c.TotalSpent)
                    .Take(top)
                    .ToList();

                return Ok(new { success = true, data = topCustomers });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top customers");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy khách hàng VIP" });
            }
        }

        /// <summary>
        /// Top selling services
        /// </summary>
        [HttpGet("top-services")]
        public async Task<IActionResult> GetTopServices(
            [FromQuery] int top = 10,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var from = fromDate ?? DateTime.Now.AddMonths(-3);
                var to = toDate ?? DateTime.Now;

                var serviceOrderItems = await _unitOfWork.Repository<ServiceOrderItem>().GetAllAsync();
                
                var topServices = serviceOrderItems
                    .GroupBy(i => i.ServiceId)
                    .Select(g => new
                    {
                        ServiceId = g.Key,
                        ServiceName = g.First().ServiceName,
                        TimesOrdered = g.Sum(i => i.Quantity),
                        TotalRevenue = g.Sum(i => i.TotalPrice),
                        AvgPrice = g.Average(i => i.UnitPrice)
                    })
                    .OrderByDescending(s => s.TotalRevenue)
                    .Take(top)
                    .ToList();

                return Ok(new { success = true, data = topServices });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top services");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy dịch vụ bán chạy" });
            }
        }

        /// <summary>
        /// Top selling parts
        /// </summary>
        [HttpGet("top-parts")]
        public async Task<IActionResult> GetTopParts(
            [FromQuery] int top = 10,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var from = fromDate ?? DateTime.Now.AddMonths(-3);
                var to = toDate ?? DateTime.Now;

                var serviceOrderParts = await _unitOfWork.Repository<ServiceOrderPart>().GetAllAsync();
                
                var topParts = serviceOrderParts
                    .GroupBy(p => p.PartId)
                    .Select(g => new
                    {
                        PartId = g.Key,
                        PartName = g.First().PartName,
                        QuantitySold = g.Sum(p => p.Quantity),
                        TotalRevenue = g.Sum(p => p.TotalPrice),
                        TotalCost = g.Sum(p => p.UnitCost * p.Quantity),
                        Profit = g.Sum(p => p.TotalPrice - (p.UnitCost * p.Quantity)),
                        AvgSellingPrice = g.Average(p => p.UnitPrice)
                    })
                    .OrderByDescending(p => p.TotalRevenue)
                    .Take(top)
                    .ToList();

                return Ok(new { success = true, data = topParts });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top parts");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy phụ tùng bán chạy" });
            }
        }

        /// <summary>
        /// Inventory status report
        /// </summary>
        [HttpGet("inventory-status")]
        public async Task<IActionResult> GetInventoryStatus()
        {
            try
            {
                var parts = await _unitOfWork.Parts.GetAllAsync();
                
                var inventoryStatus = new
                {
                    TotalParts = parts.Count(),
                    LowStock = parts.Where(p => p.QuantityInStock <= p.MinimumStock).Select(p => new
                    {
                        p.Id,
                        p.PartName,
                        p.PartNumber,
                        StockLevel = p.QuantityInStock,
                        MinStockLevel = p.MinimumStock,
                        Deficit = p.MinimumStock - p.QuantityInStock
                    }).OrderBy(p => p.StockLevel).ToList(),
                    OutOfStock = parts.Where(p => p.QuantityInStock == 0).Select(p => new
                    {
                        p.Id,
                        p.PartName,
                        p.PartNumber
                    }).ToList(),
                    OverStock = parts.Where(p => p.ReorderLevel.HasValue && p.QuantityInStock > p.ReorderLevel * 2).Select(p => new
                    {
                        p.Id,
                        p.PartName,
                        p.PartNumber,
                        StockLevel = p.QuantityInStock,
                        MaxStockLevel = p.ReorderLevel * 2,
                        Excess = p.QuantityInStock - (p.ReorderLevel * 2)
                    }).ToList(),
                    TotalValue = parts.Sum(p => p.QuantityInStock * p.CostPrice)
                };

                return Ok(new { success = true, data = inventoryStatus });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventory status");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy tình trạng tồn kho" });
            }
        }

        /// <summary>
        /// Service order statistics
        /// </summary>
        [HttpGet("service-order-stats")]
        public async Task<IActionResult> GetServiceOrderStats(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var from = fromDate ?? DateTime.Now.AddMonths(-1);
                var to = toDate ?? DateTime.Now;

                var orders = await _unitOfWork.ServiceOrders.GetAllAsync();
                var periodOrders = orders.Where(o => o.OrderDate >= from && o.OrderDate <= to).ToList();

                var stats = new
                {
                    TotalOrders = periodOrders.Count,
                    CompletedOrders = periodOrders.Count(o => o.Status == "Completed"),
                    PendingOrders = periodOrders.Count(o => o.Status == "Pending"),
                    InProgressOrders = periodOrders.Count(o => o.Status == "InProgress"),
                    CancelledOrders = periodOrders.Count(o => o.Status == "Cancelled"),
                    
                    // By vehicle type
                    PersonalOrders = periodOrders.Count(o => o.Vehicle?.VehicleType == "Personal"),
                    CompanyOrders = periodOrders.Count(o => o.Vehicle?.VehicleType == "Company"),
                    InsuranceOrders = periodOrders.Count(o => o.Vehicle?.VehicleType == "Insurance"),
                    
                    // Financial
                    TotalValue = periodOrders.Sum(o => o.FinalAmount),
                    PaidValue = periodOrders.Sum(o => o.AmountPaid),
                    RemainingValue = periodOrders.Sum(o => o.AmountRemaining),
                    
                    // Avg completion time
                    AvgCompletionDays = periodOrders
                        .Where(o => o.CompletedDate.HasValue)
                        .Select(o => (o.CompletedDate!.Value - o.OrderDate).TotalDays)
                        .DefaultIfEmpty(0)
                        .Average()
                };

                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service order stats");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thống kê đơn hàng" });
            }
        }

        /// <summary>
        /// Customer statistics
        /// </summary>
        [HttpGet("customer-stats")]
        public async Task<IActionResult> GetCustomerStats()
        {
            try
            {
                var customers = await _unitOfWork.Customers.GetAllAsync();
                var vehicles = await _unitOfWork.Vehicles.GetAllAsync();
                var serviceOrders = await _unitOfWork.ServiceOrders.GetAllAsync();

                var stats = new
                {
                    TotalCustomers = customers.Count(),
                    ActiveCustomers = customers.Count(c => c.IsActive),
                    TotalVehicles = vehicles.Count(),
                    
                    // New customers this month
                    NewCustomersThisMonth = customers.Count(c => 
                        c.CreatedDate.Year == DateTime.Now.Year && 
                        c.CreatedDate.Month == DateTime.Now.Month),
                    
                    // Customers by type (inferred from vehicles)
                    PersonalCustomers = customers.Count(c => c.Vehicles.Any(v => v.VehicleType == "Personal")),
                    CompanyCustomers = customers.Count(c => c.Vehicles.Any(v => v.VehicleType == "Company")),
                    InsuranceCustomers = customers.Count(c => c.Vehicles.Any(v => v.VehicleType == "Insurance")),
                    
                    // Repeat customers (more than 1 order)
                    RepeatCustomers = serviceOrders.GroupBy(o => o.CustomerId)
                        .Where(g => g.Count() > 1)
                        .Count(),
                    
                    AvgVehiclesPerCustomer = customers.Any() ? 
                        Math.Round((double)vehicles.Count() / customers.Count(), 2) : 0
                };

                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer stats");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thống kê khách hàng" });
            }
        }

        /// <summary>
        /// Employee performance report
        /// </summary>
        [HttpGet("employee-performance")]
        public async Task<IActionResult> GetEmployeePerformance(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var from = fromDate ?? DateTime.Now.AddMonths(-1);
                var to = toDate ?? DateTime.Now;

                var serviceOrders = await _unitOfWork.ServiceOrders.GetAllAsync();
                var periodOrders = serviceOrders.Where(o => 
                    o.OrderDate >= from && 
                    o.OrderDate <= to &&
                    o.PrimaryTechnicianId.HasValue).ToList();

                var performance = periodOrders
                    .GroupBy(o => o.PrimaryTechnicianId)
                    .Select(g => new
                    {
                        EmployeeId = g.Key,
                        EmployeeName = g.First().PrimaryTechnician?.Name,
                        TotalJobs = g.Count(),
                        CompletedJobs = g.Count(o => o.Status == "Completed"),
                        TotalRevenue = g.Sum(o => o.FinalAmount),
                        AvgJobValue = g.Average(o => o.FinalAmount),
                        CompletionRate = g.Any() ? Math.Round((double)g.Count(o => o.Status == "Completed") / g.Count() * 100, 2) : 0
                    })
                    .OrderByDescending(e => e.TotalRevenue)
                    .ToList();

                return Ok(new { success = true, data = performance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employee performance");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy hiệu suất nhân viên" });
            }
        }

        /// <summary>
        /// Parts usage analysis
        /// </summary>
        [HttpGet("parts-usage")]
        public async Task<IActionResult> GetPartsUsageAnalysis(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var from = fromDate ?? DateTime.Now.AddMonths(-3);
                var to = toDate ?? DateTime.Now;

                var stockTransactions = await _unitOfWork.StockTransactions.GetAllAsync();
                var usageTransactions = stockTransactions.Where(st => 
                    st.TransactionDate >= from && 
                    st.TransactionDate <= to &&
                    st.TransactionType.ToString() == "Out").ToList();

                var analysis = usageTransactions
                    .GroupBy(st => st.PartId)
                    .Select(g => new
                    {
                        PartId = g.Key,
                        PartName = g.First().Part?.PartName,
                        QuantityUsed = g.Sum(st => st.Quantity),
                        TotalValue = g.Sum(st => st.TotalCost),
                        UsageCount = g.Count(),
                        AvgQuantityPerUsage = g.Average(st => st.Quantity),
                        LastUsed = g.Max(st => st.TransactionDate)
                    })
                    .OrderByDescending(p => p.QuantityUsed)
                    .ToList();

                return Ok(new { success = true, data = analysis });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parts usage analysis");
                return StatusCode(500, new { success = false, message = "Lỗi khi phân tích sử dụng phụ tùng" });
            }
        }

        /// <summary>
        /// Insurance claims summary
        /// </summary>
        [HttpGet("insurance-summary")]
        public async Task<IActionResult> GetInsuranceSummary(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var from = fromDate ?? DateTime.Now.AddMonths(-6);
                var to = toDate ?? DateTime.Now;

                var claims = await _unitOfWork.InsuranceClaims.GetAllAsync();
                var periodClaims = claims.Where(c => c.ClaimDate >= from && c.ClaimDate <= to).ToList();

                var summary = new
                {
                    TotalClaims = periodClaims.Count,
                    PendingClaims = periodClaims.Count(c => c.Status == "Pending"),
                    ApprovedClaims = periodClaims.Count(c => c.Status == "Approved"),
                    RejectedClaims = periodClaims.Count(c => c.Status == "Rejected"),
                    SettledClaims = periodClaims.Count(c => c.Status == "Settled"),
                    
                    TotalEstimatedAmount = periodClaims.Sum(c => c.EstimatedAmount ?? 0),
                    TotalApprovedAmount = periodClaims.Sum(c => c.ApprovedAmount ?? 0),
                    TotalSettlementAmount = periodClaims.Sum(c => c.SettlementAmount ?? 0),
                    
                    ApprovalRate = periodClaims.Any() ? 
                        Math.Round((double)periodClaims.Count(c => c.Status == "Approved" || c.Status == "Settled") / periodClaims.Count * 100, 2) : 0,
                    
                    AvgProcessingDays = periodClaims
                        .Where(c => c.SettlementDate.HasValue)
                        .Select(c => (c.SettlementDate!.Value - c.ClaimDate).TotalDays)
                        .DefaultIfEmpty(0)
                        .Average(),
                    
                    // By insurance company
                    ByCompany = periodClaims
                        .Where(c => !string.IsNullOrEmpty(c.InsuranceCompany))
                        .GroupBy(c => c.InsuranceCompany)
                        .Select(g => new
                        {
                            Company = g.Key,
                            ClaimCount = g.Count(),
                            TotalAmount = g.Sum(c => c.SettlementAmount ?? 0),
                            ApprovalRate = Math.Round((double)g.Count(c => c.Status == "Approved" || c.Status == "Settled") / g.Count() * 100, 2)
                        })
                        .OrderByDescending(x => x.ClaimCount)
                        .ToList()
                };

                return Ok(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting insurance summary");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy tổng kết bảo hiểm" });
            }
        }

        /// <summary>
        /// Export revenue report to Excel
        /// </summary>
        [HttpGet("export/revenue")]
        public async Task<IActionResult> ExportRevenueReport(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var from = fromDate ?? DateTime.Now.AddMonths(-1);
                var to = toDate ?? DateTime.Now;

                // Get data
                var payments = await _unitOfWork.Payments.GetAllAsync();
                var periodPayments = payments.Where(p => 
                    p.PaymentDate >= from && 
                    p.PaymentDate <= to && 
                    p.Status == "Completed").OrderBy(p => p.PaymentDate).ToList();

                // Create CSV content
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("Ngày,Số phiếu,Khách hàng,Số hóa đơn,Số tiền,Phương thức");
                
                foreach (var payment in periodPayments)
                {
                    csv.AppendLine($"{payment.PaymentDate:yyyy-MM-dd},PM-{payment.Id},{payment.CustomerName},{payment.InvoiceNumber},{payment.Amount},{payment.PaymentMethod}");
                }

                var fileName = $"Revenue_Report_{from:yyyyMMdd}_{to:yyyyMMdd}.csv";
                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                
                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting revenue report");
                return StatusCode(500, new { success = false, message = "Lỗi khi xuất báo cáo" });
            }
        }
    }
}

