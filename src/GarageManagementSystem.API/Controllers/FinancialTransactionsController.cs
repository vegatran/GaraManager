using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FinancialTransactionsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FinancialTransactionsController> _logger;

        public FinancialTransactionsController(IUnitOfWork unitOfWork, ILogger<FinancialTransactionsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? transactionType = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var transactions = await _unitOfWork.Repository<FinancialTransaction>().GetAllAsync();
                
                if (!string.IsNullOrEmpty(transactionType))
                    transactions = transactions.Where(t => t.TransactionType == transactionType);
                
                if (fromDate.HasValue)
                    transactions = transactions.Where(t => t.TransactionDate >= fromDate.Value);
                
                if (toDate.HasValue)
                    transactions = transactions.Where(t => t.TransactionDate <= toDate.Value);

                var result = transactions.Select(t => new
                {
                    t.Id,
                    t.TransactionNumber,
                    t.TransactionDate,
                    t.TransactionType,
                    t.Category,
                    t.Amount,
                    t.Description,
                    t.RelatedEntity,
                    t.RelatedEntityId,
                    t.CreatedAt
                }).OrderByDescending(t => t.TransactionDate).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting financial transactions");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy giao dịch tài chính" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var transaction = await _unitOfWork.Repository<FinancialTransaction>().GetByIdAsync(id);
                if (transaction == null)
                    return NotFound(new { success = false, message = "Không tìm thấy giao dịch" });

                return Ok(new { success = true, data = transaction });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting financial transaction");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy giao dịch" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FinancialTransaction transaction)
        {
            try
            {
                // Generate transaction number
                var count = (await _unitOfWork.Repository<FinancialTransaction>().GetAllAsync()).Count();
                transaction.TransactionNumber = $"FT-{DateTime.Now:yyyyMMdd}-{(count + 1):D4}";
                transaction.CreatedAt = DateTime.Now;
                
                await _unitOfWork.Repository<FinancialTransaction>().AddAsync(transaction);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = transaction, message = "Tạo giao dịch thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating financial transaction");
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo giao dịch" });
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

