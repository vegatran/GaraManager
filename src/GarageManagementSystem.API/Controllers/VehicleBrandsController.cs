using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleBrandsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<VehicleBrandsController> _logger;

        public VehicleBrandsController(IUnitOfWork unitOfWork, ILogger<VehicleBrandsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var brands = await _unitOfWork.Repository<VehicleBrand>().GetAllAsync();
                return Ok(new { success = true, data = brands.OrderBy(b => b.BrandName).ToList() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle brands");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy danh sách hãng xe" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var brand = await _unitOfWork.Repository<VehicleBrand>().GetByIdAsync(id);
                if (brand == null)
                    return NotFound(new { success = false, message = "Không tìm thấy hãng xe" });

                return Ok(new { success = true, data = brand });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle brand");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thông tin hãng xe" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VehicleBrand brand)
        {
            try
            {
                brand.CreatedAt = DateTime.Now;
                await _unitOfWork.Repository<VehicleBrand>().AddAsync(brand);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = brand, message = "Tạo hãng xe thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle brand");
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo hãng xe" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] VehicleBrand brand)
        {
            try
            {
                var existing = await _unitOfWork.Repository<VehicleBrand>().GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { success = false, message = "Không tìm thấy hãng xe" });

                existing.BrandName = brand.BrandName;
                existing.Country = brand.Country;
                existing.UpdatedAt = DateTime.Now;

                await _unitOfWork.Repository<VehicleBrand>().UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = existing, message = "Cập nhật thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle brand");
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var brand = await _unitOfWork.Repository<VehicleBrand>().GetByIdAsync(id);
                if (brand == null)
                    return NotFound(new { success = false, message = "Không tìm thấy hãng xe" });

                await _unitOfWork.Repository<VehicleBrand>().DeleteAsync(brand);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle brand");
                return StatusCode(500, new { success = false, message = "Lỗi khi xóa" });
            }
        }
    }
}

