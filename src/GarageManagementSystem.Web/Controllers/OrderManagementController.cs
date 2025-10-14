using GarageManagementSystem.Shared.DTOs;
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
        /// Lấy thông tin chi tiết đơn hàng sửa chữa theo ID thông qua API
        /// </summary>
        [HttpGet("GetOrder/{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var response = await _apiService.GetAsync<ServiceOrderDto>(ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceOrders.GetById, id));
            
            return Json(response);
        }

        /// <summary>
        /// Lấy danh sách khách hàng cho dropdown
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
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errorMessage = "Dữ liệu không hợp lệ" });
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

        private static string TranslateOrderStatus(string status)
        {
            return status switch
            {
                "Pending" => "Chờ Xử Lý",
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

