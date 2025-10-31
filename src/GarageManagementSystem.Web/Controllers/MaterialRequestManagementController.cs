using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using GarageManagementSystem.Web.Configuration;
using GarageManagementSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceQuotationDto = GarageManagementSystem.Shared.DTOs.ServiceQuotationDto;

namespace GarageManagementSystem.Web.Controllers
{
    [Authorize]
    [Route("MaterialRequestManagement")]
    public class MaterialRequestManagementController : Controller
    {
        private readonly ApiService _apiService;

        public MaterialRequestManagementController(ApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("GetMRsPaged")]
        public async Task<IActionResult> GetMRsPaged(int pageNumber = 1, int pageSize = 10, int? serviceOrderId = null, string? status = null)
        {
            try
            {
                var query = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };
                if (serviceOrderId.HasValue) query.Add($"serviceOrderId={serviceOrderId.Value}");
                if (!string.IsNullOrEmpty(status)) query.Add($"status={Uri.EscapeDataString(status)}");

                var endpoint = ApiEndpoints.MaterialRequests.GetAll + "?" + string.Join("&", query);
                var response = await _apiService.GetAsync<PagedResponse<MaterialRequestDto>>(endpoint);

                if (response.Success && response.Data != null)
                {
                    var list = response.Data.Data.Select(m => new
                    {
                        id = m.Id,
                        mrNumber = m.MRNumber,
                        serviceOrderId = m.ServiceOrderId,
                        status = m.Status.ToString(),
                        createdAt = m.Id > 0 ? "" : "" // placeholder
                    }).Cast<object>().ToList();

                    return Json(new
                    {
                        success = true,
                        data = list,
                        totalCount = response.Data.TotalCount,
                        pageNumber = response.Data.PageNumber,
                        pageSize = response.Data.PageSize
                    });
                }

                return Json(new { success = false, data = new List<object>(), totalCount = 0, pageNumber, pageSize, message = response.ErrorMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, data = new List<object>(), totalCount = 0, pageNumber, pageSize, message = ex.Message });
            }
        }

        [HttpPost("CreateMR")]
        public async Task<IActionResult> CreateMR([FromBody] CreateMaterialRequestDto dto)
        {
            var response = await _apiService.PostAsync<MaterialRequestDto>(ApiEndpoints.MaterialRequests.Create, dto);
            return Json(response);
        }

        [HttpPut("Submit/{id}")]
        public async Task<IActionResult> Submit(int id)
        {
            var response = await _apiService.PutAsync<object>(ApiEndpoints.Builder.WithId(ApiEndpoints.MaterialRequests.Submit, id), new { });
            return Json(response);
        }

