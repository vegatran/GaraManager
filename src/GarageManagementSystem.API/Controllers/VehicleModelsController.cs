using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleModelsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<VehicleModelsController> _logger;

        public VehicleModelsController(IUnitOfWork unitOfWork, ILogger<VehicleModelsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? brandId = null)
        {
            try
            {
                var models = await _unitOfWork.Repository<VehicleModel>().GetAllAsync();
                
                if (brandId.HasValue)
                    models = models.Where(m => m.BrandId == brandId.Value);

                return Ok(new { success = true, data = models.OrderBy(m => m.ModelName).ToList() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle models");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy danh sách dòng xe" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var model = await _unitOfWork.Repository<VehicleModel>().GetByIdAsync(id);
                if (model == null)
                    return NotFound(new { success = false, message = "Không tìm thấy dòng xe" });

                return Ok(new { success = true, data = model });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle model");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thông tin dòng xe" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VehicleModel model)
        {
            try
            {
                model.CreatedAt = DateTime.Now;
                await _unitOfWork.Repository<VehicleModel>().AddAsync(model);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = model, message = "Tạo dòng xe thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle model");
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo dòng xe" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] VehicleModel model)
        {
            try
            {
                var existing = await _unitOfWork.Repository<VehicleModel>().GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { success = false, message = "Không tìm thấy dòng xe" });

                existing.ModelName = model.ModelName;
                existing.ModelCode = model.ModelCode;
                existing.StartYear = model.StartYear;
                existing.EndYear = model.EndYear;
                existing.VehicleType = model.VehicleType;
                existing.UpdatedAt = DateTime.Now;

                await _unitOfWork.Repository<VehicleModel>().UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = existing, message = "Cập nhật thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle model");
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var model = await _unitOfWork.Repository<VehicleModel>().GetByIdAsync(id);
                if (model == null)
                    return NotFound(new { success = false, message = "Không tìm thấy dòng xe" });

                await _unitOfWork.Repository<VehicleModel>().DeleteAsync(model);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle model");
                return StatusCode(500, new { success = false, message = "Lỗi khi xóa" });
            }
        }
    }
}

