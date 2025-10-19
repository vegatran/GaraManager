using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using GarageManagementSystem.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller quản lý tiếp đón khách hàng với đầy đủ các thao tác CRUD thông qua API
    /// </summary>
    [Authorize]
    [Route("CustomerReception")]
    public class CustomerReceptionController : Controller
    {
        private readonly ApiService _apiService;

        public CustomerReceptionController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Hiển thị trang quản lý tiếp đón khách hàng
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Lấy danh sách phiếu tiếp đón với pagination cho DataTable thông qua API
        /// </summary>
        [HttpGet("GetReceptions")]
        public async Task<IActionResult> GetReceptions(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? status = null)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (!string.IsNullOrEmpty(searchTerm))
                    queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");

                if (!string.IsNullOrEmpty(status))
                    queryParams.Add($"status={Uri.EscapeDataString(status)}");

                var queryString = string.Join("&", queryParams);
                var endpoint = $"{ApiEndpoints.CustomerReceptions.GetAll}?{queryString}";

                var response = await _apiService.GetAsync<PagedResponse<CustomerReceptionDto>>(endpoint);

                if (response.Success && response.Data != null)
                {
                    var receptionList = response.Data.Data.Select(r => new
                    {
                        id = r.Id,
                        receptionNumber = r.ReceptionNumber,
                        customerName = r.CustomerName ?? "N/A",
                        vehiclePlate = r.VehiclePlate ?? "N/A",
                        vehicleInfo = !string.IsNullOrEmpty(r.VehicleMake) && !string.IsNullOrEmpty(r.VehicleModel)
                            ? $"{r.VehicleMake} {r.VehicleModel} ({r.VehicleYear})"
                            : "N/A",
                        receptionDate = r.ReceptionDate.ToString("yyyy-MM-dd HH:mm"),
                        technicianName = r.AssignedTechnician?.Name ?? "Chưa phân công",
                        status = TranslateReceptionStatus(r.Status),
                        priority = TranslatePriority(r.Priority),
                        serviceType = TranslateServiceType(r.ServiceType)
                    }).ToList();

                    return Json(new
                    {
                        success = true,
                        data = receptionList,
                        totalCount = response.Data.TotalCount,
                        message = "Lấy danh sách phiếu tiếp đón thành công"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        error = response.ErrorMessage ?? "Lỗi khi lấy danh sách phiếu tiếp đón"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = "Lỗi khi lấy danh sách phiếu tiếp đón: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết phiếu tiếp đón theo ID thông qua API
        /// </summary>
        [HttpGet("GetReception/{id}")]
        public async Task<IActionResult> GetReception(int id)
        {
            var response = await _apiService.GetAsync<ApiResponse<CustomerReceptionDto>>($"CustomerReceptions/{id}");
            
            if (response.Success && response.Data != null)
            {
                return Json(new { 
                    success = true,
                    data = response.Data.Data,
                    message = "Lấy thông tin phiếu tiếp đón thành công"
                });
            }
            
            return Json(new { 
                success = false,
                data = (object)null,
                message = "Không tìm thấy phiếu tiếp đón"
            });
        }

        /// <summary>
        /// Tạo phiếu tiếp đón mới thông qua API
        /// </summary>
        [HttpPost]
        [Route("CreateReception")]
        public async Task<IActionResult> CreateReception([FromBody] CreateCustomerReceptionDto receptionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errorMessage = "Dữ liệu không hợp lệ" });
            }

            var response = await _apiService.PostAsync<CustomerReceptionDto>(
                "CustomerReceptions",
                receptionDto
            );

            return Json(response);
        }

        /// <summary>
        /// Cập nhật thông tin phiếu tiếp đón thông qua API
        /// </summary>
        [HttpPut("UpdateReception/{id}")]
        public async Task<IActionResult> UpdateReception(int id, [FromBody] UpdateCustomerReceptionDto receptionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errorMessage = "Dữ liệu không hợp lệ" });
            }

            // Note: UpdateCustomerReceptionDto doesn't have Id property, so we use the route parameter

            var response = await _apiService.PutAsync<CustomerReceptionDto>(
                $"CustomerReceptions/{id}",
                receptionDto
            );

            return Json(response);
        }

        /// <summary>
        /// Xóa phiếu tiếp đón thông qua API
        /// </summary>
        [HttpDelete("DeleteReception/{id}")]
        public async Task<IActionResult> DeleteReception(int id)
        {
            var response = await _apiService.DeleteAsync<CustomerReceptionDto>($"CustomerReceptions/{id}");

            return Json(response);
        }

        /// <summary>
        /// Phân công kỹ thuật viên cho phiếu tiếp đón
        /// </summary>
        [HttpPut("AssignTechnician/{id}")]
        public async Task<IActionResult> AssignTechnician(int id, [FromQuery] int technicianId)
        {
            var response = await _apiService.PutAsync<bool>(
                $"CustomerReceptions/{id}/assign",
                new { TechnicianId = technicianId }
            );

            return Json(response);
        }

        /// <summary>
        /// Lấy danh sách khách hàng có sẵn cho dropdown
        /// </summary>
        [HttpGet("GetAvailableCustomers")]
        public async Task<IActionResult> GetAvailableCustomers()
        {
            var response = await _apiService.GetAsync< ApiResponse<List<CustomerDto>>>(ApiEndpoints.Customers.GetAll);
            
            if (response.Success && response.Data != null)
            {
                var customers = response.Data.Data.Select(c => new
                {
                    value = c.Id.ToString(),
                    text = c.Name + " - " + (c.Phone ?? "")
                }).Cast<object>().ToList();
                
                return Json(customers);
            }

            return Json(new List<object>());
        }

        /// <summary>
        /// Lấy danh sách xe có sẵn cho dropdown
        /// </summary>
        [HttpGet("GetAvailableVehicles")]
        public async Task<IActionResult> GetAvailableVehicles()
        {
            var response = await _apiService.GetAsync<ApiResponse<List<VehicleDto>>>(ApiEndpoints.Vehicles.GetAll);
            
            if (response.Success && response.Data != null)
            {
                var vehicles = response.Data.Data.Select(v => new
                {
                    value = v.Id.ToString(),
                    text = $"{v.LicensePlate} - {v.Brand} {v.Model}",
                    customerId = v.CustomerId
                }).Cast<object>().ToList();
                
                return Json(vehicles);
            }

            return Json(new List<object>());
        }

        /// <summary>
        /// Lấy danh sách kỹ thuật viên có sẵn cho dropdown
        /// </summary>
        [HttpGet("GetAvailableTechnicians")]
        public async Task<IActionResult> GetAvailableTechnicians()
        {
            var response = await _apiService.GetAsync<ApiResponse<List<EmployeeDto>>>(ApiEndpoints.Employees.GetActive);
            
            if (response.Success && response.Data != null)
            {
                // Debug: Log all employees to see what we have
                var allEmployees = response.Data.Data.ToList();
                
                // More flexible filter - include any employee that could be a technician
                var technicians = allEmployees
                    .Where(e => e.Position != null && (
                        e.Position.Contains("Kỹ Thuật") || 
                        e.Position.Contains("Technician") ||
                        e.Position.Contains("Kỹ thuật") ||
                        e.Position.Contains("kỹ thuật") ||
                        e.Position.Contains("Thợ") ||
                        e.Position.Contains("Mechanic") ||
                        e.Position.Contains("Sửa chữa") ||
                        e.Position.Contains("Repair")
                    ))
                    .Select(e => new
                    {
                        value = e.Id.ToString(),
                        text = e.Name + " - " + (e.Position ?? "")
                    }).Cast<object>().ToList();
                
                // If no technicians found, return all active employees for debugging
                if (technicians.Count == 0)
                {
                    technicians = allEmployees
                        .Select(e => new
                        {
                            value = e.Id.ToString(),
                            text = e.Name + " - " + (e.Position ?? "Không có chức vụ")
                        }).Cast<object>().ToList();
                }
                
                return Json(technicians);
            }

            return Json(new List<object>());
        }

        private static string TranslateReceptionStatus(ReceptionStatus status)
        {
            return status switch
            {
                ReceptionStatus.Pending => "Chờ Kiểm Tra",
                ReceptionStatus.Assigned => "Đã Phân Công",
                ReceptionStatus.InProgress => "Đang Kiểm Tra",
                ReceptionStatus.Completed => "Đã Hoàn Thành",
                ReceptionStatus.Cancelled => "Đã Hủy",
                _ => status.ToString()
            };
        }

        private static string TranslatePriority(string priority)
        {
            return priority switch
            {
                "Low" => "Thấp",
                "Normal" => "Bình Thường",
                "High" => "Cao",
                "Urgent" => "Khẩn Cấp",
                _ => priority
            };
        }

        private static string TranslateServiceType(string serviceType)
        {
            return serviceType switch
            {
                "Repair" => "Sửa Chữa",
                "Maintenance" => "Bảo Dưỡng",
                "Warranty" => "Bảo Hành",
                "Bodywork" => "Sửa Chữa Thân Xe",
                "Other" => "Khác",
                _ => serviceType
            };
        }
    }
}
