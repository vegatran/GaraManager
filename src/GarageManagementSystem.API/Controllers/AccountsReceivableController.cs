using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GarageManagementSystem.API.Controllers
{
    /// <summary>
    /// ✅ 4.3.2.1: Controller quản lý Accounts Receivable (Công nợ Phải Thu)
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsReceivableController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AccountsReceivableController> _logger;
        private readonly GarageManagementSystem.Infrastructure.Data.GarageDbContext _context;

        public AccountsReceivableController(
            IUnitOfWork unitOfWork,
            ILogger<AccountsReceivableController> logger,
            GarageManagementSystem.Infrastructure.Data.GarageDbContext context)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// ✅ 4.3.2.1: Lấy danh sách công nợ phải thu
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResponse<AccountsReceivableDto>>> GetAccountsReceivable(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? customerId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int? overdueDays = null,
            [FromQuery] string? paymentStatus = null)
        {
            try
            {
                // ✅ SỬA: Validate pagination parameters
                if (pageSize <= 0) pageSize = 10;
                if (pageNumber <= 0) pageNumber = 1;
                if (pageSize > 100) pageSize = 100; // Giới hạn max để tránh performance issues

                // ✅ SỬA: Validate date range
                if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
                {
                    return BadRequest(ApiResponse<List<AccountsReceivableDto>>.ErrorResult("Ngày bắt đầu không được lớn hơn ngày kết thúc"));
                }

                var today = DateTime.Now.Date;
                var receivables = new List<AccountsReceivableDto>();

                // ✅ OPTIMIZED: Build queries trước
                var invoiceQuery = _context.Invoices
                    .AsNoTracking() // ✅ OPTIMIZED: Read-only query, không cần tracking
                    .Include(i => i.Customer)
                    .Where(i => !i.IsDeleted 
                        && (i.PaymentStatus == "Unpaid" || i.PaymentStatus == "Partial")
                        && i.Status != "Cancelled"
                        && i.FinalAmount > 0
                        && (i.FinalAmount - i.PaidAmount) > 0); // ✅ OPTIMIZED: Filter RemainingAmount > 0 ở DB level

                if (customerId.HasValue)
                {
                    invoiceQuery = invoiceQuery.Where(i => i.CustomerId == customerId.Value);
                }

                if (fromDate.HasValue)
                {
                    invoiceQuery = invoiceQuery.Where(i => i.IssuedDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    invoiceQuery = invoiceQuery.Where(i => i.IssuedDate <= toDate.Value);
                }

                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    invoiceQuery = invoiceQuery.Where(i => i.PaymentStatus == paymentStatus);
                }

                // ✅ OPTIMIZED: Materialize subquery trước để tối ưu performance
                var serviceOrderIdsWithInvoiceTask = _context.Invoices
                    .AsNoTracking() // ✅ OPTIMIZED: Read-only query
                    .Where(i => !i.IsDeleted && i.Status != "Cancelled" && i.ServiceOrderId.HasValue)
                    .Select(i => i.ServiceOrderId!.Value)
                    .Distinct()
                    .ToListAsync();

                // ✅ OPTIMIZED: Project sang DTO ở database level (trừ OverdueDays tính sau)
                var invoiceReceivablesTask = invoiceQuery
                    .Select(i => new AccountsReceivableDto
                    {
                        Id = i.Id,
                        Type = "Invoice",
                        ReferenceId = i.Id,
                        ReferenceNumber = i.InvoiceNumber ?? string.Empty,
                        CustomerId = i.CustomerId ?? 0,
                        CustomerName = i.Customer != null ? i.Customer.Name : string.Empty,
                        CustomerPhone = i.Customer != null ? i.Customer.Phone : null,
                        CustomerEmail = i.Customer != null ? i.Customer.Email : null,
                        TotalAmount = i.FinalAmount,
                        PaidAmount = i.PaidAmount,
                        RemainingAmount = (i.FinalAmount - i.PaidAmount) > 0 ? (i.FinalAmount - i.PaidAmount) : 0,
                        PaymentStatus = i.PaymentStatus ?? "Unpaid",
                        IssuedDate = i.IssuedDate,
                        DueDate = i.DueDate ?? (i.IssuedDate.HasValue ? i.IssuedDate.Value.AddDays(30) : today),
                        OverdueDays = 0, // ✅ OPTIMIZED: Sẽ tính sau khi projection (vì đã filter RemainingAmount > 0 ở DB level)
                        LastPaymentDate = i.PaidDate,
                        Notes = i.Notes
                    })
                    .ToListAsync();

                // ✅ OPTIMIZED: Execute queries in parallel
                await Task.WhenAll(invoiceReceivablesTask, serviceOrderIdsWithInvoiceTask);
                var invoiceReceivables = await invoiceReceivablesTask;
                var serviceOrderIdsWithInvoice = await serviceOrderIdsWithInvoiceTask;

                // ✅ OPTIMIZED: Tính OverdueDays sau projection (số lượng records đã giảm đáng kể nhờ filter ở DB level)
                foreach (var receivable in invoiceReceivables)
                {
                    if (receivable.DueDate.HasValue)
                    {
                        var overdueDaysCalc = (today - receivable.DueDate.Value.Date).Days;
                        receivable.OverdueDays = overdueDaysCalc < 0 ? 0 : overdueDaysCalc;
                    }
                }

                // ✅ OPTIMIZED: Filter overdueDays ở memory level (sau projection) nếu cần
                if (overdueDays.HasValue)
                {
                    invoiceReceivables = invoiceReceivables.Where(r => r.OverdueDays >= overdueDays.Value).ToList();
                }

                receivables.AddRange(invoiceReceivables);

                // ✅ OPTIMIZED: Get unpaid/partial ServiceOrders (without Invoice) với projection ở database level
                var serviceOrderQuery = _context.ServiceOrders
                    .AsNoTracking() // ✅ OPTIMIZED: Read-only query, không cần tracking
                    .Include(so => so.Customer)
                    .Where(so => !so.IsDeleted 
                        && (so.PaymentStatus == "Unpaid" || so.PaymentStatus == "Partial")
                        && so.Status != "Cancelled"
                        && so.FinalAmount > 0
                        && (so.FinalAmount - so.AmountPaid) > 0 // ✅ OPTIMIZED: Filter RemainingAmount > 0 ở DB level
                        && !serviceOrderIdsWithInvoice.Contains(so.Id)); // ✅ OPTIMIZED: Dùng Contains với materialized list

                if (customerId.HasValue)
                {
                    serviceOrderQuery = serviceOrderQuery.Where(so => so.CustomerId == customerId.Value);
                }

                if (fromDate.HasValue)
                {
                    serviceOrderQuery = serviceOrderQuery.Where(so => so.OrderDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    serviceOrderQuery = serviceOrderQuery.Where(so => so.OrderDate <= toDate.Value);
                }

                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    serviceOrderQuery = serviceOrderQuery.Where(so => so.PaymentStatus == paymentStatus);
                }

                // ✅ OPTIMIZED: Project sang DTO ở database level (trừ OverdueDays tính sau)
                var serviceOrderReceivables = await serviceOrderQuery
                    .Select(so => new AccountsReceivableDto
                    {
                        Id = so.Id + 1000000, // Offset để tránh trùng với Invoice IDs
                        Type = "ServiceOrder",
                        ReferenceId = so.Id,
                        ReferenceNumber = so.OrderNumber,
                        CustomerId = so.CustomerId,
                        CustomerName = so.Customer != null ? so.Customer.Name : string.Empty,
                        CustomerPhone = so.Customer != null ? so.Customer.Phone : null,
                        CustomerEmail = so.Customer != null ? so.Customer.Email : null,
                        TotalAmount = so.FinalAmount,
                        PaidAmount = so.AmountPaid,
                        RemainingAmount = (so.FinalAmount - so.AmountPaid) > 0 ? (so.FinalAmount - so.AmountPaid) : 0,
                        PaymentStatus = so.PaymentStatus ?? "Unpaid",
                        IssuedDate = so.OrderDate,
                        DueDate = so.OrderDate.AddDays(30),
                        OverdueDays = 0, // ✅ OPTIMIZED: Sẽ tính sau khi projection (vì đã filter RemainingAmount > 0 ở DB level)
                        LastPaymentDate = null, // ServiceOrder doesn't have paid date
                        Notes = so.Notes
                    })
                    .ToListAsync();

                // ✅ OPTIMIZED: Tính OverdueDays sau projection (số lượng records đã giảm đáng kể nhờ filter ở DB level)
                foreach (var receivable in serviceOrderReceivables)
                {
                    if (receivable.DueDate.HasValue)
                    {
                        var overdueDaysCalc = (today - receivable.DueDate.Value.Date).Days;
                        receivable.OverdueDays = overdueDaysCalc < 0 ? 0 : overdueDaysCalc;
                    }
                }

                // ✅ OPTIMIZED: Filter overdueDays ở memory level (sau projection) nếu cần
                if (overdueDays.HasValue)
                {
                    serviceOrderReceivables = serviceOrderReceivables.Where(r => r.OverdueDays >= overdueDays.Value).ToList();
                }

                receivables.AddRange(serviceOrderReceivables);

                // ✅ OPTIMIZED: Sort by overdue days descending
                receivables = receivables.OrderByDescending(r => r.OverdueDays).ToList();

                // ✅ OPTIMIZED: Pagination ở memory level (sau khi đã filter tất cả)
                var totalCount = receivables.Count;
                var pagedReceivables = receivables
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                var pagedResponse = new PagedResponse<AccountsReceivableDto>
                {
                    Data = pagedReceivables,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return Ok(pagedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting accounts receivable");
                return StatusCode(500, ApiResponse<List<AccountsReceivableDto>>.ErrorResult("Lỗi khi lấy danh sách công nợ phải thu", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 4.3.2.1: Lấy thống kê công nợ phải thu
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<ApiResponse<AccountsReceivableSummaryDto>>> GetSummary(
            [FromQuery] int? customerId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                // ✅ SỬA: Validate date range
                if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
                {
                    return BadRequest(ApiResponse<AccountsReceivableSummaryDto>.ErrorResult("Ngày bắt đầu không được lớn hơn ngày kết thúc"));
                }

                var today = DateTime.Now.Date;

                // ✅ OPTIMIZED: Build queries trước
                var invoiceQuery = _context.Invoices
                    .AsNoTracking() // ✅ OPTIMIZED: Read-only query, không cần tracking
                    .Include(i => i.Customer)
                    .Where(i => !i.IsDeleted 
                        && (i.PaymentStatus == "Unpaid" || i.PaymentStatus == "Partial")
                        && i.Status != "Cancelled"
                        && i.FinalAmount > 0
                        && (i.FinalAmount - i.PaidAmount) > 0); // ✅ OPTIMIZED: Filter RemainingAmount > 0 ở DB level

                if (customerId.HasValue)
                {
                    invoiceQuery = invoiceQuery.Where(i => i.CustomerId == customerId.Value);
                }

                if (fromDate.HasValue)
                {
                    invoiceQuery = invoiceQuery.Where(i => i.IssuedDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    invoiceQuery = invoiceQuery.Where(i => i.IssuedDate <= toDate.Value);
                }

                // ✅ OPTIMIZED: Materialize subquery trước để tối ưu performance
                var serviceOrderIdsWithInvoiceTask = _context.Invoices
                    .AsNoTracking() // ✅ OPTIMIZED: Read-only query
                    .Where(i => !i.IsDeleted && i.Status != "Cancelled" && i.ServiceOrderId.HasValue)
                    .Select(i => i.ServiceOrderId!.Value)
                    .Distinct()
                    .ToListAsync();

                // ✅ OPTIMIZED: Project sang DTO ở database level (trừ OverdueDays tính sau)
                var invoiceReceivablesTask = invoiceQuery
                    .Select(i => new AccountsReceivableDto
                    {
                        Id = i.Id,
                        Type = "Invoice",
                        ReferenceId = i.Id,
                        ReferenceNumber = i.InvoiceNumber ?? string.Empty,
                        CustomerId = i.CustomerId ?? 0,
                        CustomerName = i.Customer != null ? i.Customer.Name : string.Empty,
                        CustomerPhone = i.Customer != null ? i.Customer.Phone : null,
                        CustomerEmail = i.Customer != null ? i.Customer.Email : null,
                        TotalAmount = i.FinalAmount,
                        PaidAmount = i.PaidAmount,
                        RemainingAmount = (i.FinalAmount - i.PaidAmount) > 0 ? (i.FinalAmount - i.PaidAmount) : 0,
                        PaymentStatus = i.PaymentStatus ?? "Unpaid",
                        IssuedDate = i.IssuedDate,
                        DueDate = i.DueDate ?? (i.IssuedDate.HasValue ? i.IssuedDate.Value.AddDays(30) : today),
                        OverdueDays = 0, // ✅ OPTIMIZED: Sẽ tính sau khi projection (tránh dùng EF.Functions.DateDiffDay không support MySQL)
                        LastPaymentDate = i.PaidDate,
                        Notes = i.Notes
                    })
                    .ToListAsync();

                // ✅ OPTIMIZED: Execute queries in parallel
                await Task.WhenAll(invoiceReceivablesTask, serviceOrderIdsWithInvoiceTask);
                var invoiceReceivables = await invoiceReceivablesTask;
                var serviceOrderIdsWithInvoice = await serviceOrderIdsWithInvoiceTask;

                // ✅ OPTIMIZED: Tính OverdueDays sau projection (số lượng records đã giảm đáng kể nhờ filter ở DB level)
                foreach (var receivable in invoiceReceivables)
                {
                    if (receivable.DueDate.HasValue)
                    {
                        var overdueDaysCalc = (today - receivable.DueDate.Value.Date).Days;
                        receivable.OverdueDays = overdueDaysCalc < 0 ? 0 : overdueDaysCalc;
                    }
                }

                // ✅ OPTIMIZED: Get unpaid/partial ServiceOrders (without Invoice) với projection ở database level
                var serviceOrderQuery = _context.ServiceOrders
                    .AsNoTracking() // ✅ OPTIMIZED: Read-only query, không cần tracking
                    .Include(so => so.Customer)
                    .Where(so => !so.IsDeleted 
                        && (so.PaymentStatus == "Unpaid" || so.PaymentStatus == "Partial")
                        && so.Status != "Cancelled"
                        && so.FinalAmount > 0
                        && (so.FinalAmount - so.AmountPaid) > 0 // ✅ OPTIMIZED: Filter RemainingAmount > 0 ở DB level
                        && !serviceOrderIdsWithInvoice.Contains(so.Id)); // ✅ OPTIMIZED: Dùng Contains với materialized list

                if (customerId.HasValue)
                {
                    serviceOrderQuery = serviceOrderQuery.Where(so => so.CustomerId == customerId.Value);
                }

                if (fromDate.HasValue)
                {
                    serviceOrderQuery = serviceOrderQuery.Where(so => so.OrderDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    serviceOrderQuery = serviceOrderQuery.Where(so => so.OrderDate <= toDate.Value);
                }

                // ✅ OPTIMIZED: Project sang DTO ở database level (trừ OverdueDays tính sau)
                var serviceOrderReceivables = await serviceOrderQuery
                    .Select(so => new AccountsReceivableDto
                    {
                        Id = so.Id + 1000000, // Offset để tránh trùng với Invoice IDs
                        Type = "ServiceOrder",
                        ReferenceId = so.Id,
                        ReferenceNumber = so.OrderNumber,
                        CustomerId = so.CustomerId,
                        CustomerName = so.Customer != null ? so.Customer.Name : string.Empty,
                        CustomerPhone = so.Customer != null ? so.Customer.Phone : null,
                        CustomerEmail = so.Customer != null ? so.Customer.Email : null,
                        TotalAmount = so.FinalAmount,
                        PaidAmount = so.AmountPaid,
                        RemainingAmount = (so.FinalAmount - so.AmountPaid) > 0 ? (so.FinalAmount - so.AmountPaid) : 0,
                        PaymentStatus = so.PaymentStatus ?? "Unpaid",
                        IssuedDate = so.OrderDate,
                        DueDate = so.OrderDate.AddDays(30),
                        OverdueDays = 0, // ✅ OPTIMIZED: Sẽ tính sau khi projection (vì đã filter RemainingAmount > 0 ở DB level)
                        LastPaymentDate = null, // ServiceOrder doesn't have paid date
                        Notes = so.Notes
                    })
                    .ToListAsync();

                // ✅ OPTIMIZED: Tính OverdueDays sau projection (số lượng records đã giảm đáng kể nhờ filter ở DB level)
                foreach (var receivable in serviceOrderReceivables)
                {
                    if (receivable.DueDate.HasValue)
                    {
                        var overdueDaysCalc = (today - receivable.DueDate.Value.Date).Days;
                        receivable.OverdueDays = overdueDaysCalc < 0 ? 0 : overdueDaysCalc;
                    }
                }

                var allReceivables = invoiceReceivables.Concat(serviceOrderReceivables).ToList();

                // ✅ OPTIMIZED: Tính toán metrics một lần để tránh multiple iterations
                if (!allReceivables.Any())
                {
                    return Ok(ApiResponse<AccountsReceivableSummaryDto>.SuccessResult(new AccountsReceivableSummaryDto
                    {
                        TotalReceivable = 0,
                        OverdueReceivable = 0,
                        Overdue30Days = 0,
                        Overdue60Days = 0,
                        Overdue90Days = 0,
                        TotalCount = 0,
                        OverdueCount = 0,
                        ByCustomer = new List<CustomerReceivableDto>()
                    }));
                }

                var overdueReceivables = allReceivables.Where(r => r.OverdueDays > 0).ToList();
                var overdue30Days = overdueReceivables.Where(r => r.OverdueDays <= 30).ToList();
                var overdue60Days = overdueReceivables.Where(r => r.OverdueDays > 30 && r.OverdueDays <= 60).ToList();
                var overdue90Days = overdueReceivables.Where(r => r.OverdueDays > 60).ToList();

                var summary = new AccountsReceivableSummaryDto
                {
                    TotalReceivable = allReceivables.Sum(r => r.RemainingAmount),
                    OverdueReceivable = overdueReceivables.Sum(r => r.RemainingAmount),
                    Overdue30Days = overdue30Days.Sum(r => r.RemainingAmount),
                    Overdue60Days = overdue60Days.Sum(r => r.RemainingAmount),
                    Overdue90Days = overdue90Days.Sum(r => r.RemainingAmount),
                    TotalCount = allReceivables.Count,
                    OverdueCount = overdueReceivables.Count,
                    ByCustomer = allReceivables
                        .GroupBy(r => new { r.CustomerId, r.CustomerName, r.CustomerPhone })
                        .Select(g => new CustomerReceivableDto
                        {
                            CustomerId = g.Key.CustomerId,
                            CustomerName = g.Key.CustomerName,
                            CustomerPhone = g.Key.CustomerPhone,
                            TotalReceivable = g.Sum(r => r.RemainingAmount),
                            InvoiceCount = g.Count(),
                            OverdueCount = g.Count(r => r.OverdueDays > 0)
                        })
                        .OrderByDescending(c => c.TotalReceivable)
                        .ToList()
                };

                return Ok(ApiResponse<AccountsReceivableSummaryDto>.SuccessResult(summary));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting accounts receivable summary");
                return StatusCode(500, ApiResponse<AccountsReceivableSummaryDto>.ErrorResult("Lỗi khi lấy thống kê công nợ phải thu", ex.Message));
            }
        }
    }
}

