using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Core.Services;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigurationController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfigurationService _configService;
        private readonly ILogger<ConfigurationController> _logger;

        public ConfigurationController(
            IUnitOfWork unitOfWork, 
            IConfigurationService configService,
            ILogger<ConfigurationController> logger)
        {
            _unitOfWork = unitOfWork;
            _configService = configService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy tất cả cấu hình (có thể filter theo category)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetConfigurations([FromQuery] string? category = null)
        {
            try
            {
                var configs = await _unitOfWork.Repository<SystemConfiguration>().GetAllAsync();
                
                if (!string.IsNullOrEmpty(category))
                {
                    configs = configs.Where(c => c.Category == category);
                }

                var result = configs
                    .Where(c => c.IsVisible)
                    .OrderBy(c => c.Category)
                    .ThenBy(c => c.DisplayOrder)
                    .Select(c => new
                    {
                        c.Id,
                        c.ConfigKey,
                        c.ConfigValue,
                        c.Description,
                        c.DataType,
                        c.Category,
                        c.IsEditable,
                        c.DisplayOrder,
                        c.UpdatedAt,
                        c.UpdatedBy
                    })
                    .ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting configurations");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy cấu hình" });
            }
        }

        /// <summary>
        /// Lấy cấu hình theo key
        /// </summary>
        [HttpGet("{key}")]
        public async Task<IActionResult> GetConfiguration(string key)
        {
            try
            {
                var value = await _configService.GetConfigValueAsync(key);
                
                if (string.IsNullOrEmpty(value))
                {
                    return NotFound(new { success = false, message = "Không tìm thấy cấu hình" });
                }

                return Ok(new { success = true, data = new { key, value } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting configuration {key}");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy cấu hình" });
            }
        }

        /// <summary>
        /// Lấy VAT rate hiện tại
        /// </summary>
        [HttpGet("vat/rate")]
        [AllowAnonymous] // Cho phép get VAT rate công khai
        public async Task<IActionResult> GetVATRate()
        {
            try
            {
                var vatRate = await _configService.GetVATRateAsync();
                
                return Ok(new 
                { 
                    success = true, 
                    data = new 
                    { 
                        rate = vatRate,
                        percentage = vatRate * 100,
                        formatted = $"{vatRate * 100}%"
                    } 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting VAT rate");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thuế suất VAT" });
            }
        }

        /// <summary>
        /// Cập nhật cấu hình
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")] // Chỉ Admin/Manager mới được sửa config
        public async Task<IActionResult> UpdateConfiguration(int id, [FromBody] UpdateConfigRequest request)
        {
            try
            {
                var config = await _unitOfWork.Repository<SystemConfiguration>().GetByIdAsync(id);
                if (config == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy cấu hình" });
                }

                if (!config.IsEditable)
                {
                    return BadRequest(new { success = false, message = "Cấu hình này không được phép chỉnh sửa" });
                }

                // Validate data type
                if (!ValidateDataType(request.ConfigValue, config.DataType))
                {
                    return BadRequest(new { success = false, message = $"Giá trị không đúng định dạng {config.DataType}" });
                }

                var username = User.Identity?.Name ?? "System";
                var success = await _configService.SetConfigValueAsync(config.ConfigKey, request.ConfigValue, username);

                if (!success)
                {
                    return BadRequest(new { success = false, message = "Không thể cập nhật cấu hình" });
                }

                _logger.LogInformation($"Configuration {config.ConfigKey} updated to {request.ConfigValue} by {username}");

                return Ok(new { success = true, message = "Cập nhật cấu hình thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating configuration {id}");
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật cấu hình" });
            }
        }

        /// <summary>
        /// Khởi tạo cấu hình mặc định
        /// </summary>
        [HttpPost("initialize")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> InitializeDefaultConfigurations()
        {
            try
            {
                var configs = await _unitOfWork.Repository<SystemConfiguration>().GetAllAsync();
                
                // Chỉ init nếu chưa có config nào
                if (configs.Any())
                {
                    return BadRequest(new { success = false, message = "Cấu hình đã được khởi tạo" });
                }

                var defaultConfigs = new List<SystemConfiguration>
                {
                    // VAT Configurations
                    new SystemConfiguration
                    {
                        ConfigKey = "VAT.DefaultRate",
                        ConfigValue = "0.10",
                        Description = "Thuế suất VAT mặc định (10%)",
                        DataType = "Decimal",
                        Category = "VAT",
                        IsEditable = true,
                        IsVisible = true,
                        DisplayOrder = 1,
                        CreatedAt = DateTime.Now
                    },
                    new SystemConfiguration
                    {
                        ConfigKey = "VAT.Parts.Rate",
                        ConfigValue = "0.10",
                        Description = "Thuế suất VAT cho phụ tùng (10%)",
                        DataType = "Decimal",
                        Category = "VAT",
                        IsEditable = true,
                        IsVisible = true,
                        DisplayOrder = 2,
                        CreatedAt = DateTime.Now
                    },
                    new SystemConfiguration
                    {
                        ConfigKey = "VAT.Services.Rate",
                        ConfigValue = "0.10",
                        Description = "Thuế suất VAT cho dịch vụ (10% - bao gồm công)",
                        DataType = "Decimal",
                        Category = "VAT",
                        IsEditable = true,
                        IsVisible = true,
                        DisplayOrder = 3,
                        CreatedAt = DateTime.Now
                    },
                    new SystemConfiguration
                    {
                        ConfigKey = "VAT.Labor.Included",
                        ConfigValue = "true",
                        Description = "Công lao động có tính VAT không (true = gộp vào dịch vụ)",
                        DataType = "Boolean",
                        Category = "VAT",
                        IsEditable = true,
                        IsVisible = true,
                        DisplayOrder = 4,
                        CreatedAt = DateTime.Now
                    },

                    // Invoice Configurations
                    new SystemConfiguration
                    {
                        ConfigKey = "Invoice.NumberPrefix",
                        ConfigValue = "INV",
                        Description = "Tiền tố số hóa đơn",
                        DataType = "String",
                        Category = "Invoice",
                        IsEditable = true,
                        IsVisible = true,
                        DisplayOrder = 1,
                        CreatedAt = DateTime.Now
                    },
                    new SystemConfiguration
                    {
                        ConfigKey = "Invoice.AutoGenerate",
                        ConfigValue = "true",
                        Description = "Tự động tạo hóa đơn khi hoàn thành đơn hàng",
                        DataType = "Boolean",
                        Category = "Invoice",
                        IsEditable = true,
                        IsVisible = true,
                        DisplayOrder = 2,
                        CreatedAt = DateTime.Now
                    },

                    // Payment Configurations
                    new SystemConfiguration
                    {
                        ConfigKey = "Payment.DefaultMethod",
                        ConfigValue = "Cash",
                        Description = "Phương thức thanh toán mặc định",
                        DataType = "String",
                        Category = "Payment",
                        IsEditable = true,
                        IsVisible = true,
                        DisplayOrder = 1,
                        CreatedAt = DateTime.Now
                    },
                    new SystemConfiguration
                    {
                        ConfigKey = "Payment.AllowPartial",
                        ConfigValue = "true",
                        Description = "Cho phép thanh toán một phần",
                        DataType = "Boolean",
                        Category = "Payment",
                        IsEditable = true,
                        IsVisible = true,
                        DisplayOrder = 2,
                        CreatedAt = DateTime.Now
                    },

                    // System Configurations
                    new SystemConfiguration
                    {
                        ConfigKey = "System.CompanyName",
                        ConfigValue = "GARAGE Ô TÔ ABC",
                        Description = "Tên công ty",
                        DataType = "String",
                        Category = "System",
                        IsEditable = true,
                        IsVisible = true,
                        DisplayOrder = 1,
                        CreatedAt = DateTime.Now
                    },
                    new SystemConfiguration
                    {
                        ConfigKey = "System.TaxCode",
                        ConfigValue = "0123456789",
                        Description = "Mã số thuế công ty",
                        DataType = "String",
                        Category = "System",
                        IsEditable = true,
                        IsVisible = true,
                        DisplayOrder = 2,
                        CreatedAt = DateTime.Now
                    },
                    new SystemConfiguration
                    {
                        ConfigKey = "System.Address",
                        ConfigValue = "123 Đường ABC, Quận 1, TP.HCM",
                        Description = "Địa chỉ công ty",
                        DataType = "String",
                        Category = "System",
                        IsEditable = true,
                        IsVisible = true,
                        DisplayOrder = 3,
                        CreatedAt = DateTime.Now
                    },
                    new SystemConfiguration
                    {
                        ConfigKey = "System.Phone",
                        ConfigValue = "028-12345678",
                        Description = "Số điện thoại công ty",
                        DataType = "String",
                        Category = "System",
                        IsEditable = true,
                        IsVisible = true,
                        DisplayOrder = 4,
                        CreatedAt = DateTime.Now
                    }
                };

                foreach (var config in defaultConfigs)
                {
                    await _unitOfWork.Repository<SystemConfiguration>().AddAsync(config);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Default configurations initialized");

                return Ok(new { success = true, message = "Khởi tạo cấu hình mặc định thành công", count = defaultConfigs.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing default configurations");
                return StatusCode(500, new { success = false, message = "Lỗi khi khởi tạo cấu hình" });
            }
        }

        /// <summary>
        /// Validate data type
        /// </summary>
        private bool ValidateDataType(string value, string dataType)
        {
            return dataType switch
            {
                "Number" or "Int" => int.TryParse(value, out _),
                "Decimal" => decimal.TryParse(value, out _),
                "Boolean" => bool.TryParse(value, out _),
                _ => true // String or JSON - always valid
            };
        }
    }

    public class UpdateConfigRequest
    {
        public string ConfigValue { get; set; } = string.Empty;
    }
}

