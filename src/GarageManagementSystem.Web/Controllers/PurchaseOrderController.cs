using GarageManagementSystem.Shared.DTOs;
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
        /// Lấy danh sách phiếu nhập hàng cho DataTable thông qua API
        /// </summary>
        [HttpGet("GetPurchaseOrders")]
        public async Task<IActionResult> GetPurchaseOrders()
        {
            var response = await _apiService.GetAsync<List<PurchaseOrderDto>>(ApiEndpoints.PurchaseOrders.GetAll);
            
            if (response.Success)
            {
                var purchaseOrderList = new List<object>();
                
                if (response.Data != null)
                {
                    // Map API response to frontend format
                    var purchaseOrders = response.Data.Select(po => new
                    {
                        referenceNumber = po.OrderNumber,
                        transactionDate = po.OrderDate,
                        supplierId = po.SupplierId,
                        supplierName = po.SupplierName ?? "Chưa có nhà cung cấp",
                        itemCount = po.ItemCount,
                        totalAmount = po.TotalAmount,
                        status = GetStatusDisplayName(po.Status)
                    })
                    .OrderByDescending(po => po.transactionDate)
                    .ToList();

                    purchaseOrderList = purchaseOrders.Cast<object>().ToList();
                }

                return Json(new { 
                    success = true,
                    data = purchaseOrderList,
                    message = "Lấy danh sách phiếu nhập hàng thành công"
                });
            }
            else
            {
                return Json(new { error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết phiếu nhập hàng theo ReferenceNumber thông qua API
        /// </summary>
        [HttpGet("GetPurchaseOrderDetails/{referenceNumber}")]
        public async Task<IActionResult> GetPurchaseOrderDetails(string referenceNumber)
        {
            // First get all purchase orders to find the one with matching OrderNumber
            var response = await _apiService.GetAsync<List<PurchaseOrderDto>>(ApiEndpoints.PurchaseOrders.GetAll);
            
            if (response.Success && response.Data != null)
            {
                var purchaseOrder = response.Data.FirstOrDefault(po => po.OrderNumber == referenceNumber);

                if (purchaseOrder != null)
                {
                    var purchaseOrderDetails = new
                    {
                        referenceNumber = purchaseOrder.OrderNumber,
                        transactionDate = purchaseOrder.OrderDate,
                        supplierId = purchaseOrder.SupplierId,
                        supplierName = purchaseOrder.SupplierName ?? "Chưa có nhà cung cấp",
                        totalAmount = purchaseOrder.TotalAmount,
                        itemCount = purchaseOrder.ItemCount,
                        items = purchaseOrder.Items.Select(item => new
                        {
                            id = item.Id,
                            partName = item.PartName ?? "N/A",
                            partNumber = item.SupplierPartNumber ?? "N/A",
                            quantity = item.QuantityOrdered,
                            unitPrice = item.UnitPrice,
                            totalAmount = item.TotalPrice,
                            hasInvoice = true,
                            notes = item.Notes
                        }).ToList()
                    };

                    return Json(new { success = true, data = purchaseOrderDetails });
                }
            }

            return Json(new { success = false, error = "Không tìm thấy phiếu nhập hàng" });
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
