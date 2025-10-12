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
            // Lấy danh sách customers để gán xe
            var customers = (await _unitOfWork.Customers.GetAllAsync()).ToList();
            if (!customers.Any())
            {
                return new { success = false, message = "Cần tạo khách hàng trước khi tạo xe" };
            }

            var vehicles = new[]
            {
                new Core.Entities.Vehicle
                {
                    LicensePlate = "51A-12345",
                    Brand = "Honda",
                    Model = "Civic",
                    Year = "2020",
                    Color = "Trắng",
                    VIN = "JH4DB1650MS123456",
                    EngineNumber = "R18A123456",
                    CustomerId = customers.First().Id,
                    VehicleType = "Personal",
                    InsuranceCompany = "Bảo Việt",
                    PolicyNumber = "BV-2024-001",
                    CoverageType = "Full"
                },
                new Core.Entities.Vehicle
                {
                    LicensePlate = "29B-67890",
                    Brand = "Toyota",
                    Model = "Vios",
                    Year = "2019",
                    Color = "Xám",
                    VIN = "JTDKB20U123456789",
                    EngineNumber = "2NZFE123456",
                    CustomerId = customers.Count > 1 ? customers[1].Id : customers.First().Id,
                    VehicleType = "Insurance",
                    InsuranceCompany = "Bảo Minh",
                    PolicyNumber = "BM-2024-002",
                    CoverageType = "Third Party",
                    ClaimNumber = "CL-2024-001",
                    AdjusterName = "Nguyễn Văn A",
                    AdjusterPhone = "0901234567"
                },
                new Core.Entities.Vehicle
                {
                    LicensePlate = "43C-11111",
                    Brand = "Hyundai",
                    Model = "Accent",
                    Year = "2021",
                    Color = "Đỏ",
                    VIN = "KMHDN41D6MU123456",
                    EngineNumber = "G4LC123456",
                    CustomerId = customers.Count > 2 ? customers[2].Id : customers.First().Id,
                    VehicleType = "Company",
                    CompanyName = "Công ty ABC",
                    TaxCode = "0123456789",
                    ContactPerson = "Trần Thị B",
                    ContactPhone = "0907654321",
                    Department = "Kế toán",
                    CostCenter = "CC-001"
                },
                new Core.Entities.Vehicle
                {
                    LicensePlate = "30D-22222",
                    Brand = "Ford",
                    Model = "Ranger",
                    Year = "2022",
                    Color = "Đen",
                    VIN = "MR0KB3CD7MM123456",
                    EngineNumber = "P5AT123456",
                    CustomerId = customers.First().Id,
                    VehicleType = "Personal",
                    InsuranceCompany = "PVI",
                    PolicyNumber = "PVI-2024-003",
                    CoverageType = "Comprehensive"
                },
                new Core.Entities.Vehicle
                {
                    LicensePlate = "61E-33333",
                    Brand = "Mazda",
                    Model = "CX-5",
                    Year = "2023",
                    Color = "Bạc",
                    VIN = "JM3KFADL7N0123456",
                    EngineNumber = "PY-VPTS123456",
                    CustomerId = customers.Count > 1 ? customers[1].Id : customers.First().Id,
                    VehicleType = "Insurance",
                    InsuranceCompany = "PTI",
                    PolicyNumber = "PTI-2024-004",
                    CoverageType = "Full",
                    ClaimNumber = "CL-2024-002",
                    AdjusterName = "Lê Văn C",
                    AdjusterPhone = "0909876543"
                }
            };

            foreach (var vehicle in vehicles)
            {
                await _unitOfWork.Vehicles.AddAsync(vehicle);
            }
            await _unitOfWork.SaveChangesAsync();

            return new { success = true, count = vehicles.Length, message = $"Đã tạo {vehicles.Length} xe" };
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
                    CostPrice = 350000,
                    AverageCostPrice = 350000,
                    SellPrice = 450000,
                    QuantityInStock = 50,
                    MinimumStock = 10,
                    ReorderLevel = 15,
                    Unit = "Thùng",
                    CompatibleVehicles = "Honda, Toyota, Hyundai, Ford, Mazda",
                    Location = "Kho A - Kệ 1",
                    IsActive = true
                },
                new Core.Entities.Part
                {
                    PartNumber = "FILTER-001",
                    PartName = "Lọc dầu động cơ",
                    Description = "Lọc dầu động cơ chính hãng",
                    Category = "Lọc",
                    Brand = "Bosch",
                    CostPrice = 80000,
                    AverageCostPrice = 80000,
                    SellPrice = 120000,
                    QuantityInStock = 100,
                    MinimumStock = 20,
                    ReorderLevel = 30,
                    Unit = "Cái",
                    CompatibleVehicles = "Honda Civic, Toyota Vios, Hyundai Accent",
                    Location = "Kho A - Kệ 2",
                    IsActive = true
                },
                new Core.Entities.Part
                {
                    PartNumber = "BRAKE-001",
                    PartName = "Má phanh trước",
                    Description = "Má phanh trước chính hãng",
                    Category = "Phanh",
                    Brand = "Brembo",
                    CostPrice = 450000,
                    AverageCostPrice = 450000,
                    SellPrice = 650000,
                    QuantityInStock = 25,
                    MinimumStock = 5,
                    ReorderLevel = 10,
                    Unit = "Bộ",
                    CompatibleVehicles = "Honda Civic, Toyota Vios, Ford Ranger",
                    Location = "Kho B - Kệ 3",
                    IsActive = true
                },
                new Core.Entities.Part
                {
                    PartNumber = "SPARK-001",
                    PartName = "Bugi đánh lửa",
                    Description = "Bugi đánh lửa iridium",
                    Category = "Bugi",
                    Brand = "NGK",
                    CostPrice = 120000,
                    AverageCostPrice = 120000,
                    SellPrice = 180000,
                    QuantityInStock = 40,
                    MinimumStock = 8,
                    ReorderLevel = 12,
                    Unit = "Bộ 4 cái",
                    CompatibleVehicles = "Honda, Toyota, Hyundai, Mazda",
                    Location = "Kho A - Kệ 4",
                    IsActive = true
                },
                new Core.Entities.Part
                {
                    PartNumber = "AIR-001",
                    PartName = "Lọc gió động cơ",
                    Description = "Lọc gió động cơ cao cấp",
                    Category = "Lọc",
                    Brand = "Mann",
                    CostPrice = 95000,
                    AverageCostPrice = 95000,
                    SellPrice = 140000,
                    QuantityInStock = 60,
                    MinimumStock = 12,
                    ReorderLevel = 18,
                    Unit = "Cái",
                    CompatibleVehicles = "Honda Civic, Toyota Vios, Hyundai Accent, Ford Ranger",
                    Location = "Kho A - Kệ 5",
                    IsActive = true
                },
                new Core.Entities.Part
                {
                    PartNumber = "TIRE-001",
                    PartName = "Lốp xe 195/65R15",
                    Description = "Lốp xe radial 195/65R15",
                    Category = "Lốp",
                    Brand = "Michelin",
                    CostPrice = 1200000,
                    AverageCostPrice = 1200000,
                    SellPrice = 1800000,
                    QuantityInStock = 8,
                    MinimumStock = 2,
                    ReorderLevel = 4,
                    Unit = "Cái",
                    CompatibleVehicles = "Honda Civic, Toyota Vios, Hyundai Accent",
                    Location = "Kho C - Kệ 1",
                    IsActive = true
                },
                new Core.Entities.Part
                {
                    PartNumber = "COOLANT-001",
                    PartName = "Dung dịch làm mát",
                    Description = "Dung dịch làm mát động cơ",
                    Category = "Dung dịch",
                    Brand = "Prestone",
                    CostPrice = 180000,
                    AverageCostPrice = 180000,
                    SellPrice = 250000,
                    QuantityInStock = 30,
                    MinimumStock = 6,
                    ReorderLevel = 10,
                    Unit = "Chai 4 lít",
                    CompatibleVehicles = "Tất cả xe",
                    Location = "Kho A - Kệ 6",
                    IsActive = true
                },
                new Core.Entities.Part
                {
                    PartNumber = "BATTERY-001",
                    PartName = "Ắc quy 12V 60Ah",
                    Description = "Ắc quy khô 12V 60Ah",
                    Category = "Điện",
                    Brand = "Varta",
                    CostPrice = 2200000,
                    AverageCostPrice = 2200000,
                    SellPrice = 3200000,
                    QuantityInStock = 12,
                    MinimumStock = 3,
                    ReorderLevel = 5,
                    Unit = "Cái",
                    CompatibleVehicles = "Honda, Toyota, Hyundai, Ford, Mazda",
                    Location = "Kho B - Kệ 4",
                    IsActive = true
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

        private async Task<object> CreateInspectionsAsync()
        {
            // Lấy dữ liệu cần thiết
            var vehicles = (await _unitOfWork.Vehicles.GetAllAsync()).ToList();
            var customers = (await _unitOfWork.Customers.GetAllAsync()).ToList();
            var employees = (await _unitOfWork.Employees.GetAllAsync()).ToList();

            if (!vehicles.Any() || !customers.Any() || !employees.Any())
            {
                return new { success = false, message = "Cần tạo Vehicles, Customers và Employees trước khi tạo inspections" };
            }

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
                    VehicleId = vehicles.Count > 1 ? vehicles[1].Id : vehicles.First().Id,
                    CustomerId = customers.Count > 1 ? customers[1].Id : customers.First().Id,
                    InspectorId = employees.Count > 1 ? employees[1].Id : employees.First().Id,
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
                    VehicleId = vehicles.Count > 2 ? vehicles[2].Id : vehicles.First().Id,
                    CustomerId = customers.Count > 2 ? customers[2].Id : customers.First().Id,
                    InspectorId = employees.First().Id,
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

            if (!inspections.Any() || !customers.Any() || !vehicles.Any() || !employees.Any() || !services.Any() || !parts.Any())
            {
                return new { success = false, message = "Cần tạo đầy đủ dữ liệu trước khi tạo quotations" };
            }

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
                    VehicleInspectionId = inspections.Count > 1 ? inspections[1].Id : inspections.First().Id,
                    CustomerId = customers.Count > 1 ? customers[1].Id : customers.First().Id,
                    VehicleId = vehicles.Count > 1 ? vehicles[1].Id : vehicles.First().Id,
                    PreparedById = employees.Count > 1 ? employees[1].Id : employees.First().Id,
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
                    VehicleInspectionId = inspections.Count > 2 ? inspections[2].Id : inspections.First().Id,
                    CustomerId = customers.Count > 2 ? customers[2].Id : customers.First().Id,
                    VehicleId = vehicles.Count > 2 ? vehicles[2].Id : vehicles.First().Id,
                    PreparedById = employees.First().Id,
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
                    CustomerId = customers.Count > 1 ? customers[1].Id : customers.First().Id,
                    VehicleId = vehicles.Count > 1 ? vehicles[1].Id : vehicles.First().Id,
                    OrderDate = DateTime.Now.AddDays(-1),
                    ScheduledDate = DateTime.Now.AddHours(2),
                    Status = "InProgress",
                    Notes = "Đang thực hiện sửa chữa",
                    TotalAmount = 3500000,
                    DiscountAmount = 0,
                    FinalAmount = 3500000,
                    PaymentStatus = "Partial",
                    ServiceQuotationId = quotations.Count > 1 ? quotations[1].Id : null,
                    PrimaryTechnicianId = employees.Count > 1 ? employees[1].Id : employees.First().Id,
                    ServiceTotal = 800000,
                    PartsTotal = 1300000,
                    AmountPaid = 1500000,
                    AmountRemaining = 2000000
                },
                new Core.Entities.ServiceOrder
                {
                    OrderNumber = "SO-2024-003",
                    CustomerId = customers.Count > 2 ? customers[2].Id : customers.First().Id,
                    VehicleId = vehicles.Count > 2 ? vehicles[2].Id : vehicles.First().Id,
                    OrderDate = DateTime.Now.AddHours(-4),
                    ScheduledDate = DateTime.Now.AddHours(4),
                    Status = "Pending",
                    Notes = "Chờ khách hàng xác nhận",
                    TotalAmount = 720000,
                    DiscountAmount = 80000,
                    FinalAmount = 640000,
                    PaymentStatus = "Unpaid",
                    ServiceQuotationId = quotations.Count > 2 ? quotations[2].Id : null,
                    PrimaryTechnicianId = employees.First().Id,
                    ServiceTotal = 500000,
                    PartsTotal = 220000,
                    AmountPaid = 0,
                    AmountRemaining = 640000
                }
            };

            foreach (var order in serviceOrders)
            {
                await _unitOfWork.ServiceOrders.AddAsync(order);
            }
            await _unitOfWork.SaveChangesAsync();

            // Tạo ServiceOrderItems và ServiceOrderParts cho từng order
            var ordersList = serviceOrders.ToList();
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

        #endregion
    }
}
