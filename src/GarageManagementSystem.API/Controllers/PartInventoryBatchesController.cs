using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PartInventoryBatchesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PartInventoryBatchesController> _logger;

        public PartInventoryBatchesController(IUnitOfWork unitOfWork, ILogger<PartInventoryBatchesController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? partId = null)
        {
            try
            {
                var batches = await _unitOfWork.Repository<PartInventoryBatch>().GetAllAsync();
                
                if (partId.HasValue)
                    batches = batches.Where(b => b.PartId == partId.Value);

                var result = batches.Select(b => new
                {
                    b.Id,
                    b.PartId,
                    b.BatchNumber,
                    b.ReceiveDate,
                    b.QuantityReceived,
                    b.QuantityRemaining,
                    b.UnitCost,
                    b.SupplierId,
                    b.SourceType,
                    b.HasInvoice,
                    b.ExpiryDate,
                    b.CreatedAt
                }).OrderByDescending(b => b.ReceiveDate).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventory batches");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy lô hàng" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var batch = await _unitOfWork.Repository<PartInventoryBatch>().GetByIdAsync(id);
                if (batch == null)
                    return NotFound(new { success = false, message = "Không tìm thấy lô hàng" });

                return Ok(new { success = true, data = batch });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventory batch");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy lô hàng" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PartInventoryBatch batch)
        {
            try
            {
                // ✅ OPTIMIZED: Use CountAsync thay vì GetAllAsync().Count()
                var count = await _unitOfWork.Repository<PartInventoryBatch>().CountAsync();
                batch.BatchNumber = $"BATCH-{DateTime.Now:yyyyMMdd}-{(count + 1):D4}";
                batch.CreatedAt = DateTime.Now;
                
                await _unitOfWork.Repository<PartInventoryBatch>().AddAsync(batch);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = batch, message = "Tạo lô hàng thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating inventory batch");
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo lô hàng" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PartInventoryBatch batch)
        {
            try
            {
                var existing = await _unitOfWork.Repository<PartInventoryBatch>().GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { success = false, message = "Không tìm thấy lô hàng" });

                existing.QuantityRemaining = batch.QuantityRemaining;
                existing.ExpiryDate = batch.ExpiryDate;
                existing.Notes = batch.Notes;
                existing.UpdatedAt = DateTime.Now;

                await _unitOfWork.Repository<PartInventoryBatch>().UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = existing, message = "Cập nhật thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inventory batch");
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật" });
            }
        }
    }
}

