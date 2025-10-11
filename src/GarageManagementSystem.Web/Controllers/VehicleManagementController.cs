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
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Lấy danh sách tất cả xe cho DataTable thông qua API
        /// </summary>
        [HttpGet]
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
                        customerName = v.Customer?.Name ?? "N/A",
                        customerId = v.CustomerId,
                        createdDate = v.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                    }).Cast<object>().ToList();
                }

                return Json(new { data = vehicleList });
            }
            else
            {
                return Json(new { error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết xe theo ID thông qua API
        /// </summary>
        [HttpGet]
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
        [HttpGet]
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
        [HttpGet]
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
        [HttpPost]
        public async Task<IActionResult> Create(CreateVehicleDto model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model data" });
            }

            var response = await _apiService.PostAsync<object>(ApiEndpoints.Vehicles.Create, model);
            
            return Json(new { 
                success = response.Success, 
                message = response.Success ? "Vehicle created successfully" : response.ErrorMessage 
            });
        }

        /// <summary>
        /// Cập nhật thông tin xe thông qua API
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Edit(UpdateVehicleDto model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model data" });
            }

            var response = await _apiService.PutAsync<object>(ApiEndpoints.Builder.WithId(ApiEndpoints.Vehicles.Update, model.Id), model);
            
            return Json(new { 
                success = response.Success, 
                message = response.Success ? "Vehicle updated successfully" : response.ErrorMessage 
            });
        }

        /// <summary>
        /// Xóa xe thông qua API
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _apiService.DeleteAsync<object>(ApiEndpoints.Builder.WithId(ApiEndpoints.Vehicles.Delete, id));
            
            return Json(new { 
                success = response.Success, 
                message = response.Success ? "Vehicle deleted successfully" : response.ErrorMessage 
            });
        }
    }
}

