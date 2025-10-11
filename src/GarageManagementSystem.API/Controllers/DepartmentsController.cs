using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarageManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiScope")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public DepartmentsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Lấy danh sách tất cả bộ phận
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            try
            {
                var departments = await _unitOfWork.Departments.GetAllAsync();
                var activeDepartments = departments.Where(d => d.IsActive && !d.IsDeleted).ToList();

                return Ok(new
                {
                    success = true,
                    data = activeDepartments.Select(d => new
                    {
                        id = d.Id,
                        value = d.Name,
                        text = d.Name,
                        description = d.Description
                    }),
                    message = "Lấy danh sách bộ phận thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    data = (object?)null,
                    message = $"Lỗi khi lấy danh sách bộ phận: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy bộ phận theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDepartment(int id)
        {
            try
            {
                var department = await _unitOfWork.Departments.GetByIdAsync(id);
                if (department == null || department.IsDeleted)
                {
                    return NotFound(new
                    {
                        success = false,
                        data = (object?)null,
                        message = "Không tìm thấy bộ phận"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        id = department.Id,
                        name = department.Name,
                        description = department.Description,
                        isActive = department.IsActive
                    },
                    message = "Lấy thông tin bộ phận thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    data = (object?)null,
                    message = $"Lỗi khi lấy thông tin bộ phận: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Tạo bộ phận mới
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDto dto)
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

                var department = new Department
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Departments.AddAsync(department);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        id = department.Id,
                        name = department.Name,
                        description = department.Description,
                        isActive = department.IsActive
                    },
                    message = "Tạo bộ phận thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    data = (object?)null,
                    message = $"Lỗi khi tạo bộ phận: {ex.Message}"
                });
            }
        }
    }

    public class CreateDepartmentDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
