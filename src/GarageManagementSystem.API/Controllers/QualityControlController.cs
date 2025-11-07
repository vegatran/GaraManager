using AutoMapper;
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
    /// ✅ 2.4: Quality Control Controller - Kiểm tra chất lượng và Bàn giao
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiScope")]
    public class QualityControlController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly GarageDbContext _context;
        private readonly ILogger<QualityControlController> _logger;

        public QualityControlController(
            IUnitOfWork unitOfWork, 
            IMapper mapper, 
            GarageDbContext context,
            ILogger<QualityControlController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// ✅ 2.4.1: KTV hoàn thành kỹ thuật, chuyển JO sang "WaitingForQC"
        /// </summary>
        [HttpPost("service-orders/{id}/complete-technical")]
        public async Task<ActionResult<ApiResponse<ServiceOrderDto>>> CompleteTechnical(int id)
        {
            try
            {
                // ✅ VALIDATION: Check ModelState
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult("Dữ liệu không hợp lệ"));
                }

                await _unitOfWork.BeginTransactionAsync();

                var order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                if (order == null)
                {
                    return NotFound(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                // ✅ VALIDATION: Chỉ cho phép complete technical khi status là "Completed" (tất cả items đã completed)
                // Theo logic trong ServiceOrdersController, khi tất cả items completed thì status tự động chuyển sang "Completed"
                if (order.Status != "Completed" && order.Status != "InProgress")
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult(
                        $"Không thể hoàn thành kỹ thuật. Trạng thái hiện tại: {order.Status}. " +
                        "Chỉ có thể hoàn thành kỹ thuật khi tất cả hạng mục đã hoàn thành."));
                }

                // Kiểm tra tất cả items đã Completed chưa
                var incompleteItems = order.ServiceOrderItems
                    .Where(i => !i.IsDeleted && i.Status != "Completed" && i.Status != "Cancelled")
                    .ToList();

                if (incompleteItems.Any())
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult(
                        $"Còn {incompleteItems.Count} hạng mục chưa hoàn thành. Vui lòng hoàn thành tất cả hạng mục trước khi chuyển sang QC."));
                }

                // ✅ VALIDATION: Kiểm tra đã ở trạng thái "WaitingForQC" chưa
                if (order.Status == "WaitingForQC")
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult(
                        "JO đã ở trạng thái 'Chờ QC' rồi. Không cần hoàn thành kỹ thuật lại."));
                }

                // Tính tổng giờ công thực tế
                var totalActualHours = order.ServiceOrderItems
                    .Where(i => !i.IsDeleted && i.ActualHours.HasValue)
                    .Sum(i => i.ActualHours!.Value);

                order.TotalActualHours = totalActualHours;
                order.Status = "WaitingForQC";
                order.CompletedDate = DateTime.Now;

                await _unitOfWork.ServiceOrders.UpdateAsync(order);
                await _unitOfWork.CommitTransactionAsync();

                // Reload để lấy dữ liệu mới nhất
                order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                var orderDto = _mapper.Map<ServiceOrderDto>(order);
                orderDto.TotalActualHours = totalActualHours;
                
                return Ok(ApiResponse<ServiceOrderDto>.SuccessResult(orderDto, 
                    $"Đã hoàn thành kỹ thuật. Tổng giờ công thực tế: {totalActualHours:F2} giờ"));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error completing technical work for service order {ServiceOrderId}", id);
                return StatusCode(500, ApiResponse<ServiceOrderDto>.ErrorResult("Lỗi khi hoàn thành kỹ thuật", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 2.4.1: Lấy tổng giờ công thực tế của JO
        /// </summary>
        [HttpGet("service-orders/{id}/total-actual-hours")]
        public async Task<ActionResult<ApiResponse<decimal>>> GetTotalActualHours(int id)
        {
            try
            {
                var order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                if (order == null)
                {
                    return NotFound(ApiResponse<decimal>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                var totalActualHours = order.ServiceOrderItems
                    .Where(i => !i.IsDeleted && i.ActualHours.HasValue)
                    .Sum(i => i.ActualHours!.Value);

                return Ok(ApiResponse<decimal>.SuccessResult(totalActualHours));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total actual hours for service order {ServiceOrderId}", id);
                return StatusCode(500, ApiResponse<decimal>.ErrorResult("Lỗi khi lấy tổng giờ công", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 2.4.2: Lấy danh sách JO chờ QC
        /// </summary>
        [HttpGet("service-orders/waiting-for-qc")]
        public async Task<ActionResult<PagedResponse<ServiceOrderDto>>> GetWaitingForQC(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                // ✅ OPTIMIZED: Query ở database level thay vì GetAllAsync().Where()
                var waitingForQC = (await _unitOfWork.Repository<Core.Entities.ServiceOrder>()
                    .FindAsync(o => !o.IsDeleted && o.Status == "WaitingForQC"))
                    .OrderByDescending(o => o.CompletedDate ?? o.OrderDate)
                    .ToList();

                var totalCount = waitingForQC.Count;
                var pagedOrders = waitingForQC
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var orderDtos = new List<ServiceOrderDto>();
                foreach (var order in pagedOrders)
                {
                    // Load navigation properties
                    await _context.Entry(order)
                        .Reference(o => o.Customer)
                        .LoadAsync();
                    await _context.Entry(order)
                        .Reference(o => o.Vehicle)
                        .LoadAsync();
                    await _context.Entry(order)
                        .Collection(o => o.ServiceOrderItems)
                        .LoadAsync();

                    var orderDto = _mapper.Map<ServiceOrderDto>(order);
                    orderDto.TotalActualHours = order.TotalActualHours;
                    orderDto.QCFailedCount = order.QCFailedCount;
                    orderDto.HandoverDate = order.HandoverDate;
                    orderDto.HandoverLocation = order.HandoverLocation;
                    orderDtos.Add(orderDto);
                }

                return Ok(PagedResponse<ServiceOrderDto>.CreateSuccessResult(
                    orderDtos, pageNumber, pageSize, totalCount, "Danh sách JO chờ QC"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting waiting for QC service orders");
                return StatusCode(500, PagedResponse<ServiceOrderDto>.CreateErrorResult("Lỗi khi lấy danh sách JO chờ QC"));
            }
        }

        /// <summary>
        /// ✅ 2.4.2: Bắt đầu kiểm tra QC
        /// </summary>
        [HttpPost("service-orders/{id}/qc/start")]
        public async Task<ActionResult<ApiResponse<QualityControlDto>>> StartQC(int id, [FromBody] CreateQualityControlDto createDto)
        {
            try
            {
                // ✅ FIX: Set ServiceOrderId từ route parameter vào DTO (trước khi validation)
                // Vì JavaScript không gửi ServiceOrderId trong body, chỉ gửi trong URL
                if (createDto == null)
                {
                    createDto = new CreateQualityControlDto();
                }
                createDto.ServiceOrderId = id;
                
                // ✅ FIX: Clear ModelState và validate lại sau khi set ServiceOrderId
                // Vì ModelState đã được validate trước khi set ServiceOrderId
                ModelState.Clear();
                TryValidateModel(createDto);
                
                // ✅ VALIDATION: Check ModelState
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(ApiResponse<QualityControlDto>.ErrorResult(
                        $"Dữ liệu không hợp lệ: {string.Join(", ", errors)}"));
                }
                
                // ✅ FIX: Validate checklist items có ChecklistItemName không rỗng
                if (createDto.ChecklistItems != null && createDto.ChecklistItems.Any())
                {
                    var invalidItems = createDto.ChecklistItems
                        .Where(item => string.IsNullOrWhiteSpace(item.ChecklistItemName))
                        .ToList();
                    
                    if (invalidItems.Any())
                    {
                        return BadRequest(ApiResponse<QualityControlDto>.ErrorResult(
                            "Một số checklist items không có tên. Vui lòng điền đầy đủ thông tin."));
                    }
                }

                // ✅ AUTHORIZATION: Chỉ Tổ trưởng/QC mới có quyền bắt đầu QC
                var currentUserId = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                bool isAuthorized = false;
                int userId = 0;
                
                if (!string.IsNullOrEmpty(currentUserId) && int.TryParse(currentUserId, out userId))
                {
                    var currentEmployee = await _unitOfWork.Employees.GetByIdAsync(userId);
                    if (currentEmployee != null)
                    {
                        var position = (currentEmployee.Position ?? "").ToLower();
                        var positionName = currentEmployee.PositionNavigation?.Name?.ToLower() ?? "";
                        
                        isAuthorized = position.Contains("tổ trưởng") || position.Contains("qc") ||
                                      position.Contains("quản đốc") || position.Contains("manager") ||
                                      position.Contains("supervisor") ||
                                      positionName.Contains("tổ trưởng") || positionName.Contains("qc") ||
                                      positionName.Contains("quản đốc") || positionName.Contains("manager") ||
                                      positionName.Contains("supervisor");
                        
                        var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                                                   .Select(c => c.Value.ToLower()).ToList();
                        isAuthorized = isAuthorized || userRoles.Contains("manager") || 
                                      userRoles.Contains("supervisor") || userRoles.Contains("admin") || 
                                      userRoles.Contains("superadmin") || userRoles.Contains("qc");
                    }
                }
                
                if (!isAuthorized)
                {
                    return Forbid("Chỉ Tổ trưởng, QC hoặc Quản đốc mới có quyền bắt đầu kiểm tra QC");
                }

                await _unitOfWork.BeginTransactionAsync();

                var order = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
                if (order == null)
                {
                    return NotFound(ApiResponse<QualityControlDto>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                if (order.Status != "WaitingForQC")
                {
                    return BadRequest(ApiResponse<QualityControlDto>.ErrorResult(
                        $"JO không ở trạng thái 'Chờ QC'. Trạng thái hiện tại: {order.Status}"));
                }

                // ✅ VALIDATION: Kiểm tra đã có QC record đang Pending chưa (tránh duplicate)
                var existingQCList = (await _unitOfWork.Repository<Core.Entities.QualityControl>()
                    .FindAsync(q => q.ServiceOrderId == id && !q.IsDeleted))
                    .OrderByDescending(q => q.QCDate)
                    .ToList();

                var existingPendingQC = existingQCList.FirstOrDefault(q => q.QCResult == "Pending");
                if (existingPendingQC != null)
                {
                    return BadRequest(ApiResponse<QualityControlDto>.ErrorResult(
                        "Đã có QC record đang chờ xử lý. Vui lòng hoàn thành QC hiện tại trước khi bắt đầu QC mới."));
                }

                // Lấy QC Inspector từ claims hoặc DTO (đã có userId từ authorization check ở trên)
                int? employeeId = null;
                if (userId > 0)
                {
                    var currentEmployee = await _unitOfWork.Employees.GetByIdAsync(userId);
                    if (currentEmployee != null)
                    {
                        employeeId = currentEmployee.Id;
                    }
                }
                
                if (employeeId == null && createDto.QCInspectorId == null)
                {
                    return Unauthorized(ApiResponse<QualityControlDto>.ErrorResult("Không xác định được nhân viên QC"));
                }

                // Tạo QC record
                var qc = new Core.Entities.QualityControl
                {
                    ServiceOrderId = id,
                    QCInspectorId = createDto.QCInspectorId ?? employeeId,
                    QCDate = DateTime.Now,
                    QCResult = "Pending",
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.Repository<Core.Entities.QualityControl>().AddAsync(qc);
                await _unitOfWork.SaveChangesAsync();

                // Tạo checklist items
                if (createDto.ChecklistItems != null && createDto.ChecklistItems.Any())
                {
                    foreach (var itemDto in createDto.ChecklistItems)
                    {
                        var checklistItem = new Core.Entities.QCChecklistItem
                        {
                            QualityControlId = qc.Id,
                            ChecklistItemName = itemDto.ChecklistItemName,
                            IsChecked = itemDto.IsChecked,
                            Result = itemDto.Result,
                            Notes = itemDto.Notes,
                            DisplayOrder = itemDto.DisplayOrder,
                            CreatedAt = DateTime.Now
                        };
                        await _unitOfWork.Repository<Core.Entities.QCChecklistItem>().AddAsync(checklistItem);
                    }
                }

                // Chuyển trạng thái JO sang "QCInProgress"
                order.Status = "QCInProgress";
                await _unitOfWork.ServiceOrders.UpdateAsync(order);

                await _unitOfWork.CommitTransactionAsync();

                var qcDto = await MapToDtoAsync(qc);
                return Ok(ApiResponse<QualityControlDto>.SuccessResult(qcDto, "Đã bắt đầu kiểm tra QC"));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error starting QC inspection for service order {ServiceOrderId}", id);
                return StatusCode(500, ApiResponse<QualityControlDto>.ErrorResult("Lỗi khi bắt đầu kiểm tra QC", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 2.4.2: Hoàn thành QC với kết quả (Đạt/Không đạt)
        /// </summary>
        [HttpPost("service-orders/{id}/qc/complete")]
        public async Task<ActionResult<ApiResponse<QualityControlDto>>> CompleteQC(int id, [FromBody] CompleteQCDto completeDto)
        {
            try
            {
                // ✅ VALIDATION: Check ModelState
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<QualityControlDto>.ErrorResult("Dữ liệu không hợp lệ"));
                }

                // ✅ VALIDATION: QCResult chỉ chấp nhận "Pass" hoặc "Fail"
                if (string.IsNullOrEmpty(completeDto.QCResult) || 
                    (completeDto.QCResult != "Pass" && completeDto.QCResult != "Fail"))
                {
                    return BadRequest(ApiResponse<QualityControlDto>.ErrorResult(
                        "Kết quả QC không hợp lệ. Chỉ chấp nhận 'Pass' hoặc 'Fail'"));
                }

                // ✅ AUTHORIZATION: Chỉ Tổ trưởng/QC mới có quyền hoàn thành QC
                var currentUserId = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                bool isAuthorized = false;
                
                if (!string.IsNullOrEmpty(currentUserId) && int.TryParse(currentUserId, out var userId))
                {
                    var currentEmployee = await _unitOfWork.Employees.GetByIdAsync(userId);
                    if (currentEmployee != null)
                    {
                        var position = (currentEmployee.Position ?? "").ToLower();
                        var positionName = currentEmployee.PositionNavigation?.Name?.ToLower() ?? "";
                        
                        isAuthorized = position.Contains("tổ trưởng") || position.Contains("qc") ||
                                      position.Contains("quản đốc") || position.Contains("manager") ||
                                      position.Contains("supervisor") ||
                                      positionName.Contains("tổ trưởng") || positionName.Contains("qc") ||
                                      positionName.Contains("quản đốc") || positionName.Contains("manager") ||
                                      positionName.Contains("supervisor");
                        
                        var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                                                   .Select(c => c.Value.ToLower()).ToList();
                        isAuthorized = isAuthorized || userRoles.Contains("manager") || 
                                      userRoles.Contains("supervisor") || userRoles.Contains("admin") || 
                                      userRoles.Contains("superadmin") || userRoles.Contains("qc");
                    }
                }
                
                if (!isAuthorized)
                {
                    return Forbid("Chỉ Tổ trưởng, QC hoặc Quản đốc mới có quyền hoàn thành QC");
                }

                await _unitOfWork.BeginTransactionAsync();

                var order = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
                if (order == null)
                {
                    return NotFound(ApiResponse<QualityControlDto>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                if (order.Status != "QCInProgress")
                {
                    return BadRequest(ApiResponse<QualityControlDto>.ErrorResult(
                        $"JO không ở trạng thái 'Đang kiểm tra QC'. Trạng thái hiện tại: {order.Status}"));
                }

                // ✅ OPTIMIZED: Query ở database level
                var qcList = (await _unitOfWork.Repository<Core.Entities.QualityControl>()
                    .FindAsync(q => q.ServiceOrderId == id && !q.IsDeleted))
                    .OrderByDescending(q => q.QCDate)
                    .ToList();

                var qc = qcList.FirstOrDefault();
                if (qc == null)
                {
                    return NotFound(ApiResponse<QualityControlDto>.ErrorResult("Không tìm thấy bản ghi QC"));
                }

                // ✅ VALIDATION: Kiểm tra QC record đang ở trạng thái nào
                if (qc.QCResult != "Pending")
                {
                    return BadRequest(ApiResponse<QualityControlDto>.ErrorResult(
                        $"QC record đã được xử lý. Kết quả hiện tại: {qc.QCResult}. " +
                        "Không thể hoàn thành QC lại."));
                }
                
                // Load navigation properties
                await _context.Entry(qc)
                    .Collection(q => q.QCChecklistItems)
                    .LoadAsync();

                // Cập nhật kết quả QC
                qc.QCResult = completeDto.QCResult;
                qc.QCNotes = completeDto.QCNotes;
                qc.ReworkRequired = completeDto.ReworkRequired;
                qc.ReworkNotes = completeDto.ReworkNotes;
                qc.QCCompletedDate = DateTime.Now;
                qc.UpdatedAt = DateTime.Now;

                // Cập nhật checklist items
                if (completeDto.ChecklistItems != null && completeDto.ChecklistItems.Any())
                {
                    // Xóa checklist items cũ
                    foreach (var oldItem in qc.QCChecklistItems)
                    {
                        oldItem.IsDeleted = true;
                        oldItem.UpdatedAt = DateTime.Now;
                    }

                    // Thêm checklist items mới
                    foreach (var itemDto in completeDto.ChecklistItems)
                    {
                        var checklistItem = new Core.Entities.QCChecklistItem
                        {
                            QualityControlId = qc.Id,
                            ChecklistItemName = itemDto.ChecklistItemName,
                            IsChecked = itemDto.IsChecked,
                            Result = itemDto.Result,
                            Notes = itemDto.Notes,
                            DisplayOrder = itemDto.DisplayOrder,
                            CreatedAt = DateTime.Now
                        };
                        await _unitOfWork.Repository<Core.Entities.QCChecklistItem>().AddAsync(checklistItem);
                    }
                }

                await _unitOfWork.Repository<Core.Entities.QualityControl>().UpdateAsync(qc);

                // Xử lý kết quả QC
                if (completeDto.QCResult == "Pass")
                {
                    // QC Đạt → Chuyển sang "ReadyToBill"
                    order.Status = "ReadyToBill";
                }
                else if (completeDto.QCResult == "Fail")
                {
                    // QC Không đạt → Chuyển về "InProgress" và tăng QCFailedCount
                    order.Status = "InProgress";
                    order.QCFailedCount++;
                }

                await _unitOfWork.ServiceOrders.UpdateAsync(order);
                await _unitOfWork.CommitTransactionAsync();

                // Reload để lấy dữ liệu mới nhất
                qc = await _unitOfWork.Repository<Core.Entities.QualityControl>().GetByIdAsync(qc.Id);
                if (qc != null)
                {
                    await _context.Entry(qc)
                        .Collection(q => q.QCChecklistItems)
                        .LoadAsync();
                    await _context.Entry(qc)
                        .Reference(q => q.QCInspector)
                        .LoadAsync();
                    await _context.Entry(qc)
                        .Reference(q => q.ServiceOrder)
                        .LoadAsync();
                }

                var qcDto = await MapToDtoAsync(qc!);
                return Ok(ApiResponse<QualityControlDto>.SuccessResult(qcDto, 
                    completeDto.QCResult == "Pass" ? "QC đạt. JO đã sẵn sàng thanh toán." : "QC không đạt. JO đã được trả về KTV làm lại."));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error completing QC for service order {ServiceOrderId}", id);
                return StatusCode(500, ApiResponse<QualityControlDto>.ErrorResult("Lỗi khi hoàn thành QC", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 2.4.2: Lấy thông tin QC của JO
        /// </summary>
        [HttpGet("service-orders/{id}/qc")]
        public async Task<ActionResult<ApiResponse<QualityControlDto>>> GetQC(int id)
        {
            try
            {
                // ✅ OPTIMIZED: Query ở database level
                var qcList = (await _unitOfWork.Repository<Core.Entities.QualityControl>()
                    .FindAsync(q => q.ServiceOrderId == id && !q.IsDeleted))
                    .OrderByDescending(q => q.QCDate)
                    .ToList();

                var qc = qcList.FirstOrDefault();
                if (qc == null)
                {
                    return NotFound(ApiResponse<QualityControlDto>.ErrorResult("Không tìm thấy bản ghi QC"));
                }

                // Load navigation properties
                await _context.Entry(qc)
                    .Collection(q => q.QCChecklistItems)
                    .Query()
                    .Where(i => !i.IsDeleted)
                    .LoadAsync();
                await _context.Entry(qc)
                    .Reference(q => q.QCInspector)
                    .LoadAsync();
                await _context.Entry(qc)
                    .Reference(q => q.ServiceOrder)
                    .LoadAsync();

                var qcDto = await MapToDtoAsync(qc);
                return Ok(ApiResponse<QualityControlDto>.SuccessResult(qcDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting QC for service order {ServiceOrderId}", id);
                return StatusCode(500, ApiResponse<QualityControlDto>.ErrorResult("Lỗi khi lấy thông tin QC", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 2.4.3: Ghi nhận QC không đạt, trả về KTV làm lại
        /// </summary>
        [HttpPost("service-orders/{id}/qc/fail")]
        public async Task<ActionResult<ApiResponse<ServiceOrderDto>>> FailQC(int id, [FromBody] CompleteQCDto failDto)
        {
            try
            {
                // ✅ VALIDATION: Check ModelState
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult("Dữ liệu không hợp lệ"));
                }

                // ✅ AUTHORIZATION: Chỉ Tổ trưởng/QC mới có quyền ghi nhận QC không đạt
                var currentUserId = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                bool isAuthorized = false;
                
                if (!string.IsNullOrEmpty(currentUserId) && int.TryParse(currentUserId, out var userId))
                {
                    var currentEmployee = await _unitOfWork.Employees.GetByIdAsync(userId);
                    if (currentEmployee != null)
                    {
                        var position = (currentEmployee.Position ?? "").ToLower();
                        var positionName = currentEmployee.PositionNavigation?.Name?.ToLower() ?? "";
                        
                        isAuthorized = position.Contains("tổ trưởng") || position.Contains("qc") ||
                                      position.Contains("quản đốc") || position.Contains("manager") ||
                                      position.Contains("supervisor") ||
                                      positionName.Contains("tổ trưởng") || positionName.Contains("qc") ||
                                      positionName.Contains("quản đốc") || positionName.Contains("manager") ||
                                      positionName.Contains("supervisor");
                        
                        var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                                                   .Select(c => c.Value.ToLower()).ToList();
                        isAuthorized = isAuthorized || userRoles.Contains("manager") || 
                                      userRoles.Contains("supervisor") || userRoles.Contains("admin") || 
                                      userRoles.Contains("superadmin") || userRoles.Contains("qc");
                    }
                }
                
                if (!isAuthorized)
                {
                    return Forbid("Chỉ Tổ trưởng, QC hoặc Quản đốc mới có quyền ghi nhận QC không đạt");
                }

                await _unitOfWork.BeginTransactionAsync();

                var order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                if (order == null)
                {
                    return NotFound(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                // ✅ VALIDATION: Chỉ cho phép fail QC khi status là "QCInProgress"
                if (order.Status != "QCInProgress")
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult(
                        $"JO không ở trạng thái 'Đang kiểm tra QC'. Trạng thái hiện tại: {order.Status}"));
                }

                // ✅ OPTIMIZED: Query ở database level
                var qcList = (await _unitOfWork.Repository<Core.Entities.QualityControl>()
                    .FindAsync(q => q.ServiceOrderId == id && !q.IsDeleted))
                    .OrderByDescending(q => q.QCDate)
                    .ToList();

                var qc = qcList.FirstOrDefault();
                if (qc == null)
                {
                    return NotFound(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy bản ghi QC"));
                }
                
                // Load navigation properties
                await _context.Entry(qc)
                    .Collection(q => q.QCChecklistItems)
                    .LoadAsync();

                // Cập nhật QC result
                qc.QCResult = "Fail";
                qc.QCNotes = failDto.QCNotes;
                qc.ReworkRequired = true;
                qc.ReworkNotes = failDto.ReworkNotes;
                qc.QCCompletedDate = DateTime.Now;
                qc.UpdatedAt = DateTime.Now;

                await _unitOfWork.Repository<Core.Entities.QualityControl>().UpdateAsync(qc);

                // Chuyển JO về "InProgress" và tăng QCFailedCount
                order.Status = "InProgress";
                order.QCFailedCount++;

                await _unitOfWork.ServiceOrders.UpdateAsync(order);
                await _unitOfWork.CommitTransactionAsync();

                // Reload để lấy dữ liệu mới nhất
                order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                var orderDto = _mapper.Map<ServiceOrderDto>(order);
                orderDto.QCFailedCount = order.QCFailedCount;
                
                return Ok(ApiResponse<ServiceOrderDto>.SuccessResult(orderDto, "QC không đạt. JO đã được trả về KTV làm lại."));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error failing QC for service order {ServiceOrderId}", id);
                return StatusCode(500, ApiResponse<ServiceOrderDto>.ErrorResult("Lỗi khi ghi nhận QC không đạt", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 2.4.4: Bàn giao xe và chuyển sang "ReadyToBill"
        /// </summary>
        [HttpPost("service-orders/{id}/handover")]
        public async Task<ActionResult<ApiResponse<ServiceOrderDto>>> Handover(int id, [FromBody] HandoverServiceOrderDto handoverDto)
        {
            try
            {
                // ✅ VALIDATION: Check ModelState
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult("Dữ liệu không hợp lệ"));
                }

                // ✅ AUTHORIZATION: Chỉ Cố vấn Dịch vụ mới có quyền bàn giao xe
                var currentUserId = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                bool isAuthorized = false;
                
                if (!string.IsNullOrEmpty(currentUserId) && int.TryParse(currentUserId, out var userId))
                {
                    var currentEmployee = await _unitOfWork.Employees.GetByIdAsync(userId);
                    if (currentEmployee != null)
                    {
                        var position = (currentEmployee.Position ?? "").ToLower();
                        var positionName = currentEmployee.PositionNavigation?.Name?.ToLower() ?? "";
                        
                        isAuthorized = position.Contains("cố vấn") || position.Contains("dịch vụ") ||
                                      position.Contains("tư vấn") || position.Contains("advisor") ||
                                      position.Contains("quản đốc") || position.Contains("manager") ||
                                      positionName.Contains("cố vấn") || positionName.Contains("dịch vụ") ||
                                      positionName.Contains("tư vấn") || positionName.Contains("advisor") ||
                                      positionName.Contains("quản đốc") || positionName.Contains("manager");
                        
                        var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                                                   .Select(c => c.Value.ToLower()).ToList();
                        isAuthorized = isAuthorized || userRoles.Contains("manager") || 
                                      userRoles.Contains("advisor") || userRoles.Contains("admin") || 
                                      userRoles.Contains("superadmin");
                    }
                }
                
                if (!isAuthorized)
                {
                    return Forbid("Chỉ Cố vấn Dịch vụ hoặc Quản đốc mới có quyền bàn giao xe");
                }

                await _unitOfWork.BeginTransactionAsync();

                var order = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
                if (order == null)
                {
                    return NotFound(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                // ✅ OPTIMIZED: Query ở database level
                var qcList = (await _unitOfWork.Repository<Core.Entities.QualityControl>()
                    .FindAsync(q => q.ServiceOrderId == id && !q.IsDeleted))
                    .OrderByDescending(q => q.QCDate)
                    .ToList();

                var qc = qcList.FirstOrDefault();

                if (qc == null || qc.QCResult != "Pass")
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult(
                        "QC chưa đạt. Vui lòng hoàn thành QC trước khi bàn giao xe."));
                }

                // Cập nhật thông tin bàn giao
                order.HandoverDate = handoverDto.HandoverDate ?? DateTime.Now;
                order.HandoverLocation = handoverDto.HandoverLocation;
                order.Status = "ReadyToBill";

                // Cập nhật trạng thái xe (nếu có Vehicle entity)
                var vehicle = await _unitOfWork.Repository<Core.Entities.Vehicle>().GetByIdAsync(order.VehicleId);
                if (vehicle != null)
                {
                    // Có thể cập nhật Vehicle.Status nếu cần
                    // vehicle.Status = "ReadyForDelivery";
                }

                await _unitOfWork.ServiceOrders.UpdateAsync(order);
                await _unitOfWork.CommitTransactionAsync();

                // Reload để lấy dữ liệu mới nhất
                order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                var orderDto = _mapper.Map<ServiceOrderDto>(order);
                orderDto.HandoverDate = order.HandoverDate;
                orderDto.HandoverLocation = order.HandoverLocation;
                
                return Ok(ApiResponse<ServiceOrderDto>.SuccessResult(orderDto, "Đã bàn giao xe thành công. JO đã sẵn sàng thanh toán."));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error handing over service order {ServiceOrderId}", id);
                return StatusCode(500, ApiResponse<ServiceOrderDto>.ErrorResult("Lỗi khi bàn giao xe", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 2.4.3: Ghi nhận giờ công làm lại cho item (khi QC không đạt)
        /// </summary>
        [HttpPost("service-orders/{id}/items/{itemId}/rework")]
        public async Task<ActionResult<ApiResponse<ServiceOrderDto>>> RecordReworkHours(
            int id, 
            int itemId, 
            [FromBody] RecordReworkHoursDto reworkDto)
        {
            try
            {
                // ✅ VALIDATION: Check ModelState
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult("Dữ liệu không hợp lệ"));
                }

                await _unitOfWork.BeginTransactionAsync();

                var order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                if (order == null)
                {
                    return NotFound(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                var item = order.ServiceOrderItems.FirstOrDefault(i => i.Id == itemId && !i.IsDeleted);
                if (item == null)
                {
                    return NotFound(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy hạng mục"));
                }

                // ✅ VALIDATION: Kiểm tra JO đã QC không đạt chưa
                var qcList = (await _unitOfWork.Repository<Core.Entities.QualityControl>()
                    .FindAsync(q => q.ServiceOrderId == id && !q.IsDeleted))
                    .OrderByDescending(q => q.QCDate)
                    .ToList();

                var latestQC = qcList.FirstOrDefault();
                if (latestQC == null || latestQC.QCResult != "Fail")
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult(
                        "Chỉ có thể ghi nhận giờ công làm lại khi QC không đạt"));
                }

                // Cập nhật giờ công làm lại
                item.ReworkHours = (item.ReworkHours ?? 0) + reworkDto.ReworkHours;
                item.UpdatedAt = DateTime.Now;

                await _unitOfWork.Repository<Core.Entities.ServiceOrderItem>().UpdateAsync(item);
                await _unitOfWork.CommitTransactionAsync();

                // Reload để lấy dữ liệu mới nhất
                order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                var orderDto = _mapper.Map<ServiceOrderDto>(order);
                
                return Ok(ApiResponse<ServiceOrderDto>.SuccessResult(orderDto, 
                    $"Đã ghi nhận {reworkDto.ReworkHours:F2} giờ công làm lại cho hạng mục '{item.ServiceName}'"));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error recording rework hours for service order {ServiceOrderId}, item {ItemId}", id, itemId);
                return StatusCode(500, ApiResponse<ServiceOrderDto>.ErrorResult("Lỗi khi ghi nhận giờ công làm lại", ex.Message));
            }
        }

        // Helper methods
        private async Task<QualityControlDto> MapToDtoAsync(Core.Entities.QualityControl qc)
        {
            var dto = _mapper.Map<QualityControlDto>(qc);
            dto.OrderNumber = qc.ServiceOrder?.OrderNumber ?? string.Empty;
            dto.QCInspectorName = qc.QCInspector?.Name;
            
            if (qc.QCChecklistItems != null)
            {
                dto.QCChecklistItems = qc.QCChecklistItems
                    .Where(i => !i.IsDeleted)
                    .Select(i => new QCChecklistItemDto
                    {
                        Id = i.Id,
                        QualityControlId = i.QualityControlId,
                        ChecklistItemName = i.ChecklistItemName,
                        IsChecked = i.IsChecked,
                        Result = i.Result,
                        Notes = i.Notes,
                        DisplayOrder = i.DisplayOrder,
                        CreatedAt = i.CreatedAt,
                        UpdatedAt = i.UpdatedAt
                    })
                    .ToList();
            }
            
            return dto;
        }
    }
}

