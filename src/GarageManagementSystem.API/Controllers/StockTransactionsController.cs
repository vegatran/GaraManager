using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Core.Enums;
using GarageManagementSystem.Core.Extensions;
using GarageManagementSystem.Core.Services;
using GarageManagementSystem.Core.DTOs;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using GarageManagementSystem.API.Services;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiScope")]
    public class StockTransactionsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExcelImportService _excelImportService;
        private readonly GarageManagementSystem.API.Services.ICacheService _cacheService;
        private readonly GarageDbContext _context;

        public StockTransactionsController(
            IUnitOfWork unitOfWork, 
            IExcelImportService excelImportService, 
            GarageManagementSystem.API.Services.ICacheService cacheService,
            GarageDbContext context)
        {
            _unitOfWork = unitOfWork;
            _excelImportService = excelImportService;
            _cacheService = cacheService;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<StockTransactionDto>>> GetStockTransactions(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? transactionType = null,
            [FromQuery] int? partId = null)
        {
            try
            {
                // ✅ FIX: Query trực tiếp từ DbContext thay vì load tất cả vào memory
                // GetByDateRangeAsync() trả về IEnumerable, .AsQueryable() chỉ tạo in-memory queryable
                // Không có IAsyncQueryProvider → không thể dùng async operations
                var query = _context.StockTransactions
                    .Include(t => t.Part)
                    .Include(t => t.Supplier)
                    .Include(t => t.ProcessedBy)
                    .Where(t => !t.IsDeleted)
                    .AsQueryable();
                
                // Apply search filter if provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(t => 
                        (t.ReferenceNumber != null && t.ReferenceNumber.Contains(searchTerm)) || 
                        (t.Notes != null && t.Notes.Contains(searchTerm)));
                }
                
                // Apply transaction type filter if provided
                if (!string.IsNullOrEmpty(transactionType))
                {
                    query = query.Where(t => t.TransactionType.ToString() == transactionType);
                }
                
                // Apply part filter if provided
                if (partId.HasValue)
                {
                    query = query.Where(t => t.PartId == partId.Value);
                }

                query = query.OrderByDescending(t => t.TransactionDate);

                // ✅ OPTIMIZED: Get paged results with total count - automatically chooses best method
                var (transactions, totalCount) = await query.ToPagedListWithCountAsync(pageNumber, pageSize, _context);
                var transactionDtos = transactions.Select(MapToDto).ToList();
                
                return Ok(PagedResponse<StockTransactionDto>.CreateSuccessResult(
                    transactionDtos, pageNumber, pageSize, totalCount, "Stock transactions retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, PagedResponse<StockTransactionDto>.CreateErrorResult("Lỗi khi lấy danh sách giao dịch kho"));
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
                                DefaultUnit = item.Unit,
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

                            if (!string.IsNullOrWhiteSpace(item.Unit))
                            {
                                part.PartUnits = new List<PartUnit>
                                {
                                    new PartUnit
                                    {
                                        UnitName = item.Unit!,
                                        ConversionRate = 1,
                                        IsDefault = true,
                                        Part = part
                                    }
                                };
                            }

                            await _unitOfWork.Parts.AddAsync(part);
                        }
                        else
                        {
                            // Cập nhật Part hiện tại
                            part.PartName = item.PartName;
                            part.Category = item.Category;
                            part.Brand = item.Brand;
                            part.DefaultUnit = item.Unit;
                            part.Location = item.Location;
                            part.CompatibleVehicles = item.CompatibleVehicles;
                            part.AverageCostPrice = item.UnitPrice;
                            part.QuantityInStock = item.Quantity;
                            part.UpdatedAt = DateTime.Now;

                            await _unitOfWork.Parts.UpdateAsync(part);
                        }

                        if (!string.IsNullOrWhiteSpace(item.Unit))
                        {
                            await _unitOfWork.Parts.UpdateAsync(part);
                            await _unitOfWork.SaveChangesAsync();

                            var detailedPart = await _unitOfWork.Parts.GetWithDetailsAsync(part.Id);
                            if (detailedPart != null)
                            {
                                part = detailedPart;
                                EnsurePartUnit(part, item.Unit!);
                                await _unitOfWork.Parts.UpdateAsync(part);
                            }
                        }
                        else
                        {
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

        private static void EnsurePartUnit(Part part, string unitName)
        {
            if (part.PartUnits == null)
            {
                part.PartUnits = new List<PartUnit>();
            }

            var existing = part.PartUnits.FirstOrDefault(u => u.UnitName.Equals(unitName, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                existing = new PartUnit
                {
                    PartId = part.Id,
                    UnitName = unitName,
                    ConversionRate = 1,
                    IsDefault = true
                };
                part.PartUnits.Add(existing);
            }

            existing.ConversionRate = 1;
            existing.IsDefault = true;
            part.DefaultUnit = unitName;

            foreach (var other in part.PartUnits.Where(u => !u.UnitName.Equals(unitName, StringComparison.OrdinalIgnoreCase)))
            {
                other.IsDefault = false;
            }
        }

        /// <summary>
        /// ✅ THÊM: Tạo đơn nhập hàng với nhiều phụ tùng
        /// </summary>
        [HttpPost("purchase-order")]
        public async Task<ActionResult<ApiResponse<List<StockTransactionDto>>>> CreatePurchaseOrder(CreatePurchaseOrderDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(ApiResponse<List<StockTransactionDto>>.ErrorResult("Dữ liệu không hợp lệ", string.Join(", ", errors)));
                }

                if (dto.Items == null || dto.Items.Count == 0)
                {
                    return BadRequest(ApiResponse<List<StockTransactionDto>>.ErrorResult("Vui lòng thêm ít nhất một phụ tùng vào đơn hàng"));
                }

                var createdTransactions = new List<StockTransactionDto>();
                var stockTransactions = new List<Core.Entities.StockTransaction>();
                var partsToUpdate = new Dictionary<int, Core.Entities.Part>();

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // ✅ TỐI ƯU: Pre-load tất cả Parts để tránh N+1 query
                    var partIds = dto.Items.Select(x => x.PartId).ToList();
                    var parts = await _unitOfWork.Parts.GetByIdsAsync(partIds);
                    var partsDict = parts.ToDictionary(p => p.Id);

                    foreach (var item in dto.Items)
                    {
                        // ✅ TỐI ƯU: Sử dụng pre-loaded parts
                        if (!partsDict.TryGetValue(item.PartId, out var part))
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return BadRequest(ApiResponse<List<StockTransactionDto>>.ErrorResult($"Không tìm thấy phụ tùng với ID: {item.PartId}"));
                        }

                        var qtyBefore = part.QuantityInStock;
                        var qtyChange = TransactionTypeHelper.IsIncrease("NhapKho") ? item.QuantityOrdered : -item.QuantityOrdered;
                        var qtyAfter = qtyBefore + qtyChange;

                        if (qtyAfter < 0)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return BadRequest(ApiResponse<List<StockTransactionDto>>.ErrorResult($"Không đủ tồn kho cho phụ tùng: {part.PartName}"));
                        }

                        // Convert string to enum - Default to NhapKho for purchase orders
                        var transactionType = StockTransactionType.NhapKho;

                        var transaction = new Core.Entities.StockTransaction
                        {
                            TransactionNumber = await _unitOfWork.StockTransactions.GenerateTransactionNumberAsync(),
                            PartId = item.PartId,
                            TransactionType = transactionType,
                            Quantity = item.QuantityOrdered,
                            QuantityBefore = qtyBefore,
                            QuantityAfter = qtyAfter,
                            StockAfter = qtyAfter,
                            UnitPrice = item.UnitPrice,
                            TotalAmount = item.QuantityOrdered * item.UnitPrice,
                            TransactionDate = dto.OrderDate,
                            ServiceOrderId = null,
                            SupplierId = dto.SupplierId,
                            ReferenceNumber = $"PO-{DateTime.Now:yyyyMMdd}-{DateTime.Now.Ticks}",
                            Notes = $"{dto.Notes} - {part.PartName}",
                            ProcessedById = null,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            IsDeleted = false,
                            HasInvoice = true
                        };

                        // ✅ TỐI ƯU: Collect transactions và parts để bulk update
                        stockTransactions.Add(transaction);
                        
                        // Update part quantity
                        part.QuantityInStock = qtyAfter;
                        part.UpdatedAt = DateTime.Now;
                        partsToUpdate[part.Id] = part;

                        createdTransactions.Add(MapToDto(transaction));
                    }

                    // ✅ SỬA: Chỉ SaveChangesAsync một lần sau khi xử lý tất cả items
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    return Ok(ApiResponse<List<StockTransactionDto>>.SuccessResult(createdTransactions, 
                        $"Đã tạo đơn nhập hàng thành công với {createdTransactions.Count} phụ tùng"));
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<StockTransactionDto>>.ErrorResult("Lỗi khi tạo đơn nhập hàng", ex.Message));
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
