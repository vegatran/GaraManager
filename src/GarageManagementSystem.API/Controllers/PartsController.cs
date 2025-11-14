using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Core.Extensions;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using GarageManagementSystem.API.Services;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiScope")]
    public class PartsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly GarageDbContext _context;

        public PartsController(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService, GarageDbContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<PartDto>>> GetParts(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? category = null)
        {
            try
            {
                // ✅ OPTIMIZED: Query ở database level thay vì load tất cả vào memory
                var query = _context.Parts
                    .Include(p => p.PartUnits.Where(u => !u.IsDeleted))
                    .Where(p => !p.IsDeleted)
                    .AsQueryable();
                
                // Apply search filter if provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(p => 
                        (p.PartName != null && p.PartName.Contains(searchTerm)) || 
                        (p.PartNumber != null && p.PartNumber.Contains(searchTerm)) || 
                        (p.Description != null && p.Description.Contains(searchTerm)));
                }
                
                // Apply category filter if provided
                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(p => p.Category == category);
                }

                // ✅ OPTIMIZED: Get total count ở database level (trước khi paginate)
                var totalCount = await query.CountAsync();
                
                // ✅ OPTIMIZED: Apply pagination ở database level với Skip/Take
                var pagedParts = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                
                var partDtos = pagedParts.Select(MapToDto).ToList();
                
                return Ok(PagedResponse<PartDto>.CreateSuccessResult(
                    partDtos, pageNumber, pageSize, totalCount, "Parts retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, PagedResponse<PartDto>.CreateErrorResult("Error retrieving parts"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PartDto>>> GetPart(int id)
        {
            try
            {
                var part = await _unitOfWork.Parts.GetWithDetailsAsync(id);
                if (part == null) return NotFound(ApiResponse<PartDto>.ErrorResult("Part not found"));
                return Ok(ApiResponse<PartDto>.SuccessResult(MapToDto(part)));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PartDto>.ErrorResult("Error retrieving part", ex.Message));
            }
        }

        [HttpGet("low-stock")]
        public async Task<ActionResult<ApiResponse<List<PartDto>>>> GetLowStockParts()
        {
            try
            {
                var parts = await _unitOfWork.Parts.GetLowStockPartsAsync();
                return Ok(ApiResponse<List<PartDto>>.SuccessResult(parts.Select(MapToDto).ToList()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<PartDto>>.ErrorResult("Error", ex.Message));
            }
        }

        [HttpGet("category/{category}")]
        public async Task<ActionResult<ApiResponse<List<PartDto>>>> GetByCategory(string category)
        {
            try
            {
                var parts = await _unitOfWork.Parts.GetByCategoryAsync(category);
                return Ok(ApiResponse<List<PartDto>>.SuccessResult(parts.Select(MapToDto).ToList()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<PartDto>>.ErrorResult("Error", ex.Message));
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<List<PartDto>>>> SearchParts([FromQuery] string searchTerm)
        {
            try
            {
                var parts = await _unitOfWork.Parts.SearchPartsAsync(searchTerm);
                return Ok(ApiResponse<List<PartDto>>.SuccessResult(parts.Select(MapToDto).ToList()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<PartDto>>.ErrorResult("Error", ex.Message));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<PartDto>>> CreatePart(CreatePartDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<PartDto>.ErrorResult("Invalid data"));

                // ✅ SỬA: Normalize SKU và Barcode TRƯỚC khi validate để đảm bảo consistency
                var normalizedSku = !string.IsNullOrWhiteSpace(dto.Sku) ? dto.Sku.Trim() : null;
                var normalizedBarcode = !string.IsNullOrWhiteSpace(dto.Barcode) ? dto.Barcode.Trim() : null;

                // ✅ THÊM: Validate SKU unique (chỉ check nếu không null/empty)
                if (!string.IsNullOrEmpty(normalizedSku))
                {
                    var existingSku = await _context.Parts
                        .Where(p => !p.IsDeleted && p.Sku == normalizedSku)
                        .FirstOrDefaultAsync();
                    if (existingSku != null)
                    {
                        return BadRequest(ApiResponse<PartDto>.ErrorResult($"SKU '{normalizedSku}' đã tồn tại trong hệ thống"));
                    }
                }

                // ✅ THÊM: Validate Barcode unique (chỉ check nếu không null/empty)
                if (!string.IsNullOrEmpty(normalizedBarcode))
                {
                    var existingBarcode = await _context.Parts
                        .Where(p => !p.IsDeleted && p.Barcode == normalizedBarcode)
                        .FirstOrDefaultAsync();
                    if (existingBarcode != null)
                    {
                        return BadRequest(ApiResponse<PartDto>.ErrorResult($"Mã vạch '{normalizedBarcode}' đã tồn tại trong hệ thống"));
                    }
                }

                var part = _mapper.Map<Core.Entities.Part>(dto);
                
                // ✅ Update part với normalized values (đã normalize ở trên)
                part.Sku = normalizedSku;
                part.Barcode = normalizedBarcode;

                // ✅ SỬA: Đảm bảo logic đồng bộ: EnsureDefaultUnit được gọi SAU ApplyPartUnits
                // ApplyPartUnits chỉ xử lý collection, EnsureDefaultUnit xử lý đồng bộ DefaultUnit ↔ PartUnits
                ApplyPartUnits(part, dto.Units);
                EnsureDefaultUnit(part, dto.DefaultUnit, dto.Units);

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.Parts.AddAsync(part);
                    await _unitOfWork.SaveChangesAsync();

                    // Commit transaction nếu thành công
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    // Rollback transaction nếu có lỗi
                    await _unitOfWork.RollbackTransactionAsync();
                    
                    // ✅ THÊM: Handle database unique constraint violation
                    if (dbEx.InnerException != null && dbEx.InnerException.Message.Contains("Duplicate entry"))
                    {
                        if (dbEx.InnerException.Message.Contains("Sku"))
                        {
                            return BadRequest(ApiResponse<PartDto>.ErrorResult($"SKU '{normalizedSku}' đã tồn tại trong hệ thống"));
                        }
                        if (dbEx.InnerException.Message.Contains("Barcode"))
                        {
                            return BadRequest(ApiResponse<PartDto>.ErrorResult($"Mã vạch '{normalizedBarcode}' đã tồn tại trong hệ thống"));
                        }
                    }
                    return StatusCode(500, ApiResponse<PartDto>.ErrorResult("Error creating part", dbEx.Message));
                }
                catch
                {
                    // Rollback transaction nếu có lỗi
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
                return CreatedAtAction(nameof(GetPart), new { id = part.Id }, ApiResponse<PartDto>.SuccessResult(MapToDto(part)));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PartDto>.ErrorResult("Error creating part", ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<PartDto>>> UpdatePart(int id, UpdatePartDto dto)
        {
            try
            {
                if (id != dto.Id) return BadRequest(ApiResponse<PartDto>.ErrorResult("ID mismatch"));
                var part = await _unitOfWork.Parts.GetWithDetailsAsync(id);
                if (part == null) return NotFound(ApiResponse<PartDto>.ErrorResult("Part not found"));

                // ✅ SỬA: Normalize SKU và Barcode TRƯỚC khi validate để đảm bảo consistency
                var normalizedSku = !string.IsNullOrWhiteSpace(dto.Sku) ? dto.Sku.Trim() : null;
                var normalizedBarcode = !string.IsNullOrWhiteSpace(dto.Barcode) ? dto.Barcode.Trim() : null;

                // ✅ THÊM: Validate SKU unique (chỉ check nếu không null/empty và khác với giá trị hiện tại)
                if (!string.IsNullOrEmpty(normalizedSku))
                {
                    // So sánh với giá trị hiện tại (đã normalize từ database)
                    var currentSku = part.Sku != null ? part.Sku.Trim() : null;
                    if (currentSku != normalizedSku)
                    {
                        var existingSku = await _context.Parts
                            .Where(p => !p.IsDeleted && p.Id != id && p.Sku == normalizedSku)
                            .FirstOrDefaultAsync();
                        if (existingSku != null)
                        {
                            return BadRequest(ApiResponse<PartDto>.ErrorResult($"SKU '{normalizedSku}' đã tồn tại trong hệ thống"));
                        }
                    }
                }

                // ✅ THÊM: Validate Barcode unique (chỉ check nếu không null/empty và khác với giá trị hiện tại)
                if (!string.IsNullOrEmpty(normalizedBarcode))
                {
                    // So sánh với giá trị hiện tại (đã normalize từ database)
                    var currentBarcode = part.Barcode != null ? part.Barcode.Trim() : null;
                    if (currentBarcode != normalizedBarcode)
                    {
                        var existingBarcode = await _context.Parts
                            .Where(p => !p.IsDeleted && p.Id != id && p.Barcode == normalizedBarcode)
                            .FirstOrDefaultAsync();
                        if (existingBarcode != null)
                        {
                            return BadRequest(ApiResponse<PartDto>.ErrorResult($"Mã vạch '{normalizedBarcode}' đã tồn tại trong hệ thống"));
                        }
                    }
                }

                // ✅ SỬA: Dùng AutoMapper thay vì map tay
                _mapper.Map(dto, part);
                
                // ✅ Update part với normalized values (đã normalize ở trên)
                part.Sku = normalizedSku;
                part.Barcode = normalizedBarcode;

                ApplyPartUnits(part, dto.Units);
                EnsureDefaultUnit(part, dto.DefaultUnit, dto.Units);

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.Parts.UpdateAsync(part);
                    await _unitOfWork.SaveChangesAsync();

                    // Commit transaction nếu thành công
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    // Rollback transaction nếu có lỗi
                    await _unitOfWork.RollbackTransactionAsync();
                    
                    // ✅ THÊM: Handle database unique constraint violation
                    if (dbEx.InnerException != null && dbEx.InnerException.Message.Contains("Duplicate entry"))
                    {
                        if (dbEx.InnerException.Message.Contains("Sku"))
                        {
                            return BadRequest(ApiResponse<PartDto>.ErrorResult($"SKU '{normalizedSku}' đã tồn tại trong hệ thống"));
                        }
                        if (dbEx.InnerException.Message.Contains("Barcode"))
                        {
                            return BadRequest(ApiResponse<PartDto>.ErrorResult($"Mã vạch '{normalizedBarcode}' đã tồn tại trong hệ thống"));
                        }
                    }
                    return StatusCode(500, ApiResponse<PartDto>.ErrorResult("Error updating part", dbEx.Message));
                }
                catch
                {
                    // Rollback transaction nếu có lỗi
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
                return Ok(ApiResponse<PartDto>.SuccessResult(MapToDto(part)));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PartDto>.ErrorResult("Error updating part", ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeletePart(int id)
        {
            try
            {
                var part = await _unitOfWork.Parts.GetByIdAsync(id);
                if (part == null) return NotFound(ApiResponse.ErrorResult("Part not found"));
                await _unitOfWork.Parts.DeleteAsync(part);
                await _unitOfWork.SaveChangesAsync();
                return Ok(ApiResponse.SuccessResult("Part deleted"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResult("Error", ex.Message));
            }
        }

        private PartDto MapToDto(Core.Entities.Part part)
        {
            return _mapper.Map<PartDto>(part);
        }

        private void ApplyPartUnits(Core.Entities.Part part, IEnumerable<PartUnitRequestDto>? unitDtos)
        {
            var dtoList = unitDtos?.Where(u => !string.IsNullOrWhiteSpace(u.UnitName)).ToList() ?? new List<PartUnitRequestDto>();
            if (part.PartUnits == null)
            {
                part.PartUnits = new List<PartUnit>();
            }

            var existingUnits = part.PartUnits.Where(u => !u.IsDeleted).ToList();

            foreach (var unit in existingUnits)
            {
                var keep = dtoList.Any(dto =>
                    (dto.Id.HasValue && dto.Id.Value == unit.Id) ||
                    (!dto.Id.HasValue && dto.UnitName.Equals(unit.UnitName, StringComparison.OrdinalIgnoreCase)));

                if (!keep)
                {
                    // ✅ SỬA: Soft delete thay vì hard delete để giữ lại lịch sử
                    unit.IsDeleted = true;
                    unit.DeletedAt = DateTime.UtcNow;
                    // Không remove từ collection, chỉ đánh dấu IsDeleted = true
                    // EF Core sẽ tự động cập nhật khi SaveChangesAsync
                }
            }

            foreach (var dto in dtoList)
            {
                PartUnit? unitEntity = null;

                if (dto.Id.HasValue)
                {
                    // ✅ SỬA: Tìm unit kể cả đã bị soft delete (để restore nếu cần)
                    unitEntity = part.PartUnits.FirstOrDefault(u => u.Id == dto.Id.Value);
                    // Nếu unit đã bị soft delete, restore lại
                    if (unitEntity != null && unitEntity.IsDeleted)
                    {
                        unitEntity.IsDeleted = false;
                        unitEntity.DeletedAt = null;
                        unitEntity.DeletedBy = null;
                    }
                }

                if (unitEntity == null)
                {
                    // ✅ SỬA: Kiểm tra xem có unit bị soft delete với cùng UnitName không (case-insensitive)
                    var normalizedUnitName = dto.UnitName.Trim();
                    unitEntity = part.PartUnits.FirstOrDefault(u => 
                        u.UnitName.Equals(normalizedUnitName, StringComparison.OrdinalIgnoreCase) && 
                        u.IsDeleted);
                    
                    if (unitEntity != null)
                    {
                        // Restore unit đã bị soft delete
                        unitEntity.IsDeleted = false;
                        unitEntity.DeletedAt = null;
                        unitEntity.DeletedBy = null;
                    }
                    else
                    {
                        // Tạo unit mới
                        unitEntity = new PartUnit
                        {
                            Part = part
                        };
                        part.PartUnits.Add(unitEntity);
                    }
                }

                unitEntity.UnitName = dto.UnitName.Trim();
                unitEntity.ConversionRate = dto.ConversionRate;
                // ✅ SỬA: Normalize Barcode và Notes (trim và set null nếu empty)
                unitEntity.Barcode = string.IsNullOrWhiteSpace(dto.Barcode) ? null : dto.Barcode.Trim();
                unitEntity.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
                // ✅ SỬA: Không set IsDefault ở đây, để EnsureDefaultUnit xử lý đồng bộ
                // unitEntity.IsDefault = dto.IsDefault; // Removed - handled by EnsureDefaultUnit
            }

            // ✅ SỬA: Xóa logic set IsDefault trong ApplyPartUnits vì EnsureDefaultUnit sẽ xử lý
            // ApplyPartUnits chỉ cập nhật collection và properties, không xử lý IsDefault
            // Logic đồng bộ DefaultUnit ↔ PartUnits.IsDefault được xử lý trong EnsureDefaultUnit
        }

        private void EnsureDefaultUnit(Core.Entities.Part part, string? defaultUnit, IEnumerable<PartUnitRequestDto>? unitDtos)
        {
            var dtoList = unitDtos?.ToList() ?? new List<PartUnitRequestDto>();

            if (!string.IsNullOrWhiteSpace(defaultUnit))
            {
                part.DefaultUnit = defaultUnit.Trim();
            }
            else
            {
                var defaultDto = dtoList.FirstOrDefault(u => u.IsDefault);
                if (defaultDto != null)
                {
                    part.DefaultUnit = defaultDto.UnitName.Trim();
                }
            }

            if (part.PartUnits == null || !part.PartUnits.Any())
            {
                if (!string.IsNullOrWhiteSpace(part.DefaultUnit))
                {
                    part.PartUnits = new List<PartUnit>
                    {
                        new PartUnit
                        {
                            UnitName = part.DefaultUnit!,
                            ConversionRate = 1,
                            IsDefault = true,
                            Part = part
                        }
                    };
                }
                return;
            }

            if (string.IsNullOrWhiteSpace(part.DefaultUnit))
            {
                // Nếu DefaultUnit null/empty, lấy từ PartUnits có IsDefault=true hoặc PartUnits.First()
                var defaultUnitEntity = part.PartUnits.FirstOrDefault(u => u.IsDefault) ?? part.PartUnits.FirstOrDefault();
                if (defaultUnitEntity != null)
                {
                    // Set tất cả units về IsDefault=false trước
                    foreach (var unit in part.PartUnits)
                    {
                        unit.IsDefault = false;
                    }
                    // Chỉ set defaultUnitEntity thành IsDefault=true
                    defaultUnitEntity.IsDefault = true;
                    part.DefaultUnit = defaultUnitEntity.UnitName;
                }
            }
            else
            {
                // ✅ SỬA: Đảm bảo chỉ 1 PartUnit có IsDefault=true
                var normalizedDefaultUnit = part.DefaultUnit.Trim();
                var matchingUnit = part.PartUnits.FirstOrDefault(u => 
                    u.UnitName.Equals(normalizedDefaultUnit, StringComparison.OrdinalIgnoreCase));
                
                // Set tất cả units về IsDefault=false trước
                foreach (var unit in part.PartUnits)
                {
                    unit.IsDefault = false;
                }
                
                if (matchingUnit != null)
                {
                    // Nếu có matching unit, set IsDefault=true cho unit đó
                    matchingUnit.IsDefault = true;
                    // Đảm bảo DefaultUnit match với UnitName (case-sensitive từ database)
                    part.DefaultUnit = matchingUnit.UnitName;
                }
                else
                {
                    // Nếu DefaultUnit không có trong PartUnits, tạo mới
                    var newUnit = new PartUnit
                    {
                        UnitName = normalizedDefaultUnit,
                        ConversionRate = 1,
                        IsDefault = true,
                        Part = part
                    };
                    part.PartUnits.Add(newUnit);
                    part.DefaultUnit = newUnit.UnitName;
                }
            }
        }
    }
}

