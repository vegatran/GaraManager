using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller quản lý hóa đơn bảo hiểm
    /// </summary>
    [Authorize]
    [Route("InsuranceInvoice")]
    public class InsuranceInvoiceController : Controller
    {
        private readonly ApiService _apiService;

        public InsuranceInvoiceController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Danh sách hóa đơn bảo hiểm
        /// </summary>
        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> Index(int? serviceOrderId = null, string? insuranceCompany = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (serviceOrderId.HasValue)
                    queryParams.Add($"serviceOrderId={serviceOrderId.Value}");
                if (!string.IsNullOrEmpty(insuranceCompany))
                    queryParams.Add($"insuranceCompany={Uri.EscapeDataString(insuranceCompany)}");

                var endpoint = "insuranceinvoices";
                if (queryParams.Any())
                    endpoint += "?" + string.Join("&", queryParams);

                var response = await _apiService.GetAsync<List<InsuranceInvoiceDto>>(endpoint);

                if (response.Success)
                {
                    ViewBag.ServiceOrderId = serviceOrderId;
                    ViewBag.InsuranceCompany = insuranceCompany;
                    return View(response.Data ?? new List<InsuranceInvoiceDto>());
                }
                else
                {
                    TempData["ErrorMessage"] = response.ErrorMessage ?? "Không thể tải danh sách hóa đơn bảo hiểm";
                    return View(new List<InsuranceInvoiceDto>());
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi tải danh sách hóa đơn bảo hiểm: {ex.Message}";
                return View(new List<InsuranceInvoiceDto>());
            }
        }

        /// <summary>
        /// Chi tiết hóa đơn bảo hiểm
        /// </summary>
        [HttpGet("GetInsuranceInvoice/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var response = await _apiService.GetAsync<InsuranceInvoiceDto>($"insuranceinvoices/{id}");

                if (response.Success && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    TempData["ErrorMessage"] = response.ErrorMessage ?? $"Không tìm thấy hóa đơn bảo hiểm với ID {id}";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi tải chi tiết hóa đơn bảo hiểm: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Tạo hóa đơn bảo hiểm mới
        /// </summary>
        [HttpGet("Create")]
        public async Task<IActionResult> Create(int serviceOrderId)
        {
            try
            {
                // Lấy thông tin ServiceOrder để pre-fill form
                var serviceOrderResponse = await _apiService.GetAsync<object>($"serviceorders/{serviceOrderId}");
                
                ViewBag.ServiceOrderId = serviceOrderId;
                
                if (serviceOrderResponse.Success)
                {
                    ViewBag.ServiceOrderData = serviceOrderResponse.Data;
                }

                return View(new InsuranceInvoiceDto
                {
                    ServiceOrderId = serviceOrderId,
                    AccidentDate = DateTime.Now,
                    Items = new List<InsuranceInvoiceItemDto>()
                });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi tải form tạo hóa đơn bảo hiểm: {ex.Message}";
                return RedirectToAction("Index", "ServiceOrderManagement");
            }
        }

        /// <summary>
        /// Xử lý tạo hóa đơn bảo hiểm
        /// </summary>
        [HttpPost("CreateInsuranceInvoice")]
        public async Task<IActionResult> Create(InsuranceInvoiceDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.ServiceOrderId = model.ServiceOrderId;
                    return View(model);
                }

                var response = await _apiService.PostAsync<InsuranceInvoiceDto>("insuranceinvoices", model);

                if (response.Success && response.Data != null)
                {
                    TempData["SuccessMessage"] = "Tạo hóa đơn bảo hiểm thành công";
                    return RedirectToAction(nameof(Details), new { id = response.Data.Id });
                }
                else
                {
                    TempData["ErrorMessage"] = response.ErrorMessage ?? "Không thể tạo hóa đơn bảo hiểm";
                    ViewBag.ServiceOrderId = model.ServiceOrderId;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi tạo hóa đơn bảo hiểm: {ex.Message}";
                ViewBag.ServiceOrderId = model.ServiceOrderId;
                return View(model);
            }
        }

        /// <summary>
        /// Chỉnh sửa hóa đơn bảo hiểm
        /// </summary>
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var response = await _apiService.GetAsync<InsuranceInvoiceDto>($"insuranceinvoices/{id}");

                if (response.Success && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    TempData["ErrorMessage"] = response.ErrorMessage ?? $"Không tìm thấy hóa đơn bảo hiểm với ID {id}";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi tải form chỉnh sửa hóa đơn bảo hiểm: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Xử lý cập nhật hóa đơn bảo hiểm
        /// </summary>
        [HttpPut("UpdateInsuranceInvoice/{id}")]
        public async Task<IActionResult> Edit(int id, InsuranceInvoiceDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var response = await _apiService.PutAsync<InsuranceInvoiceDto>($"insuranceinvoices/{id}", model);

                if (response.Success && response.Data != null)
                {
                    TempData["SuccessMessage"] = "Cập nhật hóa đơn bảo hiểm thành công";
                    return RedirectToAction(nameof(Details), new { id });
                }
                else
                {
                    TempData["ErrorMessage"] = response.ErrorMessage ?? "Không thể cập nhật hóa đơn bảo hiểm";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi cập nhật hóa đơn bảo hiểm: {ex.Message}";
                return View(model);
            }
        }

        /// <summary>
        /// Xóa hóa đơn bảo hiểm
        /// </summary>
        [HttpDelete("DeleteInsuranceInvoice/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _apiService.DeleteAsync<bool>($"insuranceinvoices/{id}");

                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Xóa hóa đơn bảo hiểm thành công";
                }
                else
                {
                    TempData["ErrorMessage"] = response.ErrorMessage ?? "Không thể xóa hóa đơn bảo hiểm";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi xóa hóa đơn bảo hiểm: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Xuất PDF hóa đơn bảo hiểm
        /// </summary>
        [HttpGet("ExportPdf/{id}")]
        public async Task<IActionResult> ExportPdf(int id)
        {
            try
            {
                var response = await _apiService.GetAsync<byte[]>($"insuranceinvoices/{id}/export/pdf");
                
                if (response.Success && response.Data != null)
                {
                    var fileName = $"Insurance_Invoice_{id}_{DateTime.Now:yyyyMMdd}.pdf";
                    return File(response.Data, "application/pdf", fileName);
                }
                else
                {
                    TempData["ErrorMessage"] = response.ErrorMessage ?? "Không thể xuất PDF hóa đơn bảo hiểm";
                    return RedirectToAction(nameof(Details), new { id });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi xuất PDF hóa đơn bảo hiểm: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        /// <summary>
        /// Xuất Excel hóa đơn bảo hiểm
        /// </summary>
        [HttpGet("ExportExcel/{id}")]
        public async Task<IActionResult> ExportExcel(int id)
        {
            try
            {
                var response = await _apiService.GetAsync<byte[]>($"insuranceinvoices/{id}/export/excel");
                
                if (response.Success && response.Data != null)
                {
                    var fileName = $"Insurance_Invoice_{id}_{DateTime.Now:yyyyMMdd}.xlsx";
                    return File(response.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
                else
                {
                    TempData["ErrorMessage"] = response.ErrorMessage ?? "Không thể xuất Excel hóa đơn bảo hiểm";
                    return RedirectToAction(nameof(Details), new { id });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi xuất Excel hóa đơn bảo hiểm: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}
