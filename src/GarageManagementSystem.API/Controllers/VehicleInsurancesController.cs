using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleInsurancesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<VehicleInsurancesController> _logger;

        public VehicleInsurancesController(IUnitOfWork unitOfWork, ILogger<VehicleInsurancesController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? vehicleId = null)
        {
            try
            {
                var insurances = await _unitOfWork.Repository<VehicleInsurance>().GetAllAsync();
                
                if (vehicleId.HasValue)
                    insurances = insurances.Where(i => i.VehicleId == vehicleId.Value);

                var result = insurances.Select(i => new
                {
                    i.Id,
                    i.VehicleId,
                    i.InsuranceCompany,
                    i.PolicyNumber,
                    i.CoverageType,
                    i.StartDate,
                    i.EndDate,
                    i.PremiumAmount,
                    i.IsActive,
                    i.CreatedAt
                }).OrderByDescending(i => i.CreatedAt).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle insurances");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy danh sách bảo hiểm" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var insurance = await _unitOfWork.Repository<VehicleInsurance>().GetByIdAsync(id);
                if (insurance == null)
                    return NotFound(new { success = false, message = "Không tìm thấy bảo hiểm" });

                return Ok(new { success = true, data = insurance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle insurance");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thông tin bảo hiểm" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VehicleInsurance insurance)
        {
            try
            {
                insurance.CreatedAt = DateTime.Now;
                await _unitOfWork.Repository<VehicleInsurance>().AddAsync(insurance);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = insurance, message = "Tạo bảo hiểm thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle insurance");
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo bảo hiểm" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] VehicleInsurance insurance)
        {
            try
            {
                var existing = await _unitOfWork.Repository<VehicleInsurance>().GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { success = false, message = "Không tìm thấy bảo hiểm" });

                existing.InsuranceCompany = insurance.InsuranceCompany;
                existing.PolicyNumber = insurance.PolicyNumber;
                existing.CoverageType = insurance.CoverageType;
                existing.StartDate = insurance.StartDate;
                existing.EndDate = insurance.EndDate;
                existing.PremiumAmount = insurance.PremiumAmount;
                existing.IsActive = insurance.IsActive;
                existing.Notes = insurance.Notes;
                existing.UpdatedAt = DateTime.Now;

                await _unitOfWork.Repository<VehicleInsurance>().UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = existing, message = "Cập nhật bảo hiểm thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle insurance");
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật bảo hiểm" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var insurance = await _unitOfWork.Repository<VehicleInsurance>().GetByIdAsync(id);
                if (insurance == null)
                    return NotFound(new { success = false, message = "Không tìm thấy bảo hiểm" });

                await _unitOfWork.Repository<VehicleInsurance>().DeleteAsync(insurance);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, message = "Xóa bảo hiểm thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle insurance");
                return StatusCode(500, new { success = false, message = "Lỗi khi xóa bảo hiểm" });
            }
        }
    }
}

