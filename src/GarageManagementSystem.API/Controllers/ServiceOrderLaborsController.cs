using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceOrderLaborsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ServiceOrderLaborsController> _logger;

        public ServiceOrderLaborsController(IUnitOfWork unitOfWork, ILogger<ServiceOrderLaborsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet("service-order/{serviceOrderId}")]
        public async Task<IActionResult> GetByServiceOrder(int serviceOrderId)
        {
            try
            {
                var labors = await _unitOfWork.Repository<ServiceOrderLabor>().GetAllAsync();
                var result = labors.Where(l => l.ServiceOrderId == serviceOrderId).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service order labors");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy công việc" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ServiceOrderLabor labor)
        {
            try
            {
                labor.CreatedAt = DateTime.Now;
                await _unitOfWork.Repository<ServiceOrderLabor>().AddAsync(labor);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = labor, message = "Thêm công việc thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service order labor");
                return StatusCode(500, new { success = false, message = "Lỗi khi thêm công việc" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ServiceOrderLabor labor)
        {
            try
            {
                var existing = await _unitOfWork.Repository<ServiceOrderLabor>().GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { success = false, message = "Không tìm thấy công việc" });

                existing.ActualHours = labor.ActualHours;
                existing.LaborRate = labor.LaborRate;
                existing.TotalLaborCost = labor.TotalLaborCost;
                existing.Notes = labor.Notes;
                existing.Status = labor.Status;
                existing.UpdatedAt = DateTime.Now;

                await _unitOfWork.Repository<ServiceOrderLabor>().UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = existing, message = "Cập nhật thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service order labor");
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var labor = await _unitOfWork.Repository<ServiceOrderLabor>().GetByIdAsync(id);
                if (labor == null)
                    return NotFound(new { success = false, message = "Không tìm thấy công việc" });

                await _unitOfWork.Repository<ServiceOrderLabor>().DeleteAsync(labor);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service order labor");
                return StatusCode(500, new { success = false, message = "Lỗi khi xóa" });
            }
        }
    }
}

