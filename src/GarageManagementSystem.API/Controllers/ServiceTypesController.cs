using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceTypesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ServiceTypesController> _logger;

        public ServiceTypesController(IUnitOfWork unitOfWork, ILogger<ServiceTypesController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var serviceTypes = await _unitOfWork.Repository<ServiceType>().GetAllAsync();
                return Ok(new { success = true, data = serviceTypes.OrderBy(st => st.TypeName).ToList() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service types");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy loại dịch vụ" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var serviceType = await _unitOfWork.Repository<ServiceType>().GetByIdAsync(id);
                if (serviceType == null)
                    return NotFound(new { success = false, message = "Không tìm thấy loại dịch vụ" });

                return Ok(new { success = true, data = serviceType });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service type");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy loại dịch vụ" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ServiceType serviceType)
        {
            try
            {
                serviceType.CreatedAt = DateTime.Now;
                await _unitOfWork.Repository<ServiceType>().AddAsync(serviceType);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = serviceType, message = "Tạo loại dịch vụ thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service type");
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo loại dịch vụ" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ServiceType serviceType)
        {
            try
            {
                var existing = await _unitOfWork.Repository<ServiceType>().GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { success = false, message = "Không tìm thấy loại dịch vụ" });

                existing.TypeName = serviceType.TypeName;
                existing.Description = serviceType.Description;
                existing.UpdatedAt = DateTime.Now;

                await _unitOfWork.Repository<ServiceType>().UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = existing, message = "Cập nhật thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service type");
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var serviceType = await _unitOfWork.Repository<ServiceType>().GetByIdAsync(id);
                if (serviceType == null)
                    return NotFound(new { success = false, message = "Không tìm thấy loại dịch vụ" });

                await _unitOfWork.Repository<ServiceType>().DeleteAsync(serviceType);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service type");
                return StatusCode(500, new { success = false, message = "Lỗi khi xóa" });
            }
        }
    }
}

