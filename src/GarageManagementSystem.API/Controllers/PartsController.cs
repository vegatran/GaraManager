using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarageManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiScope")]
    public class PartsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public PartsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<PartDto>>>> GetParts()
        {
            try
            {
                var parts = await _unitOfWork.Parts.GetAllAsync();
                var partDtos = parts.Select(MapToDto).ToList();
                return Ok(ApiResponse<List<PartDto>>.SuccessResult(partDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<PartDto>>.ErrorResult("Error retrieving parts", ex.Message));
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

                var part = new Core.Entities.Part
                {
                    PartNumber = dto.PartNumber,
                    PartName = dto.PartName,
                    Description = dto.Description,
                    Category = dto.Category,
                    Brand = dto.Brand,
                    CostPrice = dto.CostPrice,
                    SellPrice = dto.SellPrice,
                    QuantityInStock = dto.QuantityInStock,
                    MinimumStock = dto.MinimumStock,
                    ReorderLevel = dto.ReorderLevel,
                    Unit = dto.Unit,
                    CompatibleVehicles = dto.CompatibleVehicles,
                    Location = dto.Location,
                    IsActive = dto.IsActive
                };

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

                part.PartNumber = dto.PartNumber;
                part.PartName = dto.PartName;
                part.Description = dto.Description;
                part.Category = dto.Category;
                part.Brand = dto.Brand;
                part.CostPrice = dto.CostPrice;
                part.SellPrice = dto.SellPrice;
                part.QuantityInStock = dto.QuantityInStock;
                part.MinimumStock = dto.MinimumStock;
                part.ReorderLevel = dto.ReorderLevel;
                part.Unit = dto.Unit;
                part.CompatibleVehicles = dto.CompatibleVehicles;
                part.Location = dto.Location;
                part.IsActive = dto.IsActive;

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

        private static PartDto MapToDto(Core.Entities.Part part)
        {
            return new PartDto
            {
                Id = part.Id,
                PartNumber = part.PartNumber,
                PartName = part.PartName,
                Description = part.Description,
                Category = part.Category,
                Brand = part.Brand,
                CostPrice = part.CostPrice,
                SellPrice = part.SellPrice,
                QuantityInStock = part.QuantityInStock,
                MinimumStock = part.MinimumStock,
                ReorderLevel = part.ReorderLevel,
                Unit = part.Unit,
                CompatibleVehicles = part.CompatibleVehicles,
                Location = part.Location,
                IsActive = part.IsActive,
                CreatedAt = part.CreatedAt,
                CreatedBy = part.CreatedBy,
                UpdatedAt = part.UpdatedAt,
                UpdatedBy = part.UpdatedBy
            };
        }
    }
}

