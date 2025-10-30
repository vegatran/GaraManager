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

                // Get supplier info
                var supplier = await _unitOfWork.Repository<Supplier>().GetByIdAsync(order.SupplierId);
                
                // Get all parts for stock calculation
                var parts = await _unitOfWork.Repository<Part>().GetAllAsync();
                var partsDict = parts.ToDictionary(p => p.Id, p => p);

                // ✅ OPTIMIZED: Filter purchase order items ở database level
                var orderItems = (await _unitOfWork.Repository<PurchaseOrderItem>()
                    .FindAsync(i => i.PurchaseOrderId == id)).ToList();

                // Generate transaction numbers
                // ✅ OPTIMIZED: Use CountAsync thay vì GetAllAsync().Count()
                var txCount = await _unitOfWork.Repository<StockTransaction>().CountAsync() + 1;
                var dateStr = DateTime.Now.ToString("yyyyMMdd");
                
                // Create Stock Transactions for each item
                foreach (var item in orderItems)
                {
                    if (!partsDict.ContainsKey(item.PartId)) continue;
                    
                    var part = partsDict[item.PartId];
                    
                    // ✅ OPTIMIZED: Filter stock transactions ở database level
                    var partTransactions = (await _unitOfWork.Repository<StockTransaction>()
                        .FindAsync(t => t.PartId == item.PartId)).ToList();
                    
                    int currentStock = 0;
                    foreach (var tx in partTransactions.OrderBy(t => t.TransactionDate))
                    {
                        if (tx.TransactionType == Core.Enums.StockTransactionType.NhapKho)
                            currentStock += tx.Quantity;
                        else if (tx.TransactionType == Core.Enums.StockTransactionType.XuatKho)
                            currentStock -= tx.Quantity;
                    }
                    
                    // Create Stock Transaction
                    var stockTx = new Core.Entities.StockTransaction
                    {
                        TransactionNumber = $"STK-{dateStr}-{txCount++:D4}",
                        TransactionType = Core.Enums.StockTransactionType.NhapKho,
                        PartId = item.PartId,
                        Quantity = item.QuantityOrdered,
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
                        SourceType = "Purchased",
                        StockAfter = currentStock + item.QuantityOrdered,
                        QuantityBefore = currentStock,
                        QuantityAfter = currentStock + item.QuantityOrdered
                    };
                    
                    await _unitOfWork.Repository<StockTransaction>().AddAsync(stockTx);
                    
                    // Update Part Stock
                    part.QuantityInStock += item.QuantityOrdered;
                    part.UpdatedAt = DateTime.Now;
                    await _unitOfWork.Repository<Part>().UpdateAsync(part);
                    
                    // ✅ OPTIMIZED: Find PartSupplier ở database level
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
                // ✅ OPTIMIZED: Use CountAsync thay vì GetAllAsync().Count()
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

                return Ok(new { success = true, data = order, message = "Đã nhận hàng, nhập kho và tạo phiếu chi" });
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

