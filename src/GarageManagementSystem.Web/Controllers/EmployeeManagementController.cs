using GarageManagementSystem.Shared.DTOs;
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
        /// Lấy danh sách tất cả nhân viên cho DataTable thông qua API
        /// </summary>
        [HttpGet("GetEmployees")]
        public async Task<IActionResult> GetEmployees()
        {
            var response = await _apiService.GetAsync<List<EmployeeDto>>(ApiEndpoints.Employees.GetAll);
            
            if (response.Success)
            {
                var employeeList = new List<object>();
                
                if (response.Data != null)
                {
                    employeeList = response.Data.Select(e => new
                    {
                        id = e.Id,
                        name = e.Name,
                        position = e.PositionName ?? e.Position ?? "N/A",
                        department = e.DepartmentName ?? e.Department ?? "N/A",
                        positionId = e.PositionId,
                        departmentId = e.DepartmentId,
                        phone = e.Phone ?? "N/A",
                        email = e.Email ?? "N/A",
                        hireDate = e.HireDate?.ToString("dd/MM/yyyy") ?? "N/A",
                        status = e.Status ?? "Active"
                    }).Cast<object>().ToList();
                }

                return Json(new { 
                    success = true,
                    data = employeeList,
                    message = "Lấy danh sách nhân viên thành công"
                });
            }
            else
            {
                return Json(new { error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết nhân viên theo ID thông qua API
        /// </summary>
        [HttpGet("GetEmployee/{id}")]
        public async Task<IActionResult> GetEmployee(int id)
        {
            var response = await _apiService.GetAsync<EmployeeDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.Employees.GetById, id)
            );
            
            return Json(response);
        }

        /// <summary>
        /// Tạo nhân viên mới thông qua API
        /// </summary>
        [HttpPost]
        [Route("CreateEmployee")]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDto employeeDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errorMessage = "Dữ liệu không hợp lệ" });
            }

            var response = await _apiService.PostAsync<EmployeeDto>(
                ApiEndpoints.Employees.Create,
                employeeDto
            );

            return Json(response);
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
            var response = await _apiService.GetAsync<List<EmployeeDto>>(ApiEndpoints.Employees.GetActive);
            
            if (response.Success && response.Data != null)
            {
                var employeeList = response.Data.Select(e => new
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
            var response = await _apiService.GetAsync<List<DepartmentDto>>(ApiEndpoints.Departments.GetAll);
            
            if (response.Success && response.Data != null)
            {
                return Json(response.Data);
            }

            // Dự phòng nếu API gặp lỗi
            var fallbackDepartments = new[]
            {
                new { value = "Service", text = "Dịch Vụ" },
                new { value = "Parts", text = "Phụ Tùng" },
                new { value = "Administration", text = "Hành Chính" },
                new { value = "Accounting", text = "Kế Toán" },
                new { value = "CustomerService", text = "Chăm Sóc Khách Hàng" },
                new { value = "Management", text = "Quản Lý" }
            };

            return Json(fallbackDepartments);
        }

        /// <summary>
        /// Lấy danh sách chức vụ có sẵn cho dropdown
        /// </summary>
        [HttpGet("GetAvailablePositions")]
        public async Task<IActionResult> GetAvailablePositions()
        {
            var response = await _apiService.GetAsync<List<PositionDto>>(ApiEndpoints.Positions.GetAll);
            
            if (response.Success && response.Data != null)
            {
                return Json(response.Data);
            }

            // Dự phòng nếu API gặp lỗi
            var fallbackPositions = new[]
            {
                new { value = "Technician", text = "Kỹ Thuật Viên" },
                new { value = "SeniorTechnician", text = "Kỹ Thuật Viên Cao Cấp" },
                new { value = "PartsSpecialist", text = "Chuyên Viên Phụ Tùng" },
                new { value = "ServiceAdvisor", text = "Tư Vấn Dịch Vụ" },
                new { value = "Receptionist", text = "Lễ Tân" },
                new { value = "Accountant", text = "Kế Toán" },
                new { value = "Manager", text = "Quản Lý" },
                new { value = "Assistant", text = "Trợ Lý" },
                new { value = "Supervisor", text = "Giám Sát" }
            };

            return Json(fallbackPositions);
        }

    }
}

