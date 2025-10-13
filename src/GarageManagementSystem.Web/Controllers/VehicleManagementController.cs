using GarageManagementSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller quản lý xe with full CRUD operations via API
    /// </summary>
    [Authorize]
    [Route("VehicleManagement")]
    public class VehicleManagementController : Controller
    {
        private readonly ApiService _apiService;

        public VehicleManagementController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Hiển thị trang quản lý xe
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Lấy danh sách tất cả xe cho DataTable thông qua API
        /// </summary>
        [HttpGet("GetVehicles")]
        public async Task<IActionResult> GetVehicles()
        {
            var response = await _apiService.GetAsync<List<VehicleDto>>(ApiEndpoints.Vehicles.GetAll);
            
            if (response.Success)
            {
                var vehicleList = new List<object>();
                
                if (response.Data != null)
                {
                    vehicleList = response.Data.Select(v => new
                    {
                        id = v.Id,
                        licensePlate = v.LicensePlate,
                        brand = v.Brand,
                        model = v.Model,
                        year = v.Year ?? "N/A",
                        color = v.Color ?? "N/A",
                        vin = v.VIN ?? "N/A",
                        engineNumber = v.EngineNumber ?? "N/A",
                        customerName = v.CustomerName ?? "N/A",
                        customerId = v.CustomerId,
                        createdDate = v.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                    }).Cast<object>().ToList();
                }

                return Json(new { 
                    success = true,
                    data = vehicleList,
                    message = "Lấy danh sách xe thành công"
                });
            }
            else
            {
                return Json(new { error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết xe theo ID thông qua API
        /// </summary>
        [HttpGet("GetVehicle/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var response = await _apiService.GetAsync<VehicleDto>(ApiEndpoints.Builder.WithId(ApiEndpoints.Vehicles.GetById, id));
            
            if (response.Success)
            {
                return Json(new { success = true, data = response.Data });
            }
            else
            {
                return Json(new { success = false, error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Get vehicles by customer ID
        /// </summary>
        [HttpGet("GetByCustomer/{customerId}")]
        public async Task<IActionResult> GetByCustomer(int customerId)
        {
            var response = await _apiService.GetAsync<List<VehicleDto>>(ApiEndpoints.Builder.WithId(ApiEndpoints.Vehicles.GetByCustomerId, customerId));
            
            if (response.Success)
            {
                return Json(new { success = true, data = response.Data });
            }
            else
            {
                return Json(new { success = false, error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Get all customers for dropdown
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
                    name = c.Name
                }).Cast<object>().ToList() ?? new List<object>();

                return Json(new { success = true, data = customers });
            }
            else
            {
                return Json(new { success = false, error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Tạo xe mới thông qua API
        /// </summary>
        [HttpPost("CreateVehicle")]
        public async Task<IActionResult> Create(CreateVehicleDto model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model data" });
            }

            var response = await _apiService.PostAsync<VehicleDto>(ApiEndpoints.Vehicles.Create, model);
            
            return Json(response);
        }

        /// <summary>
        /// Cập nhật thông tin xe thông qua API
        /// </summary>
        [HttpPut("UpdateVehicle/{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] UpdateVehicleDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errorMessage = "Dữ liệu không hợp lệ" });
            }

            if (id != model.Id)
            {
                return BadRequest(new { success = false, errorMessage = "ID không khớp" });
            }

            var response = await _apiService.PutAsync<VehicleDto>(ApiEndpoints.Builder.WithId(ApiEndpoints.Vehicles.Update, id), model);
            
            return Json(response);
        }

        /// <summary>
        /// Xóa xe thông qua API
        /// </summary>
        [HttpDelete("DeleteVehicle/{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var response = await _apiService.DeleteAsync<VehicleDto>(ApiEndpoints.Builder.WithId(ApiEndpoints.Vehicles.Delete, id));
            
            return Json(response);
        }

        /// <summary>
        /// Lấy danh sách khách hàng đang hoạt động cho dropdown
        /// </summary>
        [HttpGet("GetActiveCustomers")]
        public async Task<IActionResult> GetActiveCustomers()
        {
            var response = await _apiService.GetAsync<List<CustomerDto>>(ApiEndpoints.Customers.GetAll);
            
            if (response.Success && response.Data != null)
            {
                var customerList = response.Data.Select(c => new
                {
                    id = c.Id,
                    name = c.Name
                }).Cast<object>().ToList();

                return Json(customerList);
            }

            return Json(new List<object>());
        }
    }
}

