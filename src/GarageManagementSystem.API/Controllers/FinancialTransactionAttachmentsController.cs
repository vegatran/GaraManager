using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FinancialTransactionAttachmentsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FinancialTransactionAttachmentsController> _logger;

        public FinancialTransactionAttachmentsController(IUnitOfWork unitOfWork, ILogger<FinancialTransactionAttachmentsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet("transaction/{transactionId}")]
        public async Task<IActionResult> GetByTransaction(int transactionId)
        {
            try
            {
                var attachments = await _unitOfWork.Repository<FinancialTransactionAttachment>().GetAllAsync();
                var result = attachments.Where(a => a.FinancialTransactionId == transactionId).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attachments");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy tài liệu đính kèm" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FinancialTransactionAttachment attachment)
        {
            try
            {
                attachment.UploadedAt = DateTime.Now;
                attachment.CreatedAt = DateTime.Now;
                await _unitOfWork.Repository<FinancialTransactionAttachment>().AddAsync(attachment);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = attachment, message = "Upload thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating attachment");
                return StatusCode(500, new { success = false, message = "Lỗi khi upload" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var attachment = await _unitOfWork.Repository<FinancialTransactionAttachment>().GetByIdAsync(id);
                if (attachment == null)
                    return NotFound(new { success = false, message = "Không tìm thấy tài liệu" });

                await _unitOfWork.Repository<FinancialTransactionAttachment>().DeleteAsync(attachment);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attachment");
                return StatusCode(500, new { success = false, message = "Lỗi khi xóa" });
            }
        }
    }
}

