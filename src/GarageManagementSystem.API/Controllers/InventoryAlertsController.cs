using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryAlertsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<InventoryAlertsController> _logger;

        public InventoryAlertsController(IUnitOfWork unitOfWork, ILogger<InventoryAlertsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Get low stock alerts
        /// </summary>
        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStockAlerts()
        {
            try
            {
                var parts = await _unitOfWork.Parts.GetAllAsync();
                
                var lowStockParts = parts
                    .Where(p => p.QuantityInStock <= p.MinimumStock && p.QuantityInStock > 0)
                    .Select(p => new
                    {
                        p.Id,
                        p.PartName,
                        p.PartNumber,
                        CurrentStock = p.QuantityInStock,
                        MinStock = p.MinimumStock,
                        Deficit = p.MinimumStock - p.QuantityInStock,
                        ReorderQuantity = (p.ReorderLevel ?? p.MinimumStock * 2) - p.QuantityInStock,
                        EstimatedCost = ((p.ReorderLevel ?? p.MinimumStock * 2) - p.QuantityInStock) * p.CostPrice,
                        AlertLevel = p.QuantityInStock == 0 ? "Critical" : 
                                    p.QuantityInStock <= p.MinimumStock * 0.5m ? "High" : "Medium",
                        p.Location
                    })
                    .OrderBy(p => p.CurrentStock)
                    .ToList();

                return Ok(new { success = true, data = lowStockParts, count = lowStockParts.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting low stock alerts");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy cảnh báo tồn kho thấp" });
            }
        }

        /// <summary>
        /// Get out of stock items
        /// </summary>
        [HttpGet("out-of-stock")]
        public async Task<IActionResult> GetOutOfStockAlerts()
        {
            try
            {
                var parts = await _unitOfWork.Parts.GetAllAsync();
                
                var outOfStockParts = parts
                    .Where(p => p.QuantityInStock == 0)
                    .Select(p => new
                    {
                        p.Id,
                        p.PartName,
                        p.PartNumber,
                        ReorderQuantity = p.ReorderLevel ?? p.MinimumStock * 2,
                        EstimatedCost = (p.ReorderLevel ?? p.MinimumStock * 2) * p.CostPrice,
                        // Check last usage
                        DaysSinceOutOfStock = 0, // Would need last transaction date
                        p.Location
                    })
                    .ToList();

                return Ok(new { success = true, data = outOfStockParts, count = outOfStockParts.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting out of stock alerts");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy cảnh báo hết hàng" });
            }
        }

        /// <summary>
        /// Get overstock alerts
        /// </summary>
        [HttpGet("overstock")]
        public async Task<IActionResult> GetOverstockAlerts()
        {
            try
            {
                var parts = await _unitOfWork.Parts.GetAllAsync();
                
                var overstockParts = parts
                    .Where(p => p.ReorderLevel.HasValue && p.QuantityInStock > p.ReorderLevel * 3)
                    .Select(p => new
                    {
                        p.Id,
                        p.PartName,
                        p.PartNumber,
                        CurrentStock = p.QuantityInStock,
                        MaxStock = p.ReorderLevel * 3,
                        Excess = p.QuantityInStock - (p.ReorderLevel * 3),
                        TiedUpValue = (p.QuantityInStock - (p.ReorderLevel * 3)) * p.CostPrice,
                        p.Location
                    })
                    .OrderByDescending(p => p.TiedUpValue)
                    .ToList();

                return Ok(new { success = true, data = overstockParts, count = overstockParts.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overstock alerts");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy cảnh báo tồn kho cao" });
            }
        }

        /// <summary>
        /// Generate auto-reorder suggestions
        /// </summary>
        [HttpGet("reorder-suggestions")]
        public async Task<IActionResult> GetReorderSuggestions()
        {
            try
            {
                var parts = await _unitOfWork.Parts.GetAllAsync();
                var stockTransactions = await _unitOfWork.StockTransactions.GetAllAsync();
                
                // Parts that need reordering
                var reorderParts = parts
                    .Where(p => p.QuantityInStock <= p.MinimumStock)
                    .Select(p =>
                    {
                        // Calculate avg usage in last 30 days
                        var last30Days = stockTransactions
                            .Where(st => st.PartId == p.Id && 
                                        st.TransactionType.ToString() == "Out" &&
                                        st.TransactionDate >= DateTime.Now.AddDays(-30))
                            .Sum(st => st.Quantity);
                        
                        var avgDailyUsage = last30Days / 30.0m;
                        var maxStock = p.ReorderLevel ?? p.MinimumStock * 2;
                        var suggestedQty = Math.Max(maxStock - p.QuantityInStock, (int)(avgDailyUsage * 30)); // 30 days supply

                        return new
                        {
                            p.Id,
                            p.PartName,
                            p.PartNumber,
                            CurrentStock = p.QuantityInStock,
                            MinStock = p.MinimumStock,
                            MaxStock = maxStock,
                            AvgDailyUsage = Math.Round(avgDailyUsage, 2),
                            Usage30Days = last30Days,
                            SuggestedOrderQuantity = suggestedQty,
                            EstimatedCost = suggestedQty * p.CostPrice,
                            Priority = p.QuantityInStock == 0 ? "Urgent" :
                                      p.QuantityInStock <= p.MinimumStock * 0.5m ? "High" : "Normal",
                            LeadTime = "7 days" // Would come from supplier data
                        };
                    })
                    .OrderBy(p => p.CurrentStock)
                    .ToList();

                var totalCost = reorderParts.Sum(p => p.EstimatedCost);

                return Ok(new 
                { 
                    success = true, 
                    data = reorderParts, 
                    count = reorderParts.Count,
                    totalEstimatedCost = totalCost
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reorder suggestions");
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo gợi ý đặt hàng" });
            }
        }

        /// <summary>
        /// Create purchase orders from reorder suggestions
        /// </summary>
        [HttpPost("auto-reorder")]
        public async Task<IActionResult> CreateAutoReorderPurchaseOrders([FromBody] AutoReorderRequest request)
        {
            try
            {
                var parts = await _unitOfWork.Parts.GetAllAsync();
                var partsToOrder = parts.Where(p => request.PartIds.Contains(p.Id)).ToList();

                if (!partsToOrder.Any())
                    return BadRequest(new { success = false, message = "Không tìm thấy phụ tùng cần đặt hàng" });

                // Group by supplier (would need part-supplier relationship)
                var ordersBySupplier = new List<object>();

                foreach (var part in partsToOrder)
                {
                    var maxStock = part.ReorderLevel ?? part.MinimumStock * 2;
                    var suggestedQty = maxStock - part.QuantityInStock;
                    if (suggestedQty <= 0) continue;

                    ordersBySupplier.Add(new
                    {
                        PartId = part.Id,
                        PartName = part.PartName,
                        Quantity = suggestedQty,
                        EstimatedCost = suggestedQty * part.CostPrice
                    });
                }

                return Ok(new 
                { 
                    success = true, 
                    message = $"Đã tạo đề xuất đặt hàng cho {partsToOrder.Count} phụ tùng",
                    data = ordersBySupplier
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating auto reorder");
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo đơn đặt hàng tự động" });
            }
        }

        /// <summary>
        /// Get expiring parts (batches near expiry)
        /// </summary>
        [HttpGet("expiring-soon")]
        public async Task<IActionResult> GetExpiringSoonAlerts([FromQuery] int daysAhead = 30)
        {
            try
            {
                var batches = await _unitOfWork.Repository<PartInventoryBatch>().GetAllAsync();
                var expiringDate = DateTime.Now.AddDays(daysAhead);
                
                var expiringBatches = batches
                    .Where(b => b.ExpiryDate.HasValue && 
                               b.ExpiryDate <= expiringDate && 
                               b.QuantityRemaining > 0)
                    .Select(b => new
                    {
                        b.Id,
                        b.BatchNumber,
                        b.PartId,
                        b.ExpiryDate,
                        DaysUntilExpiry = (b.ExpiryDate!.Value - DateTime.Now).Days,
                        b.QuantityRemaining,
                        ValueAtRisk = b.QuantityRemaining * b.UnitCost,
                        AlertLevel = (b.ExpiryDate!.Value - DateTime.Now).Days <= 7 ? "Critical" :
                                    (b.ExpiryDate!.Value - DateTime.Now).Days <= 14 ? "High" : "Medium"
                    })
                    .OrderBy(b => b.DaysUntilExpiry)
                    .ToList();

                return Ok(new { success = true, data = expiringBatches, count = expiringBatches.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expiring alerts");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy cảnh báo hết hạn" });
            }
        }
    }

    public class AutoReorderRequest
    {
        public List<int> PartIds { get; set; } = new();
        public int? PreferredSupplierId { get; set; }
    }
}

