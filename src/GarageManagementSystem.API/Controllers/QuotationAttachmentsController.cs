using AutoMapper;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarageManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuotationAttachmentsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<QuotationAttachmentsController> _logger;

        public QuotationAttachmentsController(
            IUnitOfWork unitOfWork, 
            IMapper mapper, 
            IWebHostEnvironment environment,
            ILogger<QuotationAttachmentsController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách file đính kèm theo báo giá
        /// </summary>
        [HttpGet("quotation/{quotationId}")]
        public async Task<ActionResult<List<QuotationAttachmentDto>>> GetByQuotationId(int quotationId)
        {
            try
            {
                var attachments = await _unitOfWork.QuotationAttachments.GetByQuotationIdAsync(quotationId);
                var attachmentDtos = _mapper.Map<List<QuotationAttachmentDto>>(attachments);
                
                return Ok(attachmentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attachments for quotation {QuotationId}", quotationId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy danh sách tài liệu bảo hiểm theo báo giá
        /// </summary>
        [HttpGet("quotation/{quotationId}/insurance")]
        public async Task<ActionResult<List<QuotationAttachmentDto>>> GetInsuranceDocumentsByQuotationId(int quotationId)
        {
            try
            {
                var attachments = await _unitOfWork.QuotationAttachments.GetInsuranceDocumentsByQuotationIdAsync(quotationId);
                var attachmentDtos = _mapper.Map<List<QuotationAttachmentDto>>(attachments);
                
                return Ok(attachmentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving insurance documents for quotation {QuotationId}", quotationId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Upload file đính kèm cho báo giá
        /// </summary>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<QuotationAttachmentDto>> UploadAttachment([FromForm] CreateQuotationAttachmentDto createDto, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "File không được để trống" });
                }

                // Validate file type and size
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".xlsx", ".xls" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { message = "Loại file không được hỗ trợ" });
                }

                if (file.Length > 10 * 1024 * 1024) // 10MB
                {
                    return BadRequest(new { message = "File không được vượt quá 10MB" });
                }

                // Create upload directory
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "quotations", createDto.ServiceQuotationId.ToString());
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Generate unique file name
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Create attachment record
                var attachment = _mapper.Map<Core.Entities.QuotationAttachment>(createDto);
                attachment.FileName = file.FileName;
                attachment.FilePath = Path.Combine("uploads", "quotations", createDto.ServiceQuotationId.ToString(), fileName).Replace("\\", "/");
                attachment.FileType = file.ContentType;
                attachment.FileSize = file.Length;
                attachment.UploadedDate = DateTime.Now;
                // attachment.UploadedById = GetCurrentUserId(); // TODO: Implement when authentication is ready

                await _unitOfWork.QuotationAttachments.AddAsync(attachment);
                await _unitOfWork.SaveChangesAsync();

                var attachmentDto = _mapper.Map<QuotationAttachmentDto>(attachment);
                return CreatedAtAction(nameof(GetByQuotationId), new { quotationId = createDto.ServiceQuotationId }, attachmentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading attachment for quotation {QuotationId}", createDto.ServiceQuotationId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Download file đính kèm
        /// </summary>
        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadAttachment(int id)
        {
            try
            {
                var attachment = await _unitOfWork.QuotationAttachments.GetByIdAsync(id);
                if (attachment == null)
                {
                    return NotFound(new { message = "File không tồn tại" });
                }

                var filePath = Path.Combine(_environment.WebRootPath, attachment.FilePath);
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { message = "File không tồn tại trên server" });
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                return File(fileBytes, attachment.FileType ?? "application/octet-stream", attachment.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading attachment {AttachmentId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Xóa file đính kèm
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAttachment(int id)
        {
            try
            {
                var attachment = await _unitOfWork.QuotationAttachments.GetByIdAsync(id);
                if (attachment == null)
                {
                    return NotFound(new { message = "File không tồn tại" });
                }

                // Delete physical file
                var filePath = Path.Combine(_environment.WebRootPath, attachment.FilePath);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // Delete database record
                await _unitOfWork.QuotationAttachments.DeleteAsync(attachment);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { message = "Xóa file thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attachment {AttachmentId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
