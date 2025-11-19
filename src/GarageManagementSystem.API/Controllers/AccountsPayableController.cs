using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Helpers;
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
    /// ✅ 4.3.2.3: Controller quản lý Accounts Payable (Công nợ Phải Trả)
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsPayableController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AccountsPayableController> _logger;
        private readonly GarageManagementSystem.Infrastructure.Data.GarageDbContext _context;

        public AccountsPayableController(
            IUnitOfWork unitOfWork,
            ILogger<AccountsPayableController> logger,
            GarageManagementSystem.Infrastructure.Data.GarageDbContext context)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _context = context;
        }


        /// <summary>
        /// ✅ 4.3.2.3: Lấy danh sách công nợ phải trả
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResponse<AccountsPayableDto>>> GetAccountsPayable(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? supplierId = null,
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
                    return BadRequest(ApiResponse<List<AccountsPayableDto>>.ErrorResult("Ngày bắt đầu không được lớn hơn ngày kết thúc"));
                }

                var today = DateTime.Now.Date;

                // ✅ OPTIMIZED: Get received PurchaseOrders với projection ở database level
                // Chỉ lấy PO đã nhận hàng (Status = "Received") và còn nợ
                var purchaseOrderQuery = _context.PurchaseOrders
                    .AsNoTracking() // ✅ OPTIMIZED: Read-only query, không cần tracking
                    .Include(po => po.Supplier)
                    .Where(po => !po.IsDeleted 
                        && po.Status == "Received" // Chỉ lấy PO đã nhận hàng
                        && po.TotalAmount > 0); // ✅ OPTIMIZED: Filter TotalAmount > 0 ở DB level

                if (supplierId.HasValue)
                {
                    purchaseOrderQuery = purchaseOrderQuery.Where(po => po.SupplierId == supplierId.Value);
                }

                if (fromDate.HasValue)
                {
                    purchaseOrderQuery = purchaseOrderQuery.Where(po => po.OrderDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    purchaseOrderQuery = purchaseOrderQuery.Where(po => po.OrderDate <= toDate.Value);
                }

                // ✅ OPTIMIZED: Tính PaidAmount ở DB level bằng LEFT JOIN với subquery
                var payablesWithPaidAmountQuery = purchaseOrderQuery
                    .GroupJoin(
                        _context.FinancialTransactions
                            .AsNoTracking()
                            .Where(ft => !ft.IsDeleted
                                && ft.TransactionType == "Expense"
                                && ft.RelatedEntity == "PurchaseOrder"
                                && ft.RelatedEntityId.HasValue),
                        po => po.Id,
                        ft => ft.RelatedEntityId!.Value,
                        (po, ftGroup) => new
                        {
                            PurchaseOrder = po,
                            PaidAmount = ftGroup.Sum(ft => (decimal?)ft.Amount) ?? 0,
                            LastPaymentDate = ftGroup.Max(ft => (DateTime?)ft.TransactionDate)
                        })
                    .Select(x => new
                    {
                        x.PurchaseOrder,
                        x.PaidAmount,
                        x.LastPaymentDate,
                        RemainingAmount = (x.PurchaseOrder.TotalAmount - x.PaidAmount) > 0 
                            ? (x.PurchaseOrder.TotalAmount - x.PaidAmount) 
                            : 0
                    })
                    .Where(x => x.RemainingAmount > 0); // ✅ OPTIMIZED: Filter RemainingAmount > 0 ở DB level

                // ✅ OPTIMIZED: Filter paymentStatus ở DB level nếu có thể
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    if (paymentStatus == "Unpaid")
                    {
                        payablesWithPaidAmountQuery = payablesWithPaidAmountQuery.Where(x => x.PaidAmount == 0);
                    }
                    else if (paymentStatus == "Partial")
                    {
                        payablesWithPaidAmountQuery = payablesWithPaidAmountQuery.Where(x => x.PaidAmount > 0 && x.PaidAmount < x.PurchaseOrder.TotalAmount);
                    }
                }

                // ✅ OPTIMIZED: Tính OverdueDays và filter ở DB level (không load tất cả vào memory)
                List<AccountsPayableDto> payables;

                // ✅ 4.3.2.3: Tính OverdueDays ở DB level sử dụng CreditDays
                // Tính DueDate và OverdueDays trong SELECT để có thể filter và sort
                var payablesWithOverdueQuery = payablesWithPaidAmountQuery
                    .Select(x => new
                    {
                        PurchaseOrder = x.PurchaseOrder,
                        PaidAmount = x.PaidAmount,
                        RemainingAmount = x.RemainingAmount,
                        LastPaymentDate = x.LastPaymentDate,
                        // ✅ Tính DueDate ở DB level
                        DueDateBase = x.PurchaseOrder.ReceivedDate ?? x.PurchaseOrder.OrderDate,
                        CreditDays = x.PurchaseOrder.CreditDays ?? 30, // Default 30 nếu null
                        // ✅ Tính OverdueDays ở DB level
                        // DueDate = DueDateBase + CreditDays (nếu CreditDays > 0)
                        // Note: Phải tính trong SELECT để có thể sort, nhưng cũng cần replicate trong WHERE để filter
                        OverdueDays = EF.Functions.DateDiffDay(
                            (x.PurchaseOrder.CreditDays == null || x.PurchaseOrder.CreditDays <= 0)
                                ? (x.PurchaseOrder.ReceivedDate ?? x.PurchaseOrder.OrderDate)
                                : (x.PurchaseOrder.ReceivedDate ?? x.PurchaseOrder.OrderDate).AddDays((double)(x.PurchaseOrder.CreditDays ?? 30)),
                            today) <= 0
                            ? 0
                            : EF.Functions.DateDiffDay(
                                (x.PurchaseOrder.CreditDays == null || x.PurchaseOrder.CreditDays <= 0)
                                    ? (x.PurchaseOrder.ReceivedDate ?? x.PurchaseOrder.OrderDate)
                                    : (x.PurchaseOrder.ReceivedDate ?? x.PurchaseOrder.OrderDate).AddDays((double)(x.PurchaseOrder.CreditDays ?? 30)),
                                today)
                    });

                // ✅ OPTIMIZED: Filter OverdueDays ở DB level nếu có filter
                // Replicate logic tính OverdueDays trực tiếp trong WHERE clause để đảm bảo EF Core translate đúng
                if (overdueDays.HasValue)
                {
                    payablesWithOverdueQuery = payablesWithOverdueQuery
                        .Where(x => 
                            EF.Functions.DateDiffDay(
                                (x.PurchaseOrder.CreditDays == null || x.PurchaseOrder.CreditDays <= 0)
                                    ? (x.PurchaseOrder.ReceivedDate ?? x.PurchaseOrder.OrderDate)
                                    : (x.PurchaseOrder.ReceivedDate ?? x.PurchaseOrder.OrderDate).AddDays((double)(x.PurchaseOrder.CreditDays ?? 30)),
                                today) > 0 && // ✅ Chỉ lấy records đã quá hạn (OverdueDays > 0)
                            EF.Functions.DateDiffDay(
                                (x.PurchaseOrder.CreditDays == null || x.PurchaseOrder.CreditDays <= 0)
                                    ? (x.PurchaseOrder.ReceivedDate ?? x.PurchaseOrder.OrderDate)
                                    : (x.PurchaseOrder.ReceivedDate ?? x.PurchaseOrder.OrderDate).AddDays((double)(x.PurchaseOrder.CreditDays ?? 30)),
                                today) >= overdueDays.Value);
                }

                // ✅ OPTIMIZED: Count và paginate (anonymous type không thể dùng extension method, phải dùng cách cũ)
                var totalCountTask = payablesWithOverdueQuery.CountAsync();
                var orderedQuery = payablesWithOverdueQuery
                    .OrderByDescending(x => x.OverdueDays)
                    .ThenByDescending(x => x.PurchaseOrder.OrderDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                
                await Task.WhenAll(totalCountTask, orderedQuery);
                var totalCount = await totalCountTask;
                var result = await orderedQuery;

                // ✅ Map sang DTO
                payables = result.Select(x =>
                {
                    var dueDateBase = x.DueDateBase;
                    var creditDays = x.CreditDays;
                    DateTime? dueDate;

                    if (creditDays <= 0)
                    {
                        dueDate = dueDateBase; // COD hoặc Prepaid
                    }
                    else
                    {
                        dueDate = dueDateBase.AddDays(creditDays);
                    }

                    return new AccountsPayableDto
                    {
                        Id = x.PurchaseOrder.Id,
                        Type = "PurchaseOrder",
                        ReferenceId = x.PurchaseOrder.Id,
                        ReferenceNumber = x.PurchaseOrder.OrderNumber,
                        SupplierId = x.PurchaseOrder.SupplierId,
                        SupplierName = x.PurchaseOrder.Supplier != null ? x.PurchaseOrder.Supplier.SupplierName : string.Empty,
                        SupplierPhone = x.PurchaseOrder.Supplier != null ? x.PurchaseOrder.Supplier.Phone : null,
                        SupplierEmail = x.PurchaseOrder.Supplier != null ? x.PurchaseOrder.Supplier.Email : null,
                        TotalAmount = x.PurchaseOrder.TotalAmount,
                        PaidAmount = x.PaidAmount,
                        RemainingAmount = x.RemainingAmount,
                        PaymentStatus = x.PaidAmount == 0 ? "Unpaid" : "Partial",
                        OrderDate = x.PurchaseOrder.OrderDate,
                        ReceivedDate = x.PurchaseOrder.ReceivedDate,
                        DueDate = dueDate,
                        OverdueDays = x.OverdueDays,
                        PaymentTerms = x.PurchaseOrder.PaymentTerms,
                        CreditDays = x.PurchaseOrder.CreditDays,
                        LastPaymentDate = x.LastPaymentDate,
                        Notes = x.PurchaseOrder.Notes
                    };
                }).ToList();

                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                var pagedResponse = new PagedResponse<AccountsPayableDto>
                {
                    Data = payables,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return Ok(pagedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting accounts payable");
                return StatusCode(500, ApiResponse<List<AccountsPayableDto>>.ErrorResult("Lỗi khi lấy danh sách công nợ phải trả", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 4.3.2.3: Lấy thống kê công nợ phải trả
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<ApiResponse<AccountsPayableSummaryDto>>> GetSummary(
            [FromQuery] int? supplierId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                // ✅ SỬA: Validate date range
                if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
                {
                    return BadRequest(ApiResponse<AccountsPayableSummaryDto>.ErrorResult("Ngày bắt đầu không được lớn hơn ngày kết thúc"));
                }

                var today = DateTime.Now.Date;

                // ✅ OPTIMIZED: Get received PurchaseOrders với projection ở database level
                var purchaseOrderQuery = _context.PurchaseOrders
                    .AsNoTracking() // ✅ OPTIMIZED: Read-only query, không cần tracking
                    .Include(po => po.Supplier)
                    .Where(po => !po.IsDeleted 
                        && po.Status == "Received" // Chỉ lấy PO đã nhận hàng
                        && po.TotalAmount > 0); // ✅ OPTIMIZED: Filter TotalAmount > 0 ở DB level

                if (supplierId.HasValue)
                {
                    purchaseOrderQuery = purchaseOrderQuery.Where(po => po.SupplierId == supplierId.Value);
                }

                if (fromDate.HasValue)
                {
                    purchaseOrderQuery = purchaseOrderQuery.Where(po => po.OrderDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    purchaseOrderQuery = purchaseOrderQuery.Where(po => po.OrderDate <= toDate.Value);
                }

                var purchaseOrderIds = await purchaseOrderQuery
                    .Select(po => po.Id)
                    .ToListAsync();

                // ✅ OPTIMIZED: Return early nếu không có PO
                if (!purchaseOrderIds.Any())
                {
                    return Ok(ApiResponse<AccountsPayableSummaryDto>.SuccessResult(new AccountsPayableSummaryDto
                    {
                        TotalPayable = 0,
                        OverduePayable = 0,
                        Overdue30Days = 0,
                        Overdue60Days = 0,
                        Overdue90Days = 0,
                        TotalCount = 0,
                        OverdueCount = 0,
                        BySupplier = new List<SupplierPayableDto>()
                    }));
                }

                // ✅ OPTIMIZED: Get paid amounts và last payment dates từ FinancialTransactions
                var financialTransactionsQuery = _context.FinancialTransactions
                    .AsNoTracking() // ✅ OPTIMIZED: Read-only query
                    .Where(ft => !ft.IsDeleted
                        && ft.TransactionType == "Expense"
                        && ft.RelatedEntity == "PurchaseOrder"
                        && ft.RelatedEntityId.HasValue
                        && purchaseOrderIds.Contains(ft.RelatedEntityId.Value));

                // ✅ OPTIMIZED: Get paid amounts grouped by PurchaseOrderId
                var paidAmountsQuery = financialTransactionsQuery
                    .GroupBy(ft => ft.RelatedEntityId!.Value)
                    .Select(g => new
                    {
                        PurchaseOrderId = g.Key,
                        PaidAmount = g.Sum(ft => ft.Amount)
                    });

                // ✅ OPTIMIZED: Get last payment dates grouped by PurchaseOrderId
                var lastPaymentDatesQuery = financialTransactionsQuery
                    .GroupBy(ft => ft.RelatedEntityId!.Value)
                    .Select(g => new
                    {
                        PurchaseOrderId = g.Key,
                        LastPaymentDate = g.Max(ft => (DateTime?)ft.TransactionDate)
                    });

                var paidAmountsTask = paidAmountsQuery.ToListAsync();
                var lastPaymentDatesTask = lastPaymentDatesQuery.ToListAsync();

                // ✅ OPTIMIZED: Execute queries in parallel
                await Task.WhenAll(paidAmountsTask, lastPaymentDatesTask);
                var paidAmounts = await paidAmountsTask;
                var lastPaymentDates = await lastPaymentDatesTask;
                var paidAmountsDict = paidAmounts.ToDictionary(pa => pa.PurchaseOrderId, pa => pa.PaidAmount);
                var lastPaymentDatesDict = lastPaymentDates.ToDictionary(lpd => lpd.PurchaseOrderId, lpd => lpd.LastPaymentDate);

                // ✅ OPTIMIZED: Project sang DTO ở database level (trừ OverdueDays và PaidAmount tính sau)
                var purchaseOrderPayables = await purchaseOrderQuery
                    .Select(po => new AccountsPayableDto
                    {
                        Id = po.Id,
                        Type = "PurchaseOrder",
                        ReferenceId = po.Id,
                        ReferenceNumber = po.OrderNumber,
                        SupplierId = po.SupplierId,
                        SupplierName = po.Supplier != null ? po.Supplier.SupplierName : string.Empty,
                        SupplierPhone = po.Supplier != null ? po.Supplier.Phone : null,
                        SupplierEmail = po.Supplier != null ? po.Supplier.Email : null,
                        TotalAmount = po.TotalAmount,
                        PaidAmount = 0, // ✅ OPTIMIZED: Sẽ tính sau từ FinancialTransactions
                        RemainingAmount = 0, // ✅ OPTIMIZED: Sẽ tính sau
                        PaymentStatus = "Unpaid", // ✅ OPTIMIZED: Sẽ tính sau
                        OrderDate = po.OrderDate,
                        ReceivedDate = po.ReceivedDate,
                        DueDate = null, // ✅ OPTIMIZED: Sẽ tính sau từ PaymentTerms
                        OverdueDays = 0, // ✅ OPTIMIZED: Sẽ tính sau khi projection
                        PaymentTerms = po.PaymentTerms,
                        LastPaymentDate = null, // ✅ OPTIMIZED: Sẽ tính sau từ FinancialTransactions
                        Notes = po.Notes,
                        // ✅ 4.3.2.3: Include CreditDays để dùng trực tiếp (không cần parse)
                        CreditDays = po.CreditDays ?? PaymentTermsHelper.ParsePaymentTermsDays(po.PaymentTerms)
                    })
                    .ToListAsync();

                // ✅ OPTIMIZED: Tính PaidAmount, RemainingAmount, PaymentStatus, DueDate, và OverdueDays sau projection
                var allPayables = new List<AccountsPayableDto>();
                foreach (var payable in purchaseOrderPayables)
                {
                    // Get paid amount từ FinancialTransactions
                    var paidAmount = paidAmountsDict.ContainsKey(payable.ReferenceId) 
                        ? paidAmountsDict[payable.ReferenceId] 
                        : 0;

                    payable.PaidAmount = paidAmount;
                    payable.RemainingAmount = (payable.TotalAmount - paidAmount) > 0 ? (payable.TotalAmount - paidAmount) : 0;

                    // Skip nếu đã trả hết (RemainingAmount = 0)
                    if (payable.RemainingAmount <= 0)
                    {
                        continue;
                    }

                    // ✅ OPTIMIZED: Calculate PaymentStatus (chỉ cần Unpaid hoặc Partial vì đã filter RemainingAmount > 0)
                    payable.PaymentStatus = paidAmount == 0 ? "Unpaid" : "Partial";

                    // ✅ 4.3.2.3: Calculate DueDate sử dụng CreditDays (đã có sẵn, không cần parse)
                    var dueDateBase = payable.ReceivedDate ?? payable.OrderDate ?? today;
                    var creditDays = payable.CreditDays ?? 30; // Default 30 nếu null
                    
                    // ✅ Handle special cases: Prepaid (-1) và COD (0)
                    if (creditDays < 0)
                    {
                        // Prepaid - DueDate = ReceivedDate (hoặc OrderDate) vì đã trả trước
                        payable.DueDate = dueDateBase;
                    }
                    else if (creditDays == 0)
                    {
                        // COD - DueDate = ReceivedDate (thanh toán ngay khi nhận hàng)
                        payable.DueDate = dueDateBase;
                    }
                    else
                    {
                        payable.DueDate = dueDateBase.AddDays(creditDays);
                    }

                    // Calculate OverdueDays
                    if (payable.DueDate.HasValue)
                    {
                        var overdueDaysCalc = (today - payable.DueDate.Value.Date).Days;
                        payable.OverdueDays = overdueDaysCalc < 0 ? 0 : overdueDaysCalc;
                    }

                    // ✅ OPTIMIZED: Get LastPaymentDate từ dictionary (đã load sẵn)
                    payable.LastPaymentDate = lastPaymentDatesDict.ContainsKey(payable.ReferenceId) 
                        ? lastPaymentDatesDict[payable.ReferenceId] 
                        : null;

                    allPayables.Add(payable);
                }

                // ✅ OPTIMIZED: Tính toán metrics một lần để tránh multiple iterations
                if (!allPayables.Any())
                {
                    return Ok(ApiResponse<AccountsPayableSummaryDto>.SuccessResult(new AccountsPayableSummaryDto
                    {
                        TotalPayable = 0,
                        OverduePayable = 0,
                        Overdue30Days = 0,
                        Overdue60Days = 0,
                        Overdue90Days = 0,
                        TotalCount = 0,
                        OverdueCount = 0,
                        BySupplier = new List<SupplierPayableDto>()
                    }));
                }

                var overduePayables = allPayables.Where(p => p.OverdueDays > 0).ToList();
                var overdue30Days = overduePayables.Where(p => p.OverdueDays <= 30).ToList();
                var overdue60Days = overduePayables.Where(p => p.OverdueDays > 30 && p.OverdueDays <= 60).ToList();
                var overdue90Days = overduePayables.Where(p => p.OverdueDays > 60).ToList();

                var summary = new AccountsPayableSummaryDto
                {
                    TotalPayable = allPayables.Sum(p => p.RemainingAmount),
                    OverduePayable = overduePayables.Sum(p => p.RemainingAmount),
                    Overdue30Days = overdue30Days.Sum(p => p.RemainingAmount),
                    Overdue60Days = overdue60Days.Sum(p => p.RemainingAmount),
                    Overdue90Days = overdue90Days.Sum(p => p.RemainingAmount),
                    TotalCount = allPayables.Count,
                    OverdueCount = overduePayables.Count,
                    BySupplier = allPayables
                        .GroupBy(p => new { p.SupplierId, p.SupplierName, p.SupplierPhone })
                        .Select(g => new SupplierPayableDto
                        {
                            SupplierId = g.Key.SupplierId,
                            SupplierName = g.Key.SupplierName,
                            SupplierPhone = g.Key.SupplierPhone,
                            TotalPayable = g.Sum(p => p.RemainingAmount),
                            PurchaseOrderCount = g.Count(),
                            OverdueCount = g.Count(p => p.OverdueDays > 0)
                        })
                        .OrderByDescending(s => s.TotalPayable)
                        .ToList()
                };

                return Ok(ApiResponse<AccountsPayableSummaryDto>.SuccessResult(summary));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting accounts payable summary");
                return StatusCode(500, ApiResponse<AccountsPayableSummaryDto>.ErrorResult("Lỗi khi lấy thống kê công nợ phải trả", ex.Message));
            }
        }
    }
}

