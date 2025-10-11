using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
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

        public PositionsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Lấy danh sách tất cả chức vụ
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPositions()
        {
            try
            {
                var positions = await _unitOfWork.Positions.GetAllAsync();
                var activePositions = positions.Where(p => p.IsActive && !p.IsDeleted).ToList();

                return Ok(new
                {
                    success = true,
                    data = activePositions.Select(p => new
                    {
                        id = p.Id,
                        value = p.Name,
                        text = p.Name,
                        description = p.Description
                    }),
                    message = "Lấy danh sách chức vụ thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    data = (object?)null,
                    message = $"Lỗi khi lấy danh sách chức vụ: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy chức vụ theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPosition(int id)
        {
            try
            {
                var position = await _unitOfWork.Positions.GetByIdAsync(id);
                if (position == null || position.IsDeleted)
                {
                    return NotFound(new
                    {
                        success = false,
                        data = (object?)null,
                        message = "Không tìm thấy chức vụ"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        id = position.Id,
                        name = position.Name,
                        description = position.Description,
                        isActive = position.IsActive
                    },
                    message = "Lấy thông tin chức vụ thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    data = (object?)null,
                    message = $"Lỗi khi lấy thông tin chức vụ: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Tạo chức vụ mới
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePosition([FromBody] CreatePositionDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        data = (object?)null,
                        message = "Dữ liệu không hợp lệ",
                        errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var position = new Position
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Positions.AddAsync(position);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        id = position.Id,
                        name = position.Name,
                        description = position.Description,
                        isActive = position.IsActive
                    },
                    message = "Tạo chức vụ thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    data = (object?)null,
                    message = $"Lỗi khi tạo chức vụ: {ex.Message}"
                });
            }
        }
    }

    public class CreatePositionDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
