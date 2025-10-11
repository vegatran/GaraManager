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
        [HttpGet]
        [Route("GetCustomers")]
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

                return Json(new { data = customerList });
            }
            else
            {
                return Json(new { error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết khách hàng theo ID thông qua API
        /// </summary>
        [HttpGet]
        [Route("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var response = await _apiService.GetAsync<CustomerDto>(ApiEndpoints.Builder.WithId(ApiEndpoints.Customers.GetById, id));
            
            if (response.Success)
            {
                return PartialView("_CustomerDetails", response.Data);
            }
            else
            {
                return Json(new { error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Tạo khách hàng mới thông qua API
        /// </summary>
        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create(CreateCustomerDto model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model data" });
            }

            var response = await _apiService.PostAsync<object>(ApiEndpoints.Customers.Create, model);
            
            return Json(new { 
                success = response.Success, 
                message = response.Success ? "Customer created successfully" : response.ErrorMessage 
            });
        }

        /// <summary>
        /// Cập nhật thông tin khách hàng thông qua API
        /// </summary>
        [HttpPost]
        [Route("Edit")]
        public async Task<IActionResult> Edit(UpdateCustomerDto model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model data" });
            }

            var response = await _apiService.PutAsync<object>(ApiEndpoints.Builder.WithId(ApiEndpoints.Customers.Update, model.Id), model);
            
            return Json(new { 
                success = response.Success, 
                message = response.Success ? "Customer updated successfully" : response.ErrorMessage 
            });
        }

        /// <summary>
        /// Xóa khách hàng thông qua API
        /// </summary>
        [HttpPost]
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _apiService.DeleteAsync<object>(ApiEndpoints.Builder.WithId(ApiEndpoints.Customers.Delete, id));
            
            return Json(new { 
                success = response.Success, 
                message = response.Success ? "Customer deleted successfully" : response.ErrorMessage 
            });
        }
    }
}