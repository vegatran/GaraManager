using GarageManagementSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;
using GarageManagementSystem.Web.Models;
using GarageManagementSystem.Shared.Models;
using AutoMapper;

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
        private readonly IMapper _mapper;

        public QuotationManagementController(ApiService apiService, IMapper mapper)
        {
            _apiService = apiService;
            _mapper = mapper;
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
        /// Lấy danh sách báo giá với phân trang cho DataTable thông qua API
        /// </summary>
        [HttpGet("GetQuotations")]
        public async Task<IActionResult> GetQuotations(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null,
            string? status = null)
        {
            try
            {
                var queryParams = new List<string>();
                queryParams.Add($"pageNumber={pageNumber}");
                queryParams.Add($"pageSize={pageSize}");
                
                if (!string.IsNullOrEmpty(searchTerm))
                    queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
                if (!string.IsNullOrEmpty(status))
                    queryParams.Add($"status={Uri.EscapeDataString(status)}");
                
                var queryString = string.Join("&", queryParams);
                var endpoint = $"{ApiEndpoints.ServiceQuotations.GetAll}?{queryString}";
                
                var response = await _apiService.GetAsync<PagedResponse<ServiceQuotationDto>>(endpoint);
                
                if (response.Success && response.Data != null)
                {
                    var quotationList = response.Data.Data.Select(q => new
                    {
                        id = q.Id,
                        quotationNumber = q.QuotationNumber,
                        vehicleInfo = $"{q.Vehicle?.Brand} {q.Vehicle?.Model} - {q.Vehicle?.LicensePlate}",
                        customerName = q.Customer?.Name ?? "N/A",
                        quotationType = q.QuotationType,
                        // ✅ SỬA: Tính lại totalAmount từ SubTotal + TaxAmount - DiscountAmount
                        totalAmount = (q.SubTotal + q.TaxAmount - q.DiscountAmount).ToString("N0"),
                        status = TranslateQuotationStatus(q.Status),
                        statusOriginal = q.Status, // ✅ THÊM: Lưu status gốc (tiếng Anh) để JavaScript check
                        serviceOrderId = q.ServiceOrderId, // ✅ 2.1.1: Thêm ServiceOrderId để check lock
                        validUntil = q.ValidUntil?.ToString("yyyy-MM-dd") ?? "",
                        createdDate = q.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                    }).ToList();

                    return Json(new { 
                        success = true,
                        data = quotationList,
                        totalCount = response.Data.TotalCount,
                        message = "Lấy danh sách báo giá thành công"
                    });
                }
                else
                {
                    return Json(new { 
                        success = false,
                        message = response.ErrorMessage ?? "Lỗi khi lấy danh sách báo giá"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false,
                    message = "Lỗi khi lấy danh sách báo giá: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết báo giá theo ID thông qua API
        /// </summary>
        [HttpGet("GetQuotation/{id}")]
        public async Task<IActionResult> GetQuotation(int id)
        {
            try
            {
                var response = await _apiService.GetAsync<ApiResponse<ServiceQuotationDto>>(
                    ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceQuotations.GetById, id)
                );
                
                if (response.Success && response.Data != null)
                {
                    var quotation = response.Data.Data;
                    // Lấy thông tin bảo hiểm (nếu có) để trả thêm insuranceFilePath
                    string? insuranceFilePath = null;
                    if (string.Equals(quotation.QuotationType, "Insurance", StringComparison.OrdinalIgnoreCase))
                    {
                        var insuranceRes = await _apiService.GetAsync<ApiResponse<InsuranceApprovedPricingDto>>(
                            ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceQuotations.GetInsuranceApprovedPricing, id)
                        );
                        insuranceFilePath = insuranceRes?.Data?.Data?.InsuranceFilePath;
                    }
                    var quotationData = new
                    {
                        id = quotation.Id,
                        quotationNumber = quotation.QuotationNumber,
                        customerId = quotation.CustomerId,
                        vehicleId = quotation.VehicleId,
                        vehicleInspectionId = quotation.VehicleInspectionId,
                        totalAmount = quotation.TotalAmount,
                        status = quotation.Status,
                        validUntil = quotation.ValidUntil?.ToString("yyyy-MM-dd") ?? "",
                        notes = quotation.CustomerNotes ?? "",
                        customerName = quotation.Customer?.Name ?? "",
                        vehicleInfo = $"{quotation.Vehicle?.Brand} {quotation.Vehicle?.Model} - {quotation.Vehicle?.LicensePlate}",
                        inspectionNumber = quotation.VehicleInspection?.InspectionNumber ?? "",
                        // ✅ THÊM: Trả về quotationType và các field tài chính
                        quotationType = quotation.QuotationType,
                        quotationDate = quotation.QuotationDate.ToString("yyyy-MM-dd"),
                        description = quotation.Description ?? "",
                        terms = quotation.Terms ?? "",
                        insuranceFilePath = insuranceFilePath,
                        discountAmount = quotation.DiscountAmount,
                        subTotal = quotation.SubTotal,
                        taxAmount = quotation.TaxAmount,
                        taxRate = quotation.TaxRate,
                        // ✅ THÊM: Trả về items cho JavaScript
                        items = quotation.Items?.Select(item => new
                        {
                            id = item.Id,
                            serviceId = item.ServiceId,
                            itemName = item.ItemName,
                            quantity = item.Quantity,
                            unitPrice = item.UnitPrice,
                            totalPrice = item.TotalPrice,
                            isOptional = item.IsOptional,
                            hasInvoice = item.HasInvoice,
                            isVATApplicable = item.IsVATApplicable,
                            notes = item.Notes,
                            serviceType = item.ServiceType,
                            itemCategory = item.ItemCategory,
                            vatRate = item.VATRate,
                            // ✅ THÊM: Các field VAT mới
                            overrideVATRate = item.OverrideVATRate,
                            overrideIsVATApplicable = item.OverrideIsVATApplicable,
                            partVATRate = item.PartVATRate,
                            partIsVATApplicable = item.PartIsVATApplicable,
                            service = item.Service != null ? new
                            {
                                id = item.Service.Id,
                                name = item.Service.Name,
                                price = item.Service.Price
                            } : null
                        }).ToList()
                    };
                    
                    return Json(new { success = true, data = quotationData });
                }
                else
                {
                    return Json(new { success = false, error = response.ErrorMessage ?? "Không tìm thấy báo giá" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi lấy thông tin báo giá: " + ex.Message });
            }
        }

        /// <summary>
        /// Tạo báo giá mới thông qua API
        /// </summary>
        [HttpPost("CreateQuotation")]
        public async Task<IActionResult> CreateQuotation([FromBody] CreateServiceQuotationDto quotationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(new { success = false, error = "Dữ liệu không hợp lệ: " + string.Join(", ", errors) });
                }

                var response = await _apiService.PostAsync<ApiResponse<ServiceQuotationDto>>(
                    ApiEndpoints.ServiceQuotations.Create,
                    quotationDto
                );

                if (response.Success)
                {
                    return Json(new { success = true, message = "Tạo báo giá thành công" });
                }
                else
                {
                    return Json(new { success = false, error = response.ErrorMessage ?? "Lỗi khi tạo báo giá" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi tạo báo giá: " + ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật thông tin báo giá thông qua API
        /// </summary>
        [HttpPut("UpdateQuotation/{id}")]
        public async Task<IActionResult> UpdateQuotation(int id, [FromBody] UpdateServiceQuotationDto quotationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(new { success = false, error = "Dữ liệu không hợp lệ: " + string.Join(", ", errors) });
                }

                if (id != quotationDto.Id)
                {
                    return BadRequest(new { success = false, error = "ID không khớp" });
                }

                var response = await _apiService.PutAsync<ApiResponse<ServiceQuotationDto>>(
                    ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceQuotations.Update, id),
                    quotationDto
                );

                if (response.Success)
                {
                    return Json(new { success = true, message = "Cập nhật báo giá thành công" });
                }
                else
                {
                    // Xây dựng thông báo lỗi chi tiết
                    var errorMessage = "Lỗi khi cập nhật báo giá";
                    
                    if (!string.IsNullOrEmpty(response.Message))
                    {
                        errorMessage = response.Message;
                    }
                    
                    if (!string.IsNullOrEmpty(response.ErrorMessage))
                    {
                        errorMessage += ": " + response.ErrorMessage;
                    }
                    
                    // Thêm errors chi tiết nếu có
                    if (response.Errors != null && response.Errors.Any())
                    {
                        errorMessage += "\n" + string.Join("\n", response.Errors);
                    }
                    
                    return Json(new { success = false, error = errorMessage });
                }
            }
            catch (Exception ex)
            {
                var errorMessage = "Lỗi khi cập nhật báo giá";
                if (ex.InnerException != null)
                {
                    errorMessage += $": {ex.InnerException.Message}";
                }
                else
                {
                    errorMessage += $": {ex.Message}";
                }
                return Json(new { success = false, error = errorMessage });
            }
        }

        /// <summary>
        /// Xóa báo giá thông qua API
        /// </summary>
        [HttpDelete("DeleteQuotation/{id}")]
        public async Task<IActionResult> DeleteQuotation(int id)
        {
            try
            {
                var response = await _apiService.DeleteAsync<ApiResponse<bool>>(
                    ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceQuotations.Delete, id)
                );

                if (response.Success)
                {
                    return Json(new { success = true, message = "Xóa báo giá thành công" });
                }
                else
                {
                    return Json(new { success = false, error = response.ErrorMessage ?? "Lỗi khi xóa báo giá" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi xóa báo giá: " + ex.Message });
            }
        }

        /// <summary>
        /// Duyệt báo giá
        /// </summary>
        [HttpPost("ApproveQuotation/{id}")]
        public async Task<IActionResult> ApproveQuotation(int id, [FromBody] ApproveQuotationDto approveDto)
        {
            var response = await _apiService.PostAsync<ServiceQuotationDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceQuotations.Approve, id),
                approveDto
            );

            return Json(response);
        }

        /// <summary>
        /// Từ chối báo giá
        /// </summary>
        [HttpPost("RejectQuotation/{id}")]
        public async Task<IActionResult> RejectQuotation(int id, [FromBody] RejectQuotationDto rejectDto)
        {
            var response = await _apiService.PostAsync<ServiceQuotationDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceQuotations.Reject, id),
                rejectDto
            );

            return Json(response);
        }

        /// <summary>
        /// Lấy danh sách VehicleInspection đã hoàn thành để tạo báo giá
        /// </summary>
        [HttpGet("GetAvailableInspections")]
        public async Task<IActionResult> GetAvailableInspections()
        {
            try
            {
                var response = await _apiService.GetAsync<PagedResponse<VehicleInspectionDto>>(ApiEndpoints.VehicleInspections.GetAll);
                
                if (response.Success && response.Data != null)
                {
                    // Chỉ lấy những inspection đã hoàn thành và chưa có báo giá
                    var availableInspections = response.Data.Data
                        .Where(i => i.Status == "Completed")
                        .Select(i => new
                        {
                            value = i.Id.ToString(),
                            text = $"{i.InspectionNumber} - {i.Vehicle?.Brand} {i.Vehicle?.Model} ({i.Vehicle?.LicensePlate}) - {i.Customer?.Name}",
                            vehicleId = i.VehicleId,
                            customerId = i.CustomerId,
                            vehicleInfo = $"{i.Vehicle?.Brand} {i.Vehicle?.Model} - {i.Vehicle?.LicensePlate}",
                            customerName = i.Customer?.Name ?? "Không xác định",
                            inspectionDate = i.InspectionDate.ToString("yyyy-MM-dd")
                        }).ToList();
                    
                    return Json(availableInspections);
                }

                return Json(new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }

        /// <summary>
        /// Lấy danh sách xe có sẵn cho dropdown (legacy - không dùng trong quy trình mới)
        /// </summary>
        [HttpGet("GetAvailableVehicles")]
        public async Task<IActionResult> GetAvailableVehicles()
        {
            try
            {
                var response = await _apiService.GetAsync<List<VehicleDto>>(ApiEndpoints.Vehicles.GetAllForDropdown);
                
                if (response.Success && response.Data != null)
                {
                    var vehicles = response.Data.Select(v => new
                    {
                        value = v.Id.ToString(),
                        text = $"{v.Brand} {v.Model} - {v.LicensePlate}"
                    }).ToList();
                    
                    return Json(vehicles);
                }

                return Json(new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }

        /// <summary>
        /// Lấy danh sách khách hàng có sẵn cho dropdown
        /// </summary>
        [HttpGet("GetAvailableCustomers")]
        public async Task<IActionResult> GetAvailableCustomers()
        {
            try
            {
                var response = await _apiService.GetAsync<List<CustomerDto>>(ApiEndpoints.Customers.GetAllForDropdown);
                
                if (response.Success && response.Data != null)
                {
                    var customers = response.Data.Select(c => new
                    {
                        value = c.Id.ToString(),
                        text = c.Name
                    }).ToList();
                    
                    return Json(customers);
                }

                return Json(new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
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
        /// In báo giá
        /// </summary>
        [HttpGet("PrintQuotation/{id}")]
        public async Task<IActionResult> PrintQuotation(int id)
        {
            try
            {
                // Load quotation data
                var quotationResponse = await _apiService.GetAsync<ApiResponse<ServiceQuotationDto>>(ApiEndpoints.ServiceQuotations.GetById.Replace("{0}", id.ToString()));
                
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
                var quotation = quotationResponse.Data.Data;

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
                "Pending" => "Chờ Duyệt",
                "Sent" => "Đã Gửi",
                "Accepted" => "Đã Chấp Nhận",
                "Approved" => "Đã Duyệt",
                "Rejected" => "Đã Từ Chối",
                "Expired" => "Hết Hạn",
                "Converted" => "Đã Chuyển Đổi",
                _ => status
            };
        }

        /// <summary>
        /// Lấy danh sách file đính kèm của báo giá
        /// </summary>
        [HttpGet("GetAttachments/{quotationId}")]
        public async Task<IActionResult> GetAttachments(int quotationId)
        {
            try
            {
                var response = await _apiService.GetAsync<List<QuotationAttachmentDto>>(
                    string.Format(ApiEndpoints.QuotationAttachments.GetByQuotationId, quotationId));
                
                if (response.Success)
                {
                    return Json(new { success = true, data = response.Data });
                }
                
                return Json(new { success = false, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi không xác định" });
            }
        }

        /// <summary>
        /// Lấy danh sách tài liệu bảo hiểm của báo giá
        /// </summary>
        [HttpGet("GetInsuranceDocuments/{quotationId}")]
        public async Task<IActionResult> GetInsuranceDocuments(int quotationId)
        {
            try
            {
                var response = await _apiService.GetAsync<List<QuotationAttachmentDto>>(
                    string.Format(ApiEndpoints.QuotationAttachments.GetInsuranceDocumentsByQuotationId, quotationId));
                
                if (response.Success)
                {
                    return Json(new { success = true, data = response.Data });
                }
                
                return Json(new { success = false, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi không xác định" });
            }
        }

        /// <summary>
        /// Upload file đính kèm cho báo giá
        /// </summary>
        [HttpPost("UploadAttachment")]
        public async Task<IActionResult> UploadAttachment(int quotationId, IFormFile file, string attachmentType = "General", string description = "", bool isInsuranceDocument = false)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "File không được để trống" });
                }

                var createDto = new CreateQuotationAttachmentDto
                {
                    ServiceQuotationId = quotationId,
                    AttachmentType = attachmentType,
                    Description = description,
                    IsInsuranceDocument = isInsuranceDocument
                };

                var response = await _apiService.PostFormAsync<QuotationAttachmentDto>(
                    ApiEndpoints.QuotationAttachments.Upload, createDto, file);
                
                if (response.Success)
                {
                    return Json(new { success = true, data = response.Data, message = "Upload file thành công" });
                }
                
                return Json(new { success = false, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi không xác định" });
            }
        }

        /// <summary>
        /// Xóa file đính kèm
        /// </summary>
        [HttpDelete("DeleteAttachment/{attachmentId}")]
        public async Task<IActionResult> DeleteAttachment(int attachmentId)
        {
            try
            {
                var response = await _apiService.DeleteAsync<bool>(
                    string.Format(ApiEndpoints.QuotationAttachments.Delete, attachmentId));
                
                if (response.Success)
                {
                    return Json(new { success = true, message = "Xóa file thành công" });
                }
                
                return Json(new { success = false, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi không xác định" });
            }
        }

        /// <summary>
        /// Cập nhật bảng giá duyệt của bảo hiểm
        /// </summary>
        [HttpPost("UpdateInsuranceApprovedPricing/{quotationId}")]
        public async Task<IActionResult> UpdateInsuranceApprovedPricing(int quotationId, IFormFile insuranceFile, 
            string insuranceCompany, string taxCode, string policyNumber, string approvalDate, 
            string approvedAmount, string customerCoPayment, string approvalNotes, string approvedItems)
        {
            try
            {
                // Tạo pricingData từ form data
                var pricingData = new InsuranceApprovedPricingDto
                {
                    QuotationId = quotationId,
                    InsuranceCompany = insuranceCompany ?? "",
                    TaxCode = taxCode ?? "",
                    PolicyNumber = policyNumber ?? "",
                    ApprovalDate = !string.IsNullOrEmpty(approvalDate) ? DateTime.Parse(approvalDate) : null,
                    ApprovedAmount = decimal.TryParse(approvedAmount, out var amount) ? amount : 0,
                    CustomerCoPayment = decimal.TryParse(customerCoPayment, out var coPayment) ? coPayment : 0,
                    ApprovalNotes = approvalNotes ?? "",
                    ApprovedItems = !string.IsNullOrEmpty(approvedItems) ? 
                        System.Text.Json.JsonSerializer.Deserialize<List<InsuranceApprovedItemDto>>(approvedItems) ?? new List<InsuranceApprovedItemDto>() : 
                        new List<InsuranceApprovedItemDto>()
                };

                // Xử lý file upload
                if (insuranceFile != null && insuranceFile.Length > 0)
                {
                    // Tạo thư mục lưu file nếu chưa có
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "insurance-files");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Tạo tên file unique
                    var fileName = $"insurance_{quotationId}_{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(insuranceFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Lưu file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await insuranceFile.CopyToAsync(stream);
                    }

                    // Lưu đường dẫn file public để API persist và UI hiển thị/tải xuống
                    var publicPath = $"/uploads/insurance-files/{fileName}";
                    pricingData.InsuranceFilePath = publicPath;
                    // Ghi chú bổ sung (không bắt buộc)
                    pricingData.ApprovalNotes += $"\n\nFile đối chứng: {fileName}";
                }

                var response = await _apiService.PostAsync<ServiceQuotationDto>(
                    ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceQuotations.UpdateInsuranceApprovedPricing, quotationId),
                    pricingData
                );

                if (response.Success)
                {
                    // Lấy lại dữ liệu insurance pricing mới nhất để trả về client
                    var latestPricing = await _apiService.GetAsync<InsuranceApprovedPricingDto>(
                        ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceQuotations.GetInsuranceApprovedPricing, quotationId)
                    );

                    return Json(new
                    {
                        success = true,
                        message = "Cập nhật bảng giá duyệt của bảo hiểm thành công",
                        data = latestPricing?.Data
                    });
                }
                else
                {
                    return Json(new { success = false, error = response.ErrorMessage ?? "Lỗi khi cập nhật bảng giá duyệt của bảo hiểm" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi cập nhật bảng giá duyệt của bảo hiểm: " + ex.Message });
            }
        }

        /// <summary>
        /// Lấy bảng giá duyệt của bảo hiểm cho báo giá
        /// </summary>
        [HttpGet("GetInsuranceApprovedPricing/{quotationId}")]
        public async Task<IActionResult> GetInsuranceApprovedPricing(int quotationId)
        {
            try
            {
                var response = await _apiService.GetAsync<ApiResponse<InsuranceApprovedPricingDto>>(
                    ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceQuotations.GetInsuranceApprovedPricing, quotationId)
                );

                if (response.Success)
                {
                    return Json(new { success = true, data = response.Data?.Data });
                }
                else
                {
                    return Json(new { success = false, error = response.ErrorMessage ?? "Lỗi khi lấy bảng giá duyệt của bảo hiểm" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi lấy bảng giá duyệt của bảo hiểm: " + ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật bảng giá duyệt của công ty
        /// </summary>
        [HttpPost("UpdateCorporateApprovedPricing/{quotationId}")]
        public async Task<IActionResult> UpdateCorporateApprovedPricing(int quotationId, IFormFile contractFile, 
            string companyName, string taxCode, string contractNumber, string approvalDate, 
            string approvedAmount, string customerCoPayment, string approvalNotes, string approvedItems)
        {
            try
            {
                // Tạo pricingData từ form data
                var pricingData = new CorporateApprovedPricingDto
                {
                    QuotationId = quotationId,
                    CompanyName = companyName ?? "",
                    TaxCode = taxCode ?? "",
                    ContractNumber = contractNumber ?? "",
                    ApprovalDate = !string.IsNullOrEmpty(approvalDate) ? DateTime.Parse(approvalDate) : null,
                    ApprovedAmount = decimal.TryParse(approvedAmount, out var amount) ? amount : 0,
                    CustomerCoPayment = decimal.TryParse(customerCoPayment, out var coPayment) ? coPayment : 0,
                    ApprovalNotes = approvalNotes ?? "",
                    ApprovedItems = !string.IsNullOrEmpty(approvedItems) ? 
                        System.Text.Json.JsonSerializer.Deserialize<List<CorporateApprovedItemDto>>(approvedItems) ?? new List<CorporateApprovedItemDto>() : 
                        new List<CorporateApprovedItemDto>()
                };

                // Xử lý file upload
                if (contractFile != null && contractFile.Length > 0)
                {
                    // Tạo thư mục lưu file nếu chưa có
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "corporate-files");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Tạo tên file unique
                    var fileName = $"corporate_{quotationId}_{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(contractFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Lưu file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await contractFile.CopyToAsync(stream);
                    }

                    // Lưu thông tin file vào database
                    pricingData.ApprovalNotes += $"\n\nFile hợp đồng: {fileName}";
                }

                var response = await _apiService.PostAsync<ServiceQuotationDto>(
                    ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceQuotations.UpdateCorporateApprovedPricing, quotationId),
                    pricingData
                );

                if (response.Success)
                {
                    return Json(new { success = true, message = "Cập nhật bảng giá duyệt của công ty thành công" });
                }
                else
                {
                    return Json(new { success = false, error = response.ErrorMessage ?? "Lỗi khi cập nhật bảng giá duyệt của công ty" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi cập nhật bảng giá duyệt của công ty: " + ex.Message });
            }
        }

        /// <summary>
        /// Lấy bảng giá duyệt của công ty cho báo giá
        /// </summary>
        [HttpGet("GetCorporateApprovedPricing/{quotationId}")]
        public async Task<IActionResult> GetCorporateApprovedPricing(int quotationId)
        {
            try
            {
                var response = await _apiService.GetAsync<CorporateApprovedPricingDto>(
                    ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceQuotations.GetCorporateApprovedPricing, quotationId)
                );

                if (response.Success)
                {
                    return Json(new { success = true, data = response.Data });
                }
                else
                {
                    return Json(new { success = false, error = response.ErrorMessage ?? "Lỗi khi lấy bảng giá duyệt của công ty" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi lấy bảng giá duyệt của công ty: " + ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật trạng thái báo giá (chỉ cho xe cá nhân)
        /// </summary>
        [HttpPost("UpdateQuotationStatus/{quotationId}")]
        public async Task<IActionResult> UpdateQuotationStatus(int quotationId, string status)
        {
            try
            {
                var statusDto = _mapper.Map<UpdateQuotationStatusDto>(new { Status = status });
                
                var response = await _apiService.PutAsync<ServiceQuotationDto>(
                    ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceQuotations.UpdateStatus, quotationId),
                    statusDto
                );

                if (response.Success)
                {
                    return Json(new { success = true, message = "Cập nhật trạng thái báo giá thành công" });
                }
                else
                {
                    return Json(new { success = false, error = response.ErrorMessage ?? "Lỗi khi cập nhật trạng thái báo giá" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi cập nhật trạng thái báo giá: " + ex.Message });
            }
        }
    }
}
