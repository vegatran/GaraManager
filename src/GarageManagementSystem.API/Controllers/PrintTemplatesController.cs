using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarageManagementSystem.API.Controllers
{
    /// <summary>
    /// API Controller cho quản lý Print Templates
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PrintTemplatesController : ControllerBase
    {
        private readonly IPrintTemplateService _printTemplateService;
        private readonly IMapper _mapper;
        private readonly ILogger<PrintTemplatesController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public PrintTemplatesController(
            IPrintTemplateService printTemplateService,
            IMapper mapper,
            ILogger<PrintTemplatesController> logger,
            IUnitOfWork unitOfWork)
        {
            _printTemplateService = printTemplateService;
            _mapper = mapper;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Lấy tất cả mẫu in
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<PrintTemplateDto>>>> GetAllTemplates()
        {
            try
             {
                var templates = await _printTemplateService.GetActiveTemplatesAsync();
                var templateDtos = _mapper.Map<List<PrintTemplateDto>>(templates);
                
                return Ok(ApiResponse<List<PrintTemplateDto>>.SuccessResult(templateDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all print templates");
                return StatusCode(500, ApiResponse<List<PrintTemplateDto>>.ErrorResult("Lỗi khi lấy danh sách mẫu in"));
            }
        }

        /// <summary>
        /// Lấy mẫu theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PrintTemplateDto>>> GetTemplateById(int id)
        {
            try
            {
                var template = await _unitOfWork.PrintTemplates.GetByIdAsync(id);
                if (template == null)
                {
                    return NotFound(ApiResponse<PrintTemplateDto>.ErrorResult("Không tìm thấy mẫu in"));
                }

                var templateDto = _mapper.Map<PrintTemplateDto>(template);
                return Ok(ApiResponse<PrintTemplateDto>.SuccessResult(templateDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting print template by id: {Id}", id);
                return StatusCode(500, ApiResponse<PrintTemplateDto>.ErrorResult("Lỗi khi lấy mẫu in"));
            }
        }

        /// <summary>
        /// Lấy mẫu mặc định theo loại
        /// </summary>
        [HttpGet("default/{templateType}")]
        public async Task<ActionResult<ApiResponse<PrintTemplateDto>>> GetDefaultTemplate(string templateType)
        {
            try
            {
                var template = await _printTemplateService.GetDefaultTemplateAsync(templateType);
                if (template == null)
                {
                    return NotFound(ApiResponse<PrintTemplateDto>.ErrorResult($"Không tìm thấy mẫu mặc định cho loại: {templateType}"));
                }

                var templateDto = _mapper.Map<PrintTemplateDto>(template);
                return Ok(ApiResponse<PrintTemplateDto>.SuccessResult(templateDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default template for type: {TemplateType}", templateType);
                return StatusCode(500, ApiResponse<PrintTemplateDto>.ErrorResult("Lỗi khi lấy mẫu mặc định"));
            }
        }

        /// <summary>
        /// Lấy mẫu theo loại
        /// </summary>
        [HttpGet("type/{templateType}")]
        public async Task<ActionResult<ApiResponse<List<PrintTemplateDto>>>> GetTemplatesByType(string templateType)
        {
            try
            {
                var templates = await _printTemplateService.GetTemplatesByTypeAsync(templateType);
                var templateDtos = _mapper.Map<List<PrintTemplateDto>>(templates);
                
                return Ok(ApiResponse<List<PrintTemplateDto>>.SuccessResult(templateDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting templates by type: {TemplateType}", templateType);
                return StatusCode(500, ApiResponse<List<PrintTemplateDto>>.ErrorResult("Lỗi khi lấy mẫu theo loại"));
            }
        }

        /// <summary>
        /// Tạo mẫu mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<PrintTemplateDto>>> CreateTemplate([FromBody] CreatePrintTemplateDto createDto)
        {
            try
            {
                var template = _mapper.Map<PrintTemplate>(createDto);
                template.CreatedAt = DateTime.Now;
                
                var createdTemplate = await _printTemplateService.CreateTemplateAsync(template);
                var templateDto = _mapper.Map<PrintTemplateDto>(createdTemplate);
                
                return CreatedAtAction(nameof(GetTemplateById), new { id = createdTemplate.Id }, 
                    ApiResponse<PrintTemplateDto>.SuccessResult(templateDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating print template");
                return StatusCode(500, ApiResponse<PrintTemplateDto>.ErrorResult("Lỗi khi tạo mẫu in"));
            }
        }

        /// <summary>
        /// Cập nhật mẫu
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<PrintTemplateDto>>> UpdateTemplate(int id, [FromBody] UpdatePrintTemplateDto updateDto)
        {
            try
            {
                var existingTemplate = await _unitOfWork.PrintTemplates.GetByIdAsync(id);
                if (existingTemplate == null)
                {
                    return NotFound(ApiResponse<PrintTemplateDto>.ErrorResult("Không tìm thấy mẫu in"));
                }

                _mapper.Map(updateDto, existingTemplate);
                existingTemplate.UpdatedAt = DateTime.Now;
                
                var updatedTemplate = await _printTemplateService.UpdateTemplateAsync(existingTemplate);
                var templateDto = _mapper.Map<PrintTemplateDto>(updatedTemplate);
                
                return Ok(ApiResponse<PrintTemplateDto>.SuccessResult(templateDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating print template: {Id}", id);
                return StatusCode(500, ApiResponse<PrintTemplateDto>.ErrorResult("Lỗi khi cập nhật mẫu in"));
            }
        }

        /// <summary>
        /// Xóa mẫu
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteTemplate(int id)
        {
            try
            {
                await _printTemplateService.DeleteTemplateAsync(id);
                return Ok(ApiResponse<bool>.SuccessResult(true, "Xóa mẫu in thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting print template: {Id}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Lỗi khi xóa mẫu in"));
            }
        }

        /// <summary>
        /// Đặt mẫu làm mặc định
        /// </summary>
        [HttpPost("{id}/set-default")]
        public async Task<ActionResult<ApiResponse<bool>>> SetAsDefault(int id, [FromBody] SetDefaultTemplateDto setDefaultDto)
        {
            try
            {
                await _printTemplateService.SetAsDefaultAsync(id, setDefaultDto.TemplateType);
                return Ok(ApiResponse<bool>.SuccessResult(true, "Đặt mẫu làm mặc định thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting template as default: {Id}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Lỗi khi đặt mẫu làm mặc định"));
            }
        }

        /// <summary>
        /// Tạo mẫu mặc định cho báo giá
        /// </summary>
        [HttpPost("create-default-quotation")]
        public async Task<ActionResult<ApiResponse<bool>>> CreateDefaultQuotationTemplate()
        {
            try
            {
                await _printTemplateService.CreateDefaultQuotationTemplateAsync();
                return Ok(ApiResponse<bool>.SuccessResult(true, "Tạo mẫu mặc định cho báo giá thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating default quotation template");
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Lỗi khi tạo mẫu mặc định"));
            }
        }
    }
}
