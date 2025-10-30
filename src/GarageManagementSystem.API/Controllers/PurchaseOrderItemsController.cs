using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseOrderItemsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PurchaseOrderItemsController> _logger;

        public PurchaseOrderItemsController(IUnitOfWork unitOfWork, ILogger<PurchaseOrderItemsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet("purchase-order/{purchaseOrderId}")]
        public async Task<IActionResult> GetByPurchaseOrder(int purchaseOrderId)
        {
            try
            {
                // ✅ OPTIMIZED: Filter ở database level thay vì load all rồi filter trong memory
                var result = (await _unitOfWork.Repository<PurchaseOrderItem>()
                    .FindAsync(i => i.PurchaseOrderId == purchaseOrderId)).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting purchase order items");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy chi tiết đơn hàng" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PurchaseOrderItem item)
        {
            try
            {
                // Get Part name
                var part = await _unitOfWork.Repository<Part>().GetByIdAsync(item.PartId);
                if (part != null)
                {
                    item.PartName = part.PartName;
                }
                
                item.CreatedAt = DateTime.Now;
                await _unitOfWork.Repository<PurchaseOrderItem>().AddAsync(item);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = item, message = "Thêm chi tiết thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating purchase order item");
                return StatusCode(500, new { success = false, message = "Lỗi khi thêm chi tiết" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PurchaseOrderItem item)
        {
            try
            {
                var existing = await _unitOfWork.Repository<PurchaseOrderItem>().GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { success = false, message = "Không tìm thấy chi tiết" });

                // Update Part name if PartId changed
                if (existing.PartId != item.PartId)
                {
                    var part = await _unitOfWork.Repository<Part>().GetByIdAsync(item.PartId);
                    if (part != null)
                    {
                        existing.PartName = part.PartName;
                    }
                    existing.PartId = item.PartId;
                }

                existing.QuantityOrdered = item.QuantityOrdered;
                existing.UnitPrice = item.UnitPrice;
                existing.TotalPrice = item.TotalPrice;
                existing.QuantityReceived = item.QuantityReceived;
                existing.Notes = item.Notes;
                existing.UpdatedAt = DateTime.Now;

                await _unitOfWork.Repository<PurchaseOrderItem>().UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = existing, message = "Cập nhật thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating purchase order item");
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var item = await _unitOfWork.Repository<PurchaseOrderItem>().GetByIdAsync(id);
                if (item == null)
                    return NotFound(new { success = false, message = "Không tìm thấy chi tiết" });

                await _unitOfWork.Repository<PurchaseOrderItem>().DeleteAsync(item);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting purchase order item");
                return StatusCode(500, new { success = false, message = "Lỗi khi xóa" });
            }
        }
    }
}

