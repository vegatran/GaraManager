using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Core.Enums;
using GarageManagementSystem.Core.Services;
using GarageManagementSystem.Core.DTOs;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarageManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiScope")]
    public class StockTransactionsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExcelImportService _excelImportService;

        public StockTransactionsController(IUnitOfWork unitOfWork, IExcelImportService excelImportService)
        {
            _unitOfWork = unitOfWork;
            _excelImportService = excelImportService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<StockTransactionDto>>>> GetStockTransactions()
        {
            try
            {
                var transactions = await _unitOfWork.StockTransactions.GetAllAsync();
                return Ok(ApiResponse<List<StockTransactionDto>>.SuccessResult(transactions.Select(MapToDto).ToList()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<StockTransactionDto>>.ErrorResult("Lỗi khi lấy danh sách giao dịch kho", ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<StockTransactionDto>>> GetStockTransaction(int id)
        {
            try
            {
                var transaction = await _unitOfWork.StockTransactions.GetByIdAsync(id);
                if (transaction == null)
                {
                    return NotFound(ApiResponse<StockTransactionDto>.ErrorResult("Giao dịch kho không tồn tại"));
                }
                return Ok(ApiResponse<StockTransactionDto>.SuccessResult(MapToDto(transaction)));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<StockTransactionDto>.ErrorResult("Lỗi khi lấy giao dịch kho", ex.Message));
            }
        }

        [HttpGet("part/{partId}")]
        public async Task<ActionResult<ApiResponse<List<StockTransactionDto>>>> GetByPart(int partId)
        {
            try
            {
                var transactions = await _unitOfWork.StockTransactions.GetByPartIdAsync(partId);
                return Ok(ApiResponse<List<StockTransactionDto>>.SuccessResult(transactions.Select(MapToDto).ToList()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<StockTransactionDto>>.ErrorResult("Lỗi khi lấy giao dịch kho", ex.Message));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<StockTransactionDto>>> CreateTransaction(CreateStockTransactionDto dto)
        {
            try
            {
                var part = await _unitOfWork.Parts.GetByIdAsync(dto.PartId);
                if (part == null) return BadRequest(ApiResponse<StockTransactionDto>.ErrorResult("Part not found"));

                var qtyBefore = part.QuantityInStock;
                var qtyChange = TransactionTypeHelper.IsIncrease(dto.TransactionType) ? dto.Quantity : -dto.Quantity;
                var qtyAfter = qtyBefore + qtyChange;

                if (qtyAfter < 0) return BadRequest(ApiResponse<StockTransactionDto>.ErrorResult("Insufficient stock"));

                // Convert string to enum
                var transactionType = dto.TransactionType switch
                {
                    "NhapKho" => StockTransactionType.NhapKho,
                    "XuatKho" => StockTransactionType.XuatKho,
                    "DieuChinh" => StockTransactionType.DieuChinh,
                    "TonDauKy" => StockTransactionType.TonDauKy,
                    _ => StockTransactionType.NhapKho
                };

                var transaction = new Core.Entities.StockTransaction
                {
                    TransactionNumber = await _unitOfWork.StockTransactions.GenerateTransactionNumberAsync(),
                    PartId = dto.PartId,
                    TransactionType = transactionType,
                    Quantity = dto.Quantity,
                    QuantityBefore = qtyBefore,
                    QuantityAfter = qtyAfter,
                    UnitPrice = dto.UnitPrice,
                    TotalAmount = dto.Quantity * dto.UnitPrice,
                    TransactionDate = DateTime.Now,
                    ServiceOrderId = dto.ServiceOrderId,
                    SupplierId = dto.SupplierId,
                    ReferenceNumber = dto.ReferenceNumber,
                    Notes = dto.Notes,
                    ProcessedById = dto.ProcessedById
                };

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.StockTransactions.AddAsync(transaction);
                    
                    // Update part stock
                    part.QuantityInStock = qtyAfter;
                    await _unitOfWork.Parts.UpdateAsync(part);
                    
                    await _unitOfWork.SaveChangesAsync();

                    // Commit transaction nếu thành công
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch
                {
                    // Rollback transaction nếu có lỗi
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
                return Ok(ApiResponse<StockTransactionDto>.SuccessResult(MapToDto(transaction), "Transaction recorded"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<StockTransactionDto>.ErrorResult("Lỗi khi tạo giao dịch kho", ex.Message));
            }
        }

        [HttpPost("opening-balance")]
        public async Task<ActionResult<ApiResponse<OpeningBalanceImportResult>>> ImportOpeningBalance(OpeningBalanceImportRequest request)
        {
            try
            {
                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                var result = new OpeningBalanceImportResult();
                var processedCount = 0;
                var successCount = 0;
                var errors = new List<string>();

                foreach (var item in request.Items.Where(x => x.IsValid))
                {
                    try
                    {
                        processedCount++;

                        // Tìm hoặc tạo Part
                        var part = await _unitOfWork.Parts.GetByPartNumberAsync(item.PartNumber);
                        if (part == null)
                        {
                            // Tạo Part mới
                            part = new Core.Entities.Part
                            {
                                PartNumber = item.PartNumber,
                                PartName = item.PartName,
                                Category = item.Category,
                                Brand = item.Brand,
                                Unit = item.Unit,
                                Location = item.Location,
                                CompatibleVehicles = item.CompatibleVehicles,
                                CostPrice = item.UnitPrice,
                                AverageCostPrice = item.UnitPrice,
                                SellPrice = item.UnitPrice * 1.2m, // Mặc định giá bán = 120% giá nhập
                                QuantityInStock = item.Quantity,
                                MinimumStock = 0,
                                IsActive = true,
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            };

                            await _unitOfWork.Parts.AddAsync(part);
                        }
                        else
                        {
                            // Cập nhật Part hiện tại
                            part.PartName = item.PartName;
                            part.Category = item.Category;
                            part.Brand = item.Brand;
                            part.Unit = item.Unit;
                            part.Location = item.Location;
                            part.CompatibleVehicles = item.CompatibleVehicles;
                            part.AverageCostPrice = item.UnitPrice;
                            part.QuantityInStock = item.Quantity;
                            part.UpdatedAt = DateTime.Now;

                            await _unitOfWork.Parts.UpdateAsync(part);
                        }

                        await _unitOfWork.SaveChangesAsync();

                        // Tạo StockTransaction cho Opening Balance
                        var transaction = new Core.Entities.StockTransaction
                        {
                            TransactionNumber = $"{TransactionTypeHelper.GetTransactionPrefix("TonDauKy")}-{DateTime.Now:yyyyMMdd}-{processedCount:D4}",
                            PartId = part.Id,
                            TransactionType = StockTransactionType.TonDauKy,
                            Quantity = item.Quantity,
                            QuantityBefore = 0,
                            QuantityAfter = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            TotalAmount = item.TotalValue,
                            TransactionDate = request.OpeningBalanceDate,
                            ServiceOrderId = null,
                            SupplierId = null,
                            ReferenceNumber = "Opening Balance Import",
                            Notes = $"Tồn đầu kỳ - {item.Notes}",
                            ProcessedById = null, // Có thể lấy từ current user
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };

                        await _unitOfWork.StockTransactions.AddAsync(transaction);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Lỗi xử lý {item.PartNumber}: {ex.Message}");
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                result.Success = true;
                result.TotalProcessed = processedCount;
                result.SuccessCount = successCount;
                result.ErrorCount = errors.Count;
                result.Errors = errors;
                result.Message = $"Đã import thành công {successCount}/{processedCount} phụ tùng. Tổng giá trị: {request.Items.Where(x => x.IsValid).Sum(x => x.TotalValue):N0} VND";

                return Ok(ApiResponse<OpeningBalanceImportResult>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return StatusCode(500, ApiResponse<OpeningBalanceImportResult>.ErrorResult("Lỗi khi import tồn đầu kỳ", ex.Message));
            }
        }

        [HttpPost("opening-balance/validate")]
        public ActionResult<ApiResponse<OpeningBalanceImportRequest>> ValidateOpeningBalance(OpeningBalanceImportRequest request)
        {
            try
            {
                var result = new OpeningBalanceImportRequest
                {
                    Items = request.Items,
                    OpeningBalanceDate = request.OpeningBalanceDate,
                    Notes = request.Notes
                };

                // Validate each item
                foreach (var item in result.Items)
                {
                    if (!item.IsValid)
                    {
                        // Log validation error
                    }
                }

                return Ok(ApiResponse<OpeningBalanceImportRequest>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<OpeningBalanceImportRequest>.ErrorResult("Lỗi khi validate dữ liệu", ex.Message));
            }
        }

        private static StockTransactionDto MapToDto(Core.Entities.StockTransaction st) => new()
        {
            Id = st.Id, 
            TransactionNumber = st.TransactionNumber, 
            PartId = st.PartId,
            TransactionType = st.TransactionType.ToString(), 
            Quantity = st.Quantity,
            QuantityBefore = st.QuantityBefore, 
            QuantityAfter = st.QuantityAfter,
            UnitPrice = st.UnitPrice, 
            TotalAmount = st.TotalAmount, 
            TransactionDate = st.TransactionDate,
            ServiceOrderId = st.ServiceOrderId, 
            SupplierId = st.SupplierId,
            ProcessedById = st.ProcessedById,
            ReferenceNumber = st.ReferenceNumber, 
            Notes = st.Notes,
            Part = st.Part != null ? new PartDto { Id = st.Part.Id, PartName = st.Part.PartName, PartNumber = st.Part.PartNumber } : null,
            CreatedAt = st.CreatedAt, 
            CreatedBy = st.CreatedBy
        };

    /// <summary>
    /// Import dữ liệu tồn kho từ Excel
    /// </summary>
    [HttpPost("import-excel")]
    public async Task<ActionResult<ApiResponse<ExcelImportResult>>> ImportExcel(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<ExcelImportResult>.ErrorResult("Vui lòng chọn file Excel để import"));
            }

            if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
            {
                return BadRequest(ApiResponse<ExcelImportResult>.ErrorResult("Chỉ hỗ trợ file Excel (.xlsx, .xls)"));
            }

            using var stream = file.OpenReadStream();
            var excelData = await _excelImportService.ReadExcelDataAsync(stream);
            
            if (excelData.Count == 0)
            {
                return BadRequest(ApiResponse<ExcelImportResult>.ErrorResult("Không tìm thấy dữ liệu trong file Excel"));
            }

            // Validate dữ liệu
            var validationResult = await _excelImportService.ValidateExcelDataAsync(excelData);
            
            if (!validationResult.Success)
            {
                return Ok(ApiResponse<ExcelImportResult>.SuccessResult(validationResult, "Dữ liệu có lỗi, vui lòng kiểm tra"));
            }

            // Import vào database
            var importResult = await _excelImportService.ImportToDatabaseAsync(excelData);
            
            if (importResult.Success)
            {
                return Ok(ApiResponse<ExcelImportResult>.SuccessResult(importResult, $"Import thành công {importResult.SuccessCount} bản ghi"));
            }
            else
            {
                return Ok(ApiResponse<ExcelImportResult>.SuccessResult(importResult, "Import hoàn thành với một số lỗi"));
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ExcelImportResult>.ErrorResult($"Lỗi import Excel: {ex.Message}"));
        }
    }

    /// <summary>
    /// Tải template Excel mẫu
    /// </summary>
    [HttpGet("download-template")]
    public ActionResult DownloadExcelTemplate()
    {
        try
        {
            var templateBytes = _excelImportService.GenerateExcelTemplate();
            var fileName = $"Stock_Import_Template_{DateTime.Now:yyyyMMdd}.xlsx";
            
            return File(templateBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult($"Lỗi tạo template: {ex.Message}"));
        }
    }

    /// <summary>
    /// Validate file Excel trước khi import
    /// </summary>
    [HttpPost("validate-excel")]
    public async Task<ActionResult<ApiResponse<ExcelImportResult>>> ValidateExcel(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<ExcelImportResult>.ErrorResult("Vui lòng chọn file Excel để validate"));
            }

            if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
            {
                return BadRequest(ApiResponse<ExcelImportResult>.ErrorResult("Chỉ hỗ trợ file Excel (.xlsx, .xls)"));
            }

            using var stream = file.OpenReadStream();
            var excelData = await _excelImportService.ReadExcelDataAsync(stream);
            
            if (excelData.Count == 0)
            {
                return BadRequest(ApiResponse<ExcelImportResult>.ErrorResult("Không tìm thấy dữ liệu trong file Excel"));
            }

            var validationResult = await _excelImportService.ValidateExcelDataAsync(excelData);
            
            return Ok(ApiResponse<ExcelImportResult>.SuccessResult(validationResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ExcelImportResult>.ErrorResult($"Lỗi validate Excel: {ex.Message}"));
        }
    }
}
}
