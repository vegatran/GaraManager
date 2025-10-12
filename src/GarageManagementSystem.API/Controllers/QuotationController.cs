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
    public class QuotationController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfigurationService _configService;
        private readonly ILogger<QuotationController> _logger;

        public QuotationController(
            IUnitOfWork unitOfWork,
            IConfigurationService configService,
            ILogger<QuotationController> logger)
        {
            _unitOfWork = unitOfWork;
            _configService = configService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách báo giá
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetQuotations(
            [FromQuery] string? status = null,
            [FromQuery] int? customerId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var quotations = await _unitOfWork.Quotations.GetAllAsync();

                // Filters
                if (!string.IsNullOrEmpty(status))
                    quotations = quotations.Where(q => q.Status == status);

                if (customerId.HasValue)
                    quotations = quotations.Where(q => q.CustomerId == customerId.Value);

                if (fromDate.HasValue)
                    quotations = quotations.Where(q => q.QuotationDate >= fromDate.Value);

                if (toDate.HasValue)
                    quotations = quotations.Where(q => q.QuotationDate <= toDate.Value);

                var result = quotations.Select(q => new
                {
                    q.Id,
                    q.QuotationNumber,
                    q.QuotationDate,
                    q.ExpiryDate,
                    q.CustomerId,
                    q.CustomerName,
                    q.VehicleId,
                    q.VehiclePlate,
                    q.InspectionId,
                    SubTotal = q.SubTotal,
                    VATAmount = q.VATAmount,
                    TotalAmount = q.TotalAmount,
                    q.Status,
                    q.CreatedAt
                }).OrderByDescending(q => q.CreatedAt).ToList();

                return Ok(new { success = true, data = result, count = result.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quotations");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy danh sách báo giá" });
            }
        }

        /// <summary>
        /// Lấy chi tiết báo giá
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuotation(int id)
        {
            try
            {
                var quotation = await _unitOfWork.Quotations.GetByIdAsync(id);
                if (quotation == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy báo giá" });
                }

                // Get quotation items
                var items = await _unitOfWork.Repository<QuotationItem>().GetAllAsync();
                var quotationItems = items.Where(i => i.QuotationId == id).Select(i => new
                {
                    i.Id,
                    i.ItemType,
                    i.PartId,
                    i.PartName,
                    i.ServiceId,
                    i.ServiceName,
                    i.Description,
                    i.Quantity,
                    i.UnitPrice,
                    i.SubTotal,
                    i.VATRate,
                    i.VATAmount,
                    i.TotalAmount
                }).ToList();

                var result = new
                {
                    quotation.Id,
                    quotation.QuotationNumber,
                    quotation.QuotationDate,
                    quotation.ExpiryDate,
                    quotation.CustomerId,
                    quotation.CustomerName,
                    quotation.CustomerPhone,
                    quotation.CustomerEmail,
                    quotation.VehicleId,
                    quotation.VehiclePlate,
                    quotation.VehicleMake,
                    quotation.VehicleModel,
                    quotation.InspectionId,
                    SubTotal = quotation.SubTotal,
                    VATAmount = quotation.VATAmount,
                    TotalAmount = quotation.TotalAmount,
                    quotation.Status,
                    quotation.Notes,
                    Items = quotationItems,
                    quotation.CreatedAt,
                    quotation.UpdatedAt
                };

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting quotation {id}");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thông tin báo giá" });
            }
        }

        /// <summary>
        /// Tạo báo giá từ Inspection
        /// </summary>
        [HttpPost("from-inspection/{inspectionId}")]
        public async Task<IActionResult> CreateFromInspection(int inspectionId)
        {
            try
            {
                var inspection = await _unitOfWork.Inspections.GetByIdAsync(inspectionId);
                if (inspection == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy phiếu kiểm tra" });
                }

                // Check if quotation already exists
                var existingQuotations = await _unitOfWork.Quotations.GetAllAsync();
                if (existingQuotations.Any(q => q.InspectionId == inspectionId))
                {
                    return BadRequest(new { success = false, message = "Phiếu kiểm tra này đã có báo giá" });
                }

                // Get customer and vehicle info
                var customer = await _unitOfWork.Customers.GetByIdAsync(inspection.CustomerId);
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(inspection.VehicleId);

                // Generate quotation number
                var quotationNumber = await GenerateQuotationNumber();

                // Create quotation
                var quotation = new ServiceQuotation
                {
                    QuotationNumber = quotationNumber,
                    QuotationDate = DateTime.Now,
                    ExpiryDate = DateTime.Now.AddDays(30), // Valid for 30 days
                    CustomerId = inspection.CustomerId,
                    CustomerName = customer?.Name ?? "",
                    CustomerPhone = customer?.Phone,
                    CustomerEmail = customer?.Email,
                    VehicleId = inspection.VehicleId,
                    VehiclePlate = vehicle?.LicensePlate,
                    VehicleMake = vehicle?.Brand,
                    VehicleModel = vehicle?.Model,
                    InspectionId = inspectionId,
                    SubTotal = 0,
                    VATAmount = 0,
                    TotalAmount = 0,
                    Status = "Draft",
                    Notes = $"Báo giá từ phiếu kiểm tra: {inspection.Findings}",
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.Quotations.AddAsync(quotation);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Created quotation {quotationNumber} from inspection {inspectionId}");

                return Ok(new
                {
                    success = true,
                    message = "Tạo báo giá thành công",
                    data = new
                    {
                        quotation.Id,
                        quotation.QuotationNumber
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating quotation from inspection {inspectionId}");
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo báo giá" });
            }
        }

        /// <summary>
        /// Tạo báo giá mới (thủ công)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateQuotation([FromBody] CreateQuotationRequest request)
        {
            try
            {
                // Validate
                var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
                if (customer == null)
                {
                    return BadRequest(new { success = false, message = "Không tìm thấy khách hàng" });
                }

                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.VehicleId);
                if (vehicle == null)
                {
                    return BadRequest(new { success = false, message = "Không tìm thấy xe" });
                }

                // Generate number
                var quotationNumber = await GenerateQuotationNumber();

                // Create quotation
                var quotation = new ServiceQuotation
                {
                    QuotationNumber = quotationNumber,
                    QuotationDate = DateTime.Now,
                    ExpiryDate = request.ExpiryDate ?? DateTime.Now.AddDays(30),
                    CustomerId = request.CustomerId,
                    CustomerName = customer.Name,
                    CustomerPhone = customer.Phone,
                    CustomerEmail = customer.Email,
                    VehicleId = request.VehicleId,
                    VehiclePlate = vehicle.LicensePlate,
                    VehicleMake = vehicle.Brand,
                    VehicleModel = vehicle.Model,
                    SubTotal = 0,
                    VATAmount = 0,
                    TotalAmount = 0,
                    Status = "Draft",
                    Notes = request.Notes,
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.Quotations.AddAsync(quotation);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Created quotation {quotationNumber}");

                return Ok(new
                {
                    success = true,
                    message = "Tạo báo giá thành công",
                    data = new
                    {
                        quotation.Id,
                        quotation.QuotationNumber
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quotation");
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo báo giá" });
            }
        }

        /// <summary>
        /// Thêm item vào báo giá
        /// </summary>
        [HttpPost("{id}/items")]
        public async Task<IActionResult> AddItem(int id, [FromBody] AddQuotationItemRequest request)
        {
            try
            {
                var quotation = await _unitOfWork.Quotations.GetByIdAsync(id);
                if (quotation == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy báo giá" });
                }

                if (quotation.Status != "Draft")
                {
                    return BadRequest(new { success = false, message = "Chỉ được sửa báo giá ở trạng thái Draft" });
                }

                // Get VAT rate
                var vatRate = request.ItemType == "Part" 
                    ? await _configService.GetDecimalConfigAsync("VAT.Parts.Rate", 0.10m)
                    : await _configService.GetDecimalConfigAsync("VAT.Services.Rate", 0.10m);

                // Calculate amounts
                var subTotal = request.Quantity * request.UnitPrice;
                var vatAmount = subTotal * vatRate;
                var totalAmount = subTotal + vatAmount;

                var item = new QuotationItem
                {
                    QuotationId = id,
                    ItemType = request.ItemType,
                    PartId = request.PartId,
                    PartName = request.PartName,
                    ServiceId = request.ServiceId,
                    ServiceName = request.ServiceName,
                    Description = request.Description,
                    Quantity = request.Quantity,
                    UnitPrice = request.UnitPrice,
                    SubTotal = subTotal,
                    VATRate = vatRate,
                    VATAmount = vatAmount,
                    TotalAmount = totalAmount,
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.Repository<QuotationItem>().AddAsync(item);

                // Recalculate quotation totals
                await RecalculateQuotationTotals(id);

                _logger.LogInformation($"Added item to quotation {id}");

                return Ok(new { success = true, message = "Thêm item thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding item to quotation {id}");
                return StatusCode(500, new { success = false, message = "Lỗi khi thêm item" });
            }
        }

        /// <summary>
        /// Xóa item khỏi báo giá
        /// </summary>
        [HttpDelete("{id}/items/{itemId}")]
        public async Task<IActionResult> DeleteItem(int id, int itemId)
        {
            try
            {
                var quotation = await _unitOfWork.Quotations.GetByIdAsync(id);
                if (quotation == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy báo giá" });
                }

                if (quotation.Status != "Draft")
                {
                    return BadRequest(new { success = false, message = "Chỉ được sửa báo giá ở trạng thái Draft" });
                }

                var item = await _unitOfWork.Repository<QuotationItem>().GetByIdAsync(itemId);
                if (item == null || item.QuotationId != id)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy item" });
                }

                await _unitOfWork.Repository<QuotationItem>().DeleteAsync(item);

                // Recalculate quotation totals
                await RecalculateQuotationTotals(id);

                _logger.LogInformation($"Deleted item {itemId} from quotation {id}");

                return Ok(new { success = true, message = "Xóa item thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting item {itemId} from quotation {id}");
                return StatusCode(500, new { success = false, message = "Lỗi khi xóa item" });
            }
        }

        /// <summary>
        /// Gửi báo giá cho khách hàng (chuyển sang Sent)
        /// </summary>
        [HttpPost("{id}/send")]
        public async Task<IActionResult> SendQuotation(int id)
        {
            try
            {
                var quotation = await _unitOfWork.Quotations.GetByIdAsync(id);
                if (quotation == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy báo giá" });
                }

                if (quotation.Status != "Draft")
                {
                    return BadRequest(new { success = false, message = "Báo giá không ở trạng thái Draft" });
                }

                // Check if has items
                var items = await _unitOfWork.Repository<QuotationItem>().GetAllAsync();
                if (!items.Any(i => i.QuotationId == id))
                {
                    return BadRequest(new { success = false, message = "Báo giá chưa có item nào" });
                }

                quotation.Status = "Sent";
                quotation.UpdatedAt = DateTime.Now;

                await _unitOfWork.Quotations.UpdateAsync(quotation);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Sent quotation {id}");

                // TODO: Send email to customer

                return Ok(new { success = true, message = "Gửi báo giá thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending quotation {id}");
                return StatusCode(500, new { success = false, message = "Lỗi khi gửi báo giá" });
            }
        }

        /// <summary>
        /// Chuyển báo giá thành Service Order
        /// </summary>
        [HttpPost("{id}/convert-to-order")]
        public async Task<IActionResult> ConvertToServiceOrder(int id)
        {
            try
            {
                var quotation = await _unitOfWork.Quotations.GetByIdAsync(id);
                if (quotation == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy báo giá" });
                }

                if (quotation.Status != "Sent" && quotation.Status != "Accepted")
                {
                    return BadRequest(new { success = false, message = "Báo giá chưa được gửi hoặc chấp nhận" });
                }

                // Check if already converted
                var existingOrders = await _unitOfWork.ServiceOrders.GetAllAsync();
                if (existingOrders.Any(o => o.QuotationId == id))
                {
                    return BadRequest(new { success = false, message = "Báo giá này đã được chuyển thành đơn hàng" });
                }

                // Generate order number
                var orderNumber = await GenerateOrderNumber();

                // Create service order
                var serviceOrder = new ServiceOrder
                {
                    OrderNumber = orderNumber,
                    OrderDate = DateTime.Now,
                    CustomerId = quotation.CustomerId,
                    VehicleId = quotation.VehicleId,
                    QuotationId = id,
                    Description = $"Đơn hàng từ báo giá {quotation.QuotationNumber}",
                    EstimatedAmount = quotation.TotalAmount,
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.ServiceOrders.AddAsync(serviceOrder);
                await _unitOfWork.SaveChangesAsync();

                // Copy quotation items to service order
                var quotationItems = await _unitOfWork.Repository<QuotationItem>().GetAllAsync();
                var items = quotationItems.Where(i => i.QuotationId == id).ToList();

                foreach (var item in items)
                {
                    if (item.ItemType == "Part")
                    {
                        var orderPart = new Core.Entities.ServiceOrderPart
                        {
                            ServiceOrderId = serviceOrder.Id,
                            PartId = item.PartId ?? 0,
                            PartName = item.PartName ?? "",
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            CreatedAt = DateTime.Now
                        };
                        await _unitOfWork.Repository<Core.Entities.ServiceOrderPart>().AddAsync(orderPart);
                    }
                    else if (item.ItemType == "Service")
                    {
                        var orderItem = new Core.Entities.ServiceOrderItem
                        {
                            ServiceOrderId = serviceOrder.Id,
                            ServiceId = item.ServiceId ?? 0,
                            ServiceName = item.ServiceName ?? "",
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            CreatedAt = DateTime.Now
                        };
                        await _unitOfWork.Repository<Core.Entities.ServiceOrderItem>().AddAsync(orderItem);
                    }
                }

                // Update quotation status
                quotation.Status = "Converted";
                quotation.UpdatedAt = DateTime.Now;
                await _unitOfWork.Quotations.UpdateAsync(quotation);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Converted quotation {id} to service order {serviceOrder.Id}");

                return Ok(new
                {
                    success = true,
                    message = "Chuyển thành đơn hàng thành công",
                    data = new
                    {
                        serviceOrder.Id,
                        serviceOrder.OrderNumber
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error converting quotation {id} to service order");
                return StatusCode(500, new { success = false, message = "Lỗi khi chuyển thành đơn hàng" });
            }
        }

        /// <summary>
        /// Recalculate quotation totals
        /// </summary>
        private async Task RecalculateQuotationTotals(int quotationId)
        {
            var quotation = await _unitOfWork.Quotations.GetByIdAsync(quotationId);
            if (quotation == null) return;

            var items = await _unitOfWork.Repository<QuotationItem>().GetAllAsync();
            var quotationItems = items.Where(i => i.QuotationId == quotationId).ToList();

            quotation.SubTotal = quotationItems.Sum(i => i.SubTotal);
            quotation.VATAmount = quotationItems.Sum(i => i.VATAmount);
            quotation.TotalAmount = quotationItems.Sum(i => i.TotalAmount);
            quotation.UpdatedAt = DateTime.Now;

            await _unitOfWork.Quotations.UpdateAsync(quotation);
            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Generate quotation number: QT-YYYYMM-XXXX
        /// </summary>
        private async Task<string> GenerateQuotationNumber()
        {
            var today = DateTime.Now;
            var prefix = $"QT-{today:yyyyMM}-";

            var quotations = await _unitOfWork.Quotations.GetAllAsync();
            var maxNumber = quotations
                .Where(q => q.QuotationNumber.StartsWith(prefix))
                .Select(q =>
                {
                    var numPart = q.QuotationNumber.Substring(prefix.Length);
                    return int.TryParse(numPart, out int num) ? num : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            return $"{prefix}{(maxNumber + 1):D4}";
        }

        /// <summary>
        /// Generate order number: SO-YYYYMM-XXXX
        /// </summary>
        private async Task<string> GenerateOrderNumber()
        {
            var today = DateTime.Now;
            var prefix = $"SO-{today:yyyyMM}-";

            var orders = await _unitOfWork.ServiceOrders.GetAllAsync();
            var maxNumber = orders
                .Where(o => o.OrderNumber.StartsWith(prefix))
                .Select(o =>
                {
                    var numPart = o.OrderNumber.Substring(prefix.Length);
                    return int.TryParse(numPart, out int num) ? num : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            return $"{prefix}{(maxNumber + 1):D4}";
        }
    }

    #region Request Models

    public class CreateQuotationRequest
    {
        public int CustomerId { get; set; }
        public int VehicleId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Notes { get; set; }
    }

    public class AddQuotationItemRequest
    {
        public string ItemType { get; set; } = "Part"; // Part or Service
        public int? PartId { get; set; }
        public string? PartName { get; set; }
        public int? ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    #endregion
}

