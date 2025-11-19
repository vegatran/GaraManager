using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace GarageManagementSystem.API.Controllers
{
    /// <summary>
    /// ✅ 4.3.1.9: Controller quản lý chứng từ đính kèm cho Financial Transaction
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FinancialTransactionAttachmentsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FinancialTransactionAttachmentsController> _logger;
        private readonly IWebHostEnvironment _environment;

        public FinancialTransactionAttachmentsController(
            IUnitOfWork unitOfWork, 
            ILogger<FinancialTransactionAttachmentsController> logger,
            IWebHostEnvironment environment)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// ✅ 4.3.1.9: Lấy danh sách attachments theo transaction ID
        /// </summary>
        [HttpGet("transaction/{transactionId}")]
        public async Task<ActionResult<ApiResponse<List<FinancialTransactionAttachmentDto>>>> GetByTransaction(int transactionId)
        {
            try
            {
                var attachments = await _unitOfWork.Repository<FinancialTransactionAttachment>().GetAllAsync();
                var result = attachments
                    .Where(a => a.FinancialTransactionId == transactionId && !a.IsDeleted)
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
                    .ToList();

                return Ok(ApiResponse<List<FinancialTransactionAttachmentDto>>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attachments for transaction {TransactionId}", transactionId);
                return StatusCode(500, ApiResponse<List<FinancialTransactionAttachmentDto>>.ErrorResult("Lỗi khi lấy tài liệu đính kèm", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 4.3.1.9: Upload file đính kèm cho Financial Transaction
        /// </summary>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<FinancialTransactionAttachmentDto>>> UploadAttachment(
            [FromForm] int financialTransactionId,
            [FromForm] IFormFile file,
            [FromForm] string? fileType = null,
            [FromForm] string? description = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(ApiResponse<FinancialTransactionAttachmentDto>.ErrorResult("File không được để trống"));
                }

                // Validate file type
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".xlsx", ".xls" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(ApiResponse<FinancialTransactionAttachmentDto>.ErrorResult("Loại file không được hỗ trợ. Chỉ chấp nhận: PDF, JPG, PNG, DOC, DOCX, XLSX, XLS"));
                }

                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(ApiResponse<FinancialTransactionAttachmentDto>.ErrorResult("File không được vượt quá 5MB"));
                }

                // Verify transaction exists
                var transaction = await _unitOfWork.Repository<FinancialTransaction>().GetByIdAsync(financialTransactionId);
                if (transaction == null || transaction.IsDeleted)
                {
                    return NotFound(ApiResponse<FinancialTransactionAttachmentDto>.ErrorResult("Không tìm thấy phiếu tài chính"));
                }

                // Create upload directory
                var uploadsPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "financial-transactions", financialTransactionId.ToString());
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Generate unique file name
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);
                var relativePath = Path.Combine("uploads", "financial-transactions", financialTransactionId.ToString(), fileName).Replace("\\", "/");

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Create attachment record
                var attachment = new FinancialTransactionAttachment
                {
                    FinancialTransactionId = financialTransactionId,
                    FileName = file.FileName,
                    FilePath = relativePath,
                    FileType = fileType ?? "Other",
                    FileSize = file.Length,
                    MimeType = file.ContentType,
                    Description = description,
                    UploadedAt = DateTime.Now,
                    UploadedBy = User.Identity?.Name ?? "System",
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.Repository<FinancialTransactionAttachment>().AddAsync(attachment);
                await _unitOfWork.SaveChangesAsync();

                var attachmentDto = new FinancialTransactionAttachmentDto
                {
                    Id = attachment.Id,
                    FinancialTransactionId = attachment.FinancialTransactionId,
                    FileName = attachment.FileName,
                    FilePath = attachment.FilePath,
                    FileType = attachment.FileType,
                    FileSize = attachment.FileSize,
                    MimeType = attachment.MimeType,
                    Description = attachment.Description,
                    UploadedAt = attachment.UploadedAt,
                    UploadedBy = attachment.UploadedBy
                };

                return Ok(ApiResponse<FinancialTransactionAttachmentDto>.SuccessResult(attachmentDto, "Upload chứng từ thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading attachment for transaction {TransactionId}", financialTransactionId);
                return StatusCode(500, ApiResponse<FinancialTransactionAttachmentDto>.ErrorResult("Lỗi khi upload chứng từ", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 4.3.1.9: Download file đính kèm
        /// </summary>
        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadAttachment(int id)
        {
            try
            {
                var attachment = await _unitOfWork.Repository<FinancialTransactionAttachment>().GetByIdAsync(id);
                if (attachment == null || attachment.IsDeleted)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("File không tồn tại"));
                }

                var filePath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, attachment.FilePath);
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(ApiResponse<object>.ErrorResult("File không tồn tại trên server"));
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                return File(fileBytes, attachment.MimeType ?? "application/octet-stream", attachment.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading attachment {AttachmentId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Lỗi khi tải file", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 4.3.1.9: Xóa file đính kèm
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            try
            {
                var attachment = await _unitOfWork.Repository<FinancialTransactionAttachment>().GetByIdAsync(id);
                if (attachment == null || attachment.IsDeleted)
                {
                    return NotFound(ApiResponse<bool>.ErrorResult("Không tìm thấy tài liệu"));
                }

                // Delete physical file
                var filePath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, attachment.FilePath);
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not delete physical file {FilePath}", filePath);
                    }
                }

                // Soft delete database record
                await _unitOfWork.Repository<FinancialTransactionAttachment>().DeleteAsync(attachment);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResult(true, "Xóa chứng từ thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attachment {AttachmentId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Lỗi khi xóa chứng từ", ex.Message));
            }
        }
    }
}

