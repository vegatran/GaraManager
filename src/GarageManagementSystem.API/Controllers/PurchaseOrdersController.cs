using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Core.Extensions;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using GarageManagementSystem.API.Services;
using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize(Policy = "ApiScope")]
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PurchaseOrdersController> _logger;
        private readonly ICacheService _cacheService;

        public PurchaseOrdersController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PurchaseOrdersController> logger, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _cacheService = cacheService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<PurchaseOrderDto>>> GetAll(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] int? supplierId = null, 
            [FromQuery] string? status = null)
        {
            try
            {
                var purchaseOrders = await _unitOfWork.Repository<PurchaseOrder>().GetAllAsync();
                var query = purchaseOrders.AsQueryable();
                
                if (supplierId.HasValue)
                    query = query.Where(o => o.SupplierId == supplierId.Value);
                
                if (!string.IsNullOrEmpty(status))
                    query = query.Where(o => o.Status == status);

                query = query.OrderByDescending(o => o.CreatedAt);

                // Get total count
                var totalCount = await query.GetTotalCountAsync();
                
                // Apply pagination
                var orders = query.ApplyPagination(pageNumber, pageSize).ToList();
                var purchaseOrderDtos = _mapper.Map<List<PurchaseOrderDto>>(orders);
                
                // OPTIMIZED: Load suppliers and items in batch to avoid N+1 queries
                var orderIds = purchaseOrderDtos.Select(dto => dto.Id).ToList();
                var supplierIds = purchaseOrderDtos.Select(dto => dto.SupplierId).Distinct().ToList();
                
                // Load suppliers from cache (they change rarely)
                var suppliers = await _cacheService.GetOrSetAsync($"suppliers_{string.Join(",", supplierIds)}", 
                    async () =>
                    {
                        var allSuppliers = await _unitOfWork.Repository<Supplier>().GetAllAsync();
                        return allSuppliers.Where(s => supplierIds.Contains(s.Id)).ToList();
                    }, 
                    TimeSpan.FromMinutes(30)); // Cache for 30 minutes
                
                // Load all items for all orders at once
                var allItems = await _unitOfWork.Repository<PurchaseOrderItem>().GetAllAsync();
                var itemsByOrderId = allItems.Where(i => orderIds.Contains(i.PurchaseOrderId))
                                           .GroupBy(i => i.PurchaseOrderId)
                                           .ToDictionary(g => g.Key, g => g.ToList());
                
                // Map supplier names and item counts efficiently
                foreach (var dto in purchaseOrderDtos)
                {
                    var supplier = suppliers.FirstOrDefault(s => s.Id == dto.SupplierId);
                    if (supplier != null)
                    {
                        dto.SupplierName = supplier.SupplierName;
                    }
                    
                    if (itemsByOrderId.TryGetValue(dto.Id, out var orderItems))
                    {
                        dto.ItemCount = orderItems.Count;
                    }
                }

                return Ok(PagedResponse<PurchaseOrderDto>.CreateSuccessResult(
                    purchaseOrderDtos, pageNumber, pageSize, totalCount, "Purchase orders retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting purchase orders");
                return StatusCode(500, PagedResponse<PurchaseOrderDto>.CreateErrorResult("Lỗi khi lấy danh sách đơn mua hàng"));
            }
        }

        [HttpGet("{orderNumber}")]
        public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> GetByOrderNumber(string orderNumber)
        {
            try
            {
                var orders = await _unitOfWork.Repository<PurchaseOrder>().GetAllAsync();
                var order = orders.FirstOrDefault(o => o.OrderNumber == orderNumber);
                
                if (order == null)
                    return NotFound(ApiResponse<PurchaseOrderDto>.ErrorResult("Không tìm thấy đơn mua hàng"));

                // Get supplier information
                var supplier = await _unitOfWork.Repository<Supplier>().GetByIdAsync(order.SupplierId);
                
                // Get purchase order items
                var items = await _unitOfWork.Repository<PurchaseOrderItem>().GetAllAsync();
                var orderItems = items.Where(i => i.PurchaseOrderId == order.Id).ToList();
                
                _logger.LogInformation($"Purchase Order {orderNumber} has {orderItems.Count} items");
                
                var orderDto = _mapper.Map<PurchaseOrderDto>(order);
                orderDto.SupplierName = supplier?.SupplierName ?? "N/A";
                orderDto.ItemCount = orderItems.Count;
                orderDto.Items = _mapper.Map<List<PurchaseOrderItemDto>>(orderItems);
                
                _logger.LogInformation($"Mapped {orderDto.Items.Count} items to DTO");

                return Ok(ApiResponse<PurchaseOrderDto>.SuccessResult(orderDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting purchase order by order number: {OrderNumber}", orderNumber);
                return StatusCode(500, ApiResponse<PurchaseOrderDto>.ErrorResult("Lỗi khi lấy thông tin đơn mua hàng", ex.Message));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> Create([FromBody] CreatePurchaseOrderDto createDto)
        {
            try
            {
                var order = _mapper.Map<PurchaseOrder>(createDto);
                
                // Generate PO number
                var count = (await _unitOfWork.Repository<PurchaseOrder>().GetAllAsync()).Count();
                order.OrderNumber = $"PO-{DateTime.Now:yyyyMMdd}-{(count + 1):D4}";
                order.CreatedAt = DateTime.Now;
                
                await _unitOfWork.Repository<PurchaseOrder>().AddAsync(order);
                await _unitOfWork.SaveChangesAsync();

                var orderDto = _mapper.Map<PurchaseOrderDto>(order);
                return CreatedAtAction(nameof(GetByOrderNumber), new { orderNumber = order.OrderNumber }, 
                    ApiResponse<PurchaseOrderDto>.SuccessResult(orderDto, "Tạo đơn mua hàng thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating purchase order");
                return StatusCode(500, ApiResponse<PurchaseOrderDto>.ErrorResult("Lỗi khi tạo đơn mua hàng", ex.Message));
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

        [HttpPut("{id}/send")]
        public async Task<IActionResult> SendOrder(int id)
        {
            try
            {
                var order = await _unitOfWork.Repository<PurchaseOrder>().GetByIdAsync(id);
                if (order == null)
                    return NotFound(new { success = false, message = "Không tìm thấy đơn mua hàng" });

                if (order.Status != "Draft")
                    return BadRequest(new { success = false, message = "Chỉ có thể gửi PO ở trạng thái Draft" });

                order.Status = "Sent";
                order.SentDate = DateTime.Now;
                order.UpdatedAt = DateTime.Now;

                await _unitOfWork.Repository<PurchaseOrder>().UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = order, message = "Đã gửi PO cho supplier" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending purchase order");
                return StatusCode(500, new { success = false, message = "Lỗi khi gửi PO" });
            }
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id, [FromBody] CancelOrderDto dto)
        {
            try
            {
                var order = await _unitOfWork.Repository<PurchaseOrder>().GetByIdAsync(id);
                if (order == null)
                    return NotFound(new { success = false, message = "Không tìm thấy đơn mua hàng" });

                if (order.Status == "Received")
                    return BadRequest(new { success = false, message = "Không thể hủy PO đã nhận hàng" });

                order.Status = "Cancelled";
                order.CancelledDate = DateTime.Now;
                order.CancelReason = dto.Reason;
                order.CancelledBy = User.Identity?.Name ?? "System";
                order.UpdatedAt = DateTime.Now;

                await _unitOfWork.Repository<PurchaseOrder>().UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = order, message = "Đã hủy PO thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling purchase order");
                return StatusCode(500, new { success = false, message = "Lỗi khi hủy PO" });
            }
        }

        [HttpPost("update-status-legacy")]
        public async Task<IActionResult> UpdateLegacyStatus()
        {
            try
            {
                var orders = await _unitOfWork.Repository<PurchaseOrder>().GetAllAsync();
                var updatedCount = 0;
                
                foreach (var order in orders)
                {
                    if (order.Status == "Pending" || order.Status == "Không hoạt động" || string.IsNullOrEmpty(order.Status))
                    {
                        order.Status = "Draft";
                        order.UpdatedAt = DateTime.Now;
                        await _unitOfWork.Repository<PurchaseOrder>().UpdateAsync(order);
                        updatedCount++;
                    }
                }
                
                await _unitOfWork.SaveChangesAsync();
                
                return Ok(new { success = true, message = $"Đã cập nhật {updatedCount} Purchase Orders", updatedCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating legacy status");
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật status" });
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

                if (order.Status != "Sent")
                    return BadRequest(new { success = false, message = "Chỉ có thể nhận hàng PO đã được gửi" });

                order.Status = "Received";
                order.ReceivedDate = DateTime.Now;
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

    /// <summary>
    /// DTO để hủy Purchase Order
    /// </summary>
    public class CancelOrderDto
    {
        [Required(ErrorMessage = "Lý do hủy là bắt buộc")]
        [StringLength(500, ErrorMessage = "Lý do hủy không được vượt quá 500 ký tự")]
        public string Reason { get; set; } = string.Empty;
    }
}

