using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Shared.Models;
using GarageManagementSystem.Shared.DTOs;
using System.Text.Json;

namespace GarageManagementSystem.Web.Controllers
{
    [Authorize]
    [Route("InventoryAlerts")]
    public class InventoryAlertsController : Controller
    {
        private readonly ApiService _apiService;

        public InventoryAlertsController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: InventoryAlerts
        [HttpGet]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        // GET: InventoryAlerts/GetAlerts
        [HttpGet("GetAlerts")]
        public async Task<IActionResult> GetAlerts(string alertType = "")
        {
            try
            {
                List<object> alerts = new List<object>();

                // Get alerts based on type
                if (string.IsNullOrEmpty(alertType) || alertType == "LowStock")
                {
                    var lowStockResponse = await _apiService.GetAsync<dynamic>("api/inventory-alerts/low-stock");
                    if (lowStockResponse.Success && lowStockResponse.Data != null)
                    {
                        var jsonString = JsonSerializer.Serialize(lowStockResponse.Data);
                        using (var jsonDoc = JsonDocument.Parse(jsonString))
                        {
                            var root = jsonDoc.RootElement;
                            if (root.TryGetProperty("data", out JsonElement dataProp) && dataProp.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var item in dataProp.EnumerateArray())
                                {
                                    alerts.Add(new
                                    {
                                        id = item.TryGetProperty("Id", out var idProp) ? idProp.GetInt32() : 0,
                                        partName = item.TryGetProperty("PartName", out var nameProp) ? nameProp.GetString() : "",
                                        partNumber = item.TryGetProperty("PartNumber", out var numProp) ? numProp.GetString() : "",
                                        alertType = "LowStock",
                                        severity = item.TryGetProperty("AlertLevel", out var levelProp) ? levelProp.GetString() : "Medium",
                                        message = $"Tồn kho thấp: {(item.TryGetProperty("CurrentStock", out var stockProp) ? stockProp.GetInt32() : 0)} (Tối thiểu: {(item.TryGetProperty("MinStock", out var minProp) ? minProp.GetInt32() : 0)})",
                                        currentQuantity = item.TryGetProperty("CurrentStock", out var qtyProp) ? qtyProp.GetInt32() : 0,
                                        minimumQuantity = item.TryGetProperty("MinStock", out var minQtyProp) ? minQtyProp.GetInt32() : 0,
                                        deficit = item.TryGetProperty("Deficit", out var defProp) ? defProp.GetInt32() : 0,
                                        location = item.TryGetProperty("Location", out var locProp) ? locProp.GetString() : "",
                                        createdAt = DateTime.Now
                                    });
                                }
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(alertType) || alertType == "OutOfStock")
                {
                    var outOfStockResponse = await _apiService.GetAsync<dynamic>("api/inventory-alerts/out-of-stock");
                    if (outOfStockResponse.Success && outOfStockResponse.Data != null)
                    {
                        var jsonString = JsonSerializer.Serialize(outOfStockResponse.Data);
                        using (var jsonDoc = JsonDocument.Parse(jsonString))
                        {
                            var root = jsonDoc.RootElement;
                            if (root.TryGetProperty("data", out JsonElement dataProp) && dataProp.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var item in dataProp.EnumerateArray())
                                {
                                    alerts.Add(new
                                    {
                                        id = item.TryGetProperty("Id", out var idProp) ? idProp.GetInt32() : 0,
                                        partName = item.TryGetProperty("PartName", out var nameProp) ? nameProp.GetString() : "",
                                        partNumber = item.TryGetProperty("PartNumber", out var numProp) ? numProp.GetString() : "",
                                        alertType = "OutOfStock",
                                        severity = "Critical",
                                        message = "Hết hàng",
                                        currentQuantity = 0,
                                        minimumQuantity = 0,
                                        deficit = 0,
                                        location = item.TryGetProperty("Location", out var locProp) ? locProp.GetString() : "",
                                        createdAt = DateTime.Now
                                    });
                                }
                            }
                        }
                    }
                }

                // ✅ THÊM: Get Overstock alerts
                if (string.IsNullOrEmpty(alertType) || alertType == "Overstock")
                {
                    var overstockResponse = await _apiService.GetAsync<dynamic>("api/inventory-alerts/overstock");
                    if (overstockResponse.Success && overstockResponse.Data != null)
                    {
                        var jsonString = JsonSerializer.Serialize(overstockResponse.Data);
                        using (var jsonDoc = JsonDocument.Parse(jsonString))
                        {
                            var root = jsonDoc.RootElement;
                            if (root.TryGetProperty("data", out JsonElement dataProp) && dataProp.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var item in dataProp.EnumerateArray())
                                {
                                    var currentStock = item.TryGetProperty("CurrentStock", out var stockProp) ? stockProp.GetInt32() : 0;
                                    var maxStock = item.TryGetProperty("MaxStock", out var maxProp) ? maxProp.GetDecimal() : 0;
                                    var excess = item.TryGetProperty("Excess", out var excessProp) ? excessProp.GetDecimal() : 0;
                                    
                                    alerts.Add(new
                                    {
                                        id = item.TryGetProperty("Id", out var idProp) ? idProp.GetInt32() : 0,
                                        partName = item.TryGetProperty("PartName", out var nameProp) ? nameProp.GetString() : "",
                                        partNumber = item.TryGetProperty("PartNumber", out var numProp) ? numProp.GetString() : "",
                                        alertType = "Overstock",
                                        severity = "Low", // Overstock là mức độ thấp
                                        message = $"Tồn kho cao: {currentStock} (Tối đa đề xuất: {maxStock}, Dư thừa: {excess})",
                                        currentQuantity = currentStock,
                                        minimumQuantity = 0,
                                        deficit = 0,
                                        excess = excess,
                                        maxStock = maxStock,
                                        location = item.TryGetProperty("Location", out var locProp) ? locProp.GetString() : "",
                                        createdAt = DateTime.Now
                                    });
                                }
                            }
                        }
                    }
                }

                // ✅ THÊM: Get Expiring Soon alerts
                if (string.IsNullOrEmpty(alertType) || alertType == "ExpiringSoon")
                {
                    var expiringSoonResponse = await _apiService.GetAsync<dynamic>("api/inventory-alerts/expiring-soon");
                    if (expiringSoonResponse.Success && expiringSoonResponse.Data != null)
                    {
                        var jsonString = JsonSerializer.Serialize(expiringSoonResponse.Data);
                        using (var jsonDoc = JsonDocument.Parse(jsonString))
                        {
                            var root = jsonDoc.RootElement;
                            if (root.TryGetProperty("data", out JsonElement dataProp) && dataProp.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var item in dataProp.EnumerateArray())
                                {
                                    var daysUntilExpiry = item.TryGetProperty("DaysUntilExpiry", out var daysProp) ? daysProp.GetInt32() : 0;
                                    var expiryDate = item.TryGetProperty("ExpiryDate", out var expiryProp) ? expiryProp.GetDateTime() : (DateTime?)null;
                                    var alertLevel = item.TryGetProperty("AlertLevel", out var levelProp) ? levelProp.GetString() : "Medium";
                                    var partId = item.TryGetProperty("PartId", out var partIdProp) ? partIdProp.GetInt32() : 0;
                                    var partName = item.TryGetProperty("PartName", out var partNameProp) ? partNameProp.GetString() : "";
                                    var partNumber = item.TryGetProperty("PartNumber", out var partNumProp) ? partNumProp.GetString() : "";
                                    var batchNumber = item.TryGetProperty("BatchNumber", out var batchNumProp) ? batchNumProp.GetString() : "";
                                    var location = item.TryGetProperty("Location", out var locProp) ? locProp.GetString() : "";
                                    
                                    alerts.Add(new
                                    {
                                        id = partId,
                                        partName = partName,
                                        partNumber = partNumber,
                                        alertType = "ExpiringSoon",
                                        severity = alertLevel,
                                        message = $"Sắp hết hạn: Còn {daysUntilExpiry} ngày (Hết hạn: {expiryDate?.ToString("dd/MM/yyyy") ?? "N/A"}) - Lô: {batchNumber}",
                                        currentQuantity = item.TryGetProperty("QuantityRemaining", out var qtyProp) ? qtyProp.GetInt32() : 0,
                                        minimumQuantity = 0,
                                        deficit = 0,
                                        daysUntilExpiry = daysUntilExpiry,
                                        expiryDate = expiryDate,
                                        batchNumber = batchNumber,
                                        location = location,
                                        createdAt = DateTime.Now
                                    });
                                }
                            }
                        }
                    }
                }

                return Json(new
                {
                    draw = Request.Query["draw"].FirstOrDefault(),
                    recordsTotal = alerts.Count,
                    recordsFiltered = alerts.Count,
                    data = alerts
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting inventory alerts: {ex.Message}");
                return Json(new
                {
                    draw = Request.Query["draw"].FirstOrDefault(),
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>()
                });
            }
        }

        // GET: InventoryAlerts/GetAlertTypes
        [HttpGet("GetAlertTypes")]
        public async Task<IActionResult> GetAlertTypes()
        {
            try
            {
                var response = await _apiService.GetAsync<ApiResponse<List<string>>>("/api/InventoryAlerts/alert-types");
                
                if (response.Success && response.Data != null)
                {
                    return Json(response.Data.Data);
                }
                
                return Json(new List<string>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting alert types: {ex.Message}");
                return Json(new List<string>());
            }
        }

        // POST: InventoryAlerts/MarkAsResolved
        [HttpPost("MarkAsResolved")]
        public async Task<IActionResult> MarkAsResolved(int alertId)
        {
            try
            {
                var response = await _apiService.PostAsync<ApiResponse<bool>>($"/api/InventoryAlerts/{alertId}/resolve", null);
                
                if (response.Success)
                {
                    return Json(new { success = true, message = "Đã đánh dấu cảnh báo là đã xử lý" });
                }
                
                return Json(new { success = false, message = response.Message ?? "Không thể đánh dấu cảnh báo" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking alert as resolved: {ex.Message}");
                return Json(new { success = false, message = "Lỗi khi xử lý cảnh báo" });
            }
        }

        // POST: InventoryAlerts/MarkAllAsResolved
        [HttpPost("MarkAllAsResolved")]
        public async Task<IActionResult> MarkAllAsResolved(string alertType = "")
        {
            try
            {
                var endpoint = "/api/InventoryAlerts/resolve-all";
                if (!string.IsNullOrEmpty(alertType))
                {
                    endpoint += $"?alertType={Uri.EscapeDataString(alertType)}";
                }

                var response = await _apiService.PostAsync<ApiResponse<int>>(endpoint, null);
                
                if (response.Success)
                {
                    return Json(new { success = true, message = $"Đã đánh dấu {response.Data} cảnh báo là đã xử lý" });
                }
                
                return Json(new { success = false, message = response.Message ?? "Không thể đánh dấu cảnh báo" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking all alerts as resolved: {ex.Message}");
                return Json(new { success = false, message = "Lỗi khi xử lý cảnh báo" });
            }
        }

        // ✅ THÊM: Get total alerts count for badge
        [HttpGet("GetAlertsCount")]
        public async Task<IActionResult> GetAlertsCount()
        {
            try
            {
                // Get low stock alerts - API trả về { success: true, data: [...], count: ... }
                // ApiService.GetAsync<dynamic> sẽ unwrap ApiResponse, nên response.Data sẽ là object { success: true, data: [...], count: ... }
                var lowStockResponse = await _apiService.GetAsync<dynamic>("api/inventory-alerts/low-stock");
                var lowStockCount = 0;
                if (lowStockResponse.Success && lowStockResponse.Data != null)
                {
                    // response.Data có thể là object { success: true, data: [...], count: ... } hoặc là object khác
                    // Sử dụng JsonDocument để parse
                    try
                    {
                        var jsonString = JsonSerializer.Serialize(lowStockResponse.Data);
                        using (var jsonDoc = JsonDocument.Parse(jsonString))
                        {
                            var root = jsonDoc.RootElement;
                            
                            // Ưu tiên lấy từ property "count"
                            if (root.TryGetProperty("count", out JsonElement countProp) && countProp.ValueKind == JsonValueKind.Number)
                            {
                                lowStockCount = countProp.GetInt32();
                            }
                            // Nếu không có count, đếm từ property "data" (array)
                            else if (root.TryGetProperty("data", out JsonElement dataProp) && dataProp.ValueKind == JsonValueKind.Array)
                            {
                                lowStockCount = dataProp.GetArrayLength();
                            }
                            // Nếu root chính nó là array
                            else if (root.ValueKind == JsonValueKind.Array)
                            {
                                lowStockCount = root.GetArrayLength();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing low stock response: {ex.Message}");
                        lowStockCount = 0;
                    }
                }

                // Get out of stock alerts
                var outOfStockResponse = await _apiService.GetAsync<dynamic>("api/inventory-alerts/out-of-stock");
                var outOfStockCount = 0;
                if (outOfStockResponse.Success && outOfStockResponse.Data != null)
                {
                    try
                    {
                        var jsonString = JsonSerializer.Serialize(outOfStockResponse.Data);
                        using (var jsonDoc = JsonDocument.Parse(jsonString))
                        {
                            var root = jsonDoc.RootElement;
                            
                            if (root.TryGetProperty("count", out JsonElement countProp) && countProp.ValueKind == JsonValueKind.Number)
                            {
                                outOfStockCount = countProp.GetInt32();
                            }
                            else if (root.TryGetProperty("data", out JsonElement dataProp) && dataProp.ValueKind == JsonValueKind.Array)
                            {
                                outOfStockCount = dataProp.GetArrayLength();
                            }
                            else if (root.ValueKind == JsonValueKind.Array)
                            {
                                outOfStockCount = root.GetArrayLength();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing out of stock response: {ex.Message}");
                        outOfStockCount = 0;
                    }
                }

                var totalCount = lowStockCount + outOfStockCount;

                return Json(new { success = true, count = totalCount, lowStock = lowStockCount, outOfStock = outOfStockCount });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting alerts count: {ex.Message}");
                return Json(new { success = false, count = 0, lowStock = 0, outOfStock = 0 });
            }
        }

        // GET: InventoryAlerts/ExportExcel
        [HttpGet("ExportExcel")]
        public async Task<IActionResult> ExportExcel(string alertType = "")
        {
            try
            {
                // Call API to get Excel file
                var endpoint = "api/inventory-alerts/export-excel";
                if (!string.IsNullOrEmpty(alertType))
                {
                    endpoint += $"?alertType={Uri.EscapeDataString(alertType)}";
                }

                var response = await _apiService.GetByteArrayAsync(endpoint);
                if (!response.Success || response.Data == null)
                {
                    return Json(new { success = false, error = response.ErrorMessage ?? "Không thể xuất Excel." });
                }

                var fileName = string.IsNullOrWhiteSpace(response.Message)
                    ? $"CanhBaoTonKho-{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                    : response.Message.Trim('"');

                return File(response.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Lỗi: {ex.Message}" });
            }
        }
    }
}
