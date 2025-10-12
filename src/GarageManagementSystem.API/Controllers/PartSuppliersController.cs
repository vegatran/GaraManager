using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PartSuppliersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PartSuppliersController> _logger;

        public PartSuppliersController(IUnitOfWork unitOfWork, ILogger<PartSuppliersController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? partId = null, [FromQuery] int? supplierId = null)
        {
            try
            {
                var partSuppliers = await _unitOfWork.Repository<PartSupplier>().GetAllAsync();
                
                if (partId.HasValue)
                    partSuppliers = partSuppliers.Where(ps => ps.PartId == partId.Value);
                
                if (supplierId.HasValue)
                    partSuppliers = partSuppliers.Where(ps => ps.SupplierId == supplierId.Value);

                return Ok(new { success = true, data = partSuppliers.ToList() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting part suppliers");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy danh sách nhà cung cấp phụ tùng" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var partSupplier = await _unitOfWork.Repository<PartSupplier>().GetByIdAsync(id);
                if (partSupplier == null)
                    return NotFound(new { success = false, message = "Không tìm thấy" });

                return Ok(new { success = true, data = partSupplier });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting part supplier");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thông tin" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PartSupplier partSupplier)
        {
            try
            {
                partSupplier.CreatedAt = DateTime.Now;
                await _unitOfWork.Repository<PartSupplier>().AddAsync(partSupplier);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = partSupplier, message = "Tạo thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating part supplier");
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PartSupplier partSupplier)
        {
            try
            {
                var existing = await _unitOfWork.Repository<PartSupplier>().GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { success = false, message = "Không tìm thấy" });

                existing.SupplierPartNumber = partSupplier.SupplierPartNumber;
                existing.CostPrice = partSupplier.CostPrice;
                existing.LeadTimeDays = partSupplier.LeadTimeDays;
                existing.MinimumOrderQuantity = partSupplier.MinimumOrderQuantity;
                existing.IsPreferred = partSupplier.IsPreferred;
                existing.IsActive = partSupplier.IsActive;
                existing.LastOrderDate = partSupplier.LastOrderDate;
                existing.LastCostPrice = partSupplier.LastCostPrice;
                existing.UpdatedAt = DateTime.Now;

                await _unitOfWork.Repository<PartSupplier>().UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = existing, message = "Cập nhật thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating part supplier");
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var partSupplier = await _unitOfWork.Repository<PartSupplier>().GetByIdAsync(id);
                if (partSupplier == null)
                    return NotFound(new { success = false, message = "Không tìm thấy" });

                await _unitOfWork.Repository<PartSupplier>().DeleteAsync(partSupplier);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting part supplier");
                return StatusCode(500, new { success = false, message = "Lỗi khi xóa" });
            }
        }
    }
}

