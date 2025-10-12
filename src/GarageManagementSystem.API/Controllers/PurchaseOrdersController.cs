using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PurchaseOrdersController> _logger;

        public PurchaseOrdersController(IUnitOfWork unitOfWork, ILogger<PurchaseOrdersController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? supplierId = null, [FromQuery] string? status = null)
        {
            try
            {
                var orders = await _unitOfWork.Repository<PurchaseOrder>().GetAllAsync();
                
                if (supplierId.HasValue)
                    orders = orders.Where(o => o.SupplierId == supplierId.Value);
                
                if (!string.IsNullOrEmpty(status))
                    orders = orders.Where(o => o.Status == status);

                var result = orders.Select(o => new
                {
                    o.Id,
                    o.OrderNumber,
                    o.SupplierId,
                    o.OrderDate,
                    o.ExpectedDeliveryDate,
                    o.Status,
                    o.TotalAmount,
                    o.Notes,
                    o.CreatedAt
                }).OrderByDescending(o => o.CreatedAt).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting purchase orders");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy danh sách đơn mua hàng" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var order = await _unitOfWork.Repository<PurchaseOrder>().GetByIdAsync(id);
                if (order == null)
                    return NotFound(new { success = false, message = "Không tìm thấy đơn mua hàng" });

                return Ok(new { success = true, data = order });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting purchase order");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thông tin đơn mua hàng" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PurchaseOrder order)
        {
            try
            {
                // Generate PO number
                var count = (await _unitOfWork.Repository<PurchaseOrder>().GetAllAsync()).Count();
                order.OrderNumber = $"PO-{DateTime.Now:yyyyMMdd}-{(count + 1):D4}";
                order.CreatedAt = DateTime.Now;
                
                await _unitOfWork.Repository<PurchaseOrder>().AddAsync(order);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = order, message = "Tạo đơn mua hàng thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating purchase order");
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo đơn mua hàng" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PurchaseOrder order)
        {
            try
            {
                var existing = await _unitOfWork.Repository<PurchaseOrder>().GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { success = false, message = "Không tìm thấy đơn mua hàng" });

                existing.ExpectedDeliveryDate = order.ExpectedDeliveryDate;
                existing.ActualDeliveryDate = order.ActualDeliveryDate;
                existing.Status = order.Status;
                existing.Notes = order.Notes;
                existing.UpdatedAt = DateTime.Now;

                await _unitOfWork.Repository<PurchaseOrder>().UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = existing, message = "Cập nhật thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating purchase order");
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật" });
            }
        }

        [HttpPost("{id}/receive")]
        public async Task<IActionResult> ReceiveOrder(int id)
        {
            try
            {
                var order = await _unitOfWork.Repository<PurchaseOrder>().GetByIdAsync(id);
                if (order == null)
                    return NotFound(new { success = false, message = "Không tìm thấy đơn mua hàng" });

                order.Status = "Received";
                order.ActualDeliveryDate = DateTime.Now;
                order.UpdatedAt = DateTime.Now;

                await _unitOfWork.Repository<PurchaseOrder>().UpdateAsync(order);

                // Update LastOrderDate and LastCostPrice in PartSupplier
                var poItems = await _unitOfWork.Repository<PurchaseOrderItem>().GetAllAsync();
                var orderItems = poItems.Where(i => i.PurchaseOrderId == id).ToList();

                foreach (var item in orderItems)
                {
                    var partSuppliers = await _unitOfWork.Repository<PartSupplier>().GetAllAsync();
                    var partSupplier = partSuppliers.FirstOrDefault(ps => ps.PartId == item.PartId && ps.SupplierId == order.SupplierId);
                    
                    if (partSupplier != null)
                    {
                        partSupplier.LastOrderDate = order.OrderDate;
                        partSupplier.LastCostPrice = item.UnitPrice;
                        partSupplier.UpdatedAt = DateTime.Now;
                        await _unitOfWork.Repository<PartSupplier>().UpdateAsync(partSupplier);
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = order, message = "Đã nhận hàng" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving purchase order");
                return StatusCode(500, new { success = false, message = "Lỗi khi nhận hàng" });
            }
        }
    }
}

