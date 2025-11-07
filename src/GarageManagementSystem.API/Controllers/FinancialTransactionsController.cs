using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
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

                // ✅ OPTIMIZED: Get total count ở database level (trước khi paginate)
                var totalCount = await query.CountAsync();
                
                // ✅ OPTIMIZED: Apply pagination ở database level với Skip/Take
                var pagedTransactions = await query
                    .OrderByDescending(t => t.TransactionDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                
                // Get employee names for transactions
                var employeeIds = pagedTransactions.Where(t => t.EmployeeId.HasValue).Select(t => t.EmployeeId.Value).Distinct().ToList();
                var employees = await _unitOfWork.Repository<Employee>().FindAsync(e => employeeIds.Contains(e.Id));
                var employeeDict = employees.ToDictionary(e => e.Id, e => e.Name);

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
                var transaction = await _unitOfWork.Repository<FinancialTransaction>().GetByIdAsync(id);
                if (transaction == null)
                    return NotFound(ApiResponse<FinancialTransactionDto>.ErrorResult("Financial transaction not found"));

                var transactionDto = _mapper.Map<FinancialTransactionDto>(transaction);
                
                // Get employee name if available
                if (transaction.EmployeeId.HasValue)
                {
                    var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(transaction.EmployeeId.Value);
                    if (employee != null)
                    {
                        transactionDto.EmployeeName = employee.Name;
                    }
                }
                
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
                
                // ✅ OPTIMIZED: Use CountAsync thay vì GetAllAsync().Count()
                var count = await _unitOfWork.Repository<FinancialTransaction>().CountAsync();
                transaction.TransactionNumber = $"FT-{DateTime.Now:yyyyMMdd}-{(count + 1):D4}";
                transaction.CreatedAt = DateTime.Now;
                
                await _unitOfWork.Repository<FinancialTransaction>().AddAsync(transaction);
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

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var transactions = await _unitOfWork.Repository<FinancialTransaction>().GetAllAsync();
                
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
    }
}

