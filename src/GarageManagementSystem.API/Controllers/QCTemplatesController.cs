using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using GarageManagementSystem.Infrastructure.Data;
using GarageManagementSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GarageManagementSystem.API.Controllers
{
    /// <summary>
    /// ✅ OPTIMIZED: QC Templates Controller - Quản lý QC Checklist Templates (Admin only)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiScope")]
    public class QCTemplatesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly GarageDbContext _context;
        private readonly ICacheService _cacheService;
        private readonly ILogger<QCTemplatesController> _logger;

        public QCTemplatesController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            GarageDbContext context,
            ICacheService cacheService,
            ILogger<QCTemplatesController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
            _cacheService = cacheService;
            _logger = logger;
        }

        /// <summary>
        /// ✅ OPTIMIZED: Lấy danh sách tất cả templates (Admin)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<QCChecklistTemplateDto>>>> GetAll()
        {
            try
            {
                var templates = await _context.QCChecklistTemplates
                    .Where(t => !t.IsDeleted)
                    .Include(t => t.TemplateItems.Where(i => !i.IsDeleted))
                    .OrderByDescending(t => t.IsDefault)
                    .ThenBy(t => t.TemplateName)
                    .ToListAsync();

                var templateDtos = templates.Select(t =>
                {
                    var dto = _mapper.Map<QCChecklistTemplateDto>(t);
                    // ✅ FIX: Đảm bảo TemplateItems không null
                    if (dto.TemplateItems == null)
                    {
                        dto.TemplateItems = new List<QCChecklistTemplateItemDto>();
                    }
                    return dto;
                }).ToList();
                
                return Ok(ApiResponse<List<QCChecklistTemplateDto>>.SuccessResult(templateDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting QC templates");
                return StatusCode(500, ApiResponse<List<QCChecklistTemplateDto>>.ErrorResult("Lỗi khi lấy danh sách templates", ex.Message));
            }
        }

        /// <summary>
        /// ✅ OPTIMIZED: Lấy template theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<QCChecklistTemplateDto>>> GetById(int id)
        {
            try
            {
                // ✅ VALIDATION: Không cho phép query template với Id = 0 (template fallback không tồn tại trong DB)
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<QCChecklistTemplateDto>.ErrorResult("ID template không hợp lệ. Template fallback (Id = 0) không tồn tại trong database."));
                }

                var template = await _context.QCChecklistTemplates
                    .Where(t => !t.IsDeleted && t.Id == id)
                    .Include(t => t.TemplateItems.Where(i => !i.IsDeleted))
                    .FirstOrDefaultAsync();

                if (template == null)
                {
                    return NotFound(ApiResponse<QCChecklistTemplateDto>.ErrorResult("Không tìm thấy template"));
                }

                var templateDto = _mapper.Map<QCChecklistTemplateDto>(template);
                // ✅ FIX: Đảm bảo TemplateItems không null
                if (templateDto.TemplateItems == null)
                {
                    templateDto.TemplateItems = new List<QCChecklistTemplateItemDto>();
                }
                return Ok(ApiResponse<QCChecklistTemplateDto>.SuccessResult(templateDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting QC template {TemplateId}", id);
                return StatusCode(500, ApiResponse<QCChecklistTemplateDto>.ErrorResult("Lỗi khi lấy template", ex.Message));
            }
        }

        /// <summary>
        /// ✅ OPTIMIZED: Tạo template mới (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,SuperAdmin")]
        public async Task<ActionResult<ApiResponse<QCChecklistTemplateDto>>> Create([FromBody] CreateQCChecklistTemplateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<QCChecklistTemplateDto>.ErrorResult("Dữ liệu không hợp lệ"));
                }

                // ✅ VALIDATION: Kiểm tra createDto không null
                if (createDto == null)
                {
                    return BadRequest(ApiResponse<QCChecklistTemplateDto>.ErrorResult("Dữ liệu không hợp lệ"));
                }

                // ✅ VALIDATION: Kiểm tra TemplateName không rỗng
                if (string.IsNullOrWhiteSpace(createDto.TemplateName))
                {
                    return BadRequest(ApiResponse<QCChecklistTemplateDto>.ErrorResult(
                        "Tên template không được để trống"));
                }

                // ✅ VALIDATION: Kiểm tra TemplateItems không null và có ít nhất 1 item
                if (createDto.TemplateItems == null || !createDto.TemplateItems.Any())
                {
                    return BadRequest(ApiResponse<QCChecklistTemplateDto>.ErrorResult(
                        "Template phải có ít nhất 1 hạng mục kiểm tra"));
                }

                await _unitOfWork.BeginTransactionAsync();

                // ✅ VALIDATION: Nếu IsDefault = true, đảm bảo chỉ có 1 template mặc định
                if (createDto.IsDefault)
                {
                    var existingDefault = await _context.QCChecklistTemplates
                        .FirstOrDefaultAsync(t => !t.IsDeleted && t.IsDefault && t.Id != 0);
                    
                    if (existingDefault != null)
                    {
                        existingDefault.IsDefault = false;
                        await _unitOfWork.Repository<QCChecklistTemplate>().UpdateAsync(existingDefault);
                        // ✅ FIX: Save changes cho existingDefault trước khi tiếp tục
                        await _unitOfWork.SaveChangesAsync();
                    }
                }

                var template = _mapper.Map<QCChecklistTemplate>(createDto);
                // ✅ FIX: Đảm bảo TemplateName được trim để loại bỏ whitespace thừa
                template.TemplateName = createDto.TemplateName.Trim();
                await _unitOfWork.Repository<QCChecklistTemplate>().AddAsync(template);
                await _unitOfWork.SaveChangesAsync();

                // ✅ FIX: Thêm template items với validation đầy đủ
                if (createDto.TemplateItems != null && createDto.TemplateItems.Any())
                {
                    int displayOrder = 1;
                    foreach (var itemDto in createDto.TemplateItems.OrderBy(i => i.DisplayOrder))
                    {
                        // ✅ VALIDATION: Kiểm tra ChecklistItemName không rỗng
                        if (string.IsNullOrWhiteSpace(itemDto.ChecklistItemName))
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return BadRequest(ApiResponse<QCChecklistTemplateDto>.ErrorResult(
                                $"Hạng mục thứ {displayOrder} không có tên. Vui lòng điền đầy đủ thông tin."));
                        }

                        var item = _mapper.Map<QCChecklistTemplateItem>(itemDto);
                        item.TemplateId = template.Id;
                        item.DisplayOrder = displayOrder++;
                        // ✅ FIX: Đảm bảo IsRequired được map đúng
                        item.IsRequired = itemDto.IsRequired;
                        await _unitOfWork.Repository<QCChecklistTemplateItem>().AddAsync(item);
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // ✅ OPTIMIZED: Invalidate cache
                await InvalidateTemplateCache();

                // ✅ FIX: Reload với items và kiểm tra null (sau khi commit, không thể rollback)
                var reloadedTemplate = await _context.QCChecklistTemplates
                    .Include(t => t.TemplateItems.Where(i => !i.IsDeleted))
                    .FirstOrDefaultAsync(t => t.Id == template.Id);

                if (reloadedTemplate is null)
                {
                    // ✅ CRITICAL FIX: Không thể rollback sau khi commit, chỉ log error
                    // Trường hợp này rất hiếm (có thể do concurrency hoặc database issue)
                    _logger.LogError("Template không tồn tại sau khi tạo và commit. TemplateId: {TemplateId}. Có thể do concurrency issue.", template.Id);
                    return StatusCode(500, ApiResponse<QCChecklistTemplateDto>.ErrorResult(
                        "Template đã được tạo nhưng không thể reload. Vui lòng refresh trang và thử lại."));
                }

                var templateDto = _mapper.Map<QCChecklistTemplateDto>(reloadedTemplate);
                // ✅ FIX: Đảm bảo TemplateItems không null
                if (templateDto.TemplateItems == null)
                {
                    templateDto.TemplateItems = new List<QCChecklistTemplateItemDto>();
                }
                return CreatedAtAction(nameof(GetById), new { id = reloadedTemplate.Id }, 
                    ApiResponse<QCChecklistTemplateDto>.SuccessResult(templateDto, "Đã tạo template thành công"));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error creating QC template");
                return StatusCode(500, ApiResponse<QCChecklistTemplateDto>.ErrorResult("Lỗi khi tạo template", ex.Message));
            }
        }

        /// <summary>
        /// ✅ OPTIMIZED: Cập nhật template (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager,SuperAdmin")]
        public async Task<ActionResult<ApiResponse<QCChecklistTemplateDto>>> Update(int id, [FromBody] UpdateQCChecklistTemplateDto updateDto)
        {
            try
            {
                if (id != updateDto.Id)
                {
                    return BadRequest(ApiResponse<QCChecklistTemplateDto>.ErrorResult("ID không khớp"));
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<QCChecklistTemplateDto>.ErrorResult("Dữ liệu không hợp lệ"));
                }

                // ✅ VALIDATION: Kiểm tra updateDto không null
                if (updateDto == null)
                {
                    return BadRequest(ApiResponse<QCChecklistTemplateDto>.ErrorResult("Dữ liệu không hợp lệ"));
                }

                // ✅ VALIDATION: Không cho phép update template với Id <= 0
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<QCChecklistTemplateDto>.ErrorResult("ID template không hợp lệ"));
                }

                // ✅ VALIDATION: Kiểm tra TemplateName không rỗng
                if (string.IsNullOrWhiteSpace(updateDto.TemplateName))
                {
                    return BadRequest(ApiResponse<QCChecklistTemplateDto>.ErrorResult(
                        "Tên template không được để trống"));
                }

                // ✅ VALIDATION: Kiểm tra TemplateItems không null và có ít nhất 1 item
                if (updateDto.TemplateItems == null || !updateDto.TemplateItems.Any())
                {
                    return BadRequest(ApiResponse<QCChecklistTemplateDto>.ErrorResult(
                        "Template phải có ít nhất 1 hạng mục kiểm tra"));
                }

                await _unitOfWork.BeginTransactionAsync();

                var template = await _unitOfWork.Repository<QCChecklistTemplate>().GetByIdAsync(id);
                if (template == null || template.IsDeleted)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return NotFound(ApiResponse<QCChecklistTemplateDto>.ErrorResult("Không tìm thấy template"));
                }

                // ✅ VALIDATION: Nếu IsDefault = true, đảm bảo chỉ có 1 template mặc định
                // ✅ FIX: Check cả trường hợp template đã là default và đang được set lại default
                if (updateDto.IsDefault)
                {
                    var existingDefault = await _context.QCChecklistTemplates
                        .FirstOrDefaultAsync(t => !t.IsDeleted && t.IsDefault && t.Id != id);
                    
                    if (existingDefault != null)
                    {
                        existingDefault.IsDefault = false;
                        await _unitOfWork.Repository<QCChecklistTemplate>().UpdateAsync(existingDefault);
                        // ✅ FIX: Save changes cho existingDefault trước khi tiếp tục
                        await _unitOfWork.SaveChangesAsync();
                    }
                }
                else
                {
                    // ✅ VALIDATION: Nếu đang set IsDefault = false cho template hiện tại là default
                    // Đảm bảo có ít nhất 1 template mặc định khác
                    if (template.IsDefault)
                    {
                        var otherDefault = await _context.QCChecklistTemplates
                            .FirstOrDefaultAsync(t => !t.IsDeleted && t.IsDefault && t.Id != id);
                        
                        if (otherDefault == null)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return BadRequest(ApiResponse<QCChecklistTemplateDto>.ErrorResult(
                                "Không thể bỏ template mặc định. Phải có ít nhất 1 template mặc định trong hệ thống."));
                        }
                    }
                }

                // Update template properties
                template.TemplateName = updateDto.TemplateName.Trim(); // ✅ FIX: Trim để loại bỏ whitespace thừa
                template.Description = updateDto.Description;
                template.IsDefault = updateDto.IsDefault;
                template.VehicleType = updateDto.VehicleType;
                template.ServiceType = updateDto.ServiceType;
                template.IsActive = updateDto.IsActive;

                await _unitOfWork.Repository<QCChecklistTemplate>().UpdateAsync(template);

                // ✅ FIX: Xóa tất cả items cũ (soft delete) và SaveChanges trước khi add items mới
                var existingItems = await _context.QCChecklistTemplateItems
                    .Where(i => i.TemplateId == id && !i.IsDeleted)
                    .ToListAsync();

                if (existingItems.Any())
                {
                    foreach (var item in existingItems)
                    {
                        item.IsDeleted = true;
                        item.UpdatedAt = DateTime.Now;
                        await _unitOfWork.Repository<QCChecklistTemplateItem>().UpdateAsync(item);
                    }
                    // ✅ FIX: Save changes cho soft delete items trước khi add items mới
                    await _unitOfWork.SaveChangesAsync();
                }

                // ✅ FIX: Thêm items mới với validation đầy đủ
                if (updateDto.TemplateItems != null && updateDto.TemplateItems.Any())
                {
                    int displayOrder = 1;
                    foreach (var itemDto in updateDto.TemplateItems.OrderBy(i => i.DisplayOrder))
                    {
                        // ✅ VALIDATION: Kiểm tra ChecklistItemName không rỗng
                        if (string.IsNullOrWhiteSpace(itemDto.ChecklistItemName))
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return BadRequest(ApiResponse<QCChecklistTemplateDto>.ErrorResult(
                                $"Hạng mục thứ {displayOrder} không có tên. Vui lòng điền đầy đủ thông tin."));
                        }

                        var item = _mapper.Map<QCChecklistTemplateItem>(itemDto);
                        item.TemplateId = id;
                        item.DisplayOrder = displayOrder++;
                        // ✅ FIX: Đảm bảo IsRequired được map đúng
                        item.IsRequired = itemDto.IsRequired;
                        await _unitOfWork.Repository<QCChecklistTemplateItem>().AddAsync(item);
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // ✅ OPTIMIZED: Invalidate cache
                await InvalidateTemplateCache();

                // ✅ FIX: Reload với items và kiểm tra null (sau khi commit, không thể rollback)
                var reloadedTemplate = await _context.QCChecklistTemplates
                    .Include(t => t.TemplateItems.Where(i => !i.IsDeleted))
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (reloadedTemplate is null)
                {
                    // ✅ CRITICAL FIX: Không thể rollback sau khi commit, chỉ log error
                    // Trường hợp này rất hiếm (có thể do concurrency hoặc database issue)
                    _logger.LogError("Template không tồn tại sau khi cập nhật và commit. TemplateId: {TemplateId}. Có thể do concurrency issue.", id);
                    return StatusCode(500, ApiResponse<QCChecklistTemplateDto>.ErrorResult(
                        "Template đã được cập nhật nhưng không thể reload. Vui lòng refresh trang và thử lại."));
                }

                var templateDto = _mapper.Map<QCChecklistTemplateDto>(reloadedTemplate);
                // ✅ FIX: Đảm bảo TemplateItems không null
                if (templateDto.TemplateItems == null)
                {
                    templateDto.TemplateItems = new List<QCChecklistTemplateItemDto>();
                }
                return Ok(ApiResponse<QCChecklistTemplateDto>.SuccessResult(templateDto, "Đã cập nhật template thành công"));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error updating QC template {TemplateId}", id);
                return StatusCode(500, ApiResponse<QCChecklistTemplateDto>.ErrorResult("Lỗi khi cập nhật template", ex.Message));
            }
        }

        /// <summary>
        /// ✅ OPTIMIZED: Xóa template (soft delete, Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager,SuperAdmin")]
        public async Task<ActionResult<ApiResponse>> Delete(int id)
        {
            try
            {
                // ✅ VALIDATION: Không cho phép xóa template với Id <= 0
                if (id <= 0)
                {
                    return BadRequest(ApiResponse.ErrorResult("ID template không hợp lệ"));
                }

                await _unitOfWork.BeginTransactionAsync();

                var template = await _unitOfWork.Repository<QCChecklistTemplate>().GetByIdAsync(id);
                if (template == null || template.IsDeleted)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return NotFound(ApiResponse.ErrorResult("Không tìm thấy template"));
                }

                // ✅ VALIDATION: Không cho phép xóa template mặc định
                if (template.IsDefault)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return BadRequest(ApiResponse.ErrorResult("Không thể xóa template mặc định"));
                }
                
                // ✅ VALIDATION: Đảm bảo sau khi xóa vẫn còn ít nhất 1 template active
                var remainingTemplates = await _context.QCChecklistTemplates
                    .CountAsync(t => !t.IsDeleted && t.IsActive && t.Id != id);
                
                if (remainingTemplates == 0)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return BadRequest(ApiResponse.ErrorResult(
                        "Không thể xóa template này. Phải có ít nhất 1 template đang hoạt động trong hệ thống."));
                }
                
                // ✅ VALIDATION: Nếu template đang active và là template cuối cùng, không cho phép xóa
                if (template.IsActive && remainingTemplates == 1)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return BadRequest(ApiResponse.ErrorResult(
                        "Không thể xóa template đang hoạt động này. Đây là template cuối cùng đang hoạt động trong hệ thống."));
                }

                await _unitOfWork.Repository<QCChecklistTemplate>().DeleteAsync(template);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // ✅ OPTIMIZED: Invalidate cache
                await InvalidateTemplateCache();

                return Ok(ApiResponse.SuccessResult("Đã xóa template thành công"));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error deleting QC template {TemplateId}", id);
                return StatusCode(500, ApiResponse.ErrorResult("Lỗi khi xóa template", ex.Message));
            }
        }

        /// <summary>
        /// ✅ OPTIMIZED: Invalidate cache khi template được tạo/sửa/xóa
        /// </summary>
        private async Task InvalidateTemplateCache()
        {
            try
            {
                // Invalidate tất cả cache keys liên quan đến QC template
                await _cacheService.RemoveByPrefixAsync("qc_template_");
                _logger.LogDebug("QC Template cache invalidated");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error invalidating QC template cache");
            }
        }
    }
}

