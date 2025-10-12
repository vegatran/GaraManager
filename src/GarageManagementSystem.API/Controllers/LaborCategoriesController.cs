using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LaborCategoriesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LaborCategoriesController> _logger;

        public LaborCategoriesController(IUnitOfWork unitOfWork, ILogger<LaborCategoriesController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var categories = await _unitOfWork.Repository<LaborCategory>().GetAllAsync();
                return Ok(new { success = true, data = categories.OrderBy(c => c.CategoryName).ToList() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting labor categories");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy danh sách danh mục công việc" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var category = await _unitOfWork.Repository<LaborCategory>().GetByIdAsync(id);
                if (category == null)
                    return NotFound(new { success = false, message = "Không tìm thấy danh mục" });

                return Ok(new { success = true, data = category });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting labor category");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thông tin danh mục" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LaborCategory category)
        {
            try
            {
                category.CreatedAt = DateTime.Now;
                await _unitOfWork.Repository<LaborCategory>().AddAsync(category);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = category, message = "Tạo danh mục thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating labor category");
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo danh mục" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] LaborCategory category)
        {
            try
            {
                var existing = await _unitOfWork.Repository<LaborCategory>().GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { success = false, message = "Không tìm thấy danh mục" });

                existing.CategoryName = category.CategoryName;
                existing.Description = category.Description;
                existing.UpdatedAt = DateTime.Now;

                await _unitOfWork.Repository<LaborCategory>().UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = existing, message = "Cập nhật thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating labor category");
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var category = await _unitOfWork.Repository<LaborCategory>().GetByIdAsync(id);
                if (category == null)
                    return NotFound(new { success = false, message = "Không tìm thấy danh mục" });

                await _unitOfWork.Repository<LaborCategory>().DeleteAsync(category);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting labor category");
                return StatusCode(500, new { success = false, message = "Lỗi khi xóa" });
            }
        }
    }
}

