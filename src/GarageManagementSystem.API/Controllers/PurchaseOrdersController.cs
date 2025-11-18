using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Core.Extensions;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using GarageManagementSystem.API.Services;
using GarageManagementSystem.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

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
        private readonly GarageDbContext _context;

        public PurchaseOrdersController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PurchaseOrdersController> logger, ICacheService cacheService, GarageDbContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _cacheService = cacheService;
            _context = context;
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
                // ✅ OPTIMIZED: Query ở database level thay vì load tất cả vào memory
                var query = _context.PurchaseOrders
                    .Where(o => !o.IsDeleted)
                    .AsQueryable();
                
                if (supplierId.HasValue)
                    query = query.Where(o => o.SupplierId == supplierId.Value);
                
                if (!string.IsNullOrEmpty(status))
                    query = query.Where(o => o.Status == status);

                query = query.OrderByDescending(o => o.CreatedAt);

                // ✅ OPTIMIZED: Get total count ở database level (trước khi paginate)
                var totalCount = await query.CountAsync();
                
                // ✅ OPTIMIZED: Apply pagination ở database level với Skip/Take
                var orders = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                
                var purchaseOrderDtos = _mapper.Map<List<PurchaseOrderDto>>(orders);
                
                // OPTIMIZED: Load suppliers and items in batch to avoid N+1 queries
                var orderIds = purchaseOrderDtos.Select(dto => dto.Id).ToList();
                var supplierIds = purchaseOrderDtos.Select(dto => dto.SupplierId).Distinct().ToList();
                
                // ✅ OPTIMIZED: Load suppliers from cache (they change rarely) - filter ở database level
                var suppliers = await _cacheService.GetOrSetAsync($"suppliers_{string.Join(",", supplierIds)}", 
                    async () =>
                    {
                        var filteredSuppliers = (await _unitOfWork.Repository<Supplier>()
                            .FindAsync(s => supplierIds.Contains(s.Id))).ToList();
                        return filteredSuppliers;
                    }, 
                    TimeSpan.FromMinutes(30)); // Cache for 30 minutes
                
                // ✅ OPTIMIZED: Filter items ở database level thay vì load all
                var allItems = (await _unitOfWork.Repository<PurchaseOrderItem>()
                    .FindAsync(i => orderIds.Contains(i.PurchaseOrderId))).ToList();
                var itemsByOrderId = allItems
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
                // ✅ OPTIMIZED: Filter ở database level thay vì load all rồi filter trong memory
                var order = await _unitOfWork.Repository<PurchaseOrder>()
                    .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
                
                if (order == null)
                    return NotFound(ApiResponse<PurchaseOrderDto>.ErrorResult("Không tìm thấy đơn mua hàng"));

                // Get supplier information
                var supplier = await _unitOfWork.Repository<Supplier>().GetByIdAsync(order.SupplierId);
                
                // ✅ OPTIMIZED: Filter items ở database level
                var orderItems = (await _unitOfWork.Repository<PurchaseOrderItem>()
                    .FindAsync(i => i.PurchaseOrderId == order.Id)).ToList();
                
                _logger.LogInformation($"Purchase Order {orderNumber} has {orderItems.Count} items");
                
                // ✅ OPTIMIZED: Load only parts that are needed (filter ở database level)
                var partsDict = new Dictionary<int, Part>();
                if (orderItems.Any())
                {
                    var partIds = orderItems
                        .Select(i => i.PartId)
                        .Distinct()
                        .ToList();
                    
                    if (partIds.Any())
                    {
                        var parts = (await _unitOfWork.Repository<Part>()
                            .FindAsync(p => partIds.Contains(p.Id))).ToList();
                        partsDict = parts.ToDictionary(p => p.Id, p => p);
                    }
                }
                
                var orderDto = _mapper.Map<PurchaseOrderDto>(order);
                orderDto.SupplierName = supplier?.SupplierName ?? "N/A";
                orderDto.ItemCount = orderItems.Count;
                orderDto.Items = orderItems.Select(item => new PurchaseOrderItemDto
                {
                    Id = item.Id,
                    PartId = item.PartId,
                    PartName = partsDict.ContainsKey(item.PartId) 
                        ? partsDict[item.PartId].PartName 
                        : null,
                    QuantityOrdered = item.QuantityOrdered,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.QuantityOrdered * item.UnitPrice,
                    VATRate = item.VATRate,
                    VATAmount = item.VATAmount,
                    Notes = item.Notes
                }).ToList();
                
                _logger.LogInformation($"Mapped {orderDto.Items.Count} items to DTO");

                return Ok(ApiResponse<PurchaseOrderDto>.SuccessResult(orderDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting purchase order by order number: {OrderNumber}", orderNumber);
                return StatusCode(500, ApiResponse<PurchaseOrderDto>.ErrorResult("Lỗi khi lấy thông tin đơn mua hàng", ex.Message));
            }
        }

        [HttpGet("by-id/{id}")]
        public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> GetById(int id)
        {
            try
            {
                var order = await _unitOfWork.Repository<PurchaseOrder>().GetByIdAsync(id);
                
                if (order == null)
                    return NotFound(ApiResponse<PurchaseOrderDto>.ErrorResult("Không tìm thấy đơn mua hàng"));

                // Get supplier information
                var supplier = await _unitOfWork.Repository<Supplier>().GetByIdAsync(order.SupplierId);
                
                // ✅ OPTIMIZED: Filter purchase order items ở database level
                var orderItems = (await _unitOfWork.Repository<PurchaseOrderItem>()
                    .FindAsync(i => i.PurchaseOrderId == order.Id)).ToList();

                // ✅ OPTIMIZED: Load only parts that are needed
                var partsDict = new Dictionary<int, Part>();
                if (orderItems.Any())
                {
                    var partIds = orderItems
                        .Select(i => i.PartId)
                        .Distinct()
                        .ToList();
                    
                    if (partIds.Any())
                    {
                        var parts = (await _unitOfWork.Repository<Part>()
                            .FindAsync(p => partIds.Contains(p.Id))).ToList();
                        partsDict = parts.ToDictionary(p => p.Id, p => p);
                    }
                }

                // Map to DTO
                var orderDto = new PurchaseOrderDto
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    OrderDate = order.OrderDate,
                    SupplierId = order.SupplierId,
                    SupplierName = supplier?.SupplierName ?? "N/A",
                    TotalAmount = order.TotalAmount,
                    ItemCount = orderItems.Count,
                    Status = order.Status,
                    Notes = order.Notes,
                    ExpectedDeliveryDate = order.ExpectedDeliveryDate,
                    PaymentTerms = order.PaymentTerms,
                    DeliveryAddress = order.DeliveryAddress,
                    VATRate = order.VATRate,
                    Items = orderItems.Select(item => new PurchaseOrderItemDto
                    {
                        Id = item.Id,
                        PartId = item.PartId,
                        PartName = partsDict.ContainsKey(item.PartId) 
                        ? partsDict[item.PartId].PartName 
                        : null,
                        QuantityOrdered = item.QuantityOrdered,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.QuantityOrdered * item.UnitPrice,
                        VATRate = item.VATRate,
                        VATAmount = item.VATAmount,
                        Notes = item.Notes
                    }).ToList()
                };
                
                _logger.LogInformation($"Purchase Order {order.OrderNumber} - Items: {orderDto.Items.Count}, First item QuantityOrdered: {orderDto.Items.FirstOrDefault()?.QuantityOrdered}");

                return Ok(ApiResponse<PurchaseOrderDto>.SuccessResult(orderDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting purchase order by ID");
                return StatusCode(500, ApiResponse<PurchaseOrderDto>.ErrorResult("Lỗi khi lấy thông tin đơn mua hàng"));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> Create([FromBody] CreatePurchaseOrderDto createDto)
        {
            try
            {
                var order = _mapper.Map<PurchaseOrder>(createDto);
                
                // ✅ OPTIMIZED: Use CountAsync thay vì GetAllAsync().Count()
                var count = await _unitOfWork.Repository<PurchaseOrder>().CountAsync();
                order.OrderNumber = $"PO-{DateTime.Now:yyyyMMdd}-{(count + 1):D4}";
                order.CreatedAt = DateTime.Now;
                
                // Calculate totals from items
                if (createDto.Items != null && createDto.Items.Any())
                {
                    order.SubTotal = createDto.Items.Sum(item => item.QuantityOrdered * item.UnitPrice);
                    order.TaxAmount = createDto.Items.Sum(item => item.VATAmount);
                    order.TotalAmount = order.SubTotal + order.TaxAmount;
                    
                    // Calculate average VAT rate
                    if (order.SubTotal > 0)
                    {
                        order.VATRate = (order.TaxAmount / order.SubTotal) * 100;
                    }
                }
                
                await _unitOfWork.Repository<PurchaseOrder>().AddAsync(order);
                await _unitOfWork.SaveChangesAsync();

                // Reload order with all related data for complete mapping
                var savedOrder = await _unitOfWork.Repository<PurchaseOrder>().GetByIdAsync(order.Id);
                var orderItems = (await _unitOfWork.Repository<PurchaseOrderItem>()
                    .FindAsync(i => i.PurchaseOrderId == order.Id)).ToList();
                var orderSupplier = await _unitOfWork.Repository<Supplier>().GetByIdAsync(order.SupplierId);
                
                var orderDto = _mapper.Map<PurchaseOrderDto>(savedOrder);
                orderDto.SupplierName = orderSupplier?.SupplierName ?? "N/A";
                orderDto.ItemCount = orderItems.Count;
                orderDto.Items = orderItems.Select(item => _mapper.Map<PurchaseOrderItemDto>(item)).ToList();
                
                return CreatedAtAction(nameof(GetByOrderNumber), new { orderNumber = order.OrderNumber }, 
                    ApiResponse<PurchaseOrderDto>.SuccessResult(orderDto, "Tạo đơn mua hàng thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating purchase order");
                return StatusCode(500, ApiResponse<PurchaseOrderDto>.ErrorResult("Lỗi khi tạo đơn mua hàng", ex.Message));
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePurchaseOrder(int id, [FromBody] UpdatePurchaseOrderDto dto)
        {
            try
            {
                _logger.LogInformation($"UpdatePurchaseOrder called with ID: {id}");
                
                if (dto == null)
                {
                    _logger.LogWarning("UpdatePurchaseOrderDto is null");
                    return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });
                }
                
                _logger.LogInformation($"UpdatePurchaseOrderDto - SupplierId: {dto.SupplierId}, Items Count: {dto.Items?.Count ?? 0}");
                
                var order = await _unitOfWork.Repository<PurchaseOrder>().GetByIdAsync(id);
                if (order == null)
                {
                    _logger.LogWarning($"Purchase order with ID {id} not found");
                    return NotFound(new { success = false, message = "Không tìm thấy đơn mua hàng" });
                }

                if (order.Status != "Draft")
                {
                    _logger.LogWarning($"Purchase order {id} is not in Draft status. Current status: {order.Status}");
                    return BadRequest(new { success = false, message = "Chỉ có thể chỉnh sửa PO ở trạng thái Draft" });
                }

                // Update order properties
                order.SupplierId = dto.SupplierId;
                order.OrderDate = dto.OrderDate;
                order.ExpectedDeliveryDate = dto.ExpectedDeliveryDate;
                order.PaymentTerms = dto.PaymentTerms;
                order.DeliveryAddress = dto.DeliveryAddress;
                order.Notes = dto.Notes;
                order.UpdatedAt = DateTime.Now;

                // ✅ OPTIMIZED: Filter existing items ở database level
                var itemsToDelete = (await _unitOfWork.Repository<PurchaseOrderItem>()
                    .FindAsync(x => x.PurchaseOrderId == id)).ToList();
                
                foreach (var item in itemsToDelete)
                {
                    await _unitOfWork.Repository<PurchaseOrderItem>().DeleteAsync(item);
                }

                // Add new items and calculate totals
                decimal subtotal = 0;
                decimal totalVatAmount = 0;
                
                foreach (var itemDto in dto.Items)
                {
                    var item = new PurchaseOrderItem
                    {
                        PurchaseOrderId = id,
                        PartId = itemDto.PartId,
                        QuantityOrdered = itemDto.QuantityOrdered,
                        UnitPrice = itemDto.UnitPrice,
                        VATRate = itemDto.VATRate,
                        VATAmount = itemDto.VATAmount,
                        Notes = itemDto.Notes,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    
                    await _unitOfWork.Repository<PurchaseOrderItem>().AddAsync(item);
                    
                    subtotal += itemDto.QuantityOrdered * itemDto.UnitPrice;
                    totalVatAmount += itemDto.VATAmount;
                }

                // Update totals
                order.SubTotal = subtotal;
                order.TaxAmount = totalVatAmount;
                order.TotalAmount = subtotal + totalVatAmount;
                
                // Calculate average VAT rate
                if (subtotal > 0)
                {
                    order.VATRate = (totalVatAmount / subtotal) * 100;
                }
                // Note: ItemCount is calculated property, no need to set it

                await _unitOfWork.Repository<PurchaseOrder>().UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync();

                // Get supplier information
                var supplier = await _unitOfWork.Repository<Supplier>().GetByIdAsync(order.SupplierId);
                
                // ✅ OPTIMIZED: Filter updated items ở database level
                var orderItems = (await _unitOfWork.Repository<PurchaseOrderItem>()
                    .FindAsync(i => i.PurchaseOrderId == order.Id)).ToList();
                
                // ✅ OPTIMIZED: Load only parts that are needed
                var partsDict = new Dictionary<int, Part>();
                if (orderItems.Any())
                {
                    var partIds = orderItems
                        .Select(i => i.PartId)
                        .Distinct()
                        .ToList();
                    
                    if (partIds.Any())
                    {
                        var parts = (await _unitOfWork.Repository<Part>()
                            .FindAsync(p => partIds.Contains(p.Id))).ToList();
                        partsDict = parts.ToDictionary(p => p.Id, p => p);
                    }
                }

                // Return DTO instead of entity to avoid circular references
                var responseDto = new PurchaseOrderDto
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    OrderDate = order.OrderDate,
                    SupplierId = order.SupplierId,
                    SupplierName = supplier?.SupplierName ?? "N/A",
                    TotalAmount = order.TotalAmount,
                    ItemCount = orderItems.Count,
                    Status = order.Status,
                    Notes = order.Notes,
                    ExpectedDeliveryDate = order.ExpectedDeliveryDate,
                    PaymentTerms = order.PaymentTerms,
                    DeliveryAddress = order.DeliveryAddress,
                    VATRate = order.VATRate,
                    Items = orderItems.Select(item => new PurchaseOrderItemDto
                    {
                        Id = item.Id,
                        PartId = item.PartId,
                        PartName = partsDict.ContainsKey(item.PartId) 
                        ? partsDict[item.PartId].PartName 
                        : null,
                        QuantityOrdered = item.QuantityOrdered,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.QuantityOrdered * item.UnitPrice,
                        VATRate = item.VATRate,
                        VATAmount = item.VATAmount,
                        Notes = item.Notes
                    }).ToList()
                };

                return Ok(new { success = true, data = responseDto, message = "Đã cập nhật PO thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating purchase order");
                return StatusCode(500, new { success = false, message = $"Lỗi khi cập nhật PO: {ex.Message}", error = ex.Message });
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
            // ✅ FIX: Wrap toàn bộ operation trong transaction để đảm bảo data consistency
            await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                var order = await _unitOfWork.Repository<PurchaseOrder>().GetByIdAsync(id);
                if (order == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return NotFound(new { success = false, message = "Không tìm thấy đơn mua hàng" });
                }

                if (order.Status != "Sent")
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return BadRequest(new { success = false, message = "Chỉ có thể nhận hàng PO đã được gửi" });
                }

                // ✅ FIX: Validate order có items
                var orderItems = (await _unitOfWork.Repository<PurchaseOrderItem>()
                    .FindAsync(i => i.PurchaseOrderId == id)).ToList();
                
                if (orderItems == null || !orderItems.Any())
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return BadRequest(new { success = false, message = "Đơn mua hàng không có vật tư nào" });
                }

                // ✅ FIX: Validate tất cả items có QuantityOrdered > 0
                var invalidItems = orderItems.Where(i => i.QuantityOrdered <= 0).ToList();
                if (invalidItems.Any())
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return BadRequest(new { success = false, message = $"Có {invalidItems.Count} vật tư có số lượng đặt hàng <= 0" });
                }

                order.Status = "Received";
                order.ReceivedDate = DateTime.Now;
                order.ActualDeliveryDate = DateTime.Now;
                order.UpdatedAt = DateTime.Now;

                await _unitOfWork.Repository<PurchaseOrder>().UpdateAsync(order);

                // Get supplier info
                var supplier = await _unitOfWork.Repository<Supplier>().GetByIdAsync(order.SupplierId);
                
                // ✅ FIX: Pre-load parts và validate tất cả parts tồn tại
                var partIds = orderItems.Select(i => i.PartId).Distinct().ToList();
                var parts = await _unitOfWork.Repository<Part>()
                    .FindAsync(p => partIds.Contains(p.Id));
                var partsDict = parts.ToDictionary(p => p.Id, p => p);

                // ✅ FIX: Validate tất cả parts tồn tại
                var missingParts = orderItems.Where(i => !partsDict.ContainsKey(i.PartId)).ToList();
                if (missingParts.Any())
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return BadRequest(new { success = false, message = $"Không tìm thấy {missingParts.Count} phụ tùng trong hệ thống" });
                }

                // ✅ FIX: Collect parts to update (tránh update nhiều lần cùng part)
                var partsToUpdate = new Dictionary<int, Core.Entities.Part>();
                var stockTransactions = new List<Core.Entities.StockTransaction>();

                // Generate transaction numbers
                var txCount = await _unitOfWork.Repository<StockTransaction>().CountAsync() + 1;
                var dateStr = DateTime.Now.ToString("yyyyMMdd");
                
                // Create Stock Transactions for each item
                foreach (var item in orderItems)
                {
                    // ✅ FIX: Validate QuantityOrdered > 0 (đã validate ở trên, nhưng defensive check)
                    if (item.QuantityOrdered <= 0)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return BadRequest(new { success = false, message = $"Số lượng đặt hàng phải > 0 cho phụ tùng ID {item.PartId}" });
                    }

                    var part = partsDict[item.PartId];
                    
                    // ✅ FIX: Sử dụng part.QuantityInStock trực tiếp thay vì tính từ transactions (chính xác hơn)
                    var quantityBefore = part.QuantityInStock;
                    var quantityAfter = quantityBefore + item.QuantityOrdered;
                    
                    // ✅ FIX: Validate quantityAfter không overflow (defensive check)
                    if (quantityAfter < quantityBefore) // Overflow check
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return BadRequest(new { success = false, message = $"Số lượng sau nhập kho vượt quá giới hạn cho phụ tùng {part.PartName ?? part.PartNumber}" });
                    }

                    // ✅ FIX: Collect part để update một lần (tránh update nhiều lần cùng part)
                    if (!partsToUpdate.ContainsKey(part.Id))
                    {
                        // ✅ FIX: Reload part từ database để lấy quantity mới nhất (tránh race condition)
                        var freshPart = await _unitOfWork.Repository<Part>().GetByIdAsync(part.Id);
                        if (freshPart == null || freshPart.IsDeleted)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return BadRequest(new { success = false, message = $"Phụ tùng ID {part.Id} không tồn tại hoặc đã bị xóa" });
                        }
                        partsToUpdate[part.Id] = freshPart;
                        quantityBefore = freshPart.QuantityInStock;
                        quantityAfter = quantityBefore + item.QuantityOrdered;
                    }
                    else
                    {
                        // ✅ FIX: Nếu đã có part trong dictionary, cộng dồn quantity (nhiều items cùng part)
                        var existingPart = partsToUpdate[part.Id];
                        quantityBefore = existingPart.QuantityInStock; // Lấy từ part đã được update trong loop trước
                        quantityAfter = quantityBefore + item.QuantityOrdered; // Cộng dồn
                    }
                    
                    // Create Stock Transaction
                    var stockTx = new Core.Entities.StockTransaction
                    {
                        TransactionNumber = $"STK-{dateStr}-{txCount++:D4}",
                        TransactionType = Core.Enums.StockTransactionType.NhapKho,
                        PartId = item.PartId,
                        Quantity = item.QuantityOrdered,
                        QuantityBefore = quantityBefore,
                        QuantityAfter = quantityAfter,
                        StockAfter = quantityAfter, // ✅ FIX: Set StockAfter
                        UnitCost = item.UnitPrice,
                        UnitPrice = part.SellPrice,
                        TotalCost = item.QuantityOrdered * item.UnitPrice,
                        TotalAmount = item.QuantityOrdered * part.SellPrice,
                        TransactionDate = DateTime.Now,
                        SupplierId = order.SupplierId,
                        SupplierName = supplier?.SupplierName,
                        RelatedEntity = "PurchaseOrder",
                        RelatedEntityId = order.Id,
                        Notes = $"Nhập hàng từ PO: {order.OrderNumber}",
                        Condition = "New",
                        SourceType = "Purchased"
                    };
                    
                    stockTransactions.Add(stockTx);
                    
                    // ✅ FIX: Update part quantity (nếu có nhiều items cùng part, chỉ update 1 lần với giá trị cuối cùng)
                    partsToUpdate[part.Id].QuantityInStock = quantityAfter;
                    partsToUpdate[part.Id].UpdatedAt = DateTime.Now;
                }

                // ✅ FIX: Update parts một lần (tránh update nhiều lần cùng part)
                foreach (var part in partsToUpdate.Values)
                {
                    await _unitOfWork.Repository<Part>().UpdateAsync(part);
                }

                // ✅ FIX: Add stock transactions
                foreach (var stockTx in stockTransactions)
                {
                    await _unitOfWork.Repository<StockTransaction>().AddAsync(stockTx);
                }

                // ✅ FIX: Update PartSupplier
                foreach (var item in orderItems)
                {
                    var partSupplier = await _unitOfWork.Repository<PartSupplier>()
                        .FirstOrDefaultAsync(ps => ps.PartId == item.PartId && ps.SupplierId == order.SupplierId);
                    
                    if (partSupplier != null)
                    {
                        partSupplier.LastOrderDate = order.OrderDate;
                        partSupplier.LastCostPrice = item.UnitPrice;
                        partSupplier.UpdatedAt = DateTime.Now;
                        await _unitOfWork.Repository<PartSupplier>().UpdateAsync(partSupplier);
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                // Create Financial Transaction (Expense)
                var finCount = await _unitOfWork.Repository<Core.Entities.FinancialTransaction>().CountAsync() + 1;
                var finNumber = $"FIN-{dateStr}-{finCount:D4}";
                
                var financialTx = new Core.Entities.FinancialTransaction
                {
                    TransactionNumber = finNumber,
                    TransactionType = "Expense",
                    Category = "Parts Purchase",
                    SubCategory = "Purchase Order",
                    Amount = order.TotalAmount,
                    Currency = order.Currency,
                    TransactionDate = DateTime.Now,
                    PaymentMethod = order.PaymentTerms ?? "Cash",
                    ReferenceNumber = order.OrderNumber,
                    Description = $"Chi mua phụ tùng từ nhà cung cấp: {supplier?.SupplierName}",
                    RelatedEntity = "PurchaseOrder",
                    RelatedEntityId = order.Id,
                    EmployeeId = order.EmployeeId,
                    Notes = $"Thanh toán cho PO: {order.OrderNumber}",
                    Status = "Pending", // Chưa thanh toán thực tế
                    IsApproved = false
                };
                
                await _unitOfWork.Repository<Core.Entities.FinancialTransaction>().AddAsync(financialTx);
                await _unitOfWork.SaveChangesAsync();

                // ✅ FIX: Commit transaction sau khi tất cả operations thành công
                await _unitOfWork.CommitTransactionAsync();

                return Ok(new { success = true, data = order, message = "Đã nhận hàng, nhập kho và tạo phiếu chi" });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error receiving purchase order");
                return StatusCode(500, new { success = false, message = "Lỗi khi nhận hàng" });
            }
        }

        #region Phase 4.2.3: PO Tracking Endpoints

        /// <summary>
        /// GET /api/purchase-orders/in-transit
        /// Lấy danh sách PO đang chờ giao hàng (Sent hoặc InTransit)
        /// </summary>
        [HttpGet("in-transit")]
        public async Task<ActionResult<PagedResponse<PurchaseOrderDto>>> GetInTransitOrders(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? supplierId = null,
            [FromQuery] string? deliveryStatus = null,
            [FromQuery] int? daysUntilDelivery = null)
        {
            try
            {
                // ✅ SAFETY: Validate pagination parameters
                if (pageSize <= 0) pageSize = 20;
                if (pageNumber <= 0) pageNumber = 1;
                if (pageSize > 100) pageSize = 100; // Giới hạn max để tránh performance issues

                var today = DateTime.Now.Date;
                
                // ✅ OPTIMIZED: Query ở database level
                var query = _context.PurchaseOrders
                    .AsNoTracking()
                    .Include(po => po.Supplier)
                    .Where(po => !po.IsDeleted && 
                                (po.Status == "Sent" || po.Status == "InTransit"))
                    .AsQueryable();

                if (supplierId.HasValue)
                    query = query.Where(po => po.SupplierId == supplierId.Value);

                // Calculate delivery status và filter
                var orders = await query
                    .OrderByDescending(po => po.ExpectedDeliveryDate ?? po.OrderDate)
                    .ToListAsync();

                // Calculate delivery status for each order
                var ordersWithStatus = orders.Select(po =>
                {
                    var calculatedStatus = CalculateDeliveryStatus(po, today);
                    return new { Order = po, DeliveryStatus = calculatedStatus };
                }).ToList();

                // Filter by delivery status if provided
                if (!string.IsNullOrEmpty(deliveryStatus))
                {
                    ordersWithStatus = ordersWithStatus
                        .Where(x => x.DeliveryStatus.Equals(deliveryStatus, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                // Filter by days until delivery if provided
                if (daysUntilDelivery.HasValue)
                {
                    ordersWithStatus = ordersWithStatus
                        .Where(x =>
                        {
                            // ✅ SAFETY: Null check
                            if (x?.Order == null) return false;
                            if (x.Order.ExpectedDeliveryDate.HasValue)
                            {
                                var daysUntil = (x.Order.ExpectedDeliveryDate.Value.Date - today).Days;
                                return daysUntil <= daysUntilDelivery.Value;
                            }
                            return false;
                        })
                        .ToList();
                }

                var totalCount = ordersWithStatus.Count;

                // Apply pagination
                var pagedOrders = ordersWithStatus
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => x.Order)
                    .ToList();

                var purchaseOrderDtos = _mapper.Map<List<PurchaseOrderDto>>(pagedOrders);

                // Update delivery status in DTOs
                foreach (var dto in purchaseOrderDtos)
                {
                    var order = pagedOrders.FirstOrDefault(o => o.Id == dto.Id);
                    if (order != null)
                    {
                        dto.DeliveryStatus = CalculateDeliveryStatus(order, today);
                        // ✅ SAFETY: Ensure SupplierName is set
                        if (order.Supplier != null && string.IsNullOrEmpty(dto.SupplierName))
                            dto.SupplierName = order.Supplier.SupplierName;
                        // Calculate days until/overdue
                        if (order.ExpectedDeliveryDate.HasValue)
                        {
                            var daysDiff = (order.ExpectedDeliveryDate.Value.Date - today).Days;
                            dto.DaysUntilDelivery = daysDiff;
                        }
                    }
                }

                return Ok(PagedResponse<PurchaseOrderDto>.CreateSuccessResult(
                    purchaseOrderDtos, pageNumber, pageSize, totalCount, "In-transit orders retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting in-transit orders");
                return StatusCode(500, PagedResponse<PurchaseOrderDto>.CreateErrorResult("Error retrieving in-transit orders"));
            }
        }

        /// <summary>
        /// GET /api/purchase-orders/{id}/tracking
        /// Lấy thông tin tracking của PO
        /// </summary>
        [HttpGet("{id}/tracking")]
        public async Task<ActionResult<ApiResponse<PurchaseOrderTrackingDto>>> GetTrackingInfo(int id)
        {
            try
            {
                var order = await _context.PurchaseOrders
                    .AsNoTracking()
                    .Include(po => po.Supplier)
                    .Include(po => po.StatusHistory.OrderByDescending(sh => sh.StatusDate))
                    .FirstOrDefaultAsync(po => po.Id == id && !po.IsDeleted);

                if (order == null)
                {
                    return NotFound(ApiResponse<PurchaseOrderTrackingDto>.ErrorResult("Purchase order not found"));
                }

                var today = DateTime.Now.Date;
                var deliveryStatus = CalculateDeliveryStatus(order, today);

                var trackingDto = new PurchaseOrderTrackingDto
                {
                    PurchaseOrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    SupplierName = order.Supplier?.SupplierName ?? "",
                    OrderDate = order.OrderDate,
                    SentDate = order.SentDate,
                    InTransitDate = order.InTransitDate,
                    ExpectedDeliveryDate = order.ExpectedDeliveryDate,
                    ActualDeliveryDate = order.ActualDeliveryDate,
                    TrackingNumber = order.TrackingNumber,
                    ShippingMethod = order.ShippingMethod,
                    DeliveryStatus = deliveryStatus,
                    DeliveryNotes = order.DeliveryNotes,
                    DaysUntilDelivery = order.ExpectedDeliveryDate.HasValue
                        ? (order.ExpectedDeliveryDate.Value.Date - today).Days
                        : null,
                    // ✅ SAFETY: Null check for StatusHistory
                    StatusHistory = (order.StatusHistory ?? new List<PurchaseOrderStatusHistory>())
                        .Select(sh => new PurchaseOrderStatusHistoryDto
                        {
                            Id = sh.Id,
                            Status = sh.Status,
                            StatusDate = sh.StatusDate,
                            Notes = sh.Notes,
                            UpdatedByEmployeeId = sh.UpdatedByEmployeeId
                        }).ToList()
                };

                return Ok(ApiResponse<PurchaseOrderTrackingDto>.SuccessResult(trackingDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tracking info for PO {OrderId}", id);
                return StatusCode(500, ApiResponse<PurchaseOrderTrackingDto>.ErrorResult("Error retrieving tracking info"));
            }
        }

        /// <summary>
        /// PUT /api/purchase-orders/{id}/update-tracking
        /// Cập nhật thông tin tracking của PO
        /// </summary>
        [HttpPut("{id}/update-tracking")]
        public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> UpdateTracking(int id, [FromBody] UpdateTrackingDto dto)
        {
            try
            {
                // ✅ SAFETY: Validate DTO
                if (dto == null)
                {
                    return BadRequest(ApiResponse<PurchaseOrderDto>.ErrorResult("Request body is required"));
                }

                var order = await _unitOfWork.Repository<PurchaseOrder>().GetByIdAsync(id);
                if (order == null || order.IsDeleted)
                {
                    return NotFound(ApiResponse<PurchaseOrderDto>.ErrorResult("Purchase order not found"));
                }

                // ✅ SAFETY: Validate status transition - cannot update tracking for Received/Cancelled orders
                if (order.Status == "Received" || order.Status == "Cancelled")
                {
                    return BadRequest(ApiResponse<PurchaseOrderDto>.ErrorResult(
                        $"Cannot update tracking for PO with status '{order.Status}'. Only 'Sent' or 'InTransit' orders can be tracked."));
                }
                
                if (order.Status != "Sent" && order.Status != "InTransit")
                {
                    return BadRequest(ApiResponse<PurchaseOrderDto>.ErrorResult(
                        $"Cannot update tracking for PO with status '{order.Status}'. Only 'Sent' or 'InTransit' orders can be tracked."));
                }

                // ✅ SAFETY: Get today's date once for all validations
                var today = DateTime.Now.Date;

                // ✅ SAFETY: Validate ExpectedDeliveryDate if provided
                if (dto.ExpectedDeliveryDate.HasValue)
                {
                    var expectedDate = dto.ExpectedDeliveryDate.Value.Date;
                    var maxFutureDate = today.AddYears(2); // Max 2 years in future
                    var minPastDate = today.AddYears(-1); // Max 1 year in past
                    
                    if (expectedDate > maxFutureDate)
                    {
                        return BadRequest(ApiResponse<PurchaseOrderDto>.ErrorResult(
                            "Expected delivery date cannot be more than 2 years in the future."));
                    }
                    
                    if (expectedDate < minPastDate)
                    {
                        return BadRequest(ApiResponse<PurchaseOrderDto>.ErrorResult(
                            "Expected delivery date cannot be more than 1 year in the past."));
                    }
                    
                    // ✅ SAFETY: ExpectedDeliveryDate should be >= SentDate (if exists)
                    if (order.SentDate.HasValue && expectedDate < order.SentDate.Value.Date)
                    {
                        return BadRequest(ApiResponse<PurchaseOrderDto>.ErrorResult(
                            "Expected delivery date cannot be earlier than sent date."));
                    }
                }

                // ✅ SAFETY: Validate InTransitDate if provided
                if (dto.InTransitDate.HasValue)
                {
                    var inTransitDate = dto.InTransitDate.Value.Date;
                    
                    if (inTransitDate > today)
                    {
                        return BadRequest(ApiResponse<PurchaseOrderDto>.ErrorResult(
                            "In-transit date cannot be in the future."));
                    }
                    
                    // ✅ SAFETY: InTransitDate should be >= SentDate (if exists)
                    if (order.SentDate.HasValue && inTransitDate < order.SentDate.Value.Date)
                    {
                        return BadRequest(ApiResponse<PurchaseOrderDto>.ErrorResult(
                            "In-transit date cannot be earlier than sent date."));
                    }
                    
                    // ✅ SAFETY: InTransitDate should be <= ExpectedDeliveryDate (if exists)
                    if (order.ExpectedDeliveryDate.HasValue && inTransitDate > order.ExpectedDeliveryDate.Value.Date)
                    {
                        return BadRequest(ApiResponse<PurchaseOrderDto>.ErrorResult(
                            "In-transit date cannot be later than expected delivery date."));
                    }
                }

                // Update tracking fields
                if (dto.TrackingNumber != null)
                    order.TrackingNumber = dto.TrackingNumber;
                if (dto.ShippingMethod != null)
                    order.ShippingMethod = dto.ShippingMethod;
                if (dto.ExpectedDeliveryDate.HasValue)
                    order.ExpectedDeliveryDate = dto.ExpectedDeliveryDate;
                if (dto.DeliveryNotes != null)
                    order.DeliveryNotes = dto.DeliveryNotes;

                // Update delivery status based on new expected date
                if (dto.ExpectedDeliveryDate.HasValue || order.ExpectedDeliveryDate.HasValue)
                {
                    order.DeliveryStatus = CalculateDeliveryStatus(order, today);
                }

                // Create status history if status changed
                if (dto.MarkAsInTransit == true && order.Status == "Sent")
                {
                    // ✅ SAFETY: Check if already has InTransit status history (avoid duplicates)
                    var existingInTransitHistories = await _unitOfWork.Repository<PurchaseOrderStatusHistory>()
                        .FindAsync(sh => sh.PurchaseOrderId == order.Id && sh.Status == "InTransit" && !sh.IsDeleted);
                    var existingInTransitHistory = existingInTransitHistories?.FirstOrDefault();
                    
                    if (existingInTransitHistory == null)
                    {
                        order.Status = "InTransit";
                        // ✅ SAFETY: Ensure InTransitDate is set
                        order.InTransitDate = dto.InTransitDate ?? DateTime.Now;

                        var statusHistory = new PurchaseOrderStatusHistory
                        {
                            PurchaseOrderId = order.Id,
                            Status = "InTransit",
                            StatusDate = order.InTransitDate.Value,
                            Notes = dto.DeliveryNotes ?? "Đã gửi hàng",
                            UpdatedByEmployeeId = await GetCurrentEmployeeIdAsync()
                        };
                        await _unitOfWork.Repository<PurchaseOrderStatusHistory>().AddAsync(statusHistory);
                    }
                    else
                    {
                        // Already has InTransit history, just update the order status
                        order.Status = "InTransit";
                        if (dto.InTransitDate.HasValue)
                            order.InTransitDate = dto.InTransitDate.Value;
                        else if (!order.InTransitDate.HasValue)
                            order.InTransitDate = DateTime.Now;
                    }
                }

                order.UpdatedAt = DateTime.Now;
                await _unitOfWork.Repository<PurchaseOrder>().UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync();

                var orderDto = _mapper.Map<PurchaseOrderDto>(order);
                return Ok(ApiResponse<PurchaseOrderDto>.SuccessResult(orderDto, "Tracking information updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tracking for PO {OrderId}", id);
                return StatusCode(500, ApiResponse<PurchaseOrderDto>.ErrorResult("Error updating tracking information"));
            }
        }

        /// <summary>
        /// PUT /api/purchase-orders/{id}/mark-in-transit
        /// Đánh dấu PO đã gửi hàng (chuyển sang InTransit)
        /// </summary>
        [HttpPut("{id}/mark-in-transit")]
        public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> MarkAsInTransit(int id, [FromBody] MarkInTransitDto dto)
        {
            try
            {
                // ✅ SAFETY: Validate DTO (optional, but good practice)
                // dto can be null for this endpoint (all fields optional)

                var order = await _unitOfWork.Repository<PurchaseOrder>().GetByIdAsync(id);
                if (order == null || order.IsDeleted)
                {
                    return NotFound(ApiResponse<PurchaseOrderDto>.ErrorResult("Purchase order not found"));
                }

                // ✅ SAFETY: Validate status - cannot mark as InTransit if already Received/Cancelled
                if (order.Status == "Received" || order.Status == "Cancelled")
                {
                    return BadRequest(ApiResponse<PurchaseOrderDto>.ErrorResult(
                        $"Cannot mark PO as InTransit. Current status is '{order.Status}'. Only 'Sent' orders can be marked as InTransit."));
                }
                
                if (order.Status != "Sent")
                {
                    return BadRequest(ApiResponse<PurchaseOrderDto>.ErrorResult(
                        $"Cannot mark PO as InTransit. Current status is '{order.Status}'. Only 'Sent' orders can be marked as InTransit."));
                }

                // ✅ SAFETY: Get today's date once for all validations
                var today = DateTime.Now.Date;

                // ✅ SAFETY: Validate InTransitDate if provided
                if (dto?.InTransitDate.HasValue == true)
                {
                    var inTransitDate = dto.InTransitDate.Value.Date;
                    
                    if (inTransitDate > today)
                    {
                        return BadRequest(ApiResponse<PurchaseOrderDto>.ErrorResult(
                            "In-transit date cannot be in the future."));
                    }
                    
                    // ✅ SAFETY: InTransitDate should be >= SentDate (if exists)
                    if (order.SentDate.HasValue && inTransitDate < order.SentDate.Value.Date)
                    {
                        return BadRequest(ApiResponse<PurchaseOrderDto>.ErrorResult(
                            "In-transit date cannot be earlier than sent date."));
                    }
                    
                    // ✅ SAFETY: InTransitDate should be <= ExpectedDeliveryDate (if exists)
                    if (order.ExpectedDeliveryDate.HasValue && inTransitDate > order.ExpectedDeliveryDate.Value.Date)
                    {
                        return BadRequest(ApiResponse<PurchaseOrderDto>.ErrorResult(
                            "In-transit date cannot be later than expected delivery date."));
                    }
                }

                // ✅ SAFETY: Validate ExpectedDeliveryDate if provided
                if (dto?.ExpectedDeliveryDate.HasValue == true)
                {
                    var expectedDate = dto.ExpectedDeliveryDate.Value.Date;
                    var maxFutureDate = today.AddYears(2);
                    var minPastDate = today.AddYears(-1);
                    
                    if (expectedDate > maxFutureDate)
                    {
                        return BadRequest(ApiResponse<PurchaseOrderDto>.ErrorResult(
                            "Expected delivery date cannot be more than 2 years in the future."));
                    }
                    
                    if (expectedDate < minPastDate)
                    {
                        return BadRequest(ApiResponse<PurchaseOrderDto>.ErrorResult(
                            "Expected delivery date cannot be more than 1 year in the past."));
                    }
                }

                // Update status and tracking info
                order.Status = "InTransit";
                // ✅ SAFETY: Ensure InTransitDate is always set
                order.InTransitDate = dto?.InTransitDate ?? DateTime.Now;
                if (dto != null)
                {
                    if (dto.TrackingNumber != null)
                        order.TrackingNumber = dto.TrackingNumber;
                    if (dto.ShippingMethod != null)
                        order.ShippingMethod = dto.ShippingMethod;
                    if (dto.ExpectedDeliveryDate.HasValue)
                        order.ExpectedDeliveryDate = dto.ExpectedDeliveryDate;
                }

                // Calculate delivery status
                order.DeliveryStatus = CalculateDeliveryStatus(order, today);

                // ✅ SAFETY: Check if already has InTransit status history (avoid duplicates)
                var existingInTransitHistories = await _unitOfWork.Repository<PurchaseOrderStatusHistory>()
                    .FindAsync(sh => sh.PurchaseOrderId == order.Id && sh.Status == "InTransit" && !sh.IsDeleted);
                var existingInTransitHistory = existingInTransitHistories?.FirstOrDefault();
                
                if (existingInTransitHistory == null)
                {
                    // Create status history
                    // ✅ SAFETY: InTransitDate is guaranteed to be set above
                    var statusHistory = new PurchaseOrderStatusHistory
                    {
                        PurchaseOrderId = order.Id,
                        Status = "InTransit",
                        StatusDate = order.InTransitDate.Value,
                        Notes = dto?.Notes ?? "Đã gửi hàng",
                        UpdatedByEmployeeId = await GetCurrentEmployeeIdAsync()
                    };
                    await _unitOfWork.Repository<PurchaseOrderStatusHistory>().AddAsync(statusHistory);
                }
                else
                {
                    // Update existing history if needed
                    existingInTransitHistory.StatusDate = order.InTransitDate.Value;
                    if (dto?.Notes != null)
                        existingInTransitHistory.Notes = dto.Notes;
                    existingInTransitHistory.UpdatedByEmployeeId = await GetCurrentEmployeeIdAsync();
                    existingInTransitHistory.UpdatedAt = DateTime.Now;
                    await _unitOfWork.Repository<PurchaseOrderStatusHistory>().UpdateAsync(existingInTransitHistory);
                }

                order.UpdatedAt = DateTime.Now;
                await _unitOfWork.Repository<PurchaseOrder>().UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync();

                var orderDto = _mapper.Map<PurchaseOrderDto>(order);
                return Ok(ApiResponse<PurchaseOrderDto>.SuccessResult(orderDto, "PO marked as InTransit successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking PO {OrderId} as InTransit", id);
                return StatusCode(500, ApiResponse<PurchaseOrderDto>.ErrorResult("Error marking PO as InTransit"));
            }
        }

        /// <summary>
        /// GET /api/purchase-orders/delivery-alerts
        /// Lấy danh sách PO cần cảnh báo (sắp đến hạn, quá hạn)
        /// </summary>
        [HttpGet("delivery-alerts")]
        public async Task<ActionResult<ApiResponse<DeliveryAlertsDto>>> GetDeliveryAlerts()
        {
            try
            {
                var today = DateTime.Now.Date;
                var atRiskDate = today.AddDays(3); // Within 3 days

                // ✅ OPTIMIZED: Query ở database level
                var inTransitOrders = await _context.PurchaseOrders
                    .AsNoTracking()
                    .Include(po => po.Supplier)
                    .Where(po => !po.IsDeleted && 
                                (po.Status == "Sent" || po.Status == "InTransit") &&
                                po.ExpectedDeliveryDate.HasValue)
                    .ToListAsync();

                var atRiskOrders = new List<PurchaseOrderDto>();
                var delayedOrders = new List<PurchaseOrderDto>();

                foreach (var order in inTransitOrders)
                {
                    // ✅ OPTIMIZED: ExpectedDeliveryDate already filtered in query, but double-check for safety
                    if (!order.ExpectedDeliveryDate.HasValue) continue;

                    var deliveryDate = order.ExpectedDeliveryDate.Value.Date;
                    var daysDiff = (deliveryDate - today).Days;
                    var deliveryStatus = CalculateDeliveryStatus(order, today);

                    var orderDto = _mapper.Map<PurchaseOrderDto>(order);
                    // ✅ SAFETY: Ensure SupplierName is set
                    if (order.Supplier != null)
                        orderDto.SupplierName = order.Supplier.SupplierName;
                    orderDto.DeliveryStatus = deliveryStatus;
                    orderDto.DaysUntilDelivery = daysDiff;

                    if (deliveryStatus == "Delayed")
                    {
                        delayedOrders.Add(orderDto);
                    }
                    else if (deliveryStatus == "AtRisk")
                    {
                        atRiskOrders.Add(orderDto);
                    }
                }

                var alertsDto = new DeliveryAlertsDto
                {
                    AtRiskCount = atRiskOrders.Count,
                    DelayedCount = delayedOrders.Count,
                    TotalCount = atRiskOrders.Count + delayedOrders.Count,
                    AtRiskOrders = atRiskOrders.OrderBy(o => o.ExpectedDeliveryDate).ToList(),
                    DelayedOrders = delayedOrders.OrderBy(o => o.ExpectedDeliveryDate).ToList()
                };

                return Ok(ApiResponse<DeliveryAlertsDto>.SuccessResult(alertsDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting delivery alerts");
                return StatusCode(500, ApiResponse<DeliveryAlertsDto>.ErrorResult("Error retrieving delivery alerts"));
            }
        }

        /// <summary>
        /// Helper method: Calculate delivery status based on expected date
        /// </summary>
        private string CalculateDeliveryStatus(PurchaseOrder order, DateTime today)
        {
            // ✅ SAFETY: Null check for order (defensive programming)
            if (order == null)
                return "Unknown";

            if (!order.ExpectedDeliveryDate.HasValue)
                return "Unknown";

            var expectedDate = order.ExpectedDeliveryDate.Value.Date;
            var daysDiff = (expectedDate - today).Days;

            if (daysDiff < 0)
                return "Delayed"; // Quá hạn
            else if (daysDiff <= 3)
                return "AtRisk"; // Sắp đến hạn (trong 3 ngày)
            else
                return "OnTime"; // Còn nhiều thời gian
        }

        /// <summary>
        /// Helper method: Get current employee ID from claims
        /// </summary>
        private async Task<int?> GetCurrentEmployeeIdAsync()
        {
            try
            {
                var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                    ?? User.FindFirst("email")?.Value;

                if (!string.IsNullOrEmpty(userEmail))
                {
                    var employee = await _context.Employees
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Email != null && e.Email == userEmail);
                    if (employee != null) return employee.Id;
                }

                var employeeIdClaim = User.FindFirst("employee_id")?.Value
                    ?? User.FindFirst("employeeid")?.Value;
                if (!string.IsNullOrEmpty(employeeIdClaim) && int.TryParse(employeeIdClaim, out var empId))
                {
                    return empId;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        #endregion
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

