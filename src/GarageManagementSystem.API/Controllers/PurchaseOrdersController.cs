using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;

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

        public PurchaseOrdersController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PurchaseOrdersController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<PurchaseOrderDto>>>> GetAll([FromQuery] int? supplierId = null, [FromQuery] string? status = null)
        {
            try
            {
                var orders = await _unitOfWork.Repository<PurchaseOrder>().GetAllAsync();
                
                if (supplierId.HasValue)
                    orders = orders.Where(o => o.SupplierId == supplierId.Value);
                
                if (!string.IsNullOrEmpty(status))
                    orders = orders.Where(o => o.Status == status);

                // Map to DTO with supplier information
                var result = new List<PurchaseOrderDto>();
                
                foreach (var order in orders.OrderByDescending(o => o.CreatedAt))
                {
                    // Get supplier information
                    var supplier = await _unitOfWork.Repository<Supplier>().GetByIdAsync(order.SupplierId);
                    
                    // Get purchase order items
                    var items = await _unitOfWork.Repository<PurchaseOrderItem>().GetAllAsync();
                    var orderItems = items.Where(i => i.PurchaseOrderId == order.Id).ToList();
                    
                    var orderDto = _mapper.Map<PurchaseOrderDto>(order);
                    orderDto.SupplierName = supplier?.SupplierName ?? "N/A";
                    orderDto.ItemCount = orderItems.Count;
                    orderDto.Items = _mapper.Map<List<PurchaseOrderItemDto>>(orderItems);
                    
                    result.Add(orderDto);
                }

                return Ok(ApiResponse<List<PurchaseOrderDto>>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting purchase orders");
                return StatusCode(500, ApiResponse<List<PurchaseOrderDto>>.ErrorResult("Lỗi khi lấy danh sách đơn mua hàng", ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> GetById(int id)
        {
            try
            {
                var order = await _unitOfWork.Repository<PurchaseOrder>().GetByIdAsync(id);
                if (order == null)
                    return NotFound(ApiResponse<PurchaseOrderDto>.ErrorResult("Không tìm thấy đơn mua hàng"));

                // Get supplier information
                var supplier = await _unitOfWork.Repository<Supplier>().GetByIdAsync(order.SupplierId);
                
                // Get purchase order items
                var items = await _unitOfWork.Repository<PurchaseOrderItem>().GetAllAsync();
                var orderItems = items.Where(i => i.PurchaseOrderId == order.Id).ToList();
                
                var orderDto = _mapper.Map<PurchaseOrderDto>(order);
                orderDto.SupplierName = supplier?.SupplierName ?? "N/A";
                orderDto.ItemCount = orderItems.Count;
                orderDto.Items = _mapper.Map<List<PurchaseOrderItemDto>>(orderItems);

                return Ok(ApiResponse<PurchaseOrderDto>.SuccessResult(orderDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting purchase order by id: {Id}", id);
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
                return CreatedAtAction(nameof(GetById), new { id = order.Id }, 
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

