using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        public SetupController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
                    appointmentCount = await _unitOfWork.Appointments.CountAsync()
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
                // Xóa theo thứ tự để tránh foreign key constraint
                var appointmentTasks = await _unitOfWork.Appointments.GetAllAsync();
                foreach (var item in appointmentTasks) await _unitOfWork.Appointments.DeleteAsync(item);

                var paymentTasks = await _unitOfWork.PaymentTransactions.GetAllAsync();
                foreach (var item in paymentTasks) await _unitOfWork.PaymentTransactions.DeleteAsync(item);

                var orderTasks = await _unitOfWork.ServiceOrders.GetAllAsync();
                foreach (var item in orderTasks) await _unitOfWork.ServiceOrders.DeleteAsync(item);

                var quotationTasks = await _unitOfWork.ServiceQuotations.GetAllAsync();
                foreach (var item in quotationTasks) await _unitOfWork.ServiceQuotations.DeleteAsync(item);

                var inspectionTasks = await _unitOfWork.VehicleInspections.GetAllAsync();
                foreach (var item in inspectionTasks) await _unitOfWork.VehicleInspections.DeleteAsync(item);

                var vehicleTasks = await _unitOfWork.Vehicles.GetAllAsync();
                foreach (var item in vehicleTasks) await _unitOfWork.Vehicles.DeleteAsync(item);

                var customerTasks = await _unitOfWork.Customers.GetAllAsync();
                foreach (var item in customerTasks) await _unitOfWork.Customers.DeleteAsync(item);

                var employeeTasks = await _unitOfWork.Employees.GetAllAsync();
                foreach (var item in employeeTasks) await _unitOfWork.Employees.DeleteAsync(item);

                var serviceTasks = await _unitOfWork.Services.GetAllAsync();
                foreach (var item in serviceTasks) await _unitOfWork.Services.DeleteAsync(item);

                var partTasks = await _unitOfWork.Parts.GetAllAsync();
                foreach (var item in partTasks) await _unitOfWork.Parts.DeleteAsync(item);

                var supplierTasks = await _unitOfWork.Suppliers.GetAllAsync();
                foreach (var item in supplierTasks) await _unitOfWork.Suppliers.DeleteAsync(item);

                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResult(new { message = "Đã xóa tất cả dữ liệu demo thành công" }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult("Lỗi khi xóa dữ liệu demo", ex.Message));
            }
        }

        #region Private Methods - Create Demo Data

        private async Task<object> CreateCustomersAsync()
        {
            var customers = new[]
            {
                new Core.Entities.Customer
                {
                    Name = "Nguyễn Văn An",
                    Email = "an.nguyen@email.com",
                    Phone = "0901234567",
                    Address = "123 Đường ABC, Quận 1, TP.HCM"
                },
                new Core.Entities.Customer
                {
                    Name = "Trần Thị Bình",
                    Email = "binh.tran@email.com",
                    Phone = "0907654321",
                    Address = "456 Đường XYZ, Quận 2, TP.HCM"
                },
                new Core.Entities.Customer
                {
                    Name = "Lê Văn Cường",
                    Email = "cuong.le@email.com",
                    Phone = "0909876543",
                    Address = "789 Đường DEF, Quận 3, TP.HCM"
                }
            };

            foreach (var customer in customers)
            {
                await _unitOfWork.Customers.AddAsync(customer);
            }
            await _unitOfWork.SaveChangesAsync();

            return new { success = true, count = customers.Length, message = $"Đã tạo {customers.Length} khách hàng" };
        }

        private async Task<object> CreateVehiclesAsync()
        {
            return new { success = true, count = 0, message = "Tạm thời bỏ qua tạo vehicles" };
        }

        private async Task<object> CreateEmployeesAsync()
        {
            return new { success = true, count = 0, message = "Tạm thời bỏ qua tạo employees" };
        }

        private async Task<object> CreateServicesAsync()
        {
            return new { success = true, count = 0, message = "Tạm thời bỏ qua tạo services" };
        }

        private async Task<object> CreatePartsAsync()
        {
            return new { success = true, count = 0, message = "Tạm thời bỏ qua tạo parts" };
        }

        private async Task<object> CreateSuppliersAsync()
        {
            return new { success = true, count = 0, message = "Tạm thời bỏ qua tạo suppliers" };
        }

        private async Task<object> CreateInspectionsAsync()
        {
            return new { success = true, count = 0, message = "Tạm thời bỏ qua tạo inspections" };
        }

        private async Task<object> CreateQuotationsAsync()
        {
            return new { success = true, count = 0, message = "Tạm thời bỏ qua tạo quotations" };
        }

        private async Task<object> CreateServiceOrdersAsync()
        {
            return new { success = true, count = 0, message = "Tạm thời bỏ qua tạo service orders" };
        }

        private async Task<object> CreatePaymentsAsync()
        {
            return new { success = true, count = 0, message = "Tạm thời bỏ qua tạo payments" };
        }

        private async Task<object> CreateAppointmentsAsync()
        {
            return new { success = true, count = 0, message = "Tạm thời bỏ qua tạo appointments" };
        }

        #endregion
    }
}
