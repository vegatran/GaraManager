using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;
using AutoMapper;
using GarageManagementSystem.Web.Models;
using POTrackingDtos = GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller quản lý phiếu nhập hàng với đầy đủ các thao tác CRUD thông qua API
    /// </summary>
    [Authorize]
    [Route("PurchaseOrder")]
    public class PurchaseOrderController : Controller
    {
        private readonly ApiService _apiService;
        private readonly IMapper _mapper;

        public PurchaseOrderController(ApiService apiService, IMapper mapper)
        {
            _apiService = apiService;
            _mapper = mapper;
        }

        /// <summary>
        /// Hiển thị trang quản lý phiếu nhập hàng
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Tạo phiếu nhập hàng mới thông qua API
        /// </summary>
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderDto createDto)
        {
            try
            {
                Console.WriteLine($"[PurchaseOrderController.Create] Received request to create purchase order");
                Console.WriteLine($"[PurchaseOrderController.Create] SupplierId: {createDto.SupplierId}, Items count: {createDto.Items?.Count ?? 0}");

                var response = await _apiService.PostAsync<object>(ApiEndpoints.PurchaseOrders.Create, createDto);
                
                if (response.Success)
                {
                    Console.WriteLine($"[PurchaseOrderController.Create] Successfully created purchase order");
                    return Json(new { success = true, message = "Tạo phiếu nhập hàng thành công" });
                }
                else
                {
                    Console.WriteLine($"[PurchaseOrderController.Create] Failed to create: {response.ErrorMessage}");
                    return Json(new { success = false, error = response.ErrorMessage ?? "Lỗi khi tạo phiếu nhập hàng" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PurchaseOrderController.Create] Exception: {ex.Message}");
                return Json(new { success = false, error = "Có lỗi xảy ra khi tạo phiếu nhập hàng: " + ex.Message });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết phiếu nhập hàng theo ID thông qua API
        /// </summary>
        [HttpGet("GetPurchaseOrderById/{id}")]
        public async Task<IActionResult> GetPurchaseOrderById(int id)
        {
            try
            {
                var response = await _apiService.GetAsync<ApiResponse<PurchaseOrderDto>>(
                    ApiEndpoints.PurchaseOrders.GetById.Replace("{0}", id.ToString())
                );
                
                if (response.Success && response.Data != null)
                {
                    var purchaseOrderData = _mapper.Map<PurchaseOrderDetailsViewModel>(response.Data.Data);
                    
                    // Debug logging
                    Console.WriteLine($"PurchaseOrder ID: {purchaseOrderData.Id}");
                    Console.WriteLine($"OrderNumber: {purchaseOrderData.OrderNumber}");
                    Console.WriteLine($"Items Count: {purchaseOrderData.Items?.Count ?? 0}");
                    
                    return Json(new { success = true, data = purchaseOrderData });
                }
                else
                {
                    Console.WriteLine($"API Response failed: Success={response.Success}, Data={response.Data}");
                    return Json(new { success = false, error = "Phiếu nhập hàng không tồn tại" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetPurchaseOrderById: {ex.Message}");
                return Json(new { success = false, error = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// Cập nhật phiếu nhập hàng thông qua API
        /// </summary>
        [HttpPut("Update")]
        public async Task<IActionResult> Update([FromBody] UpdatePurchaseOrderDto dto)
        {
            try
            {
                var response = await _apiService.PutAsync<ApiResponse<PurchaseOrderDto>>(
                    ApiEndpoints.PurchaseOrders.Update.Replace("{0}", dto.Id.ToString()),
                    dto
                );
                
                if (response.Success && response.Data != null)
                {
                    return Json(new ApiResponse { Data = response.Data.Data, Success = true, StatusCode = System.Net.HttpStatusCode.OK });
                }
                else
                {
                    // Build detailed error message
                    var errorMessage = response.Message ?? "Có lỗi xảy ra khi cập nhật phiếu nhập hàng";
                    
                    // Add detailed errors if available
                    if (response.Errors != null && response.Errors.Any())
                    {
                        errorMessage += "\n" + string.Join("\n", response.Errors);
                    }
                    
                    // Add error message if available
                    if (!string.IsNullOrEmpty(response.ErrorMessage))
                    {
                        errorMessage += "\n" + response.ErrorMessage;
                    }
                    
                    return Json(new { success = false, error = errorMessage });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// Lấy danh sách phiếu nhập hàng cho DataTable thông qua API với pagination
        /// </summary>
        [HttpGet("GetPurchaseOrders")]
        public async Task<IActionResult> GetPurchaseOrders(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? status = null,
            int? supplierId = null)
        {
            try
            {
                // Build query parameters
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (!string.IsNullOrEmpty(searchTerm))
                    queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
                if (!string.IsNullOrEmpty(status))
                    queryParams.Add($"status={Uri.EscapeDataString(status)}");
                if (supplierId.HasValue)
                    queryParams.Add($"supplierId={supplierId.Value}");

                var endpoint = ApiEndpoints.PurchaseOrders.GetAll + "?" + string.Join("&", queryParams);
                var response = await _apiService.GetAsync<PagedResponse<PurchaseOrderDto>>(endpoint);
                
                if (response.Success && response.Data != null)
                {
                    return Json(response.Data);
                }
                else
                {
                    return Json(new PagedResponse<PurchaseOrderDto>
                    {
                        Data = new List<PurchaseOrderDto>(),
                        TotalCount = 0,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        Success = false,
                        Message = response.ErrorMessage ?? "Lỗi khi lấy danh sách phiếu nhập hàng"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new PagedResponse<PurchaseOrderDto>
                {
                    Data = new List<PurchaseOrderDto>(),
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết phiếu nhập hàng theo OrderNumber thông qua API
        /// </summary>
        [HttpGet("GetPurchaseOrder/{orderNumber}")]
        public async Task<IActionResult> GetPurchaseOrder(string orderNumber)
        {
            try
            {
                var response = await _apiService.GetAsync<ApiResponse<PurchaseOrderDto>>(
                    ApiEndpoints.PurchaseOrders.GetByOrderNumber.Replace("{orderNumber}", orderNumber)
                );
                
                if (response.Success && response.Data != null)
                {
                    var purchaseOrderData = _mapper.Map<PurchaseOrderDetailsViewModel>(response.Data.Data);
                    
                    return Json(new { success = true, data = purchaseOrderData });
                }
                else
                {
                    return Json(new { success = false, error = "Phiếu nhập hàng không tồn tại" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// In phiếu nhập hàng
        /// </summary>
        [HttpGet("PrintPurchaseOrder/{referenceNumber}")]
        public IActionResult PrintPurchaseOrder(string referenceNumber)
        {
            ViewBag.ReferenceNumber = referenceNumber;
            return View();
        }

        /// <summary>
        /// Xác nhận nhập kho thông qua API
        /// </summary>
        [HttpPost("ConfirmReceipt/{referenceNumber}")]
        public async Task<IActionResult> ConfirmReceipt(string referenceNumber, [FromBody] ConfirmReceiptDto dto)
        {
            try
            {
                // First get the purchase order ID by reference number
                var response = await _apiService.GetAsync<List<PurchaseOrderDto>>(ApiEndpoints.PurchaseOrders.GetAll);
                
                if (response.Success && response.Data != null)
                {
                    var purchaseOrder = response.Data.FirstOrDefault(po => po.OrderNumber == referenceNumber);

                    if (purchaseOrder != null)
                    {
                        // Call API to receive the order
                        var receiveResponse = await _apiService.PostAsync<object>(
                            ApiEndpoints.Builder.WithId(ApiEndpoints.PurchaseOrders.ReceiveOrder, purchaseOrder.Id), 
                            new { }
                        );
                        
                        if (receiveResponse.Success)
                        {
                            return Json(new { success = true, message = "Đã xác nhận nhập kho thành công" });
                        }
                        else
                        {
                            return Json(new { success = false, error = receiveResponse.ErrorMessage });
                        }
                    }
                }
                
                return Json(new { success = false, error = "Không tìm thấy phiếu nhập hàng" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        /// <summary>
        /// Hủy phiếu nhập hàng thông qua API
        /// </summary>
        [HttpPost("CancelPurchaseOrder/{referenceNumber}")]
        public async Task<IActionResult> CancelPurchaseOrder(string referenceNumber)
        {
            // TODO: Call API endpoint for cancel purchase order
            // var response = await _apiService.PostAsync<object>(ApiEndpoints.PurchaseOrders.Cancel, new { referenceNumber });
            
            return Json(new { success = true, message = "Đã hủy phiếu nhập hàng thành công" });
        }

        #region Phase 4.2.3: PO Tracking

        /// <summary>
        /// Hiển thị trang Theo dõi PO (PO Tracking)
        /// </summary>
        [HttpGet("Tracking")]
        public IActionResult Tracking()
        {
            return View();
        }

        /// <summary>
        /// Lấy danh sách PO đang chờ giao hàng (In-Transit)
        /// </summary>
        [HttpGet("GetInTransitOrders")]
        public async Task<IActionResult> GetInTransitOrders(
            int pageNumber = 1,
            int pageSize = 20,
            int? supplierId = null,
            string? deliveryStatus = null,
            int? daysUntilDelivery = null)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (supplierId.HasValue)
                    queryParams.Add($"supplierId={supplierId.Value}");
                if (!string.IsNullOrEmpty(deliveryStatus))
                    queryParams.Add($"deliveryStatus={Uri.EscapeDataString(deliveryStatus)}");
                if (daysUntilDelivery.HasValue)
                    queryParams.Add($"daysUntilDelivery={daysUntilDelivery.Value}");

                var endpoint = ApiEndpoints.PurchaseOrders.GetInTransit + "?" + string.Join("&", queryParams);
                var response = await _apiService.GetAsync<PagedResponse<PurchaseOrderDto>>(endpoint);

                if (response.Success && response.Data != null)
                {
                    return Json(response.Data);
                }
                else
                {
                    return Json(new PagedResponse<PurchaseOrderDto>
                    {
                        Data = new List<PurchaseOrderDto>(),
                        TotalCount = 0,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        Success = false,
                        Message = response.ErrorMessage ?? "Lỗi khi lấy danh sách PO đang chờ giao hàng"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new PagedResponse<PurchaseOrderDto>
                {
                    Data = new List<PurchaseOrderDto>(),
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy thông tin tracking của PO
        /// </summary>
        [HttpGet("GetTrackingInfo/{id}")]
        public async Task<IActionResult> GetTrackingInfo(int id)
        {
            try
            {
                var endpoint = ApiEndpoints.PurchaseOrders.GetTracking.Replace("{0}", id.ToString());
                var response = await _apiService.GetAsync<ApiResponse<POTrackingDtos.PurchaseOrderTrackingDto>>(endpoint);

                if (response.Success && response.Data != null)
                {
                    return Json(response);
                }
                else
                {
                    return Json(new ApiResponse<POTrackingDtos.PurchaseOrderTrackingDto>
                    {
                        Success = false,
                        ErrorMessage = response.ErrorMessage ?? "Lỗi khi lấy thông tin tracking"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<POTrackingDtos.PurchaseOrderTrackingDto>
                {
                    Success = false,
                    ErrorMessage = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Cập nhật thông tin tracking của PO
        /// </summary>
        [HttpPut("UpdateTracking/{id}")]
        public async Task<IActionResult> UpdateTracking(int id, [FromBody] POTrackingDtos.UpdateTrackingDto dto)
        {
            try
            {
                var endpoint = ApiEndpoints.PurchaseOrders.UpdateTracking.Replace("{0}", id.ToString());
                var response = await _apiService.PutAsync<ApiResponse<PurchaseOrderDto>>(endpoint, dto);

                if (response.Success && response.Data != null)
                {
                    return Json(response);
                }
                else
                {
                    return Json(new ApiResponse<PurchaseOrderDto>
                    {
                        Success = false,
                        ErrorMessage = response.ErrorMessage ?? "Lỗi khi cập nhật thông tin tracking"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<PurchaseOrderDto>
                {
                    Success = false,
                    ErrorMessage = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Đánh dấu PO đã gửi hàng (chuyển sang InTransit)
        /// </summary>
        [HttpPut("MarkAsInTransit/{id}")]
        public async Task<IActionResult> MarkAsInTransit(int id, [FromBody] POTrackingDtos.MarkInTransitDto dto)
        {
            try
            {
                var endpoint = ApiEndpoints.PurchaseOrders.MarkInTransit.Replace("{0}", id.ToString());
                var response = await _apiService.PutAsync<ApiResponse<PurchaseOrderDto>>(endpoint, dto);

                if (response.Success && response.Data != null)
                {
                    return Json(response);
                }
                else
                {
                    return Json(new ApiResponse<PurchaseOrderDto>
                    {
                        Success = false,
                        ErrorMessage = response.ErrorMessage ?? "Lỗi khi đánh dấu PO đã gửi hàng"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<PurchaseOrderDto>
                {
                    Success = false,
                    ErrorMessage = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy danh sách PO cần cảnh báo (sắp đến hạn, quá hạn)
        /// </summary>
        [HttpGet("GetDeliveryAlerts")]
        public async Task<IActionResult> GetDeliveryAlerts()
        {
            try
            {
                var response = await _apiService.GetAsync<ApiResponse<POTrackingDtos.DeliveryAlertsDto>>(ApiEndpoints.PurchaseOrders.GetDeliveryAlerts);

                if (response.Success && response.Data != null)
                {
                    return Json(response);
                }
                else
                {
                    return Json(new ApiResponse<POTrackingDtos.DeliveryAlertsDto>
                    {
                        Success = false,
                        ErrorMessage = response.ErrorMessage ?? "Lỗi khi lấy danh sách cảnh báo"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<POTrackingDtos.DeliveryAlertsDto>
                {
                    Success = false,
                    ErrorMessage = $"Lỗi: {ex.Message}"
                });
            }
        }

        #endregion

        /// <summary>
        /// Convert status từ API sang display name
        /// </summary>
        private string GetStatusDisplayName(string status)
        {
            return status switch
            {
                "Pending" => "Chờ duyệt",
                "Ordered" => "Đã đặt hàng",
                "PartiallyReceived" => "Nhập một phần",
                "Received" => "Đã nhập",
                "Cancelled" => "Hủy",
                _ => "Chờ nhập"
            };
        }
    }

    /// <summary>
    /// DTO cho xác nhận nhập kho
    /// </summary>
    public class ConfirmReceiptDto
    {
        public List<ReceiptItemDto> Items { get; set; } = new List<ReceiptItemDto>();
        public string? Notes { get; set; }
    }

    public class ReceiptItemDto
    {
        public int StockTransactionId { get; set; }
        public int ActualQuantity { get; set; }
        public string? Notes { get; set; }
    }
}
