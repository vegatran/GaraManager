using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PartGroupsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PartGroupsController> _logger;

        public PartGroupsController(IUnitOfWork unitOfWork, ILogger<PartGroupsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var groups = await _unitOfWork.Repository<PartGroup>().GetAllAsync();
                return Ok(new { success = true, data = groups.OrderBy(g => g.GroupName).ToList() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting part groups");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy danh sách nhóm phụ tùng" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var group = await _unitOfWork.Repository<PartGroup>().GetByIdAsync(id);
                if (group == null)
                    return NotFound(new { success = false, message = "Không tìm thấy nhóm phụ tùng" });

                return Ok(new { success = true, data = group });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting part group");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thông tin nhóm phụ tùng" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PartGroup group)
        {
            try
            {
                group.CreatedAt = DateTime.Now;
                await _unitOfWork.Repository<PartGroup>().AddAsync(group);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = group, message = "Tạo nhóm phụ tùng thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating part group");
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo nhóm phụ tùng" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PartGroup group)
        {
            try
            {
                var existing = await _unitOfWork.Repository<PartGroup>().GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { success = false, message = "Không tìm thấy nhóm phụ tùng" });

                existing.GroupName = group.GroupName;
                existing.Description = group.Description;
                existing.UpdatedAt = DateTime.Now;

                await _unitOfWork.Repository<PartGroup>().UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = existing, message = "Cập nhật thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating part group");
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var group = await _unitOfWork.Repository<PartGroup>().GetByIdAsync(id);
                if (group == null)
                    return NotFound(new { success = false, message = "Không tìm thấy nhóm phụ tùng" });

                await _unitOfWork.Repository<PartGroup>().DeleteAsync(group);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting part group");
                return StatusCode(500, new { success = false, message = "Lỗi khi xóa" });
            }
        }
    }
}

