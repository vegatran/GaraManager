using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Shared.DTOs;

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

                // Map to DTO with supplier information
                var result = new List<PurchaseOrderDto>();
                
                foreach (var order in orders.OrderByDescending(o => o.CreatedAt))
                {
                    // Get supplier information
                    var supplier = await _unitOfWork.Repository<Supplier>().GetByIdAsync(order.SupplierId);
                    
                    // Get purchase order items
                    var items = await _unitOfWork.Repository<PurchaseOrderItem>().GetAllAsync();
                    var orderItems = items.Where(i => i.PurchaseOrderId == order.Id).ToList();
                    
                    var orderDto = new PurchaseOrderDto
                    {
                        Id = order.Id,
                        OrderNumber = order.OrderNumber,
                        SupplierId = order.SupplierId,
                        SupplierName = supplier?.SupplierName ?? "N/A",
                        OrderDate = order.OrderDate,
                        ExpectedDeliveryDate = order.ExpectedDeliveryDate,
                        ActualDeliveryDate = order.ActualDeliveryDate,
                        Status = order.Status,
                        SupplierOrderNumber = order.SupplierOrderNumber,
                        ContactPerson = order.ContactPerson,
                        ContactPhone = order.ContactPhone,
                        ContactEmail = order.ContactEmail,
                        DeliveryAddress = order.DeliveryAddress,
                        PaymentTerms = order.PaymentTerms,
                        DeliveryTerms = order.DeliveryTerms,
                        Currency = order.Currency,
                        SubTotal = order.SubTotal,
                        TaxAmount = order.TaxAmount,
                        ShippingCost = order.ShippingCost,
                        TotalAmount = order.TotalAmount,
                        EmployeeId = order.EmployeeId,
                        ApprovedBy = order.ApprovedBy,
                        ApprovedDate = order.ApprovedDate,
                        Notes = order.Notes,
                        IsApproved = order.IsApproved,
                        ItemCount = orderItems.Count,
                        Items = orderItems.Select(item => new PurchaseOrderItemDto
                        {
                            Id = item.Id,
                            PurchaseOrderId = item.PurchaseOrderId,
                            PartId = item.PartId,
                            PartName = item.PartName,
                            QuantityOrdered = item.QuantityOrdered,
                            QuantityReceived = item.QuantityReceived,
                            UnitPrice = item.UnitPrice,
                            TotalPrice = item.TotalPrice,
                            SupplierPartNumber = item.SupplierPartNumber,
                            PartDescription = item.PartDescription,
                            Unit = item.Unit,
                            ExpectedDeliveryDate = item.ExpectedDeliveryDate,
                            Notes = item.Notes,
                            IsReceived = item.IsReceived,
                            ReceivedDate = item.ReceivedDate,
                            CreatedAt = item.CreatedAt,
                            CreatedBy = item.CreatedBy,
                            UpdatedAt = item.UpdatedAt,
                            UpdatedBy = item.UpdatedBy
                        }).ToList(),
                        CreatedAt = order.CreatedAt,
                        CreatedBy = order.CreatedBy,
                        UpdatedAt = order.UpdatedAt,
                        UpdatedBy = order.UpdatedBy
                    };
                    
                    result.Add(orderDto);
                }

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

                // Get supplier information
                var supplier = await _unitOfWork.Repository<Supplier>().GetByIdAsync(order.SupplierId);
                
                // Get purchase order items
                var items = await _unitOfWork.Repository<PurchaseOrderItem>().GetAllAsync();
                var orderItems = items.Where(i => i.PurchaseOrderId == order.Id).ToList();
                
                var orderDto = new PurchaseOrderDto
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    SupplierId = order.SupplierId,
                    SupplierName = supplier?.SupplierName ?? "N/A",
                    OrderDate = order.OrderDate,
                    ExpectedDeliveryDate = order.ExpectedDeliveryDate,
                    ActualDeliveryDate = order.ActualDeliveryDate,
                    Status = order.Status,
                    SupplierOrderNumber = order.SupplierOrderNumber,
                    ContactPerson = order.ContactPerson,
                    ContactPhone = order.ContactPhone,
                    ContactEmail = order.ContactEmail,
                    DeliveryAddress = order.DeliveryAddress,
                    PaymentTerms = order.PaymentTerms,
                    DeliveryTerms = order.DeliveryTerms,
                    Currency = order.Currency,
                    SubTotal = order.SubTotal,
                    TaxAmount = order.TaxAmount,
                    ShippingCost = order.ShippingCost,
                    TotalAmount = order.TotalAmount,
                    EmployeeId = order.EmployeeId,
                    ApprovedBy = order.ApprovedBy,
                    ApprovedDate = order.ApprovedDate,
                    Notes = order.Notes,
                    IsApproved = order.IsApproved,
                    ItemCount = orderItems.Count,
                    Items = orderItems.Select(item => new PurchaseOrderItemDto
                    {
                        Id = item.Id,
                        PurchaseOrderId = item.PurchaseOrderId,
                        PartId = item.PartId,
                        PartName = item.PartName,
                        QuantityOrdered = item.QuantityOrdered,
                        QuantityReceived = item.QuantityReceived,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice,
                        SupplierPartNumber = item.SupplierPartNumber,
                        PartDescription = item.PartDescription,
                        Unit = item.Unit,
                        ExpectedDeliveryDate = item.ExpectedDeliveryDate,
                        Notes = item.Notes,
                        IsReceived = item.IsReceived,
                        ReceivedDate = item.ReceivedDate,
                        CreatedAt = item.CreatedAt,
                        CreatedBy = item.CreatedBy,
                        UpdatedAt = item.UpdatedAt,
                        UpdatedBy = item.UpdatedBy
                    }).ToList(),
                    CreatedAt = order.CreatedAt,
                    CreatedBy = order.CreatedBy,
                    UpdatedAt = order.UpdatedAt,
                    UpdatedBy = order.UpdatedBy
                };

                return Ok(new { success = true, data = orderDto });
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

