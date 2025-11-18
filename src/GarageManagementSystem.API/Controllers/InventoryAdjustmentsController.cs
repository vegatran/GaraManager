using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Enums;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiScope")]
    public class InventoryAdjustmentsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly GarageDbContext _context;
        private readonly Services.INotificationService? _notificationService;
        private readonly ILogger<InventoryAdjustmentsController>? _logger;

        public InventoryAdjustmentsController(
            IUnitOfWork unitOfWork, 
            IMapper mapper, 
            GarageDbContext context,
            Services.INotificationService? notificationService = null,
            ILogger<InventoryAdjustmentsController>? logger = null)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách phiếu điều chỉnh tồn kho
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<InventoryAdjustmentDto>>>> GetInventoryAdjustments(
            [FromQuery] int? warehouseId = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = _context.InventoryAdjustments
                    .AsNoTracking()
                    .Where(ia => !ia.IsDeleted)
                    .Include(ia => ia.InventoryCheck)
                    .Include(ia => ia.Warehouse)
                    .Include(ia => ia.WarehouseZone)
                    .Include(ia => ia.WarehouseBin)
                    .Include(ia => ia.ApprovedByEmployee)
                    .Include(ia => ia.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Part)
                    .AsQueryable();

                // Apply filters
                if (warehouseId.HasValue)
                    query = query.Where(ia => ia.WarehouseId == warehouseId.Value);
                if (!string.IsNullOrEmpty(status))
                    query = query.Where(ia => ia.Status == status);
                if (startDate.HasValue)
                    query = query.Where(ia => ia.AdjustmentDate >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(ia => ia.AdjustmentDate <= endDate.Value);

                var adjustments = await query
                    .OrderByDescending(ia => ia.AdjustmentDate)
                    .ThenByDescending(ia => ia.CreatedAt)
                    .ToListAsync();

                var dto = adjustments.Select(ia => new InventoryAdjustmentDto
                {
                    Id = ia.Id,
                    AdjustmentNumber = ia.AdjustmentNumber,
                    InventoryCheckId = ia.InventoryCheckId,
                    InventoryCheckCode = ia.InventoryCheck?.Code,
                    InventoryCheckName = ia.InventoryCheck?.Name,
                    WarehouseId = ia.WarehouseId,
                    WarehouseName = ia.Warehouse?.Name,
                    WarehouseZoneId = ia.WarehouseZoneId,
                    WarehouseZoneName = ia.WarehouseZone?.Name,
                    WarehouseBinId = ia.WarehouseBinId,
                    WarehouseBinName = ia.WarehouseBin?.Name,
                    AdjustmentDate = ia.AdjustmentDate,
                    Status = ia.Status,
                    Reason = ia.Reason,
                    ApprovedByEmployeeId = ia.ApprovedByEmployeeId,
                    ApprovedByEmployeeName = ia.ApprovedByEmployee?.Name,
                    ApprovedAt = ia.ApprovedAt,
                    RejectionReason = ia.RejectionReason,
                    Notes = ia.Notes,
                    Items = ia.Items.Select(i => new InventoryAdjustmentItemDto
                    {
                        Id = i.Id,
                        InventoryAdjustmentId = i.InventoryAdjustmentId,
                        PartId = i.PartId,
                        PartNumber = i.Part?.PartNumber,
                        PartName = i.Part?.PartName,
                        PartSku = i.Part?.Sku,
                        InventoryCheckItemId = i.InventoryCheckItemId,
                        QuantityChange = i.QuantityChange,
                        SystemQuantityBefore = i.SystemQuantityBefore,
                        SystemQuantityAfter = i.SystemQuantityAfter,
                        Notes = i.Notes
                    }).ToList(),
                    CreatedAt = ia.CreatedAt,
                    UpdatedAt = ia.UpdatedAt
                }).ToList();

                return Ok(ApiResponse<List<InventoryAdjustmentDto>>.SuccessResult(dto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<InventoryAdjustmentDto>>.ErrorResult("Error retrieving inventory adjustments", ex.Message));
            }
        }

        /// <summary>
        /// Lấy chi tiết phiếu điều chỉnh tồn kho
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<InventoryAdjustmentDto>>> GetInventoryAdjustment(int id)
        {
            try
            {
                var adjustment = await _context.InventoryAdjustments
                    .AsNoTracking()
                    .Where(ia => !ia.IsDeleted && ia.Id == id)
                    .Include(ia => ia.InventoryCheck)
                    .Include(ia => ia.Warehouse)
                    .Include(ia => ia.WarehouseZone)
                    .Include(ia => ia.WarehouseBin)
                    .Include(ia => ia.ApprovedByEmployee)
                    .Include(ia => ia.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Part)
                    .FirstOrDefaultAsync();

                if (adjustment == null)
                {
                    return NotFound(ApiResponse<InventoryAdjustmentDto>.ErrorResult("Inventory adjustment not found"));
                }

                var dto = new InventoryAdjustmentDto
                {
                    Id = adjustment.Id,
                    AdjustmentNumber = adjustment.AdjustmentNumber,
                    InventoryCheckId = adjustment.InventoryCheckId,
                    InventoryCheckCode = adjustment.InventoryCheck?.Code,
                    InventoryCheckName = adjustment.InventoryCheck?.Name,
                    WarehouseId = adjustment.WarehouseId,
                    WarehouseName = adjustment.Warehouse?.Name,
                    WarehouseZoneId = adjustment.WarehouseZoneId,
                    WarehouseZoneName = adjustment.WarehouseZone?.Name,
                    WarehouseBinId = adjustment.WarehouseBinId,
                    WarehouseBinName = adjustment.WarehouseBin?.Name,
                    AdjustmentDate = adjustment.AdjustmentDate,
                    Status = adjustment.Status,
                    Reason = adjustment.Reason,
                    ApprovedByEmployeeId = adjustment.ApprovedByEmployeeId,
                    ApprovedByEmployeeName = adjustment.ApprovedByEmployee?.Name,
                    ApprovedAt = adjustment.ApprovedAt,
                    RejectionReason = adjustment.RejectionReason,
                    Notes = adjustment.Notes,
                    Items = adjustment.Items.Select(i => new InventoryAdjustmentItemDto
                    {
                        Id = i.Id,
                        InventoryAdjustmentId = i.InventoryAdjustmentId,
                        PartId = i.PartId,
                        PartNumber = i.Part?.PartNumber,
                        PartName = i.Part?.PartName,
                        PartSku = i.Part?.Sku,
                        InventoryCheckItemId = i.InventoryCheckItemId,
                        QuantityChange = i.QuantityChange,
                        SystemQuantityBefore = i.SystemQuantityBefore,
                        SystemQuantityAfter = i.SystemQuantityAfter,
                        Notes = i.Notes
                    }).ToList(),
                    CreatedAt = adjustment.CreatedAt,
                    UpdatedAt = adjustment.UpdatedAt
                };

                return Ok(ApiResponse<InventoryAdjustmentDto>.SuccessResult(dto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InventoryAdjustmentDto>.ErrorResult("Error retrieving inventory adjustment", ex.Message));
            }
        }

        /// <summary>
        /// Tạo phiếu điều chỉnh tồn kho từ kiểm kê
        /// </summary>
        [HttpPost("from-check/{checkId}")]
        public async Task<ActionResult<ApiResponse<InventoryAdjustmentDto>>> CreateAdjustmentFromCheck(int checkId, [FromBody] CreateInventoryAdjustmentFromCheckDto? dto = null)
        {
            try
            {
                // Get inventory check with items
                var inventoryCheck = await _context.InventoryChecks
                    .Where(ic => !ic.IsDeleted && ic.Id == checkId)
                    .Include(ic => ic.Items.Where(i => !i.IsDeleted && i.IsDiscrepancy && !i.IsAdjusted))
                    .ThenInclude(i => i.Part)
                    .FirstOrDefaultAsync();

                if (inventoryCheck == null)
                {
                    return NotFound(ApiResponse<InventoryAdjustmentDto>.ErrorResult("Inventory check not found"));
                }

                if (inventoryCheck.Status != "Completed")
                {
                    return BadRequest(ApiResponse<InventoryAdjustmentDto>.ErrorResult("Chỉ có thể tạo điều chỉnh từ phiếu kiểm kê đã hoàn thành"));
                }

                var discrepancyItems = inventoryCheck.Items.Where(i => i.IsDiscrepancy && !i.IsAdjusted).ToList();
                if (!discrepancyItems.Any())
                {
                    return BadRequest(ApiResponse<InventoryAdjustmentDto>.ErrorResult("Không có items có chênh lệch chưa được điều chỉnh"));
                }

                // Validate reason if provided
                if (dto != null && string.IsNullOrWhiteSpace(dto.Reason))
                {
                    return BadRequest(ApiResponse<InventoryAdjustmentDto>.ErrorResult("Vui lòng nhập lý do điều chỉnh"));
                }

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Generate adjustment number
                    var adjustmentNumber = await GenerateAdjustmentNumberAsync();

                    // Create adjustment - use DTO values if provided, otherwise use defaults
                    var adjustment = new InventoryAdjustment
                    {
                        AdjustmentNumber = adjustmentNumber,
                        InventoryCheckId = checkId,
                        WarehouseId = inventoryCheck.WarehouseId,
                        WarehouseZoneId = inventoryCheck.WarehouseZoneId,
                        WarehouseBinId = inventoryCheck.WarehouseBinId,
                        AdjustmentDate = DateTime.Now,
                        Status = "Pending",
                        Reason = !string.IsNullOrWhiteSpace(dto?.Reason) 
                            ? dto.Reason 
                            : $"Điều chỉnh từ phiếu kiểm kê {inventoryCheck.Code}",
                        Notes = !string.IsNullOrWhiteSpace(dto?.Notes) 
                            ? dto.Notes 
                            : $"Tự động tạo từ phiếu kiểm kê: {inventoryCheck.Name}"
                    };

                    await _unitOfWork.InventoryAdjustments.AddAsync(adjustment);
                    await _unitOfWork.SaveChangesAsync();

                    // Create adjustment items
                    var adjustmentItems = new List<InventoryAdjustmentItem>();
                    foreach (var checkItem in discrepancyItems)
                    {
                        var part = checkItem.Part;
                        if (part == null || part.IsDeleted)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return BadRequest(ApiResponse<InventoryAdjustmentDto>.ErrorResult($"Phụ tùng không tồn tại hoặc đã bị xóa cho InventoryCheckItem ID {checkItem.Id}"));
                        }
                        
                        var quantityChange = checkItem.DiscrepancyQuantity; // ActualQuantity - SystemQuantity
                        var systemQuantityBefore = part.QuantityInStock;
                        var systemQuantityAfter = systemQuantityBefore + quantityChange;

                        // Validate: SystemQuantityAfter không được âm
                        if (systemQuantityAfter < 0)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return BadRequest(ApiResponse<InventoryAdjustmentDto>.ErrorResult($"Số lượng sau điều chỉnh không được âm cho phụ tùng {part.PartNumber}. Tồn kho hiện tại: {systemQuantityBefore}, Thay đổi: {quantityChange}"));
                        }

                        var adjustmentItem = new InventoryAdjustmentItem
                        {
                            InventoryAdjustmentId = adjustment.Id,
                            PartId = checkItem.PartId,
                            InventoryCheckItemId = checkItem.Id,
                            QuantityChange = quantityChange,
                            SystemQuantityBefore = systemQuantityBefore,
                            SystemQuantityAfter = systemQuantityAfter,
                            Notes = checkItem.Notes
                        };

                        await _unitOfWork.InventoryAdjustmentItems.AddAsync(adjustmentItem);
                        adjustmentItems.Add(adjustmentItem);
                    }

                    // Save to get IDs
                    await _unitOfWork.SaveChangesAsync();

                    // Now update check items with adjustment item IDs
                    for (int i = 0; i < discrepancyItems.Count; i++)
                    {
                        var checkItem = discrepancyItems[i];
                        var adjustmentItem = adjustmentItems[i];
                        checkItem.IsAdjusted = true;
                        checkItem.InventoryAdjustmentItemId = adjustmentItem.Id;
                        await _unitOfWork.InventoryCheckItems.UpdateAsync(checkItem);
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    // Reload with includes
                    var createdAdjustment = await _context.InventoryAdjustments
                        .Where(ia => ia.Id == adjustment.Id)
                        .Include(ia => ia.InventoryCheck)
                        .Include(ia => ia.Warehouse)
                        .Include(ia => ia.WarehouseZone)
                        .Include(ia => ia.WarehouseBin)
                        .Include(ia => ia.Items.Where(i => !i.IsDeleted))
                        .ThenInclude(i => i.Part)
                        .FirstOrDefaultAsync();

                    var adjustmentDto = MapToDto(createdAdjustment!);
                    
                    // ✅ Phase 4.1 - Advanced Features: Log audit
                    // ✅ FIX: Handle null dto.Reason - use parameter dto, not adjustmentDto
                    var reasonText = dto?.Reason ?? "Không có";
                    await LogAuditAsync("InventoryAdjustment", createdAdjustment.Id, "CreateFromCheck", 
                        $"Tạo điều chỉnh từ kiểm kê: {createdAdjustment.AdjustmentNumber}, Từ check: {inventoryCheck.Code}, Items: {adjustmentItems.Count}, Reason: {reasonText}", 
                        "Info");

                    return Ok(ApiResponse<InventoryAdjustmentDto>.SuccessResult(adjustmentDto));
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InventoryAdjustmentDto>.ErrorResult("Error creating inventory adjustment from check", ex.Message));
            }
        }

        /// <summary>
        /// Tạo phiếu điều chỉnh tồn kho thủ công
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<InventoryAdjustmentDto>>> CreateInventoryAdjustment(CreateInventoryAdjustmentDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<InventoryAdjustmentDto>.ErrorResult("Invalid data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                }

                if (dto.Items == null || !dto.Items.Any())
                {
                    return BadRequest(ApiResponse<InventoryAdjustmentDto>.ErrorResult("Phải có ít nhất một item"));
                }

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Generate adjustment number
                    var adjustmentNumber = await GenerateAdjustmentNumberAsync();

                    // Create adjustment
                    var adjustment = new InventoryAdjustment
                    {
                        AdjustmentNumber = adjustmentNumber,
                        InventoryCheckId = dto.InventoryCheckId,
                        WarehouseId = dto.WarehouseId,
                        WarehouseZoneId = dto.WarehouseZoneId,
                        WarehouseBinId = dto.WarehouseBinId,
                        AdjustmentDate = dto.AdjustmentDate,
                        Status = "Pending",
                        Reason = dto.Reason,
                        Notes = dto.Notes
                    };

                    await _unitOfWork.InventoryAdjustments.AddAsync(adjustment);
                    await _unitOfWork.SaveChangesAsync();

                    // Create adjustment items
                    foreach (var itemDto in dto.Items)
                    {
                        var part = await _unitOfWork.Parts.GetByIdAsync(itemDto.PartId);
                        if (part == null || part.IsDeleted)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return BadRequest(ApiResponse<InventoryAdjustmentDto>.ErrorResult($"Phụ tùng với ID {itemDto.PartId} không tồn tại hoặc đã bị xóa"));
                        }

                        // Validate quantities
                        if (itemDto.SystemQuantityAfter != itemDto.SystemQuantityBefore + itemDto.QuantityChange)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return BadRequest(ApiResponse<InventoryAdjustmentDto>.ErrorResult($"Tính toán số lượng không hợp lệ cho phụ tùng {part.PartNumber}"));
                        }

                        // Validate: SystemQuantityAfter không được âm
                        if (itemDto.SystemQuantityAfter < 0)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return BadRequest(ApiResponse<InventoryAdjustmentDto>.ErrorResult($"Số lượng sau điều chỉnh không được âm cho phụ tùng {part.PartNumber}"));
                        }

                        // Validate: SystemQuantityBefore phải match với Part.QuantityInStock (cho manual adjustment)
                        if (itemDto.SystemQuantityBefore != part.QuantityInStock)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return BadRequest(ApiResponse<InventoryAdjustmentDto>.ErrorResult($"Số lượng trước điều chỉnh không khớp với tồn kho hiện tại cho phụ tùng {part.PartNumber}. Tồn kho hiện tại: {part.QuantityInStock}, Giá trị nhập: {itemDto.SystemQuantityBefore}"));
                        }

                        var adjustmentItem = new InventoryAdjustmentItem
                        {
                            InventoryAdjustmentId = adjustment.Id,
                            PartId = itemDto.PartId,
                            InventoryCheckItemId = itemDto.InventoryCheckItemId,
                            QuantityChange = itemDto.QuantityChange,
                            SystemQuantityBefore = itemDto.SystemQuantityBefore,
                            SystemQuantityAfter = itemDto.SystemQuantityAfter,
                            Notes = itemDto.Notes
                        };

                        await _unitOfWork.InventoryAdjustmentItems.AddAsync(adjustmentItem);
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    // Reload with includes
                    var createdAdjustment = await _context.InventoryAdjustments
                        .Where(ia => ia.Id == adjustment.Id)
                        .Include(ia => ia.InventoryCheck)
                        .Include(ia => ia.Warehouse)
                        .Include(ia => ia.WarehouseZone)
                        .Include(ia => ia.WarehouseBin)
                        .Include(ia => ia.Items.Where(i => !i.IsDeleted))
                        .ThenInclude(i => i.Part)
                        .FirstOrDefaultAsync();

                    var resultDto = MapToDto(createdAdjustment!);
                    
                    // ✅ Phase 4.1 - Advanced Features: Log audit
                    // ✅ FIX: Handle null dto.Reason
                    var reasonText = dto?.Reason ?? "Không có";
                    await LogAuditAsync("InventoryAdjustment", createdAdjustment.Id, "Create", 
                        $"Tạo điều chỉnh thủ công: {createdAdjustment.AdjustmentNumber}, Items: {dto.Items?.Count ?? 0}, Reason: {reasonText}", 
                        "Info");

                    return Ok(ApiResponse<InventoryAdjustmentDto>.SuccessResult(resultDto));
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InventoryAdjustmentDto>.ErrorResult("Error creating inventory adjustment", ex.Message));
            }
        }

        /// <summary>
        /// Duyệt phiếu điều chỉnh tồn kho
        /// </summary>
        [HttpPost("{id}/approve")]
        public async Task<ActionResult<ApiResponse<InventoryAdjustmentDto>>> ApproveAdjustment(int id, ApproveInventoryAdjustmentDto dto)
        {
            try
            {
                var adjustment = await _context.InventoryAdjustments
                    .Where(ia => !ia.IsDeleted && ia.Id == id)
                    .Include(ia => ia.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Part)
                    .FirstOrDefaultAsync();

                if (adjustment == null)
                {
                    return NotFound(ApiResponse<InventoryAdjustmentDto>.ErrorResult("Inventory adjustment not found"));
                }

                if (adjustment.Status != "Pending")
                {
                    return BadRequest(ApiResponse<InventoryAdjustmentDto>.ErrorResult($"Không thể duyệt phiếu điều chỉnh với trạng thái {adjustment.Status}"));
                }

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Get current user
                    var currentUserId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    int? userId = null;
                    if (!string.IsNullOrEmpty(currentUserId) && int.TryParse(currentUserId, out var parsedUserId))
                    {
                        userId = parsedUserId;
                    }

                    // ✅ FIX: Process items first, then update adjustment status
                    // This ensures data consistency: only mark as Approved if all items are processed successfully
                    
                    // ✅ FIX: Collect parts to update (tránh update nhiều lần cùng part nếu có nhiều items)
                    var partsToUpdate = new Dictionary<int, Part>();
                    var stockTransactions = new List<StockTransaction>();

                    // Update part quantities and create stock transactions
                    foreach (var item in adjustment.Items)
                    {
                        var part = item.Part;
                        if (part == null)
                        {
                            part = await _unitOfWork.Parts.GetByIdAsync(item.PartId);
                        }

                        if (part == null || part.IsDeleted)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return BadRequest(ApiResponse<InventoryAdjustmentDto>.ErrorResult($"Phụ tùng với ID {item.PartId} không tồn tại hoặc đã bị xóa"));
                        }

                        // ✅ FIX: Reload part từ database để lấy quantity mới nhất (tránh race condition)
                        if (!partsToUpdate.ContainsKey(part.Id))
                        {
                            var freshPart = await _unitOfWork.Parts.GetByIdAsync(part.Id);
                            if (freshPart == null || freshPart.IsDeleted)
                            {
                                await _unitOfWork.RollbackTransactionAsync();
                                return BadRequest(ApiResponse<InventoryAdjustmentDto>.ErrorResult($"Phụ tùng với ID {item.PartId} không tồn tại hoặc đã bị xóa"));
                            }
                            partsToUpdate[part.Id] = freshPart;
                        }
                        part = partsToUpdate[part.Id];

                        // Validate: SystemQuantityAfter không được âm
                        if (item.SystemQuantityAfter < 0)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return BadRequest(ApiResponse<InventoryAdjustmentDto>.ErrorResult($"Số lượng sau điều chỉnh không được âm cho phụ tùng {part.PartNumber}"));
                        }

                        // ✅ FIX: Update part quantity (nếu có nhiều items cùng part, chỉ update 1 lần với giá trị cuối cùng)
                        part.QuantityInStock = item.SystemQuantityAfter;

                        // ✅ FIX: Create stock transaction với đầy đủ thông tin QuantityBefore, QuantityAfter, StockAfter
                        var quantityBefore = item.SystemQuantityBefore;
                        var quantityAfter = item.SystemQuantityAfter;
                        var transaction = new StockTransaction
                        {
                            PartId = item.PartId,
                            TransactionNumber = await GenerateTransactionNumberAsync(),
                            TransactionType = item.QuantityChange > 0 ? StockTransactionType.NhapKho : StockTransactionType.XuatKho,
                            Quantity = Math.Abs(item.QuantityChange),
                            QuantityBefore = quantityBefore,
                            QuantityAfter = quantityAfter,
                            StockAfter = quantityAfter, // ✅ FIX: Set StockAfter
                            UnitCost = part.CostPrice,
                            UnitPrice = part.SellPrice,
                            TotalCost = Math.Abs(item.QuantityChange) * part.CostPrice,
                            TotalAmount = Math.Abs(item.QuantityChange) * part.SellPrice,
                            TransactionDate = DateTime.Now,
                            ReferenceNumber = adjustment.AdjustmentNumber,
                            RelatedEntity = "InventoryAdjustment",
                            RelatedEntityId = adjustment.Id,
                            Notes = $"Điều chỉnh tồn kho: {(item.QuantityChange > 0 ? "Tăng" : "Giảm")} {Math.Abs(item.QuantityChange)}. {item.Notes ?? ""}",
                            ProcessedById = userId
                        };

                        stockTransactions.Add(transaction);
                    }

                    // ✅ FIX: Update parts một lần (tránh update nhiều lần cùng part)
                    foreach (var part in partsToUpdate.Values)
                    {
                        await _unitOfWork.Parts.UpdateAsync(part);
                    }

                    // ✅ FIX: Add stock transactions
                    foreach (var transaction in stockTransactions)
                    {
                        await _unitOfWork.StockTransactions.AddAsync(transaction);
                    }

                    // ✅ FIX: Only update adjustment status after all items are processed successfully
                    if (userId.HasValue)
                    {
                        adjustment.ApprovedByEmployeeId = userId.Value;
                    }
                    adjustment.Status = "Approved";
                    adjustment.ApprovedAt = DateTime.Now;
                    if (!string.IsNullOrEmpty(dto.Notes))
                    {
                        adjustment.Notes = dto.Notes;
                    }

                    await _unitOfWork.InventoryAdjustments.UpdateAsync(adjustment);

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    // Reload with includes
                    var updatedAdjustment = await _context.InventoryAdjustments
                        .Where(ia => ia.Id == adjustment.Id)
                        .Include(ia => ia.InventoryCheck)
                        .Include(ia => ia.Warehouse)
                        .Include(ia => ia.WarehouseZone)
                        .Include(ia => ia.WarehouseBin)
                        .Include(ia => ia.ApprovedByEmployee)
                        .Include(ia => ia.Items.Where(i => !i.IsDeleted))
                        .ThenInclude(i => i.Part)
                        .FirstOrDefaultAsync();

                    var resultDto = MapToDto(updatedAdjustment!);
                    
                    // ✅ Phase 4.1 - Advanced Features: Log audit
                    // ✅ FIX: Handle null dto.Notes
                    var itemsCount = updatedAdjustment.Items.Count;
                    var totalQtyChange = updatedAdjustment.Items.Sum(i => Math.Abs(i.QuantityChange));
                    var notesText = dto?.Notes ?? "Không có";
                    await LogAuditAsync("InventoryAdjustment", updatedAdjustment.Id, "Approve", 
                        $"Duyệt điều chỉnh: {updatedAdjustment.AdjustmentNumber}, Items: {itemsCount}, Tổng thay đổi: {totalQtyChange}, Notes: {notesText}", 
                        "Info");

                    // ✅ Real-time: Push notification for inventory alert count update
                    if (_notificationService != null)
                    {
                        try
                        {
                            // ✅ Performance: Use CountAsync instead of loading all alerts
                            var totalUnresolvedCount = await _unitOfWork.Repository<Core.Entities.InventoryAlert>()
                                .CountAsync(a => !a.IsResolved);
                            await _notificationService.NotifyInventoryAlertUpdatedAsync(totalUnresolvedCount);
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "Error sending inventory alert notification after approve");
                        }
                    }

                    return Ok(ApiResponse<InventoryAdjustmentDto>.SuccessResult(resultDto));
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InventoryAdjustmentDto>.ErrorResult("Error approving inventory adjustment", ex.Message));
            }
        }

        /// <summary>
        /// Từ chối phiếu điều chỉnh tồn kho
        /// </summary>
        [HttpPost("{id}/reject")]
        public async Task<ActionResult<ApiResponse<InventoryAdjustmentDto>>> RejectAdjustment(int id, RejectInventoryAdjustmentDto dto)
        {
            try
            {
                var adjustment = await _context.InventoryAdjustments
                    .Where(ia => !ia.IsDeleted && ia.Id == id)
                    .FirstOrDefaultAsync();

                if (adjustment == null)
                {
                    return NotFound(ApiResponse<InventoryAdjustmentDto>.ErrorResult("Inventory adjustment not found"));
                }

                if (adjustment.Status != "Pending")
                {
                    return BadRequest(ApiResponse<InventoryAdjustmentDto>.ErrorResult($"Không thể từ chối phiếu điều chỉnh với trạng thái {adjustment.Status}"));
                }

                adjustment.Status = "Rejected";
                adjustment.RejectionReason = dto.RejectionReason;
                adjustment.UpdatedAt = DateTime.Now;

                await _unitOfWork.InventoryAdjustments.UpdateAsync(adjustment);
                await _unitOfWork.SaveChangesAsync();

                // Reload with includes
                var updatedAdjustment = await _context.InventoryAdjustments
                    .Where(ia => ia.Id == adjustment.Id)
                    .Include(ia => ia.InventoryCheck)
                    .Include(ia => ia.Warehouse)
                    .Include(ia => ia.WarehouseZone)
                    .Include(ia => ia.WarehouseBin)
                    .Include(ia => ia.ApprovedByEmployee)
                    .Include(ia => ia.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Part)
                    .FirstOrDefaultAsync();

                var resultDto = MapToDto(updatedAdjustment!);
                
                // ✅ Phase 4.1 - Advanced Features: Log audit
                // ✅ FIX: Handle null dto.RejectionReason
                var rejectionReason = dto?.RejectionReason ?? "Không có";
                await LogAuditAsync("InventoryAdjustment", adjustment.Id, "Reject", 
                    $"Từ chối điều chỉnh: {adjustment.AdjustmentNumber}, Lý do: {rejectionReason}", 
                    "Warning");

                return Ok(ApiResponse<InventoryAdjustmentDto>.SuccessResult(resultDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InventoryAdjustmentDto>.ErrorResult("Error rejecting inventory adjustment", ex.Message));
            }
        }

        /// <summary>
        /// Xóa phiếu điều chỉnh tồn kho (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteInventoryAdjustment(int id)
        {
            try
            {
                var adjustment = await _unitOfWork.InventoryAdjustments.GetByIdAsync(id);
                if (adjustment == null || adjustment.IsDeleted)
                {
                    return NotFound(ApiResponse<bool>.ErrorResult("Inventory adjustment not found"));
                }

                if (adjustment.Status == "Approved")
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Không thể xóa phiếu điều chỉnh đã được duyệt"));
                }

                var adjustmentNumber = adjustment.AdjustmentNumber;
                var adjustmentStatus = adjustment.Status;
                adjustment.IsDeleted = true;
                await _unitOfWork.InventoryAdjustments.UpdateAsync(adjustment);
                await _unitOfWork.SaveChangesAsync();

                // ✅ Phase 4.1 - Advanced Features: Log audit
                await LogAuditAsync("InventoryAdjustment", id, "Delete", 
                    $"Xóa điều chỉnh: {adjustmentNumber}, Status: {adjustmentStatus}", 
                    "Warning");

                return Ok(ApiResponse<bool>.SuccessResult(true));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Error deleting inventory adjustment", ex.Message));
            }
        }

        private InventoryAdjustmentDto MapToDto(InventoryAdjustment adjustment)
        {
            return new InventoryAdjustmentDto
            {
                Id = adjustment.Id,
                AdjustmentNumber = adjustment.AdjustmentNumber,
                InventoryCheckId = adjustment.InventoryCheckId,
                InventoryCheckCode = adjustment.InventoryCheck?.Code,
                InventoryCheckName = adjustment.InventoryCheck?.Name,
                WarehouseId = adjustment.WarehouseId,
                WarehouseName = adjustment.Warehouse?.Name,
                WarehouseZoneId = adjustment.WarehouseZoneId,
                WarehouseZoneName = adjustment.WarehouseZone?.Name,
                WarehouseBinId = adjustment.WarehouseBinId,
                WarehouseBinName = adjustment.WarehouseBin?.Name,
                AdjustmentDate = adjustment.AdjustmentDate,
                Status = adjustment.Status,
                Reason = adjustment.Reason,
                ApprovedByEmployeeId = adjustment.ApprovedByEmployeeId,
                ApprovedByEmployeeName = adjustment.ApprovedByEmployee?.Name,
                ApprovedAt = adjustment.ApprovedAt,
                RejectionReason = adjustment.RejectionReason,
                Notes = adjustment.Notes,
                Items = adjustment.Items.Select(i => new InventoryAdjustmentItemDto
                {
                    Id = i.Id,
                    InventoryAdjustmentId = i.InventoryAdjustmentId,
                    PartId = i.PartId,
                    PartNumber = i.Part?.PartNumber,
                    PartName = i.Part?.PartName,
                    PartSku = i.Part?.Sku,
                    InventoryCheckItemId = i.InventoryCheckItemId,
                    QuantityChange = i.QuantityChange,
                    SystemQuantityBefore = i.SystemQuantityBefore,
                    SystemQuantityAfter = i.SystemQuantityAfter,
                    Notes = i.Notes
                }).ToList(),
                CreatedAt = adjustment.CreatedAt,
                UpdatedAt = adjustment.UpdatedAt
            };
        }

        /// <summary>
        /// ✅ Phase 4.1 - Advanced Features: Helper method để log audit
        /// </summary>
        private async Task LogAuditAsync(string entityName, int? entityId, string action, string? details = null, string severity = "Info")
        {
            try
            {
                var currentUserId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var currentUserName = User.FindFirst("name")?.Value ?? User.Identity?.Name ?? "System";

                // ✅ FIX: Truncate Details nếu quá dài (tránh database error)
                // Details field không có max length constraint, nhưng nên giới hạn để tránh vấn đề
                var truncatedDetails = details;
                if (!string.IsNullOrEmpty(details) && details.Length > 4000)
                {
                    truncatedDetails = details.Substring(0, 4000) + "... (truncated)";
                }

                // ✅ FIX: Truncate UserAgent nếu quá dài (max 500)
                var userAgent = Request.Headers["User-Agent"].ToString();
                if (userAgent.Length > 500)
                {
                    userAgent = userAgent.Substring(0, 500);
                }

                var auditLog = new AuditLog
                {
                    EntityName = entityName,
                    EntityId = entityId,
                    Action = action,
                    UserId = currentUserId,
                    UserName = currentUserName,
                    Timestamp = DateTime.Now,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = userAgent,
                    Details = truncatedDetails,
                    Severity = severity,
                    CreatedAt = DateTime.Now
                };

                await _context.AuditLogs.AddAsync(auditLog);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // ✅ Không throw exception nếu audit log fail (không ảnh hưởng business logic)
            }
        }

        /// <summary>
        /// ✅ Performance Optimization: Create audit log object without saving (for batch insert)
        /// </summary>
        private AuditLog CreateAuditLog(string entityName, int? entityId, string action, string? details = null, string severity = "Info")
        {
            var currentUserId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var currentUserName = User.FindFirst("name")?.Value ?? User.Identity?.Name ?? "System";

            // Truncate Details nếu quá dài
            var truncatedDetails = details;
            if (!string.IsNullOrEmpty(details) && details.Length > 4000)
            {
                truncatedDetails = details.Substring(0, 4000) + "... (truncated)";
            }

            // Truncate UserAgent nếu quá dài
            var userAgent = Request.Headers["User-Agent"].ToString();
            if (userAgent.Length > 500)
            {
                userAgent = userAgent.Substring(0, 500);
            }

            return new AuditLog
            {
                EntityName = entityName,
                EntityId = entityId,
                Action = action,
                UserId = currentUserId,
                UserName = currentUserName,
                Timestamp = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = userAgent,
                Details = truncatedDetails,
                Severity = severity,
                CreatedAt = DateTime.Now
            };
        }

        private async Task<string> GenerateAdjustmentNumberAsync()
        {
            var year = DateTime.Now.Year;
            var prefix = $"ADJ-{year}";

            var lastAdjustment = await _context.InventoryAdjustments
                .Where(ia => ia.AdjustmentNumber.StartsWith(prefix))
                .OrderByDescending(ia => ia.AdjustmentNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastAdjustment != null)
            {
                var parts = lastAdjustment.AdjustmentNumber.Split('-');
                if (parts.Length >= 3 && int.TryParse(parts[2], out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}-{nextNumber:D3}";
        }

        private async Task<string> GenerateTransactionNumberAsync()
        {
            var year = DateTime.Now.Year;
            var prefix = $"STK-{year}";

            var lastTransaction = await _context.StockTransactions
                .Where(st => st.TransactionNumber.StartsWith(prefix))
                .OrderByDescending(st => st.TransactionNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastTransaction != null)
            {
                var parts = lastTransaction.TransactionNumber.Split('-');
                if (parts.Length >= 3 && int.TryParse(parts[2], out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}-{nextNumber:D3}";
        }

        /// <summary>
        /// ✅ Performance Optimization: Generate multiple transaction numbers at once
        /// </summary>
        private async Task<List<string>> GenerateTransactionNumbersAsync(int count)
        {
            var year = DateTime.Now.Year;
            var prefix = $"STK-{year}";

            var lastTransaction = await _context.StockTransactions
                .Where(st => st.TransactionNumber.StartsWith(prefix))
                .OrderByDescending(st => st.TransactionNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastTransaction != null)
            {
                var parts = lastTransaction.TransactionNumber.Split('-');
                if (parts.Length >= 3 && int.TryParse(parts[2], out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            var numbers = new List<string>();
            for (int i = 0; i < count; i++)
            {
                numbers.Add($"{prefix}-{(nextNumber + i):D3}");
            }

            return numbers;
        }

        /// <summary>
        /// ✅ Phase 4.1 - Advanced Features: Lấy lịch sử audit cho Inventory Adjustment
        /// </summary>
        [HttpGet("{id}/history")]
        public async Task<ActionResult<ApiResponse<List<AuditLogDto>>>> GetInventoryAdjustmentHistory(
            int id,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? action = null)
        {
            try
            {
                // Verify adjustment exists
                var adjustment = await _context.InventoryAdjustments
                    .Where(ia => !ia.IsDeleted && ia.Id == id)
                    .FirstOrDefaultAsync();
                
                if (adjustment == null)
                {
                    return NotFound(ApiResponse<List<AuditLogDto>>.ErrorResult("Inventory adjustment not found"));
                }

                // ✅ FIX: Build query với filters ở database level để optimize performance
                var query = _context.AuditLogs
                    .AsNoTracking()
                    .Where(al => al.EntityName == "InventoryAdjustment" && al.EntityId == id);

                // Also include related InventoryAdjustmentItem audit logs (if needed in future)
                var itemIds = await _context.InventoryAdjustmentItems
                    .Where(i => !i.IsDeleted && i.InventoryAdjustmentId == id)
                    .Select(i => i.Id)
                    .ToListAsync();

                IQueryable<AuditLog> itemQuery = _context.AuditLogs.AsNoTracking();
                
                // ✅ FIX: Chỉ query item logs nếu có items
                if (itemIds.Any())
                {
                    itemQuery = _context.AuditLogs
                        .AsNoTracking()
                        .Where(al => al.EntityName == "InventoryAdjustmentItem" && 
                            al.EntityId.HasValue && 
                            itemIds.Contains(al.EntityId.Value));
                }
                else
                {
                    // ✅ FIX: Nếu không có items, tạo empty query
                    itemQuery = _context.AuditLogs
                        .AsNoTracking()
                        .Where(al => false); // Empty query
                }

                // ✅ FIX: Apply filters ở database level trước khi Union
                if (startDate.HasValue)
                {
                    query = query.Where(al => al.Timestamp >= startDate.Value);
                    itemQuery = itemQuery.Where(al => al.Timestamp >= startDate.Value);
                }
                if (endDate.HasValue)
                {
                    query = query.Where(al => al.Timestamp <= endDate.Value);
                    itemQuery = itemQuery.Where(al => al.Timestamp <= endDate.Value);
                }
                if (!string.IsNullOrEmpty(action))
                {
                    query = query.Where(al => al.Action == action);
                    itemQuery = itemQuery.Where(al => al.Action == action);
                }

                // Combine queries và order
                var allLogs = await query
                    .Union(itemQuery)
                    .OrderByDescending(al => al.Timestamp)
                    .ToListAsync();

                var dtos = allLogs.Select(al => new AuditLogDto
                {
                    Id = al.Id,
                    EntityName = al.EntityName,
                    EntityId = al.EntityId,
                    Action = al.Action,
                    UserId = al.UserId,
                    UserName = al.UserName,
                    Timestamp = al.Timestamp,
                    IpAddress = al.IpAddress,
                    UserAgent = al.UserAgent,
                    Details = al.Details,
                    Severity = al.Severity,
                    CreatedAt = al.CreatedAt,
                    UpdatedAt = al.UpdatedAt
                }).ToList();

                return Ok(ApiResponse<List<AuditLogDto>>.SuccessResult(dtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AuditLogDto>>.ErrorResult("Error getting audit history", ex.Message));
            }
        }

        /// <summary>
        /// ✅ Phase 4.1 - Advanced Features: Thêm comment vào Inventory Adjustment
        /// </summary>
        [HttpPost("{id}/comments")]
        public async Task<ActionResult<ApiResponse<InventoryAdjustmentCommentDto>>> AddComment(int id, [FromBody] CreateInventoryAdjustmentCommentDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<InventoryAdjustmentCommentDto>.ErrorResult("Invalid data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                }

                if (id != dto.InventoryAdjustmentId)
                {
                    return BadRequest(ApiResponse<InventoryAdjustmentCommentDto>.ErrorResult("ID mismatch"));
                }

                // Verify adjustment exists
                var adjustment = await _context.InventoryAdjustments
                    .Where(ia => !ia.IsDeleted && ia.Id == id)
                    .FirstOrDefaultAsync();
                
                if (adjustment == null)
                {
                    return NotFound(ApiResponse<InventoryAdjustmentCommentDto>.ErrorResult("Inventory adjustment not found"));
                }

                // ✅ FIX: Validate CommentText không được empty sau khi trim
                var commentText = dto.CommentText?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(commentText))
                {
                    return BadRequest(ApiResponse<InventoryAdjustmentCommentDto>.ErrorResult("Comment không được để trống"));
                }

                // Get current user
                var currentUserId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var currentUserName = User.FindFirst("name")?.Value ?? User.Identity?.Name ?? "System";

                var comment = new InventoryAdjustmentComment
                {
                    InventoryAdjustmentId = id,
                    CommentText = commentText,
                    CreatedByUserName = currentUserName,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };

                if (!string.IsNullOrEmpty(currentUserId) && int.TryParse(currentUserId, out var userId))
                {
                    comment.CreatedByEmployeeId = userId;
                }

                await _unitOfWork.InventoryAdjustmentComments.AddAsync(comment);
                await _unitOfWork.SaveChangesAsync();

                // Reload with employee
                var savedComment = await _context.InventoryAdjustmentComments
                    .AsNoTracking()
                    .Where(c => !c.IsDeleted && c.Id == comment.Id)
                    .Include(c => c.CreatedByEmployee)
                    .FirstOrDefaultAsync();

                if (savedComment == null)
                {
                    return StatusCode(500, ApiResponse<InventoryAdjustmentCommentDto>.ErrorResult("Không thể tải lại thông tin comment sau khi tạo"));
                }

                var commentDto = new InventoryAdjustmentCommentDto
                {
                    Id = savedComment.Id,
                    InventoryAdjustmentId = savedComment.InventoryAdjustmentId,
                    CommentText = savedComment.CommentText,
                    CreatedByEmployeeId = savedComment.CreatedByEmployeeId,
                    CreatedByEmployeeName = savedComment.CreatedByEmployee?.Name,
                    CreatedByUserName = savedComment.CreatedByUserName,
                    CreatedAt = savedComment.CreatedAt
                };

                // ✅ Phase 4.1 - Advanced Features: Log audit
                // ✅ FIX: Check null/empty trước khi Substring
                var auditCommentPreview = !string.IsNullOrEmpty(savedComment.CommentText) 
                    ? (savedComment.CommentText.Length > 100 
                        ? savedComment.CommentText.Substring(0, 100) + "..." 
                        : savedComment.CommentText)
                    : "(empty)";
                await LogAuditAsync("InventoryAdjustmentComment", savedComment.Id, "Create", 
                    $"Thêm comment vào điều chỉnh {adjustment.AdjustmentNumber}: {auditCommentPreview}", 
                    "Info");

                return CreatedAtAction(nameof(GetComments), new { id = id }, ApiResponse<InventoryAdjustmentCommentDto>.SuccessResult(commentDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InventoryAdjustmentCommentDto>.ErrorResult("Error adding comment", ex.Message));
            }
        }

        /// <summary>
        /// ✅ Phase 4.1 - Advanced Features: Lấy danh sách comments của Inventory Adjustment (timeline)
        /// </summary>
        [HttpGet("{id}/comments")]
        public async Task<ActionResult<ApiResponse<List<InventoryAdjustmentCommentDto>>>> GetComments(int id)
        {
            try
            {
                // Verify adjustment exists
                var adjustment = await _context.InventoryAdjustments
                    .Where(ia => !ia.IsDeleted && ia.Id == id)
                    .FirstOrDefaultAsync();
                
                if (adjustment == null)
                {
                    return NotFound(ApiResponse<List<InventoryAdjustmentCommentDto>>.ErrorResult("Inventory adjustment not found"));
                }

                var comments = await _context.InventoryAdjustmentComments
                    .AsNoTracking()
                    .Where(c => !c.IsDeleted && c.InventoryAdjustmentId == id)
                    .Include(c => c.CreatedByEmployee)
                    .OrderByDescending(c => c.CreatedAt) // ✅ FIX: Timeline mới nhất trước (mới → cũ)
                    .ToListAsync();

                var dtos = comments.Select(c => new InventoryAdjustmentCommentDto
                {
                    Id = c.Id,
                    InventoryAdjustmentId = c.InventoryAdjustmentId,
                    CommentText = c.CommentText,
                    CreatedByEmployeeId = c.CreatedByEmployeeId,
                    CreatedByEmployeeName = c.CreatedByEmployee?.Name,
                    CreatedByUserName = c.CreatedByUserName,
                    CreatedAt = c.CreatedAt
                }).ToList();

                return Ok(ApiResponse<List<InventoryAdjustmentCommentDto>>.SuccessResult(dtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<InventoryAdjustmentCommentDto>>.ErrorResult("Error getting comments", ex.Message));
            }
        }

        /// <summary>
        /// ✅ Phase 4.1 - Advanced Features: Bulk approve Inventory Adjustments
        /// </summary>
        [HttpPost("bulk-approve")]
        public async Task<ActionResult<ApiResponse<BulkOperationResultDto>>> BulkApproveInventoryAdjustments(
            [FromBody] BulkApproveInventoryAdjustmentsDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<BulkOperationResultDto>.ErrorResult("Invalid data", 
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                }

                if (dto.AdjustmentIds == null || !dto.AdjustmentIds.Any())
                {
                    return BadRequest(ApiResponse<BulkOperationResultDto>.ErrorResult("Phải chọn ít nhất 1 phiếu điều chỉnh"));
                }

                var result = new BulkOperationResultDto();
                var adjustmentIds = dto.AdjustmentIds.Distinct().ToList();

                // Load all adjustments that need to be approved
                var adjustments = await _context.InventoryAdjustments
                    .Where(ia => !ia.IsDeleted && adjustmentIds.Contains(ia.Id))
                    .Include(ia => ia.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Part)
                    .ToListAsync();

                if (!adjustments.Any())
                {
                    return BadRequest(ApiResponse<BulkOperationResultDto>.ErrorResult("Không tìm thấy phiếu điều chỉnh nào"));
                }

                // ✅ Performance Optimization: Check if some IDs are missing
                var foundIds = adjustments.Select(a => a.Id).ToHashSet();
                var missingIds = adjustmentIds.Where(id => !foundIds.Contains(id)).ToList();
                if (missingIds.Any())
                {
                    // Log warning but continue processing
                    // Could add to result.Errors if needed
                }

                // Get current user
                var currentUserId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                int? userId = null;
                if (!string.IsNullOrEmpty(currentUserId) && int.TryParse(currentUserId, out var parsedUserId))
                {
                    userId = parsedUserId;
                }

                // ✅ Performance Optimization: Pre-calculate total items count for transaction number generation
                var totalItemsCount = adjustments
                    .Where(a => a.Status == "Pending" && a.Items != null && a.Items.Any())
                    .Sum(a => a.Items.Count);

                // ✅ Performance Optimization: Generate all transaction numbers at once
                var transactionNumbers = totalItemsCount > 0 
                    ? await GenerateTransactionNumbersAsync(totalItemsCount)
                    : new List<string>();
                var transactionNumberIndex = 0;

                // ✅ Performance Optimization: Collect audit logs for batch insert
                var auditLogs = new List<AuditLog>();

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    foreach (var adjustment in adjustments)
                    {
                        try
                        {
                            // Get adjustment number for error messages
                            var adjustmentNumber = !string.IsNullOrEmpty(adjustment.AdjustmentNumber) 
                                ? adjustment.AdjustmentNumber 
                                : $"ID {adjustment.Id}";

                            // Validate status
                            if (adjustment.Status != "Pending")
                            {
                                result.FailureCount++;
                                result.FailedIds.Add(adjustment.Id);
                                result.Errors.Add($"Phiếu {adjustmentNumber}: Không thể duyệt với trạng thái {adjustment.Status}");
                                continue;
                            }

                            // Validate: Adjustment must have items
                            if (adjustment.Items == null || !adjustment.Items.Any())
                            {
                                throw new InvalidOperationException("Phiếu điều chỉnh không có items");
                            }

                            // ✅ FIX: Process items first, then update adjustment status
                            // This ensures data consistency: only mark as Approved if all items are processed successfully
                            var processedItems = new List<InventoryAdjustmentItem>();

                            // Update part quantities and create stock transactions
                            foreach (var item in adjustment.Items)
                            {
                                var part = item.Part;
                                if (part == null)
                                {
                                    part = await _unitOfWork.Parts.GetByIdAsync(item.PartId);
                                }

                                if (part == null || part.IsDeleted)
                                {
                                    throw new InvalidOperationException($"Phụ tùng với ID {item.PartId} không tồn tại hoặc đã bị xóa");
                                }

                                // Validate: SystemQuantityAfter không được âm
                                if (item.SystemQuantityAfter < 0)
                                {
                                    var partNumber = part.PartNumber ?? $"ID {part.Id}";
                                    throw new InvalidOperationException($"Số lượng sau điều chỉnh không được âm cho phụ tùng {partNumber}");
                                }

                                // Update part quantity
                                part.QuantityInStock = item.SystemQuantityAfter;
                                await _unitOfWork.Parts.UpdateAsync(part);

                                // ✅ Performance Optimization: Use pre-generated transaction number
                                var transactionNumber = transactionNumberIndex < transactionNumbers.Count
                                    ? transactionNumbers[transactionNumberIndex++]
                                    : await GenerateTransactionNumberAsync(); // Fallback if count mismatch

                                // Create stock transaction
                                var transaction = new StockTransaction
                                {
                                    PartId = item.PartId,
                                    TransactionNumber = transactionNumber,
                                    TransactionType = item.QuantityChange > 0 ? StockTransactionType.NhapKho : StockTransactionType.XuatKho,
                                    Quantity = Math.Abs(item.QuantityChange),
                                    UnitCost = part.CostPrice,
                                    UnitPrice = part.SellPrice,
                                    TotalCost = Math.Abs(item.QuantityChange) * part.CostPrice,
                                    TotalAmount = Math.Abs(item.QuantityChange) * part.SellPrice,
                                    TransactionDate = DateTime.Now,
                                    ReferenceNumber = adjustment.AdjustmentNumber,
                                    RelatedEntity = "InventoryAdjustment",
                                    RelatedEntityId = adjustment.Id,
                                    Notes = $"Điều chỉnh tồn kho: {(item.QuantityChange > 0 ? "Tăng" : "Giảm")} {Math.Abs(item.QuantityChange)}. {item.Notes ?? ""}"
                                };

                                await _unitOfWork.StockTransactions.AddAsync(transaction);
                                processedItems.Add(item);
                            }

                            // ✅ FIX: Only update adjustment status after all items are processed successfully
                            adjustment.Status = "Approved";
                            adjustment.ApprovedAt = DateTime.Now;
                            if (userId.HasValue)
                            {
                                adjustment.ApprovedByEmployeeId = userId.Value;
                            }
                            if (!string.IsNullOrEmpty(dto.Notes))
                            {
                                adjustment.Notes = dto.Notes;
                            }

                            await _unitOfWork.InventoryAdjustments.UpdateAsync(adjustment);

                            result.SuccessCount++;
                            result.SuccessIds.Add(adjustment.Id);

                            // ✅ Performance Optimization: Collect audit log for batch insert
                            var itemsCount = processedItems.Count;
                            var totalQtyChange = processedItems.Sum(i => Math.Abs(i.QuantityChange));
                            var notesText = dto.Notes ?? "Không có";
                            var auditLog = CreateAuditLog("InventoryAdjustment", adjustment.Id, "BulkApprove", 
                                $"Duyệt điều chỉnh (Bulk): {adjustmentNumber}, Items: {itemsCount}, Tổng thay đổi: {totalQtyChange}, Notes: {notesText}", 
                                "Info");
                            auditLogs.Add(auditLog);
                        }
                        catch (Exception ex)
                        {
                            result.FailureCount++;
                            result.FailedIds.Add(adjustment.Id);
                            var adjustmentNumber = !string.IsNullOrEmpty(adjustment.AdjustmentNumber) 
                                ? adjustment.AdjustmentNumber 
                                : $"ID {adjustment.Id}";
                            result.Errors.Add($"Phiếu {adjustmentNumber}: {ex.Message}");
                        }
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    // ✅ Performance Optimization: Batch insert audit logs after commit
                    if (auditLogs.Any())
                    {
                        try
                        {
                            await _context.AuditLogs.AddRangeAsync(auditLogs);
                            await _context.SaveChangesAsync();
                        }
                        catch
                        {
                            // ✅ Không throw exception nếu audit log fail (không ảnh hưởng business logic)
                        }
                    }

                    // ✅ Real-time: Push notification for inventory alert count update
                    if (_notificationService != null && result.SuccessCount > 0)
                    {
                        try
                        {
                            // ✅ Performance: Use CountAsync instead of loading all alerts
                            var totalUnresolvedCount = await _unitOfWork.Repository<Core.Entities.InventoryAlert>()
                                .CountAsync(a => !a.IsResolved);
                            await _notificationService.NotifyInventoryAlertUpdatedAsync(totalUnresolvedCount);
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "Error sending inventory alert notification after bulk approve");
                        }
                    }

                    return Ok(ApiResponse<BulkOperationResultDto>.SuccessResult(result, 
                        $"Đã xử lý {result.SuccessCount} phiếu thành công, {result.FailureCount} phiếu thất bại"));
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return StatusCode(500, ApiResponse<BulkOperationResultDto>.ErrorResult("Error in bulk approve operation", ex.Message));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<BulkOperationResultDto>.ErrorResult("Error bulk approving inventory adjustments", ex.Message));
            }
        }

        /// <summary>
        /// ✅ Phase 4.1 - Advanced Features: Bulk reject Inventory Adjustments
        /// </summary>
        [HttpPost("bulk-reject")]
        public async Task<ActionResult<ApiResponse<BulkOperationResultDto>>> BulkRejectInventoryAdjustments(
            [FromBody] BulkRejectInventoryAdjustmentsDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<BulkOperationResultDto>.ErrorResult("Invalid data", 
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                }

                if (dto.AdjustmentIds == null || !dto.AdjustmentIds.Any())
                {
                    return BadRequest(ApiResponse<BulkOperationResultDto>.ErrorResult("Phải chọn ít nhất 1 phiếu điều chỉnh"));
                }

                if (string.IsNullOrWhiteSpace(dto.RejectionReason))
                {
                    return BadRequest(ApiResponse<BulkOperationResultDto>.ErrorResult("Vui lòng nhập lý do từ chối"));
                }

                var result = new BulkOperationResultDto();
                var adjustmentIds = dto.AdjustmentIds.Distinct().ToList();

                // Load all adjustments that need to be rejected
                var adjustments = await _context.InventoryAdjustments
                    .Where(ia => !ia.IsDeleted && adjustmentIds.Contains(ia.Id))
                    .ToListAsync();

                if (!adjustments.Any())
                {
                    return BadRequest(ApiResponse<BulkOperationResultDto>.ErrorResult("Không tìm thấy phiếu điều chỉnh nào"));
                }

                // ✅ Performance Optimization: Check if some IDs are missing
                var foundIds = adjustments.Select(a => a.Id).ToHashSet();
                var missingIds = adjustmentIds.Where(id => !foundIds.Contains(id)).ToList();
                if (missingIds.Any())
                {
                    // Log warning but continue processing
                    // Could add to result.Errors if needed
                }

                // ✅ Performance Optimization: Collect audit logs for batch insert
                var auditLogs = new List<AuditLog>();

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    foreach (var adjustment in adjustments)
                    {
                        try
                        {
                            // Get adjustment number for error messages
                            var adjustmentNumber = !string.IsNullOrEmpty(adjustment.AdjustmentNumber) 
                                ? adjustment.AdjustmentNumber 
                                : $"ID {adjustment.Id}";

                            // Validate status
                            if (adjustment.Status != "Pending")
                            {
                                result.FailureCount++;
                                result.FailedIds.Add(adjustment.Id);
                                result.Errors.Add($"Phiếu {adjustmentNumber}: Không thể từ chối với trạng thái {adjustment.Status}");
                                continue;
                            }

                            // Update adjustment
                            adjustment.Status = "Rejected";
                            adjustment.RejectionReason = dto.RejectionReason;
                            adjustment.UpdatedAt = DateTime.Now;

                            await _unitOfWork.InventoryAdjustments.UpdateAsync(adjustment);

                            result.SuccessCount++;
                            result.SuccessIds.Add(adjustment.Id);

                            // ✅ Performance Optimization: Collect audit log for batch insert
                            var auditLog = CreateAuditLog("InventoryAdjustment", adjustment.Id, "BulkReject", 
                                $"Từ chối điều chỉnh (Bulk): {adjustmentNumber}, Lý do: {dto.RejectionReason}", 
                                "Warning");
                            auditLogs.Add(auditLog);
                        }
                        catch (Exception ex)
                        {
                            result.FailureCount++;
                            result.FailedIds.Add(adjustment.Id);
                            var adjustmentNumber = !string.IsNullOrEmpty(adjustment.AdjustmentNumber) 
                                ? adjustment.AdjustmentNumber 
                                : $"ID {adjustment.Id}";
                            result.Errors.Add($"Phiếu {adjustmentNumber}: {ex.Message}");
                        }
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    // ✅ Performance Optimization: Batch insert audit logs after commit
                    if (auditLogs.Any())
                    {
                        try
                        {
                            await _context.AuditLogs.AddRangeAsync(auditLogs);
                            await _context.SaveChangesAsync();
                        }
                        catch
                        {
                            // ✅ Không throw exception nếu audit log fail (không ảnh hưởng business logic)
                        }
                    }

                    return Ok(ApiResponse<BulkOperationResultDto>.SuccessResult(result, 
                        $"Đã xử lý {result.SuccessCount} phiếu thành công, {result.FailureCount} phiếu thất bại"));
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return StatusCode(500, ApiResponse<BulkOperationResultDto>.ErrorResult("Error in bulk reject operation", ex.Message));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<BulkOperationResultDto>.ErrorResult("Error bulk rejecting inventory adjustments", ex.Message));
            }
        }

        /// <summary>
        /// ✅ Phase 4.1 - Advanced Features: Xóa comment (soft delete)
        /// </summary>
        [HttpDelete("{id}/comments/{commentId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteComment(int id, int commentId)
        {
            try
            {
                var comment = await _context.InventoryAdjustmentComments
                    .Where(c => !c.IsDeleted && c.Id == commentId && c.InventoryAdjustmentId == id)
                    .FirstOrDefaultAsync();

                if (comment == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResult("Comment not found"));
                }

                await _unitOfWork.InventoryAdjustmentComments.DeleteAsync(comment);
                await _unitOfWork.SaveChangesAsync();

                // ✅ Phase 4.1 - Advanced Features: Log audit
                await LogAuditAsync("InventoryAdjustmentComment", commentId, "Delete", 
                    $"Xóa comment khỏi điều chỉnh ID {id}", 
                    "Warning");

                return Ok(ApiResponse<bool>.SuccessResult(true));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Error deleting comment", ex.Message));
            }
        }
    }
}

