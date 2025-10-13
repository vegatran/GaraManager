using AutoMapper;
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
    public class PositionsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PositionsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<PositionDto>>>> GetPositions()
        {
            try
            {
                var positions = await _unitOfWork.Positions.GetAllAsync();
                var positionDtos = positions.Where(p => !p.IsDeleted).Select(p => _mapper.Map<PositionDto>(p)).ToList();
                
                return Ok(ApiResponse<List<PositionDto>>.SuccessResult(positionDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<PositionDto>>.ErrorResult("Error retrieving positions", ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PositionDto>>> GetPosition(int id)
        {
            try
            {
                var position = await _unitOfWork.Positions.GetByIdAsync(id);
                if (position == null)
                {
                    return NotFound(ApiResponse<PositionDto>.ErrorResult("Position not found"));
                }

                var positionDto = _mapper.Map<PositionDto>(position);
                return Ok(ApiResponse<PositionDto>.SuccessResult(positionDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PositionDto>.ErrorResult("Error retrieving position", ex.Message));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<PositionDto>>> CreatePosition(CreatePositionDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<PositionDto>.ErrorResult("Invalid data", errors));
                }

                // Use AutoMapper to map DTO to Entity
                var position = _mapper.Map<Core.Entities.Position>(createDto);

                // Begin transaction
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.Positions.AddAsync(position);
                    await _unitOfWork.SaveChangesAsync();

                    // Commit transaction
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch
                {
                    // Rollback transaction
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }

                var positionDto = _mapper.Map<PositionDto>(position);
                return CreatedAtAction(nameof(GetPosition), new { id = position.Id }, 
                    ApiResponse<PositionDto>.SuccessResult(positionDto, "Position created successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PositionDto>.ErrorResult("Error creating position", ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<PositionDto>>> UpdatePosition(int id, UpdatePositionDto updateDto)
        {
            try
            {
                if (id != updateDto.Id)
                {
                    return BadRequest(ApiResponse<PositionDto>.ErrorResult("ID mismatch"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<PositionDto>.ErrorResult("Invalid data", errors));
                }

                var position = await _unitOfWork.Positions.GetByIdAsync(id);
                if (position == null)
                {
                    return NotFound(ApiResponse<PositionDto>.ErrorResult("Position not found"));
                }

                // Use AutoMapper to map DTO to existing Entity
                _mapper.Map(updateDto, position);

                // Begin transaction
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.Positions.UpdateAsync(position);
                    await _unitOfWork.SaveChangesAsync();

                    // Commit transaction
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch
                {
                    // Rollback transaction
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }

                var positionDto = _mapper.Map<PositionDto>(position);
                return Ok(ApiResponse<PositionDto>.SuccessResult(positionDto, "Position updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PositionDto>.ErrorResult("Error updating position", ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeletePosition(int id)
        {
            try
            {
                var position = await _unitOfWork.Positions.GetByIdAsync(id);
                if (position == null)
                {
                    return NotFound(ApiResponse.ErrorResult("Position not found"));
                }

                await _unitOfWork.Positions.DeleteAsync(position);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse.SuccessResult("Position deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResult("Error deleting position", ex.Message));
            }
        }
    }
}