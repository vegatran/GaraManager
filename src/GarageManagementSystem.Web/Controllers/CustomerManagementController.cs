using GarageManagementSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller quản lý khách hàng với đầy đủ các thao tác CRUD thông qua API
    /// </summary>
    [Authorize]
    [Route("CustomerManagement")]
    public class CustomerManagementController : Controller
    {
        private readonly ApiService _apiService;

        public CustomerManagementController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Hiển thị trang quản lý khách hàng
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            // Debug: Log authentication status
            Console.WriteLine($"CustomerManagement Index - User authenticated: {User.Identity?.IsAuthenticated}");
            Console.WriteLine($"CustomerManagement Index - User name: {User.Identity?.Name}");
            return View();
        }

        /// <summary>
        /// Hiển thị trang tiếp đón khách hàng - khách đang có xe tại gara
        /// </summary>
        [HttpGet("Reception")]
        public IActionResult Reception()
        {
            ViewBag.Title = "Tiếp Đón Khách Hàng";
            ViewBag.Subtitle = "Khách hàng đang có xe tại gara";
            return View("Reception");
        }

        /// <summary>
        /// Debug endpoint - test redirect
        /// </summary>
        [HttpGet("Test")]
        public IActionResult Test()
        {
            return Content("CustomerManagement Test - No redirect!");
        }

        /// <summary>
        /// Lấy danh sách khách hàng đang hoạt động
        /// </summary>
        [HttpGet("GetActiveCustomers")]
        public async Task<IActionResult> GetActiveCustomers()
        {
            var response = await _apiService.GetAsync<List<CustomerDto>>(ApiEndpoints.Customers.GetAll);
            
            if (response.Success && response.Data != null)
            {
                // Return all customers (no IsActive filter for now)
                return Json(response.Data);
            }
            
            return Json(new List<CustomerDto>());
        }

        /// <summary>
        /// Lấy danh sách khách hàng đang có xe tại gara (đang sửa chữa/kiểm tra)
        /// </summary>
        [HttpGet("GetCustomersWithActiveVehicles")]
        public async Task<IActionResult> GetCustomersWithActiveVehicles()
        {
            try
            {
                // Lấy danh sách xe đang có service order hoặc appointment chưa hoàn thành
                var serviceOrdersResponse = await _apiService.GetAsync<List<ServiceOrderDto>>(ApiEndpoints.ServiceOrders.GetAll);
                var appointmentsResponse = await _apiService.GetAsync<List<AppointmentDto>>(ApiEndpoints.Appointments.GetAll);
                
                var activeCustomerIds = new HashSet<int>();
                
                // Thu thập customer ID từ service orders đang hoạt động
                if (serviceOrdersResponse.Success && serviceOrdersResponse.Data != null)
                {
                    var activeServiceOrders = serviceOrdersResponse.Data
                        .Where(so => so.Status != "Completed" && 
                                   so.Status != "Cancelled")
                        .Select(so => so.CustomerId);
                    foreach (var customerId in activeServiceOrders)
                    {
                        activeCustomerIds.Add(customerId);
                    }
                }
                
                // Thu thập customer ID từ appointments đang hoạt động
                if (appointmentsResponse.Success && appointmentsResponse.Data != null)
                {
                    var activeAppointments = appointmentsResponse.Data
                        .Where(a => a.Status != "Completed" && 
                                  a.Status != "Cancelled" && 
                                  a.Status != "NoShow")
                        .Select(a => a.CustomerId);
                    foreach (var customerId in activeAppointments)
                    {
                        activeCustomerIds.Add(customerId);
                    }
                }
                
                // Lấy thông tin khách hàng
                var customersResponse = await _apiService.GetAsync<List<CustomerDto>>(ApiEndpoints.Customers.GetAll);
                
                if (customersResponse.Success && customersResponse.Data != null)
                {
                    var activeCustomers = customersResponse.Data
                        .Where(c => activeCustomerIds.Contains(c.Id))
                        .ToList();
                    
                    return Json(activeCustomers);
                }
                
                return Json(new List<CustomerDto>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting customers with active vehicles: {ex.Message}");
                return Json(new List<CustomerDto>());
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả khách hàng cho DataTable thông qua API
        /// </summary>
        [HttpGet("GetCustomers")]
        public async Task<IActionResult> GetCustomers()
        {
            var response = await _apiService.GetAsync<List<CustomerDto>>(ApiEndpoints.Customers.GetAll);
            
            if (response.Success)
            {
                var customerList = new List<object>();
                
                if (response.Data != null)
                {
                    customerList = response.Data.Select(c => new
                    {
                        id = c.Id,
                        name = c.Name,
                        email = c.Email ?? "N/A",
                        phone = c.Phone ?? "N/A",
                        address = c.Address ?? "N/A",
                        createdDate = c.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        isActive = true // Assuming active from API
                    }).Cast<object>().ToList();
                }

                return Json(new { 
                    success = true,
                    data = customerList,
                    message = "Lấy danh sách khách hàng thành công"
                });
            }
            else
            {
                return Json(new { error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết khách hàng theo ID thông qua API
        /// </summary>
        [HttpGet("GetCustomer/{id}")]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var response = await _apiService.GetAsync<CustomerDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.Customers.GetById, id)
            );
            
            return Json(response);
        }

        /// <summary>
        /// Tạo khách hàng mới thông qua API
        /// </summary>
        [HttpPost("CreateCustomer")]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto customerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errorMessage = "Dữ liệu không hợp lệ" });
            }

            var response = await _apiService.PostAsync<CustomerDto>(
                ApiEndpoints.Customers.Create,
                customerDto
            );

            return Json(response);
        }

        /// <summary>
        /// Cập nhật thông tin khách hàng thông qua API
        /// </summary>
        [HttpPut("UpdateCustomer/{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerDto customerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errorMessage = "Dữ liệu không hợp lệ" });
            }

            if (id != customerDto.Id)
            {
                return BadRequest(new { success = false, errorMessage = "ID không khớp" });
            }

            var response = await _apiService.PutAsync<CustomerDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.Customers.Update, id),
                customerDto
            );

            return Json(response);
        }

        /// <summary>
        /// Xóa khách hàng thông qua API
        /// </summary>
        [HttpDelete("DeleteCustomer/{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var response = await _apiService.DeleteAsync<CustomerDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.Customers.Delete, id)
            );

            return Json(response);
        }
    }
}