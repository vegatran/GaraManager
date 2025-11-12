using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;
using AutoMapper;
using GarageManagementSystem.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller quản lý phụ tùng với đầy đủ các thao tác CRUD thông qua API
    /// </summary>
    [Authorize]
    [Route("PartsManagement")]
    public class PartsManagementController : Controller
    {
        private readonly ApiService _apiService;
        private readonly IMapper _mapper;

        public PartsManagementController(ApiService apiService, IMapper mapper)
        {
            _apiService = apiService;
            _mapper = mapper;
        }

        /// <summary>
        /// Hiển thị trang quản lý phụ tùng
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Lấy danh sách phụ tùng cho DataTable thông qua API với pagination
        /// </summary>
        [HttpGet("GetParts")]
        public async Task<IActionResult> GetParts(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? category = null,
            string? brand = null)
        {
            try
            {
                // Build query parameters
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (!string.IsNullOrEmpty(searchTerm))
                    queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
                if (!string.IsNullOrEmpty(category))
                    queryParams.Add($"category={Uri.EscapeDataString(category)}");
                if (!string.IsNullOrEmpty(brand))
                    queryParams.Add($"brand={Uri.EscapeDataString(brand)}");

                var endpoint = ApiEndpoints.Parts.GetAll + "?" + string.Join("&", queryParams);
                var response = await _apiService.GetAsync<PagedResponse<PartDto>>(endpoint);
                
                if (response.Success && response.Data != null)
                {
                    return Json(response.Data);
                }
                else
                {
                    return Json(new PagedResponse<PartDto>
                    {
                        Data = new List<PartDto>(),
                        TotalCount = 0,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        Success = false,
                        Message = response.ErrorMessage ?? "Lỗi khi lấy danh sách phụ tùng"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new PagedResponse<PartDto>
                {
                    Data = new List<PartDto>(),
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết phụ tùng theo ID thông qua API
        /// </summary>
        [HttpGet("GetPart/{id}")]
        public async Task<IActionResult> GetPart(int id)
        {
            var response = await _apiService.GetAsync<ApiResponse<PartDto>>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.Parts.GetById, id)
            );
            
            if (response.Success && response.Data?.Data != null)
            {
                var partData = _mapper.Map<PartDetailsViewModel>(response.Data.Data);
                return Json(new { success = true, data = partData });
            }
            
            return Json(new { success = false, error = response.ErrorMessage ?? "Phụ tùng không tồn tại" });
        }

        /// <summary>
        /// Tạo phụ tùng mới thông qua API
        /// </summary>
        [HttpPost("CreatePart")]
        public async Task<IActionResult> CreatePart([FromBody] CreatePartDto partDto)
        {
            try
            {
                // Log input data for debugging
                Console.WriteLine($"DEBUG: CreatePart input: {System.Text.Json.JsonSerializer.Serialize(partDto)}");

                if (!ModelState.IsValid)
                {
                    var errors = new Dictionary<string, string[]>();
                    foreach (var key in ModelState.Keys)
                    {
                        var modelErrors = ModelState[key].Errors.Select(e => e.ErrorMessage).ToArray();
                        if (modelErrors.Length > 0)
                        {
                            errors[key] = modelErrors;
                        }
                    }
                    Console.WriteLine($"DEBUG: ModelState errors: {System.Text.Json.JsonSerializer.Serialize(errors)}");
                    return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = errors });
                }

                var response = await _apiService.PostAsync<PartDto>(
                    ApiEndpoints.Parts.Create,
                    partDto
                );

                if (response.Success)
                {
                    return Json(new { success = true, message = "Tạo phụ tùng thành công" });
                }
                else
                {
                    return Json(new { success = false, message = response.ErrorMessage ?? "Lỗi khi tạo phụ tùng" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: CreatePart exception: {ex.Message}");
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// Cập nhật thông tin phụ tùng thông qua API
        /// </summary>
        [HttpPut("UpdatePart/{id}")]
        public async Task<IActionResult> UpdatePart(int id, [FromBody] UpdatePartDto partDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errorMessage = "Dữ liệu không hợp lệ" });
            }

            if (id != partDto.Id)
            {
                return BadRequest(new { success = false, errorMessage = "ID không khớp" });
            }

            var response = await _apiService.PutAsync<PartDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.Parts.Update, id),
                partDto
            );

            return Json(response);
        }

        /// <summary>
        /// Xóa phụ tùng thông qua API
        /// </summary>
        [HttpDelete("DeletePart/{id}")]
        public async Task<IActionResult> DeletePart(int id)
        {
            var response = await _apiService.DeleteAsync<PartDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.Parts.Delete, id)
            );

            return Json(response);
        }

        /// <summary>
        /// Tìm kiếm phụ tùng cho Typeahead trong Quotation Management
        /// </summary>
        [HttpGet("SearchParts")]
        public async Task<IActionResult> SearchParts(string searchTerm)
        {
            try
            {
                var response = await _apiService.GetAsync<ApiResponse<List<PartDto>>>(
                    ApiEndpoints.Parts.Search + "?searchTerm=" + Uri.EscapeDataString(searchTerm ?? ""));
                
                if (response.Success && response.Data != null)
                {
                    var parts = response.Data.Data.Select(p => new
                    {
                        id = p.Id,
                        partId = p.Id,
                        partName = p.PartName,
                        partNumber = p.PartNumber,
                        text = $"{p.PartName} ({p.PartNumber}) - {p.SellPrice.ToString("N0")} VNĐ",
                        costPrice = p.CostPrice,
                        sellPrice = p.SellPrice,
                        vatRate = p.VATRate,
                        isVATApplicable = p.IsVATApplicable,
                        hasInvoice = p.HasInvoice
                    }).Cast<object>().ToList();
                    
                    return Json(new { success = true, data = parts });
                }

                return Json(new { success = true, data = new List<object>() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi tìm kiếm phụ tùng: {ex.Message}" });
            }
        }

        /// <summary>
        /// Lấy danh sách nhà cung cấp có sẵn cho dropdown
        /// </summary>
        [HttpGet("GetAvailableSuppliers")]
        public async Task<IActionResult> GetAvailableSuppliers()
        {
            var response = await _apiService.GetAsync<List<SupplierDto>>(ApiEndpoints.Suppliers.GetAll);
            
            if (response.Success && response.Data != null)
            {
                var suppliers = response.Data.Select(s => new
                {
                    value = s.Id.ToString(),
                    text = s.SupplierName
                }).Cast<object>().ToList();
                
                return Json(suppliers);
            }

            return Json(new List<object>());
        }

        [HttpGet("Warehouses")]
        public async Task<IActionResult> GetWarehouses()
        {
            var response = await _apiService.GetAsync<List<WarehouseDto>>(ApiEndpoints.Warehouses.GetAll);
            if (response.Success && response.Data != null)
            {
                return Json(new { success = true, data = response.Data });
            }

            return Json(new { success = false, data = new List<object>() });
        }
    }
}
