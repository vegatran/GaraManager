using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PartGroupCompatibilitiesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PartGroupCompatibilitiesController> _logger;

        public PartGroupCompatibilitiesController(IUnitOfWork unitOfWork, ILogger<PartGroupCompatibilitiesController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet("part-group/{partGroupId}")]
        public async Task<IActionResult> GetByPartGroup(int partGroupId)
        {
            try
            {
                var compatibilities = await _unitOfWork.Repository<PartGroupCompatibility>().GetAllAsync();
                var result = compatibilities.Where(c => c.PartGroupId == partGroupId).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting compatibilities");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy danh sách tương thích" });
            }
        }

        [HttpGet("vehicle-model/{vehicleModelId}")]
        public async Task<IActionResult> GetByVehicleModel(int vehicleModelId)
        {
            try
            {
                var compatibilities = await _unitOfWork.Repository<PartGroupCompatibility>().GetAllAsync();
                var result = compatibilities.Where(c => c.ModelId == vehicleModelId).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting compatibilities");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy danh sách tương thích" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PartGroupCompatibility compatibility)
        {
            try
            {
                compatibility.CreatedAt = DateTime.Now;
                await _unitOfWork.Repository<PartGroupCompatibility>().AddAsync(compatibility);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = compatibility, message = "Thêm tương thích thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating compatibility");
                return StatusCode(500, new { success = false, message = "Lỗi khi thêm tương thích" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var compatibility = await _unitOfWork.Repository<PartGroupCompatibility>().GetByIdAsync(id);
                if (compatibility == null)
                    return NotFound(new { success = false, message = "Không tìm thấy" });

                await _unitOfWork.Repository<PartGroupCompatibility>().DeleteAsync(compatibility);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting compatibility");
                return StatusCode(500, new { success = false, message = "Lỗi khi xóa" });
            }
        }
    }
}

