using GarageManagementSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller quản lý dịch vụ with full CRUD operations via API
    /// </summary>
    [Authorize]
    public class ServiceManagementController : Controller
    {
        private readonly ApiService _apiService;

        public ServiceManagementController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Hiển thị trang quản lý dịch vụ
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Lấy danh sách tất cả dịch vụ cho DataTable thông qua API
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetServices()
        {
            var response = await _apiService.GetAsync<List<ServiceDto>>(ApiEndpoints.Services.GetAll);
            
            if (response.Success)
            {
                var serviceList = new List<object>();
                
                if (response.Data != null)
                {
                    serviceList = response.Data.Select(s => new
                    {
                        id = s.Id,
                        name = s.Name,
                        description = s.Description ?? "N/A",
                        price = s.Price.ToString("N0"),
                        priceValue = s.Price,
                        duration = s.Duration,
                        category = s.Category ?? "N/A",
                        isActive = s.IsActive,
                        status = s.IsActive ? "Active" : "Inactive",
                        createdDate = s.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                    }).Cast<object>().ToList();
                }

                return Json(new { data = serviceList });
            }
            else
            {
                return Json(new { error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết dịch vụ theo ID thông qua API
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var response = await _apiService.GetAsync<ServiceDto>(ApiEndpoints.Builder.WithId(ApiEndpoints.Services.GetById, id));
            
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
        /// Tạo dịch vụ mới thông qua API
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(CreateServiceDto model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model data" });
            }

            var response = await _apiService.PostAsync<object>(ApiEndpoints.Services.Create, model);
            
            return Json(new { 
                success = response.Success, 
                message = response.Success ? "Service created successfully" : response.ErrorMessage 
            });
        }

        /// <summary>
        /// Cập nhật thông tin dịch vụ thông qua API
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Edit(UpdateServiceDto model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model data" });
            }

            var response = await _apiService.PutAsync<object>(ApiEndpoints.Builder.WithId(ApiEndpoints.Services.Update, model.Id), model);
            
            return Json(new { 
                success = response.Success, 
                message = response.Success ? "Service updated successfully" : response.ErrorMessage 
            });
        }

        /// <summary>
        /// Xóa dịch vụ thông qua API
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _apiService.DeleteAsync<object>(ApiEndpoints.Builder.WithId(ApiEndpoints.Services.Delete, id));
            
            return Json(new { 
                success = response.Success, 
                message = response.Success ? "Service deleted successfully" : response.ErrorMessage 
            });
        }
    }
}

