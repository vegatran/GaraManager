using GarageManagementSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;
using GarageManagementSystem.Shared.Models;

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
            var response = await _apiService.GetAsync<ApiResponse<List<VehicleDto>>>(ApiEndpoints.Vehicles.GetAll);
            
            if (response.Success)
            {
                var vehicleList = new List<object>();
                
                if (response.Data != null)
                {
                    vehicleList = response.Data.Data.Select(v => new
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
        public async Task<IActionResult> Create([FromBody] CreateVehicleDto model)
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
            var response = await _apiService.GetAsync<ApiResponse<List<CustomerDto>>>(ApiEndpoints.Customers.GetAll);
            
            if (response.Success && response.Data != null)
            {
                var customerList = response.Data.Data.Select(c => new
                {
                    id = c.Id,
                    name = c.Name
                }).Cast<object>().ToList();

                return Json(customerList);
            }

            return Json(new List<object>());
        }

        #region Vehicle Inspection Methods

        /// <summary>
        /// Lấy danh sách kiểm tra xe cho DataTable thông qua API
        /// </summary>
        [HttpGet("GetInspections")]
        public async Task<IActionResult> GetInspections(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? status = null,
            int? customerId = null)
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
                if (!string.IsNullOrEmpty(status))
                    queryParams.Add($"status={Uri.EscapeDataString(status)}");
                if (customerId.HasValue)
                    queryParams.Add($"customerId={customerId.Value}");

                var endpoint = ApiEndpoints.VehicleInspections.GetAll + "?" + string.Join("&", queryParams);
                var response = await _apiService.GetAsync<PagedResponse<VehicleInspectionDto>>(endpoint);
                
                if (response.Success && response.Data != null)
                {
                    var inspectionList = response.Data.Data.Select(i => new
                    {
                        id = i.Id,
                        inspectionNumber = i.InspectionNumber,
                        vehiclePlate = i.Vehicle?.LicensePlate ?? "N/A",
                        customerName = i.Customer?.Name ?? "N/A",
                        inspectionDate = i.InspectionDate.ToString("yyyy-MM-dd HH:mm"),
                        inspectionType = i.InspectionType,
                        status = i.Status,
                        inspectorName = i.Inspector?.Name ?? "N/A",
                        completedDate = i.CompletedDate?.ToString("yyyy-MM-dd") ?? "N/A",
                        currentMileage = i.CurrentMileage?.ToString() ?? "N/A",
                        generalCondition = i.GeneralCondition ?? "N/A"
                    }).Cast<object>().ToList();

                    return Json(new { 
                        success = true,
                        data = inspectionList,
                        totalCount = response.Data.TotalCount,
                        recordsTotal = response.Data.TotalCount,
                        recordsFiltered = response.Data.TotalCount
                    });
                }

                return Json(new { 
                    success = false, 
                    error = response.ErrorMessage ?? "Lỗi khi lấy danh sách kiểm tra xe" 
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    error = "Lỗi khi lấy danh sách kiểm tra xe: " + ex.Message 
                });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết kiểm tra xe theo ID thông qua API
        /// </summary>
        [HttpGet("GetInspection/{id}")]
        public async Task<IActionResult> GetInspection(int id)
        {
            try
            {
                var response = await _apiService.GetAsync<ApiResponse<VehicleInspectionDto>>(ApiEndpoints.Builder.WithId(ApiEndpoints.VehicleInspections.GetById, id));
                
                if (response.Success && response.Data != null)
                {
                    var inspection = response.Data.Data;
                    
                    // Format dates for HTML input fields
                    var inspectionData = new
                    {
                        id = inspection.Id,
                        inspectionNumber = inspection.InspectionNumber,
                        vehicleId = inspection.VehicleId,
                        customerId = inspection.CustomerId,
                        inspectorId = inspection.InspectorId,
                        inspectionDate = inspection.InspectionDate.ToString("yyyy-MM-ddTHH:mm"),
                        inspectionType = inspection.InspectionType,
                        currentMileage = inspection.CurrentMileage,
                        fuelLevel = inspection.FuelLevel,
                        generalCondition = inspection.GeneralCondition,
                        exteriorCondition = inspection.ExteriorCondition,
                        interiorCondition = inspection.InteriorCondition,
                        engineCondition = inspection.EngineCondition,
                        brakeCondition = inspection.BrakeCondition,
                        suspensionCondition = inspection.SuspensionCondition,
                        tireCondition = inspection.TireCondition,
                        electricalCondition = inspection.ElectricalCondition,
                        lightsCondition = inspection.LightsCondition,
                        customerComplaints = inspection.CustomerComplaints,
                        recommendations = inspection.Recommendations,
                        technicianNotes = inspection.TechnicianNotes,
                        status = inspection.Status,
                        completedDate = inspection.CompletedDate?.ToString("yyyy-MM-dd"),
                        quotationId = inspection.QuotationId,
                        // Navigation properties for View modal
                        vehicle = inspection.Vehicle,
                        customer = inspection.Customer,
                        inspector = inspection.Inspector
                    };

                    return Json(new { success = true, data = inspectionData });
                }

                return Json(new { success = false, error = response.ErrorMessage ?? "Không tìm thấy thông tin kiểm tra xe" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi lấy thông tin kiểm tra xe: " + ex.Message });
            }
        }

        /// <summary>
        /// Tạo mới kiểm tra xe
        /// </summary>
        [HttpPost("CreateInspection")]
        public async Task<IActionResult> CreateInspection([FromBody] CreateVehicleInspectionDto inspectionDto)
        {
            try
            {
                var response = await _apiService.PostAsync<VehicleInspectionDto>(ApiEndpoints.VehicleInspections.Create, inspectionDto);
                
                if (response.Success)
                {
                    return Json(new { success = true, data = response.Data, message = "Tạo kiểm tra xe thành công" });
                }

                return Json(new { success = false, error = response.ErrorMessage ?? "Lỗi khi tạo kiểm tra xe" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi tạo kiểm tra xe: " + ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật kiểm tra xe
        /// </summary>
        [HttpPut("UpdateInspection/{id}")]
        public async Task<IActionResult> UpdateInspection(int id, [FromBody] UpdateVehicleInspectionDto inspectionDto)
        {
            try
            {
                var response = await _apiService.PutAsync<VehicleInspectionDto>(ApiEndpoints.Builder.WithId(ApiEndpoints.VehicleInspections.Update, id), inspectionDto);
                
                if (response.Success)
                {
                    return Json(new { success = true, data = response.Data, message = "Cập nhật kiểm tra xe thành công" });
                }

                return Json(new { success = false, error = response.ErrorMessage ?? "Lỗi khi cập nhật kiểm tra xe" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi cập nhật kiểm tra xe: " + ex.Message });
            }
        }

        /// <summary>
        /// Xóa kiểm tra xe
        /// </summary>
        [HttpDelete("DeleteInspection/{id}")]
        public async Task<IActionResult> DeleteInspection(int id)
        {
            try
            {
                var response = await _apiService.DeleteAsync<object>(ApiEndpoints.Builder.WithId(ApiEndpoints.VehicleInspections.Delete, id));
                
                if (response.Success)
                {
                    return Json(new { success = true, message = "Xóa kiểm tra xe thành công" });
                }

                return Json(new { success = false, error = response.ErrorMessage ?? "Lỗi khi xóa kiểm tra xe" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi xóa kiểm tra xe: " + ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách kỹ thuật viên kiểm tra cho dropdown
        /// </summary>
        [HttpGet("GetAvailableInspectors")]
        public async Task<IActionResult> GetAvailableInspectors()
        {
            try
            {
                var response = await _apiService.GetAsync<ApiResponse<List<EmployeeDto>>>(ApiEndpoints.Employees.GetActive);
                
                if (response.Success && response.Data != null)
                {
                    var allEmployees = response.Data.Data.ToList();
                    
                    var inspectors = allEmployees
                        .Where(e => e.Position != null && (
                            e.Position.Contains("Kỹ Thuật") || 
                            e.Position.Contains("Technician") ||
                            e.Position.Contains("Kỹ thuật") ||
                            e.Position.Contains("kỹ thuật") ||
                            e.Position.Contains("Thợ") ||
                            e.Position.Contains("Mechanic") ||
                            e.Position.Contains("Sửa chữa") ||
                            e.Position.Contains("Repair") ||
                            e.Position.Contains("Kiểm tra") ||
                            e.Position.Contains("Inspection")
                        ))
                        .Select(e => new
                        {
                            value = e.Id.ToString(),
                            text = e.Name + " - " + (e.Position ?? "")
                        }).Cast<object>().ToList();
                    
                    if (inspectors.Count == 0)
                    {
                        inspectors = allEmployees
                            .Select(e => new
                            {
                                value = e.Id.ToString(),
                                text = e.Name + " - " + (e.Position ?? "")
                            }).Cast<object>().ToList();
                    }
                    
                    return Json(inspectors);
                }

                return Json(new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }

        /// <summary>
        /// Lấy danh sách khách hàng cho dropdown (cho Vehicle Inspection)
        /// </summary>
        [HttpGet("GetAvailableCustomers")]
        public async Task<IActionResult> GetAvailableCustomers()
        {
            try
            {
                var response = await _apiService.GetAsync<List<CustomerDto>>(ApiEndpoints.Customers.GetAllForDropdown);
                
                if (response.Success && response.Data != null)
                {
                    var customers = response.Data.Select(c => new
                    {
                        value = c.Id.ToString(),
                        text = c.Name + " - " + (c.Phone ?? "N/A")
                    }).Cast<object>().ToList();
                    
                    return Json(customers);
                }

                return Json(new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }

        /// <summary>
        /// Lấy danh sách xe cho dropdown (cho Vehicle Inspection)
        /// </summary>
        [HttpGet("GetAvailableVehicles")]
        public async Task<IActionResult> GetAvailableVehicles()
        {
            try
            {
                var response = await _apiService.GetAsync<List<VehicleDto>>(ApiEndpoints.Vehicles.GetAllForDropdown);
                
                if (response.Success && response.Data != null)
                {
                    var vehicles = response.Data.Select(v => new
                    {
                        value = v.Id.ToString(),
                        text = v.LicensePlate + " - " + v.Brand + " " + v.Model,
                        customerId = v.CustomerId
                    }).Cast<object>().ToList();
                    
                    return Json(vehicles);
                }

                return Json(new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }

        /// <summary>
        /// Lấy danh sách nhân viên cho dropdown (cho Vehicle Inspection)
        /// </summary>
        [HttpGet("GetAvailableEmployees")]
        public async Task<IActionResult> GetAvailableEmployees()
        {
            try
            {
                var response = await _apiService.GetAsync<ApiResponse<List<EmployeeDto>>>(ApiEndpoints.Employees.GetActive);
                
                if (response.Success && response.Data != null)
                {
                    var employees = response.Data.Data.Select(e => new
                    {
                        value = e.Id.ToString(),
                        text = e.Name + " - " + (e.Position ?? "N/A")
                    }).Cast<object>().ToList();
                    
                    return Json(employees);
                }

                return Json(new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }

        /// <summary>
        /// Lấy danh sách tiếp đón khách hàng cho dropdown (cho Vehicle Inspection)
        /// </summary>
        [HttpGet("GetAvailableReceptions")]
        public async Task<IActionResult> GetAvailableReceptions()
        {
            try
            {
                // Sử dụng endpoint riêng cho dropdown (chỉ lấy phiếu tiếp đón có thể tạo kiểm tra)
                var response = await _apiService.GetAsync<List<CustomerReceptionDto>>(ApiEndpoints.CustomerReceptions.GetInspectionEligibleForDropdown);
                
                if (response.Success && response.Data != null)
                {
                    var receptions = response.Data.Select(r => new
                    {
                        value = r.Id.ToString(),
                        text = r.ReceptionNumber + " - " + (r.Customer?.Name ?? "N/A") + " (" + r.Vehicle?.LicensePlate + ")",
                        customerId = r.CustomerId.ToString(),
                        vehicleId = r.VehicleId.ToString(),
                        customerName = r.Customer?.Name ?? "",
                        vehicleInfo = r.Vehicle?.LicensePlate + " - " + r.Vehicle?.Brand + " " + r.Vehicle?.Model ?? "",
                        assignedTechnicianId = r.AssignedTechnicianId?.ToString() ?? ""
                    }).Cast<object>().ToList();
                    
                    return Json(receptions);
                }

                return Json(new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }

        #endregion
    }
}