        [HttpPut("Approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            var response = await _apiService.PutAsync<object>(ApiEndpoints.Builder.WithId(ApiEndpoints.MaterialRequests.Approve, id), new { });
            return Json(response);
        }

        [HttpPut("Reject/{id}")]
        public async Task<IActionResult> Reject(int id, [FromBody] ChangeMaterialRequestStatusDto dto)
        {
            var response = await _apiService.PutAsync<object>(ApiEndpoints.Builder.WithId(ApiEndpoints.MaterialRequests.Reject, id), dto);
            return Json(response);
        }

        /// <summary>
        /// Lấy danh sách Service Orders (JO) có sẵn để tạo Material Request
        /// Chỉ lấy các JO ở trạng thái có thể tạo MR (PendingAssignment, WaitingForParts, ReadyToWork, InProgress)
        /// </summary>
        [HttpGet("GetAvailableServiceOrders")]
        public async Task<IActionResult> GetAvailableServiceOrders()
        {
            try
            {
                // Lấy tất cả Service Orders, filter ở client để hiển thị đầy đủ
                var response = await _apiService.GetAsync<PagedResponse<ServiceOrderDto>>(
                    ApiEndpoints.ServiceOrders.GetAll + "?pageNumber=1&pageSize=1000"
                );

                if (response.Success && response.Data != null && response.Data.Data != null)
                {
                    // Filter các JO ở trạng thái có thể tạo MR
                    var availableOrders = response.Data.Data
                        .Where(so => so.Status == "PendingAssignment" || 
                                     so.Status == "WaitingForParts" || 
                                     so.Status == "ReadyToWork" || 
                                     so.Status == "InProgress" ||
                                     so.Status == "Pending") // ✅ Cho phép tạo MR ngay từ khi tạo JO
                        .OrderByDescending(so => so.OrderDate)
                        .Select(so => new
                        {
                            id = so.Id,
                            orderNumber = so.OrderNumber,
                            text = $"{so.OrderNumber} - {so.Customer?.Name ?? "N/A"} ({so.Vehicle?.LicensePlate ?? "N/A"})",
                            customerName = so.Customer?.Name ?? "Không xác định",
                            vehiclePlate = so.Vehicle?.LicensePlate ?? "Không xác định",
                            status = so.Status
                        })
                        .Cast<object>()
                        .ToList();

                    return Json(new { success = true, data = availableOrders });
                }

                return Json(new { success = false, data = new List<object>() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, data = new List<object>(), error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy chi tiết Service Order (JO) bao gồm các phụ tùng để gợi ý cho Material Request
        /// Lấy từ ServiceQuotation.Items (các items có PartId)
        /// </summary>
        [HttpGet("GetServiceOrderParts/{id}")]
        public async Task<IActionResult> GetServiceOrderParts(int id)
        {
            try
            {
                // Lấy chi tiết Service Order
                var orderResponse = await _apiService.GetAsync<ApiResponse<ServiceOrderDto>>(
                    ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceOrders.GetById, id)
                );

                if (!orderResponse.Success || orderResponse.Data?.Data == null)
                {
                    return Json(new { success = false, error = "Không tìm thấy phiếu sửa chữa" });
                }

                var order = orderResponse.Data.Data;
                var parts = new List<object>();

                // Nếu có ServiceQuotationId, lấy phụ tùng từ Quotation gốc
                if (order.ServiceQuotationId.HasValue)
                {
                    var quotationResponse = await _apiService.GetAsync<ApiResponse<ServiceQuotationDto>>(
                        ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceQuotations.GetById, order.ServiceQuotationId.Value)
                    );

                    if (quotationResponse.Success && quotationResponse.Data?.Data != null)
                    {
                        var quotation = quotationResponse.Data.Data;
                        
                        // ✅ DEBUG: Log để kiểm tra
                        Console.WriteLine($"[GetServiceOrderParts] QuotationId: {order.ServiceQuotationId.Value}");
                        Console.WriteLine($"[GetServiceOrderParts] Items count: {quotation.Items?.Count ?? 0}");
                        
                        // Lọc các items có PartId (phụ tùng) HOẶC ItemType/ItemCategory là Part/Material
                        if (quotation.Items != null && quotation.Items.Any())
                        {
                            // ✅ Log chi tiết từng item để debug
                            foreach (var item in quotation.Items)
                            {
                                Console.WriteLine($"[GetServiceOrderParts] Item: {item.ItemName}, PartId: {item.PartId}, ServiceId: {item.ServiceId}, ServiceType: {item.ServiceType}, ItemCategory: {item.ItemCategory}");
                            }
                            
                            // ✅ Lọc phụ tùng: Chỉ lấy items có PartId hoặc là phụ tùng
                            var partItems = quotation.Items
                                .Where(item => 
                                    (item.PartId.HasValue && item.PartId.Value > 0) || 
                                    (!string.IsNullOrEmpty(item.ServiceType) && item.ServiceType.ToLower() == "parts") ||
                                    (!string.IsNullOrEmpty(item.ItemCategory) && item.ItemCategory.ToLower() == "material" && item.ServiceId == null && !item.ItemName.ToLower().Contains("công"))
                                )
                                .ToList();
                            
                            Console.WriteLine($"[GetServiceOrderParts] PartItems count: {partItems.Count}");
                            
                            // ✅ Lọc các items KHÔNG phải phụ tùng (dịch vụ, tiền công) để thông báo
                            var nonPartItems = quotation.Items
                                .Where(item => !partItems.Contains(item))
                                .Select(item => new
                                {
                                    itemName = item.ItemName,
                                    itemCategory = item.ItemCategory ?? "",
                                    serviceType = item.ServiceType ?? "",
                                    serviceId = item.ServiceId,
                                    partId = item.PartId
                                })
                                .ToList();
                            
                            if (nonPartItems.Any())
                            {
                                Console.WriteLine($"[GetServiceOrderParts] Non-part items: {string.Join(", ", nonPartItems.Select(i => $"{i.itemName} ({i.itemCategory})"))}");
                            }
                            
                            parts = partItems
                                .Select(item => new
                                {
                                    partId = item.PartId ?? 0,
                                    partName = item.ItemName,
                                    quantity = item.Quantity,
                                    unitPrice = item.UnitPrice,
                                    description = item.Description ?? "",
                                    serviceType = item.ServiceType ?? "",
                                    itemCategory = item.ItemCategory ?? "",
                                    source = "Quotation"
                                })
                                .Cast<object>()
                                .ToList();
                            
                            Console.WriteLine($"[GetServiceOrderParts] Final parts count: {parts.Count}");
                        }
                        else
                        {
                            Console.WriteLine($"[GetServiceOrderParts] Items is null or empty");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[GetServiceOrderParts] Failed to load quotation: {quotationResponse.ErrorMessage}");
                    }
                }
                else
                {
                    Console.WriteLine($"[GetServiceOrderParts] Order has no ServiceQuotationId");
                }

                // ✅ THÊM: Thông báo nếu không có phụ tùng
                var hasParts = parts != null && parts.Any();
                var message = hasParts 
                    ? null 
                    : "Không có phụ tùng cần xuất kho cho đơn hàng này. Đơn hàng chỉ bao gồm dịch vụ và tiền công.\n\n" +
                      "👉 Bước tiếp theo: Quay lại \"Phiếu Sửa Chữa\" để:\n" +
                      "1. Chuyển trạng thái sang \"Chờ Phân Công\"\n" +
                      "2. Phân công KTV và giờ công dự kiến\n" +
                      "3. Bắt đầu sửa chữa";

                var result = new
                {
                    success = true,
                    orderNumber = order.OrderNumber,
                    customerName = order.Customer?.Name ?? "Không xác định",
                    vehiclePlate = order.Vehicle?.LicensePlate ?? "Không xác định",
                    status = order.Status,
                    description = order.Notes ?? "Không có mô tả",
                    parts = parts,
                    hasParts = hasParts, // ✅ THÊM: Flag để frontend biết có phụ tùng không
                    message = message // ✅ THÊM: Thông báo cho user
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}


