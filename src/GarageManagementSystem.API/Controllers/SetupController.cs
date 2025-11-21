using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace GarageManagementSystem.API.Controllers
{
    /// <summary>
    /// Setup controller để tạo dữ liệu demo
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize(Policy = "ApiScope")] // Tạm thời bỏ để test
    public class SetupController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly GarageDbContext _context;

        public SetupController(IUnitOfWork unitOfWork, GarageDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        /// <summary>
        /// Lấy số lượng dữ liệu hiện có
        /// </summary>
        [HttpGet("counts")]
        public async Task<ActionResult<ApiResponse<object>>> GetCounts()
        {
            try
            {
                var counts = new
                {
                    customerCount = await _unitOfWork.Customers.CountAsync(),
                    vehicleCount = await _unitOfWork.Vehicles.CountAsync(),
                    employeeCount = await _unitOfWork.Employees.CountAsync(),
                    serviceCount = await _unitOfWork.Services.CountAsync(),
                    partCount = await _unitOfWork.Parts.CountAsync(),
                    supplierCount = await _unitOfWork.Suppliers.CountAsync(),
                    inspectionCount = await _unitOfWork.VehicleInspections.CountAsync(),
                    quotationCount = await _unitOfWork.ServiceQuotations.CountAsync(),
                    orderCount = await _unitOfWork.ServiceOrders.CountAsync(),
                    paymentCount = await _unitOfWork.PaymentTransactions.CountAsync(),
                    appointmentCount = await _unitOfWork.Appointments.CountAsync(),
                    warehouseCount = await _unitOfWork.Warehouses.CountAsync(),
                    warehouseZoneCount = await _unitOfWork.WarehouseZones.CountAsync(),
                    warehouseBinCount = await _unitOfWork.WarehouseBins.CountAsync(),
                    inventoryCheckCount = await _unitOfWork.InventoryChecks.CountAsync(),
                    inventoryAdjustmentCount = await _unitOfWork.InventoryAdjustments.CountAsync()
                };

                return Ok(ApiResponse<object>.SuccessResult(counts));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult("Lỗi khi lấy số lượng dữ liệu", ex.Message));
            }
        }

        /// <summary>
        /// Tạo dữ liệu demo cho module cụ thể
        /// </summary>
        [HttpPost("create/{module}")]
        public async Task<ActionResult<ApiResponse<object>>> CreateDemoData(string module)
        {
            try
            {
                var result = module.ToLower() switch
                {
                    "customers" => await CreateCustomersAsync(),
                    "vehicles" => await CreateVehiclesAsync(),
                    "employees" => await CreateEmployeesAsync(),
                    "services" => await CreateServicesAsync(),
                    "parts" => await CreatePartsAsync(),
                    "suppliers" => await CreateSuppliersAsync(),
                    "warehouses" => await CreateWarehousesAsync(),
                    "inventorychecks" => await CreateInventoryChecksAsync(),
                    "inventoryadjustments" => await CreateInventoryAdjustmentsAsync(),
                    "inspections" => await CreateInspectionsAsync(),
                    "quotations" => await CreateQuotationsAsync(),
                    "orders" => await CreateServiceOrdersAsync(),
                    "payments" => await CreatePaymentsAsync(),
                    "appointments" => await CreateAppointmentsAsync(),
                    _ => new { success = false, message = $"Module {module} không được hỗ trợ" }
                };

                return Ok(ApiResponse<object>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Lỗi khi tạo dữ liệu demo cho {module}", ex.Message));
            }
        }

        /// <summary>
        /// Xóa tất cả dữ liệu demo
        /// </summary>
        [HttpPost("clear-all")]
        public async Task<ActionResult<ApiResponse<object>>> ClearAllData()
        {
            try
            {
                var summary = await ClearAllDataInternalAsync();

                return Ok(ApiResponse<object>.SuccessResult(new
                {
                    message = "Đã xóa tất cả dữ liệu demo thành công",
                    cleared = summary
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult("Lỗi khi xóa dữ liệu demo", ex.Message));
            }
        }

        [HttpPost("clear-phase-1")]
        public async Task<ActionResult<ApiResponse<object>>> ClearPhase1DemoData()
        {
            try
            {
                var summary = await ClearPhase1Async();
                return Ok(ApiResponse<object>.SuccessResult(new
                {
                    message = "Đã xóa dữ liệu Phase 1",
                    phase = "Phase 1",
                    cleared = summary
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult("Lỗi khi xóa dữ liệu Phase 1", ex.Message));
            }
        }

        [HttpPost("clear-phase-2")]
        public async Task<ActionResult<ApiResponse<object>>> ClearPhase2DemoData()
        {
            try
            {
                var summary = await ClearPhase2Async();
                return Ok(ApiResponse<object>.SuccessResult(new
                {
                    message = "Đã xóa dữ liệu Phase 2",
                    phase = "Phase 2",
                    cleared = summary
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult("Lỗi khi xóa dữ liệu Phase 2", ex.Message));
            }
        }

        [HttpPost("clear-phase-3")]
        public async Task<ActionResult<ApiResponse<object>>> ClearPhase3DemoData()
        {
            try
            {
                var summary = await ClearPhase3Async();
                return Ok(ApiResponse<object>.SuccessResult(new
                {
                    message = "Đã xóa dữ liệu Phase 3",
                    phase = "Phase 3",
                    cleared = summary
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult("Lỗi khi xóa dữ liệu Phase 3", ex.Message));
            }
        }

        /// <summary>
        /// Xóa toàn bộ dữ liệu hiện có và tạo lại dataset demo đầy đủ
        /// </summary>
        [HttpPost("create-all")]
        public async Task<ActionResult<ApiResponse<object>>> CreateAllDemoData()
        {
            List<object> cleared = new();
            var seeded = new List<object>();

            // ✅ Sử dụng transaction để đảm bảo tính toàn vẹn dữ liệu
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                cleared = await ClearAllDataInternalAsync();

                async Task RunModuleAsync(string moduleName, Func<Task<object>> moduleFactory)
                {
                    var (success, result, message) = await ExecuteSeedAsync(moduleFactory);
                    seeded.Add(new { module = moduleName, success, message, result });

                    if (!success)
                    {
                        throw new InvalidOperationException(
                            string.IsNullOrWhiteSpace(message)
                                ? $"Tạo dữ liệu cho module {moduleName} thất bại."
                                : message);
                    }
                }

                await RunModuleAsync("customers", CreateCustomersAsync);
                await RunModuleAsync("vehicles", CreateVehiclesAsync);
                await RunModuleAsync("employees", CreateEmployeesAsync);
                await RunModuleAsync("services", CreateServicesAsync);
                await RunModuleAsync("warehouses", CreateWarehousesAsync);
                await RunModuleAsync("parts", CreatePartsAsync);
                await RunModuleAsync("suppliers", CreateSuppliersAsync);
                await RunModuleAsync("laborCategories", CreateLaborCategoriesAsync);
                await RunModuleAsync("laborItems", CreateLaborItemsAsync);
                await RunModuleAsync("inventorychecks", CreateInventoryChecksAsync);
                await RunModuleAsync("inventoryadjustments", CreateInventoryAdjustmentsAsync);
                await RunModuleAsync("inspections", CreateInspectionsAsync);
                await RunModuleAsync("quotations", CreateQuotationsAsync);
                await RunModuleAsync("orders", CreateServiceOrdersAsync);
                await RunModuleAsync("serviceOrderLabors", CreateServiceOrderLaborsAsync);
                await RunModuleAsync("materialRequests", CreateMaterialRequestsAsync);
                await RunModuleAsync("payments", CreatePaymentsAsync);
                await RunModuleAsync("serviceFeeTypes", CreateServiceFeeTypesAsync);
                await RunModuleAsync("serviceOrderFees", CreateServiceOrderFeesAsync);
                await RunModuleAsync("warranties", CreateWarrantiesAsync);
                await RunModuleAsync("feedbackChannels", CreateFeedbackChannelsAsync);
                await RunModuleAsync("customerFeedbacks", CreateCustomerFeedbacksAsync);
                await RunModuleAsync("appointments", CreateAppointmentsAsync);

                // ✅ Commit transaction nếu tất cả thành công
                await transaction.CommitAsync();

                return Ok(ApiResponse<object>.SuccessResult(new
                {
                    message = "Đã khởi tạo dữ liệu demo đầy đủ thành công",
                    cleared,
                    seeded
                }));
            }
            catch (InvalidOperationException ex)
            {
                // ✅ Rollback transaction nếu có lỗi
                await transaction.RollbackAsync();
                var errorResponse = ApiResponse<object>.ErrorResult(ex.Message);
                errorResponse.Data = new { message = ex.Message, cleared = cleared, seeded };
                return BadRequest(errorResponse);
            }
            catch (Exception ex)
            {
                // ✅ Rollback transaction nếu có exception
                await transaction.RollbackAsync();
                return StatusCode(500, ApiResponse<object>.ErrorResult("Lỗi khi khởi tạo dữ liệu demo", ex.Message));
            }
        }

        /// <summary>
        /// Tạo demo data cho Giai đoạn 1: Tiếp nhận &amp; Báo giá
        /// </summary>
        [HttpPost("create-phase-1")]
        public async Task<ActionResult<ApiResponse<object>>> CreatePhase1DemoData()
        {
            var seeded = new List<object>();

            // ✅ Sử dụng transaction để đảm bảo tính toàn vẹn dữ liệu
            await using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                async Task RunModuleAsync(string moduleName, Func<Task<object>> moduleFactory)
                {
                    var (success, result, message) = await ExecuteSeedAsync(moduleFactory);
                    seeded.Add(new { module = moduleName, success, message, result });

                    if (!success)
                    {
                        throw new InvalidOperationException(
                            string.IsNullOrWhiteSpace(message)
                                ? $"Tạo dữ liệu cho module {moduleName} thất bại."
                                : message);
                    }
                }

                // Giai đoạn 1: Tiếp nhận & Báo giá
                await RunModuleAsync("customers", CreateCustomersAsync);
                await RunModuleAsync("vehicles", CreateVehiclesAsync);
                await RunModuleAsync("employees", CreateEmployeesAsync);
                await RunModuleAsync("services", CreateServicesAsync);
                await RunModuleAsync("parts", CreatePartsAsync);
                await RunModuleAsync("suppliers", CreateSuppliersAsync);
                await RunModuleAsync("laborCategories", CreateLaborCategoriesAsync);
                await RunModuleAsync("laborItems", CreateLaborItemsAsync);
                await RunModuleAsync("inspections", CreateInspectionsAsync);
                await RunModuleAsync("quotations", CreateQuotationsAsync);
                await RunModuleAsync("appointments", CreateAppointmentsAsync);

                // ✅ Commit transaction nếu tất cả thành công
                await transaction.CommitAsync();

                return Ok(ApiResponse<object>.SuccessResult(new
                {
                    message = "Đã tạo demo data cho Giai đoạn 1: Tiếp nhận & Báo giá thành công",
                    phase = "Phase 1",
                    phaseName = "Tiếp nhận & Báo giá",
                    seeded
                }));
            }
            catch (InvalidOperationException ex)
            {
                // ✅ Rollback transaction nếu có lỗi
                await transaction.RollbackAsync();
                // ✅ FIX: Đảm bảo luôn có message, không được rỗng
                var errorMessage = !string.IsNullOrWhiteSpace(ex.Message) 
                    ? ex.Message 
                    : "Có lỗi xảy ra khi tạo demo data cho Giai đoạn 1";
                
                // ✅ FIX: Nếu vẫn rỗng, thêm thông tin từ seeded list
                if (string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = "Có lỗi xảy ra khi tạo demo data cho Giai đoạn 1";
                    if (seeded.Any())
                    {
                        var lastFailed = seeded.LastOrDefault(s => s.GetType().GetProperty("success")?.GetValue(s)?.ToString() == "False");
                        if (lastFailed != null)
                        {
                            var lastMessage = lastFailed.GetType().GetProperty("message")?.GetValue(lastFailed)?.ToString();
                            if (!string.IsNullOrWhiteSpace(lastMessage))
                            {
                                errorMessage = lastMessage;
                            }
                        }
                    }
                }
                
                var errorResponse = ApiResponse<object>.ErrorResult(errorMessage);
                errorResponse.Data = new { message = errorMessage, seeded, error = ex.Message ?? "Unknown error" };
                return BadRequest(errorResponse);
            }
            catch (Exception ex)
            {
                // ✅ Rollback transaction nếu có exception
                await transaction.RollbackAsync();
                // ✅ FIX: Đảm bảo luôn có message, không được rỗng
                var errorMessage = !string.IsNullOrWhiteSpace(ex.Message) 
                    ? $"Lỗi khi tạo demo data cho Giai đoạn 1: {ex.Message}" 
                    : "Có lỗi xảy ra khi tạo demo data cho Giai đoạn 1 (không có thông báo lỗi cụ thể)";
                    
                if (ex.InnerException != null && !string.IsNullOrWhiteSpace(ex.InnerException.Message))
                {
                    errorMessage += $" Chi tiết: {ex.InnerException.Message}";
                }
                
                // ✅ FIX: Nếu vẫn rỗng, thêm thông tin từ exception type
                if (string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = $"Lỗi khi tạo demo data cho Giai đoạn 1: {ex.GetType().Name}";
                }
                
                var errorResponse = ApiResponse<object>.ErrorResult(errorMessage);
                errorResponse.Data = new { message = errorMessage, seeded, error = ex.Message ?? ex.GetType().Name, innerError = ex.InnerException?.Message };
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Tạo demo data cho Giai đoạn 2: Sửa chữa &amp; Quản lý xuất kho
        /// </summary>
        [HttpPost("create-phase-2")]
        public async Task<ActionResult<ApiResponse<object>>> CreatePhase2DemoData()
        {
            var seeded = new List<object>();

            // ✅ Sử dụng transaction để đảm bảo tính toàn vẹn dữ liệu
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Kiểm tra dữ liệu giai đoạn 1 có tồn tại không
                var hasPhase1Data = await _unitOfWork.ServiceQuotations.CountAsync() > 0;
                if (!hasPhase1Data)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(ApiResponse<object>.ErrorResult(
                        "Cần tạo demo data cho Giai đoạn 1 trước (Tiếp nhận & Báo giá)"));
                }

                async Task RunModuleAsync(string moduleName, Func<Task<object>> moduleFactory)
                {
                    var (success, result, message) = await ExecuteSeedAsync(moduleFactory);
                    seeded.Add(new { module = moduleName, success, message, result });

                    if (!success)
                    {
                        throw new InvalidOperationException(
                            string.IsNullOrWhiteSpace(message)
                                ? $"Tạo dữ liệu cho module {moduleName} thất bại."
                                : message);
                    }
                }

                // Giai đoạn 2: Sửa chữa & Quản lý xuất kho
                await RunModuleAsync("orders", CreateServiceOrdersAsync);
                await RunModuleAsync("serviceOrderLabors", CreateServiceOrderLaborsAsync);
                await RunModuleAsync("materialRequests", CreateMaterialRequestsAsync);

                // ✅ Commit transaction nếu tất cả thành công
                await transaction.CommitAsync();

                return Ok(ApiResponse<object>.SuccessResult(new
                {
                    message = "Đã tạo demo data cho Giai đoạn 2: Sửa chữa & Quản lý xuất kho thành công",
                    phase = "Phase 2",
                    phaseName = "Sửa chữa & Quản lý xuất kho",
                    seeded
                }));
            }
            catch (InvalidOperationException ex)
            {
                // ✅ Rollback transaction nếu có lỗi
                await transaction.RollbackAsync();
                var errorResponse = ApiResponse<object>.ErrorResult(ex.Message);
                errorResponse.Data = new { message = ex.Message, seeded };
                return BadRequest(errorResponse);
            }
            catch (Exception ex)
            {
                // ✅ Rollback transaction nếu có exception
                await transaction.RollbackAsync();
                return StatusCode(500, ApiResponse<object>.ErrorResult("Lỗi khi tạo demo data cho Giai đoạn 2", ex.Message));
            }
        }

        /// <summary>
        /// Tạo demo data cho Giai đoạn 3: Quyết toán &amp; Chăm sóc hậu mãi
        /// </summary>
        [HttpPost("create-phase-3")]
        public async Task<ActionResult<ApiResponse<object>>> CreatePhase3DemoData()
        {
            var seeded = new List<object>();

            // ✅ Sử dụng transaction để đảm bảo tính toàn vẹn dữ liệu
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Kiểm tra dữ liệu giai đoạn 2 có tồn tại không
                var hasPhase2Data = await _unitOfWork.ServiceOrders.CountAsync() > 0;
                if (!hasPhase2Data)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(ApiResponse<object>.ErrorResult(
                        "Cần tạo demo data cho Giai đoạn 2 trước (Sửa chữa & Quản lý xuất kho)"));
                }

                async Task RunModuleAsync(string moduleName, Func<Task<object>> moduleFactory)
                {
                    var (success, result, message) = await ExecuteSeedAsync(moduleFactory);
                    seeded.Add(new { module = moduleName, success, message, result });

                    if (!success)
                    {
                        throw new InvalidOperationException(
                            string.IsNullOrWhiteSpace(message)
                                ? $"Tạo dữ liệu cho module {moduleName} thất bại."
                                : message);
                    }
                }

                // Giai đoạn 3: Quyết toán & Chăm sóc hậu mãi
                await RunModuleAsync("payments", CreatePaymentsAsync);
                await RunModuleAsync("serviceFeeTypes", CreateServiceFeeTypesAsync);
                await RunModuleAsync("serviceOrderFees", CreateServiceOrderFeesAsync);
                await RunModuleAsync("warranties", CreateWarrantiesAsync);
                await RunModuleAsync("feedbackChannels", CreateFeedbackChannelsAsync);
                await RunModuleAsync("customerFeedbacks", CreateCustomerFeedbacksAsync);

                // ✅ Commit transaction nếu tất cả thành công
                await transaction.CommitAsync();

                return Ok(ApiResponse<object>.SuccessResult(new
                {
                    message = "Đã tạo demo data cho Giai đoạn 3: Quyết toán & Chăm sóc hậu mãi thành công",
                    phase = "Phase 3",
                    phaseName = "Quyết toán & Chăm sóc hậu mãi",
                    seeded
                }));
            }
            catch (InvalidOperationException ex)
            {
                // ✅ Rollback transaction nếu có lỗi
                await transaction.RollbackAsync();
                var errorResponse = ApiResponse<object>.ErrorResult(ex.Message);
                errorResponse.Data = new { message = ex.Message, seeded };
                return BadRequest(errorResponse);
            }
            catch (Exception ex)
            {
                // ✅ Rollback transaction nếu có exception
                await transaction.RollbackAsync();
                return StatusCode(500, ApiResponse<object>.ErrorResult("Lỗi khi tạo demo data cho Giai đoạn 3", ex.Message));
            }
        }

        /// <summary>
        /// Tạo demo data cho Giai đoạn 4: Chuẩn hóa quản lý phụ tùng &amp; Procurement
        /// </summary>
        [HttpPost("create-phase-4")]
        public async Task<ActionResult<ApiResponse<object>>> CreatePhase4DemoData()
        {
            var seeded = new List<object>();

            // ✅ Sử dụng transaction để đảm bảo tính toàn vẹn dữ liệu
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                async Task RunModuleAsync(string moduleName, Func<Task<object>> moduleFactory)
                {
                    var (success, result, message) = await ExecuteSeedAsync(moduleFactory);
                    seeded.Add(new { module = moduleName, success, message, result });

                    if (!success)
                    {
                        throw new InvalidOperationException(
                            string.IsNullOrWhiteSpace(message)
                                ? $"Tạo dữ liệu cho module {moduleName} thất bại."
                                : message);
                    }
                }

                // Giai đoạn 4: Chuẩn hóa quản lý phụ tùng & Procurement
                await RunModuleAsync("warehouses", CreateWarehousesAsync);
                await RunModuleAsync("inventorychecks", CreateInventoryChecksAsync);
                await RunModuleAsync("inventoryadjustments", CreateInventoryAdjustmentsAsync);

                // ✅ Commit transaction nếu tất cả thành công
                await transaction.CommitAsync();

                return Ok(ApiResponse<object>.SuccessResult(new
                {
                    message = "Đã tạo demo data cho Giai đoạn 4: Chuẩn hóa quản lý phụ tùng & Procurement thành công",
                    phase = "Phase 4",
                    phaseName = "Chuẩn hóa quản lý phụ tùng & Procurement",
                    seeded
                }));
            }
            catch (InvalidOperationException ex)
            {
                // ✅ Rollback transaction nếu có lỗi
                await transaction.RollbackAsync();
                var errorResponse = ApiResponse<object>.ErrorResult(ex.Message);
                errorResponse.Data = new { message = ex.Message, seeded };
                return BadRequest(errorResponse);
            }
            catch (Exception ex)
            {
                // ✅ Rollback transaction nếu có exception
                await transaction.RollbackAsync();
                return StatusCode(500, ApiResponse<object>.ErrorResult("Lỗi khi tạo demo data cho Giai đoạn 4", ex.Message));
            }
        }

        #region Private Methods - Create Demo Data

        private async Task<List<object>> ClearAllDataInternalAsync()
        {
            var summary = new List<object>();

            async Task ClearAsync<T>(string name, IGenericRepository<T> repository) where T : BaseEntity
            {
                var entities = (await repository.GetAllAsync()).ToList();
                if (entities.Count == 0)
                {
                    return;
                }

                foreach (var entity in entities)
                {
                    await repository.DeleteAsync(entity);
                }

                summary.Add(new { entity = name, cleared = entities.Count });
            }

            await ClearAsync("PaymentTransactions", _unitOfWork.PaymentTransactions);
            await ClearAsync("ServiceOrderParts", _unitOfWork.Repository<ServiceOrderPart>());
            await ClearAsync("ServiceOrderItems", _unitOfWork.Repository<ServiceOrderItem>());
            await ClearAsync("ServiceOrders", _unitOfWork.ServiceOrders);
            await ClearAsync("QuotationItems", _unitOfWork.Repository<QuotationItem>());
            await ClearAsync("ServiceQuotations", _unitOfWork.ServiceQuotations);
            await ClearAsync("VehicleInspections", _unitOfWork.VehicleInspections);
            await ClearAsync("Appointments", _unitOfWork.Appointments);
            await ClearAsync("Vehicles", _unitOfWork.Vehicles);
            await ClearAsync("Customers", _unitOfWork.Customers);
            await ClearAsync("Employees", _unitOfWork.Employees);
            await ClearAsync("Services", _unitOfWork.Services);
            await ClearAsync("Parts", _unitOfWork.Parts);
            await ClearAsync("Suppliers", _unitOfWork.Suppliers);
            await ClearAsync("InventoryAdjustmentItems", _unitOfWork.InventoryAdjustmentItems);
            await ClearAsync("InventoryAdjustments", _unitOfWork.InventoryAdjustments);
            await ClearAsync("InventoryCheckItems", _unitOfWork.InventoryCheckItems);
            await ClearAsync("InventoryChecks", _unitOfWork.InventoryChecks);
            await ClearAsync("WarehouseBins", _unitOfWork.WarehouseBins);
            await ClearAsync("WarehouseZones", _unitOfWork.WarehouseZones);
            await ClearAsync("Warehouses", _unitOfWork.Warehouses);

            await _unitOfWork.SaveChangesAsync();
            return summary;
        }

        private async Task ClearEntitiesAsync<T>(List<object> summary, string name, IGenericRepository<T> repository) where T : BaseEntity
        {
            var entities = (await repository.GetAllAsync()).ToList();
            if (!entities.Any())
            {
                return;
            }

            foreach (var entity in entities)
            {
                await repository.DeleteAsync(entity);
            }

            summary.Add(new { entity = name, cleared = entities.Count });
        }

        private async Task<List<object>> ClearPhase1Async()
        {
            var summary = new List<object>();
            await ClearEntitiesAsync(summary, "Appointments", _unitOfWork.Appointments);
            await ClearEntitiesAsync(summary, "Quotations", _unitOfWork.ServiceQuotations);
            await ClearEntitiesAsync(summary, "Inspections", _unitOfWork.VehicleInspections);
            await ClearEntitiesAsync(summary, "LaborItems", _unitOfWork.Repository<LaborItem>());
            await ClearEntitiesAsync(summary, "LaborCategories", _unitOfWork.Repository<LaborCategory>());
            await ClearEntitiesAsync(summary, "Suppliers", _unitOfWork.Suppliers);
            await ClearEntitiesAsync(summary, "Parts", _unitOfWork.Parts);
            await ClearEntitiesAsync(summary, "Services", _unitOfWork.Services);
            await ClearEntitiesAsync(summary, "Employees", _unitOfWork.Employees);
            await ClearEntitiesAsync(summary, "Vehicles", _unitOfWork.Vehicles);
            await ClearEntitiesAsync(summary, "Customers", _unitOfWork.Customers);
            await _unitOfWork.SaveChangesAsync();
            return summary;
        }

        private async Task<List<object>> ClearPhase2Async()
        {
            var summary = new List<object>();
            await ClearEntitiesAsync(summary, "MaterialRequestItems", _unitOfWork.Repository<Core.Entities.MaterialRequestItem>());
            await ClearEntitiesAsync(summary, "MaterialRequests", _unitOfWork.Repository<Core.Entities.MaterialRequest>());
            await ClearEntitiesAsync(summary, "ServiceOrderLabors", _unitOfWork.Repository<ServiceOrderLabor>());
            await ClearEntitiesAsync(summary, "ServiceOrderParts", _unitOfWork.Repository<ServiceOrderPart>());
            await ClearEntitiesAsync(summary, "ServiceOrderItems", _unitOfWork.Repository<ServiceOrderItem>());
            await ClearEntitiesAsync(summary, "ServiceOrders", _unitOfWork.ServiceOrders);
            await _unitOfWork.SaveChangesAsync();
            return summary;
        }

        private async Task<List<object>> ClearPhase3Async()
        {
            var summary = new List<object>();
            await ClearEntitiesAsync(summary, "CustomerFeedbacks", _unitOfWork.CustomerFeedbacks);
            await ClearEntitiesAsync(summary, "FeedbackChannels", _unitOfWork.Repository<Core.Entities.FeedbackChannel>());
            await ClearEntitiesAsync(summary, "Warranties", _unitOfWork.Warranties);
            await ClearEntitiesAsync(summary, "ServiceOrderFees", _unitOfWork.ServiceOrderFees);
            await ClearEntitiesAsync(summary, "ServiceFeeTypes", _unitOfWork.Repository<Core.Entities.ServiceFeeType>());
            await ClearEntitiesAsync(summary, "PaymentTransactions", _unitOfWork.PaymentTransactions);
            await _unitOfWork.SaveChangesAsync();
            return summary;
        }

        private async Task<(bool Success, object Result, string Message)> ExecuteSeedAsync(Func<Task<object>> seedFunc)
        {
            try
            {
                var result = await seedFunc();
                var resultType = result.GetType();

                bool success = false;
                string message = string.Empty;

                var successProp = resultType.GetProperty("success", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (successProp != null && successProp.PropertyType == typeof(bool))
                {
                    success = (bool)(successProp.GetValue(result) ?? false);
                }

                var messageProp = resultType.GetProperty("message", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (messageProp != null)
                {
                    message = messageProp.GetValue(result)?.ToString() ?? string.Empty;
                }

                // ✅ FIX: Nếu success = false nhưng message rỗng, tạo message mặc định
                if (!success && string.IsNullOrWhiteSpace(message))
                {
                    message = "Tạo dữ liệu thất bại nhưng không có thông báo lỗi cụ thể.";
                }

                return (success, result, message);
            }
            catch (Exception ex)
            {
                // ✅ FIX: Catch exception và return error message đầy đủ
                // ✅ Đảm bảo luôn có message, không được rỗng
                var errorMessage = !string.IsNullOrWhiteSpace(ex.Message) 
                    ? $"Lỗi khi tạo dữ liệu: {ex.Message}" 
                    : $"Lỗi khi tạo dữ liệu: {ex.GetType().Name}";
                    
                if (ex.InnerException != null && !string.IsNullOrWhiteSpace(ex.InnerException.Message))
                {
                    errorMessage += $" Chi tiết: {ex.InnerException.Message}";
                }
                
                // ✅ FIX: Nếu vẫn rỗng, tạo message mặc định
                if (string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = "Lỗi khi tạo dữ liệu (không có thông báo lỗi cụ thể)";
                }
                
                return (false, new { success = false, message = errorMessage }, errorMessage);
            }
        }

        private async Task<object> CreateCustomersAsync()
        {
            // Kiểm tra existing customers để tránh duplicate
            var existingEmails = (await _context.Customers
                .Where(c => !c.IsDeleted)
                .Select(c => c.Email)
                .ToListAsync())
                .Where(e => !string.IsNullOrEmpty(e))
                .ToHashSet();

            var existingPhones = (await _context.Customers
                .Where(c => !c.IsDeleted)
                .Select(c => c.Phone)
                .ToListAsync())
                .Where(p => !string.IsNullOrEmpty(p))
                .ToHashSet();

            var customersToCreate = new List<Core.Entities.Customer>();

            var customers = new[]
            {
                new { Name = "Nguyễn Văn An", Email = "an.nguyen@email.com", Phone = "0901234567", Address = "123 Đường ABC, Quận 1, TP.HCM" },
                new { Name = "Trần Thị Bình", Email = "binh.tran@email.com", Phone = "0907654321", Address = "456 Đường XYZ, Quận 2, TP.HCM" },
                new { Name = "Lê Văn Cường", Email = "cuong.le@email.com", Phone = "0909876543", Address = "789 Đường DEF, Quận 3, TP.HCM" }
            };

            foreach (var customerData in customers)
            {
                // Kiểm tra duplicate email hoặc phone
                if ((!string.IsNullOrEmpty(customerData.Email) && existingEmails.Contains(customerData.Email)) ||
                    (!string.IsNullOrEmpty(customerData.Phone) && existingPhones.Contains(customerData.Phone)))
                {
                    continue; // Bỏ qua nếu đã tồn tại
                }

                customersToCreate.Add(new Core.Entities.Customer
                {
                    Name = customerData.Name,
                    Email = customerData.Email,
                    Phone = customerData.Phone,
                    Address = customerData.Address,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                });

                // Thêm vào hashset để tránh duplicate trong cùng batch
                if (!string.IsNullOrEmpty(customerData.Email))
                    existingEmails.Add(customerData.Email);
                if (!string.IsNullOrEmpty(customerData.Phone))
                    existingPhones.Add(customerData.Phone);
            }

            if (customersToCreate.Any())
            {
                foreach (var customer in customersToCreate)
                {
                    await _unitOfWork.Customers.AddAsync(customer);
                }
                await _unitOfWork.SaveChangesAsync();
            }

            return new { success = true, count = customersToCreate.Count, message = $"Đã tạo {customersToCreate.Count} khách hàng" };
        }

        private async Task<object> CreateVehiclesAsync()
        {
            // Lấy danh sách customers để gán xe
            var customers = (await _unitOfWork.Customers.GetAllAsync()).ToList();
            if (!customers.Any())
            {
                return new { success = false, message = "Cần tạo khách hàng trước khi tạo xe" };
            }

            // ✅ FIX: Kiểm tra existing license plates và VINs để tránh duplicate (chỉ check records chưa deleted)
            var existingLicensePlates = (await _context.Vehicles
                .Where(v => !v.IsDeleted)
                .Select(v => v.LicensePlate)
                .ToListAsync())
                .ToHashSet();
                
            var existingVINs = (await _context.Vehicles
                .Where(v => !v.IsDeleted)
                .Select(v => v.VIN)
                .ToListAsync())
                .Where(v => !string.IsNullOrEmpty(v))
                .ToHashSet();

            var vehiclesToCreate = new List<Core.Entities.Vehicle>();

            var vehiclesData = new[]
            {
                new { LicensePlate = "51A-12345", Brand = "Honda", Model = "Civic", Year = "2020", Color = "Trắng", VIN = "JH4DB1650MS123456", EngineNumber = "R18A123456", CustomerIndex = 0, VehicleType = "Personal", InsuranceCompany = (string?)"Bảo Việt", PolicyNumber = (string?)"BV-2024-001", CoverageType = (string?)"Full", ClaimNumber = (string?)null, AdjusterName = (string?)null, AdjusterPhone = (string?)null, CompanyName = (string?)null, TaxCode = (string?)null, ContactPerson = (string?)null, ContactPhone = (string?)null, Department = (string?)null, CostCenter = (string?)null },
                new { LicensePlate = "29B-67890", Brand = "Toyota", Model = "Vios", Year = "2019", Color = "Xám", VIN = "JTDKB20U123456789", EngineNumber = "2NZFE123456", CustomerIndex = 1, VehicleType = "Insurance", InsuranceCompany = (string?)"Bảo Minh", PolicyNumber = (string?)"BM-2024-002", CoverageType = (string?)"Third Party", ClaimNumber = (string?)"CL-2024-001", AdjusterName = (string?)"Nguyễn Văn A", AdjusterPhone = (string?)"0901234567", CompanyName = (string?)null, TaxCode = (string?)null, ContactPerson = (string?)null, ContactPhone = (string?)null, Department = (string?)null, CostCenter = (string?)null },
                new { LicensePlate = "43C-11111", Brand = "Hyundai", Model = "Accent", Year = "2021", Color = "Đỏ", VIN = "KMHDN41D6MU123456", EngineNumber = "G4LC123456", CustomerIndex = 2, VehicleType = "Company", InsuranceCompany = (string?)null, PolicyNumber = (string?)null, CoverageType = (string?)null, ClaimNumber = (string?)null, AdjusterName = (string?)null, AdjusterPhone = (string?)null, CompanyName = (string?)"Công ty ABC", TaxCode = (string?)"0123456789", ContactPerson = (string?)"Trần Thị B", ContactPhone = (string?)"0907654321", Department = (string?)"Kế toán", CostCenter = (string?)"CC-001" },
                new { LicensePlate = "30D-22222", Brand = "Ford", Model = "Ranger", Year = "2022", Color = "Đen", VIN = "MR0KB3CD7MM123456", EngineNumber = "P5AT123456", CustomerIndex = 0, VehicleType = "Personal", InsuranceCompany = (string?)"PVI", PolicyNumber = (string?)"PVI-2024-003", CoverageType = (string?)"Comprehensive", ClaimNumber = (string?)null, AdjusterName = (string?)null, AdjusterPhone = (string?)null, CompanyName = (string?)null, TaxCode = (string?)null, ContactPerson = (string?)null, ContactPhone = (string?)null, Department = (string?)null, CostCenter = (string?)null },
                new { LicensePlate = "61E-33333", Brand = "Mazda", Model = "CX-5", Year = "2023", Color = "Bạc", VIN = "JM3KFADL7N0123456", EngineNumber = "PY-VPTS123456", CustomerIndex = 1, VehicleType = "Insurance", InsuranceCompany = (string?)"PTI", PolicyNumber = (string?)"PTI-2024-004", CoverageType = (string?)"Full", ClaimNumber = (string?)"CL-2024-002", AdjusterName = (string?)"Lê Văn C", AdjusterPhone = (string?)"0909876543", CompanyName = (string?)null, TaxCode = (string?)null, ContactPerson = (string?)null, ContactPhone = (string?)null, Department = (string?)null, CostCenter = (string?)null }
            };

            foreach (var vehicleData in vehiclesData)
            {
                // ✅ FIX: Kiểm tra duplicate license plate và VIN (chỉ check records chưa deleted)
                if (existingLicensePlates.Contains(vehicleData.LicensePlate) ||
                    (!string.IsNullOrEmpty(vehicleData.VIN) && existingVINs.Contains(vehicleData.VIN)))
                {
                    continue; // Bỏ qua nếu đã tồn tại
                }

                // ✅ SAFE: customers.Any() checked at line 608, so First() is safe here
                // But use FirstOrDefault() + null check for extra safety
                var customer = vehicleData.CustomerIndex < customers.Count 
                    ? customers[vehicleData.CustomerIndex] 
                    : customers.FirstOrDefault();
                
                if (customer == null)
                {
                    // Should not happen due to check at line 608, but defensive check
                    continue;
                }

                var vehicle = new Core.Entities.Vehicle
                {
                    LicensePlate = vehicleData.LicensePlate,
                    Brand = vehicleData.Brand,
                    Model = vehicleData.Model,
                    Year = vehicleData.Year,
                    Color = vehicleData.Color,
                    VIN = vehicleData.VIN,
                    EngineNumber = vehicleData.EngineNumber,
                    CustomerId = customer.Id,
                    VehicleType = vehicleData.VehicleType,
                    OwnershipType = vehicleData.VehicleType,
                    UsageType = "Private"
                };

                // Set optional fields
                if (!string.IsNullOrEmpty(vehicleData.InsuranceCompany))
                {
                    vehicle.InsuranceCompany = vehicleData.InsuranceCompany;
                    vehicle.PolicyNumber = vehicleData.PolicyNumber;
                    vehicle.CoverageType = vehicleData.CoverageType;
                    vehicle.HasInsurance = true;
                    vehicle.IsInsuranceActive = true;
                }

                if (!string.IsNullOrEmpty(vehicleData.ClaimNumber))
                {
                    vehicle.ClaimNumber = vehicleData.ClaimNumber;
                    vehicle.AdjusterName = vehicleData.AdjusterName;
                    vehicle.AdjusterPhone = vehicleData.AdjusterPhone;
                    vehicle.ClaimStatus = "Pending";
                }

                if (!string.IsNullOrEmpty(vehicleData.CompanyName))
                {
                    vehicle.CompanyName = vehicleData.CompanyName;
                    vehicle.TaxCode = vehicleData.TaxCode;
                    vehicle.ContactPerson = vehicleData.ContactPerson;
                    vehicle.ContactPhone = vehicleData.ContactPhone;
                    vehicle.Department = vehicleData.Department;
                    vehicle.CostCenter = vehicleData.CostCenter;
                }

                vehiclesToCreate.Add(vehicle);
                existingLicensePlates.Add(vehicleData.LicensePlate); // Thêm vào hashset để tránh duplicate trong cùng batch
                if (!string.IsNullOrEmpty(vehicleData.VIN))
                    existingVINs.Add(vehicleData.VIN); // Thêm VIN vào hashset để tránh duplicate trong cùng batch
            }

            if (vehiclesToCreate.Any())
            {
                foreach (var vehicle in vehiclesToCreate)
                {
                    await _unitOfWork.Vehicles.AddAsync(vehicle);
                }
                await _unitOfWork.SaveChangesAsync();
            }

            return new { success = true, count = vehiclesToCreate.Count, message = $"Đã tạo {vehiclesToCreate.Count} xe" };
        }

        private async Task<object> CreateEmployeesAsync()
        {
            var employees = new[]
            {
                new Core.Entities.Employee
                {
                    Name = "Nguyễn Văn Minh",
                    Phone = "0901111111",
                    Email = "minh.nguyen@garage.com",
                    Address = "123 Đường ABC, Quận 1, TP.HCM",
                    Position = "Kỹ thuật viên chính",
                    Department = "Kỹ thuật",
                    HireDate = new DateTime(2020, 1, 15),
                    Salary = 15000000,
                    Status = "Active",
                    Skills = "Sửa chữa động cơ, Hệ thống điện, Phanh, Treo"
                },
                new Core.Entities.Employee
                {
                    Name = "Trần Thị Hoa",
                    Phone = "0902222222",
                    Email = "hoa.tran@garage.com",
                    Address = "456 Đường XYZ, Quận 2, TP.HCM",
                    Position = "Kỹ thuật viên",
                    Department = "Kỹ thuật",
                    HireDate = new DateTime(2021, 3, 10),
                    Salary = 12000000,
                    Status = "Active",
                    Skills = "Bảo dưỡng, Thay dầu, Kiểm tra tổng thể"
                },
                new Core.Entities.Employee
                {
                    Name = "Lê Văn Đức",
                    Phone = "0903333333",
                    Email = "duc.le@garage.com",
                    Address = "789 Đường DEF, Quận 3, TP.HCM",
                    Position = "Thủ kho",
                    Department = "Kho",
                    HireDate = new DateTime(2019, 6, 1),
                    Salary = 10000000,
                    Status = "Active",
                    Skills = "Quản lý kho, Nhập xuất hàng, Kiểm kê"
                },
                new Core.Entities.Employee
                {
                    Name = "Phạm Thị Lan",
                    Phone = "0904444444",
                    Email = "lan.pham@garage.com",
                    Address = "321 Đường GHI, Quận 4, TP.HCM",
                    Position = "Kế toán",
                    Department = "Hành chính",
                    HireDate = new DateTime(2020, 8, 20),
                    Salary = 13000000,
                    Status = "Active",
                    Skills = "Kế toán, Thuế, Báo cáo tài chính"
                },
                new Core.Entities.Employee
                {
                    Name = "Hoàng Văn Nam",
                    Phone = "0905555555",
                    Email = "nam.hoang@garage.com",
                    Address = "654 Đường JKL, Quận 5, TP.HCM",
                    Position = "Tư vấn dịch vụ",
                    Department = "Kinh doanh",
                    HireDate = new DateTime(2022, 2, 14),
                    Salary = 11000000,
                    Status = "Active",
                    Skills = "Tư vấn khách hàng, Báo giá, Chăm sóc khách hàng"
                },
                new Core.Entities.Employee
                {
                    Name = "Võ Thị Mai",
                    Phone = "0906666666",
                    Email = "mai.vo@garage.com",
                    Address = "987 Đường MNO, Quận 6, TP.HCM",
                    Position = "Kỹ thuật viên điện",
                    Department = "Kỹ thuật",
                    HireDate = new DateTime(2021, 11, 5),
                    Salary = 14000000,
                    Status = "Active",
                    Skills = "Điện tử ô tô, Hệ thống điều khiển, Sửa chữa điện"
                }
            };

            foreach (var employee in employees)
            {
                await _unitOfWork.Employees.AddAsync(employee);
            }
            await _unitOfWork.SaveChangesAsync();

            return new { success = true, count = employees.Length, message = $"Đã tạo {employees.Length} nhân viên" };
        }

        private async Task<object> CreateServicesAsync()
        {
            var services = new[]
            {
                new Core.Entities.Service
                {
                    Name = "Thay dầu động cơ",
                    Description = "Thay dầu động cơ và lọc dầu định kỳ",
                    Price = 500000,
                    Duration = 30,
                    Category = "Bảo dưỡng",
                    IsActive = true
                },
                new Core.Entities.Service
                {
                    Name = "Kiểm tra phanh",
                    Description = "Kiểm tra và bảo dưỡng hệ thống phanh",
                    Price = 800000,
                    Duration = 60,
                    Category = "Sửa chữa",
                    IsActive = true
                },
                new Core.Entities.Service
                {
                    Name = "Cân bằng lốp",
                    Description = "Cân bằng và đảo lốp xe",
                    Price = 200000,
                    Duration = 45,
                    Category = "Lốp",
                    IsActive = true
                },
                new Core.Entities.Service
                {
                    Name = "Sửa chữa hệ thống điện",
                    Description = "Kiểm tra và sửa chữa hệ thống điện ô tô",
                    Price = 1200000,
                    Duration = 120,
                    Category = "Điện",
                    IsActive = true
                },
                new Core.Entities.Service
                {
                    Name = "Thay lọc gió",
                    Description = "Thay lọc gió động cơ",
                    Price = 300000,
                    Duration = 20,
                    Category = "Bảo dưỡng",
                    IsActive = true
                },
                new Core.Entities.Service
                {
                    Name = "Kiểm tra hệ thống treo",
                    Description = "Kiểm tra và bảo dưỡng hệ thống treo",
                    Price = 1000000,
                    Duration = 90,
                    Category = "Sửa chữa",
                    IsActive = true
                },
                new Core.Entities.Service
                {
                    Name = "Thay bugi",
                    Description = "Thay bugi và kiểm tra hệ thống đánh lửa",
                    Price = 400000,
                    Duration = 30,
                    Category = "Bảo dưỡng",
                    IsActive = true
                },
                new Core.Entities.Service
                {
                    Name = "Sửa chữa điều hòa",
                    Description = "Kiểm tra và sửa chữa hệ thống điều hòa",
                    Price = 1500000,
                    Duration = 150,
                    Category = "Điều hòa",
                    IsActive = true
                },
                new Core.Entities.Service
                {
                    Name = "Thay dầu hộp số",
                    Description = "Thay dầu hộp số tự động/tay",
                    Price = 600000,
                    Duration = 60,
                    Category = "Bảo dưỡng",
                    IsActive = true
                },
                new Core.Entities.Service
                {
                    Name = "Kiểm tra tổng thể",
                    Description = "Kiểm tra tổng thể xe và báo cáo tình trạng",
                    Price = 300000,
                    Duration = 120,
                    Category = "Kiểm tra",
                    IsActive = true
                }
            };

            foreach (var service in services)
            {
                await _unitOfWork.Services.AddAsync(service);
            }
            await _unitOfWork.SaveChangesAsync();

            return new { success = true, count = services.Length, message = $"Đã tạo {services.Length} dịch vụ" };
        }

        private async Task<object> CreatePartsAsync()
        {
            var parts = new[]
            {
                new Core.Entities.Part
                {
                    PartNumber = "OIL-001",
                    PartName = "Dầu động cơ 5W-30",
                    Description = "Dầu động cơ tổng hợp 5W-30, 4 lít",
                    Category = "Dầu nhớt",
                    Brand = "Castrol",
                    Sku = "OIL-5W30-001",
                    Barcode = "1234567890123",
                    CostPrice = 350000,
                    AverageCostPrice = 350000,
                    SellPrice = 450000,
                    QuantityInStock = 50,
                    MinimumStock = 10,
                    ReorderLevel = 15,
                    DefaultUnit = "Thùng",
                    CompatibleVehicles = "Honda, Toyota, Hyundai, Ford, Mazda",
                    Location = "Kho A - Kệ 1",
                    IsActive = true,
                    PartUnits = new List<Core.Entities.PartUnit>
                    {
                        new Core.Entities.PartUnit { UnitName = "Thùng", ConversionRate = 1, IsDefault = true }
                    }
                },
                new Core.Entities.Part
                {
                    PartNumber = "FILTER-001",
                    PartName = "Lọc dầu động cơ",
                    Description = "Lọc dầu động cơ chính hãng",
                    Category = "Lọc",
                    Brand = "Bosch",
                    Sku = "FILTER-OIL-001",
                    Barcode = "1234567890124",
                    CostPrice = 80000,
                    AverageCostPrice = 80000,
                    SellPrice = 120000,
                    QuantityInStock = 100,
                    MinimumStock = 20,
                    ReorderLevel = 30,
                    DefaultUnit = "Cái",
                    CompatibleVehicles = "Honda Civic, Toyota Vios, Hyundai Accent",
                    Location = "Kho A - Kệ 2",
                    IsActive = true,
                    PartUnits = new List<Core.Entities.PartUnit>
                    {
                        new Core.Entities.PartUnit { UnitName = "Cái", ConversionRate = 1, IsDefault = true }
                    }
                },
                new Core.Entities.Part
                {
                    PartNumber = "BRAKE-001",
                    PartName = "Má phanh trước",
                    Description = "Má phanh trước chính hãng",
                    Category = "Phanh",
                    Brand = "Brembo",
                    Sku = "BRAKE-FRONT-001",
                    Barcode = "1234567890125",
                    CostPrice = 450000,
                    AverageCostPrice = 450000,
                    SellPrice = 650000,
                    QuantityInStock = 25,
                    MinimumStock = 5,
                    ReorderLevel = 10,
                    DefaultUnit = "Bộ",
                    CompatibleVehicles = "Honda Civic, Toyota Vios, Ford Ranger",
                    Location = "Kho B - Kệ 3",
                    IsActive = true,
                    PartUnits = new List<Core.Entities.PartUnit>
                    {
                        new Core.Entities.PartUnit { UnitName = "Bộ", ConversionRate = 1, IsDefault = true }
                    }
                },
                new Core.Entities.Part
                {
                    PartNumber = "SPARK-001",
                    PartName = "Bugi đánh lửa",
                    Description = "Bugi đánh lửa iridium",
                    Category = "Bugi",
                    Brand = "NGK",
                    Sku = "SPARK-IRIDIUM-001",
                    Barcode = "1234567890126",
                    CostPrice = 120000,
                    AverageCostPrice = 120000,
                    SellPrice = 180000,
                    QuantityInStock = 40,
                    MinimumStock = 8,
                    ReorderLevel = 12,
                    DefaultUnit = "Bộ 4 cái",
                    CompatibleVehicles = "Honda, Toyota, Hyundai, Mazda",
                    Location = "Kho A - Kệ 4",
                    IsActive = true,
                    PartUnits = new List<Core.Entities.PartUnit>
                    {
                        new Core.Entities.PartUnit { UnitName = "Bộ 4 cái", ConversionRate = 1, IsDefault = true }
                    }
                },
                new Core.Entities.Part
                {
                    PartNumber = "AIR-001",
                    PartName = "Lọc gió động cơ",
                    Description = "Lọc gió động cơ cao cấp",
                    Category = "Lọc",
                    Brand = "Mann",
                    Sku = "AIR-FILTER-001",
                    Barcode = "1234567890127",
                    CostPrice = 95000,
                    AverageCostPrice = 95000,
                    SellPrice = 140000,
                    QuantityInStock = 60,
                    MinimumStock = 12,
                    ReorderLevel = 18,
                    DefaultUnit = "Cái",
                    CompatibleVehicles = "Honda Civic, Toyota Vios, Hyundai Accent, Ford Ranger",
                    Location = "Kho A - Kệ 5",
                    IsActive = true,
                    PartUnits = new List<Core.Entities.PartUnit>
                    {
                        new Core.Entities.PartUnit { UnitName = "Cái", ConversionRate = 1, IsDefault = true }
                    }
                },
                new Core.Entities.Part
                {
                    PartNumber = "TIRE-001",
                    PartName = "Lốp xe 195/65R15",
                    Description = "Lốp xe radial 195/65R15",
                    Category = "Lốp",
                    Brand = "Michelin",
                    Sku = "TIRE-195-65R15-001",
                    Barcode = "1234567890128",
                    CostPrice = 1200000,
                    AverageCostPrice = 1200000,
                    SellPrice = 1800000,
                    QuantityInStock = 8,
                    MinimumStock = 2,
                    ReorderLevel = 4,
                    DefaultUnit = "Cái",
                    CompatibleVehicles = "Honda Civic, Toyota Vios, Hyundai Accent",
                    Location = "Kho C - Kệ 1",
                    IsActive = true,
                    PartUnits = new List<Core.Entities.PartUnit>
                    {
                        new Core.Entities.PartUnit { UnitName = "Cái", ConversionRate = 1, IsDefault = true }
                    }
                },
                new Core.Entities.Part
                {
                    PartNumber = "COOLANT-001",
                    PartName = "Dung dịch làm mát",
                    Description = "Dung dịch làm mát động cơ",
                    Category = "Dung dịch",
                    Brand = "Prestone",
                    Sku = "COOLANT-4L-001",
                    Barcode = "1234567890129",
                    CostPrice = 180000,
                    AverageCostPrice = 180000,
                    SellPrice = 250000,
                    QuantityInStock = 30,
                    MinimumStock = 6,
                    ReorderLevel = 10,
                    DefaultUnit = "Chai 4 lít",
                    CompatibleVehicles = "Tất cả xe",
                    Location = "Kho A - Kệ 6",
                    IsActive = true,
                    PartUnits = new List<Core.Entities.PartUnit>
                    {
                        new Core.Entities.PartUnit { UnitName = "Chai 4 lít", ConversionRate = 1, IsDefault = true }
                    }
                },
                new Core.Entities.Part
                {
                    PartNumber = "BATTERY-001",
                    PartName = "Ắc quy 12V 60Ah",
                    Description = "Ắc quy khô 12V 60Ah",
                    Category = "Điện",
                    Brand = "Varta",
                    Sku = "BATTERY-12V-60AH-001",
                    Barcode = "1234567890130",
                    CostPrice = 2200000,
                    AverageCostPrice = 2200000,
                    SellPrice = 3200000,
                    QuantityInStock = 12,
                    MinimumStock = 3,
                    ReorderLevel = 5,
                    DefaultUnit = "Cái",
                    CompatibleVehicles = "Honda, Toyota, Hyundai, Ford, Mazda",
                    Location = "Kho B - Kệ 4",
                    IsActive = true,
                    PartUnits = new List<Core.Entities.PartUnit>
                    {
                        new Core.Entities.PartUnit { UnitName = "Cái", ConversionRate = 1, IsDefault = true }
                    }
                }
            };

            foreach (var part in parts)
            {
                await _unitOfWork.Parts.AddAsync(part);
            }
            await _unitOfWork.SaveChangesAsync();

            return new { success = true, count = parts.Length, message = $"Đã tạo {parts.Length} phụ tùng" };
        }

        private async Task<object> CreateSuppliersAsync()
        {
            var suppliers = new[]
            {
                new Core.Entities.Supplier
                {
                    SupplierCode = "SUP-001",
                    SupplierName = "Công ty TNHH Phụ tùng ô tô ABC",
                    Phone = "028-12345678",
                    Email = "info@abcparts.com",
                    Address = "123 Đường Phụ tùng, Quận 7, TP.HCM",
                    ContactPerson = "Nguyễn Văn A",
                    ContactPhone = "0901111111",
                    TaxCode = "0123456789",
                    BankAccount = "1234567890",
                    BankName = "Vietcombank - Chi nhánh TP.HCM",
                    Notes = "Chuyên cung cấp phụ tùng chính hãng Honda, Toyota",
                    IsActive = true,
                    Rating = 5
                },
                new Core.Entities.Supplier
                {
                    SupplierCode = "SUP-002",
                    SupplierName = "Công ty CP Dầu nhớt XYZ",
                    Phone = "028-87654321",
                    Email = "sales@xyzoil.com",
                    Address = "456 Đường Dầu nhớt, Quận 8, TP.HCM",
                    ContactPerson = "Trần Thị B",
                    ContactPhone = "0902222222",
                    TaxCode = "0987654321",
                    BankAccount = "0987654321",
                    BankName = "Techcombank - Chi nhánh TP.HCM",
                    Notes = "Nhà phân phối độc quyền Castrol, Shell tại miền Nam",
                    IsActive = true,
                    Rating = 4
                },
                new Core.Entities.Supplier
                {
                    SupplierCode = "SUP-003",
                    SupplierName = "Công ty TNHH Lốp xe DEF",
                    Phone = "028-11223344",
                    Email = "contact@deftire.com",
                    Address = "789 Đường Lốp xe, Quận 9, TP.HCM",
                    ContactPerson = "Lê Văn C",
                    ContactPhone = "0903333333",
                    TaxCode = "1122334455",
                    BankAccount = "1122334455",
                    BankName = "BIDV - Chi nhánh TP.HCM",
                    Notes = "Chuyên lốp xe Michelin, Bridgestone, Continental",
                    IsActive = true,
                    Rating = 5
                },
                new Core.Entities.Supplier
                {
                    SupplierCode = "SUP-004",
                    SupplierName = "Công ty TNHH Phụ tùng điện GHI",
                    Phone = "028-55667788",
                    Email = "info@ghielectric.com",
                    Address = "321 Đường Điện tử, Quận 10, TP.HCM",
                    ContactPerson = "Phạm Thị D",
                    ContactPhone = "0904444444",
                    TaxCode = "5566778899",
                    BankAccount = "5566778899",
                    BankName = "ACB - Chi nhánh TP.HCM",
                    Notes = "Chuyên phụ tùng điện, ắc quy, hệ thống điều hòa",
                    IsActive = true,
                    Rating = 4
                },
                new Core.Entities.Supplier
                {
                    SupplierCode = "SUP-005",
                    SupplierName = "Công ty CP Dụng cụ sửa chữa JKL",
                    Phone = "028-99887766",
                    Email = "sales@jkltools.com",
                    Address = "654 Đường Dụng cụ, Quận 11, TP.HCM",
                    ContactPerson = "Hoàng Văn E",
                    ContactPhone = "0905555555",
                    TaxCode = "9988776655",
                    BankAccount = "9988776655",
                    BankName = "VPBank - Chi nhánh TP.HCM",
                    Notes = "Dụng cụ sửa chữa chuyên nghiệp, thiết bị đo lường",
                    IsActive = true,
                    Rating = 3
                }
            };

            foreach (var supplier in suppliers)
            {
                await _unitOfWork.Suppliers.AddAsync(supplier);
            }
            await _unitOfWork.SaveChangesAsync();

            return new { success = true, count = suppliers.Length, message = $"Đã tạo {suppliers.Length} nhà cung cấp" };
        }

        private async Task<object> CreateWarehousesAsync()
        {
            // Select chỉ Code từ database, đưa vào HashSet trong memory
            var existingWarehouseCodes = (await _context.Warehouses
                .Where(w => !w.IsDeleted)
                .Select(w => w.Code)
                .ToListAsync()).ToHashSet();

            var warehousesToCreate = new List<Core.Entities.Warehouse>();

            // Kiểm tra và tạo WH-001 nếu chưa tồn tại
            if (!existingWarehouseCodes.Contains("WH-001"))
            {
                warehousesToCreate.Add(new Core.Entities.Warehouse
                {
                    Code = "WH-001",
                    Name = "Kho Chính",
                    Description = "Kho chính của garage, lưu trữ phụ tùng và linh kiện chính",
                    Address = "123 Đường ABC, Quận 1, TP.HCM",
                    ManagerName = "Nguyễn Văn A",
                    PhoneNumber = "0123456789",
                    IsDefault = true,
                    IsActive = true
                });
            }

            // Kiểm tra và tạo WH-002 nếu chưa tồn tại
            if (!existingWarehouseCodes.Contains("WH-002"))
            {
                warehousesToCreate.Add(new Core.Entities.Warehouse
                {
                    Code = "WH-002",
                    Name = "Kho Phụ",
                    Description = "Kho phụ lưu trữ phụ tùng dự phòng và hàng tồn kho",
                    Address = "456 Đường XYZ, Quận 2, TP.HCM",
                    ManagerName = "Trần Văn B",
                    PhoneNumber = "0987654321",
                    IsDefault = false,
                    IsActive = true
                });
            }

            // Nếu tạo warehouse mới với IsDefault = true, set IsDefault = false cho các warehouse khác
            if (warehousesToCreate.Any(w => w.IsDefault))
            {
                // Select chỉ Id và IsDefault để update
                var defaultWarehouseIds = await _context.Warehouses
                    .Where(w => !w.IsDeleted && w.IsDefault)
                    .Select(w => w.Id)
                    .ToListAsync();
                
                foreach (var id in defaultWarehouseIds)
                {
                    var wh = await _unitOfWork.Warehouses.GetByIdAsync(id);
                    if (wh != null)
                    {
                        wh.IsDefault = false;
                        await _unitOfWork.Warehouses.UpdateAsync(wh);
                    }
                }
            }

            // Tạo warehouses mới
            foreach (var warehouse in warehousesToCreate)
            {
                await _unitOfWork.Warehouses.AddAsync(warehouse);
            }
            
            if (warehousesToCreate.Any() || (warehousesToCreate.Any(w => w.IsDefault) && await _context.Warehouses.AnyAsync(w => !w.IsDeleted && w.IsDefault)))
            {
                await _unitOfWork.SaveChangesAsync();
            }

            // Lấy warehouses để có ID (query trực tiếp trong DB)
            var mainWarehouse = await _unitOfWork.Warehouses.FirstOrDefaultAsync(w => w.Code == "WH-001");
            var subWarehouse = await _unitOfWork.Warehouses.FirstOrDefaultAsync(w => w.Code == "WH-002");

            // Kiểm tra nếu không tìm thấy warehouses
            if (mainWarehouse == null && subWarehouse == null)
            {
                return new { success = false, message = "Không thể tạo hoặc tìm thấy warehouses" };
            }

            // Select chỉ WarehouseId và Code từ database, đưa vào HashSet trong memory
            var existingZoneKeys = (await _context.WarehouseZones
                .Where(z => !z.IsDeleted)
                .Select(z => new { z.WarehouseId, z.Code })
                .ToListAsync())
                .Select(z => (z.WarehouseId, z.Code))
                .ToHashSet();

            // Tạo Warehouse Zones
            var zones = new List<Core.Entities.WarehouseZone>();

            if (mainWarehouse != null)
            {
                var zoneData = new[]
                {
                    new { Code = "ZONE-A", Name = "Khu A - Phụ Tùng Động Cơ", Description = "Khu vực lưu trữ phụ tùng động cơ", Order = 1 },
                    new { Code = "ZONE-B", Name = "Khu B - Phụ Tùng Gầm Xe", Description = "Khu vực lưu trữ phụ tùng gầm xe, phanh, lốp", Order = 2 },
                    new { Code = "ZONE-C", Name = "Khu C - Dầu Nhớt & Hóa Chất", Description = "Khu vực lưu trữ dầu nhớt, hóa chất bảo dưỡng", Order = 3 }
                };

                foreach (var zoneInfo in zoneData)
                {
                    var zoneKey = (mainWarehouse.Id, zoneInfo.Code);
                    if (!existingZoneKeys.Contains(zoneKey))
                    {
                        zones.Add(new Core.Entities.WarehouseZone
                        {
                            WarehouseId = mainWarehouse.Id,
                            Code = zoneInfo.Code,
                            Name = zoneInfo.Name,
                            Description = zoneInfo.Description,
                            DisplayOrder = zoneInfo.Order,
                            IsActive = true
                        });
                    }
                }
            }

            if (subWarehouse != null)
            {
                var zoneKey = (subWarehouse.Id, "ZONE-1");
                if (!existingZoneKeys.Contains(zoneKey))
                {
                    zones.Add(new Core.Entities.WarehouseZone
                    {
                        WarehouseId = subWarehouse.Id,
                        Code = "ZONE-1",
                        Name = "Khu 1 - Hàng Tồn Kho",
                        Description = "Khu vực lưu trữ hàng tồn kho",
                        DisplayOrder = 1,
                        IsActive = true
                    });
                }
            }

            if (zones.Any())
            {
                foreach (var zone in zones)
                {
                    await _unitOfWork.WarehouseZones.AddAsync(zone);
                }
                await _unitOfWork.SaveChangesAsync();
            }

            // Lấy zones để có ID (query trực tiếp trong DB)
            var zoneA = mainWarehouse != null ? await _unitOfWork.WarehouseZones.FirstOrDefaultAsync(z => z.WarehouseId == mainWarehouse.Id && z.Code == "ZONE-A") : null;
            var zoneB = mainWarehouse != null ? await _unitOfWork.WarehouseZones.FirstOrDefaultAsync(z => z.WarehouseId == mainWarehouse.Id && z.Code == "ZONE-B") : null;
            var zoneC = mainWarehouse != null ? await _unitOfWork.WarehouseZones.FirstOrDefaultAsync(z => z.WarehouseId == mainWarehouse.Id && z.Code == "ZONE-C") : null;
            var zone1 = subWarehouse != null ? await _unitOfWork.WarehouseZones.FirstOrDefaultAsync(z => z.WarehouseId == subWarehouse.Id && z.Code == "ZONE-1") : null;

            // Select chỉ WarehouseId và Code từ database, đưa vào HashSet trong memory
            var existingBinKeys = (await _context.WarehouseBins
                .Where(b => !b.IsDeleted)
                .Select(b => new { b.WarehouseId, b.Code })
                .ToListAsync())
                .Select(b => (b.WarehouseId, b.Code))
                .ToHashSet();

            // Tạo Warehouse Bins
            var bins = new List<Core.Entities.WarehouseBin>();

            if (mainWarehouse != null)
            {
                // Bins trong Khu A
                if (zoneA != null)
                {
                    var binData = new[]
                    {
                        new { Code = "BIN-A1", Name = "Kệ A1 - Bugi & Dây Điện", Description = "Kệ A1 lưu trữ bugi và dây điện", Capacity = 100m },
                        new { Code = "BIN-A2", Name = "Kệ A2 - Lọc Dầu & Lọc Gió", Description = "Kệ A2 lưu trữ lọc dầu và lọc gió", Capacity = 150m }
                    };

                    foreach (var binInfo in binData)
                    {
                        var binKey = (mainWarehouse.Id, binInfo.Code);
                        if (!existingBinKeys.Contains(binKey))
                        {
                            bins.Add(new Core.Entities.WarehouseBin
                            {
                                WarehouseId = mainWarehouse.Id,
                                WarehouseZoneId = zoneA.Id,
                                Code = binInfo.Code,
                                Name = binInfo.Name,
                                Description = binInfo.Description,
                                Capacity = binInfo.Capacity,
                                IsDefault = false,
                                IsActive = true
                            });
                        }
                    }
                }

                // Bins trong Khu B
                if (zoneB != null)
                {
                    var binData = new[]
                    {
                        new { Code = "BIN-B1", Name = "Kệ B1 - Lốp Xe", Description = "Kệ B1 lưu trữ lốp xe các loại", Capacity = 200m },
                        new { Code = "BIN-B2", Name = "Kệ B2 - Phanh & Đĩa Phanh", Description = "Kệ B2 lưu trữ phanh và đĩa phanh", Capacity = 80m }
                    };

                    foreach (var binInfo in binData)
                    {
                        var binKey = (mainWarehouse.Id, binInfo.Code);
                        if (!existingBinKeys.Contains(binKey))
                        {
                            bins.Add(new Core.Entities.WarehouseBin
                            {
                                WarehouseId = mainWarehouse.Id,
                                WarehouseZoneId = zoneB.Id,
                                Code = binInfo.Code,
                                Name = binInfo.Name,
                                Description = binInfo.Description,
                                Capacity = binInfo.Capacity,
                                IsDefault = false,
                                IsActive = true
                            });
                        }
                    }
                }

                // Bins trong Khu C
                if (zoneC != null)
                {
                    var binKey = (mainWarehouse.Id, "BIN-C1");
                    if (!existingBinKeys.Contains(binKey))
                    {
                        bins.Add(new Core.Entities.WarehouseBin
                        {
                            WarehouseId = mainWarehouse.Id,
                            WarehouseZoneId = zoneC.Id,
                            Code = "BIN-C1",
                            Name = "Kệ C1 - Dầu Nhớt",
                            Description = "Kệ C1 lưu trữ dầu nhớt các loại",
                            Capacity = 300,
                            IsDefault = false,
                            IsActive = true
                        });
                    }
                }

                // Bins trực thuộc Kho Chính (không qua Zone)
                var mainBinData = new[]
                {
                    new { Code = "BIN-MAIN-1", Name = "Kệ Chính 1 - Hàng Nhanh", Description = "Kệ chính lưu trữ hàng bán nhanh", Capacity = 100m, IsDefault = true },
                    new { Code = "BIN-MAIN-2", Name = "Kệ Chính 2 - Hàng Tồn", Description = "Kệ chính lưu trữ hàng tồn kho", Capacity = 150m, IsDefault = false }
                };

                foreach (var binInfo in mainBinData)
                {
                    var binKey = (mainWarehouse.Id, binInfo.Code);
                    if (!existingBinKeys.Contains(binKey))
                    {
                        bins.Add(new Core.Entities.WarehouseBin
                        {
                            WarehouseId = mainWarehouse.Id,
                            WarehouseZoneId = null,
                            Code = binInfo.Code,
                            Name = binInfo.Name,
                            Description = binInfo.Description,
                            Capacity = binInfo.Capacity,
                            IsDefault = binInfo.IsDefault,
                            IsActive = true
                        });
                    }
                }
            }

            if (subWarehouse != null)
            {
                // Bins trong Khu 1
                if (zone1 != null)
                {
                    var binKey = (subWarehouse.Id, "BIN-P1");
                    if (!existingBinKeys.Contains(binKey))
                    {
                        bins.Add(new Core.Entities.WarehouseBin
                        {
                            WarehouseId = subWarehouse.Id,
                            WarehouseZoneId = zone1.Id,
                            Code = "BIN-P1",
                            Name = "Kệ Phụ 1",
                            Description = "Kệ phụ 1 trong kho phụ",
                            Capacity = 100,
                            IsDefault = false,
                            IsActive = true
                        });
                    }
                }

                // Bins trực thuộc Kho Phụ (không qua Zone)
                var binKeyMain = (subWarehouse.Id, "BIN-P-MAIN");
                if (!existingBinKeys.Contains(binKeyMain))
                {
                    bins.Add(new Core.Entities.WarehouseBin
                    {
                        WarehouseId = subWarehouse.Id,
                        WarehouseZoneId = null,
                        Code = "BIN-P-MAIN",
                        Name = "Kệ Chính Kho Phụ",
                        Description = "Kệ chính trong kho phụ",
                        Capacity = 120,
                        IsDefault = true,
                        IsActive = true
                    });
                }
            }

            if (bins.Any())
            {
                foreach (var bin in bins)
                {
                    await _unitOfWork.WarehouseBins.AddAsync(bin);
                }
                await _unitOfWork.SaveChangesAsync();
            }

            return new 
            { 
                success = true, 
                count = warehousesToCreate.Count, 
                zoneCount = zones.Count,
                binCount = bins.Count,
                message = $"Đã tạo {warehousesToCreate.Count} kho, {zones.Count} khu vực, {bins.Count} kệ" 
            };
        }

        private async Task<object> CreateInspectionsAsync()
        {
            // Lấy dữ liệu cần thiết
            var vehicles = (await _unitOfWork.Vehicles.GetAllAsync()).ToList();
            var customers = (await _unitOfWork.Customers.GetAllAsync()).ToList();
            var employees = (await _unitOfWork.Employees.GetAllAsync()).ToList();

            // ✅ FIX: Check số lượng tối thiểu
            if (!vehicles.Any() || !customers.Any() || !employees.Any())
            {
                return new { success = false, message = "Cần tạo Vehicles, Customers và Employees trước khi tạo inspections" };
            }
            
            // ✅ FIX: Đảm bảo có đủ dữ liệu (hoặc sẽ dùng modulo nếu thiếu)

            var inspections = new[]
            {
                new Core.Entities.VehicleInspection
                {
                    InspectionNumber = "INS-2024-001",
                    VehicleId = vehicles.First().Id,
                    CustomerId = customers.First().Id,
                    InspectorId = employees.First().Id,
                    InspectionDate = DateTime.Now.AddDays(-5),
                    InspectionType = "General",
                    CurrentMileage = 45000,
                    FuelLevel = "3/4",
                    GeneralCondition = "Xe trong tình trạng tốt, cần bảo dưỡng định kỳ",
                    ExteriorCondition = "Thân xe sạch sẽ, không có vết trầy xước lớn",
                    InteriorCondition = "Nội thất gọn gàng, ghế ngồi còn tốt",
                    EngineCondition = "Động cơ hoạt động bình thường, không có tiếng lạ",
                    BrakeCondition = "Phanh hoạt động tốt, cần kiểm tra má phanh",
                    SuspensionCondition = "Hệ thống treo ổn định, không có tiếng kêu",
                    TireCondition = "Lốp còn độ sâu gai tốt, áp suất đúng",
                    CustomerComplaints = "Khách hàng phản ánh xe hơi rung khi chạy tốc độ cao",
                    Recommendations = "Cần cân bằng lốp và kiểm tra hệ thống treo",
                    TechnicianNotes = "Đã kiểm tra kỹ, cần thực hiện cân bằng lốp",
                    Status = "Completed",
                    CompletedDate = DateTime.Now.AddDays(-4)
                },
                new Core.Entities.VehicleInspection
                {
                    InspectionNumber = "INS-2024-002",
                    // ✅ FIX: Sử dụng modulo để tránh IndexOutOfRangeException
                    VehicleId = vehicles[1 % vehicles.Count].Id,
                    CustomerId = customers[1 % customers.Count].Id,
                    InspectorId = employees[1 % employees.Count].Id,
                    InspectionDate = DateTime.Now.AddDays(-3),
                    InspectionType = "Diagnostic",
                    CurrentMileage = 78000,
                    FuelLevel = "1/2",
                    GeneralCondition = "Xe cần sửa chữa một số bộ phận",
                    ExteriorCondition = "Có vết trầy xước nhỏ ở cản trước",
                    InteriorCondition = "Nội thất tốt, ghế lái hơi mòn",
                    EngineCondition = "Động cơ có tiếng lạ, cần kiểm tra bugi",
                    BrakeCondition = "Má phanh mòn, cần thay mới",
                    SuspensionCondition = "Hệ thống treo cần kiểm tra, có tiếng kêu",
                    TireCondition = "Lốp còn tốt nhưng cần đảo lốp",
                    CustomerComplaints = "Xe khó nổ máy vào buổi sáng, phanh kém hiệu quả",
                    Recommendations = "Thay bugi, má phanh và kiểm tra hệ thống treo",
                    TechnicianNotes = "Cần sửa chữa nhiều bộ phận, ước tính chi phí cao",
                    Status = "Completed",
                    CompletedDate = DateTime.Now.AddDays(-2)
                },
                new Core.Entities.VehicleInspection
                {
                    InspectionNumber = "INS-2024-003",
                    // ✅ FIX: Sử dụng modulo để tránh IndexOutOfRangeException
                    VehicleId = vehicles[2 % vehicles.Count].Id,
                    CustomerId = customers[2 % customers.Count].Id,
                    InspectorId = employees[0 % employees.Count].Id, // Sử dụng employee đầu tiên
                    InspectionDate = DateTime.Now.AddDays(-1),
                    InspectionType = "Pre-service",
                    CurrentMileage = 25000,
                    FuelLevel = "Full",
                    GeneralCondition = "Xe mới, tình trạng rất tốt",
                    ExteriorCondition = "Thân xe mới, không có vết trầy xước",
                    InteriorCondition = "Nội thất mới, ghế ngồi tốt",
                    EngineCondition = "Động cơ mới, hoạt động hoàn hảo",
                    BrakeCondition = "Phanh mới, hoạt động tốt",
                    SuspensionCondition = "Hệ thống treo mới, ổn định",
                    TireCondition = "Lốp mới, độ sâu gai tốt",
                    CustomerComplaints = "Khách hàng muốn bảo dưỡng định kỳ",
                    Recommendations = "Thay dầu và lọc dầu định kỳ",
                    TechnicianNotes = "Xe trong tình trạng tốt, chỉ cần bảo dưỡng cơ bản",
                    Status = "InProgress"
                }
            };

            foreach (var inspection in inspections)
            {
                await _unitOfWork.VehicleInspections.AddAsync(inspection);
            }
            await _unitOfWork.SaveChangesAsync();

            return new { success = true, count = inspections.Length, message = $"Đã tạo {inspections.Length} phiếu kiểm tra xe" };
        }

        private async Task<object> CreateQuotationsAsync()
        {
            // Lấy dữ liệu cần thiết
            var inspections = (await _unitOfWork.VehicleInspections.GetAllAsync()).ToList();
            var customers = (await _unitOfWork.Customers.GetAllAsync()).ToList();
            var vehicles = (await _unitOfWork.Vehicles.GetAllAsync()).ToList();
            var employees = (await _unitOfWork.Employees.GetAllAsync()).ToList();
            var services = (await _unitOfWork.Services.GetAllAsync()).ToList();
            var parts = (await _unitOfWork.Parts.GetAllAsync()).ToList();

            // ✅ FIX: Check số lượng tối thiểu để tránh IndexOutOfRangeException
            if (!inspections.Any() || !customers.Any() || !vehicles.Any() || !employees.Any() || !services.Any() || !parts.Any())
            {
                return new { success = false, message = "Cần tạo đầy đủ dữ liệu trước khi tạo quotations" };
            }

            // ✅ FIX: Đảm bảo có đủ dữ liệu để tạo 3 quotations (hoặc sử dụng modulo nếu không đủ)
            // Tối thiểu cần: 1 inspection, 1 customer, 1 vehicle, 1 employee (sẽ dùng lại nếu thiếu)
            // Nhưng nên có ít nhất 2-3 để tạo dữ liệu đa dạng

            var quotations = new[]
            {
                new Core.Entities.ServiceQuotation
                {
                    QuotationNumber = "QUO-2024-001",
                    VehicleInspectionId = inspections.First().Id,
                    CustomerId = customers.First().Id,
                    VehicleId = vehicles.First().Id,
                    PreparedById = employees.First().Id,
                    QuotationDate = DateTime.Now.AddDays(-4),
                    ValidUntil = DateTime.Now.AddDays(30),
                    Description = "Báo giá sửa chữa theo kết quả kiểm tra xe",
                    Terms = "Báo giá có hiệu lực 30 ngày. Thanh toán bằng tiền mặt hoặc chuyển khoản.",
                    QuotationType = "Personal",
                    SubTotal = 1200000,
                    TaxAmount = 120000,
                    TaxRate = 10,
                    DiscountAmount = 0,
                    TotalAmount = 1320000,
                    Status = "Approved",
                    SentDate = DateTime.Now.AddDays(-3),
                    ApprovedDate = DateTime.Now.AddDays(-2),
                    CustomerNotes = "Khách hàng đồng ý với báo giá"
                },
                new Core.Entities.ServiceQuotation
                {
                    QuotationNumber = "QUO-2024-002",
                    // ✅ FIX: Sử dụng modulo để tránh IndexOutOfRangeException
                    VehicleInspectionId = inspections[1 % inspections.Count].Id,
                    CustomerId = customers[1 % customers.Count].Id,
                    VehicleId = vehicles[1 % vehicles.Count].Id,
                    PreparedById = employees[1 % employees.Count].Id,
                    QuotationDate = DateTime.Now.AddDays(-2),
                    ValidUntil = DateTime.Now.AddDays(30),
                    Description = "Báo giá sửa chữa toàn diện",
                    Terms = "Báo giá có hiệu lực 30 ngày. Hỗ trợ thanh toán trả góp.",
                    QuotationType = "Insurance",
                    SubTotal = 3500000,
                    TaxAmount = 0,
                    TaxRate = 0,
                    DiscountAmount = 0,
                    TotalAmount = 3500000,
                    MaxInsuranceAmount = 4000000,
                    Deductible = 500000,
                    Status = "Sent",
                    SentDate = DateTime.Now.AddDays(-1),
                    InsuranceAdjusterContact = "Nguyễn Văn A - 0901234567"
                },
                new Core.Entities.ServiceQuotation
                {
                    QuotationNumber = "QUO-2024-003",
                    // ✅ FIX: Sử dụng modulo để tránh IndexOutOfRangeException
                    VehicleInspectionId = inspections[2 % inspections.Count].Id,
                    CustomerId = customers[2 % customers.Count].Id,
                    VehicleId = vehicles[2 % vehicles.Count].Id,
                    PreparedById = employees[0 % employees.Count].Id, // Sử dụng employee đầu tiên
                    QuotationDate = DateTime.Now.AddDays(-1),
                    ValidUntil = DateTime.Now.AddDays(30),
                    Description = "Báo giá bảo dưỡng định kỳ",
                    Terms = "Báo giá có hiệu lực 30 ngày. Giá đã bao gồm thuế.",
                    QuotationType = "Company",
                    SubTotal = 800000,
                    TaxAmount = 0,
                    TaxRate = 0,
                    DiscountAmount = 80000,
                    TotalAmount = 720000,
                    PONumber = "PO-2024-001",
                    PaymentTerms = "Net30",
                    IsTaxExempt = true,
                    Status = "Draft",
                    CompanyContactPerson = "Trần Thị B - 0907654321"
                }
            };

            foreach (var quotation in quotations)
            {
                await _unitOfWork.ServiceQuotations.AddAsync(quotation);
            }
            await _unitOfWork.SaveChangesAsync();

            // Tạo QuotationItems cho từng quotation
            var quotationsList = quotations.ToList();
            
            // ✅ FIX: Check bounds trước khi access array index
            if (quotationsList.Count < 3)
            {
                return new { success = false, message = "Không đủ quotations để tạo items (cần ít nhất 3)" };
            }
            
            var quotationItems = new List<Core.Entities.QuotationItem>();

            // Quotation 1 - Personal
            quotationItems.AddRange(new[]
            {
                new Core.Entities.QuotationItem
                {
                    ServiceQuotationId = quotationsList[0].Id,
                    ServiceId = services.First().Id,
                    ItemName = services.First().Name,
                    Description = services.First().Description,
                    Quantity = 1,
                    UnitPrice = services.First().Price,
                    TotalPrice = services.First().Price,
                    ItemType = "Service", // ✅ FIX: Set ItemType để phân biệt Service vs Part
                    ItemCategory = "Service", // ✅ FIX: Set ItemCategory
                    IsOptional = false,
                    IsApproved = true,
                    DisplayOrder = 1
                },
                new Core.Entities.QuotationItem
                {
                    ServiceQuotationId = quotationsList[0].Id,
                    PartId = parts.First().Id,
                    ItemName = parts.First().PartName,
                    Description = parts.First().Description,
                    Quantity = 1,
                    UnitPrice = parts.First().SellPrice,
                    TotalPrice = parts.First().SellPrice,
                    ItemType = "Part", // ✅ FIX: Set ItemType để phân biệt Service vs Part
                    ItemCategory = "Material", // ✅ FIX: Set ItemCategory
                    IsOptional = false,
                    IsApproved = true,
                    DisplayOrder = 2
                }
            });

            // Quotation 2 - Insurance
            quotationItems.AddRange(new[]
            {
                new Core.Entities.QuotationItem
                {
                    ServiceQuotationId = quotationsList[1].Id,
                    ServiceId = services.Count > 1 ? services[1].Id : services.First().Id,
                    ItemName = services.Count > 1 ? services[1].Name : services.First().Name,
                    Description = services.Count > 1 ? services[1].Description : services.First().Description,
                    Quantity = 1,
                    UnitPrice = services.Count > 1 ? services[1].Price : services.First().Price,
                    TotalPrice = services.Count > 1 ? services[1].Price : services.First().Price,
                    ItemType = "Service", // ✅ FIX: Set ItemType
                    ItemCategory = "Service", // ✅ FIX: Set ItemCategory
                    IsOptional = false,
                    IsApproved = false,
                    DisplayOrder = 1
                },
                new Core.Entities.QuotationItem
                {
                    ServiceQuotationId = quotationsList[1].Id,
                    PartId = parts.Count > 1 ? parts[1].Id : parts.First().Id,
                    ItemName = parts.Count > 1 ? parts[1].PartName : parts.First().PartName,
                    Description = parts.Count > 1 ? parts[1].Description : parts.First().Description,
                    Quantity = 2,
                    UnitPrice = parts.Count > 1 ? parts[1].SellPrice : parts.First().SellPrice,
                    TotalPrice = (parts.Count > 1 ? parts[1].SellPrice : parts.First().SellPrice) * 2,
                    ItemType = "Part", // ✅ FIX: Set ItemType
                    ItemCategory = "Material", // ✅ FIX: Set ItemCategory
                    IsOptional = false,
                    IsApproved = false,
                    DisplayOrder = 2
                }
            });

            // Quotation 3 - Company
            quotationItems.AddRange(new[]
            {
                new Core.Entities.QuotationItem
                {
                    ServiceQuotationId = quotationsList[2].Id,
                    ServiceId = services.First().Id,
                    ItemName = services.First().Name,
                    Description = services.First().Description,
                    Quantity = 1,
                    UnitPrice = services.First().Price,
                    TotalPrice = services.First().Price,
                    ItemType = "Service", // ✅ FIX: Set ItemType
                    ItemCategory = "Service", // ✅ FIX: Set ItemCategory
                    IsOptional = false,
                    IsApproved = false,
                    DisplayOrder = 1
                }
            });

            foreach (var item in quotationItems)
            {
                await _unitOfWork.Repository<Core.Entities.QuotationItem>().AddAsync(item);
            }
            await _unitOfWork.SaveChangesAsync();

            return new { success = true, count = quotations.Length, message = $"Đã tạo {quotations.Length} báo giá dịch vụ" };
        }

        private async Task<object> CreateServiceOrdersAsync()
        {
            // ✅ FIX: Không tạo lại khi đã có ServiceOrders để tránh duplicate index
            var existingServiceOrdersCount = await _context.ServiceOrders
                .Where(so => !so.IsDeleted)
                .CountAsync();

            if (existingServiceOrdersCount > 0)
            {
                return new
                {
                    success = false,
                    message = "Đã có đơn hàng sửa chữa trong hệ thống. Vui lòng xóa dữ liệu Phase 2 trước khi tạo lại."
                };
            }

            // Lấy dữ liệu cần thiết
            var quotations = (await _unitOfWork.ServiceQuotations.GetAllAsync()).ToList();
            var customers = (await _unitOfWork.Customers.GetAllAsync()).ToList();
            var vehicles = (await _unitOfWork.Vehicles.GetAllAsync()).ToList();
            var employees = (await _unitOfWork.Employees.GetAllAsync()).ToList();
            var services = (await _unitOfWork.Services.GetAllAsync()).ToList();
            var parts = (await _unitOfWork.Parts.GetAllAsync()).ToList();

            if (!customers.Any() || !vehicles.Any() || !employees.Any() || !services.Any() || !parts.Any())
            {
                return new { success = false, message = "Cần tạo đầy đủ dữ liệu trước khi tạo service orders" };
            }

            var serviceOrders = new[]
            {
                new Core.Entities.ServiceOrder
                {
                    OrderNumber = "SO-2024-001",
                    CustomerId = customers.First().Id,
                    VehicleId = vehicles.First().Id,
                    OrderDate = DateTime.Now.AddDays(-2),
                    ScheduledDate = DateTime.Now.AddDays(-1),
                    CompletedDate = DateTime.Now.AddHours(-2),
                    Status = "Completed",
                    Notes = "Đơn hàng hoàn thành theo đúng tiến độ",
                    TotalAmount = 1320000,
                    DiscountAmount = 0,
                    FinalAmount = 1320000,
                    PaymentStatus = "Paid",
                    ServiceQuotationId = quotations.Any() ? quotations.First().Id : null,
                    PrimaryTechnicianId = employees.First().Id,
                    ServiceTotal = 500000,
                    PartsTotal = 450000,
                    AmountPaid = 1320000,
                    AmountRemaining = 0
                },
                new Core.Entities.ServiceOrder
                {
                    OrderNumber = "SO-2024-002",
                    // ✅ FIX: Sử dụng modulo để tránh IndexOutOfRangeException
                    CustomerId = customers[1 % customers.Count].Id,
                    VehicleId = vehicles[1 % vehicles.Count].Id,
                    OrderDate = DateTime.Now.AddDays(-1),
                    ScheduledDate = DateTime.Now.AddHours(2),
                    Status = "InProgress",
                    Notes = "Đang thực hiện sửa chữa",
                    TotalAmount = 3500000,
                    DiscountAmount = 0,
                    FinalAmount = 3500000,
                    PaymentStatus = "Partial",
                    ServiceQuotationId = quotations.Any() ? quotations[1 % quotations.Count].Id : null,
                    PrimaryTechnicianId = employees[1 % employees.Count].Id,
                    ServiceTotal = 800000,
                    PartsTotal = 1300000,
                    AmountPaid = 1500000,
                    AmountRemaining = 2000000
                },
                new Core.Entities.ServiceOrder
                {
                    OrderNumber = "SO-2024-003",
                    // ✅ FIX: Sử dụng modulo để tránh IndexOutOfRangeException
                    CustomerId = customers[2 % customers.Count].Id,
                    VehicleId = vehicles[2 % vehicles.Count].Id,
                    OrderDate = DateTime.Now.AddHours(-4),
                    ScheduledDate = DateTime.Now.AddHours(4),
                    Status = "Pending",
                    Notes = "Chờ khách hàng xác nhận",
                    TotalAmount = 720000,
                    DiscountAmount = 80000,
                    FinalAmount = 640000,
                    PaymentStatus = "Unpaid",
                    ServiceQuotationId = quotations.Any() ? quotations[2 % quotations.Count].Id : null,
                    PrimaryTechnicianId = employees[0 % employees.Count].Id, // Sử dụng employee đầu tiên
                    ServiceTotal = 500000,
                    PartsTotal = 220000,
                    AmountPaid = 0,
                    AmountRemaining = 640000
                }
            };

            foreach (var order in serviceOrders)
            {
                if (order.ServiceQuotationId.HasValue)
                {
                    var quotation = quotations.FirstOrDefault(q => q.Id == order.ServiceQuotationId.Value);
                    if (quotation != null)
                    {
                        order.SubTotal = quotation.SubTotal;
                        order.VATAmount = quotation.VATAmount;
                        order.DiscountAmount = quotation.DiscountAmount;
                        order.TotalAmount = quotation.TotalAmount;
                        order.FinalAmount = quotation.TotalAmount;
                    }
                }

                await _unitOfWork.ServiceOrders.AddAsync(order);
            }
            await _unitOfWork.SaveChangesAsync();

            // Tạo ServiceOrderItems và ServiceOrderParts cho từng order
            var ordersList = serviceOrders.ToList();
            
            // ✅ FIX: Check bounds trước khi access array index
            if (ordersList.Count < 3)
            {
                return new { success = false, message = "Không đủ service orders để tạo items (cần ít nhất 3)" };
            }
            
            var orderItems = new List<Core.Entities.ServiceOrderItem>();
            var orderParts = new List<Core.Entities.ServiceOrderPart>();

            // Service Order 1 - Completed
            orderItems.Add(new Core.Entities.ServiceOrderItem
            {
                ServiceOrderId = ordersList[0].Id,
                ServiceId = services.First().Id,
                Quantity = 1,
                UnitPrice = services.First().Price,
                TotalPrice = services.First().Price,
                Notes = "Thay dầu động cơ hoàn thành"
            });

            orderParts.Add(new Core.Entities.ServiceOrderPart
            {
                ServiceOrderId = ordersList[0].Id,
                PartId = parts.First().Id,
                Quantity = 1,
                UnitCost = parts.First().CostPrice,
                UnitPrice = parts.First().SellPrice,
                TotalPrice = parts.First().SellPrice,
                Notes = "Dầu động cơ 5W-30"
            });

            // Service Order 2 - In Progress
            orderItems.Add(new Core.Entities.ServiceOrderItem
            {
                ServiceOrderId = ordersList[1].Id,
                ServiceId = services.Count > 1 ? services[1].Id : services.First().Id,
                Quantity = 1,
                UnitPrice = services.Count > 1 ? services[1].Price : services.First().Price,
                TotalPrice = services.Count > 1 ? services[1].Price : services.First().Price,
                Notes = "Đang kiểm tra và sửa chữa phanh"
            });

            orderParts.Add(new Core.Entities.ServiceOrderPart
            {
                ServiceOrderId = ordersList[1].Id,
                PartId = parts.Count > 1 ? parts[1].Id : parts.First().Id,
                Quantity = 2,
                UnitCost = parts.Count > 1 ? parts[1].CostPrice : parts.First().CostPrice,
                UnitPrice = parts.Count > 1 ? parts[1].SellPrice : parts.First().SellPrice,
                TotalPrice = (parts.Count > 1 ? parts[1].SellPrice : parts.First().SellPrice) * 2,
                Notes = "Má phanh trước - 2 bộ"
            });

            // Service Order 3 - Pending
            orderItems.Add(new Core.Entities.ServiceOrderItem
            {
                ServiceOrderId = ordersList[2].Id,
                ServiceId = services.First().Id,
                Quantity = 1,
                UnitPrice = services.First().Price,
                TotalPrice = services.First().Price,
                Notes = "Bảo dưỡng định kỳ"
            });

            orderParts.Add(new Core.Entities.ServiceOrderPart
            {
                ServiceOrderId = ordersList[2].Id,
                PartId = parts.Count > 2 ? parts[2].Id : parts.First().Id,
                Quantity = 1,
                UnitCost = parts.Count > 2 ? parts[2].CostPrice : parts.First().CostPrice,
                UnitPrice = parts.Count > 2 ? parts[2].SellPrice : parts.First().SellPrice,
                TotalPrice = parts.Count > 2 ? parts[2].SellPrice : parts.First().SellPrice,
                Notes = "Bugi đánh lửa"
            });

            foreach (var item in orderItems)
            {
                await _unitOfWork.Repository<Core.Entities.ServiceOrderItem>().AddAsync(item);
            }

            foreach (var part in orderParts)
            {
                await _unitOfWork.Repository<Core.Entities.ServiceOrderPart>().AddAsync(part);
            }

            await _unitOfWork.SaveChangesAsync();

            return new { success = true, count = serviceOrders.Length, message = $"Đã tạo {serviceOrders.Length} đơn hàng sửa chữa" };
        }

        private async Task<object> CreatePaymentsAsync()
        {
            // Lấy dữ liệu cần thiết
            var serviceOrders = (await _unitOfWork.ServiceOrders.GetAllAsync()).ToList();
            var employees = (await _unitOfWork.Employees.GetAllAsync()).ToList();

            if (!serviceOrders.Any() || !employees.Any())
            {
                return new { success = false, message = "Cần tạo ServiceOrders và Employees trước khi tạo payments" };
            }

            var payments = new[]
            {
                new Core.Entities.PaymentTransaction
                {
                    ReceiptNumber = "PT-2024-001",
                    ServiceOrderId = serviceOrders.First().Id,
                    PaymentDate = DateTime.Now.AddHours(-2),
                    Amount = 1320000,
                    PaymentMethod = "Cash",
                    TransactionReference = null,
                    CardType = null,
                    CardLastFourDigits = null,
                    ReceivedById = employees.First().Id,
                    Notes = "Thanh toán đầy đủ bằng tiền mặt",
                    IsRefund = false,
                    RefundReason = null
                },
                new Core.Entities.PaymentTransaction
                {
                    ReceiptNumber = "PT-2024-002",
                    ServiceOrderId = serviceOrders.Count > 1 ? serviceOrders[1].Id : serviceOrders.First().Id,
                    PaymentDate = DateTime.Now.AddHours(-1),
                    Amount = 1500000,
                    PaymentMethod = "Transfer",
                    TransactionReference = "TXN123456789",
                    CardType = null,
                    CardLastFourDigits = null,
                    ReceivedById = employees.Count > 1 ? employees[1].Id : employees.First().Id,
                    Notes = "Thanh toán một phần qua chuyển khoản",
                    IsRefund = false,
                    RefundReason = null
                },
                new Core.Entities.PaymentTransaction
                {
                    ReceiptNumber = "PT-2024-003",
                    ServiceOrderId = serviceOrders.Count > 1 ? serviceOrders[1].Id : serviceOrders.First().Id,
                    PaymentDate = DateTime.Now.AddMinutes(-30),
                    Amount = 500000,
                    PaymentMethod = "Card",
                    TransactionReference = "TXN987654321",
                    CardType = "Visa",
                    CardLastFourDigits = "1234",
                    ReceivedById = employees.First().Id,
                    Notes = "Thanh toán bằng thẻ Visa",
                    IsRefund = false,
                    RefundReason = null
                },
                new Core.Entities.PaymentTransaction
                {
                    ReceiptNumber = "PT-2024-004",
                    ServiceOrderId = serviceOrders.First().Id,
                    PaymentDate = DateTime.Now.AddMinutes(-15),
                    Amount = 100000,
                    PaymentMethod = "Cash",
                    TransactionReference = null,
                    CardType = null,
                    CardLastFourDigits = null,
                    ReceivedById = employees.First().Id,
                    Notes = "Hoàn tiền do phụ tùng lỗi",
                    IsRefund = true,
                    RefundReason = "Phụ tùng bị lỗi sản xuất"
                }
            };

            foreach (var payment in payments)
            {
                await _unitOfWork.PaymentTransactions.AddAsync(payment);
            }
            await _unitOfWork.SaveChangesAsync();

            return new { success = true, count = payments.Length, message = $"Đã tạo {payments.Length} giao dịch thanh toán" };
        }

        private async Task<object> CreateAppointmentsAsync()
        {
            // Lấy dữ liệu cần thiết
            var customers = (await _unitOfWork.Customers.GetAllAsync()).ToList();
            var vehicles = (await _unitOfWork.Vehicles.GetAllAsync()).ToList();
            var employees = (await _unitOfWork.Employees.GetAllAsync()).ToList();
            var inspections = (await _unitOfWork.VehicleInspections.GetAllAsync()).ToList();
            var serviceOrders = (await _unitOfWork.ServiceOrders.GetAllAsync()).ToList();

            if (!customers.Any() || !vehicles.Any() || !employees.Any())
            {
                return new { success = false, message = "Cần tạo Customers, Vehicles và Employees trước khi tạo appointments" };
            }

            var appointments = new[]
            {
                new Core.Entities.Appointment
                {
                    AppointmentNumber = "APT-2024-001",
                    CustomerId = customers.First().Id,
                    VehicleId = vehicles.First().Id,
                    ScheduledDateTime = DateTime.Now.AddDays(1).AddHours(9),
                    EstimatedDuration = 60,
                    AppointmentType = "Inspection",
                    ServiceRequested = "Kiểm tra tổng thể xe",
                    CustomerNotes = "Xe có tiếng lạ khi chạy",
                    Status = "Scheduled",
                    ConfirmedDate = DateTime.Now.AddHours(-2),
                    AssignedToId = employees.First().Id,
                    ReminderSent = false,
                    VehicleInspectionId = inspections.Any() ? inspections.First().Id : null
                },
                new Core.Entities.Appointment
                {
                    AppointmentNumber = "APT-2024-002",
                    CustomerId = customers.Count > 1 ? customers[1].Id : customers.First().Id,
                    VehicleId = vehicles.Count > 1 ? vehicles[1].Id : vehicles.First().Id,
                    ScheduledDateTime = DateTime.Now.AddDays(2).AddHours(14),
                    EstimatedDuration = 120,
                    AppointmentType = "Service",
                    ServiceRequested = "Bảo dưỡng định kỳ và thay dầu",
                    CustomerNotes = "Đã chạy 10,000km từ lần bảo dưỡng cuối",
                    Status = "Confirmed",
                    ConfirmedDate = DateTime.Now.AddHours(-1),
                    AssignedToId = employees.Count > 1 ? employees[1].Id : employees.First().Id,
                    ReminderSent = true,
                    ReminderSentDate = DateTime.Now.AddHours(-1),
                    ServiceOrderId = serviceOrders.Any() ? serviceOrders.First().Id : null
                },
                new Core.Entities.Appointment
                {
                    AppointmentNumber = "APT-2024-003",
                    CustomerId = customers.Count > 2 ? customers[2].Id : customers.First().Id,
                    VehicleId = vehicles.Count > 2 ? vehicles[2].Id : vehicles.First().Id,
                    ScheduledDateTime = DateTime.Now.AddHours(2),
                    EstimatedDuration = 90,
                    AppointmentType = "Service",
                    ServiceRequested = "Sửa chữa phanh",
                    CustomerNotes = "Phanh kém hiệu quả, cần kiểm tra ngay",
                    Status = "Arrived",
                    ConfirmedDate = DateTime.Now.AddHours(-3),
                    ArrivalTime = DateTime.Now.AddMinutes(-30),
                    AssignedToId = employees.First().Id,
                    ReminderSent = true,
                    ReminderSentDate = DateTime.Now.AddHours(-2)
                },
                new Core.Entities.Appointment
                {
                    AppointmentNumber = "APT-2024-004",
                    CustomerId = customers.First().Id,
                    VehicleId = vehicles.First().Id,
                    ScheduledDateTime = DateTime.Now.AddDays(3).AddHours(10),
                    EstimatedDuration = 45,
                    AppointmentType = "Pickup",
                    ServiceRequested = "Nhận xe sau sửa chữa",
                    CustomerNotes = "Nhận xe sau khi hoàn thành bảo dưỡng",
                    Status = "Scheduled",
                    ConfirmedDate = DateTime.Now.AddMinutes(-30),
                    AssignedToId = employees.Count > 2 ? employees[2].Id : employees.First().Id,
                    ReminderSent = false
                },
                new Core.Entities.Appointment
                {
                    AppointmentNumber = "APT-2024-005",
                    CustomerId = customers.Count > 1 ? customers[1].Id : customers.First().Id,
                    VehicleId = vehicles.Count > 1 ? vehicles[1].Id : vehicles.First().Id,
                    ScheduledDateTime = DateTime.Now.AddDays(-1).AddHours(15),
                    EstimatedDuration = 60,
                    AppointmentType = "Inspection",
                    ServiceRequested = "Kiểm tra trước khi bán",
                    CustomerNotes = "Định bán xe, cần kiểm tra tình trạng",
                    Status = "Completed",
                    ConfirmedDate = DateTime.Now.AddDays(-2),
                    ArrivalTime = DateTime.Now.AddDays(-1).AddMinutes(15),
                    ActualStartTime = DateTime.Now.AddDays(-1).AddMinutes(30),
                    ActualEndTime = DateTime.Now.AddDays(-1).AddMinutes(90),
                    AssignedToId = employees.First().Id,
                    ReminderSent = true,
                    ReminderSentDate = DateTime.Now.AddDays(-2),
                    VehicleInspectionId = inspections.Count > 1 ? inspections[1].Id : null
                },
                new Core.Entities.Appointment
                {
                    AppointmentNumber = "APT-2024-006",
                    CustomerId = customers.Count > 2 ? customers[2].Id : customers.First().Id,
                    VehicleId = vehicles.Count > 2 ? vehicles[2].Id : vehicles.First().Id,
                    ScheduledDateTime = DateTime.Now.AddDays(-2).AddHours(11),
                    EstimatedDuration = 120,
                    AppointmentType = "Service",
                    ServiceRequested = "Sửa chữa điều hòa",
                    CustomerNotes = "Điều hòa không lạnh",
                    Status = "Cancelled",
                    ConfirmedDate = DateTime.Now.AddDays(-3),
                    AssignedToId = employees.Count > 1 ? employees[1].Id : employees.First().Id,
                    ReminderSent = true,
                    ReminderSentDate = DateTime.Now.AddDays(-3),
                    CancellationReason = "Khách hàng có việc đột xuất"
                }
            };

            foreach (var appointment in appointments)
            {
                await _unitOfWork.Appointments.AddAsync(appointment);
            }
            await _unitOfWork.SaveChangesAsync();

            return new { success = true, count = appointments.Length, message = $"Đã tạo {appointments.Length} lịch hẹn" };
        }

        /// <summary>
        /// ✅ Phase 4.1: Tạo demo data cho Inventory Checks
        /// </summary>
        private async Task<object> CreateInventoryChecksAsync()
        {
            // Lấy dữ liệu cần thiết
            var warehouses = await _context.Warehouses
                .Where(w => !w.IsDeleted)
                .Include(w => w.Zones.Where(z => !z.IsDeleted))
                .Include(w => w.Bins.Where(b => !b.IsDeleted && b.WarehouseZoneId == null))
                .ToListAsync();

            var parts = await _context.Parts
                .Where(p => !p.IsDeleted && p.IsActive)
                .Take(20) // Lấy 20 parts đầu tiên
                .ToListAsync();

            var employees = await _context.Employees
                .Where(e => !e.IsDeleted && e.Status == "Active")
                .Take(3)
                .ToListAsync();

            if (!warehouses.Any() || !parts.Any() || !employees.Any())
            {
                return new { success = false, message = "Cần có warehouses, parts và employees trước khi tạo inventory checks" };
            }

            // ✅ FIX: Safe access với FirstOrDefault và null check
            var mainWarehouse = warehouses.FirstOrDefault(w => w.IsDefault) ?? warehouses.FirstOrDefault();
            if (mainWarehouse == null)
            {
                return new { success = false, message = "Không tìm thấy warehouse nào" };
            }
            var zoneA = mainWarehouse.Zones?.FirstOrDefault(z => z.Code == "ZONE-A");
            var bin1 = zoneA?.Bins?.FirstOrDefault() ?? mainWarehouse.Bins?.FirstOrDefault();

            // Kiểm tra existing codes để tránh duplicate
            var existingCodes = (await _context.InventoryChecks
                .Where(ic => !ic.IsDeleted)
                .Select(ic => ic.Code)
                .ToListAsync()).ToHashSet();

            var year = DateTime.Now.Year;
            var checks = new List<Core.Entities.InventoryCheck>();

            // 1. Inventory Check - Draft (nháp, chưa bắt đầu)
            var check0Code = $"IK-{year}-001";
            if (!existingCodes.Contains(check0Code))
            {
                var check0 = new Core.Entities.InventoryCheck
                {
                    Code = check0Code,
                    Name = "Kiểm kê kho chính - Dự kiến tháng sau",
                    WarehouseId = mainWarehouse.Id,
                    WarehouseZoneId = zoneA?.Id,
                    CheckDate = DateTime.Now.AddDays(7),
                    Status = "Draft",
                    Notes = "Phiếu kiểm kê dự kiến, chưa bắt đầu"
                };
                checks.Add(check0);
            }

            // 2. Inventory Check - InProgress (đang kiểm kê)
            var check1Code = $"IK-{year}-002";
            if (!existingCodes.Contains(check1Code))
            {
                var check1 = new Core.Entities.InventoryCheck
                {
                    Code = check1Code,
                    Name = "Kiểm kê kho chính - Tháng 1",
                    WarehouseId = mainWarehouse.Id,
                    WarehouseZoneId = zoneA?.Id,
                    WarehouseBinId = bin1?.Id,
                    CheckDate = DateTime.Now.AddDays(-2),
                    Status = "InProgress",
                    StartedByEmployeeId = employees.FirstOrDefault()?.Id ?? throw new InvalidOperationException("Không có employees để tạo inventory check"),
                    StartedDate = DateTime.Now.AddDays(-2).AddHours(9),
                    Notes = "Kiểm kê định kỳ tháng 1, đang trong quá trình kiểm kê"
                };
                checks.Add(check1);
            }

            // 3. Inventory Check - Completed (có discrepancies)
            var check2Code = $"IK-{year}-003";
            if (!existingCodes.Contains(check2Code))
            {
                var check2 = new Core.Entities.InventoryCheck
                {
                    Code = check2Code,
                    Name = "Kiểm kê kho chính - Tháng 12",
                    WarehouseId = mainWarehouse.Id,
                    WarehouseZoneId = zoneA?.Id,
                    CheckDate = DateTime.Now.AddDays(-10),
                    Status = "Completed",
                    StartedByEmployeeId = employees.FirstOrDefault()?.Id ?? throw new InvalidOperationException("Không có employees để tạo inventory check"),
                    StartedDate = DateTime.Now.AddDays(-10).AddHours(9),
                    CompletedByEmployeeId = employees.Count > 1 ? employees[1].Id : employees.FirstOrDefault()?.Id,
                    CompletedDate = DateTime.Now.AddDays(-10).AddHours(15),
                    Notes = "Kiểm kê tháng 12, phát hiện một số chênh lệch"
                };
                checks.Add(check2);
            }

            // 4. Inventory Check - Completed (không có discrepancies)
            var check3Code = $"IK-{year}-004";
            if (!existingCodes.Contains(check3Code))
            {
                var check3 = new Core.Entities.InventoryCheck
                {
                    Code = check3Code,
                    Name = "Kiểm kê kho phụ",
                    WarehouseId = warehouses.Count > 1 ? warehouses[1].Id : mainWarehouse.Id,
                    CheckDate = DateTime.Now.AddDays(-5),
                    Status = "Completed",
                    StartedByEmployeeId = employees.Count > 1 ? employees[1].Id : employees.FirstOrDefault()?.Id,
                    StartedDate = DateTime.Now.AddDays(-5).AddHours(8),
                    CompletedByEmployeeId = employees.Count > 1 ? employees[1].Id : employees.FirstOrDefault()?.Id,
                    CompletedDate = DateTime.Now.AddDays(-5).AddHours(12),
                    Notes = "Kiểm kê kho phụ, không có chênh lệch"
                };
                checks.Add(check3);
            }

            // 5. Inventory Check - InProgress (mới bắt đầu)
            var check4Code = $"IK-{year}-005";
            if (!existingCodes.Contains(check4Code))
            {
                var check4 = new Core.Entities.InventoryCheck
                {
                    Code = check4Code,
                    Name = "Kiểm kê kho chính - Tháng hiện tại",
                    WarehouseId = mainWarehouse.Id,
                    WarehouseZoneId = zoneA?.Id,
                    WarehouseBinId = bin1?.Id,
                    CheckDate = DateTime.Now,
                    Status = "InProgress",
                    StartedByEmployeeId = employees.FirstOrDefault()?.Id ?? throw new InvalidOperationException("Không có employees để tạo inventory check"),
                    StartedDate = DateTime.Now.AddHours(-2),
                    Notes = "Kiểm kê tháng hiện tại, đang tiến hành"
                };
                checks.Add(check4);
            }

            // Tạo checks
            foreach (var check in checks)
            {
                await _unitOfWork.InventoryChecks.AddAsync(check);
            }
            await _unitOfWork.SaveChangesAsync();

            // Reload checks để có IDs
            var createdChecks = await _context.InventoryChecks
                .Where(ic => checks.Select(c => c.Code).Contains(ic.Code))
                .ToListAsync();

            // Tạo items cho các checks
            var allItems = new List<Core.Entities.InventoryCheckItem>();

            // Check 0 (Draft) - chưa có items (đúng quy trình)
            // Không tạo items cho check này

            // Check 1 (InProgress) - một số items đã kiểm
            var check1ForItems = createdChecks.FirstOrDefault(c => c.Code == check1Code);
            if (check1ForItems != null && parts.Count >= 5)
            {
                for (int i = 0; i < 5; i++)
                {
                    var part = parts[i];
                    var systemQty = part.QuantityInStock;
                    var actualQty = systemQty + (i % 2 == 0 ? 0 : (i == 1 ? -2 : 1)); // Một số có chênh lệch
                    allItems.Add(new Core.Entities.InventoryCheckItem
                    {
                        InventoryCheckId = check1ForItems.Id,
                        PartId = part.Id,
                        SystemQuantity = systemQty,
                        ActualQuantity = actualQty,
                        DiscrepancyQuantity = actualQty - systemQty,
                        IsDiscrepancy = actualQty != systemQty,
                        Notes = actualQty != systemQty ? "Phát hiện chênh lệch" : null
                    });
                }
            }

            // Check 2 (Completed với discrepancies) - nhiều items có chênh lệch
            // ✅ FIX: Tạo 10 items để đủ cho 3 adjustments (adj1: 5, adj2: 3, adj3: 2)
            var check2ForItems = createdChecks.FirstOrDefault(c => c.Code == check2Code);
            if (check2ForItems != null && parts.Count >= 10)
            {
                for (int i = 0; i < 10; i++)
                {
                    var part = parts[i];
                    var systemQty = part.QuantityInStock;
                    // Tạo các scenarios khác nhau: thiếu, thừa, đúng
                    // ✅ FIX: Đảm bảo có đủ 10 items có discrepancy để chia cho 3 adjustments
                    var actualQtyRaw = i switch
                    {
                        0 => systemQty - 5,  // Thiếu 5
                        1 => systemQty + 3,  // Thừa 3
                        2 => systemQty - 2,  // Thiếu 2
                        3 => systemQty + 1,  // Thừa 1 (thay vì đúng)
                        4 => systemQty + 1,  // Thừa 1
                        5 => systemQty - 10, // Thiếu nhiều
                        6 => systemQty - 3,  // Thiếu 3 (thay vì đúng)
                        7 => systemQty + 2,  // Thừa 2
                        8 => systemQty - 4,  // Thiếu 4 (cho adj3)
                        9 => systemQty + 5,  // Thừa 5 (cho adj3)
                        _ => systemQty       // Default: không chênh lệch
                    };
                    // ✅ FIX: Đảm bảo ActualQuantity không âm (tối thiểu = 0)
                    var actualQty = Math.Max(0, actualQtyRaw);
                    allItems.Add(new Core.Entities.InventoryCheckItem
                    {
                        InventoryCheckId = check2ForItems.Id,
                        PartId = part.Id,
                        SystemQuantity = systemQty,
                        ActualQuantity = actualQty,
                        DiscrepancyQuantity = actualQty - systemQty,
                        IsDiscrepancy = actualQty != systemQty,
                        Notes = actualQty != systemQty 
                            ? (actualQty < systemQty ? $"Thiếu {systemQty - actualQty} sản phẩm" : $"Thừa {actualQty - systemQty} sản phẩm")
                            : null
                    });
                }
            }

            // Check 3 (Completed không có discrepancies)
            var check3ForItems = createdChecks.FirstOrDefault(c => c.Code == check3Code);
            if (check3ForItems != null && parts.Count >= 5)
            {
                for (int i = 5; i < Math.Min(10, parts.Count); i++)
                {
                    var part = parts[i];
                    var systemQty = part.QuantityInStock;
                    allItems.Add(new Core.Entities.InventoryCheckItem
                    {
                        InventoryCheckId = check3ForItems.Id,
                        PartId = part.Id,
                        SystemQuantity = systemQty,
                        ActualQuantity = systemQty,
                        DiscrepancyQuantity = 0,
                        IsDiscrepancy = false,
                        Notes = "Không có chênh lệch"
                    });
                }
            }

            // Check 4 (InProgress mới bắt đầu) - chưa có items
            // Không tạo items cho check này để demo trạng thái mới bắt đầu

            // Tạo items
            foreach (var item in allItems)
            {
                await _unitOfWork.InventoryCheckItems.AddAsync(item);
            }
            await _unitOfWork.SaveChangesAsync();

            return new 
            { 
                success = true, 
                checkCount = checks.Count, 
                itemCount = allItems.Count,
                message = $"Đã tạo {checks.Count} phiếu kiểm kê với {allItems.Count} items" 
            };
        }

        /// <summary>
        /// ✅ Phase 4.1: Tạo demo data cho Inventory Adjustments
        /// </summary>
        private async Task<object> CreateInventoryAdjustmentsAsync()
        {
            // Lấy completed inventory checks có discrepancies
            var completedChecks = await _context.InventoryChecks
                .Where(ic => !ic.IsDeleted && ic.Status == "Completed")
                .Include(ic => ic.Items.Where(i => !i.IsDeleted && i.IsDiscrepancy && !i.IsAdjusted))
                .ThenInclude(i => i.Part)
                .ToListAsync();

            if (!completedChecks.Any())
            {
                return new { success = false, message = "Không có inventory checks completed với discrepancies để tạo adjustments" };
            }

            var employees = await _context.Employees
                .Where(e => !e.IsDeleted && e.Status == "Active")
                .Take(2)
                .ToListAsync();

            if (!employees.Any())
            {
                return new { success = false, message = "Cần có employees để tạo adjustments" };
            }

            // Kiểm tra existing codes
            var existingCodes = (await _context.InventoryAdjustments
                .Where(ia => !ia.IsDeleted)
                .Select(ia => ia.AdjustmentNumber)
                .ToListAsync()).ToHashSet();

            var year = DateTime.Now.Year;
            var adjustments = new List<Core.Entities.InventoryAdjustment>();
            var allAdjustmentItems = new List<Core.Entities.InventoryAdjustmentItem>();

            // 1. Adjustment từ check 2 (Pending - chờ duyệt)
            // ✅ FIX: Tìm check2 bằng code "003" (check2Code = IK-YYYY-003)
            var yearForCheck = DateTime.Now.Year;
            var check2Code = $"IK-{yearForCheck}-003";
            var check2 = completedChecks.FirstOrDefault(c => c.Code == check2Code);
            if (check2 != null)
            {
                var discrepancyItems = check2.Items.Where(i => i.IsDiscrepancy && !i.IsAdjusted).Take(5).ToList();
                if (discrepancyItems.Any())
                {
                    var adj1Code = $"ADJ-{year}-001";
                    if (!existingCodes.Contains(adj1Code))
                    {
                        var adjustment1 = new Core.Entities.InventoryAdjustment
                        {
                            AdjustmentNumber = adj1Code,
                            InventoryCheckId = check2.Id,
                            WarehouseId = check2.WarehouseId,
                            WarehouseZoneId = check2.WarehouseZoneId,
                            WarehouseBinId = check2.WarehouseBinId,
                            AdjustmentDate = DateTime.Now.AddDays(-8),
                            Status = "Pending",
                            Reason = "Điều chỉnh tồn kho sau kiểm kê tháng 12",
                            Notes = "Phát hiện một số chênh lệch trong quá trình kiểm kê"
                        };
                        adjustments.Add(adjustment1);
                    }
                }
            }

            // 2. Adjustment từ check 2 (Approved - đã duyệt)
            if (check2 != null)
            {
                var remainingItems = check2.Items.Where(i => i.IsDiscrepancy && !i.IsAdjusted).Skip(5).Take(3).ToList();
                if (remainingItems.Any())
                {
                    var adj2Code = $"ADJ-{year}-002";
                    if (!existingCodes.Contains(adj2Code))
                    {
                        var adjustment2 = new Core.Entities.InventoryAdjustment
                        {
                            AdjustmentNumber = adj2Code,
                            InventoryCheckId = check2.Id,
                            WarehouseId = check2.WarehouseId,
                            WarehouseZoneId = check2.WarehouseZoneId,
                            WarehouseBinId = check2.WarehouseBinId,
                            AdjustmentDate = DateTime.Now.AddDays(-7),
                            Status = "Approved",
                            Reason = "Điều chỉnh tồn kho - đã được duyệt",
                            Notes = "Điều chỉnh các items còn lại",
                            ApprovedByEmployeeId = employees.First().Id,
                            ApprovedAt = DateTime.Now.AddDays(-7).AddHours(2)
                        };
                        adjustments.Add(adjustment2);
                    }
                }
            }

            // 3. Adjustment từ check 2 (Rejected - bị từ chối)
            // ✅ FIX: check2 có 8 items (0-7), adj1 lấy 5 items (0-4), adj2 lấy 3 items (5-7)
            // Không còn items nào cho adj3, nên sẽ không tạo adj3 nếu không đủ items
            if (check2 != null)
            {
                // Reload check2 với items để đảm bảo có đủ items
                var check2WithItems = await _context.InventoryChecks
                    .Where(ic => ic.Id == check2.Id)
                    .Include(ic => ic.Items.Where(i => !i.IsDeleted && i.IsDiscrepancy && !i.IsAdjusted))
                    .ThenInclude(i => i.Part)
                    .FirstOrDefaultAsync();
                
                if (check2WithItems != null)
                {
                    check2 = check2WithItems;
                }
                
                var remainingItemsForReject = check2.Items.Where(i => i.IsDiscrepancy && !i.IsAdjusted).Skip(8).Take(2).ToList();
                if (remainingItemsForReject.Any())
                {
                    var adj3Code = $"ADJ-{year}-003";
                    if (!existingCodes.Contains(adj3Code))
                    {
                        var adjustment3 = new Core.Entities.InventoryAdjustment
                        {
                            AdjustmentNumber = adj3Code,
                            InventoryCheckId = check2.Id,
                            WarehouseId = check2.WarehouseId,
                            WarehouseZoneId = check2.WarehouseZoneId,
                            WarehouseBinId = check2.WarehouseBinId,
                            AdjustmentDate = DateTime.Now.AddDays(-6),
                            Status = "Rejected",
                            Reason = "Điều chỉnh tồn kho - bị từ chối",
                            Notes = "Điều chỉnh không hợp lý, cần kiểm tra lại",
                            RejectionReason = "Số lượng chênh lệch quá lớn, cần kiểm tra lại thực tế"
                        };
                        adjustments.Add(adjustment3);
                    }
                }
            }

            // 4. Manual Adjustment (không từ check)
            var adj4Code = $"ADJ-{year}-004";
            if (!existingCodes.Contains(adj4Code))
            {
                var mainWarehouse = await _context.Warehouses
                    .Where(w => !w.IsDeleted && w.IsDefault)
                    .FirstOrDefaultAsync() ?? await _context.Warehouses.Where(w => !w.IsDeleted).FirstOrDefaultAsync();

                if (mainWarehouse != null)
                {
                    var parts = await _context.Parts
                        .Where(p => !p.IsDeleted && p.IsActive && p.QuantityInStock > 0)
                        .Take(3)
                        .ToListAsync();

                    if (parts.Any())
                    {
                        var adjustment4 = new Core.Entities.InventoryAdjustment
                        {
                            AdjustmentNumber = adj4Code,
                            InventoryCheckId = null,
                            WarehouseId = mainWarehouse.Id,
                            AdjustmentDate = DateTime.Now.AddDays(-3),
                            Status = "Pending",
                            Reason = "Điều chỉnh thủ công - hàng hỏng",
                            Notes = "Phát hiện hàng hỏng trong kho, cần điều chỉnh"
                        };
                        adjustments.Add(adjustment4);
                    }
                }
            }

            // Tạo adjustments
            foreach (var adjustment in adjustments)
            {
                await _unitOfWork.InventoryAdjustments.AddAsync(adjustment);
            }
            await _unitOfWork.SaveChangesAsync();

            // Reload adjustments để có IDs
            var createdAdjustments = await _context.InventoryAdjustments
                .Where(ia => adjustments.Select(a => a.AdjustmentNumber).Contains(ia.AdjustmentNumber))
                .ToListAsync();

            // ✅ FIX: Reload check2 với items để đảm bảo có đủ items và Part data
            if (check2 != null)
            {
                check2 = await _context.InventoryChecks
                    .Where(ic => ic.Id == check2.Id)
                    .Include(ic => ic.Items.Where(i => !i.IsDeleted && i.IsDiscrepancy && !i.IsAdjusted))
                    .ThenInclude(i => i.Part)
                    .FirstOrDefaultAsync();
            }

            // Tạo items cho adjustments
            var adj1 = createdAdjustments.FirstOrDefault(a => a.AdjustmentNumber.Contains("001"));
            if (adj1 != null && check2 != null)
            {
                var itemsForAdj1 = check2.Items.Where(i => i.IsDiscrepancy && !i.IsAdjusted).Take(5).ToList();
                foreach (var checkItem in itemsForAdj1)
                {
                    var part = checkItem.Part;
                    if (part == null) continue;

                    // ✅ FIX: Sử dụng SystemQuantity từ check item (giá trị tại thời điểm check)
                    var qtyChange = checkItem.DiscrepancyQuantity;
                    var systemQtyBefore = checkItem.SystemQuantity;
                    var systemQtyAfter = Math.Max(0, checkItem.ActualQuantity); // ✅ FIX: Đảm bảo không âm

                    var adjItem = new Core.Entities.InventoryAdjustmentItem
                    {
                        InventoryAdjustmentId = adj1.Id,
                        PartId = part.Id,
                        InventoryCheckItemId = checkItem.Id,
                        QuantityChange = qtyChange,
                        SystemQuantityBefore = systemQtyBefore,
                        SystemQuantityAfter = systemQtyAfter,
                        Notes = $"Điều chỉnh từ kiểm kê {check2.Code}"
                    };
                    allAdjustmentItems.Add(adjItem);
                }
            }

            var adj2 = createdAdjustments.FirstOrDefault(a => a.AdjustmentNumber.Contains("002"));
            if (adj2 != null && check2 != null)
            {
                var itemsForAdj2 = check2.Items.Where(i => i.IsDiscrepancy && !i.IsAdjusted).Skip(5).Take(3).ToList();
                foreach (var checkItem in itemsForAdj2)
                {
                    var part = checkItem.Part;
                    if (part == null) continue;

                    // ✅ FIX: Sử dụng SystemQuantity từ check item (giá trị tại thời điểm check)
                    var qtyChange = checkItem.DiscrepancyQuantity;
                    var systemQtyBefore = checkItem.SystemQuantity;
                    var systemQtyAfter = Math.Max(0, checkItem.ActualQuantity); // ✅ FIX: Đảm bảo không âm

                    var adjItem = new Core.Entities.InventoryAdjustmentItem
                    {
                        InventoryAdjustmentId = adj2.Id,
                        PartId = part.Id,
                        InventoryCheckItemId = checkItem.Id,
                        QuantityChange = qtyChange,
                        SystemQuantityBefore = systemQtyBefore,
                        SystemQuantityAfter = systemQtyAfter,
                        Notes = $"Điều chỉnh từ kiểm kê {check2.Code} - đã duyệt"
                    };
                    allAdjustmentItems.Add(adjItem);
                }
            }

            var adj3 = createdAdjustments.FirstOrDefault(a => a.AdjustmentNumber.Contains("003"));
            if (adj3 != null && check2 != null)
            {
                var itemsForAdj3 = check2.Items.Where(i => i.IsDiscrepancy && !i.IsAdjusted).Skip(8).Take(2).ToList();
                foreach (var checkItem in itemsForAdj3)
                {
                    var part = checkItem.Part;
                    if (part == null) continue;

                    var qtyChange = checkItem.DiscrepancyQuantity;
                    var systemQtyBefore = checkItem.SystemQuantity;
                    var systemQtyAfter = Math.Max(0, checkItem.ActualQuantity); // ✅ FIX: Đảm bảo không âm

                    var adjItem = new Core.Entities.InventoryAdjustmentItem
                    {
                        InventoryAdjustmentId = adj3.Id,
                        PartId = part.Id,
                        InventoryCheckItemId = checkItem.Id,
                        QuantityChange = qtyChange,
                        SystemQuantityBefore = systemQtyBefore,
                        SystemQuantityAfter = systemQtyAfter,
                        Notes = $"Điều chỉnh từ kiểm kê {check2.Code} - bị từ chối"
                    };
                    allAdjustmentItems.Add(adjItem);
                }
            }

            var adj4 = createdAdjustments.FirstOrDefault(a => a.AdjustmentNumber.Contains("004"));
            if (adj4 != null)
            {
                var parts = await _context.Parts
                    .Where(p => !p.IsDeleted && p.IsActive && p.QuantityInStock > 0)
                    .Take(3)
                    .ToListAsync();

                foreach (var part in parts)
                {
                    var qtyChange = -2; // Giảm 2 do hỏng
                    var systemQtyBefore = part.QuantityInStock;
                    var systemQtyAfter = Math.Max(0, systemQtyBefore + qtyChange);

                    var adjItem = new Core.Entities.InventoryAdjustmentItem
                    {
                        InventoryAdjustmentId = adj3.Id,
                        PartId = part.Id,
                        InventoryCheckItemId = null, // Manual adjustment
                        QuantityChange = qtyChange,
                        SystemQuantityBefore = systemQtyBefore,
                        SystemQuantityAfter = systemQtyAfter,
                        Notes = "Hàng hỏng, cần loại bỏ"
                    };
                    allAdjustmentItems.Add(adjItem);
                }
            }

            // Tạo adjustment items
            foreach (var item in allAdjustmentItems)
            {
                await _unitOfWork.InventoryAdjustmentItems.AddAsync(item);
            }
            await _unitOfWork.SaveChangesAsync();

            // Update check items để link với adjustment items
            // ✅ Lưu ý: Chỉ mark IsAdjusted = true cho Pending và Approved, không mark cho Rejected
            if (adj1 != null && check2 != null)
            {
                var adj1Items = await _context.InventoryAdjustmentItems
                    .Where(iai => iai.InventoryAdjustmentId == adj1.Id)
                    .ToListAsync();
                
                foreach (var adjItem in adj1Items)
                {
                    if (adjItem.InventoryCheckItemId.HasValue)
                    {
                        var checkItem = await _unitOfWork.InventoryCheckItems.GetByIdAsync(adjItem.InventoryCheckItemId.Value);
                        if (checkItem != null)
                        {
                            checkItem.InventoryAdjustmentItemId = adjItem.Id;
                            // ✅ Pending adjustment: Không mark IsAdjusted = true (chờ duyệt)
                            // checkItem.IsAdjusted = false; // Giữ nguyên false
                            await _unitOfWork.InventoryCheckItems.UpdateAsync(checkItem);
                        }
                    }
                }
            }

            if (adj2 != null && check2 != null)
            {
                var adj2Items = await _context.InventoryAdjustmentItems
                    .Where(iai => iai.InventoryAdjustmentId == adj2.Id)
                    .ToListAsync();
                
                foreach (var adjItem in adj2Items)
                {
                    if (adjItem.InventoryCheckItemId.HasValue)
                    {
                        var checkItem = await _unitOfWork.InventoryCheckItems.GetByIdAsync(adjItem.InventoryCheckItemId.Value);
                        if (checkItem != null)
                        {
                            checkItem.InventoryAdjustmentItemId = adjItem.Id;
                            checkItem.IsAdjusted = true; // ✅ Approved: Mark IsAdjusted = true
                            await _unitOfWork.InventoryCheckItems.UpdateAsync(checkItem);
                        }
                    }
                }
            }

            // ✅ Rejected adjustment: Link nhưng không mark IsAdjusted = true (vì bị reject)
            if (adj3 != null && check2 != null)
            {
                var adj3Items = await _context.InventoryAdjustmentItems
                    .Where(iai => iai.InventoryAdjustmentId == adj3.Id)
                    .ToListAsync();
                
                foreach (var adjItem in adj3Items)
                {
                    if (adjItem.InventoryCheckItemId.HasValue)
                    {
                        var checkItem = await _unitOfWork.InventoryCheckItems.GetByIdAsync(adjItem.InventoryCheckItemId.Value);
                        if (checkItem != null)
                        {
                            checkItem.InventoryAdjustmentItemId = adjItem.Id;
                            // ✅ Rejected: Không mark IsAdjusted = true (có thể tạo adjustment mới)
                            // checkItem.IsAdjusted = false; // Giữ nguyên false
                            await _unitOfWork.InventoryCheckItems.UpdateAsync(checkItem);
                        }
                    }
                }
            }

            // Update stock và tạo StockTransaction cho approved adjustment (adj2)
            if (adj2 != null && adj2.Status == "Approved")
            {
                var adj2Items = await _context.InventoryAdjustmentItems
                    .Where(iai => iai.InventoryAdjustmentId == adj2.Id)
                    .Include(iai => iai.Part)
                    .ToListAsync();

                // Generate transaction number prefix
                var yearForTransaction = DateTime.Now.Year;
                var transactionPrefix = $"STK-{yearForTransaction}";
                var lastTransaction = await _context.StockTransactions
                    .Where(st => st.TransactionNumber.StartsWith(transactionPrefix))
                    .OrderByDescending(st => st.TransactionNumber)
                    .FirstOrDefaultAsync();

                int nextTransactionNumber = 1;
                if (lastTransaction != null)
                {
                    var parts = lastTransaction.TransactionNumber.Split('-');
                    if (parts.Length >= 3 && int.TryParse(parts[2], out var lastNumber))
                    {
                        nextTransactionNumber = lastNumber + 1;
                    }
                }

                foreach (var adjItem in adj2Items)
                {
                    if (adjItem.Part != null)
                    {
                        // Update part stock
                        adjItem.Part.QuantityInStock = adjItem.SystemQuantityAfter;
                        adjItem.Part.UpdatedAt = DateTime.Now;
                        await _unitOfWork.Parts.UpdateAsync(adjItem.Part);

                        // Create StockTransaction
                        var transaction = new Core.Entities.StockTransaction
                        {
                            TransactionNumber = $"{transactionPrefix}-{nextTransactionNumber:D3}",
                            PartId = adjItem.PartId,
                            TransactionType = adjItem.QuantityChange > 0 
                                ? Core.Enums.StockTransactionType.NhapKho 
                                : Core.Enums.StockTransactionType.XuatKho,
                            Quantity = Math.Abs(adjItem.QuantityChange),
                            UnitCost = adjItem.Part.CostPrice,
                            UnitPrice = adjItem.Part.SellPrice,
                            TotalCost = Math.Abs(adjItem.QuantityChange) * adjItem.Part.CostPrice,
                            TotalAmount = Math.Abs(adjItem.QuantityChange) * adjItem.Part.SellPrice,
                            TransactionDate = adj2.ApprovedAt ?? DateTime.Now,
                            ReferenceNumber = adj2.AdjustmentNumber,
                            RelatedEntity = "InventoryAdjustment",
                            RelatedEntityId = adj2.Id,
                            Notes = $"Điều chỉnh tồn kho: {(adjItem.QuantityChange > 0 ? "Tăng" : "Giảm")} {Math.Abs(adjItem.QuantityChange)}. {adjItem.Notes ?? ""}",
                            QuantityBefore = adjItem.SystemQuantityBefore,
                            QuantityAfter = adjItem.SystemQuantityAfter,
                            StockAfter = adjItem.SystemQuantityAfter,
                            ProcessedById = adj2.ApprovedByEmployeeId
                        };

                        await _unitOfWork.StockTransactions.AddAsync(transaction);
                        nextTransactionNumber++;
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();

            return new 
            { 
                success = true, 
                adjustmentCount = adjustments.Count, 
                itemCount = allAdjustmentItems.Count,
                message = $"Đã tạo {adjustments.Count} phiếu điều chỉnh với {allAdjustmentItems.Count} items" 
            };
        }

        private async Task<object> CreateLaborCategoriesAsync()
        {
            var existingCategories = await _context.Set<Core.Entities.LaborCategory>()
                .Where(c => !c.IsDeleted)
                .Select(c => c.CategoryCode)
                .ToListAsync();

            var categoriesToCreate = new List<Core.Entities.LaborCategory>();

            var categories = new[]
            {
                new { Code = "REMOVE_INSTALL", Name = "Công tháo lắp", BaseRate = 50000m },
                new { Code = "REPAIR", Name = "Công sửa chữa", BaseRate = 80000m },
                new { Code = "PAINT", Name = "Công sơn", BaseRate = 100000m },
                new { Code = "DIAGNOSIS", Name = "Công chẩn đoán", BaseRate = 60000m },
                new { Code = "MAINTENANCE", Name = "Công bảo dưỡng", BaseRate = 40000m }
            };

            foreach (var cat in categories)
            {
                if (!existingCategories.Contains(cat.Code))
                {
                    categoriesToCreate.Add(new Core.Entities.LaborCategory
                    {
                        CategoryCode = cat.Code,
                        CategoryName = cat.Name,
                        BaseRate = cat.BaseRate,
                        StandardRate = cat.BaseRate,
                        Description = $"Danh mục công lao động: {cat.Name}",
                        IsActive = true
                    });
                }
            }

            if (categoriesToCreate.Any())
            {
                foreach (var category in categoriesToCreate)
                {
                    await _unitOfWork.Repository<Core.Entities.LaborCategory>().AddAsync(category);
                }
                await _unitOfWork.SaveChangesAsync();
            }

            return new { success = true, count = categoriesToCreate.Count, message = $"Đã tạo {categoriesToCreate.Count} danh mục công lao động" };
        }

        private async Task<object> CreateLaborItemsAsync()
        {
            var laborCategories = (await _unitOfWork.Repository<Core.Entities.LaborCategory>().GetAllAsync()).ToList();
            if (!laborCategories.Any())
            {
                return new { success = false, message = "Cần tạo LaborCategories trước khi tạo LaborItems" };
            }

            var existingItems = await _context.Set<Core.Entities.LaborItem>()
                .Where(i => !i.IsDeleted)
                .Select(i => i.LaborCode)
                .ToListAsync();

            var itemsToCreate = new List<Core.Entities.LaborItem>();

            var removeInstallCategory = laborCategories.FirstOrDefault(c => c.CategoryCode == "REMOVE_INSTALL") ?? laborCategories.First();
            var repairCategory = laborCategories.FirstOrDefault(c => c.CategoryCode == "REPAIR") ?? laborCategories.First();
            var paintCategory = laborCategories.FirstOrDefault(c => c.CategoryCode == "PAINT") ?? laborCategories.First();
            var diagnosisCategory = laborCategories.FirstOrDefault(c => c.CategoryCode == "DIAGNOSIS") ?? laborCategories.First();
            var maintenanceCategory = laborCategories.FirstOrDefault(c => c.CategoryCode == "MAINTENANCE") ?? laborCategories.First();

            var laborItems = new[]
            {
                new { Code = "LAB-001", Name = "Công tháo đèn pha", Category = removeInstallCategory, Hours = 1.5m, Rate = 50000m, PartName = "Đèn pha" },
                new { Code = "LAB-002", Name = "Công thay nhớt", Category = maintenanceCategory, Hours = 0.5m, Rate = 40000m, PartName = "Nhớt động cơ" },
                new { Code = "LAB-003", Name = "Công sửa chữa phanh", Category = repairCategory, Hours = 2.0m, Rate = 80000m, PartName = "Hệ thống phanh" },
                new { Code = "LAB-004", Name = "Công sơn cản", Category = paintCategory, Hours = 3.0m, Rate = 100000m, PartName = "Cản xe" },
                new { Code = "LAB-005", Name = "Công chẩn đoán lỗi động cơ", Category = diagnosisCategory, Hours = 1.0m, Rate = 60000m, PartName = "Động cơ" },
                new { Code = "LAB-006", Name = "Công thay lốp", Category = removeInstallCategory, Hours = 0.5m, Rate = 50000m, PartName = "Lốp xe" },
                new { Code = "LAB-007", Name = "Công sửa chữa điều hòa", Category = repairCategory, Hours = 2.5m, Rate = 80000m, PartName = "Hệ thống điều hòa" }
            };

            foreach (var item in laborItems)
            {
                if (!existingItems.Contains(item.Code))
                {
                    var totalCost = item.Hours * item.Rate;
                    itemsToCreate.Add(new Core.Entities.LaborItem
                    {
                        LaborCode = item.Code,
                        ItemName = item.Name,
                        LaborName = item.Name,
                        PartName = item.PartName,
                        LaborCategoryId = item.Category.Id,
                        CategoryId = item.Category.Id,
                        StandardHours = item.Hours,
                        LaborRate = item.Rate,
                        TotalLaborCost = totalCost,
                        SkillLevel = "Trung bình",
                        Difficulty = "Trung bình",
                        IsActive = true
                    });
                }
            }

            if (itemsToCreate.Any())
            {
                foreach (var item in itemsToCreate)
                {
                    await _unitOfWork.Repository<Core.Entities.LaborItem>().AddAsync(item);
                }
                await _unitOfWork.SaveChangesAsync();
            }

            return new { success = true, count = itemsToCreate.Count, message = $"Đã tạo {itemsToCreate.Count} công lao động" };
        }

        private async Task<object> CreateServiceOrderLaborsAsync()
        {
            var serviceOrders = (await _unitOfWork.ServiceOrders.GetAllAsync()).ToList();
            var laborItems = (await _unitOfWork.Repository<Core.Entities.LaborItem>().GetAllAsync()).ToList();
            var employees = (await _unitOfWork.Employees.GetAllAsync()).ToList();

            if (!serviceOrders.Any() || !laborItems.Any() || !employees.Any())
            {
                return new { success = false, message = "Cần tạo ServiceOrders, LaborItems và Employees trước khi tạo ServiceOrderLabors" };
            }

            var existingLabors = await _context.Set<Core.Entities.ServiceOrderLabor>()
                .Where(l => !l.IsDeleted)
                .CountAsync();

            if (existingLabors > 0)
            {
                return new { success = true, count = 0, message = "Đã có ServiceOrderLabors, bỏ qua" };
            }

            var labors = new List<Core.Entities.ServiceOrderLabor>();

            // Tạo labors cho từng service order
            foreach (var order in serviceOrders.Take(3))
            {
                var laborItem = laborItems[labors.Count % laborItems.Count];
                var employee = employees[labors.Count % employees.Count];
                var statuses = new[] { "Completed", "InProgress", "Pending" };
                var status = statuses[labors.Count % statuses.Length];

                var labor = new Core.Entities.ServiceOrderLabor
                {
                    ServiceOrderId = order.Id,
                    LaborItemId = laborItem.Id,
                    EmployeeId = employee.Id,
                    EstimatedHours = laborItem.StandardHours,
                    ActualHours = laborItem.StandardHours + (decimal)(new Random().NextDouble() * 0.5 - 0.25), // ±0.25 hours
                    LaborRate = laborItem.LaborRate,
                    TotalLaborCost = laborItem.StandardHours * laborItem.LaborRate,
                    Status = status,
                    Notes = $"Công lao động: {laborItem.ItemName}",
                    StartTime = status != "Pending" ? DateTime.Now.AddHours(-(labors.Count + 1) * 2) : null,
                    EndTime = status == "Completed" ? DateTime.Now.AddHours(-labors.Count) : null,
                    CompletedTime = status == "Completed" ? DateTime.Now.AddHours(-labors.Count) : null
                };

                // Recalculate total cost based on actual hours
                labor.TotalLaborCost = labor.ActualHours * labor.LaborRate;

                labors.Add(labor);
            }

            foreach (var labor in labors)
            {
                await _unitOfWork.Repository<Core.Entities.ServiceOrderLabor>().AddAsync(labor);
            }
            await _unitOfWork.SaveChangesAsync();

            return new { success = true, count = labors.Count, message = $"Đã tạo {labors.Count} công lao động cho service orders" };
        }

        private async Task<object> CreateMaterialRequestsAsync()
        {
            var serviceOrders = (await _unitOfWork.ServiceOrders.GetAllAsync()).ToList();
            var employees = (await _unitOfWork.Employees.GetAllAsync()).ToList();
            var parts = (await _unitOfWork.Parts.GetAllAsync()).ToList();

            if (!serviceOrders.Any() || !employees.Any() || !parts.Any())
            {
                return new { success = false, message = "Cần tạo ServiceOrders, Employees và Parts trước khi tạo MaterialRequests" };
            }

            var existingRequests = await _context.Set<Core.Entities.MaterialRequest>()
                .Where(mr => !mr.IsDeleted)
                .CountAsync();

            if (existingRequests > 0)
            {
                return new { success = true, count = 0, message = "Đã có MaterialRequests, bỏ qua" };
            }

            var materialRequests = new List<Core.Entities.MaterialRequest>();
            var requestItems = new List<Core.Entities.MaterialRequestItem>();
            var today = DateTime.Now;
            var mrCounter = 1;

            foreach (var order in serviceOrders.Take(2))
            {
                var requestedBy = employees[new Random().Next(employees.Count)];
                var approvedBy = employees.Count > 1 ? employees[new Random().Next(employees.Count)] : requestedBy;
                var issuedBy = employees.Count > 2 ? employees[new Random().Next(employees.Count)] : approvedBy;

                var mrNumber = $"MR-{today:yyyyMMdd}-{mrCounter:D4}";
                mrCounter++;

                var statuses = new[] { 
                    Core.Enums.MaterialRequestStatus.Approved, 
                    Core.Enums.MaterialRequestStatus.PendingApproval 
                };
                var status = statuses[new Random().Next(statuses.Length)];

                var materialRequest = new Core.Entities.MaterialRequest
                {
                    MRNumber = mrNumber,
                    ServiceOrderId = order.Id,
                    RequestedById = requestedBy.Id,
                    ApprovedById = status == Core.Enums.MaterialRequestStatus.Approved ? approvedBy.Id : null,
                    IssuedById = status == Core.Enums.MaterialRequestStatus.Approved ? issuedBy.Id : null,
                    Status = status,
                    ApprovedAt = status == Core.Enums.MaterialRequestStatus.Approved ? today.AddHours(-2) : null,
                    IssuedAt = status == Core.Enums.MaterialRequestStatus.Approved ? today.AddHours(-1) : null,
                    Notes = $"Yêu cầu vật tư cho đơn hàng {order.OrderNumber}"
                };

                materialRequests.Add(materialRequest);
            }

            // Save material requests first to get IDs
            foreach (var mr in materialRequests)
            {
                await _unitOfWork.Repository<Core.Entities.MaterialRequest>().AddAsync(mr);
            }
            await _unitOfWork.SaveChangesAsync();

            // Create items for each material request
            foreach (var mr in materialRequests)
            {
                var selectedParts = parts.Take(2).ToList();
                foreach (var part in selectedParts)
                {
                    var quantity = new Random().Next(1, 4);
                    var item = new Core.Entities.MaterialRequestItem
                    {
                        MaterialRequestId = mr.Id,
                        PartId = part.Id,
                        QuantityRequested = quantity,
                        QuantityApproved = mr.Status == Core.Enums.MaterialRequestStatus.Approved ? quantity : 0,
                        QuantityPicked = mr.Status == Core.Enums.MaterialRequestStatus.Approved ? quantity : 0,
                        QuantityIssued = mr.Status == Core.Enums.MaterialRequestStatus.Approved ? quantity : 0,
                        PartName = part.PartName,
                        Notes = $"Yêu cầu {quantity} {part.DefaultUnit}"
                    };
                    requestItems.Add(item);
                }
            }

            foreach (var item in requestItems)
            {
                await _unitOfWork.Repository<Core.Entities.MaterialRequestItem>().AddAsync(item);
            }
            await _unitOfWork.SaveChangesAsync();

            return new { success = true, requestCount = materialRequests.Count, itemCount = requestItems.Count, message = $"Đã tạo {materialRequests.Count} yêu cầu vật tư với {requestItems.Count} items" };
        }

        private async Task<object> CreateServiceFeeTypesAsync()
        {
            var existingTypes = await _context.Set<Core.Entities.ServiceFeeType>()
                .Where(t => !t.IsDeleted)
                .Select(t => t.Code)
                .ToListAsync();

            var typesToCreate = new List<Core.Entities.ServiceFeeType>();

            var feeTypes = new[]
            {
                new { Code = "LABOR", Name = "Phí lao động", Description = "Phí công lao động sửa chữa", Order = 1 },
                new { Code = "PARTS", Name = "Phí phụ tùng", Description = "Phí phụ tùng thay thế", Order = 2 },
                new { Code = "SURCHARGE", Name = "Phụ phí", Description = "Phụ phí khác", Order = 3 },
                new { Code = "DISCOUNT", Name = "Giảm giá", Description = "Giảm giá dịch vụ", Order = 4 },
                new { Code = "VAT", Name = "VAT", Description = "Thuế giá trị gia tăng", Order = 5 }
            };

            foreach (var type in feeTypes)
            {
                if (!existingTypes.Contains(type.Code))
                {
                    typesToCreate.Add(new Core.Entities.ServiceFeeType
                    {
                        Code = type.Code,
                        Name = type.Name,
                        Description = type.Description,
                        DisplayOrder = type.Order,
                        IsActive = true,
                        IsSystem = type.Code == "VAT" || type.Code == "DISCOUNT"
                    });
                }
            }

            if (typesToCreate.Any())
            {
                foreach (var type in typesToCreate)
                {
                    await _unitOfWork.ServiceFeeTypes.AddAsync(type);
                }
                await _unitOfWork.SaveChangesAsync();
            }

            return new { success = true, count = typesToCreate.Count, message = $"Đã tạo {typesToCreate.Count} loại phí dịch vụ" };
        }

        private async Task<object> CreateServiceOrderFeesAsync()
        {
            var serviceOrders = (await _unitOfWork.ServiceOrders.GetAllAsync()).ToList();
            var feeTypes = (await _unitOfWork.ServiceFeeTypes.GetAllAsync()).ToList();

            if (!serviceOrders.Any() || !feeTypes.Any())
            {
                return new { success = false, message = "Cần tạo ServiceOrders và ServiceFeeTypes trước khi tạo ServiceOrderFees" };
            }

            var existingFees = await _context.Set<Core.Entities.ServiceOrderFee>()
                .Where(f => !f.IsDeleted)
                .CountAsync();

            if (existingFees > 0)
            {
                return new { success = true, count = 0, message = "Đã có ServiceOrderFees, bỏ qua" };
            }

            var fees = new List<Core.Entities.ServiceOrderFee>();

            var laborFeeType = feeTypes.FirstOrDefault(t => t.Code == "LABOR") ?? feeTypes.First();
            var partsFeeType = feeTypes.FirstOrDefault(t => t.Code == "PARTS") ?? feeTypes.First();
            var surchargeFeeType = feeTypes.FirstOrDefault(t => t.Code == "SURCHARGE") ?? feeTypes.First();

            foreach (var order in serviceOrders.Take(3))
            {
                // Labor fee
                fees.Add(new Core.Entities.ServiceOrderFee
                {
                    ServiceOrderId = order.Id,
                    ServiceFeeTypeId = laborFeeType.Id,
                    Amount = order.ServiceTotal,
                    VatAmount = 0,
                    DiscountAmount = 0,
                    Notes = "Phí lao động",
                    IsManual = false
                });

                // Parts fee
                fees.Add(new Core.Entities.ServiceOrderFee
                {
                    ServiceOrderId = order.Id,
                    ServiceFeeTypeId = partsFeeType.Id,
                    Amount = order.PartsTotal,
                    VatAmount = 0,
                    DiscountAmount = 0,
                    Notes = "Phí phụ tùng",
                    IsManual = false
                });

                // Surcharge (optional, only for some orders)
                if (order.Id % 2 == 0)
                {
                    fees.Add(new Core.Entities.ServiceOrderFee
                    {
                        ServiceOrderId = order.Id,
                        ServiceFeeTypeId = surchargeFeeType.Id,
                        Amount = 50000,
                        VatAmount = 0,
                        DiscountAmount = 0,
                        Notes = "Phụ phí khẩn cấp",
                        IsManual = true
                    });
                }
            }

            foreach (var fee in fees)
            {
                await _unitOfWork.ServiceOrderFees.AddAsync(fee);
            }
            await _unitOfWork.SaveChangesAsync();

            return new { success = true, count = fees.Count, message = $"Đã tạo {fees.Count} phí dịch vụ cho service orders" };
        }

        private async Task<object> CreateWarrantiesAsync()
        {
            var serviceOrders = (await _unitOfWork.ServiceOrders.GetAllAsync()).ToList();
            var customers = (await _unitOfWork.Customers.GetAllAsync()).ToList();
            var vehicles = (await _unitOfWork.Vehicles.GetAllAsync()).ToList();
            var serviceOrderParts = await _context.Set<Core.Entities.ServiceOrderPart>()
                .Where(sop => !sop.IsDeleted)
                .ToListAsync();

            if (!serviceOrders.Any() || !customers.Any() || !vehicles.Any())
            {
                return new { success = false, message = "Cần tạo ServiceOrders, Customers và Vehicles trước khi tạo Warranties" };
            }

            var existingWarranties = await _unitOfWork.Warranties.CountAsync();
            if (existingWarranties > 0)
            {
                return new { success = true, count = 0, message = "Đã có Warranties, bỏ qua" };
            }

            var warranties = new List<Core.Entities.Warranty>();
            var warrantyItems = new List<Core.Entities.WarrantyItem>();
            var warrantyCounter = 1;
            var today = DateTime.Now;

            foreach (var order in serviceOrders.Where(o => o.Status == "Completed").Take(2))
            {
                var customer = customers.FirstOrDefault(c => c.Id == order.CustomerId) ?? customers.First();
                var vehicle = vehicles.FirstOrDefault(v => v.Id == order.VehicleId) ?? vehicles.First();
                var warrantyCode = $"WR-{today:yyyyMMdd}-{warrantyCounter:D4}";
                warrantyCounter++;

                var warrantyMonths = 12; // 12 months warranty
                var warrantyStartDate = order.CompletedDate ?? today.AddDays(-30);
                var warrantyEndDate = warrantyStartDate.AddMonths(warrantyMonths);

                var warranty = new Core.Entities.Warranty
                {
                    WarrantyCode = warrantyCode,
                    ServiceOrderId = order.Id,
                    CustomerId = customer.Id,
                    VehicleId = vehicle.Id,
                    WarrantyStartDate = warrantyStartDate,
                    WarrantyEndDate = warrantyEndDate,
                    Status = warrantyEndDate > today ? "Active" : "Expired",
                    Notes = $"Bảo hành cho đơn hàng {order.OrderNumber}",
                    HandoverBy = "Nhân viên bảo hành",
                    HandoverLocation = "Garage chính"
                };

                warranties.Add(warranty);
            }

            // Save warranties first to get IDs
            foreach (var warranty in warranties)
            {
                await _unitOfWork.Warranties.AddAsync(warranty);
            }
            await _unitOfWork.SaveChangesAsync();

            // Create warranty items for each warranty
            foreach (var warranty in warranties)
            {
                var orderParts = serviceOrderParts.Where(sop => sop.ServiceOrderId == warranty.ServiceOrderId).Take(2).ToList();
                if (!orderParts.Any())
                {
                    // Create warranty items without ServiceOrderPart reference
                    var parts = (await _unitOfWork.Parts.GetAllAsync()).Take(2).ToList();
                    foreach (var part in parts)
                    {
                        var warrantyItem = new Core.Entities.WarrantyItem
                        {
                            WarrantyId = warranty.Id,
                            PartId = part.Id,
                            PartName = part.PartName,
                            PartNumber = part.PartNumber,
                            WarrantyMonths = 12,
                            WarrantyStartDate = warranty.WarrantyStartDate,
                            WarrantyEndDate = warranty.WarrantyEndDate,
                            Status = "Active",
                            Notes = $"Bảo hành {part.PartName}"
                        };
                        warrantyItems.Add(warrantyItem);
                    }
                }
                else
                {
                    foreach (var orderPart in orderParts)
                    {
                        var warrantyItem = new Core.Entities.WarrantyItem
                        {
                            WarrantyId = warranty.Id,
                            ServiceOrderPartId = orderPart.Id,
                            PartId = orderPart.PartId,
                            PartName = orderPart.PartName,
                            WarrantyMonths = 12,
                            WarrantyStartDate = warranty.WarrantyStartDate,
                            WarrantyEndDate = warranty.WarrantyEndDate,
                            Status = "Active",
                            Notes = $"Bảo hành {orderPart.PartName}"
                        };
                        warrantyItems.Add(warrantyItem);
                    }
                }
            }

            foreach (var item in warrantyItems)
            {
                await _unitOfWork.WarrantyItems.AddAsync(item);
            }
            await _unitOfWork.SaveChangesAsync();

            return new { success = true, warrantyCount = warranties.Count, itemCount = warrantyItems.Count, message = $"Đã tạo {warranties.Count} bảo hành với {warrantyItems.Count} items" };
        }

        private async Task<object> CreateFeedbackChannelsAsync()
        {
            var existingChannels = await _unitOfWork.FeedbackChannels.GetAllAsync();
            var existingCodes = existingChannels.Select(c => c.Code).ToList();

            var channelsToCreate = new List<Core.Entities.FeedbackChannel>();

            var channels = new[]
            {
                new { Code = "HOTLINE", Name = "Hotline", Description = "Phản hồi qua điện thoại", Order = 1 },
                new { Code = "EMAIL", Name = "Email", Description = "Phản hồi qua email", Order = 2 },
                new { Code = "IN_PERSON", Name = "Trực tiếp", Description = "Phản hồi trực tiếp tại garage", Order = 3 },
                new { Code = "MOBILE_APP", Name = "Ứng dụng di động", Description = "Phản hồi qua ứng dụng", Order = 4 },
                new { Code = "WEBSITE", Name = "Website", Description = "Phản hồi qua website", Order = 5 },
                new { Code = "SOCIAL_MEDIA", Name = "Mạng xã hội", Description = "Phản hồi qua mạng xã hội", Order = 6 }
            };

            foreach (var channel in channels)
            {
                if (!existingCodes.Contains(channel.Code))
                {
                    channelsToCreate.Add(new Core.Entities.FeedbackChannel
                    {
                        Code = channel.Code,
                        Name = channel.Name,
                        Description = channel.Description,
                        DisplayOrder = channel.Order,
                        IsActive = true,
                        IsSystem = true
                    });
                }
            }

            if (channelsToCreate.Any())
            {
                foreach (var channel in channelsToCreate)
                {
                    await _unitOfWork.FeedbackChannels.AddAsync(channel);
                }
                await _unitOfWork.SaveChangesAsync();
            }

            return new { success = true, count = channelsToCreate.Count, message = $"Đã tạo {channelsToCreate.Count} kênh phản hồi" };
        }

        private async Task<object> CreateCustomerFeedbacksAsync()
        {
            var customers = (await _unitOfWork.Customers.GetAllAsync()).ToList();
            var serviceOrders = (await _unitOfWork.ServiceOrders.GetAllAsync()).ToList();
            var feedbackChannels = (await _unitOfWork.FeedbackChannels.GetAllAsync()).ToList();
            var employees = (await _unitOfWork.Employees.GetAllAsync()).ToList();

            if (!customers.Any() || !serviceOrders.Any())
            {
                return new { success = false, message = "Cần tạo Customers và ServiceOrders trước khi tạo CustomerFeedbacks" };
            }

            var existingFeedbacks = await _unitOfWork.CustomerFeedbacks.CountAsync();
            if (existingFeedbacks > 0)
            {
                return new { success = true, count = 0, message = "Đã có CustomerFeedbacks, bỏ qua" };
            }

            var feedbacks = new List<Core.Entities.CustomerFeedback>();

            var sources = new[] { 
                Core.Enums.FeedbackSources.Hotline, 
                Core.Enums.FeedbackSources.Email, 
                Core.Enums.FeedbackSources.InPerson,
                Core.Enums.FeedbackSources.MobileApp
            };
            var ratings = new[] { 
                Core.Enums.FeedbackRatings.VerySatisfied, 
                Core.Enums.FeedbackRatings.Satisfied, 
                Core.Enums.FeedbackRatings.Neutral 
            };
            var statuses = new[] { 
                Core.Enums.FeedbackStatuses.New, 
                Core.Enums.FeedbackStatuses.InProgress, 
                Core.Enums.FeedbackStatuses.Resolved 
            };

            var feedbackTopics = new[]
            {
                "Chất lượng dịch vụ",
                "Thái độ nhân viên",
                "Thời gian hoàn thành",
                "Giá cả",
                "Môi trường garage"
            };

            var feedbackContents = new[]
            {
                "Dịch vụ rất tốt, nhân viên chuyên nghiệp",
                "Thời gian hoàn thành đúng hẹn, rất hài lòng",
                "Giá cả hợp lý, chất lượng tốt",
                "Nhân viên nhiệt tình, tư vấn rõ ràng",
                "Garage sạch sẽ, thiết bị hiện đại"
            };

            for (int i = 0; i < Math.Min(5, serviceOrders.Count); i++)
            {
                var order = serviceOrders[i];
                var customer = customers.FirstOrDefault(c => c.Id == order.CustomerId) ?? customers[i % customers.Count];
                var channel = feedbackChannels.Any() ? feedbackChannels[i % feedbackChannels.Count] : null;
                var followUpBy = employees.Any() ? employees[i % employees.Count] : null;

                var feedback = new Core.Entities.CustomerFeedback
                {
                    CustomerId = customer.Id,
                    ServiceOrderId = order.Id,
                    Source = sources[i % sources.Length],
                    Rating = ratings[i % ratings.Length],
                    Topic = feedbackTopics[i % feedbackTopics.Length],
                    Content = feedbackContents[i % feedbackContents.Length],
                    Status = statuses[i % statuses.Length],
                    FollowUpDate = statuses[i % statuses.Length] == Core.Enums.FeedbackStatuses.New ? null : DateTime.Now.AddDays(1),
                    FollowUpById = followUpBy?.Id,
                    FeedbackChannelId = channel?.Id,
                    Score = ratings[i % ratings.Length] == Core.Enums.FeedbackRatings.VerySatisfied ? 5 : 
                            ratings[i % ratings.Length] == Core.Enums.FeedbackRatings.Satisfied ? 4 : 3
                };

                feedbacks.Add(feedback);
            }

            foreach (var feedback in feedbacks)
            {
                await _unitOfWork.CustomerFeedbacks.AddAsync(feedback);
            }
            await _unitOfWork.SaveChangesAsync();

            return new { success = true, count = feedbacks.Count, message = $"Đã tạo {feedbacks.Count} phản hồi khách hàng" };
        }

        #endregion
    }
}
