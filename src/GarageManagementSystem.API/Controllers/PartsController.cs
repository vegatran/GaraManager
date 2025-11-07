using AutoMapper;
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
                var part = await _unitOfWork.Parts.GetByIdAsync(id);
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
                var part = await _unitOfWork.Parts.GetByIdAsync(id);
                if (part == null) return NotFound(ApiResponse<PartDto>.ErrorResult("Part not found"));

                // ✅ SỬA: Dùng AutoMapper thay vì map tay
                _mapper.Map(dto, part);

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
    }
}

