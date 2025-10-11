using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;
using GarageManagementSystem.Shared.Models;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller cho trang Setup - Insert dữ liệu demo
    /// </summary>
    [Route("Setup")]
    public class SetupController : Controller
    {
        private readonly ApiService _apiService;

        public SetupController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Trang chính Setup
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Lấy danh sách các module có thể setup
        /// </summary>
        [HttpGet("GetSetupModules")]
        public IActionResult GetSetupModules()
        {
            var modules = new List<object>
            {
                // === PHASE 1: CORE ENTITIES (Foundational Data) ===
                new { 
                    id = "customers", 
                    name = "1. Khách hàng", 
                    description = "Tạo dữ liệu khách hàng mẫu (Bước đầu tiên)", 
                    icon = "fas fa-users",
                    order = 1,
                    phase = "Core Entities",
                    phaseDescription = "Dữ liệu cơ bản cần thiết"
                },
                new { 
                    id = "employees", 
                    name = "2. Nhân viên", 
                    description = "Tạo dữ liệu nhân viên mẫu (Cần thiết cho dịch vụ)", 
                    icon = "fas fa-user-tie",
                    order = 2,
                    phase = "Core Entities",
                    phaseDescription = "Dữ liệu cơ bản cần thiết"
                },
                new { 
                    id = "suppliers", 
                    name = "3. Nhà cung cấp", 
                    description = "Tạo dữ liệu nhà cung cấp mẫu (Cần cho phụ tùng)", 
                    icon = "fas fa-truck",
                    order = 3,
                    phase = "Core Entities",
                    phaseDescription = "Dữ liệu cơ bản cần thiết"
                },

                // === PHASE 2: ASSETS & OFFERINGS ===
                new { 
                    id = "vehicles", 
                    name = "4. Xe", 
                    description = "Tạo dữ liệu xe mẫu (Personal/Insurance/Company)", 
                    icon = "fas fa-car",
                    order = 4,
                    phase = "Assets & Offerings",
                    phaseDescription = "Tài sản và dịch vụ"
                },
                new { 
                    id = "parts", 
                    name = "5. Phụ tùng", 
                    description = "Tạo dữ liệu phụ tùng mẫu (Cần nhà cung cấp)", 
                    icon = "fas fa-cogs",
                    order = 5,
                    phase = "Assets & Offerings",
                    phaseDescription = "Tài sản và dịch vụ"
                },
                new { 
                    id = "services", 
                    name = "6. Dịch vụ", 
                    description = "Tạo dữ liệu dịch vụ mẫu (Cần nhân viên)", 
                    icon = "fas fa-tools",
                    order = 6,
                    phase = "Assets & Offerings",
                    phaseDescription = "Tài sản và dịch vụ"
                },

                // === PHASE 3: OPERATIONAL DATA ===
                new { 
                    id = "inspections", 
                    name = "7. Kiểm tra xe", 
                    description = "Tạo dữ liệu kiểm tra mẫu (Cần xe + nhân viên)", 
                    icon = "fas fa-search",
                    order = 7,
                    phase = "Operational Data",
                    phaseDescription = "Dữ liệu vận hành"
                },
                new { 
                    id = "appointments", 
                    name = "8. Lịch hẹn", 
                    description = "Tạo dữ liệu lịch hẹn mẫu (Cần khách hàng + xe + nhân viên)", 
                    icon = "fas fa-calendar-alt",
                    order = 8,
                    phase = "Operational Data",
                    phaseDescription = "Dữ liệu vận hành"
                },
                new { 
                    id = "quotations", 
                    name = "9. Báo giá", 
                    description = "Tạo dữ liệu báo giá mẫu (Cần kiểm tra xe + dịch vụ + phụ tùng)", 
                    icon = "fas fa-file-invoice-dollar",
                    order = 9,
                    phase = "Operational Data",
                    phaseDescription = "Dữ liệu vận hành"
                },

                // === PHASE 4: TRANSACTIONAL DATA ===
                new { 
                    id = "orders", 
                    name = "10. Đơn hàng", 
                    description = "Tạo dữ liệu đơn hàng mẫu (Cần báo giá đã duyệt)", 
                    icon = "fas fa-clipboard-list",
                    order = 10,
                    phase = "Transactional Data",
                    phaseDescription = "Dữ liệu giao dịch"
                },
                new { 
                    id = "payments", 
                    name = "11. Thanh toán", 
                    description = "Tạo dữ liệu thanh toán mẫu (Cần đơn hàng)", 
                    icon = "fas fa-credit-card",
                    order = 11,
                    phase = "Transactional Data",
                    phaseDescription = "Dữ liệu giao dịch"
                },

                // === SPECIAL: ALL AT ONCE ===
                new { 
                    id = "all", 
                    name = "🚀 Tất cả (Auto)", 
                    description = "Tạo tất cả dữ liệu mẫu theo thứ tự tự động", 
                    icon = "fas fa-database",
                    order = 99,
                    phase = "Auto Setup",
                    phaseDescription = "Tự động tạo theo thứ tự"
                }
            };

            return Json(modules);
        }

        /// <summary>
        /// Kiểm tra trạng thái dữ liệu hiện tại
        /// </summary>
        [HttpGet("CheckDataStatus")]
        public async Task<IActionResult> CheckDataStatus()
        {
            try
            {
                var status = new
                {
                    customers = await GetEntityCount("customers"),
                    vehicles = await GetEntityCount("vehicles"),
                    employees = await GetEntityCount("employees"),
                    services = await GetEntityCount("services"),
                    parts = await GetEntityCount("parts"),
                    suppliers = await GetEntityCount("suppliers"),
                    inspections = await GetEntityCount("vehicleinspections"),
                    quotations = await GetEntityCount("servicequotations"),
                    orders = await GetEntityCount("serviceorders"),
                    payments = await GetEntityCount("paymenttransactions"),
                    appointments = await GetEntityCount("appointments")
                };

                return Json(new { success = true, data = status });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Xóa tất cả dữ liệu (Soft delete)
        /// </summary>
        [HttpPost("ClearAllData")]
        public async Task<IActionResult> ClearAllData()
        {
            try
            {
                // Gọi API để clear data
                var result = await _apiService.PostAsync<object>(ApiEndpoints.Setup.ClearAllData, null);
                
                if (result.Success)
                {
                    return Json(new { success = true, message = "Đã xóa tất cả dữ liệu thành công" });
                }
                else
                {
                    return Json(new { success = false, message = result.Message ?? "Lỗi không xác định" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #region Create Methods for Individual Modules

        /// <summary>
        /// Tạo khách hàng mới
        /// </summary>
        [HttpPost("CreateCustomers")]
        public async Task<IActionResult> CreateCustomers([FromBody] object customerData)
        {
            try
            {
                var result = await _apiService.PostAsync<object>(ApiEndpoints.Customers.Create, customerData);
                return Json(new { success = result.Success, data = result.Data, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Tạo xe mới
        /// </summary>
        [HttpPost("CreateVehicles")]
        public async Task<IActionResult> CreateVehicles([FromBody] object vehicleData)
        {
            try
            {
                var result = await _apiService.PostAsync<object>(ApiEndpoints.Vehicles.Create, vehicleData);
                return Json(new { success = result.Success, data = result.Data, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Tạo nhân viên mới
        /// </summary>
        [HttpPost("CreateEmployees")]
        public async Task<IActionResult> CreateEmployees([FromBody] object employeeData)
        {
            try
            {
                var result = await _apiService.PostAsync<object>(ApiEndpoints.Employees.Create, employeeData);
                return Json(new { success = result.Success, data = result.Data, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Tạo dịch vụ mới
        /// </summary>
        [HttpPost("CreateServices")]
        public async Task<IActionResult> CreateServices([FromBody] object serviceData)
        {
            try
            {
                var result = await _apiService.PostAsync<object>(ApiEndpoints.Services.Create, serviceData);
                return Json(new { success = result.Success, data = result.Data, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Tạo phụ tùng mới
        /// </summary>
        [HttpPost("CreateParts")]
        public async Task<IActionResult> CreateParts([FromBody] object partData)
        {
            try
            {
                var result = await _apiService.PostAsync<object>(ApiEndpoints.Parts.Create, partData);
                return Json(new { success = result.Success, data = result.Data, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Tạo nhà cung cấp mới
        /// </summary>
        [HttpPost("CreateSuppliers")]
        public async Task<IActionResult> CreateSuppliers([FromBody] object supplierData)
        {
            try
            {
                var result = await _apiService.PostAsync<object>(ApiEndpoints.Suppliers.Create, supplierData);
                return Json(new { success = result.Success, data = result.Data, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        /// <summary>
        /// Lấy số lượng entity
        /// </summary>
        private async Task<int> GetEntityCount(string entityName)
        {
            try
            {
                // Sử dụng ApiEndpoints để map entity name sang endpoint
                string endpoint = entityName switch
                {
                    "customers" => ApiEndpoints.Customers.GetAll,
                    "vehicles" => ApiEndpoints.Vehicles.GetAll,
                    "employees" => ApiEndpoints.Employees.GetAll,
                    "services" => ApiEndpoints.Services.GetAll,
                    "parts" => ApiEndpoints.Parts.GetAll,
                    "suppliers" => ApiEndpoints.Suppliers.GetAll,
                    "vehicleinspections" => ApiEndpoints.VehicleInspections.GetAll,
                    "servicequotations" => ApiEndpoints.ServiceQuotations.GetAll,
                    "serviceorders" => ApiEndpoints.ServiceOrders.GetAll,
                    "paymenttransactions" => ApiEndpoints.PaymentTransactions.GetAll,
                    "appointments" => ApiEndpoints.Appointments.GetAll,
                    _ => entityName
                };

                // API trả về ApiResponse<List<CustomerDto>>, cần unwrap đúng cách
                var response = await _apiService.GetAsync<List<object>>(endpoint);
                if (response.Success && response.Data != null)
                {
                    return response.Data.Count;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}
