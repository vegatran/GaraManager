using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;
using System.Linq;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller quản lý nhập xuất kho với đầy đủ các thao tác CRUD thông qua API
    /// </summary>
    [Authorize]
    [Route("StockManagement")]
    public class StockManagementController : Controller
    {
        private readonly ApiService _apiService;

        public StockManagementController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Hiển thị trang quản lý nhập xuất kho
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Lấy danh sách giao dịch kho cho DataTable thông qua API với pagination
        /// </summary>
        [HttpGet("GetStockTransactions")]
        public async Task<IActionResult> GetStockTransactions(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? transactionType = null,
            int? partId = null)
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
                if (!string.IsNullOrEmpty(transactionType))
                    queryParams.Add($"transactionType={Uri.EscapeDataString(transactionType)}");
                if (partId.HasValue)
                    queryParams.Add($"partId={partId.Value}");

                var endpoint = ApiEndpoints.StockTransactions.GetAll + "?" + string.Join("&", queryParams);
                var response = await _apiService.GetAsync<PagedResponse<StockTransactionDto>>(endpoint);
                var drawValue = 0;
                var drawString = Request.Query["draw"].FirstOrDefault();
                if (!string.IsNullOrEmpty(drawString) && int.TryParse(drawString, out var parsedDraw))
                {
                    drawValue = parsedDraw;
                }

                if (response.Success && response.Data != null)
                {
                    return Json(new
                    {
                        success = true,
                        draw = drawValue,
                        recordsTotal = response.Data.TotalCount,
                        recordsFiltered = response.Data.TotalCount,
                        data = response.Data.Data
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        draw = drawValue,
                        recordsTotal = 0,
                        recordsFiltered = 0,
                        data = new List<StockTransactionDto>(),
                        message = response.ErrorMessage ?? "Lỗi khi lấy danh sách giao dịch kho"
                    });
                }
            }
            catch (Exception ex)
            {
                var drawValue = Request.Query["draw"].FirstOrDefault() ?? "0";
                return Json(new
                {
                    success = false,
                    draw = drawValue,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<StockTransactionDto>(),
                    message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết giao dịch kho theo ID thông qua API
        /// </summary>
        [HttpGet("GetStockTransaction/{id}")]
        public async Task<IActionResult> GetStockTransaction(int id)
        {
            try
            {
                var response = await _apiService.GetAsync<ApiResponse<StockTransactionDto>>(
                    ApiEndpoints.Builder.WithId(ApiEndpoints.StockTransactions.GetById, id)
                );
                
                if (response.Success && response.Data != null)
                {
                    var transaction = response.Data.Data;
                    var transactionData = new
                    {
                        id = transaction.Id,
                        transactionNumber = transaction.TransactionNumber,
                        partId = transaction.PartId,
                        partName = transaction.Part?.PartName ?? "N/A",
                        transactionType = transaction.TransactionType,
                        quantity = transaction.Quantity,
                        quantityBefore = transaction.QuantityBefore,
                        quantityAfter = transaction.QuantityAfter,
                        unitPrice = transaction.UnitPrice,
                        totalAmount = transaction.TotalAmount,
                        transactionDate = transaction.TransactionDate,
                        referenceNumber = transaction.ReferenceNumber,
                        notes = transaction.Notes
                    };
                    
                    return Json(new ApiResponse { Data = transactionData, Success = true, StatusCode = System.Net.HttpStatusCode.OK });
                }
                else
                {
                    return Json(new { success = false, error = "Giao dịch kho không tồn tại" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Lỗi: {ex.Message}" });
            }
        }


        /// <summary>
        /// Lấy danh sách nhà cung cấp để hiển thị trong dropdown
        /// </summary>
        [HttpGet("GetAvailableSuppliers")]
        public async Task<IActionResult> GetAvailableSuppliers()
        {
            try
            {
                var response = await _apiService.GetAsync<List<SupplierDto>>(ApiEndpoints.Suppliers.GetAll);
                
                if (response.Success && response.Data != null)
                {
                    var suppliers = response.Data.Select(s => new
                    {
                        id = s.Id,
                        text = s.SupplierName
                    }).ToList();

                    return Json(new { success = true, data = suppliers });
                }
                else
                {
                    return Json(new { success = false, message = response.ErrorMessage ?? "Không thể tải danh sách nhà cung cấp" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi tải danh sách nhà cung cấp: {ex.Message}" });
            }
        }

        /// <summary>
        /// Tạo giao dịch kho mới
        /// </summary>
        [HttpPost("CreateStockTransaction")]
        public async Task<IActionResult> CreateStockTransaction([FromBody] CreateStockTransactionDto dto)
        {
            try
            {
                // Log input data for debugging
                Console.WriteLine($"DEBUG: CreateStockTransaction input: {System.Text.Json.JsonSerializer.Serialize(dto)}");

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

                var response = await _apiService.PostAsync<StockTransactionDto>(
                    ApiEndpoints.StockTransactions.Create, dto);

                if (response.Success)
                {
                    return Json(new { success = true, message = "Tạo giao dịch kho thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = response.ErrorMessage ?? "Không thể tạo giao dịch kho" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: CreateStockTransaction exception: {ex.Message}");
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// ✅ THÊM: Tìm kiếm phụ tùng cho Typeahead
        /// </summary>
        [HttpGet("SearchParts")]
        public async Task<IActionResult> SearchParts(string q)
        {
            try
            {
                // ✅ SỬA: Sử dụng searchTerm thay vì q
                var response = await _apiService.GetAsync<ApiResponse<List<PartDto>>>(ApiEndpoints.Parts.Search + "?searchTerm=" + Uri.EscapeDataString(q ?? ""));
                
                if (response.Success && response.Data != null)
                {
                    var parts = response.Data.Data.Select(p => new
                    {
                        id = p.Id,
                        text = $"{p.PartName} ({p.PartNumber})",
                        costPrice = p.CostPrice,
                        sellPrice = p.SellPrice
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
        /// ✅ THÊM: Lấy danh sách phụ tùng có sẵn cho Typeahead
        /// </summary>
        [HttpGet("GetAvailableParts")]
        public async Task<IActionResult> GetAvailableParts()
        {
            try
            {
                var response = await _apiService.GetAsync<List<PartDto>>(ApiEndpoints.Parts.GetAll);
                
                if (response.Success && response.Data != null)
                {
                    var parts = response.Data.Select(p => new
                    {
                        id = p.Id,
                        text = $"{p.PartName} ({p.PartNumber})"
                    }).Cast<object>().ToList();
                    
                    return Json(new { success = true, data = parts });
                }

                return Json(new { success = true, data = new List<object>() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi lấy danh sách phụ tùng: {ex.Message}" });
            }
        }

        /// <summary>
        /// Lấy danh sách loại giao dịch kho
        /// </summary>
        [HttpGet("GetTransactionTypes")]
        public IActionResult GetTransactionTypes()
        {
            var transactionTypes = new[]
            {
                new { id = "NhapKho", text = "Nhập kho" },
                new { id = "XuatKho", text = "Xuất kho" },
                new { id = "DieuChinh", text = "Điều chỉnh" },
                new { id = "TonDauKy", text = "Tồn đầu kỳ" }
            };

            return Json(new { success = true, data = transactionTypes });
        }

        /// <summary>
        /// Import dữ liệu tồn đầu kỳ từ Excel
        /// </summary>
        [HttpPost("ImportOpeningBalance")]
        public async Task<IActionResult> ImportOpeningBalance([FromBody] OpeningBalanceImportRequest request)
        {
            try
            {
                var response = await _apiService.PostAsync<OpeningBalanceImportResult>(
                    ApiEndpoints.StockTransactions.ImportOpeningBalance, request);

                if (response.Success)
                {
                    return Json(new { 
                        success = true, 
                        message = $"Import thành công {response.Data?.TotalProcessed ?? 0} sản phẩm",
                        data = response.Data 
                    });
                }
                else
                {
                    return Json(new { success = false, message = response.ErrorMessage ?? "Không thể import dữ liệu" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi import dữ liệu: {ex.Message}" });
            }
        }

        /// <summary>
        /// Import dữ liệu từ Excel file
        /// </summary>
        [HttpPost("ImportExcel")]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "Vui lòng chọn file Excel để import" });
                }

                if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
                {
                    return Json(new { success = false, message = "Chỉ hỗ trợ file Excel (.xlsx, .xls)" });
                }

                var response = await _apiService.PostFileAsync<object>(
                    ApiEndpoints.StockTransactions.ImportExcel,
                    file
                );

                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi import Excel: {ex.Message}" });
            }
        }

        /// <summary>
        /// Validate Excel file trước khi import
        /// </summary>
        [HttpPost("ValidateExcel")]
        public async Task<IActionResult> ValidateExcel(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "Vui lòng chọn file Excel để validate" });
                }

                if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
                {
                    return Json(new { success = false, message = "Chỉ hỗ trợ file Excel (.xlsx, .xls)" });
                }

                var response = await _apiService.PostFileAsync<object>(
                    ApiEndpoints.StockTransactions.ValidateExcel,
                    file
                );

                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi validate Excel: {ex.Message}" });
            }
        }

        /// <summary>
        /// Tải template Excel mẫu
        /// </summary>
        [HttpGet("DownloadTemplate")]
        public IActionResult DownloadTemplate()
        {
            try
            {
                // Tạo template Excel đơn giản
                var templateData = CreateExcelTemplate();
                var fileName = $"Stock_Import_Template_{DateTime.Now:yyyyMMdd}.xlsx";
                
                return File(templateData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi tải template: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo template Excel cho nhập kho
        /// </summary>
        private byte[] CreateExcelTemplate()
        {
            // Tạo CSV template đơn giản thay vì Excel phức tạp
            var csvContent = @"Mã Hiệu,Tên VTTH,Đơn Vị,Tồn Đầu Kỳ,Đơn Giá,Ghi Chú
PART001,Lốp Michelin 205/55R16,Chiếc,20,1000000,Lốp xe hạng trung
PART002,Ắc Quy GS 60Ah,Chiếc,15,1500000,Ắc quy xe hạng trung
PART003,Má Phanh BOSCH,Bộ,25,350000,Má phanh chất lượng cao
PART004,Đĩa Phanh BOSCH,Chiếc,20,650000,Đĩa phanh chất lượng cao
PART005,Sơn Xe 2K,Bộ,10,1000000,Sơn xe chất lượng cao
PART006,Dầu Động Cơ 5W-30,Lít,50,150000,Dầu động cơ tổng hợp";

            return System.Text.Encoding.UTF8.GetBytes(csvContent);
        }
    }
}
