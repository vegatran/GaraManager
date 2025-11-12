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

                var part = _mapper.Map<Core.Entities.Part>(dto);

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

                // ✅ SỬA: Dùng AutoMapper thay vì map tay
                _mapper.Map(dto, part);

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

            var existingUnits = part.PartUnits.ToList();

            foreach (var unit in existingUnits)
            {
                var keep = dtoList.Any(dto =>
                    (dto.Id.HasValue && dto.Id.Value == unit.Id) ||
                    (!dto.Id.HasValue && dto.UnitName.Equals(unit.UnitName, StringComparison.OrdinalIgnoreCase)));

                if (!keep)
                {
                    part.PartUnits.Remove(unit);
                }
            }

            foreach (var dto in dtoList)
            {
                PartUnit? unitEntity = null;

                if (dto.Id.HasValue)
                {
                    unitEntity = part.PartUnits.FirstOrDefault(u => u.Id == dto.Id.Value);
                }

                if (unitEntity == null)
                {
                    unitEntity = new PartUnit
                    {
                        Part = part
                    };
                    part.PartUnits.Add(unitEntity);
                }

                unitEntity.UnitName = dto.UnitName.Trim();
                unitEntity.ConversionRate = dto.ConversionRate;
                unitEntity.Barcode = dto.Barcode;
                unitEntity.Notes = dto.Notes;
                unitEntity.IsDefault = dto.IsDefault;
            }

            // Ensure only one default unit
            var defaultDto = dtoList.FirstOrDefault(u => u.IsDefault);
            if (defaultDto != null)
            {
                foreach (var unit in part.PartUnits)
                {
                    unit.IsDefault = unit.UnitName.Equals(defaultDto.UnitName, StringComparison.OrdinalIgnoreCase) &&
                                     (!defaultDto.Id.HasValue || unit.Id == defaultDto.Id.Value);
                }
            }
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
                var defaultUnitEntity = part.PartUnits.FirstOrDefault(u => u.IsDefault) ?? part.PartUnits.First();
                defaultUnitEntity.IsDefault = true;
                part.DefaultUnit = defaultUnitEntity.UnitName;
            }
            else
            {
                foreach (var unit in part.PartUnits)
                {
                    unit.IsDefault = unit.UnitName.Equals(part.DefaultUnit, StringComparison.OrdinalIgnoreCase);
                }

                if (!part.PartUnits.Any(u => u.IsDefault))
                {
                    var match = part.PartUnits.FirstOrDefault();
                    if (match != null)
                    {
                        match.IsDefault = true;
                        part.DefaultUnit = match.UnitName;
                    }
                }
            }
        }
    }
}

