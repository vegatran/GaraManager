using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Shared.Models;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GarageManagementSystem.API.Controllers
{
    /// <summary>
    /// Procurement Controller - Phase 4.2: Quản lý Mua hàng
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProcurementController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProcurementController> _logger;
        private readonly GarageDbContext _context;

        public ProcurementController(
            IUnitOfWork unitOfWork, 
            ILogger<ProcurementController> logger,
            GarageDbContext context)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// GET /api/procurement/demand-analysis
        /// Lấy tổng hợp nhu cầu từ alerts và JO
        /// </summary>
        [HttpGet("demand-analysis")]
        public async Task<ActionResult<PagedResponse<DemandAnalysisDto>>> GetDemandAnalysis(
            [FromQuery] int? warehouseId = null,
            [FromQuery] string? priority = null,
            [FromQuery] string? source = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                // Validate pagination
                if (pageSize <= 0) pageSize = 20;
                if (pageNumber <= 0) pageNumber = 1;
                if (pageSize > 100) pageSize = 100;

                var demandItems = new List<DemandAnalysisDto>();

                // 1. Get Inventory Alerts (Low Stock, Out of Stock)
                if (source == null || source == "InventoryAlert" || source == "All")
                {
                    // ✅ OPTIMIZED: Single query to get alerts with parts in one go
                    var alertsQuery = _context.Set<InventoryAlert>()
                        .AsNoTracking()
                        .Where(a => !a.IsDeleted && !a.IsResolved && 
                                   (a.AlertType == "LowStock" || a.AlertType == "OutOfStock"));
                    
                    var alertPartIds = await alertsQuery
                        .Select(a => a.PartId)
                        .Distinct()
                        .ToListAsync();
                    
                    if (alertPartIds.Any())
                    {
                        var alertsList = await alertsQuery.ToListAsync();
                        var parts = await _context.Parts
                            .AsNoTracking()
                            .Where(p => alertPartIds.Contains(p.Id) && !p.IsDeleted)
                            .ToDictionaryAsync(p => p.Id);

                    foreach (var alert in alertsList)
                    {
                            if (!parts.TryGetValue(alert.PartId, out var part) || part == null) continue;
                        var currentStock = part.QuantityInStock;
                        var minimumStock = part.MinimumStock;
                            
                            // Calculate reorder level: if not set, use minimumStock * 2, but ensure at least 1
                            var reorderLevel = part.ReorderLevel ?? (minimumStock > 0 ? minimumStock * 2 : 10);
                            if (reorderLevel <= 0) reorderLevel = 10; // Default fallback
                            
                            // Calculate suggested quantity: only suggest if current stock is below reorder level
                            var suggestedQuantity = currentStock < reorderLevel 
                                ? Math.Max(reorderLevel - currentStock, minimumStock > 0 ? minimumStock : 1)
                                : 0;
                            
                            // Skip if no quantity needed
                            if (suggestedQuantity <= 0) continue;
                            
                            var costPrice = part.CostPrice > 0 ? part.CostPrice : (part.AverageCostPrice > 0 ? part.AverageCostPrice : 0);
                            var estimatedCost = suggestedQuantity * costPrice;

                        // Determine priority
                        string priorityLevel = "Medium";
                        if (alert.AlertType == "OutOfStock" || currentStock == 0)
                            priorityLevel = "High";
                        else if (currentStock <= minimumStock * 0.5m)
                            priorityLevel = "High";
                        else if (currentStock <= minimumStock)
                            priorityLevel = "Medium";
                        else
                            priorityLevel = "Low";

                        // Apply filters
                        if (!string.IsNullOrEmpty(priority) && priorityLevel != priority) continue;

                        demandItems.Add(new DemandAnalysisDto
                        {
                            PartId = part.Id,
                            PartNumber = part.PartNumber ?? "N/A",
                            PartName = part.PartName ?? "N/A",
                            CurrentStock = currentStock,
                            MinimumStock = minimumStock,
                            SuggestedQuantity = suggestedQuantity,
                            Priority = priorityLevel,
                            Source = "InventoryAlert",
                            SourceEntityId = alert.Id,
                            RequiredByDate = null, // Can be calculated based on lead time
                            EstimatedCost = estimatedCost,
                            SuggestedDate = DateTime.Now
                        });
                        }
                    }
                }

                // 2. Get Service Orders sắp tới (scheduled trong 7-30 ngày)
                if (source == null || source == "ServiceOrder" || source == "All")
                {
                    var now = DateTime.Now;
                    // ✅ OPTIMIZED: Get Service Orders with ScheduledDate in one query
                    var upcomingServiceOrdersDict = await _context.ServiceOrders
                        .AsNoTracking()
                        .Where(so => !so.IsDeleted)
                        .Where(so => so.ScheduledDate != null && 
                                     so.ScheduledDate >= now && 
                                     so.ScheduledDate <= now.AddDays(30))
                        .Where(so => so.Status != "Completed" && so.Status != "Cancelled")
                        .ToDictionaryAsync(so => so.Id, so => so.ScheduledDate);

                    var upcomingServiceOrderIds = upcomingServiceOrdersDict.Keys.ToList();

                    // ✅ OPTIMIZED: Load ServiceOrderParts without Include (we'll load Parts separately)
                    // ✅ OPTIMIZED: Use AsNoTracking for read-only query
                    // ✅ FIX: Skip if no service orders found
                    if (upcomingServiceOrderIds.Any())
                    {
                    var serviceOrderParts = await _context.ServiceOrderParts
                            .AsNoTracking()
                        .Where(sop => !sop.IsDeleted && 
                                         upcomingServiceOrderIds.Contains(sop.ServiceOrderId))
                        .ToListAsync();

                        // ✅ OPTIMIZED: Filter out deleted parts first
                        var validPartIds = serviceOrderParts
                            .Select(sop => sop.PartId)
                            .Distinct()
                            .ToList();
                        
                        // ✅ OPTIMIZED: Load all needed parts at once with AsNoTracking
                        // ✅ FIX: Skip if no valid parts found
                        if (validPartIds.Any())
                        {
                            var neededParts = await _context.Parts
                                .AsNoTracking()
                                .Where(p => validPartIds.Contains(p.Id) && !p.IsDeleted)
                                .ToDictionaryAsync(p => p.Id);

                            // ✅ OPTIMIZED: Filter serviceOrderParts to only include valid parts
                            var validServiceOrderParts = serviceOrderParts
                                .Where(sop => neededParts.ContainsKey(sop.PartId))
                                .ToList();

                    // Group parts by PartId and sum quantities
                    var partsNeeded = new Dictionary<int, (int Quantity, DateTime? RequiredByDate, int? ServiceOrderId)>();
                    
                            foreach (var sop in validServiceOrderParts)
                    {
                                var scheduledDate = upcomingServiceOrdersDict.GetValueOrDefault(sop.ServiceOrderId);
                        if (partsNeeded.ContainsKey(sop.PartId))
                        {
                            var existing = partsNeeded[sop.PartId];
                                    // Take the earliest required date (min date)
                                    var minDate = existing.RequiredByDate.HasValue && scheduledDate.HasValue
                                        ? (existing.RequiredByDate.Value < scheduledDate.Value ? existing.RequiredByDate : scheduledDate)
                                        : (existing.RequiredByDate ?? scheduledDate);
                            partsNeeded[sop.PartId] = (
                                existing.Quantity + sop.Quantity,
                                        minDate,
                                sop.ServiceOrderId
                            );
                        }
                        else
                        {
                            partsNeeded[sop.PartId] = (sop.Quantity, scheduledDate, sop.ServiceOrderId);
                        }
                    }

                    // Create demand items from Service Orders
                    foreach (var kvp in partsNeeded)
                    {
                                if (!neededParts.TryGetValue(kvp.Key, out var part) || part == null)
                                    continue;

                        var currentStock = part.QuantityInStock;
                        var neededQuantity = kvp.Value.Quantity;
                        var suggestedQuantity = Math.Max(neededQuantity - currentStock, 0);
                        
                        if (suggestedQuantity <= 0) continue; // Đã đủ hàng

                                var costPrice = part.CostPrice > 0 ? part.CostPrice : (part.AverageCostPrice > 0 ? part.AverageCostPrice : 0);
                                var estimatedCost = suggestedQuantity * costPrice;
                        var daysUntilNeeded = kvp.Value.RequiredByDate.HasValue 
                            ? (kvp.Value.RequiredByDate.Value - DateTime.Now).Days 
                            : 30;

                        // Determine priority based on days until needed
                        string priorityLevel = "Medium";
                        if (daysUntilNeeded <= 7)
                            priorityLevel = "High";
                        else if (daysUntilNeeded <= 14)
                            priorityLevel = "Medium";
                        else
                            priorityLevel = "Low";

                        // Apply filters
                        if (!string.IsNullOrEmpty(priority) && priorityLevel != priority) continue;

                        demandItems.Add(new DemandAnalysisDto
                        {
                            PartId = part.Id,
                            PartNumber = part.PartNumber ?? "N/A",
                            PartName = part.PartName ?? "N/A",
                            CurrentStock = currentStock,
                            MinimumStock = part.MinimumStock,
                            SuggestedQuantity = suggestedQuantity,
                            Priority = priorityLevel,
                            Source = "ServiceOrder",
                            SourceEntityId = kvp.Value.ServiceOrderId,
                            RequiredByDate = kvp.Value.RequiredByDate,
                            EstimatedCost = estimatedCost,
                            SuggestedDate = DateTime.Now
                        });
                            }
                        }
                    }
                }

                // Remove duplicates (same part from multiple sources) - keep highest priority
                // Define priority order once for reuse
                var priorityOrder = new Dictionary<string, int> { { "High", 3 }, { "Medium", 2 }, { "Low", 1 } };
                
                var groupedDemand = demandItems
                    .GroupBy(d => d.PartId)
                    .Select(g =>
                    {
                        var items = g.ToList();
                        if (!items.Any())
                            return (DemandAnalysisDto?)null; // Should not happen, but safety check
                        
                        // Sort by priority (High > Medium > Low) and take first
                        // If multiple items have same priority, take the one with highest suggested quantity
                        return items.OrderByDescending(i => priorityOrder.GetValueOrDefault(i.Priority, 0))
                                    .ThenByDescending(i => i.SuggestedQuantity)
                                    .First();
                    })
                    .Where(d => d != null)
                    .Cast<DemandAnalysisDto>()
                    .ToList();

                // Apply warehouse filter if provided (future enhancement)
                // if (warehouseId.HasValue) { ... }

                // Sort by priority and suggested date
                var sortedDemand = groupedDemand
                    .OrderByDescending(d => priorityOrder.GetValueOrDefault(d.Priority, 0))
                    .ThenBy(d => d.RequiredByDate ?? DateTime.MaxValue)
                    .ToList();

                // Apply pagination
                var totalCount = sortedDemand.Count;
                var pagedDemand = sortedDemand
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Ok(PagedResponse<DemandAnalysisDto>.CreateSuccessResult(
                    pagedDemand, pageNumber, pageSize, totalCount, "Lấy danh sách nhu cầu thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting demand analysis");
                return StatusCode(500, PagedResponse<DemandAnalysisDto>.CreateErrorResult(
                    "Lỗi khi lấy danh sách nhu cầu", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// GET /api/procurement/reorder-suggestions
        /// Lấy danh sách đề xuất đặt hàng
        /// </summary>
        [HttpGet("reorder-suggestions")]
        public async Task<ActionResult<PagedResponse<ReorderSuggestionDto>>> GetReorderSuggestions(
            [FromQuery] bool includeServiceOrders = true,
            [FromQuery] string? minPriority = null,
            [FromQuery] bool? isProcessed = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                // Validate pagination
                if (pageSize <= 0) pageSize = 50;
                if (pageNumber <= 0) pageNumber = 1;
                if (pageSize > 100) pageSize = 100;

                // ✅ OPTIMIZED: Use AsNoTracking for read-only query
                var query = _context.ReorderSuggestions
                    .AsNoTracking()
                    .Include(rs => rs.Part)
                    .Include(rs => rs.PurchaseOrder)
                    .Where(rs => !rs.IsDeleted)
                    .AsQueryable();

                // Apply filters
                if (isProcessed.HasValue)
                {
                    query = query.Where(rs => rs.IsProcessed == isProcessed.Value);
                }

                // ✅ OPTIMIZED: Filter priority at database level
                if (!string.IsNullOrEmpty(minPriority))
                {
                    query = minPriority switch
                    {
                        "High" => query.Where(rs => rs.Priority == "High"),
                        "Medium" => query.Where(rs => rs.Priority == "High" || rs.Priority == "Medium"),
                        _ => query // Low includes all
                    };
                }

                // Get total count
                var totalCount = await query.CountAsync();

                // ✅ OPTIMIZED: Order by priority using dictionary lookup
                var priorityOrder = new Dictionary<string, int> { { "High", 3 }, { "Medium", 2 }, { "Low", 1 } };
                var suggestions = await query
                    .OrderByDescending(rs => priorityOrder.GetValueOrDefault(rs.Priority, 0))
                    .ThenBy(rs => rs.SuggestedDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var suggestionDtos = suggestions.Select(rs => new ReorderSuggestionDto
                {
                    Id = rs.Id,
                    PartId = rs.PartId,
                    PartNumber = rs.Part?.PartNumber ?? "N/A",
                    PartName = rs.Part?.PartName ?? "N/A",
                    CurrentStock = rs.Part?.QuantityInStock ?? 0,
                    MinimumStock = rs.Part?.MinimumStock ?? 0,
                    SuggestedQuantity = rs.SuggestedQuantity,
                    EstimatedCost = rs.EstimatedCost,
                    Priority = rs.Priority,
                    Source = rs.Source,
                    SourceEntityId = rs.SourceEntityId,
                    SuggestedDate = rs.SuggestedDate,
                    RequiredByDate = rs.RequiredByDate,
                    IsProcessed = rs.IsProcessed,
                    PurchaseOrderId = rs.PurchaseOrderId
                }).ToList();

                return Ok(PagedResponse<ReorderSuggestionDto>.CreateSuccessResult(
                    suggestionDtos, pageNumber, pageSize, totalCount, "Lấy danh sách đề xuất thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reorder suggestions");
                return StatusCode(500, PagedResponse<ReorderSuggestionDto>.CreateErrorResult(
                    "Lỗi khi lấy danh sách đề xuất", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// POST /api/procurement/bulk-create-po
        /// Tạo PO từ danh sách reorder suggestions
        /// </summary>
        [HttpPost("bulk-create-po")]
        public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> BulkCreatePurchaseOrder(
            [FromBody] BulkCreatePODto dto)
        {
            try
            {
                if (dto == null || dto.Suggestions == null || !dto.Suggestions.Any())
                {
                    return BadRequest(ApiResponse<PurchaseOrderDto>.ErrorResult(
                        "Danh sách suggestions không được để trống"));
                }

                // Validate supplier
                var supplier = await _unitOfWork.Suppliers.GetByIdAsync(dto.SupplierId);
                if (supplier == null || supplier.IsDeleted)
                {
                    return NotFound(ApiResponse<PurchaseOrderDto>.ErrorResult(
                        "Không tìm thấy nhà cung cấp"));
                }

                // Validate suggestions for duplicates (before transaction)
                var duplicateParts = dto.Suggestions
                    .GroupBy(s => s.PartId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();
                
                if (duplicateParts.Any())
                {
                    return BadRequest(ApiResponse<PurchaseOrderDto>.ErrorResult(
                        $"Danh sách suggestions có phần tử trùng lặp cho PartId: {string.Join(", ", duplicateParts)}"));
                }

                // Validate quantities (before transaction)
                var invalidQuantities = dto.Suggestions
                    .Where(s => s.Quantity <= 0)
                    .Select(s => s.PartId)
                    .ToList();
                
                if (invalidQuantities.Any())
                {
                    return BadRequest(ApiResponse<PurchaseOrderDto>.ErrorResult(
                        $"Số lượng phải lớn hơn 0 cho các PartId: {string.Join(", ", invalidQuantities)}"));
                }

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    // Generate PO number
                    var poNumber = await GeneratePONumberAsync();

                    // ✅ OPTIMIZED: Pre-load all Parts and PartSuppliers to avoid N+1 queries
                    var partIds = dto.Suggestions.Select(s => s.PartId).Distinct().ToList();
                    var parts = await _context.Parts
                        .Where(p => partIds.Contains(p.Id) && !p.IsDeleted)
                        .ToDictionaryAsync(p => p.Id);
                    
                    var partSuppliers = await _context.PartSuppliers
                        .Where(ps => partIds.Contains(ps.PartId) && 
                                    ps.SupplierId == dto.SupplierId && 
                                    !ps.IsDeleted && 
                                    ps.IsActive)
                        .ToDictionaryAsync(ps => ps.PartId);

                    // Validate all parts exist before processing
                    var missingPartIds = partIds.Where(id => !parts.ContainsKey(id)).ToList();
                    if (missingPartIds.Any())
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return NotFound(ApiResponse<PurchaseOrderDto>.ErrorResult(
                            $"Không tìm thấy phụ tùng với ID: {string.Join(", ", missingPartIds)}"));
                    }

                    // Validate all parts can be supplied by this supplier
                    var unsuppliedPartIds = partIds.Where(id => !partSuppliers.ContainsKey(id)).ToList();
                    if (unsuppliedPartIds.Any())
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        var unsuppliedPartNames = unsuppliedPartIds
                            .Where(id => parts.ContainsKey(id))
                            .Select(id => parts[id].PartName)
                            .ToList();
                        return BadRequest(ApiResponse<PurchaseOrderDto>.ErrorResult(
                            $"Nhà cung cấp không thể cung cấp các phụ tùng: {string.Join(", ", unsuppliedPartNames)}"));
                    }

                    // Create Purchase Order
                    var purchaseOrder = new PurchaseOrder
                    {
                        OrderNumber = poNumber,
                        SupplierId = dto.SupplierId,
                        OrderDate = dto.OrderDate ?? DateTime.Now,
                        ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
                        Status = "Draft",
                        Currency = "VND",
                        Notes = dto.Notes,
                        EmployeeId = await GetCurrentEmployeeIdAsync()
                    };

                    decimal subTotal = 0;
                    var items = new List<PurchaseOrderItem>();

                    // Process each suggestion
                    // ✅ SAFETY: Use TryGetValue even though validated above, to handle edge cases
                    // (e.g., if data changes during transaction due to concurrency)
                    foreach (var suggestionDto in dto.Suggestions)
                    {
                        if (!parts.TryGetValue(suggestionDto.PartId, out var part) || part == null)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return NotFound(ApiResponse<PurchaseOrderDto>.ErrorResult(
                                $"Không tìm thấy phụ tùng với ID: {suggestionDto.PartId}"));
                        }

                        if (!partSuppliers.TryGetValue(suggestionDto.PartId, out var partSupplier) || partSupplier == null)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return BadRequest(ApiResponse<PurchaseOrderDto>.ErrorResult(
                                $"Nhà cung cấp không thể cung cấp phụ tùng: {part.PartName}"));
                        }

                        // Check minimum order quantity
                        if (suggestionDto.Quantity < partSupplier.MinimumOrderQuantity)
                        {
                            _logger.LogWarning(
                                "Quantity {Quantity} is less than minimum order quantity {MinQty} for part {PartId}",
                                suggestionDto.Quantity, partSupplier.MinimumOrderQuantity, suggestionDto.PartId);
                            // Continue but log warning
                        }

                        var unitPrice = suggestionDto.UnitPrice > 0 
                            ? suggestionDto.UnitPrice 
                            : (partSupplier.CostPrice > 0 ? partSupplier.CostPrice : 0);
                        
                        if (unitPrice <= 0)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return BadRequest(ApiResponse<PurchaseOrderDto>.ErrorResult(
                                $"Giá đơn vị không hợp lệ cho phụ tùng: {part.PartName}. Vui lòng nhập giá."));
                        }
                        
                        var totalPrice = suggestionDto.Quantity * unitPrice;
                        subTotal += totalPrice;

                        var item = new PurchaseOrderItem
                        {
                            PurchaseOrder = purchaseOrder,
                            PartId = suggestionDto.PartId,
                            PartName = part.PartName ?? "N/A",
                            QuantityOrdered = suggestionDto.Quantity,
                            UnitPrice = unitPrice,
                            TotalPrice = totalPrice,
                            VATRate = 10, // Default VAT
                            VATAmount = totalPrice * 0.1m,
                            Unit = part.DefaultUnit ?? "Cái",
                            ExpectedDeliveryDate = suggestionDto.ExpectedDeliveryDate ?? dto.ExpectedDeliveryDate,
                            SupplierPartNumber = partSupplier.SupplierPartNumber // Can be null, which is OK
                        };

                        items.Add(item);
                    }

                    // Calculate totals
                    purchaseOrder.SubTotal = subTotal;
                    purchaseOrder.VATRate = 10; // Average VAT rate
                    purchaseOrder.TaxAmount = subTotal * 0.1m;
                    purchaseOrder.TotalAmount = subTotal + purchaseOrder.TaxAmount + purchaseOrder.ShippingCost;

                    // Save Purchase Order
                    await _unitOfWork.Repository<PurchaseOrder>().AddAsync(purchaseOrder);

                    // Add items
                    foreach (var item in items)
                    {
                        await _unitOfWork.Repository<PurchaseOrderItem>().AddAsync(item);
                    }

                    // Update reorder suggestions
                    var suggestionIds = dto.Suggestions
                        .Where(s => s.SuggestionId.HasValue)
                        .Select(s => s.SuggestionId!.Value)
                        .ToList();
                    
                    if (suggestionIds.Any())
                    {
                        var suggestions = await _context.ReorderSuggestions
                            .Where(rs => suggestionIds.Contains(rs.Id))
                            .ToListAsync();
                        
                        foreach (var suggestion in suggestions)
                            {
                                suggestion.IsProcessed = true;
                                suggestion.PurchaseOrderId = purchaseOrder.Id;
                                await _unitOfWork.ReorderSuggestions.UpdateAsync(suggestion);
                            }
                        }
                    
                    // Save all changes once at the end
                    await _unitOfWork.SaveChangesAsync();

                    await _unitOfWork.CommitTransactionAsync();

                    // Map to DTO - Load full order with items for complete mapping
                    var fullOrder = await _unitOfWork.Repository<PurchaseOrder>().GetByIdAsync(purchaseOrder.Id);
                    if (fullOrder == null)
                    {
                        _logger.LogError("PurchaseOrder {Id} not found after creation", purchaseOrder.Id);
                        return StatusCode(500, ApiResponse<PurchaseOrderDto>.ErrorResult(
                            "Lỗi khi tải thông tin PO sau khi tạo"));
                    }
                    
                    var fullItems = await _context.PurchaseOrderItems
                        .Where(i => i.PurchaseOrderId == purchaseOrder.Id && !i.IsDeleted)
                        .ToListAsync();
                    
                    var poDto = new PurchaseOrderDto
                    {
                        Id = purchaseOrder.Id,
                        OrderNumber = purchaseOrder.OrderNumber,
                        SupplierId = purchaseOrder.SupplierId,
                        SupplierName = supplier?.SupplierName ?? "N/A",
                        OrderDate = purchaseOrder.OrderDate,
                        ExpectedDeliveryDate = purchaseOrder.ExpectedDeliveryDate,
                        Status = purchaseOrder.Status,
                        SubTotal = purchaseOrder.SubTotal,
                        VATRate = purchaseOrder.VATRate,
                        TaxAmount = purchaseOrder.TaxAmount,
                        ShippingCost = purchaseOrder.ShippingCost,
                        TotalAmount = purchaseOrder.TotalAmount,
                        Notes = purchaseOrder.Notes,
                        PaymentTerms = purchaseOrder.PaymentTerms,
                        DeliveryAddress = purchaseOrder.DeliveryAddress,
                        Currency = purchaseOrder.Currency,
                        ItemCount = fullItems.Count,
                        Items = fullItems.Select(i => new PurchaseOrderItemDto
                        {
                            Id = i.Id,
                            PurchaseOrderId = i.PurchaseOrderId,
                            PartId = i.PartId,
                            PartName = i.PartName,
                            QuantityOrdered = i.QuantityOrdered,
                            UnitPrice = i.UnitPrice,
                            TotalPrice = i.TotalPrice,
                            VATRate = i.VATRate,
                            VATAmount = i.VATAmount,
                            Notes = i.Notes,
                            Unit = i.Unit,
                            SupplierPartNumber = i.SupplierPartNumber,
                            ExpectedDeliveryDate = i.ExpectedDeliveryDate
                        }).ToList()
                    };

                    _logger.LogInformation("Created PO {OrderNumber} from {Count} suggestions", 
                        poNumber, dto.Suggestions.Count);

                    return Ok(ApiResponse<PurchaseOrderDto>.SuccessResult(
                        poDto, "Tạo PO thành công"));
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bulk PO");
                return StatusCode(500, ApiResponse<PurchaseOrderDto>.ErrorResult(
                    "Lỗi khi tạo PO", ex));
            }
        }

        #region Helper Methods

        private async Task<string> GeneratePONumberAsync()
        {
            var year = DateTime.Now.Year;
            var prefix = $"PO-{year}-";

            // Get the last PO number for this year
            // Use AsNoTracking for better performance since we're just reading
            var lastPO = await _context.PurchaseOrders
                .AsNoTracking()
                .Where(po => !po.IsDeleted && po.OrderNumber.StartsWith(prefix))
                .OrderByDescending(po => po.OrderNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastPO != null)
            {
                var lastNumberStr = lastPO.OrderNumber.Replace(prefix, "");
                if (int.TryParse(lastNumberStr, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            // Note: In high-concurrency scenarios, consider using database sequence or lock
            // For now, transaction will help prevent duplicates
            return $"{prefix}{nextNumber:D3}";
        }

        private async Task<int?> GetCurrentEmployeeIdAsync()
        {
            try
            {
                // Lấy email từ claims
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value 
                    ?? User.FindFirst("email")?.Value;
                
                var userName = User.FindFirst(ClaimTypes.Name)?.Value 
                    ?? User.FindFirst("name")?.Value
                    ?? User.Identity?.Name;

                // ✅ OPTIMIZED: Normalize input and use AsNoTracking for read-only query
                // Try to find Employee by email (ưu tiên)
                if (!string.IsNullOrEmpty(userEmail))
                {
                    // Note: Email comparison is typically case-insensitive in most systems
                    // If database collation is case-sensitive, consider normalizing in database
                    var employeeByEmail = await _context.Employees
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Email != null && e.Email == userEmail);
                    
                    if (employeeByEmail != null)
                    {
                        _logger.LogInformation($"Found employee by email: {employeeByEmail.Id} - {employeeByEmail.Name}");
                        return employeeByEmail.Id;
                    }
                }

                // Try to find Employee by name if email not found
                if (!string.IsNullOrEmpty(userName))
                {
                    // ✅ OPTIMIZED: Load employees and filter in memory (small dataset expected)
                    // This is acceptable since employee table is typically small
                    var employees = await _context.Employees
                        .AsNoTracking()
                        .Where(e => e.Name != null)
                        .ToListAsync();
                    
                    var employeeByName = employees.FirstOrDefault(e => 
                        e.Name != null && 
                        e.Name.Contains(userName, StringComparison.OrdinalIgnoreCase));
                    
                    if (employeeByName != null)
                    {
                        _logger.LogInformation($"Found employee by name: {employeeByName.Id} - {employeeByName.Name}");
                        return employeeByName.Id;
                    }
                }

                // Try to get EmployeeId directly from claims (nếu có)
                var employeeIdClaim = User.FindFirst("employee_id")?.Value 
                    ?? User.FindFirst("employeeid")?.Value;
                
                if (!string.IsNullOrEmpty(employeeIdClaim) && int.TryParse(employeeIdClaim, out var empId))
                {
                    var employee = await _unitOfWork.Employees.GetByIdAsync(empId);
                    if (employee != null)
                    {
                        _logger.LogInformation($"Found employee by claim: {employee.Id} - {employee.Name}");
                        return employee.Id;
                    }
                }

                _logger.LogWarning($"Cannot find Employee for authenticated user. email: {userEmail}, name: {userName}");
            return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current employee ID from Identity");
                return null;
            }
        }

        #endregion

        /// <summary>
        /// GET /api/procurement/supplier-comparison
        /// So sánh các nhà cung cấp cho một phụ tùng
        /// </summary>
        [HttpGet("supplier-comparison")]
        public async Task<ActionResult<ApiResponse<List<SupplierComparisonDto>>>> GetSupplierComparison(
            [FromQuery] int partId,
            [FromQuery] int? quantity = null)
        {
            try
            {
                // Validate part exists
                var part = await _unitOfWork.Parts.GetByIdAsync(partId);
                if (part == null || part.IsDeleted)
                {
                    return NotFound(ApiResponse<List<SupplierComparisonDto>>.ErrorResult(
                        "Không tìm thấy phụ tùng"));
                }

                // Get all active PartSuppliers for this part
                var partSuppliers = await _context.PartSuppliers
                    .AsNoTracking()
                    .Include(ps => ps.Supplier)
                    .Where(ps => ps.PartId == partId && 
                                !ps.IsDeleted && 
                                ps.IsActive &&
                                ps.Supplier != null &&
                                !ps.Supplier.IsDeleted &&
                                ps.Supplier.IsActive)
                    .ToListAsync();

                if (!partSuppliers.Any())
                {
                    return Ok(ApiResponse<List<SupplierComparisonDto>>.SuccessResult(
                        new List<SupplierComparisonDto>(), 
                        "Không có nhà cung cấp nào cho phụ tùng này"));
                }

                // Get supplier IDs
                var supplierIds = partSuppliers.Select(ps => ps.SupplierId).Distinct().ToList();

                // ✅ OPTIMIZED: Parallel queries for better performance
                // ✅ SAFETY: Use FirstOrDefault to avoid exception if group is empty
                var performancesTask = _context.Set<SupplierPerformance>()
                    .AsNoTracking()
                    .Where(sp => supplierIds.Contains(sp.SupplierId) && 
                                (sp.PartId == partId || sp.PartId == null) &&
                                !sp.IsDeleted)
                    .GroupBy(sp => sp.SupplierId)
                    .Where(g => g.Any())
                    .Select(g => new { 
                        SupplierId = g.Key, 
                        Performance = g.OrderByDescending(sp => sp.CalculatedAt).FirstOrDefault() 
                    })
                    .Where(p => p.Performance != null)
                    .ToListAsync();

                // ✅ OPTIMIZED: GroupBy at database level instead of in memory
                var ratingsTask = _context.Set<SupplierRating>()
                    .AsNoTracking()
                    .Where(sr => supplierIds.Contains(sr.SupplierId) && 
                                (sr.PartId == partId || sr.PartId == null) &&
                                !sr.IsDeleted)
                    .GroupBy(sr => sr.SupplierId)
                    .Where(g => g.Any())
                    .Select(g => new { 
                        SupplierId = g.Key, 
                        AvgRating = (decimal)g.Average(sr => (double)sr.Rating), 
                        Count = g.Count() 
                    })
                    .ToListAsync();

                // ✅ OPTIMIZED: Load only most recent quotation per supplier at DB level
                // ✅ SAFETY: Use FirstOrDefault to avoid exception if group is empty
                var quotationsTask = _context.Set<SupplierQuotation>()
                    .AsNoTracking()
                    .Where(sq => sq.PartId == partId && 
                                supplierIds.Contains(sq.SupplierId) &&
                                !sq.IsDeleted &&
                                sq.Status == "Accepted" &&
                                (sq.ValidUntil == null || sq.ValidUntil >= DateTime.Now))
                    .GroupBy(sq => sq.SupplierId)
                    .Where(g => g.Any())
                    .Select(g => g.OrderByDescending(sq => sq.QuotationDate).FirstOrDefault())
                    .Where(q => q != null)
                    .ToListAsync();

                // ✅ OPTIMIZED: Execute all queries in parallel
                await Task.WhenAll(performancesTask, ratingsTask, quotationsTask);

                var performances = await performancesTask;
                var ratings = await ratingsTask;
                var quotations = await quotationsTask;

                // ✅ SAFETY: Use safe dictionary creation to avoid duplicate key exceptions
                var performanceDict = performances
                    .Where(p => p != null && p.Performance != null)
                    .GroupBy(p => p.SupplierId)
                    .ToDictionary(g => g.Key, g => g.First().Performance);
                var ratingDict = ratings
                    .Where(r => r != null)
                    .GroupBy(r => r.SupplierId)
                    .ToDictionary(g => g.Key, g => g.First());
                var quotationDict = quotations
                    .Where(q => q != null)
                    .GroupBy(q => q.SupplierId)
                    .ToDictionary(g => g.Key, g => g.First());

                // Build comparison list - First pass: collect all prices for comparison
                var priceList = new List<decimal>();
                foreach (var partSupplier in partSuppliers)
                {
                    quotationDict.TryGetValue(partSupplier.SupplierId, out var quotation);
                    var unitPrice = quotation?.UnitPrice ?? partSupplier.CostPrice;
                    priceList.Add(unitPrice);
                }
                var minPrice = priceList.Any() ? priceList.Min() : 0;
                var maxPrice = priceList.Any() ? priceList.Max() : 0;
                var avgPrice = priceList.Any() ? priceList.Average() : 0;

                var comparisons = new List<SupplierComparisonDto>();

                foreach (var partSupplier in partSuppliers)
                {
                    var supplier = partSupplier.Supplier;
                    // ✅ SAFETY: Check supplier is not null (should not happen due to filter, but defensive)
                    if (supplier == null) continue;
                    
                    // ✅ OPTIMIZED: Use TryGetValue for better performance
                    performanceDict.TryGetValue(supplier.Id, out var performance);
                    ratingDict.TryGetValue(supplier.Id, out var rating);
                    quotationDict.TryGetValue(supplier.Id, out var quotation);

                    // Calculate unit price (prefer quotation, then PartSupplier)
                    var unitPrice = quotation?.UnitPrice ?? partSupplier.CostPrice;
                    var totalPrice = quantity.HasValue ? unitPrice * quantity.Value : unitPrice;

                    // Calculate score (0-100) with price competitiveness
                    var avgRating = rating != null ? rating.AvgRating : supplier.Rating;
                    var score = CalculateSupplierScore(
                        performance,
                        avgRating,
                        unitPrice,
                        minPrice,
                        maxPrice,
                        avgPrice,
                        partSupplier.LeadTimeDays,
                        quotation?.LeadTimeDays);

                    comparisons.Add(new SupplierComparisonDto
                    {
                        SupplierId = supplier.Id,
                        SupplierName = supplier.SupplierName,
                        SupplierCode = supplier.SupplierCode,
                        ContactPerson = supplier.ContactPerson,
                        Phone = supplier.Phone ?? supplier.ContactPhone,
                        Email = supplier.Email,
                        UnitPrice = unitPrice,
                        MinimumOrderQuantity = quotation?.MinimumOrderQuantity ?? partSupplier.MinimumOrderQuantity,
                        LeadTimeDays = quotation?.LeadTimeDays ?? partSupplier.LeadTimeDays,
                        TotalPrice = totalPrice,
                        AverageRating = rating != null ? rating.AvgRating : supplier.Rating,
                        RatingCount = rating?.Count ?? 0,
                        OnTimeDeliveryRate = performance?.OnTimeDeliveryRate ?? 0,
                        DefectRate = performance?.DefectRate ?? 0,
                        OverallScore = performance?.OverallScore ?? score,
                        CalculatedScore = score,
                        IsPreferred = partSupplier.IsPreferred,
                        QuotationNumber = quotation?.QuotationNumber,
                        QuotationDate = quotation?.QuotationDate,
                        ValidUntil = quotation?.ValidUntil,
                        WarrantyPeriod = quotation?.WarrantyPeriod,
                        Notes = partSupplier.Notes
                    });
                }

                // Sort by calculated score (descending)
                comparisons = comparisons.OrderByDescending(c => c.CalculatedScore).ToList();

                return Ok(ApiResponse<List<SupplierComparisonDto>>.SuccessResult(
                    comparisons, 
                    $"So sánh {comparisons.Count} nhà cung cấp cho phụ tùng: {part.PartName}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supplier comparison");
                return StatusCode(500, ApiResponse<List<SupplierComparisonDto>>.ErrorResult(
                    "Lỗi khi so sánh nhà cung cấp", ex));
            }
        }

        /// <summary>
        /// GET /api/procurement/supplier-recommendation
        /// Đề xuất nhà cung cấp tốt nhất cho một phụ tùng
        /// </summary>
        [HttpGet("supplier-recommendation")]
        public async Task<ActionResult<ApiResponse<SupplierRecommendationDto>>> GetSupplierRecommendation(
            [FromQuery] int partId,
            [FromQuery] int quantity = 1)
        {
            try
            {
                // Validate part exists
                var part = await _unitOfWork.Parts.GetByIdAsync(partId);
                if (part == null || part.IsDeleted)
                {
                    return NotFound(ApiResponse<SupplierRecommendationDto>.ErrorResult(
                        "Không tìm thấy phụ tùng"));
                }

                // Get supplier comparison data directly (avoid calling action method)
                var partSuppliers = await _context.PartSuppliers
                    .AsNoTracking()
                    .Include(ps => ps.Supplier)
                    .Where(ps => ps.PartId == partId && 
                                !ps.IsDeleted && 
                                ps.IsActive &&
                                ps.Supplier != null &&
                                !ps.Supplier.IsDeleted &&
                                ps.Supplier.IsActive)
                    .ToListAsync();

                if (!partSuppliers.Any())
                {
                    return NotFound(ApiResponse<SupplierRecommendationDto>.ErrorResult(
                        "Không có nhà cung cấp nào cho phụ tùng này"));
                }

                var supplierIds = partSuppliers.Select(ps => ps.SupplierId).Distinct().ToList();

                // ✅ OPTIMIZED: Parallel queries for better performance
                // ✅ SAFETY: Use FirstOrDefault to avoid exception if group is empty
                var performancesTask = _context.Set<SupplierPerformance>()
                    .AsNoTracking()
                    .Where(sp => supplierIds.Contains(sp.SupplierId) && 
                                (sp.PartId == partId || sp.PartId == null) &&
                                !sp.IsDeleted)
                    .GroupBy(sp => sp.SupplierId)
                    .Where(g => g.Any())
                    .Select(g => new { 
                        SupplierId = g.Key, 
                        Performance = g.OrderByDescending(sp => sp.CalculatedAt).FirstOrDefault() 
                    })
                    .Where(p => p.Performance != null)
                    .ToListAsync();

                // ✅ OPTIMIZED: GroupBy at database level instead of in memory
                var ratingsTask = _context.Set<SupplierRating>()
                    .AsNoTracking()
                    .Where(sr => supplierIds.Contains(sr.SupplierId) && 
                                (sr.PartId == partId || sr.PartId == null) &&
                                !sr.IsDeleted)
                    .GroupBy(sr => sr.SupplierId)
                    .Where(g => g.Any())
                    .Select(g => new { 
                        SupplierId = g.Key, 
                        AvgRating = (decimal)g.Average(sr => (double)sr.Rating), 
                        Count = g.Count() 
                    })
                    .ToListAsync();

                // ✅ OPTIMIZED: Load only most recent quotation per supplier at DB level
                // ✅ SAFETY: Use FirstOrDefault to avoid exception if group is empty
                var quotationsTask = _context.Set<SupplierQuotation>()
                    .AsNoTracking()
                    .Where(sq => sq.PartId == partId && 
                                supplierIds.Contains(sq.SupplierId) &&
                                !sq.IsDeleted &&
                                sq.Status == "Accepted" &&
                                (sq.ValidUntil == null || sq.ValidUntil >= DateTime.Now))
                    .GroupBy(sq => sq.SupplierId)
                    .Where(g => g.Any())
                    .Select(g => g.OrderByDescending(sq => sq.QuotationDate).FirstOrDefault())
                    .Where(q => q != null)
                    .ToListAsync();

                // ✅ OPTIMIZED: Execute all queries in parallel
                await Task.WhenAll(performancesTask, ratingsTask, quotationsTask);

                var performances = await performancesTask;
                var ratings = await ratingsTask;
                var quotations = await quotationsTask;

                // ✅ SAFETY: Use safe dictionary creation to avoid duplicate key exceptions
                var performanceDict = performances
                    .Where(p => p != null && p.Performance != null)
                    .GroupBy(p => p.SupplierId)
                    .ToDictionary(g => g.Key, g => g.First().Performance);
                var ratingDict = ratings
                    .Where(r => r != null)
                    .GroupBy(r => r.SupplierId)
                    .ToDictionary(g => g.Key, g => g.First());
                var quotationDict = quotations
                    .Where(q => q != null)
                    .GroupBy(q => q.SupplierId)
                    .ToDictionary(g => g.Key, g => g.First());

                // Build comparison list - First pass: collect all prices for comparison
                var priceList = new List<decimal>();
                foreach (var partSupplier in partSuppliers)
                {
                    quotationDict.TryGetValue(partSupplier.SupplierId, out var quotation);
                    var unitPrice = quotation?.UnitPrice ?? partSupplier.CostPrice;
                    priceList.Add(unitPrice);
                }
                var minPrice = priceList.Any() ? priceList.Min() : 0;
                var maxPrice = priceList.Any() ? priceList.Max() : 0;
                var avgPrice = priceList.Any() ? priceList.Average() : 0;

                var comparisons = new List<SupplierComparisonDto>();

                foreach (var partSupplier in partSuppliers)
                {
                    var supplier = partSupplier.Supplier;
                    // ✅ SAFETY: Check supplier is not null (should not happen due to filter, but defensive)
                    if (supplier == null) continue;
                    
                    performanceDict.TryGetValue(supplier.Id, out var performance);
                    ratingDict.TryGetValue(supplier.Id, out var rating);
                    quotationDict.TryGetValue(supplier.Id, out var quotation);

                    var unitPrice = quotation?.UnitPrice ?? partSupplier.CostPrice;
                    // ✅ SAFETY: Validate quantity before multiplication
                    var safeQuantity = quantity > 0 ? quantity : 1;
                    var totalPrice = unitPrice * safeQuantity;

                    var avgRating = rating != null ? rating.AvgRating : supplier.Rating;
                    var score = CalculateSupplierScore(
                        performance,
                        avgRating,
                        unitPrice,
                        minPrice,
                        maxPrice,
                        avgPrice,
                        partSupplier.LeadTimeDays,
                        quotation?.LeadTimeDays);

                    comparisons.Add(new SupplierComparisonDto
                    {
                        SupplierId = supplier.Id,
                        SupplierName = supplier.SupplierName,
                        SupplierCode = supplier.SupplierCode,
                        ContactPerson = supplier.ContactPerson,
                        Phone = supplier.Phone ?? supplier.ContactPhone,
                        Email = supplier.Email,
                        UnitPrice = unitPrice,
                        MinimumOrderQuantity = quotation?.MinimumOrderQuantity ?? partSupplier.MinimumOrderQuantity,
                        LeadTimeDays = quotation?.LeadTimeDays ?? partSupplier.LeadTimeDays,
                        TotalPrice = totalPrice,
                        AverageRating = avgRating,
                        RatingCount = rating?.Count ?? 0,
                        OnTimeDeliveryRate = performance?.OnTimeDeliveryRate ?? 0,
                        DefectRate = performance?.DefectRate ?? 0,
                        OverallScore = performance?.OverallScore ?? score,
                        CalculatedScore = score,
                        IsPreferred = partSupplier.IsPreferred,
                        QuotationNumber = quotation?.QuotationNumber,
                        QuotationDate = quotation?.QuotationDate,
                        ValidUntil = quotation?.ValidUntil,
                        WarrantyPeriod = quotation?.WarrantyPeriod,
                        Notes = partSupplier.Notes
                    });
                }

                // Sort by calculated score (descending)
                comparisons = comparisons.OrderByDescending(c => c.CalculatedScore).ToList();

                // ✅ SAFETY: Check if comparisons list is not empty
                if (!comparisons.Any())
                {
                    return NotFound(ApiResponse<SupplierRecommendationDto>.ErrorResult(
                        "Không có nhà cung cấp hợp lệ để đề xuất"));
                }

                // ✅ SAFETY: Find best supplier (highest score) - should always have at least one
                var bestSupplier = comparisons.OrderByDescending(c => c.CalculatedScore).FirstOrDefault();
                if (bestSupplier == null)
                {
                    return NotFound(ApiResponse<SupplierRecommendationDto>.ErrorResult(
                        "Không thể xác định nhà cung cấp tốt nhất"));
                }
                var allSuppliers = comparisons;

                // ✅ SAFETY: Check if allSuppliers is not empty before Average
                var averagePrice = allSuppliers.Any() ? allSuppliers.Average(s => s.UnitPrice) : 0;
                var priceDifference = averagePrice - bestSupplier.UnitPrice;
                var priceDifferencePercent = averagePrice > 0 
                    ? (priceDifference / averagePrice) * 100 
                    : 0;

                var recommendation = new SupplierRecommendationDto
                {
                    PartId = partId,
                    PartNumber = part.PartNumber,
                    PartName = part.PartName,
                    RecommendedQuantity = quantity,
                    RecommendedSupplier = bestSupplier,
                    AllSuppliers = allSuppliers,
                    AveragePrice = averagePrice,
                    PriceDifference = priceDifference,
                    PriceDifferencePercent = priceDifferencePercent,
                    RecommendationReason = GetRecommendationReason(bestSupplier, allSuppliers),
                    RecommendedAt = DateTime.Now
                };

                return Ok(ApiResponse<SupplierRecommendationDto>.SuccessResult(
                    recommendation, 
                    $"Đề xuất nhà cung cấp tốt nhất: {bestSupplier.SupplierName}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supplier recommendation");
                return StatusCode(500, ApiResponse<SupplierRecommendationDto>.ErrorResult(
                    "Lỗi khi đề xuất nhà cung cấp", ex));
            }
        }

        #region Helper Methods - Supplier Analysis

        private decimal CalculateSupplierScore(
            SupplierPerformance? performance,
            decimal rating,
            decimal unitPrice,
            decimal minPrice,
            decimal maxPrice,
            decimal avgPrice,
            int leadTimeDays,
            int? quotationLeadTimeDays)
        {
            // Score components (0-100 total)
            // 1. Rating: 0-30 points (1-5 stars -> 0-30)
            var ratingScore = (rating / 5.0m) * 30;

            // 2. Performance: 0-40 points
            var performanceScore = 0m;
            if (performance != null)
            {
                // On-time delivery: 0-20 points
                performanceScore += (performance.OnTimeDeliveryRate / 100.0m) * 20;
                
                // Low defect rate: 0-20 points (inverse: lower defect = higher score)
                performanceScore += ((100 - performance.DefectRate) / 100.0m) * 20;
            }
            else
            {
                // Default performance score if no data
                performanceScore = 20m;
            }

            // 3. Price competitiveness: 0-20 points (lower price = higher score)
            var priceScore = 0m;
            if (maxPrice > minPrice && avgPrice > 0 && maxPrice != minPrice)
            {
                // ✅ SAFETY: Prevent division by zero (maxPrice != minPrice already checked above)
                // Normalize price: lower price gets higher score
                // If price = minPrice: 20 points
                // If price = maxPrice: 0 points
                // Linear interpolation
                var priceDiff = maxPrice - minPrice;
                if (priceDiff > 0)
                {
                    var priceRatio = (unitPrice - minPrice) / priceDiff;
                    priceScore = 20m * (1m - priceRatio);
                }
                else
                {
                    priceScore = 10m; // All prices same
                }
            }
            else if (unitPrice > 0 && avgPrice > 0)
            {
                // If all prices are same, give average score
                priceScore = 10m;
            }
            else
            {
                priceScore = 10m; // Default
            }

            // 4. Lead time: 0-10 points (shorter is better)
            var actualLeadTime = quotationLeadTimeDays ?? leadTimeDays;
            var leadTimeScore = actualLeadTime <= 3 ? 10m :
                               actualLeadTime <= 7 ? 8m :
                               actualLeadTime <= 14 ? 6m :
                               actualLeadTime <= 30 ? 4m : 2m;

            return ratingScore + performanceScore + priceScore + leadTimeScore;
        }

        private string GetRecommendationReason(
            SupplierComparisonDto bestSupplier,
            List<SupplierComparisonDto> allSuppliers)
        {
            var reasons = new List<string>();

            if (bestSupplier.CalculatedScore >= 80)
                reasons.Add("Điểm số tổng thể cao");

            if (bestSupplier.AverageRating >= 4.5m)
                reasons.Add("Đánh giá tốt từ người dùng");

            if (bestSupplier.OnTimeDeliveryRate >= 90)
                reasons.Add("Tỷ lệ giao hàng đúng hạn cao");

            if (bestSupplier.DefectRate <= 2)
                reasons.Add("Tỷ lệ lỗi thấp");

            // ✅ SAFETY: Check if allSuppliers is not empty before Average
            if (allSuppliers.Any())
            {
                var avgPrice = allSuppliers.Average(s => s.UnitPrice);
                if (bestSupplier.UnitPrice < avgPrice * 0.95m)
                    reasons.Add("Giá cạnh tranh");
            }

            if (bestSupplier.LeadTimeDays <= 7)
                reasons.Add("Thời gian giao hàng nhanh");

            if (bestSupplier.IsPreferred)
                reasons.Add("Nhà cung cấp ưu tiên");

            return reasons.Any() 
                ? string.Join(", ", reasons) 
                : "Nhà cung cấp được đề xuất dựa trên điểm số tổng thể";
    }

    #endregion

        #region Phase 4.2.4: Performance Evaluation

        /// <summary>
        /// GET /api/procurement/supplier-performance-report
        /// Lấy báo cáo hiệu suất nhà cung cấp
        /// </summary>
        [HttpGet("supplier-performance-report")]
        public async Task<ActionResult<PagedResponse<SupplierPerformanceReportDto>>> GetSupplierPerformanceReport(
            [FromQuery] int? supplierId = null,
            [FromQuery] int? partId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                // Validate pagination
                if (pageSize <= 0) pageSize = 20;
                if (pageNumber <= 0) pageNumber = 1;
                if (pageSize > 100) pageSize = 100;

                var query = _context.Set<SupplierPerformance>()
                    .AsNoTracking()
                    .Include(sp => sp.Supplier)
                    .Include(sp => sp.Part)
                    .Where(sp => !sp.IsDeleted)
                    .AsQueryable();

                if (supplierId.HasValue)
                    query = query.Where(sp => sp.SupplierId == supplierId.Value);

                if (partId.HasValue)
                    query = query.Where(sp => sp.PartId == partId.Value);
                else
                    // Default: chỉ lấy performance chung (PartId = null)
                    query = query.Where(sp => sp.PartId == null);

                if (startDate.HasValue)
                    query = query.Where(sp => sp.CalculatedAt >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(sp => sp.CalculatedAt <= endDate.Value);

                var totalCount = await query.CountAsync();

                var performances = await query
                    .OrderByDescending(sp => sp.OverallScore)
                    .ThenByDescending(sp => sp.CalculatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var dtos = performances.Select(sp => new SupplierPerformanceReportDto
                {
                    SupplierId = sp.SupplierId,
                    SupplierName = sp.Supplier?.SupplierName ?? "N/A",
                    SupplierCode = sp.Supplier?.SupplierCode ?? "",
                    TotalOrders = sp.TotalOrders,
                    OnTimeDeliveries = sp.OnTimeDeliveries,
                    OnTimeDeliveryRate = sp.OnTimeDeliveryRate,
                    AverageLeadTimeDays = sp.AverageLeadTimeDays,
                    DefectRate = sp.DefectRate,
                    AveragePrice = sp.AveragePrice,
                    PriceStability = sp.PriceStability,
                    OverallScore = sp.OverallScore,
                    CalculatedAt = sp.CalculatedAt,
                    PartId = sp.PartId,
                    PartNumber = sp.Part?.PartNumber,
                    PartName = sp.Part?.PartName
                }).ToList();

                return Ok(PagedResponse<SupplierPerformanceReportDto>.CreateSuccessResult(
                    dtos, pageNumber, pageSize, totalCount, "Supplier performance report retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supplier performance report");
                return StatusCode(500, PagedResponse<SupplierPerformanceReportDto>.CreateErrorResult("Error retrieving supplier performance report"));
            }
        }

        /// <summary>
        /// GET /api/procurement/supplier-ranking
        /// Lấy ranking nhà cung cấp theo các tiêu chí
        /// </summary>
        [HttpGet("supplier-ranking")]
        public async Task<ActionResult<ApiResponse<List<SupplierRankingDto>>>> GetSupplierRanking(
            [FromQuery] string? sortBy = "OverallScore", // "OverallScore", "OnTimeDelivery", "DefectRate", "Price", "LeadTime"
            [FromQuery] int? topN = null,
            [FromQuery] bool worstPerformers = false)
        {
            try
            {
                var query = _context.Set<SupplierPerformance>()
                    .AsNoTracking()
                    .Include(sp => sp.Supplier)
                    .Where(sp => !sp.IsDeleted && sp.PartId == null) // Chỉ lấy performance chung
                    .AsQueryable();

                IQueryable<SupplierPerformance> orderedQuery = sortBy?.ToLower() switch
                {
                    "ontimedelivery" => worstPerformers
                        ? query.OrderBy(sp => sp.OnTimeDeliveryRate)
                        : query.OrderByDescending(sp => sp.OnTimeDeliveryRate),
                    "defectrate" => worstPerformers
                        ? query.OrderByDescending(sp => sp.DefectRate)
                        : query.OrderBy(sp => sp.DefectRate),
                    "price" => worstPerformers
                        ? query.OrderByDescending(sp => sp.AveragePrice)
                        : query.OrderBy(sp => sp.AveragePrice),
                    "leadtime" => worstPerformers
                        ? query.OrderByDescending(sp => sp.AverageLeadTimeDays)
                        : query.OrderBy(sp => sp.AverageLeadTimeDays),
                    _ => worstPerformers
                        ? query.OrderBy(sp => sp.OverallScore)
                        : query.OrderByDescending(sp => sp.OverallScore)
                };

                if (topN.HasValue && topN.Value > 0)
                    orderedQuery = orderedQuery.Take(topN.Value);

                var performances = await orderedQuery.ToListAsync();

                var rankings = performances.Select((sp, index) => new SupplierRankingDto
                {
                    Rank = index + 1,
                    SupplierId = sp.SupplierId,
                    SupplierName = sp.Supplier?.SupplierName ?? "N/A",
                    SupplierCode = sp.Supplier?.SupplierCode ?? "",
                    OverallScore = sp.OverallScore,
                    OnTimeDeliveryRate = sp.OnTimeDeliveryRate,
                    DefectRate = sp.DefectRate,
                    AveragePrice = sp.AveragePrice,
                    AverageLeadTimeDays = sp.AverageLeadTimeDays,
                    TotalOrders = sp.TotalOrders,
                    PerformanceCategory = sp.OverallScore >= 80 ? "Excellent" :
                                        sp.OverallScore >= 60 ? "Good" :
                                        sp.OverallScore >= 40 ? "Average" : "Poor"
                }).ToList();

                return Ok(ApiResponse<List<SupplierRankingDto>>.SuccessResult(
                    rankings, "Supplier ranking retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supplier ranking");
                return StatusCode(500, ApiResponse<List<SupplierRankingDto>>.ErrorResult("Error retrieving supplier ranking"));
            }
        }

        /// <summary>
        /// GET /api/procurement/performance-alerts
        /// Lấy danh sách cảnh báo hiệu suất nhà cung cấp
        /// </summary>
        [HttpGet("performance-alerts")]
        public async Task<ActionResult<ApiResponse<List<PerformanceAlertDto>>>> GetPerformanceAlerts(
            [FromQuery] string? severity = null)
        {
            try
            {
                var performances = await _context.Set<SupplierPerformance>()
                    .AsNoTracking()
                    .Include(sp => sp.Supplier)
                    .Where(sp => !sp.IsDeleted && sp.PartId == null && sp.TotalOrders > 0)
                    .ToListAsync();

                var alerts = new List<PerformanceAlertDto>();

                foreach (var perf in performances)
                {
                    // Alert 1: Low On-Time Delivery Rate (< 80%)
                    if (perf.OnTimeDeliveryRate < 80)
                    {
                        alerts.Add(new PerformanceAlertDto
                        {
                            SupplierId = perf.SupplierId,
                            SupplierName = perf.Supplier?.SupplierName ?? "N/A",
                            AlertType = "LowOnTimeDelivery",
                            AlertMessage = $"Tỷ lệ giao hàng đúng hạn thấp: {perf.OnTimeDeliveryRate:F1}%",
                            CurrentValue = perf.OnTimeDeliveryRate,
                            ThresholdValue = 80,
                            Severity = perf.OnTimeDeliveryRate < 50 ? "High" : perf.OnTimeDeliveryRate < 70 ? "Medium" : "Low",
                            AlertDate = perf.CalculatedAt
                        });
                    }

                    // Alert 2: High Defect Rate (> 5%)
                    if (perf.DefectRate > 5)
                    {
                        alerts.Add(new PerformanceAlertDto
                        {
                            SupplierId = perf.SupplierId,
                            SupplierName = perf.Supplier?.SupplierName ?? "N/A",
                            AlertType = "HighDefectRate",
                            AlertMessage = $"Tỷ lệ lỗi cao: {perf.DefectRate:F1}%",
                            CurrentValue = perf.DefectRate,
                            ThresholdValue = 5,
                            Severity = perf.DefectRate > 10 ? "High" : perf.DefectRate > 7 ? "Medium" : "Low",
                            AlertDate = perf.CalculatedAt
                        });
                    }

                    // Alert 3: Low Overall Score (< 50)
                    if (perf.OverallScore < 50)
                    {
                        alerts.Add(new PerformanceAlertDto
                        {
                            SupplierId = perf.SupplierId,
                            SupplierName = perf.Supplier?.SupplierName ?? "N/A",
                            AlertType = "LowScore",
                            AlertMessage = $"Điểm tổng thể thấp: {perf.OverallScore:F1}/100",
                            CurrentValue = perf.OverallScore,
                            ThresholdValue = 50,
                            Severity = perf.OverallScore < 30 ? "High" : "Medium",
                            AlertDate = perf.CalculatedAt
                        });
                    }
                }

                // Filter by severity if provided
                if (!string.IsNullOrEmpty(severity))
                {
                    alerts = alerts.Where(a => a.Severity.Equals(severity, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // Sort by severity (High > Medium > Low) then by alert date
                alerts = alerts.OrderByDescending(a => a.Severity == "High" ? 3 : a.Severity == "Medium" ? 2 : 1)
                              .ThenByDescending(a => a.AlertDate)
                              .ToList();

                return Ok(ApiResponse<List<PerformanceAlertDto>>.SuccessResult(
                    alerts, "Performance alerts retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance alerts");
                return StatusCode(500, ApiResponse<List<PerformanceAlertDto>>.ErrorResult("Error retrieving performance alerts"));
            }
        }

        /// <summary>
        /// POST /api/procurement/calculate-performance
        /// Tính toán lại hiệu suất nhà cung cấp
        /// </summary>
        [HttpPost("calculate-performance")]
        public async Task<ActionResult<ApiResponse<object>>> CalculatePerformance([FromBody] CalculatePerformanceRequestDto? request)
        {
            try
            {
                var supplierId = request?.SupplierId;
                var partId = request?.PartId;
                var startDate = request?.StartDate ?? DateTime.Now.AddMonths(-6);
                var endDate = request?.EndDate ?? DateTime.Now;
                var forceRecalculate = request?.ForceRecalculate ?? false;

                // Get suppliers to calculate
                var suppliersQuery = _context.Suppliers
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .AsQueryable();

                if (supplierId.HasValue)
                    suppliersQuery = suppliersQuery.Where(s => s.Id == supplierId.Value);

                var suppliers = await suppliersQuery.ToListAsync();

                var calculatedCount = 0;

                foreach (var supplier in suppliers)
                {
                    // Get purchase orders for this supplier
                    var purchaseOrders = await _context.PurchaseOrders
                        .Include(po => po.PurchaseOrderItems)
                        .Where(po => !po.IsDeleted && 
                                   po.SupplierId == supplier.Id &&
                                   po.OrderDate >= startDate &&
                                   po.OrderDate <= endDate &&
                                   (po.Status == "Received" || po.Status == "PartiallyReceived"))
                        .ToListAsync();

                    if (!purchaseOrders.Any() && !forceRecalculate)
                        continue;

                    // Calculate metrics
                    var totalOrders = purchaseOrders.Count;
                    var onTimeDeliveries = purchaseOrders.Count(po => 
                        po.ExpectedDeliveryDate.HasValue && 
                        po.ReceivedDate.HasValue &&
                        po.ReceivedDate.Value <= po.ExpectedDeliveryDate.Value.AddDays(1)); // Allow 1 day tolerance

                    var onTimeDeliveryRate = totalOrders > 0 
                        ? (decimal)onTimeDeliveries / totalOrders * 100 
                        : 0;

                    var leadTimes = purchaseOrders
                        .Where(po => po.SentDate.HasValue && po.ReceivedDate.HasValue)
                        .Select(po => (po.ReceivedDate!.Value - po.SentDate!.Value).Days)
                        .ToList();

                    var averageLeadTimeDays = leadTimes.Any() 
                        ? (int)leadTimes.Average() 
                        : 0;

                    // Calculate defect rate from QC (if available)
                    var defectRate = 0m;
                    // TODO: Calculate from QC data if available

                    // Calculate average price
                    var allItems = purchaseOrders
                        .SelectMany(po => po.PurchaseOrderItems ?? new List<PurchaseOrderItem>())
                        .Where(item => !item.IsDeleted)
                        .ToList();

                    var averagePrice = allItems.Any() 
                        ? allItems.Average(item => item.UnitPrice) 
                        : 0;

                    // Calculate price stability (coefficient of variation)
                    var priceStability = 0m;
                    if (allItems.Count > 1)
                    {
                        var prices = allItems.Select(item => (double)item.UnitPrice).ToList();
                        var mean = prices.Average();
                        var variance = prices.Sum(p => Math.Pow(p - mean, 2)) / prices.Count;
                        var stdDev = Math.Sqrt(variance);
                        priceStability = mean > 0 ? (decimal)(stdDev / mean * 100) : 0;
                    }

                    // Calculate overall score
                    var overallScore = (onTimeDeliveryRate * 0.4m) + 
                                     ((100 - defectRate) * 0.3m) + 
                                     (Math.Max(0, 100 - priceStability) * 0.2m) + 
                                     (Math.Max(0, 100 - averageLeadTimeDays * 2) * 0.1m);
                    overallScore = Math.Min(100, Math.Max(0, overallScore));

                    // Get or create SupplierPerformance record
                    var performanceQuery = _context.Set<SupplierPerformance>()
                        .Where(sp => sp.SupplierId == supplier.Id && sp.PartId == partId);

                    var performance = await performanceQuery.FirstOrDefaultAsync();

                    if (performance == null)
                    {
                        performance = new SupplierPerformance
                        {
                            SupplierId = supplier.Id,
                            PartId = partId,
                            CalculatedAt = DateTime.Now
                        };
                        _context.Set<SupplierPerformance>().Add(performance);
                    }
                    else if (!forceRecalculate && performance.CalculatedAt > endDate.AddDays(-1))
                    {
                        // Skip if recently calculated
                        continue;
                    }

                    // Update performance data
                    performance.TotalOrders = totalOrders;
                    performance.OnTimeDeliveries = onTimeDeliveries;
                    performance.OnTimeDeliveryRate = onTimeDeliveryRate;
                    performance.AverageLeadTimeDays = averageLeadTimeDays;
                    performance.DefectRate = defectRate;
                    performance.AveragePrice = averagePrice;
                    performance.PriceStability = priceStability;
                    performance.OverallScore = overallScore;
                    performance.CalculatedAt = DateTime.Now;

                    calculatedCount++;
                }

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResult(
                    new { CalculatedCount = calculatedCount, Message = "Performance calculation completed successfully" },
                    "Performance calculation completed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating supplier performance");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Error calculating supplier performance"));
            }
        }

        #endregion
    }

}

