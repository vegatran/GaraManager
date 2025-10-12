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
    public class InvoiceController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfigurationService _configService;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(
            IUnitOfWork unitOfWork, 
            IConfigurationService configService,
            ILogger<InvoiceController> logger)
        {
            _unitOfWork = unitOfWork;
            _configService = configService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách hóa đơn
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetInvoices([FromQuery] string? status = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var invoices = await _unitOfWork.Invoices.GetAllAsync();
                
                // Filter by status
                if (!string.IsNullOrEmpty(status))
                {
                    invoices = invoices.Where(i => i.Status == status);
                }
                
                // Filter by date range
                if (fromDate.HasValue)
                {
                    invoices = invoices.Where(i => i.InvoiceDate >= fromDate.Value);
                }
                
                if (toDate.HasValue)
                {
                    invoices = invoices.Where(i => i.InvoiceDate <= toDate.Value);
                }

                var result = invoices.Select(i => new
                {
                    i.Id,
                    i.InvoiceNumber,
                    i.InvoiceDate,
                    i.CustomerId,
                    i.CustomerName,
                    i.VehicleId,
                    i.ServiceOrderId,
                    i.InsuranceClaimId,
                    SubTotal = i.SubTotal,
                    VATAmount = i.VATAmount,
                    TotalAmount = i.TotalAmount,
                    i.Status,
                    i.PaymentMethod,
                    i.Notes,
                    i.CreatedAt
                }).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoices");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy danh sách hóa đơn" });
            }
        }

        /// <summary>
        /// Lấy chi tiết hóa đơn theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoice(int id)
        {
            try
            {
                var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
                if (invoice == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                // Get invoice items
                var invoiceItems = await _unitOfWork.Repository<InvoiceItem>().GetAllAsync();
                var items = invoiceItems.Where(ii => ii.InvoiceId == id).Select(ii => new
                {
                    ii.Id,
                    ii.ItemType,
                    ii.PartId,
                    ii.PartName,
                    ii.ServiceId,
                    ii.ServiceName,
                    ii.Description,
                    ii.Quantity,
                    ii.UnitPrice,
                    ii.SubTotal,
                    ii.VATRate,
                    ii.VATAmount,
                    ii.TotalAmount
                }).ToList();

                var result = new
                {
                    invoice.Id,
                    invoice.InvoiceNumber,
                    invoice.InvoiceDate,
                    invoice.CustomerId,
                    invoice.CustomerName,
                    invoice.CustomerTaxCode,
                    invoice.CustomerAddress,
                    invoice.VehicleId,
                    invoice.VehiclePlate,
                    invoice.ServiceOrderId,
                    invoice.InsuranceClaimId,
                    invoice.InsuranceCompany,
                    invoice.ClaimNumber,
                    SubTotal = invoice.SubTotal,
                    VATAmount = invoice.VATAmount,
                    TotalAmount = invoice.TotalAmount,
                    invoice.Status,
                    invoice.PaymentMethod,
                    invoice.PaymentDate,
                    invoice.Notes,
                    Items = items,
                    invoice.CreatedAt,
                    invoice.UpdatedAt
                };

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting invoice {id}");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thông tin hóa đơn" });
            }
        }

        /// <summary>
        /// Tạo hóa đơn mới từ Service Order
        /// </summary>
        [HttpPost("from-service-order/{serviceOrderId}")]
        public async Task<IActionResult> CreateFromServiceOrder(int serviceOrderId)
        {
            try
            {
                // Get service order
                var serviceOrder = await _unitOfWork.ServiceOrders.GetByIdAsync(serviceOrderId);
                if (serviceOrder == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                // Check if invoice already exists
                var existingInvoices = await _unitOfWork.Invoices.GetAllAsync();
                if (existingInvoices.Any(i => i.ServiceOrderId == serviceOrderId))
                {
                    return BadRequest(new { success = false, message = "Đơn hàng này đã có hóa đơn" });
                }

                // Get customer and vehicle info
                var customer = await _unitOfWork.Customers.GetByIdAsync(serviceOrder.CustomerId);
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(serviceOrder.VehicleId);

                // Generate invoice number
                var invoiceNumber = await GenerateInvoiceNumber();

                // Create invoice
                var invoice = new Invoice
                {
                    InvoiceNumber = invoiceNumber,
                    InvoiceDate = DateTime.Now,
                    CustomerId = serviceOrder.CustomerId,
                    CustomerName = customer?.Name ?? "",
                    CustomerTaxCode = customer?.TaxCode,
                    CustomerAddress = customer?.Address,
                    VehicleId = serviceOrder.VehicleId,
                    VehiclePlate = vehicle?.LicensePlate,
                    ServiceOrderId = serviceOrderId,
                    SubTotal = 0,
                    VATAmount = 0,
                    TotalAmount = 0,
                    Status = "Draft",
                    PaymentMethod = "Cash",
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.Invoices.AddAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                // Get service order items (parts)
                var orderParts = await _unitOfWork.Repository<Core.Entities.ServiceOrderPart>().GetAllAsync();
                var parts = orderParts.Where(p => p.ServiceOrderId == serviceOrderId).ToList();

                // Get service order items (services)
                var orderItems = await _unitOfWork.Repository<Core.Entities.ServiceOrderItem>().GetAllAsync();
                var services = orderItems.Where(s => s.ServiceOrderId == serviceOrderId).ToList();

                // Get VAT rates from configuration
                var partsVATRate = await _configService.GetDecimalConfigAsync("VAT.Parts.Rate", 0.10m);
                var servicesVATRate = await _configService.GetDecimalConfigAsync("VAT.Services.Rate", 0.10m);

                decimal subTotal = 0;
                decimal vatAmount = 0;

                // Add parts to invoice (VAT từ config)
                foreach (var part in parts)
                {
                    var itemSubTotal = part.Quantity * part.UnitPrice;
                    var itemVAT = itemSubTotal * partsVATRate;
                    var itemTotal = itemSubTotal + itemVAT;

                    var invoiceItem = new InvoiceItem
                    {
                        InvoiceId = invoice.Id,
                        ItemType = "Part",
                        PartId = part.PartId,
                        PartName = part.PartName,
                        Description = $"Phụ tùng: {part.PartName}",
                        Quantity = part.Quantity,
                        UnitPrice = part.UnitPrice,
                        SubTotal = itemSubTotal,
                        VATRate = partsVATRate,
                        VATAmount = itemVAT,
                        TotalAmount = itemTotal,
                        CreatedAt = DateTime.Now
                    };

                    await _unitOfWork.Repository<InvoiceItem>().AddAsync(invoiceItem);
                    
                    subTotal += itemSubTotal;
                    vatAmount += itemVAT;
                }

                // Add services to invoice (VAT từ config - bao gồm công)
                foreach (var service in services)
                {
                    var itemSubTotal = service.Quantity * service.UnitPrice;
                    var itemVAT = itemSubTotal * servicesVATRate;
                    var itemTotal = itemSubTotal + itemVAT;

                    var invoiceItem = new InvoiceItem
                    {
                        InvoiceId = invoice.Id,
                        ItemType = "Service",
                        ServiceId = service.ServiceId,
                        ServiceName = service.ServiceName,
                        Description = $"Dịch vụ: {service.ServiceName} (bao gồm công)",
                        Quantity = service.Quantity,
                        UnitPrice = service.UnitPrice,
                        SubTotal = itemSubTotal,
                        VATRate = servicesVATRate,
                        VATAmount = itemVAT,
                        TotalAmount = itemTotal,
                        CreatedAt = DateTime.Now
                    };

                    await _unitOfWork.Repository<InvoiceItem>().AddAsync(invoiceItem);
                    
                    subTotal += itemSubTotal;
                    vatAmount += itemVAT;
                }

                // Update invoice totals
                invoice.SubTotal = subTotal;
                invoice.VATAmount = vatAmount;
                invoice.TotalAmount = subTotal + vatAmount;
                invoice.UpdatedAt = DateTime.Now;

                await _unitOfWork.Invoices.UpdateAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Created invoice {invoiceNumber} from service order {serviceOrderId}");

                return Ok(new 
                { 
                    success = true, 
                    message = "Tạo hóa đơn thành công",
                    data = new 
                    {
                        invoice.Id,
                        invoice.InvoiceNumber,
                        SubTotal = invoice.SubTotal,
                        VATAmount = invoice.VATAmount,
                        TotalAmount = invoice.TotalAmount
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating invoice from service order {serviceOrderId}");
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo hóa đơn" });
            }
        }

        /// <summary>
        /// Generate invoice number theo định dạng: INV-YYYYMM-XXXX
        /// </summary>
        private async Task<string> GenerateInvoiceNumber()
        {
            var today = DateTime.Now;
            var prefix = $"INV-{today:yyyyMM}-";
            
            var invoices = await _unitOfWork.Invoices.GetAllAsync();
            var maxNumber = invoices
                .Where(i => i.InvoiceNumber.StartsWith(prefix))
                .Select(i => 
                {
                    var numPart = i.InvoiceNumber.Substring(prefix.Length);
                    return int.TryParse(numPart, out int num) ? num : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            return $"{prefix}{(maxNumber + 1):D4}";
        }

        /// <summary>
        /// Cập nhật trạng thái hóa đơn
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateInvoiceStatusRequest request)
        {
            try
            {
                var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
                if (invoice == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                invoice.Status = request.Status;
                
                if (request.Status == "Paid" && !invoice.PaymentDate.HasValue)
                {
                    invoice.PaymentDate = DateTime.Now;
                }

                if (!string.IsNullOrEmpty(request.PaymentMethod))
                {
                    invoice.PaymentMethod = request.PaymentMethod;
                }

                invoice.UpdatedAt = DateTime.Now;

                await _unitOfWork.Invoices.UpdateAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Updated invoice {id} status to {request.Status}");

                return Ok(new { success = true, message = "Cập nhật trạng thái thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating invoice {id} status");
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật trạng thái" });
            }
        }

        /// <summary>
        /// Xóa hóa đơn (chỉ cho phép xóa Draft)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            try
            {
                var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
                if (invoice == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                if (invoice.Status != "Draft")
                {
                    return BadRequest(new { success = false, message = "Chỉ được xóa hóa đơn ở trạng thái Draft" });
                }

                // Delete invoice items first
                var invoiceItems = await _unitOfWork.Repository<InvoiceItem>().GetAllAsync();
                var items = invoiceItems.Where(ii => ii.InvoiceId == id).ToList();
                
                foreach (var item in items)
                {
                    await _unitOfWork.Repository<InvoiceItem>().DeleteAsync(item);
                }

                // Delete invoice
                await _unitOfWork.Invoices.DeleteAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Deleted invoice {id}");

                return Ok(new { success = true, message = "Xóa hóa đơn thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting invoice {id}");
                return StatusCode(500, new { success = false, message = "Lỗi khi xóa hóa đơn" });
            }
        }
    }

    public class UpdateInvoiceStatusRequest
    {
        public string Status { get; set; } = "Draft"; // Draft, Issued, Paid, Cancelled
        public string? PaymentMethod { get; set; }
    }
}

