using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller quản lý nhân viên với đầy đủ các thao tác CRUD thông qua API
    /// </summary>
    [Authorize]
    [Route("EmployeeManagement")]
    public class EmployeeManagementController : Controller
    {
        private readonly ApiService _apiService;

        public EmployeeManagementController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Hiển thị trang quản lý nhân viên
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Lấy danh sách tất cả nhân viên cho DataTable thông qua API với pagination
        /// </summary>
        [HttpGet("GetEmployees")]
        public async Task<IActionResult> GetEmployees(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? department = null)
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
                
                if (!string.IsNullOrEmpty(department))
                    queryParams.Add($"department={Uri.EscapeDataString(department)}");

                var queryString = string.Join("&", queryParams);
                var endpoint = $"{ApiEndpoints.Employees.GetAll}?{queryString}";

                var response = await _apiService.GetAsync<PagedResponse<EmployeeDto>>(endpoint);
                
                if (response.Success && response.Data != null)
                {
                    var employeeList = response.Data.Data.Select(e => new
                    {
                        id = e.Id,
                        name = e.Name,
                        position = e.PositionName ?? e.Position ?? "N/A",
                        department = e.DepartmentName ?? e.Department ?? "N/A",
                        positionId = e.PositionId,
                        departmentId = e.DepartmentId,
                        phone = e.Phone ?? "N/A",
                        email = e.Email ?? "N/A",
                        hireDate = e.HireDate?.ToString("yyyy-MM-dd"),
                        status = TranslateEmployeeStatus(e.Status ?? "Active")
                    }).ToList();

                    return Json(new { 
                        success = true,
                        data = employeeList,
                        totalCount = response.Data.TotalCount,
                        message = "Lấy danh sách nhân viên thành công"
                    });
                }
                else
                {
                    return Json(new { 
                        success = false,
                        error = response.ErrorMessage ?? "Lỗi khi lấy danh sách nhân viên"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false,
                    error = "Lỗi khi lấy danh sách nhân viên: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết nhân viên theo ID thông qua API
        /// </summary>
        [HttpGet("GetEmployee/{id}")]
        public async Task<IActionResult> GetEmployee(int id)
        {
            var response = await _apiService.GetAsync<ApiResponse<EmployeeDto>>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.Employees.GetById, id)
            );
            
            if (response.Success && response.Data != null)
            {
                var employee = response.Data.Data;
                var employeeData = new
                {
                    id = employee.Id,
                    name = employee.Name,
                    position = employee.PositionName ?? employee.Position ?? "N/A",
                    department = employee.DepartmentName ?? employee.Department ?? "N/A",
                    positionId = employee.PositionId,
                    departmentId = employee.DepartmentId,
                    address =employee.Address,
                    phone = employee.Phone ?? "N/A",
                    email = employee.Email ?? "N/A",
                    hireDate = employee.HireDate?.ToString("yyyy-MM-dd"),
                    salary = employee.Salary,
                    status = employee.Status,//TranslateEmployeeStatus( ?? "Active"),
                    skills = employee.Skills
                };
                
                return Json(new ApiResponse { Data = employeeData, Success=true, StatusCode=System.Net.HttpStatusCode.OK } );
            }
            
            return Json(new { success = false, error = "Employee not found" });
        }

        /// <summary>
        /// Tạo nhân viên mới thông qua API
        /// </summary>
        [HttpPost]
        [Route("CreateEmployee")]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDto employeeDto)
        {
            try
            {
                // Log input data for debugging
                Console.WriteLine($"DEBUG: CreateEmployee input: {System.Text.Json.JsonSerializer.Serialize(employeeDto)}");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    Console.WriteLine($"DEBUG: ModelState errors: {string.Join(", ", errors)}");
                    return BadRequest(new { success = false, errorMessage = "Dữ liệu không hợp lệ", errors = errors });
                }

                var response = await _apiService.PostAsync<EmployeeDto>(
                    ApiEndpoints.Employees.Create,
                    employeeDto
                );

                if (!response.Success)
                {
                    Console.WriteLine($"DEBUG: API Response Error: {response.ErrorMessage}");
                }

                return Json(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Exception in CreateEmployee: {ex.Message}");
                return BadRequest(new { success = false, errorMessage = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật thông tin nhân viên thông qua API
        /// </summary>
        [HttpPut("UpdateEmployee/{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto employeeDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errorMessage = "Dữ liệu không hợp lệ" });
            }

            if (id != employeeDto.Id)
            {
                return BadRequest(new { success = false, errorMessage = "ID không khớp" });
            }

            var response = await _apiService.PutAsync<EmployeeDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.Employees.Update, id),
                employeeDto
            );

            return Json(response);
        }

        /// <summary>
        /// Xóa nhân viên thông qua API
        /// </summary>
        [HttpDelete("DeleteEmployee/{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var response = await _apiService.DeleteAsync<EmployeeDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.Employees.Delete, id)
            );

            return Json(response);
        }

        /// <summary>
        /// Lấy danh sách nhân viên đang hoạt động cho dropdown
        /// </summary>
        [HttpGet("GetActiveEmployees")]
        public async Task<IActionResult> GetActiveEmployees()
        {
            var response = await _apiService.GetAsync<ApiResponse<List<EmployeeDto>>>(ApiEndpoints.Employees.GetActive);
            
            if (response.Success && response.Data != null && response.Data.Data != null)
            {
                var employeeList = response.Data.Data.Select(e => new
                {
                    id = e.Id,
                    text = e.Name + " - " + (e.Position ?? "")
                }).Cast<object>().ToList();

                return Json(employeeList);
            }

            return Json(new List<object>());
        }

        /// <summary>
        /// Lấy danh sách bộ phận có sẵn cho dropdown
        /// </summary>
        [HttpGet("GetAvailableDepartments")]
        public async Task<IActionResult> GetAvailableDepartments()
        {
            var response = await _apiService.GetAsync<ApiResponse<List<DepartmentDto>>>(ApiEndpoints.Departments.GetAll);
            
            if (response.Success && response.Data != null && response.Data.Data != null)
            {
                var departments = response.Data.Data.Select(d => new
                {
                    value = d.Id.ToString(),
                    text = d.Name
                }).Cast<object>().ToList();
                
                return Json(departments);
            }

            // Dự phòng nếu API gặp lỗi
            var fallbackDepartments = new[]
            {
                new { value = "1", text = "Dịch Vụ" },
                new { value = "2", text = "Phụ Tùng" },
                new { value = "3", text = "Hành Chính" },
                new { value = "4", text = "Kế Toán" },
                new { value = "5", text = "Chăm Sóc Khách Hàng" },
                new { value = "6", text = "Quản Lý" }
            };

            return Json(fallbackDepartments);
        }

        /// <summary>
        /// Lấy danh sách chức vụ có sẵn cho dropdown
        /// </summary>
        [HttpGet("GetAvailablePositions")]
        public async Task<IActionResult> GetAvailablePositions()
        {
            var response = await _apiService.GetAsync<ApiResponse<List<PositionDto>>>(ApiEndpoints.Positions.GetAll);
            
            if (response.Success && response.Data != null && response.Data.Data != null)
            {
                var positions = response.Data.Data.Select(p => new
                {
                    value = p.Id.ToString(),
                    text = p.Name
                }).Cast<object>().ToList();
                
                return Json(positions);
            }

            // Dự phòng nếu API gặp lỗi
            var fallbackPositions = new[]
            {
                new { value = "1", text = "Kỹ Thuật Viên" },
                new { value = "2", text = "Kỹ Thuật Viên Cao Cấp" },
                new { value = "3", text = "Chuyên Viên Phụ Tùng" },
                new { value = "4", text = "Tư Vấn Dịch Vụ" },
                new { value = "5", text = "Lễ Tân" },
                new { value = "6", text = "Kế Toán" },
                new { value = "7", text = "Quản Lý" },
                new { value = "8", text = "Trợ Lý" },
                new { value = "9", text = "Giám Sát" }
            };

            return Json(fallbackPositions);
        }

        private static string TranslateEmployeeStatus(string status)
        {
            return status switch
            {
                "Active" => "Đang Làm Việc",
                "Inactive" => "Ngừng Làm Việc",
                "Suspended" => "Tạm Đình Chỉ",
                "Resigned" => "Đã Nghỉ Việc",
                "Terminated" => "Đã Sa Thải",
                _ => status
            };
        }
    }
}

