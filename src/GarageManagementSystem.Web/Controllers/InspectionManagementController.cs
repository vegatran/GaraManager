using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;
using GarageManagementSystem.Web.Models;
using GarageManagementSystem.Shared.Models;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller quản lý kiểm tra xe với đầy đủ các thao tác CRUD thông qua API
    /// </summary>
    [Authorize]
    [Route("InspectionManagement")]
    public class InspectionManagementController : Controller
    {
        private readonly ApiService _apiService;

        public InspectionManagementController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Hiển thị trang quản lý kiểm tra xe
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Lấy danh sách tất cả kiểm tra xe cho DataTable thông qua API
        /// </summary>
        [HttpGet("GetInspections")]
        public async Task<IActionResult> GetInspections()
        {
            var response = await _apiService.GetAsync<List<VehicleInspectionDto>>(ApiEndpoints.VehicleInspections.GetAll);
            
            if (response.Success)
            {
                var inspectionList = new List<object>();
                
                if (response.Data != null)
                {
                    inspectionList = response.Data.Select(i => new
                    {
                        id = i.Id,
                        inspectionNumber = i.InspectionNumber,
                        vehicleInfo = $"{i.Vehicle?.Brand} {i.Vehicle?.Model} - {i.Vehicle?.LicensePlate}",
                        customerName = i.Customer?.Name ?? "N/A",
                        inspectorName = i.Inspector?.Name ?? "N/A",
                        inspectionDate = i.InspectionDate,
                        status = TranslateInspectionStatus(i.Status),
                        currentMileage = i.CurrentMileage?.ToString() ?? "N/A",
                        generalCondition = i.GeneralCondition ?? "N/A",
                        createdDate = i.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                    }).Cast<object>().ToList();
                }

                return Json(new { 
                    success = true,
                    data = inspectionList,
                    message = "Lấy danh sách kiểm tra xe thành công"
                });
            }
            else
            {
                return Json(new { error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết kiểm tra xe theo ID thông qua API
        /// </summary>
        [HttpGet("GetInspection/{id}")]
        public async Task<IActionResult> GetInspection(int id)
        {
            var response = await _apiService.GetAsync<VehicleInspectionDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.VehicleInspections.GetById, id)
            );
            
            return Json(response);
        }

        /// <summary>
        /// Tạo kiểm tra xe mới thông qua API
        /// </summary>
        [HttpPost("CreateInspection")]
        public async Task<IActionResult> CreateInspection([FromBody] CreateVehicleInspectionDto inspectionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errorMessage = "Dữ liệu không hợp lệ" });
            }

            var response = await _apiService.PostAsync<VehicleInspectionDto>(
                ApiEndpoints.VehicleInspections.Create,
                inspectionDto
            );

            return Json(response);
        }

        /// <summary>
        /// Cập nhật thông tin kiểm tra xe thông qua API
        /// </summary>
        [HttpPut("UpdateInspection/{id}")]
        public async Task<IActionResult> UpdateInspection(int id, [FromBody] UpdateVehicleInspectionDto inspectionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errorMessage = "Dữ liệu không hợp lệ" });
            }

            if (id != inspectionDto.Id)
            {
                return BadRequest(new { success = false, errorMessage = "ID không khớp" });
            }

            var response = await _apiService.PutAsync<VehicleInspectionDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.VehicleInspections.Update, id),
                inspectionDto
            );

            return Json(response);
        }

        /// <summary>
        /// Xóa kiểm tra xe thông qua API
        /// </summary>
        [HttpDelete("DeleteInspection/{id}")]
        public async Task<IActionResult> DeleteInspection(int id)
        {
            var response = await _apiService.DeleteAsync<VehicleInspectionDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.VehicleInspections.Delete, id)
            );

            return Json(response);
        }

        /// <summary>
        /// Lấy danh sách CustomerReception có thể tạo kiểm tra (trạng thái Assigned hoặc InProgress)
        /// </summary>
        [HttpGet("GetAvailableReceptions")]
        public async Task<IActionResult> GetAvailableReceptions()
        {
            var response = await _apiService.GetAsync<List<CustomerReceptionDto>>(ApiEndpoints.CustomerReceptions.GetAll);
            
            if (response.Success && response.Data != null)
            {
                // Tạm thời hiển thị tất cả reception để debug
                var availableReceptions = response.Data
                    // .Where(r => r.Status == ReceptionStatus.Assigned)
                    .Select(r => new
                    {
                        value = r.Id.ToString(),
                        text = $"{r.ReceptionNumber} - {r.CustomerName} - {r.VehiclePlate}",
                        customerId = r.CustomerId,
                        vehicleId = r.VehicleId,
                        customerName = r.CustomerName,
                        vehicleInfo = $"{r.VehicleMake} {r.VehicleModel} ({r.VehicleYear})",
                        assignedTechnicianId = r.AssignedTechnicianId
                    }).Cast<object>().ToList();
                
                return Json(availableReceptions);
            }

            return Json(new List<object>());
        }

        /// <summary>
        /// Lấy danh sách xe có sẵn cho dropdown
        /// </summary>
        [HttpGet("GetAvailableVehicles")]
        public async Task<IActionResult> GetAvailableVehicles()
        {
            // Sử dụng endpoint xe có sẵn thay vì tất cả xe
            var response = await _apiService.GetAsync<List<VehicleDto>>("/api/vehicles/available");
            
            if (response.Success && response.Data != null)
            {
                var vehicles = response.Data.Select(v => new
                {
                    value = v.Id.ToString(),
                    text = $"{v.Brand} {v.Model} - {v.LicensePlate}",
                    customerId = v.CustomerId,
                    customerName = v.Customer?.Name ?? "Không xác định"
                }).Cast<object>().ToList();
                
                return Json(vehicles);
            }

            return Json(new List<object>());
        }

        /// <summary>
        /// Lấy danh sách khách hàng có sẵn cho dropdown
        /// </summary>
        [HttpGet("GetAvailableCustomers")]
        public async Task<IActionResult> GetAvailableCustomers()
        {
            var response = await _apiService.GetAsync<List<CustomerDto>>(ApiEndpoints.Customers.GetAll);
            
            if (response.Success && response.Data != null)
            {
                var customers = response.Data.Select(c => new
                {
                    value = c.Id.ToString(),
                    text = c.Name
                }).Cast<object>().ToList();
                
                return Json(customers);
            }

            return Json(new List<object>());
        }

        /// <summary>
        /// Lấy danh sách nhân viên có sẵn cho dropdown
        /// </summary>
        [HttpGet("GetAvailableEmployees")]
        public async Task<IActionResult> GetAvailableEmployees()
        {
            var response = await _apiService.GetAsync<List<EmployeeDto>>(ApiEndpoints.Employees.GetAll);
            
            if (response.Success && response.Data != null)
            {
                var employees = response.Data.Select(e => new
                {
                    value = e.Id.ToString(),
                    text = $"{e.Name} - {e.Position ?? ""}"
                }).Cast<object>().ToList();
                
                return Json(employees);
            }

            return Json(new List<object>());
        }

        /// <summary>
        /// Hoàn thành kiểm tra xe
        /// </summary>
        [HttpPost("CompleteInspection/{id}")]
        public async Task<IActionResult> CompleteInspection(int id)
        {
            try
            {
                var response = await _apiService.PutAsync<object>($"VehicleInspections/{id}/complete", null);
                
                if (response.Success)
                {
                    return Json(new { success = true, message = "Hoàn thành kiểm tra xe thành công" });
                }
                else
                {
                    return Json(new { success = false, message = response.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi hoàn thành kiểm tra xe", error = ex.Message });
            }
        }

        private static string TranslateInspectionStatus(string status)
        {
            return status switch
            {
                "Pending" => "Chờ Xử Lý",
                "InProgress" => "Đang Kiểm Tra",
                "Completed" => "Hoàn Thành",
                "Cancelled" => "Đã Hủy",
                _ => status
            };
        }

        /// <summary>
        /// In phiếu chẩn đoán kỹ thuật
        /// </summary>
        [HttpGet("PrintInspection/{id}")]
        public async Task<IActionResult> PrintInspection(int id)
        {
            try
            {
                // ✅ FIX: API trả về ApiResponse<VehicleInspectionDto>, ApiService cũng wrap thành ApiResponse
                // Đổi sang GetAsync<VehicleInspectionDto> để tránh double nesting
                // Load inspection data
                var inspectionResponse = await _apiService.GetAsync<VehicleInspectionDto>(
                    ApiEndpoints.VehicleInspections.GetById.Replace("{0}", id.ToString()));
                
                if (!inspectionResponse.Success || inspectionResponse.Data == null)
                {
                    return NotFound("Không tìm thấy phiếu kiểm tra xe");
                }

                // Load print template
                var templateResponse = await _apiService.GetAsync<PrintTemplateDto>(
                    ApiEndpoints.PrintTemplates.GetDefault.Replace("{0}", "Inspection"));
                
                var template = templateResponse.Success ? templateResponse.Data : null;

                // ✅ FIX: response.Data giờ là VehicleInspectionDto trực tiếp (không cần .Data.Data)
                var inspection = inspectionResponse.Data;

                // Create view model
                var viewModel = new PrintInspectionViewModel
                {
                    Inspection = inspection,
                    Template = template
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi tải phiếu kiểm tra xe: {ex.Message}");
            }
        }
    }
}
