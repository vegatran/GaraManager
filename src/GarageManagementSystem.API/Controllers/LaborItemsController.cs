using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LaborItemsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LaborItemsController> _logger;

        public LaborItemsController(IUnitOfWork unitOfWork, ILogger<LaborItemsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? categoryId = null)
        {
            try
            {
                var items = await _unitOfWork.Repository<LaborItem>().GetAllAsync();
                
                if (categoryId.HasValue)
                    items = items.Where(i => i.LaborCategoryId == categoryId.Value);

                return Ok(new { success = true, data = items.OrderBy(i => i.ItemName).ToList() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting labor items");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy danh sách công việc" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var item = await _unitOfWork.Repository<LaborItem>().GetByIdAsync(id);
                if (item == null)
                    return NotFound(new { success = false, message = "Không tìm thấy công việc" });

                return Ok(new { success = true, data = item });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting labor item");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thông tin công việc" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LaborItem item)
        {
            try
            {
                item.CreatedAt = DateTime.Now;
                await _unitOfWork.Repository<LaborItem>().AddAsync(item);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = item, message = "Tạo công việc thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating labor item");
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo công việc" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] LaborItem item)
        {
            try
            {
                var existing = await _unitOfWork.Repository<LaborItem>().GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { success = false, message = "Không tìm thấy công việc" });

                existing.ItemName = item.ItemName;
                existing.Description = item.Description;
                existing.StandardHours = item.StandardHours;
                existing.LaborRate = item.LaborRate;
                existing.TotalLaborCost = item.TotalLaborCost;
                existing.IsActive = item.IsActive;
                existing.UpdatedAt = DateTime.Now;

                await _unitOfWork.Repository<LaborItem>().UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = existing, message = "Cập nhật thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating labor item");
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var item = await _unitOfWork.Repository<LaborItem>().GetByIdAsync(id);
                if (item == null)
                    return NotFound(new { success = false, message = "Không tìm thấy công việc" });

                await _unitOfWork.Repository<LaborItem>().DeleteAsync(item);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting labor item");
                return StatusCode(500, new { success = false, message = "Lỗi khi xóa" });
            }
        }
    }
}

