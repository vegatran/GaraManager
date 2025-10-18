using GarageManagementSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;
using GarageManagementSystem.Web.Models;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller quản lý báo giá với đầy đủ các thao tác CRUD thông qua API
    /// </summary>
    [Authorize]
    [Route("QuotationManagement")]
    public class QuotationManagementController : Controller
    {
        private readonly ApiService _apiService;

        public QuotationManagementController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Hiển thị trang quản lý báo giá
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Lấy danh sách tất cả báo giá cho DataTable thông qua API
        /// </summary>
        [HttpGet("GetQuotations")]
        public async Task<IActionResult> GetQuotations()
        {
            var response = await _apiService.GetAsync<List<ServiceQuotationDto>>(ApiEndpoints.ServiceQuotations.GetAll);
            
            if (response.Success)
            {
                var quotationList = new List<object>();
                
                if (response.Data != null)
                {
                    quotationList = response.Data.Select(q => new
                    {
                        id = q.Id,
                        quotationNumber = q.QuotationNumber,
                        vehicleInfo = $"{q.Vehicle?.Brand} {q.Vehicle?.Model} - {q.Vehicle?.LicensePlate}",
                        customerName = q.Customer?.Name ?? "N/A",
                        totalAmount = q.TotalAmount.ToString("N0"),
                        status = TranslateQuotationStatus(q.Status),
                        validUntil = q.ValidUntil,
                        createdDate = q.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                    }).Cast<object>().ToList();
                }

                return Json(new { 
                    success = true,
                    data = quotationList,
                    message = "Lấy danh sách báo giá thành công"
                });
            }
            else
            {
                return Json(new { error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết báo giá theo ID thông qua API
        /// </summary>
        [HttpGet("GetQuotation/{id}")]
        public async Task<IActionResult> GetQuotation(int id)
        {
            var response = await _apiService.GetAsync<ServiceQuotationDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceQuotations.GetById, id)
            );
            
            return Json(response);
        }

        /// <summary>
        /// Tạo báo giá mới thông qua API
        /// </summary>
        [HttpPost("CreateQuotation")]
        public async Task<IActionResult> CreateQuotation([FromBody] CreateServiceQuotationDto quotationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errorMessage = "Dữ liệu không hợp lệ" });
            }

            var response = await _apiService.PostAsync<ServiceQuotationDto>(
                ApiEndpoints.ServiceQuotations.Create,
                quotationDto
            );

            return Json(response);
        }

        /// <summary>
        /// Cập nhật thông tin báo giá thông qua API
        /// </summary>
        [HttpPut("UpdateQuotation/{id}")]
        public async Task<IActionResult> UpdateQuotation(int id, [FromBody] UpdateServiceQuotationDto quotationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errorMessage = "Dữ liệu không hợp lệ" });
            }

            if (id != quotationDto.Id)
            {
                return BadRequest(new { success = false, errorMessage = "ID không khớp" });
            }

            var response = await _apiService.PutAsync<ServiceQuotationDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceQuotations.Update, id),
                quotationDto
            );

            return Json(response);
        }

        /// <summary>
        /// Xóa báo giá thông qua API
        /// </summary>
        [HttpDelete("DeleteQuotation/{id}")]
        public async Task<IActionResult> DeleteQuotation(int id)
        {
            var response = await _apiService.DeleteAsync<ServiceQuotationDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceQuotations.Delete, id)
            );

            return Json(response);
        }

        /// <summary>
        /// Duyệt báo giá
        /// </summary>
        [HttpPost("ApproveQuotation/{id}")]
        public async Task<IActionResult> ApproveQuotation(int id)
        {
            var response = await _apiService.PostAsync<ServiceQuotationDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceQuotations.Approve, id),
                new { }
            );

            return Json(response);
        }

        /// <summary>
        /// Từ chối báo giá
        /// </summary>
        [HttpPost("RejectQuotation/{id}")]
        public async Task<IActionResult> RejectQuotation(int id, [FromBody] object rejectionData)
        {
            var response = await _apiService.PostAsync<ServiceQuotationDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceQuotations.Reject, id),
                rejectionData
            );

            return Json(response);
        }

        /// <summary>
        /// Lấy danh sách VehicleInspection đã hoàn thành để tạo báo giá
        /// </summary>
        [HttpGet("GetAvailableInspections")]
        public async Task<IActionResult> GetAvailableInspections()
        {
            var response = await _apiService.GetAsync<List<VehicleInspectionDto>>(ApiEndpoints.VehicleInspections.GetAll);
            
            if (response.Success && response.Data != null)
            {
                // Chỉ lấy những inspection đã hoàn thành và chưa có báo giá
                var availableInspections = response.Data
                    .Where(i => i.Status == "Completed")
                    .Select(i => new
                    {
                        value = i.Id.ToString(),
                        text = $"{i.InspectionNumber} - {i.Vehicle?.Brand} {i.Vehicle?.Model} ({i.Vehicle?.LicensePlate}) - {i.Customer?.Name}",
                        vehicleId = i.VehicleId,
                        customerId = i.CustomerId,
                        vehicleInfo = $"{i.Vehicle?.Brand} {i.Vehicle?.Model} - {i.Vehicle?.LicensePlate}",
                        customerName = i.Customer?.Name ?? "Không xác định",
                        inspectionDate = i.InspectionDate
                    }).Cast<object>().ToList();
                
                return Json(availableInspections);
            }

            return Json(new List<object>());
        }

        /// <summary>
        /// Lấy danh sách xe có sẵn cho dropdown (legacy - không dùng trong quy trình mới)
        /// </summary>
        [HttpGet("GetAvailableVehicles")]
        public async Task<IActionResult> GetAvailableVehicles()
        {
            var response = await _apiService.GetAsync<List<VehicleDto>>(ApiEndpoints.Vehicles.GetAll);
            
            if (response.Success && response.Data != null)
            {
                var vehicles = response.Data.Select(v => new
                {
                    value = v.Id.ToString(),
                    text = $"{v.Brand} {v.Model} - {v.LicensePlate}"
                }).Cast<object>().ToList();
                
                return Json(vehicles);
            }

            return Json(new List<object>());
        }

        /// <summary>
        /// Lấy danh sách khách hàng có sẵn cho dropdown
        /// </summary>
        [HttpGet("GetAvailableCustomers")]
        public async Task<IActionResult> GetAvailableCustomers()
        {
            var response = await _apiService.GetAsync<List<CustomerDto>>(ApiEndpoints.Customers.GetAll);
            
            if (response.Success && response.Data != null)
            {
                var customers = response.Data.Select(c => new
                {
                    value = c.Id.ToString(),
                    text = c.Name
                }).Cast<object>().ToList();
                
                return Json(customers);
            }

            return Json(new List<object>());
        }

        /// <summary>
        /// Lấy danh sách dịch vụ có sẵn cho dropdown
        /// </summary>
        [HttpGet("GetAvailableServices")]
        public async Task<IActionResult> GetAvailableServices()
        {
            var response = await _apiService.GetAsync<List<ServiceDto>>(ApiEndpoints.Services.GetAll);
            
            if (response.Success && response.Data != null)
            {
                var services = response.Data.Select(s => new
                {
                    value = s.Id.ToString(),
                    text = $"{s.Name} - {s.Price.ToString("N0")} VNĐ"
                }).Cast<object>().ToList();
                
                return Json(services);
            }

            return Json(new List<object>());
        }

        /// <summary>
        /// Tìm kiếm dịch vụ theo từ khóa cho typeahead
        /// </summary>
        [HttpGet("SearchServices")]
        public async Task<IActionResult> SearchServices(string q)
        {
            var response = await _apiService.GetAsync<List<ServiceDto>>(ApiEndpoints.Services.GetAll);
            
            if (response.Success && response.Data != null)
            {
                var services = response.Data
                    .Where(s => string.IsNullOrEmpty(q) || s.Name.ToLower().Contains(q.ToLower()))
                    .Select(s => new
                    {
                        value = s.Id.ToString(),
                        text = s.Name,
                        price = s.Price
                    }).Cast<object>().ToList();
                
                return Json(services);
            }

            return Json(new List<object>());
        }

        /// <summary>
        /// In báo giá
        /// </summary>
        [HttpGet("PrintQuotation/{id}")]
        public async Task<IActionResult> PrintQuotation(int id)
        {
            try
            {
                // Load quotation data
                var quotationResponse = await _apiService.GetAsync<ServiceQuotationDto>(ApiEndpoints.ServiceQuotations.GetById.Replace("{0}", id.ToString()));
                
                if (!quotationResponse.Success || quotationResponse.Data == null)
                {
                    return NotFound("Không tìm thấy báo giá");
                }

                // Load print template
                var templateResponse = await _apiService.GetAsync<PrintTemplateDto>(ApiEndpoints.PrintTemplates.GetDefault.Replace("{0}", "Quotation"));
                
                var template = templateResponse.Success ? templateResponse.Data : null;
                
                // Debug logging
                if (template == null)
                {
                    // Log error or use fallback
                    System.Diagnostics.Debug.WriteLine($"Template not found. Success: {templateResponse.Success}, Message: {templateResponse.ErrorMessage}");
                }

                // ✅ SỬA: Hiển thị tất cả items (bao gồm cả labor) khi in báo giá
                var quotation = quotationResponse.Data;

                // Create view model
                var viewModel = new PrintQuotationViewModel
                {
                    Quotation = quotation,
                    Template = template
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi tải báo giá: {ex.Message}");
            }
        }

        private static string TranslateQuotationStatus(string status)
        {
            return status switch
            {
                "Draft" => "Nháp",
                "Sent" => "Đã Gửi",
                "Accepted" => "Đã Chấp Nhận",
                "Rejected" => "Đã Từ Chối",
                "Expired" => "Hết Hạn",
                "Converted" => "Đã Chuyển Đổi",
                _ => status
            };
        }
    }
}
