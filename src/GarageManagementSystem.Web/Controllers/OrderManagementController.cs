using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller quản lý đơn hàng sửa chữa với đầy đủ các thao tác CRUD thông qua API
    /// </summary>
    [Authorize]
    [Route("OrderManagement")]
    public class OrderManagementController : Controller
    {
        private readonly ApiService _apiService;

        public OrderManagementController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Hiển thị trang quản lý đơn hàng sửa chữa
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Lấy danh sách tất cả đơn hàng sửa chữa cho DataTable thông qua API
        /// </summary>
        [HttpGet("GetOrders")]
        public async Task<IActionResult> GetOrders()
        {
            var response = await _apiService.GetAsync<List<ServiceOrderDto>>(ApiEndpoints.ServiceOrders.GetAll);
            
            if (response.Success)
            {
                var orderList = new List<object>();
                
                if (response.Data != null)
                {
                    orderList = response.Data.Select(o => new
                    {
                        id = o.Id,
                        orderNumber = o.OrderNumber,
                        customerName = o.Customer?.Name ?? "Không xác định",
                        vehiclePlate = o.Vehicle?.LicensePlate ?? "Không xác định",
                        orderDate = o.OrderDate.ToString("yyyy-MM-dd HH:mm"),
                        scheduledDate = o.ScheduledDate?.ToString("yyyy-MM-dd HH:mm") ?? "Chưa lên lịch",
                        completedDate = o.CompletedDate?.ToString("yyyy-MM-dd HH:mm") ?? "Chưa hoàn thành",
                        status = TranslateOrderStatus(o.Status),
                        statusOriginal = o.Status, // ✅ THÊM: Giữ nguyên status gốc để JS xử lý
                        finalAmount = o.FinalAmount.ToString("N0"),
                        paymentStatus = TranslatePaymentStatus(o.PaymentStatus ?? "Pending"),
                        serviceCount = o.ServiceOrderItems.Count
                    }).Cast<object>().ToList();
                }

                return Json(new { 
                    success = true,
                    data = orderList,
                    message = "Lấy danh sách đơn hàng sửa chữa thành công"
                });
            }
            else
            {
                return Json(new { error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// ✅ THÊM: Lấy danh sách đơn hàng với server-side pagination cho DataTable
        /// </summary>
        [HttpGet("GetOrdersPaged")]
        public async Task<IActionResult> GetOrdersPaged(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? status = null)
        {
            try
            {
                // Build query string
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (!string.IsNullOrEmpty(searchTerm))
                    queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
                if (!string.IsNullOrEmpty(status))
                    queryParams.Add($"status={Uri.EscapeDataString(status)}");

                var endpoint = ApiEndpoints.ServiceOrders.GetAll + "?" + string.Join("&", queryParams);
                var response = await _apiService.GetAsync<PagedResponse<ServiceOrderDto>>(endpoint);

                if (response.Success && response.Data != null)
                {
                    var orderList = response.Data.Data.Select(o => new
                    {
                        id = o.Id,
                        orderNumber = o.OrderNumber,
                        customerName = o.Customer?.Name ?? "Không xác định",
                        vehiclePlate = o.Vehicle?.LicensePlate ?? "Không xác định",
                        orderDate = o.OrderDate.ToString("yyyy-MM-dd HH:mm"),
                        scheduledDate = o.ScheduledDate?.ToString("yyyy-MM-dd HH:mm") ?? "Chưa lên lịch",
                        completedDate = o.CompletedDate?.ToString("yyyy-MM-dd HH:mm") ?? "Chưa hoàn thành",
                        status = TranslateOrderStatus(o.Status),
                        statusOriginal = o.Status, // ✅ THÊM: Giữ nguyên status gốc để JS xử lý
                        finalAmount = o.FinalAmount.ToString("N0"),
                        paymentStatus = TranslatePaymentStatus(o.PaymentStatus ?? "Pending"),
                        serviceCount = o.ServiceOrderItems?.Count ?? 0,
                        serviceOrderId = o.Id // ✅ THÊM: Để check trong JS
                    }).Cast<object>().ToList();

                    return Json(new
                    {
                        success = true,
                        data = orderList,
                        totalCount = response.Data.TotalCount,
                        pageNumber = response.Data.PageNumber,
                        pageSize = response.Data.PageSize
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        data = new List<object>(),
                        totalCount = 0,
                        pageNumber = pageNumber,
                        pageSize = pageSize,
                        message = response.ErrorMessage ?? "Lỗi khi lấy danh sách đơn hàng"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    data = new List<object>(),
                    totalCount = 0,
                    pageNumber = pageNumber,
                    pageSize = pageSize,
                    message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết đơn hàng sửa chữa theo ID thông qua API
        /// </summary>
        [HttpGet("GetOrder/{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var response = await _apiService.GetAsync<ServiceOrderDto>(ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceOrders.GetById, id));
            
            return Json(response);
        }

        /// <summary>
        /// Lấy danh sách ServiceQuotation đã được phê duyệt để tạo phiếu sửa chữa
        /// </summary>
        [HttpGet("GetAvailableQuotations")]
        public async Task<IActionResult> GetAvailableQuotations()
        {
            try
            {
                // ✅ Dùng API chuyên biệt để lấy danh sách báo giá đã duyệt, đủ điều kiện tạo SO
                var response = await _apiService.GetAsync<ApiResponse<List<object>>>(ApiEndpoints.ServiceQuotations.GetApprovedAvailableForOrder);
                
                if (response.Success && response.Data?.Data != null)
                {
                    return Json(response.Data.Data);
                }
                else
                {
                    // ✅ THÊM: Log lỗi nếu có
                    Console.WriteLine($"[GetAvailableQuotations] API Error: {response.ErrorMessage ?? "Unknown error"}");
                    return Json(new List<object>());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetAvailableQuotations] Exception: {ex.Message}");
                Console.WriteLine($"[GetAvailableQuotations] StackTrace: {ex.StackTrace}");
                return Json(new List<object>());
            }
        }

        /// <summary>
        /// Lấy danh sách khách hàng cho dropdown (legacy - không dùng trong quy trình mới)
        /// </summary>
        [HttpGet("GetCustomers")]
        public async Task<IActionResult> GetCustomers()
        {
            var response = await _apiService.GetAsync<List<CustomerDto>>(ApiEndpoints.Customers.GetAll);
            
            if (response.Success)
            {
                var customers = response.Data?.Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    phone = c.Phone ?? ""
                }).Cast<object>().ToList() ?? new List<object>();

                return Json(new { success = true, data = customers });
            }
            else
            {
                return Json(new { success = false, error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Lấy danh sách xe theo khách hàng
        /// </summary>
        [HttpGet("GetVehiclesByCustomer/{customerId}")]
        public async Task<IActionResult> GetVehiclesByCustomer(int customerId)
        {
            var response = await _apiService.GetAsync<List<VehicleDto>>(ApiEndpoints.Builder.WithId(ApiEndpoints.Vehicles.GetByCustomerId, customerId));
            
            if (response.Success)
            {
                var vehicles = response.Data?.Select(v => new
                {
                    id = v.Id,
                    licensePlate = v.LicensePlate,
                    brand = v.Brand,
                    model = v.Model,
                    displayText = $"{v.LicensePlate} - {v.Brand} {v.Model}"
                }).Cast<object>().ToList() ?? new List<object>();

                return Json(new { success = true, data = vehicles });
            }
            else
            {
                return Json(new { success = false, error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Lấy danh sách dịch vụ có sẵn
        /// </summary>
        [HttpGet("GetServices")]
        public async Task<IActionResult> GetServices()
        {
            var response = await _apiService.GetAsync<List<ServiceDto>>(ApiEndpoints.Services.GetAll);
            
            if (response.Success)
            {
                var services = response.Data?
                    .Where(s => s.IsActive)
                    .Select(s => new
                    {
                        id = s.Id,
                        name = s.Name,
                        price = s.Price,
                        duration = s.Duration,
                        category = s.Category ?? "General"
                    }).Cast<object>().ToList() ?? new List<object>();

                return Json(new { success = true, data = services });
            }
            else
            {
                return Json(new { success = false, error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Tạo đơn hàng sửa chữa mới thông qua API
        /// </summary>
        [HttpPost]
        [Route("CreateOrder")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateServiceOrderDto model)
        {
            // Cho phép tạo từ Báo Giá mà không gửi kèm ServiceOrderItems (API sẽ tự copy từ Quotation)
            if (!ModelState.IsValid)
            {
                if (!model.ServiceQuotationId.HasValue)
                {
                    return BadRequest(new { success = false, errorMessage = "Dữ liệu không hợp lệ" });
                }
                // Bỏ qua lỗi validation của ServiceOrderItems khi có ServiceQuotationId
                if (ModelState.ContainsKey("ServiceOrderItems"))
                {
                    ModelState.Remove("ServiceOrderItems");
                }
            }

            var response = await _apiService.PostAsync<ServiceOrderDto>(ApiEndpoints.ServiceOrders.Create, model);
            
            return Json(response);
        }

        /// <summary>
        /// Cập nhật đơn hàng sửa chữa thông qua API
        /// </summary>
        [HttpPut("UpdateOrder/{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateServiceOrderDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errorMessage = "Dữ liệu không hợp lệ" });
            }

            if (id != model.Id)
            {
                return BadRequest(new { success = false, errorMessage = "ID không khớp" });
            }

            var response = await _apiService.PutAsync<ServiceOrderDto>(ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceOrders.Update, id), model);
            
            return Json(response);
        }

        /// <summary>
        /// Xóa đơn hàng sửa chữa thông qua API
        /// </summary>
        [HttpDelete("DeleteOrder/{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var response = await _apiService.DeleteAsync<ServiceOrderDto>(ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceOrders.Delete, id));
            
            return Json(response);
        }

        /// <summary>
        /// Lấy danh sách nhân viên đang hoạt động cho dropdown
        /// </summary>
        [HttpGet("GetActiveEmployees")]
        public async Task<IActionResult> GetActiveEmployees()
        {
            var response = await _apiService.GetAsync<List<EmployeeDto>>(ApiEndpoints.Employees.GetActive);
            
            if (response.Success && response.Data != null)
            {
                var employeeList = response.Data.Select(e => new
                {
                    id = e.Id,
                    text = e.Name + " - " + (e.Position ?? "")
                }).Cast<object>().ToList();

                return Json(employeeList);
            }

            return Json(new List<object>());
        }

        /// <summary>
        /// ✅ 2.1.1: Chuyển trạng thái ServiceOrder
        /// </summary>
        [HttpPut("ChangeOrderStatus/{id}")]
        public async Task<IActionResult> ChangeOrderStatus(int id, [FromBody] ChangeServiceOrderStatusDto statusDto)
        {
            var response = await _apiService.PutAsync<ServiceOrderDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceOrders.ChangeStatus, id),
                statusDto
            );
            return Json(response);
        }

        /// <summary>
        /// ✅ 2.1.2: Phân công KTV cho một item
        /// </summary>
        [HttpPut("AssignTechnician/{orderId}/items/{itemId}")]
        public async Task<IActionResult> AssignTechnicianToItem(int orderId, int itemId, [FromBody] AssignTechnicianDto assignDto)
        {
            var endpoint = $"/api/ServiceOrders/{orderId}/items/{itemId}/assign-technician";
            var response = await _apiService.PutAsync<ServiceOrderDto>(endpoint, assignDto);
            return Json(response);
        }

        /// <summary>
        /// ✅ 2.1.2: Phân công hàng loạt
        /// </summary>
        [HttpPut("BulkAssignTechnician/{orderId}")]
        public async Task<IActionResult> BulkAssignTechnician(int orderId, [FromBody] BulkAssignTechnicianDto bulkDto)
        {
            var endpoint = $"/api/ServiceOrders/{orderId}/bulk-assign-technician";
            var response = await _apiService.PutAsync<ServiceOrderDto>(endpoint, bulkDto);
            return Json(response);
        }

        /// <summary>
        /// ✅ 2.1.2: Cập nhật giờ công dự kiến
        /// </summary>
        [HttpPut("SetEstimatedHours/{orderId}/items/{itemId}")]
        public async Task<IActionResult> SetEstimatedHours(int orderId, int itemId, [FromBody] decimal estimatedHours)
        {
            var endpoint = $"/api/ServiceOrders/{orderId}/items/{itemId}/set-estimated-hours";
            var response = await _apiService.PutAsync<ServiceOrderDto>(endpoint, estimatedHours);
            return Json(response);
        }

        private static string TranslateOrderStatus(string status)
        {
            return status switch
            {
                "Pending" => "Chờ Xử Lý",
                "PendingAssignment" => "Chờ Phân Công",
                "WaitingForParts" => "Đang Chờ Vật Tư",
                "ReadyToWork" => "Sẵn Sàng Làm",
                "Confirmed" => "Đã Xác Nhận",
                "InProgress" => "Đang Thực Hiện",
                "Completed" => "Hoàn Thành",
                "Cancelled" => "Đã Hủy",
                "OnHold" => "Tạm Dừng",
                _ => status
            };
        }

        private static string TranslatePaymentStatus(string status)
        {
            return status switch
            {
                "Pending" => "Chờ Thanh Toán",
                "Paid" => "Đã Thanh Toán",
                "Partial" => "Thanh Toán Một Phần",
                "Overdue" => "Quá Hạn",
                "Refunded" => "Đã Hoàn Tiền",
                _ => status
            };
        }
    }
}

