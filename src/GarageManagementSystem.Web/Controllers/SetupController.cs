using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;
using GarageManagementSystem.Shared.Models;
using System.Reflection;

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
        [Route("")]
        [Route("Index")]
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
                // ✅ FIX: Gọi endpoint counts từ API thay vì gọi từng GetAll (pagination)
                // GetAll trả về PagedResponse với pageSize=10 → chỉ lấy 10 items
                // Counts endpoint trả về tổng số records với IsDeleted = false
                var response = await _apiService.GetAsync<object>(ApiEndpoints.Setup.GetCounts);
                
                if (response.Success && response.Data != null)
                {
                    // Map từ API response sang format frontend cần
                    var counts = response.Data;
                    var status = new
                    {
                        customers = GetCountValue(counts, "customerCount"),
                        vehicles = GetCountValue(counts, "vehicleCount"),
                        employees = GetCountValue(counts, "employeeCount"),
                        services = GetCountValue(counts, "serviceCount"),
                        parts = GetCountValue(counts, "partCount"),
                        suppliers = GetCountValue(counts, "supplierCount"),
                        inspections = GetCountValue(counts, "inspectionCount"),
                        quotations = GetCountValue(counts, "quotationCount"),
                        orders = GetCountValue(counts, "orderCount"),
                        payments = GetCountValue(counts, "paymentCount"),
                        appointments = GetCountValue(counts, "appointmentCount")
                    };

                    return Json(new { success = true, data = status });
                }

                return Json(new { success = false, message = "Không thể lấy số lượng dữ liệu" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Helper method để lấy giá trị count từ dynamic object hoặc JsonElement
        /// </summary>
        private int GetCountValue(object counts, string propertyName)
        {
            try
            {
                // ✅ FIX: Handle JsonElement (System.Text.Json deserialize object thành JsonElement)
                if (counts is System.Text.Json.JsonElement jsonElement)
                {
                    if (jsonElement.TryGetProperty(propertyName, out var property))
                    {
                        return property.GetInt32();
                    }
                }
                else
                {
                    // Handle regular object với reflection
                    var property = counts.GetType().GetProperty(propertyName);
                    if (property != null)
                    {
                        var value = property.GetValue(counts);
                        return value != null ? Convert.ToInt32(value) : 0;
                    }
                }
            }
            catch { }
            return 0;
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

        [HttpPost("ClearPhase1")]
        public async Task<IActionResult> ClearPhase1()
        {
            try
            {
                var result = await _apiService.PostAsync<object>(ApiEndpoints.Setup.ClearPhase1, null);
                return Json(new { success = result.Success, message = result.Message, data = result.Data });
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("ClearPhase2")]
        public async Task<IActionResult> ClearPhase2()
        {
            try
            {
                var result = await _apiService.PostAsync<object>(ApiEndpoints.Setup.ClearPhase2, null);
                return Json(new { success = result.Success, message = result.Message, data = result.Data });
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("ClearPhase3")]
        public async Task<IActionResult> ClearPhase3()
        {
            try
            {
                var result = await _apiService.PostAsync<object>(ApiEndpoints.Setup.ClearPhase3, null);
                return Json(new { success = result.Success, message = result.Message, data = result.Data });
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Tạo demo data cho Giai đoạn 1: Tiếp nhận & Báo giá
        /// </summary>
        [HttpPost("CreatePhase1")]
        public async Task<IActionResult> CreatePhase1()
        {
            try
            {
                var result = await _apiService.PostAsync<object>(ApiEndpoints.Setup.CreatePhase1, null);
                return Json(new { success = result.Success, message = result.Message, data = result.Data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Tạo demo data cho Giai đoạn 2: Sửa chữa & Quản lý xuất kho
        /// </summary>
        [HttpPost("CreatePhase2")]
        public async Task<IActionResult> CreatePhase2()
        {
            try
            {
                var result = await _apiService.PostAsync<object>(ApiEndpoints.Setup.CreatePhase2, null);
                return Json(new { success = result.Success, message = result.Message, data = result.Data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Tạo demo data cho Giai đoạn 3: Quyết toán & Chăm sóc hậu mãi
        /// </summary>
        [HttpPost("CreatePhase3")]
        public async Task<IActionResult> CreatePhase3()
        {
            try
            {
                var result = await _apiService.PostAsync<object>(ApiEndpoints.Setup.CreatePhase3, null);
                return Json(new { success = result.Success, message = result.Message, data = result.Data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Tạo demo data cho Giai đoạn 4: Chuẩn hóa quản lý phụ tùng & Procurement
        /// </summary>
        [HttpPost("CreatePhase4")]
        public async Task<IActionResult> CreatePhase4()
        {
            try
            {
                var result = await _apiService.PostAsync<object>(ApiEndpoints.Setup.CreatePhase4, null);
                return Json(new { success = result.Success, message = result.Message, data = result.Data });
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
            /// Tạo dữ liệu demo cho một module cụ thể
            /// </summary>
            [HttpPost("CreateModule")]
            public async Task<IActionResult> CreateModule(string moduleId)
            {
                try
                {
                    var result = await _apiService.PostAsync<object>(
                        string.Format(ApiEndpoints.Setup.CreateDemoData, moduleId), 
                        null);
                    
                    return Json(new { success = result.Success, message = result.Message, data = result.Data });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
            }

            // ✅ REMOVED: GetEntityCount method - không còn dùng nữa
            // Thay vào đó, CheckDataStatus gọi trực tiếp endpoint /api/Setup/counts
            // để lấy tổng số records với IsDeleted = false (không bị giới hạn bởi pagination)
    }
}
