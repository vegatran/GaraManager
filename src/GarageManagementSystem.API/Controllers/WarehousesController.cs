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

namespace GarageManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiScope")]
    public class WarehousesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly GarageDbContext _context;

        public WarehousesController(IUnitOfWork unitOfWork, IMapper mapper, GarageDbContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<WarehouseDto>>>> GetWarehouses()
        {
            // ✅ SỬA: Sử dụng AsNoTracking để tránh tracking entities, cải thiện performance và tránh tracking conflicts
            var warehouses = await _context.Warehouses
                .AsNoTracking() // ✅ QUAN TRỌNG: Không track entities để tránh conflict và cải thiện performance
                .Where(w => !w.IsDeleted)
                .Include(w => w.Zones.Where(z => !z.IsDeleted))
                    .ThenInclude(z => z.Bins.Where(b => !b.IsDeleted))
                .Include(w => w.Bins.Where(b => !b.IsDeleted && b.WarehouseZoneId == null))
                .ToListAsync();

            var dto = _mapper.Map<List<WarehouseDto>>(warehouses);
            return Ok(ApiResponse<List<WarehouseDto>>.SuccessResult(dto));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<WarehouseDto>>> GetWarehouse(int id)
        {
            // ✅ SỬA: Load warehouse với Include để đảm bảo zones và bins được load đúng cách
            // Sử dụng Include với filter để chỉ load zones/bins không bị xóa
            var warehouse = await _context.Warehouses
                .AsNoTracking() // ✅ QUAN TRỌNG: Không track entity để tránh conflict với các operation khác
                .Where(w => !w.IsDeleted && w.Id == id)
                .Include(w => w.Zones.Where(z => !z.IsDeleted))
                    .ThenInclude(z => z.Bins.Where(b => !b.IsDeleted))
                .Include(w => w.Bins.Where(b => !b.IsDeleted && b.WarehouseZoneId == null))
                .FirstOrDefaultAsync();

            if (warehouse == null)
            {
                return NotFound(ApiResponse<WarehouseDto>.ErrorResult("Warehouse not found"));
            }

            // ✅ THÊM: Verify zones và bins có được load không
            // Nếu zones/bins không được load qua Include, load riêng
            if (warehouse.Zones == null || warehouse.Zones.Count == 0)
            {
                // ✅ FALLBACK: Load zones riêng nếu Include không load được
                var zones = await _context.WarehouseZones
                    .AsNoTracking()
                    .Where(z => z.WarehouseId == id && !z.IsDeleted)
                    .Include(z => z.Bins.Where(b => !b.IsDeleted))
                    .OrderBy(z => z.DisplayOrder)
                    .ThenBy(z => z.Code)
                    .ToListAsync();
                
                // Gán zones vào warehouse (có thể gán được ngay cả khi AsNoTracking)
                if (warehouse.Zones == null)
                {
                    warehouse.Zones = new List<WarehouseZone>();
                }
                foreach (var zone in zones)
                {
                    warehouse.Zones.Add(zone);
                }
            }

            if (warehouse.Bins == null)
            {
                // ✅ FALLBACK: Load bins riêng nếu Include không load được
                var bins = await _context.WarehouseBins
                    .AsNoTracking()
                    .Where(b => b.WarehouseId == id && !b.IsDeleted && b.WarehouseZoneId == null)
                    .OrderBy(b => b.Code)
                    .ToListAsync();
                
                // Gán bins vào warehouse
                if (warehouse.Bins == null)
                {
                    warehouse.Bins = new List<WarehouseBin>();
                }
                foreach (var bin in bins)
                {
                    warehouse.Bins.Add(bin);
                }
            }

            var dto = _mapper.Map<WarehouseDto>(warehouse);
            
            return Ok(ApiResponse<WarehouseDto>.SuccessResult(dto));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<WarehouseDto>>> CreateWarehouse(CreateWarehouseDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<WarehouseDto>.ErrorResult("Invalid data"));
            }

            // ✅ Normalize Code TRƯỚC khi validate để đảm bảo consistency
            var normalizedCode = !string.IsNullOrWhiteSpace(dto.Code) ? dto.Code.Trim() : string.Empty;

            // ✅ Validate Code không được để trống
            if (string.IsNullOrEmpty(normalizedCode))
            {
                return BadRequest(ApiResponse<WarehouseDto>.ErrorResult("Mã kho không được để trống"));
            }

            // ✅ Validate Code unique (chỉ check non-deleted warehouses)
            var existingWarehouse = await _context.Warehouses
                .Where(w => !w.IsDeleted && w.Code == normalizedCode)
                .FirstOrDefaultAsync();
            if (existingWarehouse != null)
            {
                return BadRequest(ApiResponse<WarehouseDto>.ErrorResult($"Mã kho '{normalizedCode}' đã tồn tại trong hệ thống"));
            }

            // ✅ SỬA: Kiểm tra xem có warehouse đã bị soft delete với cùng Code không
            // Nếu có, restore và cập nhật thông tin thay vì tạo mới
            var deletedWarehouse = await _context.Warehouses
                .IgnoreQueryFilters() // Bỏ qua filter IsDeleted để tìm cả warehouse đã bị soft delete
                .Where(w => w.IsDeleted && w.Code == normalizedCode)
                .FirstOrDefaultAsync();

            Warehouse entity;
            
            if (deletedWarehouse != null)
            {
                // ✅ Restore warehouse đã bị soft delete
                deletedWarehouse.IsDeleted = false;
                deletedWarehouse.DeletedAt = null;
                deletedWarehouse.DeletedBy = null;
                
                // ✅ Cập nhật thông tin với dữ liệu mới
                _mapper.Map(dto, deletedWarehouse);
                deletedWarehouse.Code = normalizedCode; // Đảm bảo Code được normalize
                
                entity = deletedWarehouse;
            }
            else
            {
                // ✅ Tạo warehouse mới
                entity = _mapper.Map<Warehouse>(dto);
                entity.Code = normalizedCode;
            }

            // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (deletedWarehouse != null)
                {
                    // ✅ Update warehouse đã restore
                    await _unitOfWork.Warehouses.UpdateAsync(entity);
                }
                else
                {
                    // ✅ Add warehouse mới
                    await _unitOfWork.Warehouses.AddAsync(entity);
                }
                
                await _unitOfWork.SaveChangesAsync();

                // Commit transaction nếu thành công
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (DbUpdateException dbEx)
            {
                // Rollback transaction nếu có lỗi
                await _unitOfWork.RollbackTransactionAsync();
                
                // ✅ Handle database unique constraint violation
                if (dbEx.InnerException != null && dbEx.InnerException.Message.Contains("Duplicate entry"))
                {
                    if (dbEx.InnerException.Message.Contains("Code"))
                    {
                        return BadRequest(ApiResponse<WarehouseDto>.ErrorResult($"Mã kho '{normalizedCode}' đã tồn tại trong hệ thống"));
                    }
                }
                return StatusCode(500, ApiResponse<WarehouseDto>.ErrorResult("Error creating warehouse", dbEx.Message));
            }
            catch
            {
                // Rollback transaction nếu có lỗi
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            // ✅ SỬA: Detach entity cũ để tránh tracking conflict
            _context.Entry(entity).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            
            // ✅ SỬA: Reload entity từ database với AsNoTracking để đảm bảo không track entity mới
            // Điều này tránh việc EF Core cố gắng save lại entity khi GetWarehouse được gọi sau đó
            var savedWarehouse = await _context.Warehouses
                .AsNoTracking() // ✅ QUAN TRỌNG: Không track entity để tránh duplicate entry error khi GetWarehouse được gọi
                .Where(w => w.Id == entity.Id && !w.IsDeleted)
                .Include(w => w.Zones.Where(z => !z.IsDeleted))
                    .ThenInclude(z => z.Bins.Where(b => !b.IsDeleted))
                .Include(w => w.Bins.Where(b => !b.IsDeleted && b.WarehouseZoneId == null))
                .FirstOrDefaultAsync();

            if (savedWarehouse == null)
            {
                return StatusCode(500, ApiResponse<WarehouseDto>.ErrorResult("Không thể tải lại thông tin kho sau khi tạo"));
            }

            return CreatedAtAction(nameof(GetWarehouse), new { id = savedWarehouse.Id }, ApiResponse<WarehouseDto>.SuccessResult(_mapper.Map<WarehouseDto>(savedWarehouse)));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<WarehouseDto>>> UpdateWarehouse(int id, UpdateWarehouseDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(ApiResponse<WarehouseDto>.ErrorResult("ID mismatch"));
            }

            var warehouse = await _context.Warehouses
                .Where(w => !w.IsDeleted && w.Id == id)
                .FirstOrDefaultAsync();
            if (warehouse == null)
            {
                return NotFound(ApiResponse<WarehouseDto>.ErrorResult("Warehouse not found"));
            }

            // ✅ Normalize Code TRƯỚC khi validate
            var normalizedCode = !string.IsNullOrWhiteSpace(dto.Code) ? dto.Code.Trim() : string.Empty;

            // ✅ Validate Code không được để trống
            if (string.IsNullOrEmpty(normalizedCode))
            {
                return BadRequest(ApiResponse<WarehouseDto>.ErrorResult("Mã kho không được để trống"));
            }

            // ✅ Validate Code unique (chỉ check nếu Code thay đổi và khác với Code hiện tại)
            if (warehouse.Code != normalizedCode)
            {
                var existingWarehouse = await _context.Warehouses
                    .Where(w => !w.IsDeleted && w.Code == normalizedCode && w.Id != id)
                    .FirstOrDefaultAsync();
                if (existingWarehouse != null)
                {
                    return BadRequest(ApiResponse<WarehouseDto>.ErrorResult($"Mã kho '{normalizedCode}' đã tồn tại trong hệ thống"));
                }
            }

            _mapper.Map(dto, warehouse);
            // ✅ Update warehouse với normalized Code
            warehouse.Code = normalizedCode;

            // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                await _unitOfWork.Warehouses.UpdateAsync(warehouse);
                await _unitOfWork.SaveChangesAsync();

                // Commit transaction nếu thành công
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (DbUpdateException dbEx)
            {
                // Rollback transaction nếu có lỗi
                await _unitOfWork.RollbackTransactionAsync();
                
                // ✅ Handle database unique constraint violation
                if (dbEx.InnerException != null && dbEx.InnerException.Message.Contains("Duplicate entry"))
                {
                    if (dbEx.InnerException.Message.Contains("Code"))
                    {
                        return BadRequest(ApiResponse<WarehouseDto>.ErrorResult($"Mã kho '{normalizedCode}' đã tồn tại trong hệ thống"));
                    }
                }
                return StatusCode(500, ApiResponse<WarehouseDto>.ErrorResult("Error updating warehouse", dbEx.Message));
            }
            catch
            {
                // Rollback transaction nếu có lỗi
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            return Ok(ApiResponse<WarehouseDto>.SuccessResult(_mapper.Map<WarehouseDto>(warehouse)));
        }

        /// <summary>
        /// ✅ Phase 4.1 - Advanced Features: Bulk update Warehouses
        /// </summary>
        [HttpPost("bulk-update")]
        public async Task<ActionResult<ApiResponse<BulkOperationResultDto>>> BulkUpdateWarehouses([FromBody] BulkUpdateWarehousesDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<BulkOperationResultDto>.ErrorResult("Invalid data"));

                if (dto.WarehouseIds == null || !dto.WarehouseIds.Any())
                    return BadRequest(ApiResponse<BulkOperationResultDto>.ErrorResult("Phải chọn ít nhất 1 kho"));

                var result = new BulkOperationResultDto();

                // Load warehouses cần update (query hiệu quả)
                var warehouseIds = dto.WarehouseIds.Distinct().ToList();
                var warehouses = await _context.Warehouses
                    .Where(w => !w.IsDeleted && warehouseIds.Contains(w.Id))
                    .ToListAsync();

                if (!warehouses.Any())
                    return BadRequest(ApiResponse<BulkOperationResultDto>.ErrorResult("Không tìm thấy kho nào"));

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // ✅ FIX: Optimize - Query các warehouse cần unset IsDefault 1 lần trước loop
                    // Chỉ query nếu có warehouse nào được set IsDefault = true
                    var warehousesToUnsetDefault = new List<Core.Entities.Warehouse>();
                    if (dto.IsDefault.HasValue && dto.IsDefault.Value)
                    {
                        warehousesToUnsetDefault = await _context.Warehouses
                            .Where(w => !w.IsDeleted && !warehouseIds.Contains(w.Id) && w.IsDefault)
                            .ToListAsync();
                    }

                    // ✅ FIX: Nếu set IsDefault = true cho nhiều warehouses, chỉ set cho warehouse đầu tiên
                    // và unset các warehouse khác trong danh sách
                    var isFirstDefaultWarehouse = true;
                    
                    foreach (var warehouse in warehouses)
                    {
                        try
                        {
                            // Update các fields nếu có giá trị
                            if (dto.IsActive.HasValue)
                                warehouse.IsActive = dto.IsActive.Value;

                            if (dto.IsDefault.HasValue)
                            {
                                // Nếu set IsDefault = true, unset các warehouse khác (chỉ 1 lần)
                                if (dto.IsDefault.Value && isFirstDefaultWarehouse)
                                {
                                    // Unset các warehouse khác ngoài danh sách
                                    foreach (var other in warehousesToUnsetDefault)
                                    {
                                        other.IsDefault = false;
                                        await _unitOfWork.Warehouses.UpdateAsync(other);
                                    }
                                    
                                    // Unset các warehouse khác trong danh sách (trừ warehouse đầu tiên)
                                    foreach (var other in warehouses.Skip(1))
                                    {
                                        other.IsDefault = false;
                                    }
                                    
                                    isFirstDefaultWarehouse = false;
                                }
                                
                                // Chỉ set IsDefault = true cho warehouse đầu tiên nếu có nhiều warehouses
                                warehouse.IsDefault = dto.IsDefault.Value && (warehouses.Count == 1 || warehouse == warehouses.First());
                            }

                            await _unitOfWork.Warehouses.UpdateAsync(warehouse);
                            result.SuccessIds.Add(warehouse.Id);
                            result.SuccessCount++;
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"Kho {warehouse.Code}: {ex.Message}");
                            result.FailedIds.Add(warehouse.Id);
                            result.FailureCount++;
                        }
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    return Ok(ApiResponse<BulkOperationResultDto>.SuccessResult(result, 
                        $"Đã cập nhật {result.SuccessCount} kho thành công" + 
                        (result.FailureCount > 0 ? $", {result.FailureCount} kho thất bại" : "")));
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<BulkOperationResultDto>.ErrorResult("Error updating warehouses", ex.Message));
            }
        }

        /// <summary>
        /// ✅ Phase 4.1 - Advanced Features: Bulk update Warehouse Zones
        /// </summary>
        [HttpPost("zones/bulk-update")]
        public async Task<ActionResult<ApiResponse<BulkOperationResultDto>>> BulkUpdateWarehouseZones([FromBody] BulkUpdateWarehouseZonesDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<BulkOperationResultDto>.ErrorResult("Invalid data"));

                if (dto.ZoneIds == null || !dto.ZoneIds.Any())
                    return BadRequest(ApiResponse<BulkOperationResultDto>.ErrorResult("Phải chọn ít nhất 1 khu vực"));

                var result = new BulkOperationResultDto();

                // Load zones cần update (query hiệu quả)
                var zoneIds = dto.ZoneIds.Distinct().ToList();
                var zones = await _context.WarehouseZones
                    .Where(z => !z.IsDeleted && zoneIds.Contains(z.Id))
                    .ToListAsync();

                if (!zones.Any())
                    return BadRequest(ApiResponse<BulkOperationResultDto>.ErrorResult("Không tìm thấy khu vực nào"));

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    foreach (var zone in zones)
                    {
                        try
                        {
                            // Update các fields nếu có giá trị
                            if (dto.IsActive.HasValue)
                                zone.IsActive = dto.IsActive.Value;

                            await _unitOfWork.WarehouseZones.UpdateAsync(zone);
                            result.SuccessIds.Add(zone.Id);
                            result.SuccessCount++;
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"Khu vực {zone.Code}: {ex.Message}");
                            result.FailedIds.Add(zone.Id);
                            result.FailureCount++;
                        }
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    return Ok(ApiResponse<BulkOperationResultDto>.SuccessResult(result, 
                        $"Đã cập nhật {result.SuccessCount} khu vực thành công" + 
                        (result.FailureCount > 0 ? $", {result.FailureCount} khu vực thất bại" : "")));
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<BulkOperationResultDto>.ErrorResult("Error updating zones", ex.Message));
            }
        }

        /// <summary>
        /// ✅ Phase 4.1 - Advanced Features: Bulk update Warehouse Bins
        /// </summary>
        [HttpPost("bins/bulk-update")]
        public async Task<ActionResult<ApiResponse<BulkOperationResultDto>>> BulkUpdateWarehouseBins([FromBody] BulkUpdateWarehouseBinsDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<BulkOperationResultDto>.ErrorResult("Invalid data"));

                if (dto.BinIds == null || !dto.BinIds.Any())
                    return BadRequest(ApiResponse<BulkOperationResultDto>.ErrorResult("Phải chọn ít nhất 1 kệ"));

                var result = new BulkOperationResultDto();

                // Load bins cần update (query hiệu quả)
                var binIds = dto.BinIds.Distinct().ToList();
                var bins = await _context.WarehouseBins
                    .Where(b => !b.IsDeleted && binIds.Contains(b.Id))
                    .ToListAsync();

                if (!bins.Any())
                    return BadRequest(ApiResponse<BulkOperationResultDto>.ErrorResult("Không tìm thấy kệ nào"));

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // ✅ FIX: Batch load các bins cần unset IsDefault để tránh N+1 query
                    var binsToUnsetDefault = new Dictionary<(int WarehouseId, int? WarehouseZoneId), List<Core.Entities.WarehouseBin>>();
                    if (dto.IsDefault.HasValue && dto.IsDefault.Value)
                    {
                        // Group bins by (WarehouseId, ZoneId) để query hiệu quả
                        var binGroups = bins.GroupBy(b => (b.WarehouseId, b.WarehouseZoneId)).ToList();
                        foreach (var group in binGroups)
                        {
                            var otherBins = await _context.WarehouseBins
                                .Where(b => !b.IsDeleted && 
                                    b.WarehouseId == group.Key.WarehouseId &&
                                    (group.Key.WarehouseZoneId == null ? b.WarehouseZoneId == null : b.WarehouseZoneId == group.Key.WarehouseZoneId) &&
                                    !binIds.Contains(b.Id) &&
                                    b.IsDefault)
                                .ToListAsync();
                            
                            if (otherBins.Any())
                            {
                                binsToUnsetDefault[group.Key] = otherBins;
                            }
                        }
                    }

                    foreach (var bin in bins)
                    {
                        try
                        {
                            // Update các fields nếu có giá trị
                            if (dto.IsActive.HasValue)
                                bin.IsActive = dto.IsActive.Value;

                            if (dto.IsDefault.HasValue)
                            {
                                // Nếu set IsDefault = true, unset các bin khác trong cùng warehouse/zone
                                if (dto.IsDefault.Value)
                                {
                                    var key = (bin.WarehouseId, bin.WarehouseZoneId);
                                    if (binsToUnsetDefault.TryGetValue(key, out var otherBins))
                                    {
                                        foreach (var other in otherBins)
                                        {
                                            other.IsDefault = false;
                                            await _unitOfWork.WarehouseBins.UpdateAsync(other);
                                        }
                                    }
                                    
                                    // Unset các bin khác trong danh sách (trừ bin đầu tiên trong group)
                                    var sameGroupBins = bins.Where(b => 
                                        b.WarehouseId == bin.WarehouseId &&
                                        (bin.WarehouseZoneId == null ? b.WarehouseZoneId == null : b.WarehouseZoneId == bin.WarehouseZoneId) &&
                                        b.Id != bin.Id).ToList();
                                    
                                    foreach (var other in sameGroupBins)
                                    {
                                        other.IsDefault = false;
                                    }
                                }
                                
                                // Chỉ set IsDefault = true cho bin đầu tiên trong group nếu có nhiều bins
                                var sameGroupBinsList = bins.Where(b => 
                                    b.WarehouseId == bin.WarehouseId &&
                                    (bin.WarehouseZoneId == null ? b.WarehouseZoneId == null : b.WarehouseZoneId == bin.WarehouseZoneId))
                                    .ToList();
                                
                                var isFirstInGroup = sameGroupBinsList.Any() && sameGroupBinsList.First() == bin;
                                
                                bin.IsDefault = dto.IsDefault.Value && isFirstInGroup;
                            }

                            await _unitOfWork.WarehouseBins.UpdateAsync(bin);
                            result.SuccessIds.Add(bin.Id);
                            result.SuccessCount++;
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"Kệ {bin.Code}: {ex.Message}");
                            result.FailedIds.Add(bin.Id);
                            result.FailureCount++;
                        }
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    return Ok(ApiResponse<BulkOperationResultDto>.SuccessResult(result, 
                        $"Đã cập nhật {result.SuccessCount} kệ thành công" + 
                        (result.FailureCount > 0 ? $", {result.FailureCount} kệ thất bại" : "")));
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<BulkOperationResultDto>.ErrorResult("Error updating bins", ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteWarehouse(int id)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id);
            if (warehouse == null)
            {
                return NotFound(ApiResponse.ErrorResult("Warehouse not found"));
            }

            await _unitOfWork.Warehouses.DeleteAsync(warehouse);
            await _unitOfWork.SaveChangesAsync();
            return Ok(ApiResponse.SuccessResult("Warehouse deleted"));
        }

        [HttpPost("{warehouseId}/zones")]
        public async Task<ActionResult<ApiResponse<WarehouseZoneDto>>> CreateZone(int warehouseId, WarehouseZoneRequestDto dto)
        {
            var warehouse = await _context.Warehouses
                .Where(w => !w.IsDeleted && w.Id == warehouseId)
                .FirstOrDefaultAsync();
            if (warehouse == null)
            {
                return NotFound(ApiResponse<WarehouseZoneDto>.ErrorResult("Warehouse not found"));
            }

            // ✅ Normalize Code TRƯỚC khi validate
            var normalizedCode = !string.IsNullOrWhiteSpace(dto.Code) ? dto.Code.Trim() : string.Empty;

            // ✅ Validate Code không được để trống
            if (string.IsNullOrEmpty(normalizedCode))
            {
                return BadRequest(ApiResponse<WarehouseZoneDto>.ErrorResult("Mã khu vực không được để trống"));
            }

            // ✅ Validate Code unique trong warehouse (Code phải unique trong mỗi warehouse)
            var existingZone = await _context.WarehouseZones
                .Where(z => !z.IsDeleted && z.WarehouseId == warehouseId && z.Code == normalizedCode)
                .FirstOrDefaultAsync();
            if (existingZone != null)
            {
                return BadRequest(ApiResponse<WarehouseZoneDto>.ErrorResult($"Mã khu vực '{normalizedCode}' đã tồn tại trong kho này"));
            }

            // ✅ SỬA: Kiểm tra xem có zone đã bị soft delete với cùng Code trong cùng warehouse không
            // Nếu có, restore và cập nhật thông tin thay vì tạo mới
            var deletedZone = await _context.WarehouseZones
                .IgnoreQueryFilters() // Bỏ qua filter IsDeleted để tìm cả zone đã bị soft delete
                .Where(z => z.IsDeleted && z.WarehouseId == warehouseId && z.Code == normalizedCode)
                .FirstOrDefaultAsync();

            WarehouseZone zone;
            
            if (deletedZone != null)
            {
                // ✅ Restore zone đã bị soft delete
                deletedZone.IsDeleted = false;
                deletedZone.DeletedAt = null;
                deletedZone.DeletedBy = null;
                
                // ✅ Cập nhật thông tin với dữ liệu mới
                deletedZone.Code = normalizedCode;
                deletedZone.Name = dto.Name?.Trim() ?? string.Empty;
                deletedZone.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
                deletedZone.DisplayOrder = dto.DisplayOrder;
                deletedZone.IsActive = dto.IsActive;
                
                zone = deletedZone;
            }
            else
            {
                // ✅ Tạo zone mới
                zone = new WarehouseZone
                {
                    WarehouseId = warehouseId,
                    Code = normalizedCode,
                    Name = dto.Name?.Trim() ?? string.Empty,
                    Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                    DisplayOrder = dto.DisplayOrder,
                    IsActive = dto.IsActive
                };
            }

            // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (deletedZone != null)
                {
                    // ✅ Update zone đã restore
                    await _unitOfWork.WarehouseZones.UpdateAsync(zone);
                }
                else
                {
                    // ✅ Add zone mới
                    await _unitOfWork.WarehouseZones.AddAsync(zone);
                }
                
                await _unitOfWork.SaveChangesAsync();

                // Commit transaction nếu thành công
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (DbUpdateException dbEx)
            {
                // Rollback transaction nếu có lỗi
                await _unitOfWork.RollbackTransactionAsync();
                
                // ✅ Handle database unique constraint violation
                if (dbEx.InnerException != null && dbEx.InnerException.Message.Contains("Duplicate entry"))
                {
                    if (dbEx.InnerException.Message.Contains("Code"))
                    {
                        return BadRequest(ApiResponse<WarehouseZoneDto>.ErrorResult($"Mã khu vực '{normalizedCode}' đã tồn tại trong kho này"));
                    }
                }
                return StatusCode(500, ApiResponse<WarehouseZoneDto>.ErrorResult("Error creating zone", dbEx.Message));
            }
            catch
            {
                // Rollback transaction nếu có lỗi
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            // ✅ SỬA: Detach entity cũ để tránh tracking conflict
            _context.Entry(zone).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            
            // ✅ SỬA: Reload zone từ database với Include bins để đảm bảo có đầy đủ dữ liệu
            var savedZone = await _context.WarehouseZones
                .AsNoTracking() // ✅ QUAN TRỌNG: Không track entity để tránh conflict
                .Where(z => z.Id == zone.Id && !z.IsDeleted && z.WarehouseId == warehouseId)
                .Include(z => z.Bins.Where(b => !b.IsDeleted))
                .FirstOrDefaultAsync();

            if (savedZone == null)
            {
                return StatusCode(500, ApiResponse<WarehouseZoneDto>.ErrorResult("Không thể tải lại thông tin khu vực sau khi tạo"));
            }

            return CreatedAtAction(nameof(GetWarehouse), new { id = warehouseId }, ApiResponse<WarehouseZoneDto>.SuccessResult(_mapper.Map<WarehouseZoneDto>(savedZone)));
        }

        [HttpPut("{warehouseId}/zones/{zoneId}")]
        public async Task<ActionResult<ApiResponse<WarehouseZoneDto>>> UpdateZone(int warehouseId, int zoneId, WarehouseZoneRequestDto dto)
        {
            var zone = await _context.WarehouseZones
                .Where(z => !z.IsDeleted && z.Id == zoneId && z.WarehouseId == warehouseId)
                .FirstOrDefaultAsync();
            if (zone == null)
            {
                return NotFound(ApiResponse<WarehouseZoneDto>.ErrorResult("Zone not found"));
            }

            // ✅ Normalize Code TRƯỚC khi validate
            var normalizedCode = !string.IsNullOrWhiteSpace(dto.Code) ? dto.Code.Trim() : string.Empty;

            // ✅ Validate Code không được để trống
            if (string.IsNullOrEmpty(normalizedCode))
            {
                return BadRequest(ApiResponse<WarehouseZoneDto>.ErrorResult("Mã khu vực không được để trống"));
            }

            // ✅ Validate Code unique trong warehouse (chỉ check nếu Code thay đổi)
            if (zone.Code != normalizedCode)
            {
                var existingZone = await _context.WarehouseZones
                    .Where(z => !z.IsDeleted && z.WarehouseId == warehouseId && z.Code == normalizedCode && z.Id != zoneId)
                    .FirstOrDefaultAsync();
                if (existingZone != null)
                {
                    return BadRequest(ApiResponse<WarehouseZoneDto>.ErrorResult($"Mã khu vực '{normalizedCode}' đã tồn tại trong kho này"));
                }
            }

            zone.Code = normalizedCode;
            zone.Name = dto.Name?.Trim() ?? string.Empty;
            zone.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
            zone.DisplayOrder = dto.DisplayOrder;
            zone.IsActive = dto.IsActive;

            // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                await _unitOfWork.WarehouseZones.UpdateAsync(zone);
                await _unitOfWork.SaveChangesAsync();

                // Commit transaction nếu thành công
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (DbUpdateException dbEx)
            {
                // Rollback transaction nếu có lỗi
                await _unitOfWork.RollbackTransactionAsync();
                
                // ✅ Handle database unique constraint violation
                if (dbEx.InnerException != null && dbEx.InnerException.Message.Contains("Duplicate entry"))
                {
                    if (dbEx.InnerException.Message.Contains("Code"))
                    {
                        return BadRequest(ApiResponse<WarehouseZoneDto>.ErrorResult($"Mã khu vực '{normalizedCode}' đã tồn tại trong kho này"));
                    }
                }
                return StatusCode(500, ApiResponse<WarehouseZoneDto>.ErrorResult("Error updating zone", dbEx.Message));
            }
            catch
            {
                // Rollback transaction nếu có lỗi
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            // ✅ SỬA: Detach entity cũ để tránh tracking conflict
            _context.Entry(zone).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            
            // ✅ SỬA: Reload zone từ database với Include bins để đảm bảo có đầy đủ dữ liệu
            var savedZone = await _context.WarehouseZones
                .AsNoTracking() // ✅ QUAN TRỌNG: Không track entity để tránh conflict
                .Where(z => z.Id == zoneId && !z.IsDeleted && z.WarehouseId == warehouseId)
                .Include(z => z.Bins.Where(b => !b.IsDeleted))
                .FirstOrDefaultAsync();

            if (savedZone == null)
            {
                return StatusCode(500, ApiResponse<WarehouseZoneDto>.ErrorResult("Không thể tải lại thông tin khu vực sau khi cập nhật"));
            }

            return Ok(ApiResponse<WarehouseZoneDto>.SuccessResult(_mapper.Map<WarehouseZoneDto>(savedZone)));
        }

        [HttpDelete("{warehouseId}/zones/{zoneId}")]
        public async Task<ActionResult<ApiResponse>> DeleteZone(int warehouseId, int zoneId)
        {
            var zone = await _unitOfWork.WarehouseZones.GetByIdAsync(zoneId);
            if (zone == null || zone.WarehouseId != warehouseId)
            {
                return NotFound(ApiResponse.ErrorResult("Zone not found"));
            }

            await _unitOfWork.WarehouseZones.DeleteAsync(zone);
            await _unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse.SuccessResult("Zone deleted"));
        }

        [HttpPost("{warehouseId}/bins")]
        public async Task<ActionResult<ApiResponse<WarehouseBinDto>>> CreateBin(int warehouseId, WarehouseBinRequestDto dto)
        {
            var warehouse = await _context.Warehouses
                .Where(w => !w.IsDeleted && w.Id == warehouseId)
                .FirstOrDefaultAsync();
            if (warehouse == null)
            {
                return NotFound(ApiResponse<WarehouseBinDto>.ErrorResult("Warehouse not found"));
            }

            if (dto.WarehouseZoneId.HasValue)
            {
                var zone = await _context.WarehouseZones
                    .Where(z => !z.IsDeleted && z.Id == dto.WarehouseZoneId.Value && z.WarehouseId == warehouseId)
                    .FirstOrDefaultAsync();
                if (zone == null)
                {
                    return BadRequest(ApiResponse<WarehouseBinDto>.ErrorResult("Zone does not belong to warehouse"));
                }
            }

            // ✅ Normalize Code TRƯỚC khi validate
            var normalizedCode = !string.IsNullOrWhiteSpace(dto.Code) ? dto.Code.Trim() : string.Empty;

            // ✅ Validate Code không được để trống
            if (string.IsNullOrEmpty(normalizedCode))
            {
                return BadRequest(ApiResponse<WarehouseBinDto>.ErrorResult("Mã kệ/ngăn không được để trống"));
            }

            // ✅ Validate Code unique trong warehouse (Code phải unique trong mỗi warehouse)
            var existingBin = await _context.WarehouseBins
                .Where(b => !b.IsDeleted && b.WarehouseId == warehouseId && b.Code == normalizedCode)
                .FirstOrDefaultAsync();
            if (existingBin != null)
            {
                return BadRequest(ApiResponse<WarehouseBinDto>.ErrorResult($"Mã kệ/ngăn '{normalizedCode}' đã tồn tại trong kho này"));
            }

            // ✅ SỬA: Kiểm tra xem có bin đã bị soft delete với cùng Code trong cùng warehouse không
            // Nếu có, restore và cập nhật thông tin thay vì tạo mới
            var deletedBin = await _context.WarehouseBins
                .IgnoreQueryFilters() // Bỏ qua filter IsDeleted để tìm cả bin đã bị soft delete
                .Where(b => b.IsDeleted && b.WarehouseId == warehouseId && b.Code == normalizedCode)
                .FirstOrDefaultAsync();

            WarehouseBin bin;
            
            if (deletedBin != null)
            {
                // ✅ Restore bin đã bị soft delete
                deletedBin.IsDeleted = false;
                deletedBin.DeletedAt = null;
                deletedBin.DeletedBy = null;
                
                // ✅ Cập nhật thông tin với dữ liệu mới
                deletedBin.WarehouseZoneId = dto.WarehouseZoneId;
                deletedBin.Code = normalizedCode;
                deletedBin.Name = dto.Name?.Trim() ?? string.Empty;
                deletedBin.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
                deletedBin.Capacity = dto.Capacity;
                deletedBin.IsDefault = dto.IsDefault;
                deletedBin.IsActive = dto.IsActive;
                
                bin = deletedBin;
            }
            else
            {
                // ✅ Tạo bin mới
                bin = new WarehouseBin
                {
                    WarehouseId = warehouseId,
                    WarehouseZoneId = dto.WarehouseZoneId,
                    Code = normalizedCode,
                    Name = dto.Name?.Trim() ?? string.Empty,
                    Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                    Capacity = dto.Capacity,
                    IsDefault = dto.IsDefault,
                    IsActive = dto.IsActive
                };
            }

            // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (deletedBin != null)
                {
                    // ✅ Update bin đã restore
                    await _unitOfWork.WarehouseBins.UpdateAsync(bin);
                }
                else
                {
                    // ✅ Add bin mới
                    await _unitOfWork.WarehouseBins.AddAsync(bin);
                }
                
                await _unitOfWork.SaveChangesAsync();

                // Commit transaction nếu thành công
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (DbUpdateException dbEx)
            {
                // Rollback transaction nếu có lỗi
                await _unitOfWork.RollbackTransactionAsync();
                
                // ✅ Handle database unique constraint violation
                if (dbEx.InnerException != null && dbEx.InnerException.Message.Contains("Duplicate entry"))
                {
                    if (dbEx.InnerException.Message.Contains("Code"))
                    {
                        return BadRequest(ApiResponse<WarehouseBinDto>.ErrorResult($"Mã kệ/ngăn '{normalizedCode}' đã tồn tại trong kho này"));
                    }
                }
                return StatusCode(500, ApiResponse<WarehouseBinDto>.ErrorResult("Error creating bin", dbEx.Message));
            }
            catch
            {
                // Rollback transaction nếu có lỗi
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            return CreatedAtAction(nameof(GetWarehouse), new { id = warehouseId }, ApiResponse<WarehouseBinDto>.SuccessResult(_mapper.Map<WarehouseBinDto>(bin)));
        }

        [HttpPut("{warehouseId}/bins/{binId}")]
        public async Task<ActionResult<ApiResponse<WarehouseBinDto>>> UpdateBin(int warehouseId, int binId, WarehouseBinRequestDto dto)
        {
            var bin = await _context.WarehouseBins
                .Where(b => !b.IsDeleted && b.Id == binId && b.WarehouseId == warehouseId)
                .FirstOrDefaultAsync();
            if (bin == null)
            {
                return NotFound(ApiResponse<WarehouseBinDto>.ErrorResult("Bin not found"));
            }

            if (dto.WarehouseZoneId.HasValue)
            {
                var zone = await _context.WarehouseZones
                    .Where(z => !z.IsDeleted && z.Id == dto.WarehouseZoneId.Value && z.WarehouseId == warehouseId)
                    .FirstOrDefaultAsync();
                if (zone == null)
                {
                    return BadRequest(ApiResponse<WarehouseBinDto>.ErrorResult("Zone does not belong to warehouse"));
                }
            }

            // ✅ Normalize Code TRƯỚC khi validate
            var normalizedCode = !string.IsNullOrWhiteSpace(dto.Code) ? dto.Code.Trim() : string.Empty;

            // ✅ Validate Code không được để trống
            if (string.IsNullOrEmpty(normalizedCode))
            {
                return BadRequest(ApiResponse<WarehouseBinDto>.ErrorResult("Mã kệ/ngăn không được để trống"));
            }

            // ✅ Validate Code unique trong warehouse (chỉ check nếu Code thay đổi)
            if (bin.Code != normalizedCode)
            {
                var existingBin = await _context.WarehouseBins
                    .Where(b => !b.IsDeleted && b.WarehouseId == warehouseId && b.Code == normalizedCode && b.Id != binId)
                    .FirstOrDefaultAsync();
                if (existingBin != null)
                {
                    return BadRequest(ApiResponse<WarehouseBinDto>.ErrorResult($"Mã kệ/ngăn '{normalizedCode}' đã tồn tại trong kho này"));
                }
            }

            bin.WarehouseZoneId = dto.WarehouseZoneId;
            bin.Code = normalizedCode;
            bin.Name = dto.Name?.Trim() ?? string.Empty;
            bin.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
            bin.Capacity = dto.Capacity;
            bin.IsDefault = dto.IsDefault;
            bin.IsActive = dto.IsActive;

            // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                await _unitOfWork.WarehouseBins.UpdateAsync(bin);
                await _unitOfWork.SaveChangesAsync();

                // Commit transaction nếu thành công
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (DbUpdateException dbEx)
            {
                // Rollback transaction nếu có lỗi
                await _unitOfWork.RollbackTransactionAsync();
                
                // ✅ Handle database unique constraint violation
                if (dbEx.InnerException != null && dbEx.InnerException.Message.Contains("Duplicate entry"))
                {
                    if (dbEx.InnerException.Message.Contains("Code"))
                    {
                        return BadRequest(ApiResponse<WarehouseBinDto>.ErrorResult($"Mã kệ/ngăn '{normalizedCode}' đã tồn tại trong kho này"));
                    }
                }
                return StatusCode(500, ApiResponse<WarehouseBinDto>.ErrorResult("Error updating bin", dbEx.Message));
            }
            catch
            {
                // Rollback transaction nếu có lỗi
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            return Ok(ApiResponse<WarehouseBinDto>.SuccessResult(_mapper.Map<WarehouseBinDto>(bin)));
        }

        [HttpDelete("{warehouseId}/bins/{binId}")]
        public async Task<ActionResult<ApiResponse>> DeleteBin(int warehouseId, int binId)
        {
            var bin = await _unitOfWork.WarehouseBins.GetByIdAsync(binId);
            if (bin == null || bin.WarehouseId != warehouseId)
            {
                return NotFound(ApiResponse.ErrorResult("Bin not found"));
            }

            await _unitOfWork.WarehouseBins.DeleteAsync(bin);
            await _unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse.SuccessResult("Bin deleted"));
        }
    }
}

