using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EngineSpecificationsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EngineSpecificationsController> _logger;

        public EngineSpecificationsController(IUnitOfWork unitOfWork, ILogger<EngineSpecificationsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? vehicleModelId = null)
        {
            try
            {
                var specs = await _unitOfWork.Repository<EngineSpecification>().GetAllAsync();
                
                if (vehicleModelId.HasValue)
                    specs = specs.Where(s => s.ModelId == vehicleModelId.Value);

                return Ok(new { success = true, data = specs.ToList() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting engine specifications");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thông số động cơ" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var spec = await _unitOfWork.Repository<EngineSpecification>().GetByIdAsync(id);
                if (spec == null)
                    return NotFound(new { success = false, message = "Không tìm thấy thông số động cơ" });

                return Ok(new { success = true, data = spec });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting engine specification");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thông số động cơ" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EngineSpecification spec)
        {
            try
            {
                spec.CreatedAt = DateTime.Now;
                await _unitOfWork.Repository<EngineSpecification>().AddAsync(spec);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = spec, message = "Tạo thông số động cơ thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating engine specification");
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo thông số động cơ" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EngineSpecification spec)
        {
            try
            {
                var existing = await _unitOfWork.Repository<EngineSpecification>().GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { success = false, message = "Không tìm thấy thông số động cơ" });

                existing.EngineCode = spec.EngineCode;
                existing.EngineName = spec.EngineName;
                existing.Displacement = spec.Displacement;
                existing.CylinderCount = spec.CylinderCount;
                existing.CylinderLayout = spec.CylinderLayout;
                existing.FuelType = spec.FuelType;
                existing.Aspiration = spec.Aspiration;
                existing.UpdatedAt = DateTime.Now;

                await _unitOfWork.Repository<EngineSpecification>().UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = existing, message = "Cập nhật thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating engine specification");
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var spec = await _unitOfWork.Repository<EngineSpecification>().GetByIdAsync(id);
                if (spec == null)
                    return NotFound(new { success = false, message = "Không tìm thấy thông số động cơ" });

                await _unitOfWork.Repository<EngineSpecification>().DeleteAsync(spec);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting engine specification");
                return StatusCode(500, new { success = false, message = "Lỗi khi xóa" });
            }
        }
    }
}

