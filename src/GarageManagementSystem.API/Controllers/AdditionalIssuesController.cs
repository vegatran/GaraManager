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

                // ✅ 2.3.2: Update ServiceOrderItem status to "OnHold" if provided
                // Chỉ set OnHold nếu Service Order Item chưa hoàn thành hoặc hủy
                if (createDto.ServiceOrderItemId.HasValue && orderItem != null)
                {
                    // Chỉ set OnHold nếu Service Order Item chưa hoàn thành hoặc hủy
                    if (orderItem.Status != "OnHold" && 
                        orderItem.Status != "Completed" && 
                        orderItem.Status != "Cancelled" &&
                        orderItem.Status != "Đã hoàn thành" &&
                        orderItem.Status != "Đã hủy")
                    {
                        // Lưu trạng thái hiện tại vào Notes để có thể restore sau
                        var previousStatus = orderItem.Status;
                        var now = DateTime.Now;
                        
                        // ✅ FIX: Nếu đang làm việc (InProgress) và có StartTime → Tính ActualHours trước khi set OnHold
                        // Điều này đảm bảo không tính thời gian chờ vào giờ công thực tế
                        if (previousStatus == "InProgress" && orderItem.StartTime.HasValue)
                        {
                            // Tính ActualHours từ StartTime đến hiện tại và cộng dồn
                            var timeSpan = now - orderItem.StartTime.Value;
                            var newActualHours = (decimal)timeSpan.TotalHours;
                            orderItem.ActualHours = (orderItem.ActualHours ?? 0) + newActualHours;
                            
                            // Set EndTime để đánh dấu thời điểm tạm dừng
                            orderItem.EndTime = now;
                            
                            _logger.LogInformation(
                                "Calculated ActualHours: {ActualHours} (added {NewHours} hours from {StartTime} to {EndTime}) before setting OnHold",
                                orderItem.ActualHours, newActualHours, orderItem.StartTime.Value, now);
                        }
                        
                        orderItem.Status = "OnHold";
                        orderItem.Notes = string.IsNullOrEmpty(orderItem.Notes)
                            ? $"Phát sinh: {issue.IssueName}. Dừng công việc chờ khách hàng duyệt. (Trạng thái trước: {previousStatus})"
                            : $"{orderItem.Notes}\nPhát sinh: {issue.IssueName}. Dừng công việc chờ khách hàng duyệt. (Trạng thái trước: {previousStatus})";
                        
                        await _unitOfWork.Repository<ServiceOrderItem>().UpdateAsync(orderItem);
                        await _unitOfWork.SaveChangesAsync();
                        
                        _logger.LogInformation(
                            "Set Service Order Item {ItemId} status to OnHold due to Additional Issue {IssueId}. Previous status: {PreviousStatus}, ActualHours: {ActualHours}",
                            orderItem.Id, issue.Id, previousStatus, orderItem.ActualHours);
                    }
                    else if (orderItem.Status == "Completed" || orderItem.Status == "Đã hoàn thành")
                    {
                        _logger.LogWarning(
                            "Cannot set Service Order Item {ItemId} to OnHold. Item is already Completed.",
                            orderItem.Id);
                    }
                    else if (orderItem.Status == "Cancelled" || orderItem.Status == "Đã hủy")
                    {
                        _logger.LogWarning(
                            "Cannot set Service Order Item {ItemId} to OnHold. Item is already Cancelled.",
                            orderItem.Id);
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
                    await _unitOfWork.RollbackTransactionAsync();
                    return NotFound(ApiResponse<AdditionalIssueDto>.ErrorResult("Không tìm thấy phát sinh"));
                }

                // ✅ VALIDATION: Cho phép chỉnh sửa phát sinh đã từ chối để tạo lại báo giá mới
                // Nếu Status = "Rejected" và updateDto.Status = "Identified" → Reset để tạo lại báo giá
                var isResettingFromRejected = issue.Status == "Rejected" && 
                    (!string.IsNullOrEmpty(updateDto.Status) && updateDto.Status == "Identified");
                
                if (isResettingFromRejected)
                {
                    // Reset Status về "Identified" và clear AdditionalQuotationId để tạo lại báo giá mới
                    issue.Status = "Identified";
                    issue.AdditionalQuotationId = null; // Clear để có thể tạo báo giá mới
                    issue.TechnicianNotes = string.IsNullOrEmpty(issue.TechnicianNotes)
                        ? $"Đã reset từ trạng thái 'Từ chối' để tạo lại báo giá mới."
                        : $"{issue.TechnicianNotes}\nĐã reset từ trạng thái 'Từ chối' để tạo lại báo giá mới.";
                    
                    _logger.LogInformation(
                        "Resetting Additional Issue {IssueId} from Rejected to Identified to allow creating new quotation",
                        issue.Id);
                }
                else if (issue.Status == "Approved" || issue.Status == "Repaired")
                {
                    // Không cho phép chỉnh sửa phát sinh đã được duyệt hoặc đã sửa
                    await _unitOfWork.RollbackTransactionAsync();
                    return BadRequest(ApiResponse<AdditionalIssueDto>.ErrorResult(
                        $"Không thể chỉnh sửa phát sinh ở trạng thái '{issue.Status}'. Phát sinh đã được xử lý hoàn tất."));
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
                if (!string.IsNullOrEmpty(updateDto.TechnicianNotes) && !isResettingFromRejected)
                    issue.TechnicianNotes = updateDto.TechnicianNotes;
                // ✅ Update Status (nếu có và không phải reset từ Rejected)
                if (!string.IsNullOrEmpty(updateDto.Status) && !isResettingFromRejected)
                {
                    issue.Status = updateDto.Status;
                }

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
        /// Logic: 
        /// - Nếu Quotation đã Approved → Không cho phép xóa (đã có Service Order)
        /// - Nếu Quotation chưa Approved → Tự động xóa Quotation cùng với Additional Issue
        /// - Nếu Service Order đã Completed → Không cho phép xóa
        /// - Nếu Service Order chưa Completed → Tự động hủy Service Order cùng với Additional Issue
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteAdditionalIssue(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var issue = await _unitOfWork.Repository<AdditionalIssue>().GetByIdAsync(id);
                if (issue == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return NotFound(ApiResponse<bool>.ErrorResult("Không tìm thấy phát sinh"));
                }

                // ✅ VALIDATION: Cho phép xóa khi Status = "Rejected" hoặc "Identified" (nếu không có Service Order)
                // Nếu Status = "Approved" hoặc "Repaired" → Không cho phép xóa (đã hoàn thành)
                if (issue.Status == "Approved" || issue.Status == "Repaired" || issue.Status == "Đã duyệt" || issue.Status == "Đã sửa")
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return BadRequest(ApiResponse<bool>.ErrorResult(
                        $"Không thể xóa phát sinh ở trạng thái '{issue.Status}'. Phát sinh đã được xử lý hoàn tất."));
                }

                // ✅ Nếu Status = "Rejected" hoặc "Identified" → Cho phép xóa nếu không có Service Order bổ sung
                if (issue.AdditionalServiceOrderId.HasValue)
                {
                    var serviceOrder = await _unitOfWork.ServiceOrders.GetByIdAsync(issue.AdditionalServiceOrderId.Value);
                    if (serviceOrder != null && !serviceOrder.IsDeleted)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return BadRequest(ApiResponse<bool>.ErrorResult(
                            $"Không thể xóa phát sinh vì đã có phiếu sửa chữa bổ sung (Phiếu #{serviceOrder.OrderNumber}). " +
                            "Vui lòng hủy phiếu sửa chữa trước khi xóa phát sinh."));
                    }
                }
                if (issue.AdditionalQuotationId.HasValue)
                {
                    var quotation = await _unitOfWork.ServiceQuotations.GetByIdAsync(issue.AdditionalQuotationId.Value);
                    if (quotation != null && !quotation.IsDeleted)
                    {
                        // Nếu Quotation đã được duyệt → Không cho phép xóa Additional Issue
                        if (quotation.Status == "Approved" || quotation.Status == "Đã duyệt")
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return BadRequest(ApiResponse<bool>.ErrorResult(
                                $"Không thể xóa phát sinh vì đã có báo giá bổ sung đã được duyệt (Báo giá #{quotation.QuotationNumber}). " +
                                "Báo giá đã được duyệt và có thể đã có phiếu sửa chữa liên quan. " +
                                "Vui lòng từ chối hoặc hủy báo giá trước khi xóa phát sinh."));
                        }

                        // ✅ CASCADE DELETE: Nếu Quotation chưa được duyệt → Tự động xóa Quotation
                        // Các trạng thái có thể xóa: Draft, Sent, Pending, Rejected, Cancelled, Expired
                        _logger.LogInformation(
                            "Cascade deleting Quotation {QuotationId} ({QuotationNumber}) when deleting Additional Issue {IssueId}",
                            quotation.Id, quotation.QuotationNumber, issue.Id);

                        await _unitOfWork.ServiceQuotations.DeleteAsync(quotation);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }

                // ✅ VALIDATION & CASCADE DELETE: Kiểm tra và xử lý liên kết với Service Order
                if (issue.AdditionalServiceOrderId.HasValue)
                {
                    var serviceOrder = await _unitOfWork.ServiceOrders.GetByIdAsync(issue.AdditionalServiceOrderId.Value);
                    if (serviceOrder != null && !serviceOrder.IsDeleted)
                    {
                        // Nếu Service Order đã hoàn thành → Không cho phép xóa Additional Issue
                        if (serviceOrder.Status == "Completed" || serviceOrder.Status == "Đã hoàn thành")
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return BadRequest(ApiResponse<bool>.ErrorResult(
                                $"Không thể xóa phát sinh vì đã có phiếu sửa chữa bổ sung đã hoàn thành (Phiếu #{serviceOrder.OrderNumber}). " +
                                "Phiếu sửa chữa đã hoàn thành không thể xóa. Vui lòng liên hệ quản trị viên để xử lý."));
                        }

                        // ✅ CASCADE DELETE: Nếu Service Order chưa hoàn thành → Tự động hủy Service Order
                        // Các trạng thái có thể hủy: Pending, ReadyToWork, WaitingForParts, InProgress, OnHold
                        _logger.LogInformation(
                            "Cascade cancelling Service Order {ServiceOrderId} ({OrderNumber}) when deleting Additional Issue {IssueId}",
                            serviceOrder.Id, serviceOrder.OrderNumber, issue.Id);

                        serviceOrder.Status = "Cancelled";
                        serviceOrder.Notes = string.IsNullOrEmpty(serviceOrder.Notes)
                            ? $"Đã hủy do xóa phát sinh #{issue.Id}"
                            : $"{serviceOrder.Notes}\nĐã hủy do xóa phát sinh #{issue.Id}";

                        await _unitOfWork.ServiceOrders.UpdateAsync(serviceOrder);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }

                // ✅ Xóa Additional Issue và Photos
                await _context.Entry(issue).Collection(i => i.Photos).LoadAsync();

                // ✅ 2.3.3: Restore Service Order Item status nếu có liên kết
                if (issue.ServiceOrderItemId.HasValue)
                {
                    var serviceOrderItem = await _unitOfWork.Repository<Core.Entities.ServiceOrderItem>()
                        .GetByIdAsync(issue.ServiceOrderItemId.Value);
                    
                    if (serviceOrderItem != null && !serviceOrderItem.IsDeleted)
                    {
                        // Nếu Service Order Item đang ở trạng thái OnHold → Restore về trạng thái hợp lý
                        if (serviceOrderItem.Status == "OnHold")
                        {
                            // ✅ FIX: Reset StartTime để bắt đầu lại từ đầu (không tính thời gian chờ)
                            // Giữ nguyên ActualHours đã tính (chỉ tính thời gian làm việc thực tế)
                            // Clear EndTime để có thể tính lại khi tiếp tục làm việc
                            serviceOrderItem.EndTime = null;
                            
                            // Nếu đã có StartTime (từ lần làm việc trước), reset StartTime = null
                            // Sẽ được set lại khi KTV bắt đầu làm việc tiếp
                            serviceOrderItem.StartTime = null;
                            
                            // Restore về trạng thái "Pending" để KTV bắt đầu lại
                            serviceOrderItem.Status = "Pending";
                            
                            serviceOrderItem.Notes = string.IsNullOrEmpty(serviceOrderItem.Notes)
                                ? $"Đã xóa phát sinh. Tiếp tục công việc."
                                : $"{serviceOrderItem.Notes}\nĐã xóa phát sinh. Tiếp tục công việc.";
                            
                            await _unitOfWork.Repository<Core.Entities.ServiceOrderItem>().UpdateAsync(serviceOrderItem);
                            await _unitOfWork.SaveChangesAsync();
                        }
                    }
                }

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

                await _unitOfWork.CommitTransactionAsync();

                var deletedItems = new List<string>();
                if (issue.AdditionalQuotationId.HasValue) deletedItems.Add("báo giá bổ sung");
                if (issue.AdditionalServiceOrderId.HasValue) deletedItems.Add("phiếu sửa chữa bổ sung");

                var message = "Đã xóa phát sinh thành công";
                if (deletedItems.Any())
                {
                    message += $". Đã tự động xóa/hủy: {string.Join(", ", deletedItems)}";
                }

                return Ok(ApiResponse<bool>.SuccessResult(true, message));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
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

                // ✅ 2.3.3: Kiểm tra nếu đã có Quotation
                // Cho phép tạo lại nếu Quotation đã bị từ chối (Rejected) hoặc hủy (Cancelled)
                if (issue.AdditionalQuotationId.HasValue)
                {
                    var existingQuotation = await _unitOfWork.ServiceQuotations.GetByIdAsync(issue.AdditionalQuotationId.Value);
                    if (existingQuotation != null && !existingQuotation.IsDeleted)
                    {
                        // Chỉ cho phép tạo lại nếu Quotation đã bị từ chối hoặc hủy
                        if (existingQuotation.Status != "Rejected" && existingQuotation.Status != "Cancelled" && existingQuotation.Status != "Từ chối" && existingQuotation.Status != "Đã hủy")
                        {
                            return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult(
                                $"Phát sinh này đã có báo giá bổ sung (Báo giá #{existingQuotation.QuotationNumber}, Trạng thái: {existingQuotation.Status}). " +
                                "Vui lòng từ chối hoặc hủy báo giá hiện tại trước khi tạo báo giá mới."));
                        }
                        
                        // ✅ Nếu Quotation đã bị từ chối/hủy, cho phép tạo lại báo giá mới
                        // Giữ nguyên AdditionalQuotationId cũ để giữ lịch sử
                        // Hoặc có thể reset AdditionalQuotationId = null để tạo báo giá hoàn toàn mới
                        // Hiện tại giữ nguyên để giữ lịch sử
                    }
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

                // ✅ 2.3.3: Cập nhật AdditionalIssue
                // Nếu đã có Quotation cũ (bị từ chối), vẫn giữ AdditionalQuotationId để giữ lịch sử
                // Nếu chưa có Quotation, set AdditionalQuotationId = quotation.Id
                if (!issue.AdditionalQuotationId.HasValue)
                {
                    issue.AdditionalQuotationId = quotation.Id;
                }
                issue.Status = "Quoted";
                issue.TechnicianNotes = string.IsNullOrEmpty(issue.TechnicianNotes)
                    ? $"Đã tạo báo giá bổ sung #{quotation.QuotationNumber}"
                    : $"{issue.TechnicianNotes}\nĐã tạo báo giá bổ sung #{quotation.QuotationNumber}";
                
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

