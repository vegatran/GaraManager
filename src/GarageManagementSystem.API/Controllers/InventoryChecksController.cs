using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace GarageManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiScope")]
    public class InventoryChecksController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly GarageDbContext _context;

        public InventoryChecksController(IUnitOfWork unitOfWork, IMapper mapper, GarageDbContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách phiếu kiểm kê
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<InventoryCheckDto>>>> GetInventoryChecks(
            [FromQuery] int? warehouseId = null,
            [FromQuery] int? warehouseZoneId = null,
            [FromQuery] int? warehouseBinId = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = _context.InventoryChecks
                    .AsNoTracking()
                    .Where(ic => !ic.IsDeleted)
                    .Include(ic => ic.Warehouse)
                    .Include(ic => ic.WarehouseZone)
                    .Include(ic => ic.WarehouseBin)
                    .Include(ic => ic.StartedByEmployee)
                    .Include(ic => ic.CompletedByEmployee)
                    .Include(ic => ic.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Part)
                    .AsQueryable();

                // Apply filters
                if (warehouseId.HasValue)
                {
                    query = query.Where(ic => ic.WarehouseId == warehouseId.Value);
                }

                if (warehouseZoneId.HasValue)
                {
                    query = query.Where(ic => ic.WarehouseZoneId == warehouseZoneId.Value);
                }

                if (warehouseBinId.HasValue)
                {
                    query = query.Where(ic => ic.WarehouseBinId == warehouseBinId.Value);
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(ic => ic.Status == status);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(ic => ic.CheckDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(ic => ic.CheckDate <= endDate.Value);
                }

                var inventoryChecks = await query
                    .OrderByDescending(ic => ic.CheckDate)
                    .ThenByDescending(ic => ic.CreatedAt)
                    .ToListAsync();

                var dto = _mapper.Map<List<InventoryCheckDto>>(inventoryChecks);
                return Ok(ApiResponse<List<InventoryCheckDto>>.SuccessResult(dto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<InventoryCheckDto>>.ErrorResult("Error retrieving inventory checks", ex.Message));
            }
        }

        /// <summary>
        /// Lấy chi tiết phiếu kiểm kê
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<InventoryCheckDto>>> GetInventoryCheck(int id)
        {
            try
            {
                var inventoryCheck = await _context.InventoryChecks
                    .AsNoTracking()
                    .Where(ic => !ic.IsDeleted && ic.Id == id)
                    .Include(ic => ic.Warehouse)
                    .Include(ic => ic.WarehouseZone)
                    .Include(ic => ic.WarehouseBin)
                    .Include(ic => ic.StartedByEmployee)
                    .Include(ic => ic.CompletedByEmployee)
                    .Include(ic => ic.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Part)
                    .FirstOrDefaultAsync();

                if (inventoryCheck == null)
                {
                    return NotFound(ApiResponse<InventoryCheckDto>.ErrorResult("Inventory check not found"));
                }

                var dto = _mapper.Map<InventoryCheckDto>(inventoryCheck);
                return Ok(ApiResponse<InventoryCheckDto>.SuccessResult(dto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InventoryCheckDto>.ErrorResult("Error retrieving inventory check", ex.Message));
            }
        }

        /// <summary>
        /// Tạo phiếu kiểm kê mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<InventoryCheckDto>>> CreateInventoryCheck(CreateInventoryCheckDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<InventoryCheckDto>.ErrorResult("Invalid data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                }

                // ✅ Normalize Code TRƯỚC khi validate
                var normalizedCode = !string.IsNullOrWhiteSpace(dto.Code) ? dto.Code.Trim() : string.Empty;

                // ✅ Nếu Code trống, tự động generate
                // ✅ NOTE: Generate code trước transaction. Nếu có race condition, unique constraint sẽ catch.
                if (string.IsNullOrEmpty(normalizedCode))
                {
                    normalizedCode = await GenerateInventoryCheckCodeAsync();
                }
                else
                {
                    // ✅ Validate Code unique
                    var existingCheck = await _context.InventoryChecks
                        .Where(ic => !ic.IsDeleted && ic.Code == normalizedCode)
                        .FirstOrDefaultAsync();
                    if (existingCheck != null)
                    {
                        return BadRequest(ApiResponse<InventoryCheckDto>.ErrorResult($"Mã phiếu kiểm kê '{normalizedCode}' đã tồn tại trong hệ thống"));
                    }
                }

                // Validate warehouse, zone, bin if provided
                if (dto.WarehouseId.HasValue)
                {
                    var warehouse = await _context.Warehouses
                        .Where(w => !w.IsDeleted && w.Id == dto.WarehouseId.Value)
                        .FirstOrDefaultAsync();
                    if (warehouse == null)
                    {
                        return BadRequest(ApiResponse<InventoryCheckDto>.ErrorResult("Kho không tồn tại"));
                    }
                }

                if (dto.WarehouseZoneId.HasValue)
                {
                    var zone = await _context.WarehouseZones
                        .Where(z => !z.IsDeleted && z.Id == dto.WarehouseZoneId.Value)
                        .FirstOrDefaultAsync();
                    if (zone == null)
                    {
                        return BadRequest(ApiResponse<InventoryCheckDto>.ErrorResult("Khu vực không tồn tại"));
                    }
                    if (dto.WarehouseId.HasValue && zone.WarehouseId != dto.WarehouseId.Value)
                    {
                        return BadRequest(ApiResponse<InventoryCheckDto>.ErrorResult("Khu vực không thuộc kho đã chọn"));
                    }
                }

                if (dto.WarehouseBinId.HasValue)
                {
                    var bin = await _context.WarehouseBins
                        .Where(b => !b.IsDeleted && b.Id == dto.WarehouseBinId.Value)
                        .FirstOrDefaultAsync();
                    if (bin == null)
                    {
                        return BadRequest(ApiResponse<InventoryCheckDto>.ErrorResult("Kệ/ngăn không tồn tại"));
                    }
                    if (dto.WarehouseId.HasValue && bin.WarehouseId != dto.WarehouseId.Value)
                    {
                        return BadRequest(ApiResponse<InventoryCheckDto>.ErrorResult("Kệ/ngăn không thuộc kho đã chọn"));
                    }
                    if (dto.WarehouseZoneId.HasValue && bin.WarehouseZoneId.HasValue && bin.WarehouseZoneId != dto.WarehouseZoneId.Value)
                    {
                        return BadRequest(ApiResponse<InventoryCheckDto>.ErrorResult("Kệ/ngăn không thuộc khu vực đã chọn"));
                    }
                }

                // Map DTO to Entity
                var inventoryCheck = _mapper.Map<InventoryCheck>(dto);
                inventoryCheck.Code = normalizedCode;
                inventoryCheck.Status = dto.Status ?? "Draft";
                inventoryCheck.CreatedAt = DateTime.Now;
                inventoryCheck.IsDeleted = false;

                // Bắt đầu transaction
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Add inventory check
                    await _unitOfWork.InventoryChecks.AddAsync(inventoryCheck);
                    await _unitOfWork.SaveChangesAsync();

                    // Add items if provided
                    if (dto.Items != null && dto.Items.Any())
                    {
                        // ✅ FIX: Check duplicate parts trong dto.Items trước khi tạo
                        var duplicateParts = dto.Items
                            .GroupBy(i => i.PartId)
                            .Where(g => g.Count() > 1)
                            .Select(g => g.Key)
                            .ToList();
                        if (duplicateParts.Any())
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return BadRequest(ApiResponse<InventoryCheckDto>.ErrorResult($"Danh sách items có phụ tùng trùng lặp (PartId: {string.Join(", ", duplicateParts)})"));
                        }

                        foreach (var itemDto in dto.Items)
                        {
                            // Validate part exists
                            var part = await _context.Parts
                                .Where(p => !p.IsDeleted && p.Id == itemDto.PartId)
                                .FirstOrDefaultAsync();
                            if (part == null)
                            {
                                await _unitOfWork.RollbackTransactionAsync();
                                return BadRequest(ApiResponse<InventoryCheckDto>.ErrorResult($"Phụ tùng với ID {itemDto.PartId} không tồn tại"));
                            }

                            // ✅ FIX: Check duplicate part trong database (nếu có items đã tồn tại từ lần tạo trước)
                            var existingItem = await _context.InventoryCheckItems
                                .Where(i => !i.IsDeleted && i.InventoryCheckId == inventoryCheck.Id && i.PartId == itemDto.PartId)
                                .FirstOrDefaultAsync();
                            if (existingItem != null)
                            {
                                await _unitOfWork.RollbackTransactionAsync();
                                return BadRequest(ApiResponse<InventoryCheckDto>.ErrorResult($"Phụ tùng với ID {itemDto.PartId} đã tồn tại trong phiếu kiểm kê"));
                            }

                            // Load SystemQuantity from Part.QuantityInStock
                            var systemQuantity = part.QuantityInStock;

                            // Create item
                            var item = _mapper.Map<InventoryCheckItem>(itemDto);
                            item.InventoryCheckId = inventoryCheck.Id;
                            item.SystemQuantity = systemQuantity;
                            item.DiscrepancyQuantity = itemDto.ActualQuantity - systemQuantity;
                            item.IsDiscrepancy = itemDto.ActualQuantity != systemQuantity;
                            item.IsAdjusted = false;
                            item.CreatedAt = DateTime.Now;
                            item.IsDeleted = false;

                            await _unitOfWork.InventoryCheckItems.AddAsync(item);
                        }

                        await _unitOfWork.SaveChangesAsync();
                    }

                    // Commit transaction
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    
                    // Handle database unique constraint violation
                    if (dbEx.InnerException != null && dbEx.InnerException.Message.Contains("Duplicate entry"))
                    {
                        if (dbEx.InnerException.Message.Contains("Code"))
                        {
                            return BadRequest(ApiResponse<InventoryCheckDto>.ErrorResult($"Mã phiếu kiểm kê '{normalizedCode}' đã tồn tại trong hệ thống"));
                        }
                    }
                    return StatusCode(500, ApiResponse<InventoryCheckDto>.ErrorResult("Error creating inventory check", dbEx.Message));
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }

                // Reload với details
                var savedCheck = await _context.InventoryChecks
                    .AsNoTracking()
                    .Where(ic => !ic.IsDeleted && ic.Id == inventoryCheck.Id)
                    .Include(ic => ic.Warehouse)
                    .Include(ic => ic.WarehouseZone)
                    .Include(ic => ic.WarehouseBin)
                    .Include(ic => ic.StartedByEmployee)
                    .Include(ic => ic.CompletedByEmployee)
                    .Include(ic => ic.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Part)
                    .FirstOrDefaultAsync();

                if (savedCheck == null)
                {
                    return StatusCode(500, ApiResponse<InventoryCheckDto>.ErrorResult("Không thể tải lại thông tin phiếu kiểm kê sau khi tạo"));
                }

                // ✅ Phase 4.1 - Advanced Features: Log audit
                await LogAuditAsync("InventoryCheck", savedCheck.Id, "Create", 
                    $"Tạo phiếu kiểm kê: {savedCheck.Code}, Status: {savedCheck.Status}, Items: {savedCheck.Items.Count}");

                return CreatedAtAction(nameof(GetInventoryCheck), new { id = savedCheck.Id }, ApiResponse<InventoryCheckDto>.SuccessResult(_mapper.Map<InventoryCheckDto>(savedCheck)));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InventoryCheckDto>.ErrorResult("Error creating inventory check", ex.Message));
            }
        }

        /// <summary>
        /// Cập nhật phiếu kiểm kê
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<InventoryCheckDto>>> UpdateInventoryCheck(int id, UpdateInventoryCheckDto dto)
        {
            try
            {
                if (id != dto.Id)
                {
                    return BadRequest(ApiResponse<InventoryCheckDto>.ErrorResult("ID mismatch"));
                }

                var inventoryCheck = await _context.InventoryChecks
                    .Where(ic => !ic.IsDeleted && ic.Id == id)
                    .FirstOrDefaultAsync();
                if (inventoryCheck == null)
                {
                    return NotFound(ApiResponse<InventoryCheckDto>.ErrorResult("Inventory check not found"));
                }

                // ✅ Normalize Code TRƯỚC khi validate
                var normalizedCode = !string.IsNullOrWhiteSpace(dto.Code) ? dto.Code.Trim() : string.Empty;

                // ✅ Validate Code không được để trống
                if (string.IsNullOrEmpty(normalizedCode))
                {
                    return BadRequest(ApiResponse<InventoryCheckDto>.ErrorResult("Mã phiếu kiểm kê không được để trống"));
                }

                // ✅ Validate Code unique (chỉ check nếu Code thay đổi)
                if (inventoryCheck.Code != normalizedCode)
                {
                    var existingCheck = await _context.InventoryChecks
                        .Where(ic => !ic.IsDeleted && ic.Code == normalizedCode && ic.Id != id)
                        .FirstOrDefaultAsync();
                    if (existingCheck != null)
                    {
                        return BadRequest(ApiResponse<InventoryCheckDto>.ErrorResult($"Mã phiếu kiểm kê '{normalizedCode}' đã tồn tại trong hệ thống"));
                    }
                }

                // Validate warehouse, zone, bin if provided
                if (dto.WarehouseId.HasValue)
                {
                    var warehouse = await _context.Warehouses
                        .Where(w => !w.IsDeleted && w.Id == dto.WarehouseId.Value)
                        .FirstOrDefaultAsync();
                    if (warehouse == null)
                    {
                        return BadRequest(ApiResponse<InventoryCheckDto>.ErrorResult("Kho không tồn tại"));
                    }
                }

                if (dto.WarehouseZoneId.HasValue)
                {
                    var zone = await _context.WarehouseZones
                        .Where(z => !z.IsDeleted && z.Id == dto.WarehouseZoneId.Value)
                        .FirstOrDefaultAsync();
                    if (zone == null)
                    {
                        return BadRequest(ApiResponse<InventoryCheckDto>.ErrorResult("Khu vực không tồn tại"));
                    }
                    if (dto.WarehouseId.HasValue && zone.WarehouseId != dto.WarehouseId.Value)
                    {
                        return BadRequest(ApiResponse<InventoryCheckDto>.ErrorResult("Khu vực không thuộc kho đã chọn"));
                    }
                }

                if (dto.WarehouseBinId.HasValue)
                {
                    var bin = await _context.WarehouseBins
                        .Where(b => !b.IsDeleted && b.Id == dto.WarehouseBinId.Value)
                        .FirstOrDefaultAsync();
                    if (bin == null)
                    {
                        return BadRequest(ApiResponse<InventoryCheckDto>.ErrorResult("Kệ/ngăn không tồn tại"));
                    }
                    if (dto.WarehouseId.HasValue && bin.WarehouseId != dto.WarehouseId.Value)
                    {
                        return BadRequest(ApiResponse<InventoryCheckDto>.ErrorResult("Kệ/ngăn không thuộc kho đã chọn"));
                    }
                    if (dto.WarehouseZoneId.HasValue && bin.WarehouseZoneId.HasValue && bin.WarehouseZoneId != dto.WarehouseZoneId.Value)
                    {
                        return BadRequest(ApiResponse<InventoryCheckDto>.ErrorResult("Kệ/ngăn không thuộc khu vực đã chọn"));
                    }
                }

                // ✅ Track old values for audit
                var oldStatus = inventoryCheck.Status;
                var oldCode = inventoryCheck.Code;

                // Map DTO to Entity
                _mapper.Map(dto, inventoryCheck);
                inventoryCheck.Code = normalizedCode;
                inventoryCheck.UpdatedAt = DateTime.Now;

                // ✅ Auto-set StartedDate and StartedByEmployeeId when status changes to InProgress
                if (dto.Status == "InProgress" && inventoryCheck.Status != "InProgress")
                {
                    if (!inventoryCheck.StartedDate.HasValue)
                    {
                        inventoryCheck.StartedDate = DateTime.Now;
                    }
                    if (!inventoryCheck.StartedByEmployeeId.HasValue)
                    {
                        var currentUserId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                        if (!string.IsNullOrEmpty(currentUserId) && int.TryParse(currentUserId, out var userId))
                        {
                            inventoryCheck.StartedByEmployeeId = userId;
                        }
                    }
                }

                // Bắt đầu transaction
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.InventoryChecks.UpdateAsync(inventoryCheck);
                    await _unitOfWork.SaveChangesAsync();

                    // Commit transaction
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    
                    // Handle database unique constraint violation
                    if (dbEx.InnerException != null && dbEx.InnerException.Message.Contains("Duplicate entry"))
                    {
                        if (dbEx.InnerException.Message.Contains("Code"))
                        {
                            return BadRequest(ApiResponse<InventoryCheckDto>.ErrorResult($"Mã phiếu kiểm kê '{normalizedCode}' đã tồn tại trong hệ thống"));
                        }
                    }
                    return StatusCode(500, ApiResponse<InventoryCheckDto>.ErrorResult("Error updating inventory check", dbEx.Message));
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }

                // Reload với details
                var savedCheck = await _context.InventoryChecks
                    .AsNoTracking()
                    .Where(ic => !ic.IsDeleted && ic.Id == inventoryCheck.Id)
                    .Include(ic => ic.Warehouse)
                    .Include(ic => ic.WarehouseZone)
                    .Include(ic => ic.WarehouseBin)
                    .Include(ic => ic.StartedByEmployee)
                    .Include(ic => ic.CompletedByEmployee)
                    .Include(ic => ic.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Part)
                    .FirstOrDefaultAsync();

                if (savedCheck == null)
                {
                    return StatusCode(500, ApiResponse<InventoryCheckDto>.ErrorResult("Không thể tải lại thông tin phiếu kiểm kê sau khi cập nhật"));
                }

                // ✅ Phase 4.1 - Advanced Features: Log audit với thông tin thay đổi
                var changes = new List<string>();
                if (oldStatus != savedCheck.Status)
                {
                    changes.Add($"Status: {oldStatus} → {savedCheck.Status}");
                }
                if (oldCode != savedCheck.Code)
                {
                    changes.Add($"Code: {oldCode} → {savedCheck.Code}");
                }
                var details = changes.Any() 
                    ? $"Cập nhật phiếu kiểm kê: {savedCheck.Code}. Thay đổi: {string.Join(", ", changes)}"
                    : $"Cập nhật phiếu kiểm kê: {savedCheck.Code}";
                await LogAuditAsync("InventoryCheck", savedCheck.Id, "Update", details);

                return Ok(ApiResponse<InventoryCheckDto>.SuccessResult(_mapper.Map<InventoryCheckDto>(savedCheck)));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InventoryCheckDto>.ErrorResult("Error updating inventory check", ex.Message));
            }
        }

        /// <summary>
        /// Xóa phiếu kiểm kê (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteInventoryCheck(int id)
        {
            try
            {
                var inventoryCheck = await _unitOfWork.InventoryChecks.GetByIdAsync(id);
                if (inventoryCheck == null || inventoryCheck.IsDeleted)
                {
                    return NotFound(ApiResponse.ErrorResult("Inventory check not found"));
                }

                var checkCode = inventoryCheck.Code;
                await _unitOfWork.InventoryChecks.DeleteAsync(inventoryCheck);
                await _unitOfWork.SaveChangesAsync();

                // ✅ Phase 4.1 - Advanced Features: Log audit
                await LogAuditAsync("InventoryCheck", id, "Delete", 
                    $"Xóa phiếu kiểm kê: {checkCode}, Status: {inventoryCheck.Status}", 
                    "Warning");

                return Ok(ApiResponse.SuccessResult("Inventory check deleted"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResult("Error deleting inventory check", ex.Message));
            }
        }

        /// <summary>
        /// Hoàn thành kiểm kê
        /// </summary>
        [HttpPost("{id}/complete")]
        public async Task<ActionResult<ApiResponse<InventoryCheckDto>>> CompleteInventoryCheck(int id)
        {
            try
            {
                var inventoryCheck = await _context.InventoryChecks
                    .Where(ic => !ic.IsDeleted && ic.Id == id)
                    .Include(ic => ic.Items.Where(i => !i.IsDeleted))
                    .FirstOrDefaultAsync();
                if (inventoryCheck == null)
                {
                    return NotFound(ApiResponse<InventoryCheckDto>.ErrorResult("Inventory check not found"));
                }

                if (inventoryCheck.Status == "Completed")
                {
                    return BadRequest(ApiResponse<InventoryCheckDto>.ErrorResult("Phiếu kiểm kê đã hoàn thành"));
                }

                // Get current user (employee)
                var currentUserId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                int? employeeId = null;
                if (!string.IsNullOrEmpty(currentUserId) && int.TryParse(currentUserId, out var userId))
                {
                    employeeId = userId;
                }

                inventoryCheck.Status = "Completed";
                inventoryCheck.CompletedDate = DateTime.Now;
                inventoryCheck.CompletedByEmployeeId = employeeId;

                // Bắt đầu transaction
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.InventoryChecks.UpdateAsync(inventoryCheck);
                    await _unitOfWork.SaveChangesAsync();

                    // Commit transaction
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }

                // Reload với details
                var savedCheck = await _context.InventoryChecks
                    .AsNoTracking()
                    .Where(ic => !ic.IsDeleted && ic.Id == inventoryCheck.Id)
                    .Include(ic => ic.Warehouse)
                    .Include(ic => ic.WarehouseZone)
                    .Include(ic => ic.WarehouseBin)
                    .Include(ic => ic.StartedByEmployee)
                    .Include(ic => ic.CompletedByEmployee)
                    .Include(ic => ic.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Part)
                    .FirstOrDefaultAsync();

                if (savedCheck == null)
                {
                    return StatusCode(500, ApiResponse<InventoryCheckDto>.ErrorResult("Không thể tải lại thông tin phiếu kiểm kê sau khi hoàn thành"));
                }

                // ✅ Phase 4.1 - Advanced Features: Log audit
                var discrepancyCount = savedCheck.Items.Count(i => i.IsDiscrepancy);
                await LogAuditAsync("InventoryCheck", savedCheck.Id, "Complete", 
                    $"Hoàn thành phiếu kiểm kê: {savedCheck.Code}. Tổng items: {savedCheck.Items.Count}, Items có chênh lệch: {discrepancyCount}", 
                    "Info");

                return Ok(ApiResponse<InventoryCheckDto>.SuccessResult(_mapper.Map<InventoryCheckDto>(savedCheck)));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InventoryCheckDto>.ErrorResult("Error completing inventory check", ex.Message));
            }
        }

        /// <summary>
        /// Thêm item vào phiếu kiểm kê
        /// </summary>
        [HttpPost("{id}/items")]
        public async Task<ActionResult<ApiResponse<InventoryCheckItemDto>>> AddInventoryCheckItem(int id, CreateInventoryCheckItemDto dto)
        {
            try
            {
                var inventoryCheck = await _context.InventoryChecks
                    .Where(ic => !ic.IsDeleted && ic.Id == id)
                    .FirstOrDefaultAsync();
                if (inventoryCheck == null)
                {
                    return NotFound(ApiResponse<InventoryCheckItemDto>.ErrorResult("Inventory check not found"));
                }

                if (inventoryCheck.Status == "Completed")
                {
                    return BadRequest(ApiResponse<InventoryCheckItemDto>.ErrorResult("Không thể thêm item vào phiếu kiểm kê đã hoàn thành"));
                }

                // Validate part exists
                var part = await _context.Parts
                    .Where(p => !p.IsDeleted && p.Id == dto.PartId)
                    .FirstOrDefaultAsync();
                if (part == null)
                {
                    return BadRequest(ApiResponse<InventoryCheckItemDto>.ErrorResult($"Phụ tùng với ID {dto.PartId} không tồn tại"));
                }

                // Check if item already exists
                var existingItem = await _context.InventoryCheckItems
                    .Where(i => !i.IsDeleted && i.InventoryCheckId == id && i.PartId == dto.PartId)
                    .FirstOrDefaultAsync();
                if (existingItem != null)
                {
                    return BadRequest(ApiResponse<InventoryCheckItemDto>.ErrorResult($"Phụ tùng đã tồn tại trong phiếu kiểm kê"));
                }

                // Load SystemQuantity from Part.QuantityInStock
                var systemQuantity = part.QuantityInStock;

                // Create item
                var item = _mapper.Map<InventoryCheckItem>(dto);
                item.InventoryCheckId = id;
                item.SystemQuantity = systemQuantity;
                item.DiscrepancyQuantity = dto.ActualQuantity - systemQuantity;
                item.IsDiscrepancy = dto.ActualQuantity != systemQuantity;
                item.IsAdjusted = false;
                item.CreatedAt = DateTime.Now;
                item.IsDeleted = false;

                // Bắt đầu transaction
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.InventoryCheckItems.AddAsync(item);
                    await _unitOfWork.SaveChangesAsync();

                    // Commit transaction
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }

                // Reload với details
                var savedItem = await _context.InventoryCheckItems
                    .AsNoTracking()
                    .Where(i => !i.IsDeleted && i.Id == item.Id)
                    .Include(i => i.Part)
                    .FirstOrDefaultAsync();

                if (savedItem == null)
                {
                    return StatusCode(500, ApiResponse<InventoryCheckItemDto>.ErrorResult("Không thể tải lại thông tin item sau khi tạo"));
                }

                // ✅ Phase 4.1 - Advanced Features: Log audit
                await LogAuditAsync("InventoryCheckItem", savedItem.Id, "Create", 
                    $"Thêm item vào phiếu kiểm kê {inventoryCheck.Code}: PartId={savedItem.PartId}, ActualQty={savedItem.ActualQuantity}, SystemQty={savedItem.SystemQuantity}, IsDiscrepancy={savedItem.IsDiscrepancy}");

                return CreatedAtAction(nameof(GetInventoryCheck), new { id = id }, ApiResponse<InventoryCheckItemDto>.SuccessResult(_mapper.Map<InventoryCheckItemDto>(savedItem)));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InventoryCheckItemDto>.ErrorResult("Error adding inventory check item", ex.Message));
            }
        }

        /// <summary>
        /// Cập nhật item trong phiếu kiểm kê
        /// </summary>
        [HttpPut("{id}/items/{itemId}")]
        public async Task<ActionResult<ApiResponse<InventoryCheckItemDto>>> UpdateInventoryCheckItem(int id, int itemId, UpdateInventoryCheckItemDto dto)
        {
            try
            {
                if (itemId != dto.Id)
                {
                    return BadRequest(ApiResponse<InventoryCheckItemDto>.ErrorResult("Item ID mismatch"));
                }

                var inventoryCheck = await _context.InventoryChecks
                    .Where(ic => !ic.IsDeleted && ic.Id == id)
                    .FirstOrDefaultAsync();
                if (inventoryCheck == null)
                {
                    return NotFound(ApiResponse<InventoryCheckItemDto>.ErrorResult("Inventory check not found"));
                }

                if (inventoryCheck.Status == "Completed")
                {
                    return BadRequest(ApiResponse<InventoryCheckItemDto>.ErrorResult("Không thể cập nhật item trong phiếu kiểm kê đã hoàn thành"));
                }

                var item = await _context.InventoryCheckItems
                    .Where(i => !i.IsDeleted && i.Id == itemId && i.InventoryCheckId == id)
                    .Include(i => i.Part)
                    .FirstOrDefaultAsync();
                if (item == null)
                {
                    return NotFound(ApiResponse<InventoryCheckItemDto>.ErrorResult("Inventory check item not found"));
                }

                // ✅ Track old values for audit
                var oldActualQty = item.ActualQuantity;
                var oldIsDiscrepancy = item.IsDiscrepancy;

                // Validate part exists (if changed)
                if (item.PartId != dto.PartId)
                {
                    var part = await _context.Parts
                        .Where(p => !p.IsDeleted && p.Id == dto.PartId)
                        .FirstOrDefaultAsync();
                    if (part == null)
                    {
                        return BadRequest(ApiResponse<InventoryCheckItemDto>.ErrorResult($"Phụ tùng với ID {dto.PartId} không tồn tại"));
                    }

                    // Check if new part already exists in this check
                    var existingItem = await _context.InventoryCheckItems
                        .Where(i => !i.IsDeleted && i.InventoryCheckId == id && i.PartId == dto.PartId && i.Id != itemId)
                        .FirstOrDefaultAsync();
                    if (existingItem != null)
                    {
                        return BadRequest(ApiResponse<InventoryCheckItemDto>.ErrorResult($"Phụ tùng đã tồn tại trong phiếu kiểm kê"));
                    }

                    // Load SystemQuantity from new Part.QuantityInStock
                    item.SystemQuantity = part.QuantityInStock;
                    item.PartId = dto.PartId;
                }
                else
                {
                    // ✅ FIX: Reload Part từ database để đảm bảo SystemQuantity chính xác
                    // item.Part có thể đã bị thay đổi trong database, nên cần reload
                    var currentPart = await _context.Parts
                        .Where(p => !p.IsDeleted && p.Id == item.PartId)
                        .FirstOrDefaultAsync();
                    if (currentPart != null)
                    {
                        item.SystemQuantity = currentPart.QuantityInStock;
                    }
                    else if (item.Part != null)
                    {
                        // Fallback: Nếu không reload được, dùng Part đã load
                        item.SystemQuantity = item.Part.QuantityInStock;
                    }
                }

                // Map DTO to Entity
                _mapper.Map(dto, item);
                item.DiscrepancyQuantity = dto.ActualQuantity - item.SystemQuantity;
                item.IsDiscrepancy = dto.ActualQuantity != item.SystemQuantity;
                item.UpdatedAt = DateTime.Now;

                // Bắt đầu transaction
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.InventoryCheckItems.UpdateAsync(item);
                    await _unitOfWork.SaveChangesAsync();

                    // Commit transaction
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }

                // Reload với details
                var savedItem = await _context.InventoryCheckItems
                    .AsNoTracking()
                    .Where(i => !i.IsDeleted && i.Id == item.Id)
                    .Include(i => i.Part)
                    .FirstOrDefaultAsync();

                if (savedItem == null)
                {
                    return StatusCode(500, ApiResponse<InventoryCheckItemDto>.ErrorResult("Không thể tải lại thông tin item sau khi cập nhật"));
                }

                // ✅ Phase 4.1 - Advanced Features: Log audit
                var changes = new List<string>();
                if (oldActualQty != savedItem.ActualQuantity)
                {
                    changes.Add($"ActualQuantity: {oldActualQty} → {savedItem.ActualQuantity}");
                }
                if (oldIsDiscrepancy != savedItem.IsDiscrepancy)
                {
                    changes.Add($"IsDiscrepancy: {oldIsDiscrepancy} → {savedItem.IsDiscrepancy}");
                }
                var details = changes.Any()
                    ? $"Cập nhật item trong phiếu kiểm kê {inventoryCheck.Code}: PartId={savedItem.PartId}. Thay đổi: {string.Join(", ", changes)}"
                    : $"Cập nhật item trong phiếu kiểm kê {inventoryCheck.Code}: PartId={savedItem.PartId}";
                await LogAuditAsync("InventoryCheckItem", savedItem.Id, "Update", details);

                return Ok(ApiResponse<InventoryCheckItemDto>.SuccessResult(_mapper.Map<InventoryCheckItemDto>(savedItem)));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InventoryCheckItemDto>.ErrorResult("Error updating inventory check item", ex.Message));
            }
        }

        /// <summary>
        /// Xóa item khỏi phiếu kiểm kê (soft delete)
        /// </summary>
        [HttpDelete("{id}/items/{itemId}")]
        public async Task<ActionResult<ApiResponse>> DeleteInventoryCheckItem(int id, int itemId)
        {
            try
            {
                var inventoryCheck = await _context.InventoryChecks
                    .Where(ic => !ic.IsDeleted && ic.Id == id)
                    .FirstOrDefaultAsync();
                if (inventoryCheck == null)
                {
                    return NotFound(ApiResponse.ErrorResult("Inventory check not found"));
                }

                if (inventoryCheck.Status == "Completed")
                {
                    return BadRequest(ApiResponse.ErrorResult("Không thể xóa item trong phiếu kiểm kê đã hoàn thành"));
                }

                var item = await _unitOfWork.InventoryCheckItems.GetByIdAsync(itemId);
                if (item == null || item.IsDeleted || item.InventoryCheckId != id)
                {
                    return NotFound(ApiResponse.ErrorResult("Inventory check item not found"));
                }

                var partInfo = item.Part != null ? $"{item.Part.PartNumber}" : $"PartId={item.PartId}";
                await _unitOfWork.InventoryCheckItems.DeleteAsync(item);
                await _unitOfWork.SaveChangesAsync();

                // ✅ Phase 4.1 - Advanced Features: Log audit
                await LogAuditAsync("InventoryCheckItem", itemId, "Delete", 
                    $"Xóa item khỏi phiếu kiểm kê {inventoryCheck.Code}: {partInfo}, ActualQty={item.ActualQuantity}", 
                    "Warning");

                return Ok(ApiResponse.SuccessResult("Inventory check item deleted"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResult("Error deleting inventory check item", ex.Message));
            }
        }

        /// <summary>
        /// ✅ Phase 4.1 - Advanced Features: Bulk update Inventory Check Items
        /// </summary>
        [HttpPost("{checkId}/items/bulk-update")]
        public async Task<ActionResult<ApiResponse<BulkOperationResultDto>>> BulkUpdateInventoryCheckItems(
            int checkId, 
            [FromBody] BulkUpdateInventoryCheckItemsDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<BulkOperationResultDto>.ErrorResult("Invalid data"));

                if (dto.InventoryCheckId != checkId)
                    return BadRequest(ApiResponse<BulkOperationResultDto>.ErrorResult("Check ID mismatch"));

                if (dto.ItemIds == null || !dto.ItemIds.Any())
                    return BadRequest(ApiResponse<BulkOperationResultDto>.ErrorResult("Phải chọn ít nhất 1 item"));

                var inventoryCheck = await _context.InventoryChecks
                    .Where(ic => !ic.IsDeleted && ic.Id == checkId)
                    .FirstOrDefaultAsync();
                
                if (inventoryCheck == null)
                    return NotFound(ApiResponse<BulkOperationResultDto>.ErrorResult("Inventory check not found"));

                if (inventoryCheck.Status == "Completed")
                    return BadRequest(ApiResponse<BulkOperationResultDto>.ErrorResult("Không thể cập nhật items trong phiếu kiểm kê đã hoàn thành"));

                var result = new BulkOperationResultDto();

                // Load items cần update (query hiệu quả)
                var itemIds = dto.ItemIds.Distinct().ToList();
                var items = await _context.InventoryCheckItems
                    .Where(i => !i.IsDeleted && i.InventoryCheckId == checkId && itemIds.Contains(i.Id))
                    .Include(i => i.Part)
                    .ToListAsync();

                if (!items.Any())
                    return BadRequest(ApiResponse<BulkOperationResultDto>.ErrorResult("Không tìm thấy items nào"));

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    foreach (var item in items)
                    {
                        try
                        {
                            // Update các fields nếu có giá trị
                            if (dto.ActualQuantity.HasValue)
                            {
                                if (dto.ActualQuantity.Value < 0)
                                {
                                    result.Errors.Add($"Item {item.Part?.PartNumber}: ActualQuantity không được âm");
                                    result.FailedIds.Add(item.Id);
                                    result.FailureCount++;
                                    continue;
                                }
                                item.ActualQuantity = dto.ActualQuantity.Value;
                                
                                // Auto-calculate discrepancy
                                item.IsDiscrepancy = item.ActualQuantity != item.SystemQuantity;
                                item.DiscrepancyQuantity = item.ActualQuantity - item.SystemQuantity;
                            }

                            if (!string.IsNullOrWhiteSpace(dto.Notes))
                                item.Notes = dto.Notes.Trim();

                            if (dto.IsDiscrepancy.HasValue)
                            {
                                // ✅ FIX: Nếu ActualQuantity đã được update, IsDiscrepancy đã được tính tự động
                                // Chỉ override nếu IsDiscrepancy được set mà ActualQuantity không được update
                                if (!dto.ActualQuantity.HasValue)
                                {
                                    item.IsDiscrepancy = dto.IsDiscrepancy.Value;
                                    if (dto.IsDiscrepancy.Value)
                                    {
                                        // Nếu set IsDiscrepancy = true nhưng chưa có ActualQuantity, giữ nguyên ActualQuantity hiện tại
                                        item.DiscrepancyQuantity = item.ActualQuantity - item.SystemQuantity;
                                    }
                                }
                                // Nếu ActualQuantity đã được update, IsDiscrepancy đã được tính tự động ở trên
                            }

                            await _unitOfWork.InventoryCheckItems.UpdateAsync(item);
                            result.SuccessIds.Add(item.Id);
                            result.SuccessCount++;
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"Item {item.Part?.PartNumber}: {ex.Message}");
                            result.FailedIds.Add(item.Id);
                            result.FailureCount++;
                        }
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    return Ok(ApiResponse<BulkOperationResultDto>.SuccessResult(result, 
                        $"Đã cập nhật {result.SuccessCount} items thành công" + 
                        (result.FailureCount > 0 ? $", {result.FailureCount} items thất bại" : "")));
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<BulkOperationResultDto>.ErrorResult("Error updating items", ex.Message));
            }
        }

        /// <summary>
        /// ✅ Phase 4.1 - Advanced Features: Bulk add Inventory Check Items
        /// </summary>
        [HttpPost("{checkId}/items/bulk-add")]
        public async Task<ActionResult<ApiResponse<BulkOperationResultDto>>> BulkAddInventoryCheckItems(
            int checkId, 
            [FromBody] BulkAddInventoryCheckItemsDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<BulkOperationResultDto>.ErrorResult("Invalid data"));

                if (dto.InventoryCheckId != checkId)
                    return BadRequest(ApiResponse<BulkOperationResultDto>.ErrorResult("Check ID mismatch"));

                if (dto.Items == null || !dto.Items.Any())
                    return BadRequest(ApiResponse<BulkOperationResultDto>.ErrorResult("Phải có ít nhất 1 item"));

                var inventoryCheck = await _context.InventoryChecks
                    .Where(ic => !ic.IsDeleted && ic.Id == checkId)
                    .FirstOrDefaultAsync();
                
                if (inventoryCheck == null)
                    return NotFound(ApiResponse<BulkOperationResultDto>.ErrorResult("Inventory check not found"));

                if (inventoryCheck.Status == "Completed")
                    return BadRequest(ApiResponse<BulkOperationResultDto>.ErrorResult("Không thể thêm items vào phiếu kiểm kê đã hoàn thành"));

                var result = new BulkOperationResultDto();

                // Get existing part IDs in this check để tránh duplicate
                var existingPartIds = (await _context.InventoryCheckItems
                    .Where(i => !i.IsDeleted && i.InventoryCheckId == checkId)
                    .Select(i => i.PartId)
                    .ToListAsync()).ToHashSet();

                // ✅ FIX: Batch load parts để tránh N+1 query problem
                var partIdsToLoad = dto.Items.Select(i => i.PartId).Distinct().ToList();
                var partsDict = partIdsToLoad.Any()
                    ? (await _context.Parts
                        .Where(p => !p.IsDeleted && partIdsToLoad.Contains(p.Id))
                        .ToListAsync())
                        .ToDictionary(p => p.Id)
                    : new Dictionary<int, Core.Entities.Part>();

                // ✅ FIX: Track successful part IDs để reload sau
                var successfulPartIds = new List<int>();
                // Note: transactionStartTime sẽ được set sau khi SaveChanges để đảm bảo chính xác

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    foreach (var itemDto in dto.Items)
                    {
                        try
                        {
                            // Validate part exists (sử dụng dictionary)
                            if (!partsDict.TryGetValue(itemDto.PartId, out var part))
                            {
                                result.Errors.Add($"Phụ tùng ID {itemDto.PartId} không tồn tại");
                                result.FailureCount++;
                                continue;
                            }

                            // Check duplicate
                            if (existingPartIds.Contains(itemDto.PartId))
                            {
                                result.Errors.Add($"Phụ tùng {part.PartNumber} đã có trong phiếu kiểm kê");
                                result.FailureCount++;
                                continue;
                            }

                            // Get current stock quantity
                            var systemQuantity = part.QuantityInStock;

                            var item = new Core.Entities.InventoryCheckItem
                            {
                                InventoryCheckId = checkId,
                                PartId = itemDto.PartId,
                                SystemQuantity = systemQuantity,
                                ActualQuantity = itemDto.ActualQuantity,
                                DiscrepancyQuantity = itemDto.ActualQuantity - systemQuantity,
                                IsDiscrepancy = itemDto.ActualQuantity != systemQuantity,
                                Notes = string.IsNullOrWhiteSpace(itemDto.Notes) ? null : itemDto.Notes.Trim()
                            };

                            await _unitOfWork.InventoryCheckItems.AddAsync(item);
                            existingPartIds.Add(itemDto.PartId); // Track để tránh duplicate trong cùng batch
                            successfulPartIds.Add(itemDto.PartId); // Track để reload sau
                            result.SuccessCount++;
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"Item PartId {itemDto.PartId}: {ex.Message}");
                            result.FailureCount++;
                        }
                    }

                    // ✅ FIX: Set time buffer trước SaveChanges để capture items được tạo
                    var timeBuffer = DateTime.UtcNow.AddSeconds(-2); // Buffer 2 giây để đảm bảo capture
                    
                    await _unitOfWork.SaveChangesAsync();
                    
                    // ✅ FIX: Reload items để lấy IDs đã được generate (chỉ reload những items vừa thêm)
                    // Sử dụng CreatedAt để filter chỉ những items vừa được tạo trong transaction này
                    if (successfulPartIds.Any())
                    {
                        var addedItems = await _context.InventoryCheckItems
                            .Where(i => !i.IsDeleted && 
                                i.InventoryCheckId == checkId && 
                                successfulPartIds.Contains(i.PartId) &&
                                i.CreatedAt >= timeBuffer)
                            .OrderByDescending(i => i.CreatedAt)
                            .Take(successfulPartIds.Count)
                            .Select(i => i.Id)
                            .ToListAsync();
                        
                        result.SuccessIds = addedItems;
                    }
                    
                    await _unitOfWork.CommitTransactionAsync();

                    return Ok(ApiResponse<BulkOperationResultDto>.SuccessResult(result, 
                        $"Đã thêm {result.SuccessCount} items thành công" + 
                        (result.FailureCount > 0 ? $", {result.FailureCount} items thất bại" : "")));
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<BulkOperationResultDto>.ErrorResult("Error adding items", ex.Message));
            }
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

                var auditLog = new Core.Entities.AuditLog
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
                // Có thể log vào file hoặc external service sau này
            }
        }

        /// <summary>
        /// Generate mã phiếu kiểm kê tự động (IK-YYYY-NNN)
        /// </summary>
        /// <remarks>
        /// ✅ NOTE: Method này có thể có race condition nếu 2 requests cùng lúc tạo phiếu kiểm kê.
        /// Tuy nhiên, unique constraint ở database sẽ catch duplicate code và throw DbUpdateException.
        /// Controller sẽ handle exception này và retry với code mới (nếu cần).
        /// 
        /// ✅ NOTE: Không filter IsDeleted vì cho phép reuse code sau khi soft delete (theo yêu cầu user).
        /// </remarks>
        private async Task<string> GenerateInventoryCheckCodeAsync()
        {
            var year = DateTime.Now.Year;
            var prefix = $"IK-{year}";

            // ✅ OPTIMIZED: Count ở database level
            // ✅ NOTE: Không filter IsDeleted để cho phép reuse code sau khi soft delete
            var lastCheck = await _context.InventoryChecks
                .Where(ic => ic.Code.StartsWith(prefix))
                .OrderByDescending(ic => ic.Code)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastCheck != null)
            {
                // Extract number from code (IK-YYYY-NNN)
                var parts = lastCheck.Code.Split('-');
                if (parts.Length >= 3 && int.TryParse(parts[2], out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}-{nextNumber:D3}";
        }

        /// <summary>
        /// Export danh sách phiếu kiểm kê ra Excel
        /// </summary>
        [HttpGet("export-excel")]
        public async Task<IActionResult> ExportExcel(
            [FromQuery] int? warehouseId = null,
            [FromQuery] int? warehouseZoneId = null,
            [FromQuery] int? warehouseBinId = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Reuse GetInventoryChecks query logic
                var query = _context.InventoryChecks
                    .AsNoTracking()
                    .Where(ic => !ic.IsDeleted)
                    .Include(ic => ic.Warehouse)
                    .Include(ic => ic.WarehouseZone)
                    .Include(ic => ic.WarehouseBin)
                    .Include(ic => ic.StartedByEmployee)
                    .Include(ic => ic.CompletedByEmployee)
                    .Include(ic => ic.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Part)
                    .AsQueryable();

                // Apply filters
                if (warehouseId.HasValue)
                    query = query.Where(ic => ic.WarehouseId == warehouseId.Value);
                if (warehouseZoneId.HasValue)
                    query = query.Where(ic => ic.WarehouseZoneId == warehouseZoneId.Value);
                if (warehouseBinId.HasValue)
                    query = query.Where(ic => ic.WarehouseBinId == warehouseBinId.Value);
                if (!string.IsNullOrEmpty(status))
                    query = query.Where(ic => ic.Status == status);
                if (startDate.HasValue)
                    query = query.Where(ic => ic.CheckDate >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(ic => ic.CheckDate <= endDate.Value);

                var inventoryChecks = await query
                    .OrderByDescending(ic => ic.CheckDate)
                    .ThenByDescending(ic => ic.CreatedAt)
                    .ToListAsync();

                var dto = _mapper.Map<List<InventoryCheckDto>>(inventoryChecks);

#pragma warning disable CS0618
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
#pragma warning restore CS0618
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Danh Sách Kiểm Kê");

                // Title
                worksheet.Cells[1, 1].Value = "DANH SÁCH PHIẾU KIỂM KÊ";
                worksheet.Cells[1, 1, 1, 10].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 16;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Export date
                worksheet.Cells[2, 1].Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
                worksheet.Cells[2, 1, 2, 10].Merge = true;
                worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Headers
                int row = 4;
                var headers = new[] { "Mã Phiếu", "Tên Phiếu", "Ngày Kiểm Kê", "Kho", "Khu Vực", "Kệ/Ngăn", "Trạng Thái", "Số Items", "Người Bắt Đầu", "Ngày Hoàn Thành" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[row, i + 1].Value = headers[i];
                    worksheet.Cells[row, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[row, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
                    worksheet.Cells[row, i + 1].Style.Font.Color.SetColor(Color.White);
                    worksheet.Cells[row, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Data
                row++;
                foreach (var check in dto)
                {
                    worksheet.Cells[row, 1].Value = check.Code;
                    worksheet.Cells[row, 2].Value = check.Name;
                    worksheet.Cells[row, 3].Value = check.CheckDate;
                    worksheet.Cells[row, 3].Style.Numberformat.Format = "dd/MM/yyyy";
                    worksheet.Cells[row, 4].Value = check.WarehouseName ?? "-";
                    worksheet.Cells[row, 5].Value = check.WarehouseZoneName ?? "-";
                    worksheet.Cells[row, 6].Value = check.WarehouseBinName ?? "-";
                    worksheet.Cells[row, 7].Value = GetStatusDisplay(check.Status);
                    worksheet.Cells[row, 8].Value = check.Items?.Count ?? 0;
                    worksheet.Cells[row, 9].Value = check.StartedByEmployeeName ?? "-";
                    worksheet.Cells[row, 10].Value = check.CompletedDate;
                    if (check.CompletedDate.HasValue)
                    {
                        worksheet.Cells[row, 10].Style.Numberformat.Format = "dd/MM/yyyy";
                    }
                    row++;
                }

                // Auto fit columns
                worksheet.Cells[4, 1, row - 1, 10].AutoFitColumns();
                worksheet.Cells[4, 1, row - 1, 10].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[4, 1, row - 1, 10].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[4, 1, row - 1, 10].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[4, 1, row - 1, 10].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[4, 1, row - 1, 10].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                var fileName = $"DanhSachKiemKe-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                var bytes = package.GetAsByteArray();
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResult("Lỗi khi xuất Excel", ex.Message));
            }
        }

        /// <summary>
        /// Export chi tiết phiếu kiểm kê ra Excel
        /// </summary>
        [HttpGet("{id}/export-excel")]
        public async Task<IActionResult> ExportCheckDetailExcel(int id)
        {
            try
            {
                var inventoryCheck = await _context.InventoryChecks
                    .AsNoTracking()
                    .Where(ic => !ic.IsDeleted && ic.Id == id)
                    .Include(ic => ic.Warehouse)
                    .Include(ic => ic.WarehouseZone)
                    .Include(ic => ic.WarehouseBin)
                    .Include(ic => ic.StartedByEmployee)
                    .Include(ic => ic.CompletedByEmployee)
                    .Include(ic => ic.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Part)
                    .FirstOrDefaultAsync();

                if (inventoryCheck == null)
                {
                    return NotFound(ApiResponse.ErrorResult("Không tìm thấy phiếu kiểm kê"));
                }

                var dto = _mapper.Map<InventoryCheckDto>(inventoryCheck);

#pragma warning disable CS0618
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
#pragma warning restore CS0618
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Chi Tiết Kiểm Kê");

                int row = 1;

                // Title
                worksheet.Cells[row, 1].Value = "PHIẾU KIỂM KÊ";
                worksheet.Cells[row, 1, row, 6].Merge = true;
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                worksheet.Cells[row, 1].Style.Font.Size = 18;
                worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                row += 2;

                // Basic Info
                worksheet.Cells[row, 1].Value = "Mã Phiếu:";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                worksheet.Cells[row, 2].Value = dto.Code;
                row++;

                worksheet.Cells[row, 1].Value = "Tên Phiếu:";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                worksheet.Cells[row, 2].Value = dto.Name;
                row++;

                worksheet.Cells[row, 1].Value = "Ngày Kiểm Kê:";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                worksheet.Cells[row, 2].Value = dto.CheckDate;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "dd/MM/yyyy";
                row++;

                worksheet.Cells[row, 1].Value = "Kho:";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                worksheet.Cells[row, 2].Value = dto.WarehouseName ?? "-";
                row++;

                if (!string.IsNullOrEmpty(dto.WarehouseZoneName))
                {
                    worksheet.Cells[row, 1].Value = "Khu Vực:";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 2].Value = dto.WarehouseZoneName;
                    row++;
                }

                if (!string.IsNullOrEmpty(dto.WarehouseBinName))
                {
                    worksheet.Cells[row, 1].Value = "Kệ/Ngăn:";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 2].Value = dto.WarehouseBinName;
                    row++;
                }

                worksheet.Cells[row, 1].Value = "Trạng Thái:";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                worksheet.Cells[row, 2].Value = GetStatusDisplay(dto.Status);
                row++;

                if (!string.IsNullOrEmpty(dto.Description))
                {
                    worksheet.Cells[row, 1].Value = "Mô Tả:";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 2].Value = dto.Description;
                    row++;
                }

                row += 2;

                // Items Table
                worksheet.Cells[row, 1].Value = "DANH SÁCH ITEMS";
                worksheet.Cells[row, 1, row, 7].Merge = true;
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                worksheet.Cells[row, 1].Style.Font.Size = 14;
                row++;

                var itemHeaders = new[] { "STT", "Mã Phụ Tùng", "Tên Phụ Tùng", "SKU", "Số Lượng Hệ Thống", "Số Lượng Thực Tế", "Chênh Lệch", "Ghi Chú" };
                for (int i = 0; i < itemHeaders.Length; i++)
                {
                    worksheet.Cells[row, i + 1].Value = itemHeaders[i];
                    worksheet.Cells[row, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[row, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
                    worksheet.Cells[row, i + 1].Style.Font.Color.SetColor(Color.White);
                    worksheet.Cells[row, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                row++;

                int stt = 1;
                foreach (var item in dto.Items ?? new List<InventoryCheckItemDto>())
                {
                    worksheet.Cells[row, 1].Value = stt++;
                    worksheet.Cells[row, 2].Value = item.PartNumber ?? "-";
                    worksheet.Cells[row, 3].Value = item.PartName ?? "-";
                    worksheet.Cells[row, 4].Value = item.PartSku ?? "-";
                    worksheet.Cells[row, 5].Value = item.SystemQuantity;
                    worksheet.Cells[row, 6].Value = item.ActualQuantity;
                    worksheet.Cells[row, 7].Value = item.DiscrepancyQuantity;
                    
                    // Color coding for discrepancy
                    if (item.DiscrepancyQuantity > 0)
                    {
                        worksheet.Cells[row, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, 7].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(198, 239, 206)); // Green
                    }
                    else if (item.DiscrepancyQuantity < 0)
                    {
                        worksheet.Cells[row, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, 7].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 199, 206)); // Red
                    }
                    
                    worksheet.Cells[row, 8].Value = item.Notes ?? "-";
                    row++;
                }

                // Statistics
                row += 2;
                worksheet.Cells[row, 1].Value = "THỐNG KÊ";
                worksheet.Cells[row, 1, row, 7].Merge = true;
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                worksheet.Cells[row, 1].Style.Font.Size = 14;
                row++;

                var totalItems = dto.Items?.Count ?? 0;
                var discrepancyItems = dto.Items?.Count(i => i.IsDiscrepancy) ?? 0;
                var shortageItems = dto.Items?.Count(i => i.DiscrepancyQuantity < 0) ?? 0;
                var surplusItems = dto.Items?.Count(i => i.DiscrepancyQuantity > 0) ?? 0;

                worksheet.Cells[row, 1].Value = "Tổng số items:";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                worksheet.Cells[row, 2].Value = totalItems;
                row++;

                worksheet.Cells[row, 1].Value = "Items có chênh lệch:";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                worksheet.Cells[row, 2].Value = discrepancyItems;
                row++;

                worksheet.Cells[row, 1].Value = "Items thiếu:";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                worksheet.Cells[row, 2].Value = shortageItems;
                row++;

                worksheet.Cells[row, 1].Value = "Items thừa:";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                worksheet.Cells[row, 2].Value = surplusItems;
                row++;

                // Auto fit columns
                worksheet.Cells[1, 1, row - 1, 8].AutoFitColumns();
                worksheet.Cells[row - 5, 1, row - 1, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                var fileName = $"ChiTietKiemKe-{dto.Code}-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                var bytes = package.GetAsByteArray();
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResult("Lỗi khi xuất Excel", ex.Message));
            }
        }

        private string GetStatusDisplay(string? status)
        {
            return status switch
            {
                "Draft" => "Nháp",
                "InProgress" => "Đang thực hiện",
                "Completed" => "Hoàn thành",
                "Cancelled" => "Đã hủy",
                _ => status ?? "-"
            };
        }

        /// <summary>
        /// ✅ Phase 4.1 - Advanced Features: Lấy lịch sử audit cho Inventory Check
        /// </summary>
        [HttpGet("{id}/history")]
        public async Task<ActionResult<ApiResponse<List<AuditLogDto>>>> GetInventoryCheckHistory(
            int id,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? action = null)
        {
            try
            {
                // Verify check exists
                var check = await _context.InventoryChecks
                    .Where(ic => !ic.IsDeleted && ic.Id == id)
                    .FirstOrDefaultAsync();
                
                if (check == null)
                {
                    return NotFound(ApiResponse<List<AuditLogDto>>.ErrorResult("Inventory check not found"));
                }

                // ✅ FIX: Build query với filters ở database level để optimize performance
                var query = _context.AuditLogs
                    .AsNoTracking()
                    .Where(al => al.EntityName == "InventoryCheck" && al.EntityId == id);

                // Also include related InventoryCheckItem audit logs
                var itemIds = await _context.InventoryCheckItems
                    .Where(i => !i.IsDeleted && i.InventoryCheckId == id)
                    .Select(i => i.Id)
                    .ToListAsync();

                IQueryable<AuditLog> itemQuery = _context.AuditLogs.AsNoTracking();
                
                // ✅ FIX: Chỉ query item logs nếu có items
                if (itemIds.Any())
                {
                    itemQuery = _context.AuditLogs
                        .AsNoTracking()
                        .Where(al => al.EntityName == "InventoryCheckItem" && 
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
        /// ✅ Phase 4.1 - Advanced Features: Thêm comment vào Inventory Check
        /// </summary>
        [HttpPost("{id}/comments")]
        public async Task<ActionResult<ApiResponse<InventoryCheckCommentDto>>> AddComment(int id, [FromBody] CreateInventoryCheckCommentDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<InventoryCheckCommentDto>.ErrorResult("Invalid data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                }

                if (id != dto.InventoryCheckId)
                {
                    return BadRequest(ApiResponse<InventoryCheckCommentDto>.ErrorResult("ID mismatch"));
                }

                // Verify check exists
                var check = await _context.InventoryChecks
                    .Where(ic => !ic.IsDeleted && ic.Id == id)
                    .FirstOrDefaultAsync();
                
                if (check == null)
                {
                    return NotFound(ApiResponse<InventoryCheckCommentDto>.ErrorResult("Inventory check not found"));
                }

                // ✅ FIX: Validate CommentText không được empty sau khi trim
                var commentText = dto.CommentText?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(commentText))
                {
                    return BadRequest(ApiResponse<InventoryCheckCommentDto>.ErrorResult("Comment không được để trống"));
                }

                // Get current user
                var currentUserId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var currentUserName = User.FindFirst("name")?.Value ?? User.Identity?.Name ?? "System";

                var comment = new Core.Entities.InventoryCheckComment
                {
                    InventoryCheckId = id,
                    CommentText = commentText,
                    CreatedByUserName = currentUserName,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };

                if (!string.IsNullOrEmpty(currentUserId) && int.TryParse(currentUserId, out var userId))
                {
                    comment.CreatedByEmployeeId = userId;
                }

                await _unitOfWork.InventoryCheckComments.AddAsync(comment);
                await _unitOfWork.SaveChangesAsync();

                // Reload with employee
                var savedComment = await _context.InventoryCheckComments
                    .AsNoTracking()
                    .Where(c => !c.IsDeleted && c.Id == comment.Id)
                    .Include(c => c.CreatedByEmployee)
                    .FirstOrDefaultAsync();

                if (savedComment == null)
                {
                    return StatusCode(500, ApiResponse<InventoryCheckCommentDto>.ErrorResult("Không thể tải lại thông tin comment sau khi tạo"));
                }

                var commentDto = new InventoryCheckCommentDto
                {
                    Id = savedComment.Id,
                    InventoryCheckId = savedComment.InventoryCheckId,
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
                await LogAuditAsync("InventoryCheckComment", savedComment.Id, "Create", 
                    $"Thêm comment vào phiếu kiểm kê {check.Code}: {auditCommentPreview}", 
                    "Info");

                return CreatedAtAction(nameof(GetComments), new { id = id }, ApiResponse<InventoryCheckCommentDto>.SuccessResult(commentDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InventoryCheckCommentDto>.ErrorResult("Error adding comment", ex.Message));
            }
        }

        /// <summary>
        /// ✅ Phase 4.1 - Advanced Features: Lấy danh sách comments của Inventory Check (timeline)
        /// </summary>
        [HttpGet("{id}/comments")]
        public async Task<ActionResult<ApiResponse<List<InventoryCheckCommentDto>>>> GetComments(int id)
        {
            try
            {
                // Verify check exists
                var check = await _context.InventoryChecks
                    .Where(ic => !ic.IsDeleted && ic.Id == id)
                    .FirstOrDefaultAsync();
                
                if (check == null)
                {
                    return NotFound(ApiResponse<List<InventoryCheckCommentDto>>.ErrorResult("Inventory check not found"));
                }

                var comments = await _context.InventoryCheckComments
                    .AsNoTracking()
                    .Where(c => !c.IsDeleted && c.InventoryCheckId == id)
                    .Include(c => c.CreatedByEmployee)
                    .OrderByDescending(c => c.CreatedAt) // ✅ FIX: Timeline mới nhất trước (mới → cũ)
                    .ToListAsync();

                var dtos = comments.Select(c => new InventoryCheckCommentDto
                {
                    Id = c.Id,
                    InventoryCheckId = c.InventoryCheckId,
                    CommentText = c.CommentText,
                    CreatedByEmployeeId = c.CreatedByEmployeeId,
                    CreatedByEmployeeName = c.CreatedByEmployee?.Name,
                    CreatedByUserName = c.CreatedByUserName,
                    CreatedAt = c.CreatedAt
                }).ToList();

                return Ok(ApiResponse<List<InventoryCheckCommentDto>>.SuccessResult(dtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<InventoryCheckCommentDto>>.ErrorResult("Error getting comments", ex.Message));
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
                var comment = await _context.InventoryCheckComments
                    .Where(c => !c.IsDeleted && c.Id == commentId && c.InventoryCheckId == id)
                    .FirstOrDefaultAsync();

                if (comment == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResult("Comment not found"));
                }

                await _unitOfWork.InventoryCheckComments.DeleteAsync(comment);
                await _unitOfWork.SaveChangesAsync();

                // ✅ Phase 4.1 - Advanced Features: Log audit
                await LogAuditAsync("InventoryCheckComment", commentId, "Delete", 
                    $"Xóa comment khỏi phiếu kiểm kê ID {id}", 
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

