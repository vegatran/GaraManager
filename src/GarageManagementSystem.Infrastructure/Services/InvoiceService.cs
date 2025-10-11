using GarageManagementSystem.Shared.Services;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GarageManagementSystem.Infrastructure.Services
{
    /// <summary>
    /// Service xử lý xuất hóa đơn bảo hiểm
    /// </summary>
    public class InvoiceService : IInvoiceService
    {
        private readonly GarageDbContext _context;
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(GarageDbContext context, ILogger<InvoiceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<InsuranceInvoiceDto> CreateInsuranceInvoiceAsync(int serviceOrderId, InsuranceInvoiceDto invoiceData)
        {
            try
            {
                // Kiểm tra ServiceOrder tồn tại
                var serviceOrder = await _context.ServiceOrders
                    .Include(so => so.Vehicle)
                    .ThenInclude(v => v.Customer)
                    .FirstOrDefaultAsync(so => so.Id == serviceOrderId);

                if (serviceOrder == null)
                {
                    throw new ArgumentException($"ServiceOrder với ID {serviceOrderId} không tồn tại");
                }

                // Tạo InsuranceInvoice entity
                var invoice = new InsuranceInvoice
                {
                    ServiceOrderId = serviceOrderId,
                    InsuranceCompany = invoiceData.InsuranceCompany,
                    PolicyNumber = invoiceData.PolicyNumber,
                    ClaimNumber = invoiceData.ClaimNumber,
                    AccidentDate = invoiceData.AccidentDate,
                    AccidentLocation = invoiceData.AccidentLocation,
                    LicensePlate = invoiceData.LicensePlate,
                    VehicleModel = invoiceData.VehicleModel,
                    TotalApprovedAmount = invoiceData.TotalApprovedAmount,
                    CustomerResponsibility = invoiceData.CustomerResponsibility,
                    InsuranceResponsibility = invoiceData.InsuranceResponsibility,
                    VatAmount = invoiceData.VatAmount,
                    FinalAmount = invoiceData.FinalAmount,
                    Notes = invoiceData.Notes ?? string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.InsuranceInvoices.Add(invoice);
                await _context.SaveChangesAsync();

                // Tạo các InsuranceInvoiceItem
                foreach (var itemDto in invoiceData.Items)
                {
                    var item = new InsuranceInvoiceItem
                    {
                        InsuranceInvoiceId = invoice.Id,
                        ItemName = itemDto.ItemName,
                        ItemType = itemDto.ItemType,
                        ApprovedPrice = itemDto.ApprovedPrice,
                        CustomerPrice = itemDto.CustomerPrice,
                        InsurancePrice = itemDto.InsurancePrice,
                        Notes = itemDto.Notes ?? string.Empty,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.InsuranceInvoiceItems.Add(item);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Tạo hóa đơn bảo hiểm thành công: Invoice ID {invoice.Id} cho ServiceOrder {serviceOrderId}");

                return await GetInsuranceInvoiceAsync(invoice.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi tạo hóa đơn bảo hiểm cho ServiceOrder {serviceOrderId}");
                throw;
            }
        }

        public async Task<byte[]> GenerateInsuranceInvoicePdfAsync(int invoiceId)
        {
            try
            {
                var invoice = await GetInsuranceInvoiceAsync(invoiceId);
                if (invoice == null)
                {
                    throw new ArgumentException($"Hóa đơn bảo hiểm với ID {invoiceId} không tồn tại");
                }

                // TODO: Implement PDF generation using iTextSharp or similar library
                // For now, return a placeholder
                _logger.LogInformation($"Xuất PDF hóa đơn bảo hiểm: Invoice ID {invoiceId}");
                
                // Placeholder - sẽ implement sau
                return System.Text.Encoding.UTF8.GetBytes("PDF Generation - Coming Soon");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xuất PDF hóa đơn bảo hiểm {invoiceId}");
                throw;
            }
        }

        public async Task<byte[]> GenerateInsuranceInvoiceExcelAsync(int invoiceId)
        {
            try
            {
                var invoice = await GetInsuranceInvoiceAsync(invoiceId);
                if (invoice == null)
                {
                    throw new ArgumentException($"Hóa đơn bảo hiểm với ID {invoiceId} không tồn tại");
                }

                // TODO: Implement Excel generation using EPPlus
                // For now, return a placeholder
                _logger.LogInformation($"Xuất Excel hóa đơn bảo hiểm: Invoice ID {invoiceId}");
                
                // Placeholder - sẽ implement sau
                return System.Text.Encoding.UTF8.GetBytes("Excel Generation - Coming Soon");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xuất Excel hóa đơn bảo hiểm {invoiceId}");
                throw;
            }
        }

        public async Task<List<InsuranceInvoiceDto>> GetInsuranceInvoicesAsync(int? serviceOrderId = null, string? insuranceCompany = null)
        {
            try
            {
                var query = _context.InsuranceInvoices
                    .Include(ii => ii.ServiceOrder)
                    .Include(ii => ii.Items)
                    .AsQueryable();

                if (serviceOrderId.HasValue)
                {
                    query = query.Where(ii => ii.ServiceOrderId == serviceOrderId.Value);
                }

                if (!string.IsNullOrEmpty(insuranceCompany))
                {
                    query = query.Where(ii => ii.InsuranceCompany.Contains(insuranceCompany));
                }

                var invoices = await query
                    .OrderByDescending(ii => ii.CreatedAt)
                    .ToListAsync();

                return invoices.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi lấy danh sách hóa đơn bảo hiểm");
                throw;
            }
        }

        public async Task<InsuranceInvoiceDto?> GetInsuranceInvoiceAsync(int invoiceId)
        {
            try
            {
                var invoice = await _context.InsuranceInvoices
                    .Include(ii => ii.ServiceOrder)
                    .Include(ii => ii.Items)
                    .FirstOrDefaultAsync(ii => ii.Id == invoiceId);

                return invoice != null ? MapToDto(invoice) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi lấy hóa đơn bảo hiểm {invoiceId}");
                throw;
            }
        }

        public async Task<InsuranceInvoiceDto> UpdateInsuranceInvoiceAsync(int invoiceId, InsuranceInvoiceDto invoiceData)
        {
            try
            {
                var invoice = await _context.InsuranceInvoices
                    .Include(ii => ii.Items)
                    .FirstOrDefaultAsync(ii => ii.Id == invoiceId);

                if (invoice == null)
                {
                    throw new ArgumentException($"Hóa đơn bảo hiểm với ID {invoiceId} không tồn tại");
                }

                // Cập nhật thông tin chính
                invoice.InsuranceCompany = invoiceData.InsuranceCompany;
                invoice.PolicyNumber = invoiceData.PolicyNumber;
                invoice.ClaimNumber = invoiceData.ClaimNumber;
                invoice.AccidentDate = invoiceData.AccidentDate;
                invoice.AccidentLocation = invoiceData.AccidentLocation;
                invoice.LicensePlate = invoiceData.LicensePlate;
                invoice.VehicleModel = invoiceData.VehicleModel;
                invoice.TotalApprovedAmount = invoiceData.TotalApprovedAmount;
                invoice.CustomerResponsibility = invoiceData.CustomerResponsibility;
                invoice.InsuranceResponsibility = invoiceData.InsuranceResponsibility;
                invoice.VatAmount = invoiceData.VatAmount;
                invoice.FinalAmount = invoiceData.FinalAmount;
                invoice.Notes = invoiceData.Notes ?? string.Empty;
                invoice.UpdatedAt = DateTime.UtcNow;

                // Xóa các item cũ và thêm mới
                _context.InsuranceInvoiceItems.RemoveRange(invoice.Items);

                foreach (var itemDto in invoiceData.Items)
                {
                    var item = new InsuranceInvoiceItem
                    {
                        InsuranceInvoiceId = invoice.Id,
                        ItemName = itemDto.ItemName,
                        ItemType = itemDto.ItemType,
                        ApprovedPrice = itemDto.ApprovedPrice,
                        CustomerPrice = itemDto.CustomerPrice,
                        InsurancePrice = itemDto.InsurancePrice,
                        Notes = itemDto.Notes ?? string.Empty,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.InsuranceInvoiceItems.Add(item);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Cập nhật hóa đơn bảo hiểm thành công: Invoice ID {invoiceId}");

                return await GetInsuranceInvoiceAsync(invoiceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi cập nhật hóa đơn bảo hiểm {invoiceId}");
                throw;
            }
        }

        public async Task<bool> DeleteInsuranceInvoiceAsync(int invoiceId)
        {
            try
            {
                var invoice = await _context.InsuranceInvoices
                    .Include(ii => ii.Items)
                    .FirstOrDefaultAsync(ii => ii.Id == invoiceId);

                if (invoice == null)
                {
                    return false;
                }

                _context.InsuranceInvoiceItems.RemoveRange(invoice.Items);
                _context.InsuranceInvoices.Remove(invoice);

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Xóa hóa đơn bảo hiểm thành công: Invoice ID {invoiceId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xóa hóa đơn bảo hiểm {invoiceId}");
                throw;
            }
        }

        private static InsuranceInvoiceDto MapToDto(InsuranceInvoice invoice)
        {
            return new InsuranceInvoiceDto
            {
                Id = invoice.Id,
                ServiceOrderId = invoice.ServiceOrderId,
                InsuranceCompany = invoice.InsuranceCompany,
                PolicyNumber = invoice.PolicyNumber,
                ClaimNumber = invoice.ClaimNumber,
                AccidentDate = invoice.AccidentDate,
                AccidentLocation = invoice.AccidentLocation,
                LicensePlate = invoice.LicensePlate,
                VehicleModel = invoice.VehicleModel,
                TotalApprovedAmount = invoice.TotalApprovedAmount,
                CustomerResponsibility = invoice.CustomerResponsibility,
                InsuranceResponsibility = invoice.InsuranceResponsibility,
                VatAmount = invoice.VatAmount,
                FinalAmount = invoice.FinalAmount,
                Notes = invoice.Notes,
                Items = invoice.Items.Select(item => new InsuranceInvoiceItemDto
                {
                    Id = item.Id,
                    InsuranceInvoiceId = item.InsuranceInvoiceId,
                    ItemName = item.ItemName,
                    ItemType = item.ItemType,
                    ApprovedPrice = item.ApprovedPrice,
                    CustomerPrice = item.CustomerPrice,
                    InsurancePrice = item.InsurancePrice,
                    Notes = item.Notes
                }).ToList(),
                CreatedAt = invoice.CreatedAt,
                CreatedBy = invoice.CreatedBy ?? string.Empty
            };
        }
    }
}
