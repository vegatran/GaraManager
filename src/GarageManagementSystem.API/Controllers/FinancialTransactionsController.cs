using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Core.Extensions;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FinancialTransactionsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<FinancialTransactionsController> _logger;
        private readonly GarageDbContext _context;

        public FinancialTransactionsController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<FinancialTransactionsController> logger, GarageDbContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<FinancialTransactionDto>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? transactionType = null,
            [FromQuery] string? category = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                // ✅ OPTIMIZED: Query ở database level thay vì load tất cả vào memory
                var query = _context.FinancialTransactions
                    .Where(t => !t.IsDeleted)
                    .AsQueryable();
                
                // Apply filters
                if (!string.IsNullOrEmpty(transactionType))
                    query = query.Where(t => t.TransactionType == transactionType);
                
                if (!string.IsNullOrEmpty(category))
                    query = query.Where(t => t.Category == category);
                
                if (fromDate.HasValue)
                    query = query.Where(t => t.TransactionDate >= fromDate.Value);
                
                if (toDate.HasValue)
                    query = query.Where(t => t.TransactionDate <= toDate.Value);

                // ✅ OPTIMIZED: Get paged results with total count - automatically chooses best method
                query = query.OrderByDescending(t => t.TransactionDate);
                var (pagedTransactions, totalCount) = await query.ToPagedListWithCountAsync(pageNumber, pageSize, _context);
                
                // Get employee names for transactions
                var employeeIds = pagedTransactions.Where(t => t.EmployeeId.HasValue).Select(t => t.EmployeeId.Value).Distinct().ToList();
                var employees = await _unitOfWork.Employees.FindAsync(e => employeeIds.Contains(e.Id));
                var employeeDict = employees.ToDictionary(e => e.Id, e => e.Name ?? string.Empty);

                // Map to DTOs and populate employee names
                var transactionDtos = pagedTransactions.Select(t => 
                {
                    var dto = _mapper.Map<FinancialTransactionDto>(t);
                    if (t.EmployeeId.HasValue && employeeDict.ContainsKey(t.EmployeeId.Value))
                    {
                        dto.EmployeeName = employeeDict[t.EmployeeId.Value];
                    }
                    return dto;
                }).ToList();
                
                return Ok(PagedResponse<FinancialTransactionDto>.CreateSuccessResult(
                    transactionDtos, pageNumber, pageSize, totalCount, "Financial transactions retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting financial transactions");
                return StatusCode(500, PagedResponse<FinancialTransactionDto>.CreateErrorResult("Error retrieving financial transactions"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<FinancialTransactionDto>>> GetById(int id)
        {
            try
            {
                var transaction = await _unitOfWork.FinancialTransactions.GetByIdAsync(id);
                if (transaction == null)
                    return NotFound(ApiResponse<FinancialTransactionDto>.ErrorResult("Financial transaction not found"));

                var transactionDto = _mapper.Map<FinancialTransactionDto>(transaction);
                
                // Get employee name if available
                if (transaction.EmployeeId.HasValue)
                {
                    var employee = await _unitOfWork.Employees.GetByIdAsync(transaction.EmployeeId.Value);
                    if (employee != null && !string.IsNullOrEmpty(employee.Name))
                    {
                        transactionDto.EmployeeName = employee.Name;
                    }
                }
                
                // ✅ 4.3.1.9: Load attachments
                var attachments = await _context.FinancialTransactionAttachments
                    .Where(a => a.FinancialTransactionId == id && !a.IsDeleted)
                    .Select(a => new FinancialTransactionAttachmentDto
                    {
                        Id = a.Id,
                        FinancialTransactionId = a.FinancialTransactionId,
                        FileName = a.FileName,
                        FilePath = a.FilePath,
                        FileType = a.FileType,
                        FileSize = a.FileSize,
                        MimeType = a.MimeType,
                        Description = a.Description,
                        UploadedAt = a.UploadedAt,
                        UploadedBy = a.UploadedBy
                    })
                    .ToListAsync();
                
                transactionDto.Attachments = attachments;
                
                return Ok(ApiResponse<FinancialTransactionDto>.SuccessResult(transactionDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting financial transaction");
                return StatusCode(500, ApiResponse<FinancialTransactionDto>.ErrorResult("Error retrieving financial transaction", ex.Message));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<FinancialTransactionDto>>> Create([FromBody] CreateFinancialTransactionDto createDto)
        {
            try
            {
                var transaction = _mapper.Map<FinancialTransaction>(createDto);
                
                // ✅ SỬA: Dùng repository method để generate transaction number (tránh race condition)
                transaction.TransactionNumber = await _unitOfWork.FinancialTransactions.GenerateTransactionNumberAsync();
                transaction.CreatedAt = DateTime.Now;
                
                await _unitOfWork.FinancialTransactions.AddAsync(transaction);
                await _unitOfWork.SaveChangesAsync();

                var transactionDto = _mapper.Map<FinancialTransactionDto>(transaction);
                return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, 
                    ApiResponse<FinancialTransactionDto>.SuccessResult(transactionDto, "Financial transaction created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating financial transaction");
                return StatusCode(500, ApiResponse<FinancialTransactionDto>.ErrorResult("Error creating financial transaction", ex.Message));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")] // ✅ SỬA: Chỉ Admin/Manager mới được sửa
        public async Task<ActionResult<ApiResponse<FinancialTransactionDto>>> Update(int id, [FromBody] CreateFinancialTransactionDto updateDto)
        {
            try
            {
                var transaction = await _unitOfWork.FinancialTransactions.GetByIdAsync(id);
                if (transaction == null)
                    return NotFound(ApiResponse<FinancialTransactionDto>.ErrorResult("Financial transaction not found"));

                // Map update fields (preserve TransactionNumber and CreatedAt)
                transaction.TransactionType = updateDto.TransactionType;
                transaction.Category = updateDto.Category;
                transaction.SubCategory = updateDto.SubCategory;
                transaction.Amount = updateDto.Amount;
                transaction.Currency = updateDto.Currency ?? "VND";
                transaction.TransactionDate = updateDto.TransactionDate;
                transaction.PaymentMethod = updateDto.PaymentMethod ?? string.Empty;
                transaction.ReferenceNumber = updateDto.ReferenceNumber;
                transaction.Description = updateDto.Description;
                transaction.RelatedEntity = updateDto.RelatedEntity;
                transaction.RelatedEntityId = updateDto.RelatedEntityId;
                transaction.EmployeeId = updateDto.EmployeeId;
                transaction.Notes = updateDto.Notes;
                transaction.UpdatedAt = DateTime.Now;

                await _unitOfWork.FinancialTransactions.UpdateAsync(transaction);
                await _unitOfWork.SaveChangesAsync();

                var transactionDto = _mapper.Map<FinancialTransactionDto>(transaction);
                
                // Get employee name if available
                if (transaction.EmployeeId.HasValue)
                {
                    var employee = await _unitOfWork.Employees.GetByIdAsync(transaction.EmployeeId.Value);
                    if (employee != null && !string.IsNullOrEmpty(employee.Name))
                    {
                        transactionDto.EmployeeName = employee.Name;
                    }
                }

                return Ok(ApiResponse<FinancialTransactionDto>.SuccessResult(transactionDto, "Financial transaction updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating financial transaction");
                return StatusCode(500, ApiResponse<FinancialTransactionDto>.ErrorResult("Error updating financial transaction", ex.Message));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")] // ✅ SỬA: Chỉ Admin/Manager mới được xóa
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            try
            {
                var transaction = await _unitOfWork.FinancialTransactions.GetByIdAsync(id);
                if (transaction == null)
                    return NotFound(ApiResponse<bool>.ErrorResult("Financial transaction not found"));

                // Soft delete
                transaction.IsDeleted = true;
                transaction.UpdatedAt = DateTime.Now;

                await _unitOfWork.FinancialTransactions.UpdateAsync(transaction);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResult(true, "Financial transaction deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting financial transaction");
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Error deleting financial transaction", ex.Message));
            }
        }

        [HttpGet("categories")]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetCategories()
        {
            try
            {
                // ✅ SỬA: Dùng repository thay vì direct context
                var allTransactions = await _unitOfWork.FinancialTransactions.GetAllAsync();
                var categories = allTransactions
                    .Select(t => t.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                return Ok(ApiResponse<List<string>>.SuccessResult(categories));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return StatusCode(500, ApiResponse<List<string>>.ErrorResult("Error retrieving categories", ex.Message));
            }
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var transactions = await _unitOfWork.FinancialTransactions.GetAllAsync();
                
                if (fromDate.HasValue)
                    transactions = transactions.Where(t => t.TransactionDate >= fromDate.Value);
                
                if (toDate.HasValue)
                    transactions = transactions.Where(t => t.TransactionDate <= toDate.Value);

                var summary = new
                {
                    TotalIncome = transactions.Where(t => t.TransactionType == "Income").Sum(t => t.Amount),
                    TotalExpense = transactions.Where(t => t.TransactionType == "Expense").Sum(t => t.Amount),
                    NetAmount = transactions.Where(t => t.TransactionType == "Income").Sum(t => t.Amount) - 
                                transactions.Where(t => t.TransactionType == "Expense").Sum(t => t.Amount),
                    TransactionCount = transactions.Count()
                };

                return Ok(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting financial summary");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy tổng kết tài chính" });
            }
        }

        /// <summary>
        /// ✅ 4.3.1.5: Lấy Sổ Quỹ Tiền Mặt (Cash Register)
        /// </summary>
        [HttpGet("cash-register")]
        public async Task<ActionResult<ApiResponse<PaymentMethodRegisterDto>>> GetCashRegister(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 100)
        {
            try
            {
                var result = await GetPaymentMethodRegisterAsync("Cash", fromDate, toDate, pageNumber, pageSize);
                return Ok(ApiResponse<PaymentMethodRegisterDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cash register");
                return StatusCode(500, ApiResponse<PaymentMethodRegisterDto>.ErrorResult("Lỗi khi lấy sổ quỹ tiền mặt", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 4.3.1.5: Lấy Sổ Quỹ Ngân Hàng (Bank Register)
        /// </summary>
        [HttpGet("bank-register")]
        public async Task<ActionResult<ApiResponse<PaymentMethodRegisterDto>>> GetBankRegister(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 100)
        {
            try
            {
                var result = await GetPaymentMethodRegisterAsync("Bank Transfer", fromDate, toDate, pageNumber, pageSize);
                return Ok(ApiResponse<PaymentMethodRegisterDto>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bank register");
                return StatusCode(500, ApiResponse<PaymentMethodRegisterDto>.ErrorResult("Lỗi khi lấy sổ quỹ ngân hàng", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 4.3.1.5: Lấy Sổ Quỹ theo PaymentMethod (generic method)
        /// </summary>
        private async Task<PaymentMethodRegisterDto> GetPaymentMethodRegisterAsync(
            string paymentMethod,
            DateTime? fromDate,
            DateTime? toDate,
            int pageNumber,
            int pageSize)
        {
            // Set default date range: nếu không có fromDate, lấy từ đầu tháng; nếu không có toDate, lấy đến hôm nay
            if (!fromDate.HasValue)
            {
                fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }
            
            if (!toDate.HasValue)
            {
                toDate = DateTime.Now.Date.AddDays(1).AddSeconds(-1); // End of today
            }

            // ✅ Tính số dư đầu kỳ: Tổng tất cả giao dịch trước fromDate với PaymentMethod này
            var openingBalanceQuery = _context.FinancialTransactions
                .Where(ft => !ft.IsDeleted 
                    && ft.PaymentMethod == paymentMethod
                    && ft.TransactionDate < fromDate.Value
                    && ft.Status == "Completed"); // Chỉ tính giao dịch đã hoàn thành

            var openingIncome = await openingBalanceQuery
                .Where(ft => ft.TransactionType == "Income")
                .SumAsync(ft => (decimal?)ft.Amount) ?? 0;

            var openingExpense = await openingBalanceQuery
                .Where(ft => ft.TransactionType == "Expense")
                .SumAsync(ft => (decimal?)ft.Amount) ?? 0;

            var openingBalance = openingIncome - openingExpense;

            // ✅ Lấy giao dịch trong kỳ
            var periodQuery = _context.FinancialTransactions
                .Where(ft => !ft.IsDeleted 
                    && ft.PaymentMethod == paymentMethod
                    && ft.TransactionDate >= fromDate.Value
                    && ft.TransactionDate <= toDate.Value);

            // Get total counts
            var totalCount = await periodQuery.CountAsync();
            var incomeCount = await periodQuery.Where(ft => ft.TransactionType == "Income").CountAsync();
            var expenseCount = await periodQuery.Where(ft => ft.TransactionType == "Expense").CountAsync();

            // Get totals
            var totalIncome = await periodQuery
                .Where(ft => ft.TransactionType == "Income" && ft.Status == "Completed")
                .SumAsync(ft => (decimal?)ft.Amount) ?? 0;

            var totalExpense = await periodQuery
                .Where(ft => ft.TransactionType == "Expense" && ft.Status == "Completed")
                .SumAsync(ft => (decimal?)ft.Amount) ?? 0;

            // ✅ Get paginated transactions
            var transactions = await periodQuery
                .Include(ft => ft.Employee)
                .OrderByDescending(ft => ft.TransactionDate)
                .ThenByDescending(ft => ft.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get employee names for transactions
            var employeeIds = transactions.Where(t => t.EmployeeId.HasValue).Select(t => t.EmployeeId.Value).Distinct().ToList();
            var employees = await _unitOfWork.Employees.FindAsync(e => employeeIds.Contains(e.Id));
            var employeeDict = employees.ToDictionary(e => e.Id, e => e.Name ?? string.Empty);

            // Map to DTOs
            var transactionDtos = transactions.Select(t =>
            {
                var dto = _mapper.Map<FinancialTransactionDto>(t);
                if (t.EmployeeId.HasValue && employeeDict.TryGetValue(t.EmployeeId.Value, out var empName))
                {
                    dto.EmployeeName = empName;
                }
                return dto;
            }).ToList();

            // Calculate closing balance
            var closingBalance = openingBalance + totalIncome - totalExpense;

            return new PaymentMethodRegisterDto
            {
                PaymentMethod = paymentMethod,
                FromDate = fromDate,
                ToDate = toDate,
                OpeningBalance = openingBalance,
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                ClosingBalance = closingBalance,
                TransactionCount = totalCount,
                IncomeCount = incomeCount,
                ExpenseCount = expenseCount,
                Transactions = transactionDtos
            };
        }
    }
}

