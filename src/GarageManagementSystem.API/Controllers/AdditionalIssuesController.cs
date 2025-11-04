using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GarageManagementSystem.API.Controllers
{
    /// <summary>
    /// API Controller cho quản lý Phát sinh (Additional Issues) trong quá trình sửa chữa
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiScope")]
    public class AdditionalIssuesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;
        private readonly GarageDbContext _context;
        private readonly ILogger<AdditionalIssuesController> _logger;

        public AdditionalIssuesController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IWebHostEnvironment environment,
            GarageDbContext context,
            ILogger<AdditionalIssuesController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _environment = environment;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách phát sinh theo ServiceOrderId
        /// </summary>
        [HttpGet("by-service-order/{serviceOrderId}")]
        public async Task<ActionResult<ApiResponse<List<AdditionalIssueDto>>>> GetByServiceOrderId(int serviceOrderId)
        {
            try
            {
                var issues = await _unitOfWork.Repository<AdditionalIssue>()
                    .FindAsync(i => i.ServiceOrderId == serviceOrderId);

                var issueDtos = new List<AdditionalIssueDto>();
                foreach (var issue in issues)
                {
                    await _context.Entry(issue)
                        .Collection(i => i.Photos)
                        .LoadAsync();
                    
                    await _context.Entry(issue)
                        .Reference(i => i.ReportedByEmployee)
                        .LoadAsync();

                    var dto = _mapper.Map<AdditionalIssueDto>(issue);
                    issueDtos.Add(dto);
                }

                return Ok(ApiResponse<List<AdditionalIssueDto>>.SuccessResult(issueDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting additional issues by service order {ServiceOrderId}", serviceOrderId);
                return StatusCode(500, ApiResponse<List<AdditionalIssueDto>>.ErrorResult("Lỗi khi lấy danh sách phát sinh", ex.Message));
            }
        }

        /// <summary>
        /// Lấy chi tiết một phát sinh
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<AdditionalIssueDto>>> GetById(int id)
        {
            try
            {
                var issue = await _unitOfWork.Repository<AdditionalIssue>().GetByIdAsync(id);
                if (issue == null)
                {
                    return NotFound(ApiResponse<AdditionalIssueDto>.ErrorResult("Không tìm thấy phát sinh"));
                }

                // Load navigation properties
                await _context.Entry(issue)
                    .Collection(i => i.Photos)
                    .LoadAsync();
                
                await _context.Entry(issue)
                    .Reference(i => i.ReportedByEmployee)
                    .LoadAsync();

                if (issue.ServiceOrderItemId.HasValue)
                {
                    await _context.Entry(issue)
                        .Reference(i => i.ServiceOrderItem)
                        .LoadAsync();
                }

                var dto = _mapper.Map<AdditionalIssueDto>(issue);
                return Ok(ApiResponse<AdditionalIssueDto>.SuccessResult(dto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting additional issue {IssueId}", id);
                return StatusCode(500, ApiResponse<AdditionalIssueDto>.ErrorResult("Lỗi khi lấy chi tiết phát sinh", ex.Message));
            }
        }

        /// <summary>
        /// Tạo phát sinh mới (với upload ảnh)
        /// </summary>
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<AdditionalIssueDto>>> CreateAdditionalIssue(
            [FromForm] CreateAdditionalIssueDto createDto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Validate ServiceOrder exists
                var serviceOrder = await _unitOfWork.ServiceOrders.GetByIdAsync(createDto.ServiceOrderId);
                if (serviceOrder == null)
                {
                    return BadRequest(ApiResponse<AdditionalIssueDto>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                // Validate ServiceOrderItem if provided and get AssignedTechnicianId
                ServiceOrderItem? orderItem = null;
                if (createDto.ServiceOrderItemId.HasValue)
                {
                    orderItem = await _unitOfWork.Repository<ServiceOrderItem>()
                        .GetByIdAsync(createDto.ServiceOrderItemId.Value);
                    if (orderItem == null)
                    {
                        return BadRequest(ApiResponse<AdditionalIssueDto>.ErrorResult("Không tìm thấy hạng mục sửa chữa"));
                    }
                }

                int? reportedByEmployeeId = null;
                
                // ✅ Logic: Nếu chọn hạng mục cụ thể → Phải có KTV được phân công
                if (createDto.ServiceOrderItemId.HasValue && orderItem != null)
                {
                    if (!orderItem.AssignedTechnicianId.HasValue)
                    {
                        return BadRequest(ApiResponse<AdditionalIssueDto>.ErrorResult(
                            $"Hạng mục '{orderItem.ServiceName}' chưa có KTV được phân công. " +
                            "Vui lòng phân công KTV cho hạng mục này trước khi báo cáo phát sinh."));
                    }
                    
                    // ✅ Dùng KTV đang phụ trách hạng mục (User nào sửa thì user đó phát hiện)
                    var assignedTechnician = await _unitOfWork.Employees.GetByIdAsync(orderItem.AssignedTechnicianId.Value);
                    if (assignedTechnician == null)
                    {
                        return BadRequest(ApiResponse<AdditionalIssueDto>.ErrorResult(
                            $"Không tìm thấy thông tin KTV (ID: {orderItem.AssignedTechnicianId.Value}) được phân công cho hạng mục này."));
                    }
                    
                    reportedByEmployeeId = orderItem.AssignedTechnicianId.Value;
                    _logger.LogInformation($"Using AssignedTechnicianId: {reportedByEmployeeId} - {assignedTechnician.Name}");
                }
                else
                {
                    // ✅ Nếu không chọn hạng mục (phát sinh ảnh hưởng toàn bộ ServiceOrder) → Dùng authenticated user
                    var userEmail = User.FindFirst(ClaimTypes.Email)?.Value 
                        ?? User.FindFirst("email")?.Value;
                    
                    var userName = User.FindFirst(ClaimTypes.Name)?.Value 
                        ?? User.FindFirst("name")?.Value
                        ?? User.Identity?.Name;
                    
                    _logger.LogInformation($"No ServiceOrderItemId, using authenticated user. email: {userEmail}, name: {userName}");
                    
                    // Try to find Employee by email
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        var employees = await _unitOfWork.Employees.FindAsync(e => 
                            e.Email != null && 
                            e.Email.ToLower() == userEmail.ToLower());
                        
                        var employeeByEmail = employees.FirstOrDefault();
                        if (employeeByEmail != null)
                        {
                            reportedByEmployeeId = employeeByEmail.Id;
                            _logger.LogInformation($"Found employee by email: {employeeByEmail.Id} - {employeeByEmail.Name}");
                        }
                    }
                    
                    // Try to find Employee by name if email not found
                    if (!reportedByEmployeeId.HasValue && !string.IsNullOrEmpty(userName))
                    {
                        var employees = await _unitOfWork.Employees.FindAsync(e => 
                            e.Name != null && 
                            e.Name.ToLower().Contains(userName.ToLower()));
                        
                        var employeeByName = employees.FirstOrDefault();
                        if (employeeByName != null)
                        {
                            reportedByEmployeeId = employeeByName.Id;
                            _logger.LogInformation($"Found employee by name: {employeeByName.Id} - {employeeByName.Name}");
                        }
                    }
                    
                    // If still not found, return error
                    if (!reportedByEmployeeId.HasValue)
                    {
                        var errorMsg = $"Tài khoản '{userEmail}' chưa có thông tin nhân viên (Employee) trong hệ thống. " +
                                      $"Vui lòng tạo Employee record trong bảng 'Quản Lý Nhân Viên' với Email '{userEmail}' " +
                                      $"hoặc liên hệ quản trị viên để được gán vào một Employee record.";
                        _logger.LogWarning($"Cannot find Employee for authenticated user. email: {userEmail}, name: {userName}");
                        return BadRequest(ApiResponse<AdditionalIssueDto>.ErrorResult(errorMsg));
                    }
                }

                // Map to entity
                var issue = _mapper.Map<AdditionalIssue>(createDto);
                issue.ReportedByEmployeeId = reportedByEmployeeId.Value;
                issue.Status = "Identified";
                issue.ReportedDate = DateTime.Now;

                await _unitOfWork.Repository<AdditionalIssue>().AddAsync(issue);
                await _unitOfWork.SaveChangesAsync();

                // Upload photos if provided - Read from Request.Form
                var photos = Request.Form.Files.Where(f => f.Name == "photos").ToList();
                if (photos != null && photos.Count > 0)
                {
                    var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "additional-issues", issue.Id.ToString());
                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var maxFileSize = 5 * 1024 * 1024; // 5MB

                    foreach (var photo in photos)
                    {
                        if (photo == null || photo.Length == 0) continue;

                        var fileExtension = Path.GetExtension(photo.FileName).ToLowerInvariant();
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            continue; // Skip invalid files
                        }

                        if (photo.Length > maxFileSize)
                        {
                            continue; // Skip oversized files
                        }

                        var fileName = $"{Guid.NewGuid()}{fileExtension}";
                        var filePath = Path.Combine(uploadsPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await photo.CopyToAsync(stream);
                        }

                        var issuePhoto = new AdditionalIssuePhoto
                        {
                            AdditionalIssueId = issue.Id,
                            FilePath = Path.Combine("uploads", "additional-issues", issue.Id.ToString(), fileName).Replace("\\", "/"),
                            FileName = photo.FileName,
                            DisplayOrder = issue.Photos.Count
                        };

                        await _unitOfWork.Repository<AdditionalIssuePhoto>().AddAsync(issuePhoto);
                    }

                    await _unitOfWork.SaveChangesAsync();
                }

                // ✅ Update ServiceOrderItem status to "OnHold" if provided
                // Reuse orderItem variable that was already loaded above
                if (createDto.ServiceOrderItemId.HasValue && orderItem != null)
                {
                    if (orderItem.Status != "OnHold")
                    {
                        orderItem.Status = "OnHold";
                        orderItem.Notes = string.IsNullOrEmpty(orderItem.Notes)
                            ? $"Phát sinh: {issue.IssueName}. Dừng công việc chờ khách hàng duyệt."
                            : $"{orderItem.Notes}\nPhát sinh: {issue.IssueName}. Dừng công việc chờ khách hàng duyệt.";
                        
                        await _unitOfWork.Repository<ServiceOrderItem>().UpdateAsync(orderItem);
                        await _unitOfWork.SaveChangesAsync();
                    } 
                }

                await _unitOfWork.CommitTransactionAsync();

                // Reload with navigation properties
                await _context.Entry(issue)
                    .Collection(i => i.Photos)
                    .LoadAsync();
                
                await _context.Entry(issue)
                    .Reference(i => i.ReportedByEmployee)
                    .LoadAsync();

                var dto = _mapper.Map<AdditionalIssueDto>(issue);
                return CreatedAtAction(nameof(GetById), new { id = issue.Id }, 
                    ApiResponse<AdditionalIssueDto>.SuccessResult(dto, "Đã tạo phát sinh thành công"));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error creating additional issue");
                return StatusCode(500, ApiResponse<AdditionalIssueDto>.ErrorResult("Lỗi khi tạo phát sinh", ex.Message));
            }
        }

        /// <summary>
        /// Cập nhật phát sinh
        /// </summary>
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<AdditionalIssueDto>>> UpdateAdditionalIssue(
            int id,
            [FromForm] UpdateAdditionalIssueDto updateDto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var issue = await _unitOfWork.Repository<AdditionalIssue>().GetByIdAsync(id);
                if (issue == null)
                {
                    return NotFound(ApiResponse<AdditionalIssueDto>.ErrorResult("Không tìm thấy phát sinh"));
                }

                // Update properties
                if (!string.IsNullOrEmpty(updateDto.Category))
                    issue.Category = updateDto.Category;
                if (!string.IsNullOrEmpty(updateDto.IssueName))
                    issue.IssueName = updateDto.IssueName;
                if (!string.IsNullOrEmpty(updateDto.Description))
                    issue.Description = updateDto.Description;
                if (!string.IsNullOrEmpty(updateDto.Severity))
                    issue.Severity = updateDto.Severity;
                if (updateDto.RequiresImmediateAction.HasValue)
                    issue.RequiresImmediateAction = updateDto.RequiresImmediateAction.Value;
                if (!string.IsNullOrEmpty(updateDto.TechnicianNotes))
                    issue.TechnicianNotes = updateDto.TechnicianNotes;
                if (!string.IsNullOrEmpty(updateDto.Status))
                    issue.Status = updateDto.Status;

                // Delete photos if requested
                // Handle both List<int> from DTO and comma-separated string from form
                List<int> deletedPhotoIds = new List<int>();
                if (updateDto.DeletedPhotoIds != null && updateDto.DeletedPhotoIds.Count > 0)
                {
                    deletedPhotoIds = updateDto.DeletedPhotoIds;
                }
                else if (!string.IsNullOrEmpty(Request.Form["DeletedPhotoIds"]))
                {
                    // Handle comma-separated string from form data
                    var photoIdsStr = Request.Form["DeletedPhotoIds"].ToString();
                    if (!string.IsNullOrEmpty(photoIdsStr))
                    {
                        var photoIdsArray = photoIdsStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var idStr in photoIdsArray)
                        {
                            if (int.TryParse(idStr, out int photoId))
                            {
                                deletedPhotoIds.Add(photoId);
                            }
                        }
                    }
                }

                if (deletedPhotoIds.Count > 0)
                {
                    var photosToDelete = await _unitOfWork.Repository<AdditionalIssuePhoto>()
                        .FindAsync(p => deletedPhotoIds.Contains(p.Id));

                    foreach (var photo in photosToDelete)
                    {
                        // Delete physical file
                        var fullPath = Path.Combine(_environment.WebRootPath, photo.FilePath);
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }

                        await _unitOfWork.Repository<AdditionalIssuePhoto>().DeleteAsync(photo);
                    }
                }

                // Upload new photos - Read from Request.Form
                var newPhotos = Request.Form.Files.Where(f => f.Name == "newPhotos" || f.Name == "photos").ToList();
                if (newPhotos != null && newPhotos.Count > 0)
                {
                    var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "additional-issues", issue.Id.ToString());
                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var maxFileSize = 5 * 1024 * 1024; // 5MB

                    await _context.Entry(issue).Collection(i => i.Photos).LoadAsync();
                    var currentMaxOrder = issue.Photos.Any() ? issue.Photos.Max(p => p.DisplayOrder ?? 0) : -1;

                    foreach (var photo in newPhotos)
                    {
                        if (photo == null || photo.Length == 0) continue;

                        var fileExtension = Path.GetExtension(photo.FileName).ToLowerInvariant();
                        if (!allowedExtensions.Contains(fileExtension)) continue;

                        if (photo.Length > maxFileSize) continue;

                        var fileName = $"{Guid.NewGuid()}{fileExtension}";
                        var filePath = Path.Combine(uploadsPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await photo.CopyToAsync(stream);
                        }

                        var issuePhoto = new AdditionalIssuePhoto
                        {
                            AdditionalIssueId = issue.Id,
                            FilePath = Path.Combine("uploads", "additional-issues", issue.Id.ToString(), fileName).Replace("\\", "/"),
                            FileName = photo.FileName,
                            DisplayOrder = ++currentMaxOrder
                        };

                        await _unitOfWork.Repository<AdditionalIssuePhoto>().AddAsync(issuePhoto);
                    }
                }

                await _unitOfWork.Repository<AdditionalIssue>().UpdateAsync(issue);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Reload with navigation properties
                await _context.Entry(issue)
                    .Collection(i => i.Photos)
                    .LoadAsync();
                
                await _context.Entry(issue)
                    .Reference(i => i.ReportedByEmployee)
                    .LoadAsync();

                var dto = _mapper.Map<AdditionalIssueDto>(issue);
                return Ok(ApiResponse<AdditionalIssueDto>.SuccessResult(dto, "Đã cập nhật phát sinh thành công"));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error updating additional issue {IssueId}", id);
                return StatusCode(500, ApiResponse<AdditionalIssueDto>.ErrorResult("Lỗi khi cập nhật phát sinh", ex.Message));
            }
        }

        /// <summary>
        /// Xóa phát sinh
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteAdditionalIssue(int id)
        {
            try
            {
                var issue = await _unitOfWork.Repository<AdditionalIssue>().GetByIdAsync(id);
                if (issue == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResult("Không tìm thấy phát sinh"));
                }

                // Load photos
                await _context.Entry(issue).Collection(i => i.Photos).LoadAsync();

                // Delete physical files
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "additional-issues", issue.Id.ToString());
                if (Directory.Exists(uploadsPath))
                {
                    Directory.Delete(uploadsPath, true);
                }

                // Soft delete photos
                foreach (var photo in issue.Photos)
                {
                    await _unitOfWork.Repository<AdditionalIssuePhoto>().DeleteAsync(photo);
                }

                // Soft delete issue
                await _unitOfWork.Repository<AdditionalIssue>().DeleteAsync(issue);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResult(true, "Đã xóa phát sinh thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting additional issue {IssueId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Lỗi khi xóa phát sinh", ex.Message));
            }
        }

        /// <summary>
        /// Upload thêm ảnh cho phát sinh
        /// </summary>
        [HttpPost("{id}/photos")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<List<AdditionalIssuePhotoDto>>>> UploadPhotos(int id, [FromForm] List<IFormFile> photos)
        {
            try
            {
                var issue = await _unitOfWork.Repository<AdditionalIssue>().GetByIdAsync(id);
                if (issue == null)
                {
                    return NotFound(ApiResponse<List<AdditionalIssuePhotoDto>>.ErrorResult("Không tìm thấy phát sinh"));
                }

                if (photos == null || photos.Count == 0)
                {
                    return BadRequest(ApiResponse<List<AdditionalIssuePhotoDto>>.ErrorResult("Không có ảnh nào được tải lên"));
                }

                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "additional-issues", issue.Id.ToString());
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var maxFileSize = 5 * 1024 * 1024; // 5MB

                await _context.Entry(issue).Collection(i => i.Photos).LoadAsync();
                var currentMaxOrder = issue.Photos.Any() ? issue.Photos.Max(p => p.DisplayOrder ?? 0) : -1;

                var uploadedPhotos = new List<AdditionalIssuePhoto>();

                foreach (var photo in photos)
                {
                    if (photo == null || photo.Length == 0) continue;

                    var fileExtension = Path.GetExtension(photo.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(fileExtension)) continue;

                    if (photo.Length > maxFileSize) continue;

                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var filePath = Path.Combine(uploadsPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await photo.CopyToAsync(stream);
                    }

                    var issuePhoto = new AdditionalIssuePhoto
                    {
                        AdditionalIssueId = issue.Id,
                        FilePath = Path.Combine("uploads", "additional-issues", issue.Id.ToString(), fileName).Replace("\\", "/"),
                        FileName = photo.FileName,
                        DisplayOrder = ++currentMaxOrder
                    };

                    await _unitOfWork.Repository<AdditionalIssuePhoto>().AddAsync(issuePhoto);
                    uploadedPhotos.Add(issuePhoto);
                }

                await _unitOfWork.SaveChangesAsync();

                var photoDtos = uploadedPhotos.Select(p => _mapper.Map<AdditionalIssuePhotoDto>(p)).ToList();
                return Ok(ApiResponse<List<AdditionalIssuePhotoDto>>.SuccessResult(photoDtos, "Đã tải lên ảnh thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading photos for additional issue {IssueId}", id);
                return StatusCode(500, ApiResponse<List<AdditionalIssuePhotoDto>>.ErrorResult("Lỗi khi tải lên ảnh", ex.Message));
            }
        }

        /// <summary>
        /// Xóa ảnh của phát sinh
        /// </summary>
        [HttpDelete("{id}/photos/{photoId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeletePhoto(int id, int photoId)
        {
            try
            {
                var photo = await _unitOfWork.Repository<AdditionalIssuePhoto>().GetByIdAsync(photoId);
                if (photo == null || photo.AdditionalIssueId != id)
                {
                    return NotFound(ApiResponse<bool>.ErrorResult("Không tìm thấy ảnh"));
                }

                // Delete physical file
                var fullPath = Path.Combine(_environment.WebRootPath, photo.FilePath);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                await _unitOfWork.Repository<AdditionalIssuePhoto>().DeleteAsync(photo);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResult(true, "Đã xóa ảnh thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting photo {PhotoId} for additional issue {IssueId}", photoId, id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Lỗi khi xóa ảnh", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 2.3.3: Tạo báo giá bổ sung từ phát sinh
        /// </summary>
        [HttpPost("{id}/create-quotation")]
        public async Task<ActionResult<ApiResponse<ServiceQuotationDto>>> CreateQuotationFromIssue(
            int id, 
            [FromBody] CreateQuotationFromIssueDto createDto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Validate AdditionalIssue exists
                var issue = await _unitOfWork.Repository<AdditionalIssue>().GetByIdAsync(id);
                if (issue == null)
                {
                    return NotFound(ApiResponse<ServiceQuotationDto>.ErrorResult("Không tìm thấy phát sinh"));
                }

                // Check if already has quotation
                if (issue.AdditionalQuotationId.HasValue)
                {
                    return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult(
                        $"Phát sinh này đã có báo giá bổ sung (ID: {issue.AdditionalQuotationId.Value})"));
                }

                // Get ServiceOrder to get CustomerId and VehicleId
                var serviceOrder = await _unitOfWork.ServiceOrders.GetByIdAsync(issue.ServiceOrderId);
                if (serviceOrder == null)
                {
                    return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                // Validate customer and vehicle
                var customer = await _unitOfWork.Customers.GetByIdAsync(serviceOrder.CustomerId);
                if (customer == null)
                {
                    return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult("Không tìm thấy khách hàng"));
                }

                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(serviceOrder.VehicleId);
                if (vehicle == null)
                {
                    return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult("Không tìm thấy xe"));
                }

                // Create quotation
                var quotation = new Core.Entities.ServiceQuotation
                {
                    QuotationNumber = await _unitOfWork.ServiceQuotations.GenerateQuotationNumberAsync(),
                    CustomerId = serviceOrder.CustomerId,
                    VehicleId = serviceOrder.VehicleId,
                    QuotationDate = DateTime.Now,
                    ValidUntil = createDto.ValidUntil ?? DateTime.Now.AddDays(7),
                    Status = "Draft",
                    Description = createDto.Description ?? $"Báo giá bổ sung cho phát sinh: {issue.IssueName}",
                    Terms = createDto.Terms,
                    TaxRate = createDto.TaxRate,
                    DiscountAmount = createDto.DiscountAmount,
                    CustomerNotes = createDto.CustomerNotes,
                    QuotationType = "Personal", // Default, có thể cập nhật sau
                    
                    // ✅ 2.3.3: Set fields for additional quotation
                    RelatedToServiceOrderId = issue.ServiceOrderId,
                    IsAdditionalQuotation = true,
                    
                    // Cache customer/vehicle info
                    CustomerName = customer.Name,
                    CustomerPhone = customer.Phone,
                    CustomerEmail = customer.Email,
                    VehiclePlate = vehicle.LicensePlate,
                    VehicleMake = vehicle.Brand,
                    VehicleModel = vehicle.Model
                };

                // Add items and calculate totals
                decimal subTotal = 0;
                foreach (var itemDto in createDto.Items)
                {
                    var item = new Core.Entities.QuotationItem
                    {
                        ServiceId = itemDto.ServiceId,
                        // PartId will be set later if needed
                        ItemName = itemDto.ItemName,
                        Description = itemDto.Description,
                        Quantity = itemDto.Quantity,
                        UnitPrice = itemDto.UnitPrice,
                        IsOptional = itemDto.IsOptional,
                        IsApproved = false,
                        Notes = itemDto.Notes,
                        ItemType = itemDto.ServiceType ?? "Service",
                        ItemCategory = itemDto.ItemCategory ?? "Material",
                        HasInvoice = itemDto.HasInvoice,
                        IsVATApplicable = itemDto.IsVATApplicable,
                        VATRate = itemDto.VATRate // ✅ Fix: VATRate is already decimal, no need for ??
                    };

                    // Calculate item totals
                    item.SubTotal = item.Quantity * item.UnitPrice;
                    if (item.IsVATApplicable && item.VATRate > 0)
                    {
                        if (item.VATRate > 1)
                        {
                            item.VATAmount = item.SubTotal * (item.VATRate / 100);
                        }
                        else
                        {
                            item.VATAmount = item.SubTotal * item.VATRate;
                        }
                    }
                    else
                    {
                        item.VATAmount = 0;
                    }
                    item.TotalPrice = item.SubTotal + item.VATAmount;
                    item.TotalAmount = item.TotalPrice; // ✅ Fix: No discount at item level

                    quotation.Items.Add(item);
                    subTotal += item.SubTotal;
                }

                quotation.SubTotal = subTotal;
                quotation.TaxAmount = quotation.Items
                    .Where(i => i.IsVATApplicable && i.VATRate > 0)
                    .Sum(i => i.VATAmount);
                quotation.TotalAmount = quotation.SubTotal + quotation.TaxAmount - quotation.DiscountAmount;

                // Save quotation
                await _unitOfWork.ServiceQuotations.AddAsync(quotation);
                await _unitOfWork.SaveChangesAsync();

                // Update AdditionalIssue
                issue.AdditionalQuotationId = quotation.Id;
                issue.Status = "Quoted";
                await _unitOfWork.Repository<AdditionalIssue>().UpdateAsync(issue);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                // Reload with details
                quotation = await _unitOfWork.ServiceQuotations.GetByIdWithDetailsAsync(quotation.Id);
                var quotationDto = _mapper.Map<ServiceQuotationDto>(quotation);

                return CreatedAtAction(nameof(GetById), new { id = quotation.Id }, 
                    ApiResponse<ServiceQuotationDto>.SuccessResult(quotationDto, "Đã tạo báo giá bổ sung từ phát sinh thành công"));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error creating quotation from additional issue {IssueId}", id);
                return StatusCode(500, ApiResponse<ServiceQuotationDto>.ErrorResult("Lỗi khi tạo báo giá từ phát sinh", ex.Message));
            }
        }
    }
}

