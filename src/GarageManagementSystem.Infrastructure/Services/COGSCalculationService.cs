using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GarageManagementSystem.Infrastructure.Services
{
    /// <summary>
    /// Service tính toán COGS (Cost of Goods Sold) theo FIFO và Bình quân gia quyền
    /// </summary>
    public class COGSCalculationService : ICOGSCalculationService
    {
        private readonly GarageDbContext _context;
        private readonly ILogger<COGSCalculationService> _logger;

        public COGSCalculationService(GarageDbContext context, ILogger<COGSCalculationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Tính COGS theo phương pháp FIFO
        /// </summary>
        public async Task<COGSCalculationResult> CalculateCOGSByFIFOAsync(int serviceOrderId)
        {
            try
            {
                // Lấy tất cả PartBatchUsage của ServiceOrder này
                // ✅ FIX: Filter null PartInventoryBatch trong query để tránh lỗi khi OrderBy
                var batchUsages = await _context.PartBatchUsages
                    .Include(pbu => pbu.PartInventoryBatch)
                        .ThenInclude(batch => batch.Part)
                    .Where(pbu => pbu.ServiceOrderId == serviceOrderId 
                        && pbu.PartInventoryBatch != null 
                        && pbu.PartInventoryBatch.Part != null)
                    .OrderBy(pbu => pbu.PartInventoryBatch!.ReceiveDate) // Sắp xếp theo ngày nhập (cũ nhất trước)
                    .ToListAsync();

                if (!batchUsages.Any())
                {
                    _logger.LogWarning($"Không có vật tư nào đã sử dụng cho ServiceOrder {serviceOrderId}");
                    return new COGSCalculationResult
                    {
                        ServiceOrderId = serviceOrderId,
                        CalculationMethod = "FIFO",
                        TotalCOGS = 0,
                        CalculationDate = DateTime.Now,
                        ItemDetails = new List<COGSItemDetail>()
                    };
                }

                // ✅ FIX: Filter null PartInventoryBatch trước khi GroupBy để tránh NullReferenceException
                var validBatchUsages = batchUsages
                    .Where(pbu => pbu.PartInventoryBatch != null && pbu.PartInventoryBatch.Part != null)
                    .ToList();

                if (!validBatchUsages.Any())
                {
                    _logger.LogWarning($"Tất cả PartBatchUsage của ServiceOrder {serviceOrderId} đều thiếu thông tin PartInventoryBatch hoặc Part");
                    return new COGSCalculationResult
                    {
                        ServiceOrderId = serviceOrderId,
                        CalculationMethod = "FIFO",
                        TotalCOGS = 0,
                        CalculationDate = DateTime.Now,
                        ItemDetails = new List<COGSItemDetail>()
                    };
                }

                var itemDetails = new List<COGSItemDetail>();
                decimal totalCOGS = 0;

                // Nhóm theo PartId để tính tổng cho mỗi loại vật tư
                var groupedByPart = validBatchUsages.GroupBy(pbu => pbu.PartInventoryBatch!.PartId);

                foreach (var partGroup in groupedByPart)
                {
                    var partId = partGroup.Key;
                    // ✅ FIX: Filter null một lần nữa trong group và OrderBy an toàn
                    var partBatchUsages = partGroup
                        .Where(pbu => pbu.PartInventoryBatch != null && pbu.PartInventoryBatch.Part != null)
                        .OrderBy(pbu => pbu.PartInventoryBatch!.ReceiveDate)
                        .ToList();
                    
                    if (!partBatchUsages.Any())
                    {
                        _logger.LogWarning($"Không có PartBatchUsage hợp lệ cho PartId {partId}");
                        continue;
                    }
                    
                    // ✅ FIX: Null check để tránh NullReferenceException
                    var firstUsage = partBatchUsages.FirstOrDefault();
                    if (firstUsage == null || firstUsage.PartInventoryBatch == null || firstUsage.PartInventoryBatch.Part == null)
                    {
                        _logger.LogWarning($"PartBatchUsage thiếu thông tin Part hoặc PartInventoryBatch cho PartId {partId}");
                        continue;
                    }

                    var part = firstUsage.PartInventoryBatch.Part;
                    var firstBatch = firstUsage.PartInventoryBatch; // Batch đầu tiên (cũ nhất) theo FIFO

                    // Tính tổng số lượng và tổng giá vốn theo FIFO (lô cũ nhất trước)
                    int totalQuantity = 0;
                    decimal totalCost = 0;

                    foreach (var usage in partBatchUsages)
                    {
                        // ✅ FIX: Null check trong loop để đảm bảo an toàn tuyệt đối
                        if (usage == null)
                        {
                            _logger.LogWarning($"Phát hiện null PartBatchUsage trong group PartId {partId}, bỏ qua");
                            continue;
                        }
                        
                        totalQuantity += usage.QuantityUsed;
                        totalCost += usage.TotalCost; // Đã lưu sẵn TotalCost = QuantityUsed * UnitCost
                    }
                    
                    // ✅ FIX: Check totalQuantity > 0 sau khi loop để tránh division by zero
                    if (totalQuantity == 0)
                    {
                        _logger.LogWarning($"Tổng số lượng = 0 cho PartId {partId}, bỏ qua");
                        continue;
                    }

                    // Giá vốn đơn vị = Tổng giá vốn / Tổng số lượng (trung bình của các lô đã dùng)
                    // ✅ FIX: Không cần check totalQuantity > 0 nữa vì đã check ở trên và continue rồi
                    decimal unitCost = totalCost / totalQuantity;

                    itemDetails.Add(new COGSItemDetail
                    {
                        PartId = partId,
                        PartName = part.PartName ?? string.Empty,
                        PartNumber = part.PartNumber ?? string.Empty,
                        QuantityUsed = totalQuantity,
                        UnitCost = unitCost,
                        TotalCost = totalCost,
                        BatchNumber = firstBatch.BatchNumber ?? string.Empty, // Batch đầu tiên (cũ nhất) cho FIFO
                        BatchReceiveDate = firstBatch.ReceiveDate,
                        CalculationMethod = "FIFO"
                    });

                    totalCOGS += totalCost;
                }

                // Tạo breakdown JSON
                var breakdownJson = JsonSerializer.Serialize(new
                {
                    Method = "FIFO",
                    CalculationDate = DateTime.Now,
                    Items = itemDetails.Select(item => new
                    {
                        item.PartId,
                        item.PartName,
                        item.PartNumber,
                        item.QuantityUsed,
                        item.UnitCost,
                        item.TotalCost,
                        item.BatchNumber,
                        item.BatchReceiveDate
                    })
                });

                var result = new COGSCalculationResult
                {
                    ServiceOrderId = serviceOrderId,
                    CalculationMethod = "FIFO",
                    TotalCOGS = totalCOGS,
                    CalculationDate = DateTime.Now,
                    ItemDetails = itemDetails,
                    BreakdownJson = breakdownJson
                };

                _logger.LogInformation($"Tính COGS theo FIFO cho ServiceOrder {serviceOrderId}: {totalCOGS:N0} VNĐ");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi tính COGS theo FIFO cho ServiceOrder {serviceOrderId}");
                throw;
            }
        }

        /// <summary>
        /// Tính COGS theo phương pháp Bình quân gia quyền
        /// </summary>
        public async Task<COGSCalculationResult> CalculateCOGSByWeightedAverageAsync(int serviceOrderId)
        {
            try
            {
                // Lấy tất cả PartBatchUsage của ServiceOrder này
                // ✅ FIX: Filter null PartInventoryBatch trong query để tránh lỗi
                var batchUsages = await _context.PartBatchUsages
                    .Include(pbu => pbu.PartInventoryBatch)
                        .ThenInclude(batch => batch.Part)
                    .Where(pbu => pbu.ServiceOrderId == serviceOrderId 
                        && pbu.PartInventoryBatch != null 
                        && pbu.PartInventoryBatch.Part != null)
                    .ToListAsync();

                if (!batchUsages.Any())
                {
                    _logger.LogWarning($"Không có vật tư nào đã sử dụng cho ServiceOrder {serviceOrderId}");
                    return new COGSCalculationResult
                    {
                        ServiceOrderId = serviceOrderId,
                        CalculationMethod = "WeightedAverage",
                        TotalCOGS = 0,
                        CalculationDate = DateTime.Now,
                        ItemDetails = new List<COGSItemDetail>()
                    };
                }

                // ✅ FIX: Filter null PartInventoryBatch trước khi GroupBy để tránh NullReferenceException
                var validBatchUsages = batchUsages
                    .Where(pbu => pbu.PartInventoryBatch != null && pbu.PartInventoryBatch.Part != null)
                    .ToList();

                if (!validBatchUsages.Any())
                {
                    _logger.LogWarning($"Tất cả PartBatchUsage của ServiceOrder {serviceOrderId} đều thiếu thông tin PartInventoryBatch hoặc Part");
                    return new COGSCalculationResult
                    {
                        ServiceOrderId = serviceOrderId,
                        CalculationMethod = "WeightedAverage",
                        TotalCOGS = 0,
                        CalculationDate = DateTime.Now,
                        ItemDetails = new List<COGSItemDetail>()
                    };
                }

                var itemDetails = new List<COGSItemDetail>();
                decimal totalCOGS = 0;

                // Nhóm theo PartId để tính bình quân gia quyền cho mỗi loại vật tư
                var groupedByPart = validBatchUsages.GroupBy(pbu => pbu.PartInventoryBatch!.PartId);

                foreach (var partGroup in groupedByPart)
                {
                    var partId = partGroup.Key;
                    // ✅ FIX: Filter null một lần nữa trong group để đảm bảo an toàn
                    var partBatchUsages = partGroup
                        .Where(pbu => pbu.PartInventoryBatch != null && pbu.PartInventoryBatch.Part != null)
                        .ToList();
                    
                    if (!partBatchUsages.Any())
                    {
                        _logger.LogWarning($"Không có PartBatchUsage hợp lệ cho PartId {partId}");
                        continue;
                    }
                    
                    // ✅ FIX: Null check để tránh NullReferenceException
                    var firstUsage = partBatchUsages.FirstOrDefault();
                    if (firstUsage == null || firstUsage.PartInventoryBatch == null || firstUsage.PartInventoryBatch.Part == null)
                    {
                        _logger.LogWarning($"PartBatchUsage thiếu thông tin Part hoặc PartInventoryBatch cho PartId {partId}");
                        continue;
                    }

                    var part = firstUsage.PartInventoryBatch.Part;

                    // Tính bình quân gia quyền: 
                    // Giá vốn bình quân = Tổng (Số lượng × Giá vốn đơn vị) / Tổng số lượng
                    int totalQuantity = 0;
                    decimal totalCost = 0;

                    foreach (var usage in partBatchUsages)
                    {
                        // ✅ FIX: Null check trong loop để đảm bảo an toàn tuyệt đối
                        if (usage == null)
                        {
                            _logger.LogWarning($"Phát hiện null PartBatchUsage trong group PartId {partId}, bỏ qua");
                            continue;
                        }
                        
                        totalQuantity += usage.QuantityUsed;
                        totalCost += usage.TotalCost; // Đã lưu sẵn TotalCost = QuantityUsed * UnitCost
                    }
                    
                    // ✅ FIX: Check totalQuantity > 0 sau khi loop để tránh division by zero
                    if (totalQuantity == 0)
                    {
                        _logger.LogWarning($"Tổng số lượng = 0 cho PartId {partId}, bỏ qua");
                        continue;
                    }

                    // Giá vốn đơn vị bình quân = Tổng giá vốn / Tổng số lượng
                    // ✅ FIX: Không cần check totalQuantity > 0 nữa vì đã check ở trên và continue rồi
                    decimal weightedAverageUnitCost = totalCost / totalQuantity;

                    itemDetails.Add(new COGSItemDetail
                    {
                        PartId = partId,
                        PartName = part.PartName ?? string.Empty,
                        PartNumber = part.PartNumber ?? string.Empty,
                        QuantityUsed = totalQuantity,
                        UnitCost = weightedAverageUnitCost,
                        TotalCost = totalCost,
                        BatchNumber = null, // Bình quân không có batch cụ thể
                        BatchReceiveDate = null,
                        CalculationMethod = "WeightedAverage"
                    });

                    totalCOGS += totalCost;
                }

                // Tạo breakdown JSON
                var breakdownJson = JsonSerializer.Serialize(new
                {
                    Method = "WeightedAverage",
                    CalculationDate = DateTime.Now,
                    Items = itemDetails.Select(item => new
                    {
                        item.PartId,
                        item.PartName,
                        item.PartNumber,
                        item.QuantityUsed,
                        item.UnitCost,
                        item.TotalCost
                    })
                });

                var result = new COGSCalculationResult
                {
                    ServiceOrderId = serviceOrderId,
                    CalculationMethod = "WeightedAverage",
                    TotalCOGS = totalCOGS,
                    CalculationDate = DateTime.Now,
                    ItemDetails = itemDetails,
                    BreakdownJson = breakdownJson
                };

                _logger.LogInformation($"Tính COGS theo WeightedAverage cho ServiceOrder {serviceOrderId}: {totalCOGS:N0} VNĐ");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi tính COGS theo WeightedAverage cho ServiceOrder {serviceOrderId}");
                throw;
            }
        }

        /// <summary>
        /// Tính COGS theo phương pháp được chỉ định
        /// </summary>
        public async Task<COGSCalculationResult> CalculateCOGSAsync(int serviceOrderId, string method)
        {
            // ✅ FIX: Validate method parameter
            if (string.IsNullOrWhiteSpace(method))
            {
                _logger.LogWarning($"COGSCalculationMethod rỗng cho ServiceOrder {serviceOrderId}, sử dụng mặc định FIFO");
                method = "FIFO";
            }

            return method.ToUpper() switch
            {
                "FIFO" => await CalculateCOGSByFIFOAsync(serviceOrderId),
                "WEIGHTEDAVERAGE" or "WEIGHTED_AVERAGE" or "AVERAGE" => await CalculateCOGSByWeightedAverageAsync(serviceOrderId),
                _ => throw new ArgumentException($"Phương pháp tính COGS không hợp lệ: {method}. Chỉ hỗ trợ 'FIFO' hoặc 'WeightedAverage'")
            };
        }

        /// <summary>
        /// Lấy chi tiết breakdown COGS từ ServiceOrder
        /// </summary>
        public async Task<COGSBreakdownResult> GetCOGSBreakdownAsync(int serviceOrderId)
        {
            try
            {
                var serviceOrder = await _context.ServiceOrders
                    .FirstOrDefaultAsync(so => so.Id == serviceOrderId);

                if (serviceOrder == null)
                {
                    throw new ArgumentException($"ServiceOrder với ID {serviceOrderId} không tồn tại");
                }

                if (string.IsNullOrEmpty(serviceOrder.COGSBreakdown))
                {
                    // Nếu chưa có breakdown, tính toán lại
                    // ✅ FIX: Validate COGSCalculationMethod trước khi sử dụng
                    var method = string.IsNullOrWhiteSpace(serviceOrder.COGSCalculationMethod) ? "FIFO" : serviceOrder.COGSCalculationMethod;
                    var calculationResult = await CalculateCOGSAsync(serviceOrderId, method);
                    return new COGSBreakdownResult
                    {
                        ServiceOrderId = serviceOrderId,
                        CalculationMethod = calculationResult.CalculationMethod,
                        TotalCOGS = calculationResult.TotalCOGS,
                        CalculationDate = calculationResult.CalculationDate,
                        ItemDetails = calculationResult.ItemDetails
                    };
                }

                // Parse breakdown từ JSON
                JsonElement breakdownData;
                try
                {
                    breakdownData = JsonSerializer.Deserialize<JsonElement>(serviceOrder.COGSBreakdown);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, $"Lỗi parse JSON breakdown cho ServiceOrder {serviceOrderId}, sẽ tính toán lại");
                    // Nếu parse lỗi, tính toán lại
                    // ✅ FIX: Validate COGSCalculationMethod trước khi sử dụng
                    var method = string.IsNullOrWhiteSpace(serviceOrder.COGSCalculationMethod) ? "FIFO" : serviceOrder.COGSCalculationMethod;
                    var calculationResult = await CalculateCOGSAsync(serviceOrderId, method);
                    return new COGSBreakdownResult
                    {
                        ServiceOrderId = serviceOrderId,
                        CalculationMethod = calculationResult.CalculationMethod,
                        TotalCOGS = calculationResult.TotalCOGS,
                        CalculationDate = calculationResult.CalculationDate,
                        ItemDetails = calculationResult.ItemDetails
                    };
                }

                var itemDetails = new List<COGSItemDetail>();

                if (breakdownData.TryGetProperty("Items", out var items))
                {
                    foreach (var item in items.EnumerateArray())
                    {
                        try
                        {
                            itemDetails.Add(new COGSItemDetail
                            {
                                PartId = item.GetProperty("PartId").GetInt32(),
                                PartName = item.GetProperty("PartName").GetString() ?? "",
                                PartNumber = item.GetProperty("PartNumber").GetString() ?? "",
                                QuantityUsed = item.GetProperty("QuantityUsed").GetInt32(),
                                UnitCost = item.GetProperty("UnitCost").GetDecimal(),
                                TotalCost = item.GetProperty("TotalCost").GetDecimal(),
                                BatchNumber = item.TryGetProperty("BatchNumber", out var batch) ? batch.GetString() : null,
                                BatchReceiveDate = item.TryGetProperty("BatchReceiveDate", out var date) && date.TryGetDateTime(out var dt) ? dt : null,
                                CalculationMethod = serviceOrder.COGSCalculationMethod
                            });
                        }
                        catch (KeyNotFoundException ex)
                        {
                            _logger.LogWarning(ex, $"Thiếu property trong breakdown JSON cho ServiceOrder {serviceOrderId}, bỏ qua item này");
                            continue; // Bỏ qua item lỗi và tiếp tục
                        }
                    }
                }

                return new COGSBreakdownResult
                {
                    ServiceOrderId = serviceOrderId,
                    CalculationMethod = string.IsNullOrWhiteSpace(serviceOrder.COGSCalculationMethod) ? "FIFO" : serviceOrder.COGSCalculationMethod,
                    TotalCOGS = serviceOrder.TotalCOGS,
                    CalculationDate = serviceOrder.COGSCalculationDate,
                    ItemDetails = itemDetails
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi lấy breakdown COGS cho ServiceOrder {serviceOrderId}");
                throw;
            }
        }

        /// <summary>
        /// Tính lợi nhuận gộp (Gross Profit = Revenue - COGS)
        /// </summary>
        public async Task<GrossProfitResult> CalculateGrossProfitAsync(int serviceOrderId)
        {
            try
            {
                var serviceOrder = await _context.ServiceOrders
                    .FirstOrDefaultAsync(so => so.Id == serviceOrderId);

                if (serviceOrder == null)
                {
                    throw new ArgumentException($"ServiceOrder với ID {serviceOrderId} không tồn tại");
                }

                // Tính tổng doanh thu = TotalAmount của ServiceOrder
                decimal totalRevenue = serviceOrder.TotalAmount;

                // Lấy COGS (nếu chưa tính thì tính lại)
                decimal totalCOGS = serviceOrder.TotalCOGS;
                // ✅ FIX: Chỉ tính lại nếu chưa tính (COGSCalculationDate == null), không phải khi totalCOGS == 0
                // Vì totalCOGS có thể = 0 nếu không có vật tư nào được sử dụng
                if (serviceOrder.COGSCalculationDate == null)
                {
                    var method = string.IsNullOrWhiteSpace(serviceOrder.COGSCalculationMethod) ? "FIFO" : serviceOrder.COGSCalculationMethod;
                    var cogsResult = await CalculateCOGSAsync(serviceOrderId, method);
                    totalCOGS = cogsResult.TotalCOGS;
                }

                // Tính lợi nhuận gộp
                decimal grossProfit = totalRevenue - totalCOGS;

                // Tính tỷ lệ lợi nhuận gộp (%)
                decimal grossProfitMargin = totalRevenue > 0 ? (grossProfit / totalRevenue) * 100 : 0;

                return new GrossProfitResult
                {
                    ServiceOrderId = serviceOrderId,
                    TotalRevenue = totalRevenue,
                    TotalCOGS = totalCOGS,
                    GrossProfit = grossProfit,
                    GrossProfitMargin = grossProfitMargin
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi tính lợi nhuận gộp cho ServiceOrder {serviceOrderId}");
                throw;
            }
        }
    }
}

