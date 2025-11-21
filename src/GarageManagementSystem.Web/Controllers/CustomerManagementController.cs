using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
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
            // ✅ FIX: Đổi sang GetAsync<List<CustomerDto>> để tránh double nesting
            var response = await _apiService.GetAsync<List<CustomerDto>>(ApiEndpoints.Customers.GetAll);
            
            if (response.Success && response.Data != null)
            {
                // ✅ FIX: response.Data giờ là List<CustomerDto> trực tiếp (không cần .Data.Data)
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
        /// Lấy danh sách tất cả khách hàng cho DataTable thông qua API với pagination
        /// </summary>
        [HttpGet("GetCustomers")]
        public async Task<IActionResult> GetCustomers(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
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

                var queryString = string.Join("&", queryParams);
                var endpoint = $"{ApiEndpoints.Customers.GetAll}?{queryString}";

                var response = await _apiService.GetAsync<PagedResponse<CustomerDto>>(endpoint);
                
                if (response.Success && response.Data != null)
                {
                    var customerList = response.Data.Data.Select(c => new
                    {
                        id = c.Id,
                        name = c.Name,
                        email = c.Email ?? "N/A",
                        phone = c.Phone ?? "N/A",
                        address = c.Address ?? "N/A",
                        createdDate = c.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        isActive = true // Assuming active from API
                    }).ToList();

                    return Json(new { 
                        success = true,
                        data = customerList,
                        totalCount = response.Data.TotalCount,
                        message = "Lấy danh sách khách hàng thành công"
                    });
                }
                else
                {
                    return Json(new { 
                        success = false,
                        error = response.ErrorMessage ?? "Lỗi khi lấy danh sách khách hàng"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false,
                    error = "Lỗi khi lấy danh sách khách hàng: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết khách hàng theo ID thông qua API
        /// </summary>
        [HttpGet("GetCustomer/{id}")]
        public async Task<IActionResult> GetCustomer(int id)
        {
            // ✅ FIX: API trả về ApiResponse<CustomerDto>, ApiService cũng wrap thành ApiResponse
            // Nên response.Data là ApiResponse<CustomerDto>, response.Data.Data là CustomerDto
            var response = await _apiService.GetAsync<CustomerDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.Customers.GetById, id)
            );
            
            if (response.Success && response.Data != null)
            {
                // ✅ FIX: response.Data giờ là CustomerDto trực tiếp (không cần .Data.Data)
                var customer = response.Data;
                var customerData = new
                {
                    id = customer.Id,
                    name = customer.Name,
                    email = customer.Email ?? "N/A",
                    phone = customer.Phone ?? "N/A",
                    address = customer.Address ?? "N/A",
                    alternativePhone = customer.AlternativePhone ?? "N/A",
                    contactPersonName = customer.ContactPersonName ?? "N/A"
                };
                
                return Json(new ApiResponse { Data = customerData, Success = true, StatusCode = System.Net.HttpStatusCode.OK });
            }
            
            return Json(new { success = false, error = response.ErrorMessage ?? "Customer not found" });
        }

        /// <summary>
        /// Tạo khách hàng mới thông qua API
        /// </summary>
        [HttpPost]
        [Route("CreateCustomer")]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto customerDto)
        {
            try
            {
                // Log input data for debugging
                Console.WriteLine($"DEBUG: CreateCustomer input: {System.Text.Json.JsonSerializer.Serialize(customerDto)}");

                if (!ModelState.IsValid)
                {
                    var errors = new Dictionary<string, string[]>();
                    foreach (var key in ModelState.Keys)
                    {
                        var modelErrors = ModelState[key].Errors.Select(e => e.ErrorMessage).ToArray();
                        if (modelErrors.Length > 0)
                        {
                            errors[key] = modelErrors;
                        }
                    }
                    Console.WriteLine($"DEBUG: ModelState errors: {System.Text.Json.JsonSerializer.Serialize(errors)}");
                    return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = errors });
                }

                var response = await _apiService.PostAsync<CustomerDto>(
                    ApiEndpoints.Customers.Create,
                    customerDto
                );

                if (!response.Success)
                {
                    Console.WriteLine($"DEBUG: API Response Error: {response.ErrorMessage}");
                }

                return Json(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Exception in CreateCustomer: {ex.Message}");
                return BadRequest(new { success = false, errorMessage = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật thông tin khách hàng thông qua API
        /// </summary>
        [HttpPut("UpdateCustomer/{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerDto customerDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = new Dictionary<string, string[]>();
                foreach (var key in ModelState.Keys)
                {
                    var modelErrors = ModelState[key].Errors.Select(e => e.ErrorMessage).ToArray();
                    if (modelErrors.Length > 0)
                    {
                        errors[key] = modelErrors;
                    }
                }
                Console.WriteLine($"DEBUG: UpdateCustomer ModelState errors: {System.Text.Json.JsonSerializer.Serialize(errors)}");
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = errors });
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