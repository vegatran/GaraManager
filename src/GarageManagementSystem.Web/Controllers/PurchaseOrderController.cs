using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;

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

        public PurchaseOrderController(ApiService apiService)
        {
            _apiService = apiService;
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
        /// Lấy thông tin chi tiết phiếu nhập hàng theo ID thông qua API
        /// </summary>
        [HttpGet("GetPurchaseOrder/{id}")]
        public async Task<IActionResult> GetPurchaseOrder(int id)
        {
            try
            {
                var response = await _apiService.GetAsync<ApiResponse<PurchaseOrderDto>>(
                    ApiEndpoints.Builder.WithId(ApiEndpoints.PurchaseOrders.GetById, id)
                );
                
                if (response.Success && response.Data != null)
                {
                    var purchaseOrder = response.Data.Data;
                    var purchaseOrderData = new
                    {
                        id = purchaseOrder.Id,
                        orderNumber = purchaseOrder.OrderNumber,
                        orderDate = purchaseOrder.OrderDate,
                        supplierId = purchaseOrder.SupplierId,
                        supplierName = purchaseOrder.SupplierName,
                        totalAmount = purchaseOrder.TotalAmount,
                        itemCount = purchaseOrder.ItemCount,
                        status = purchaseOrder.Status,
                        notes = purchaseOrder.Notes,
                        items = purchaseOrder.Items?.Select(item => new
                        {
                            id = item.Id,
                            partId = item.PartId,
                            partName = item.PartName,
                            quantity = item.QuantityOrdered,
                            unitPrice = item.UnitPrice,
                            totalPrice = item.TotalPrice
                        }).ToList()
                    };
                    
                    return Json(new ApiResponse { Data = purchaseOrderData, Success = true, StatusCode = System.Net.HttpStatusCode.OK });
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
