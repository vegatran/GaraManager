using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceOrderController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ServiceOrderController> _logger;

        public ServiceOrderController(
            IUnitOfWork unitOfWork,
            ILogger<ServiceOrderController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách service orders
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetServiceOrders(
            [FromQuery] string? status = null,
            [FromQuery] int? customerId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var orders = await _unitOfWork.ServiceOrders.GetAllAsync();

                // Filters
                if (!string.IsNullOrEmpty(status))
                    orders = orders.Where(o => o.Status == status);

                if (customerId.HasValue)
                    orders = orders.Where(o => o.CustomerId == customerId.Value);

                if (fromDate.HasValue)
                    orders = orders.Where(o => o.OrderDate >= fromDate.Value);

                if (toDate.HasValue)
                    orders = orders.Where(o => o.OrderDate <= toDate.Value);

                var result = orders.Select(o => new
                {
                    o.Id,
                    o.OrderNumber,
                    o.OrderDate,
                    o.CustomerId,
                    o.VehicleId,
                    o.QuotationId,
                    o.InsuranceClaimId,
                    o.Description,
                    EstimatedAmount = o.EstimatedAmount,
                    ActualAmount = o.ActualAmount,
                    o.Status,
                    o.CreatedAt
                }).OrderByDescending(o => o.CreatedAt).ToList();

                return Ok(new { success = true, data = result, count = result.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service orders");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy danh sách đơn hàng" });
            }
        }

        /// <summary>
        /// Lấy chi tiết service order
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceOrder(int id)
        {
            try
            {
                var order = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
                if (order == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                // Get customer and vehicle
                var customer = await _unitOfWork.Customers.GetByIdAsync(order.CustomerId);
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(order.VehicleId);

                // ✅ OPTIMIZED: Filter service items ở database level
                var serviceItems = (await _unitOfWork.Repository<ServiceOrderItem>()
                    .FindAsync(i => i.ServiceOrderId == id)).ToList();
                var items = serviceItems.Select(i => new
                {
                    i.Id,
                    i.ServiceId,
                    i.ServiceName,
                    i.Quantity,
                    i.UnitPrice,
                    Total = i.Quantity * i.UnitPrice
                }).ToList();

                // ✅ OPTIMIZED: Filter parts ở database level
                var orderParts = (await _unitOfWork.Repository<ServiceOrderPart>()
                    .FindAsync(p => p.ServiceOrderId == id)).ToList();
                var parts = orderParts.Select(p => new
                {
                    p.Id,
                    p.PartId,
                    p.PartName,
                    p.Quantity,
                    p.UnitPrice,
                    Total = p.Quantity * p.UnitPrice
                }).ToList();

                var result = new
                {
                    order.Id,
                    order.OrderNumber,
                    order.OrderDate,
                    order.CustomerId,
                    CustomerName = customer?.Name,
                    CustomerPhone = customer?.Phone,
                    order.VehicleId,
                    VehiclePlate = vehicle?.LicensePlate,
                    VehicleMake = vehicle?.Brand,
                    VehicleModel = vehicle?.Model,
                    order.QuotationId,
                    order.InsuranceClaimId,
                    order.Description,
                    EstimatedAmount = order.EstimatedAmount,
                    ActualAmount = order.ActualAmount,
                    order.Status,
                    order.StartDate,
                    order.CompletedDate,
                    ServiceItems = items,
                    Parts = parts,
                    order.CreatedAt,
                    order.UpdatedAt
                };

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting service order {id}");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thông tin đơn hàng" });
            }
        }

        /// <summary>
        /// Tạo service order mới
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateServiceOrder([FromBody] CreateServiceOrderRequest request)
        {
            try
            {
                // Validate customer and vehicle
                var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
                if (customer == null)
                {
                    return BadRequest(new { success = false, message = "Không tìm thấy khách hàng" });
                }

                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.VehicleId);
                if (vehicle == null)
                {
                    return BadRequest(new { success = false, message = "Không tìm thấy xe" });
                }

                // Generate order number
                var orderNumber = await GenerateOrderNumber();

                var order = new ServiceOrder
                {
                    OrderNumber = orderNumber,
                    OrderDate = DateTime.Now,
                    CustomerId = request.CustomerId,
                    VehicleId = request.VehicleId,
                    QuotationId = request.QuotationId,
                    InsuranceClaimId = request.InsuranceClaimId,
                    Description = request.Description,
                    EstimatedAmount = 0,
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.ServiceOrders.AddAsync(order);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Created service order {orderNumber}");

                return Ok(new
                {
                    success = true,
                    message = "Tạo đơn hàng thành công",
                    data = new
                    {
                        order.Id,
                        order.OrderNumber
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service order");
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo đơn hàng" });
            }
        }

        /// <summary>
        /// Thêm service item vào đơn hàng
        /// </summary>
        [HttpPost("{id}/items")]
        public async Task<IActionResult> AddServiceItem(int id, [FromBody] AddServiceItemRequest request)
        {
            try
            {
                var order = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
                if (order == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                if (order.Status != "Pending")
                {
                    return BadRequest(new { success = false, message = "Chỉ được sửa đơn hàng ở trạng thái Pending" });
                }

                var item = new ServiceOrderItem
                {
                    ServiceOrderId = id,
                    ServiceId = request.ServiceId,
                    ServiceName = request.ServiceName,
                    Quantity = request.Quantity,
                    UnitPrice = request.UnitPrice,
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.Repository<ServiceOrderItem>().AddAsync(item);
                
                // Recalculate estimated amount
                await RecalculateOrderAmount(id);

                _logger.LogInformation($"Added service item to order {id}");

                return Ok(new { success = true, message = "Thêm dịch vụ thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding service item to order {id}");
                return StatusCode(500, new { success = false, message = "Lỗi khi thêm dịch vụ" });
            }
        }

        /// <summary>
        /// Thêm part vào đơn hàng
        /// </summary>
        [HttpPost("{id}/parts")]
        public async Task<IActionResult> AddPart(int id, [FromBody] AddPartRequest request)
        {
            try
            {
                var order = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
                if (order == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                if (order.Status != "Pending")
                {
                    return BadRequest(new { success = false, message = "Chỉ được sửa đơn hàng ở trạng thái Pending" });
                }

                var part = new ServiceOrderPart
                {
                    ServiceOrderId = id,
                    PartId = request.PartId,
                    PartName = request.PartName,
                    Quantity = request.Quantity,
                    UnitPrice = request.UnitPrice,
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.Repository<ServiceOrderPart>().AddAsync(part);

                // Recalculate estimated amount
                await RecalculateOrderAmount(id);

                _logger.LogInformation($"Added part to order {id}");

                return Ok(new { success = true, message = "Thêm phụ tùng thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding part to order {id}");
                return StatusCode(500, new { success = false, message = "Lỗi khi thêm phụ tùng" });
            }
        }

        /// <summary>
        /// Xóa service item
        /// </summary>
        [HttpDelete("{id}/items/{itemId}")]
        public async Task<IActionResult> DeleteServiceItem(int id, int itemId)
        {
            try
            {
                var order = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
                if (order == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                if (order.Status != "Pending")
                {
                    return BadRequest(new { success = false, message = "Chỉ được sửa đơn hàng ở trạng thái Pending" });
                }

                var item = await _unitOfWork.Repository<ServiceOrderItem>().GetByIdAsync(itemId);
                if (item == null || item.ServiceOrderId != id)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy dịch vụ" });
                }

                await _unitOfWork.Repository<ServiceOrderItem>().DeleteAsync(item);

                // Recalculate estimated amount
                await RecalculateOrderAmount(id);

                _logger.LogInformation($"Deleted service item {itemId} from order {id}");

                return Ok(new { success = true, message = "Xóa dịch vụ thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting service item {itemId} from order {id}");
                return StatusCode(500, new { success = false, message = "Lỗi khi xóa dịch vụ" });
            }
        }

        /// <summary>
        /// Xóa part
        /// </summary>
        [HttpDelete("{id}/parts/{partId}")]
        public async Task<IActionResult> DeletePart(int id, int partId)
        {
            try
            {
                var order = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
                if (order == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                if (order.Status != "Pending")
                {
                    return BadRequest(new { success = false, message = "Chỉ được sửa đơn hàng ở trạng thái Pending" });
                }

                var part = await _unitOfWork.Repository<ServiceOrderPart>().GetByIdAsync(partId);
                if (part == null || part.ServiceOrderId != id)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy phụ tùng" });
                }

                await _unitOfWork.Repository<ServiceOrderPart>().DeleteAsync(part);

                // Recalculate estimated amount
                await RecalculateOrderAmount(id);

                _logger.LogInformation($"Deleted part {partId} from order {id}");

                return Ok(new { success = true, message = "Xóa phụ tùng thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting part {partId} from order {id}");
                return StatusCode(500, new { success = false, message = "Lỗi khi xóa phụ tùng" });
            }
        }

        /// <summary>
        /// Bắt đầu làm việc (Pending → In Progress)
        /// </summary>
        [HttpPost("{id}/start")]
        public async Task<IActionResult> StartOrder(int id)
        {
            try
            {
                var order = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
                if (order == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                if (order.Status != "Pending")
                {
                    return BadRequest(new { success = false, message = "Đơn hàng không ở trạng thái Pending" });
                }

                order.Status = "In Progress";
                order.StartDate = DateTime.Now;
                order.UpdatedAt = DateTime.Now;

                await _unitOfWork.ServiceOrders.UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Started service order {id}");

                return Ok(new { success = true, message = "Bắt đầu làm việc thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error starting order {id}");
                return StatusCode(500, new { success = false, message = "Lỗi khi bắt đầu làm việc" });
            }
        }

        /// <summary>
        /// Hoàn thành đơn hàng (In Progress → Completed)
        /// </summary>
        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompleteOrder(int id, [FromBody] CompleteOrderRequest request)
        {
            try
            {
                var order = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
                if (order == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                if (order.Status != "In Progress")
                {
                    return BadRequest(new { success = false, message = "Đơn hàng không ở trạng thái In Progress" });
                }

                order.Status = "Completed";
                order.CompletedDate = DateTime.Now;
                order.ActualAmount = request.ActualAmount ?? order.EstimatedAmount;
                order.UpdatedAt = DateTime.Now;

                await _unitOfWork.ServiceOrders.UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Completed service order {id}");

                return Ok(new { success = true, message = "Hoàn thành đơn hàng thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error completing order {id}");
                return StatusCode(500, new { success = false, message = "Lỗi khi hoàn thành đơn hàng" });
            }
        }

        /// <summary>
        /// Hủy đơn hàng
        /// </summary>
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id, [FromBody] CancelOrderRequest request)
        {
            try
            {
                var order = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
                if (order == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                if (order.Status == "Completed" || order.Status == "Cancelled")
                {
                    return BadRequest(new { success = false, message = "Không thể hủy đơn hàng đã hoàn thành hoặc đã hủy" });
                }

                order.Status = "Cancelled";
                order.UpdatedAt = DateTime.Now;

                await _unitOfWork.ServiceOrders.UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Cancelled service order {id}. Reason: {request.Reason}");

                return Ok(new { success = true, message = "Hủy đơn hàng thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling order {id}");
                return StatusCode(500, new { success = false, message = "Lỗi khi hủy đơn hàng" });
            }
        }

        /// <summary>
        /// Recalculate order estimated amount
        /// </summary>
        private async Task RecalculateOrderAmount(int orderId)
        {
            var order = await _unitOfWork.ServiceOrders.GetByIdAsync(orderId);
            if (order == null) return;

            // ✅ OPTIMIZED: Filter service items ở database level
            var serviceItems = (await _unitOfWork.Repository<ServiceOrderItem>()
                .FindAsync(i => i.ServiceOrderId == orderId)).ToList();
            var serviceTotal = serviceItems.Sum(i => i.Quantity * i.UnitPrice);

            // ✅ OPTIMIZED: Filter parts ở database level
            var parts = (await _unitOfWork.Repository<ServiceOrderPart>()
                .FindAsync(p => p.ServiceOrderId == orderId)).ToList();
            var partsTotal = parts.Sum(p => p.Quantity * p.UnitPrice);

            order.EstimatedAmount = serviceTotal + partsTotal;
            order.UpdatedAt = DateTime.Now;

            await _unitOfWork.ServiceOrders.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Generate order number: SO-YYYYMM-XXXX
        /// </summary>
        private async Task<string> GenerateOrderNumber()
        {
            var today = DateTime.Now;
            var prefix = $"SO-{today:yyyyMM}-";

            var orders = await _unitOfWork.ServiceOrders.GetAllAsync();
            var maxNumber = orders
                .Where(o => o.OrderNumber.StartsWith(prefix))
                .Select(o =>
                {
                    var numPart = o.OrderNumber.Substring(prefix.Length);
                    return int.TryParse(numPart, out int num) ? num : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            return $"{prefix}{(maxNumber + 1):D4}";
        }
    }

    #region Request Models

    public class CreateServiceOrderRequest
    {
        public int CustomerId { get; set; }
        public int VehicleId { get; set; }
        public int? QuotationId { get; set; }
        public int? InsuranceClaimId { get; set; }
        public string? Description { get; set; }
    }

    public class AddServiceItemRequest
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class AddPartRequest
    {
        public int PartId { get; set; }
        public string PartName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class CompleteOrderRequest
    {
        public decimal? ActualAmount { get; set; }
    }

    public class CancelOrderRequest
    {
        public string? Reason { get; set; }
    }

    #endregion
}

