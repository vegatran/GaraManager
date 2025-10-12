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
        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            // Debug: Log authentication status
            Console.WriteLine($"CustomerManagement Index - User authenticated: {User.Identity?.IsAuthenticated}");
            Console.WriteLine($"CustomerManagement Index - User name: {User.Identity?.Name}");
            return View();
        }

        /// <summary>
        /// Debug endpoint - test redirect
        /// </summary>
        [Route("Test")]
        public IActionResult Test()
        {
            return Content("CustomerManagement Test - No redirect!");
        }

        /// <summary>
        /// Lấy danh sách khách hàng đang hoạt động
        /// </summary>
        [HttpGet]
        [Route("GetActiveCustomers")]
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
        [HttpPost]
        [Route("CreateCustomer")]
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