using GarageManagementSystem.Shared.Services;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarageManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiScope")]
    public class InsuranceInvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InsuranceInvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        /// <summary>
        /// Lấy danh sách hóa đơn bảo hiểm
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<InsuranceInvoiceDto>>>> GetInsuranceInvoices(
            [FromQuery] int? serviceOrderId = null,
            [FromQuery] string? insuranceCompany = null)
        {
            try
            {
                var invoices = await _invoiceService.GetInsuranceInvoicesAsync(serviceOrderId, insuranceCompany);
                return Ok(ApiResponse<List<InsuranceInvoiceDto>>.SuccessResult(invoices));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<InsuranceInvoiceDto>>.ErrorResult($"Lỗi lấy danh sách hóa đơn bảo hiểm: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy chi tiết hóa đơn bảo hiểm
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<InsuranceInvoiceDto>>> GetInsuranceInvoice(int id)
        {
            try
            {
                var invoice = await _invoiceService.GetInsuranceInvoiceAsync(id);
                if (invoice == null)
                {
                    return NotFound(ApiResponse<InsuranceInvoiceDto>.ErrorResult($"Hóa đơn bảo hiểm với ID {id} không tồn tại"));
                }

                return Ok(ApiResponse<InsuranceInvoiceDto>.SuccessResult(invoice));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InsuranceInvoiceDto>.ErrorResult($"Lỗi lấy hóa đơn bảo hiểm: {ex.Message}"));
            }
        }

        /// <summary>
        /// Tạo hóa đơn bảo hiểm mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<InsuranceInvoiceDto>>> CreateInsuranceInvoice([FromBody] InsuranceInvoiceDto invoiceData)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse<InsuranceInvoiceDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
                }

                var invoice = await _invoiceService.CreateInsuranceInvoiceAsync(invoiceData.ServiceOrderId, invoiceData);
                return CreatedAtAction(nameof(GetInsuranceInvoice), new { id = invoice.Id }, 
                    ApiResponse<InsuranceInvoiceDto>.SuccessResult(invoice, "Tạo hóa đơn bảo hiểm thành công"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<InsuranceInvoiceDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InsuranceInvoiceDto>.ErrorResult($"Lỗi tạo hóa đơn bảo hiểm: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cập nhật hóa đơn bảo hiểm
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<InsuranceInvoiceDto>>> UpdateInsuranceInvoice(int id, [FromBody] InsuranceInvoiceDto invoiceData)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse<InsuranceInvoiceDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
                }

                var invoice = await _invoiceService.UpdateInsuranceInvoiceAsync(id, invoiceData);
                return Ok(ApiResponse<InsuranceInvoiceDto>.SuccessResult(invoice, "Cập nhật hóa đơn bảo hiểm thành công"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<InsuranceInvoiceDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InsuranceInvoiceDto>.ErrorResult($"Lỗi cập nhật hóa đơn bảo hiểm: {ex.Message}"));
            }
        }

        /// <summary>
        /// Xóa hóa đơn bảo hiểm
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteInsuranceInvoice(int id)
        {
            try
            {
                var result = await _invoiceService.DeleteInsuranceInvoiceAsync(id);
                if (!result)
                {
                    return NotFound(ApiResponse<bool>.ErrorResult($"Hóa đơn bảo hiểm với ID {id} không tồn tại"));
                }

                return Ok(ApiResponse<bool>.SuccessResult(true, "Xóa hóa đơn bảo hiểm thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResult($"Lỗi xóa hóa đơn bảo hiểm: {ex.Message}"));
            }
        }

        /// <summary>
        /// Xuất hóa đơn bảo hiểm ra PDF
        /// </summary>
        [HttpGet("{id}/export/pdf")]
        public async Task<ActionResult> ExportInsuranceInvoicePdf(int id)
        {
            try
            {
                var pdfBytes = await _invoiceService.GenerateInsuranceInvoicePdfAsync(id);
                var fileName = $"Insurance_Invoice_{id}_{DateTime.Now:yyyyMMdd}.pdf";
                
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Lỗi xuất PDF hóa đơn bảo hiểm: {ex.Message}"));
            }
        }

        /// <summary>
        /// Xuất hóa đơn bảo hiểm ra Excel
        /// </summary>
        [HttpGet("{id}/export/excel")]
        public async Task<ActionResult> ExportInsuranceInvoiceExcel(int id)
        {
            try
            {
                var excelBytes = await _invoiceService.GenerateInsuranceInvoiceExcelAsync(id);
                var fileName = $"Insurance_Invoice_{id}_{DateTime.Now:yyyyMMdd}.xlsx";
                
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Lỗi xuất Excel hóa đơn bảo hiểm: {ex.Message}"));
            }
        }
    }
}
