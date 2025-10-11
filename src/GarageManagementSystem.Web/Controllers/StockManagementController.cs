using GarageManagementSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;

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
        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Lấy danh sách tất cả giao dịch kho cho DataTable thông qua API
        /// </summary>
        [HttpGet]
        [Route("GetStockTransactions")]
        public async Task<IActionResult> GetStockTransactions()
        {
            try
            {
                var response = await _apiService.GetAsync<List<StockTransactionDto>>(ApiEndpoints.StockTransactions.GetAll);
                
                if (response.Success)
                {
                    var transactionList = new List<object>();
                    
                    if (response.Data != null)
                    {
                        transactionList = response.Data.Select(t => new
                        {
                            id = t.Id,
                            transactionNumber = t.TransactionNumber,
                            partName = t.Part?.PartName ?? "N/A",
                            transactionType = t.TransactionTypeDisplay, // Sử dụng display name tiếng Việt
                            quantity = t.Quantity,
                            quantityBefore = t.QuantityBefore,
                            quantityAfter = t.QuantityAfter,
                            unitPrice = t.UnitPrice.ToString("N0") + " VNĐ",
                            totalAmount = t.TotalAmount.ToString("N0") + " VNĐ",
                            transactionDate = t.TransactionDate.ToString("dd/MM/yyyy HH:mm"),
                            referenceNumber = t.ReferenceNumber ?? "N/A",
                            notes = t.Notes ?? "N/A",
                            createdDate = t.CreatedAt.ToString("dd/MM/yyyy HH:mm")
                        }).Cast<object>().ToList();
                    }

                    return Json(new { data = transactionList });
                }
                else
                {
                    return Json(new { error = response.ErrorMessage ?? "Không thể tải dữ liệu giao dịch kho" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = $"Lỗi khi tải dữ liệu: {ex.Message}" });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết giao dịch kho theo ID thông qua API
        /// </summary>
        [HttpGet]
        [Route("GetStockTransaction/{id}")]
        public async Task<IActionResult> GetStockTransaction(int id)
        {
            try
            {
                var response = await _apiService.GetAsync<StockTransactionDto>(string.Format(ApiEndpoints.StockTransactions.GetByPart, id));
                
                if (response.Success && response.Data != null)
                {
                    return Json(new { success = true, data = response.Data });
                }
                else
                {
                    return Json(new { success = false, message = response.ErrorMessage ?? "Không tìm thấy giao dịch kho" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi tải dữ liệu: {ex.Message}" });
            }
        }

        /// <summary>
        /// Lấy danh sách phụ tùng để hiển thị trong dropdown
        /// </summary>
        [HttpGet]
        [Route("GetAvailableParts")]
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
                    }).ToList();

                    return Json(new { success = true, data = parts });
                }
                else
                {
                    return Json(new { success = false, message = response.ErrorMessage ?? "Không thể tải danh sách phụ tùng" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi tải danh sách phụ tùng: {ex.Message}" });
            }
        }

        /// <summary>
        /// Lấy danh sách nhà cung cấp để hiển thị trong dropdown
        /// </summary>
        [HttpGet]
        [Route("GetAvailableSuppliers")]
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
        [HttpPost]
        [Route("CreateStockTransaction")]
        public async Task<IActionResult> CreateStockTransaction([FromBody] CreateStockTransactionDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var response = await _apiService.PostAsync<StockTransactionDto>(
                    ApiEndpoints.StockTransactions.Create, dto);

                if (response.Success)
                {
                    return Json(new { success = true, message = "Tạo giao dịch kho thành công!", data = response.Data });
                }
                else
                {
                    return Json(new { success = false, message = response.ErrorMessage ?? "Không thể tạo giao dịch kho" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi tạo giao dịch kho: {ex.Message}" });
            }
        }

        /// <summary>
        /// Lấy danh sách loại giao dịch kho
        /// </summary>
        [HttpGet]
        [Route("GetTransactionTypes")]
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
        [HttpPost]
        [Route("ImportOpeningBalance")]
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
        [HttpPost]
        [Route("ImportExcel")]
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
        [HttpPost]
        [Route("ValidateExcel")]
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
        [HttpGet]
        [Route("DownloadTemplate")]
        public async Task<IActionResult> DownloadTemplate()
        {
            try
            {
                var response = await _apiService.GetAsync<byte[]>(ApiEndpoints.StockTransactions.DownloadTemplate);
                
                if (response.Success && response.Data != null)
                {
                    var fileName = $"Stock_Import_Template_{DateTime.Now:yyyyMMdd}.xlsx";
                    return File(response.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
                else
                {
                    return BadRequest("Không thể tải template");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi tải template: {ex.Message}");
            }
        }
    }
}
