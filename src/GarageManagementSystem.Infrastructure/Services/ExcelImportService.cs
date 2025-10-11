using GarageManagementSystem.Core.Services;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Enums;
using GarageManagementSystem.Core.DTOs;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Infrastructure.Services
{
    /// <summary>
    /// Service xử lý import Excel
    /// </summary>
    public class ExcelImportService : IExcelImportService
    {
        private readonly GarageDbContext _context;
        private readonly ILogger<ExcelImportService> _logger;

        public ExcelImportService(GarageDbContext context, ILogger<ExcelImportService> logger)
        {
            _context = context;
            _logger = logger;
            
            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Đọc và parse dữ liệu từ Excel file
        /// </summary>
        public async Task<List<ExcelStockImportDto>> ReadExcelDataAsync(Stream fileStream)
        {
            var result = new List<ExcelStockImportDto>();

            try
            {
                using var package = new ExcelPackage(fileStream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                {
                    throw new InvalidOperationException("Không tìm thấy worksheet trong file Excel");
                }

                // Đọc từ dòng 2 (bỏ qua header)
                var rowCount = worksheet.Dimension?.Rows ?? 0;
                
                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var partCode = worksheet.Cells[row, 1]?.Text?.Trim();
                        var partName = worksheet.Cells[row, 2]?.Text?.Trim();
                        var unit = worksheet.Cells[row, 3]?.Text?.Trim();

                        // Skip empty rows
                        if (string.IsNullOrEmpty(partCode) && string.IsNullOrEmpty(partName))
                            continue;

                        var item = new ExcelStockImportDto
                        {
                            ExcelRowNumber = row,
                            PartCode = partCode ?? string.Empty,
                            PartName = partName ?? string.Empty,
                            Unit = unit ?? string.Empty,
                            OpeningQuantity = GetDecimalValue(worksheet.Cells[row, 4]),
                            OpeningValue = GetDecimalValue(worksheet.Cells[row, 5]),
                            InQuantity = GetDecimalValue(worksheet.Cells[row, 6]),
                            InValue = GetDecimalValue(worksheet.Cells[row, 7]),
                            OutQuantity = GetDecimalValue(worksheet.Cells[row, 8]),
                            OutValue = GetDecimalValue(worksheet.Cells[row, 9]),
                            UnitPrice = GetDecimalValue(worksheet.Cells[row, 12]) // Cột L - Đơn giá
                        };

                        result.Add(item);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Lỗi đọc dòng {Row} từ Excel", row);
                        // Continue with next row
                    }
                }

                _logger.LogInformation("Đã đọc {Count} dòng dữ liệu từ Excel", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi đọc file Excel");
                throw new InvalidOperationException("Lỗi đọc file Excel: " + ex.Message);
            }
        }

        /// <summary>
        /// Validate dữ liệu Excel
        /// </summary>
        public async Task<ExcelImportResult> ValidateExcelDataAsync(List<ExcelStockImportDto> data)
        {
            var result = new ExcelImportResult
            {
                TotalRows = data.Count,
                Success = true,
                Message = "Validation thành công"
            };

            var errors = new List<ExcelImportError>();

            for (int i = 0; i < data.Count; i++)
            {
                var item = data[i];
                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(item);

                // Validate using Data Annotations
                if (!Validator.TryValidateObject(item, validationContext, validationResults, true))
                {
                    foreach (var validationResult in validationResults)
                    {
                        errors.Add(new ExcelImportError
                        {
                            RowNumber = item.ExcelRowNumber,
                            ColumnName = string.Join(", ", validationResult.MemberNames),
                            ErrorMessage = validationResult.ErrorMessage ?? "Lỗi validation",
                            PartCode = item.PartCode,
                            PartName = item.PartName
                        });
                    }
                }

                // Kiểm tra trùng lặp PartCode trong file
                var duplicateCount = data.Count(x => x.PartCode == item.PartCode && !string.IsNullOrEmpty(item.PartCode));
                if (duplicateCount > 1)
                {
                    errors.Add(new ExcelImportError
                    {
                        RowNumber = item.ExcelRowNumber,
                        ColumnName = "PartCode",
                        ErrorMessage = $"Mã hiệu '{item.PartCode}' bị trùng lặp trong file",
                        PartCode = item.PartCode,
                        PartName = item.PartName
                    });
                }
            }

            result.Errors = errors;
            result.ErrorCount = errors.Count;
            result.SuccessCount = result.TotalRows - result.ErrorCount;
            result.Success = result.ErrorCount == 0;

            if (result.ErrorCount > 0)
            {
                result.Message = $"Có {result.ErrorCount} lỗi trong {result.TotalRows} dòng dữ liệu";
            }

            return result;
        }

        /// <summary>
        /// Import dữ liệu vào database
        /// </summary>
        public async Task<ExcelImportResult> ImportToDatabaseAsync(List<ExcelStockImportDto> data)
        {
            var result = new ExcelImportResult
            {
                TotalRows = data.Count,
                Success = true,
                Message = "Import thành công"
            };

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var successCount = 0;
                    var errorCount = 0;
                    var errors = new List<ExcelImportError>();

                    foreach (var item in data)
                    {
                        try
                        {
                            // Kiểm tra Part đã tồn tại chưa
                            var existingPart = await _context.Parts
                                .FirstOrDefaultAsync(p => p.PartNumber == item.PartCode);

                            Part part;
                            
                            if (existingPart != null)
                            {
                                // Cập nhật Part hiện có
                                part = existingPart;
                                part.PartName = item.PartName;
                                part.Unit = item.Unit;
                                part.AverageCostPrice = item.CalculatedUnitPrice;
                                part.UpdatedAt = DateTime.UtcNow;
                                
                                _context.Parts.Update(part);
                            }
                            else
                            {
                                // Tạo Part mới
                                part = new Part
                                {
                                    PartNumber = item.PartCode,
                                    PartName = item.PartName,
                                    Unit = item.Unit,
                                    AverageCostPrice = item.CalculatedUnitPrice,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                
                                _context.Parts.Add(part);
                                await _context.SaveChangesAsync(); // Save để lấy ID
                            }

                            // Tạo Stock Transaction cho Opening Balance
                            if (item.OpeningQuantity > 0)
                            {
                                var openingTransaction = new StockTransaction
                                {
                                    TransactionNumber = GenerateTransactionNumber(StockTransactionType.TonDauKy),
                                    PartId = part.Id,
                                    TransactionType = StockTransactionType.TonDauKy,
                                    Quantity = (int)item.OpeningQuantity,
                                    UnitPrice = item.CalculatedUnitPrice,
                                    TotalAmount = item.OpeningValue,
                                    Notes = $"Nhập tồn đầu kỳ từ Excel - Dòng {item.ExcelRowNumber}",
                                    CreatedAt = DateTime.UtcNow
                                };

                                _context.StockTransactions.Add(openingTransaction);
                            }

                            // Tạo Stock Transaction cho Nhập trong kỳ
                            if (item.InQuantity > 0)
                            {
                                var inTransaction = new StockTransaction
                                {
                                    TransactionNumber = GenerateTransactionNumber(StockTransactionType.NhapKho),
                                    PartId = part.Id,
                                    TransactionType = StockTransactionType.NhapKho,
                                    Quantity = (int)item.InQuantity,
                                    UnitPrice = item.CalculatedUnitPrice,
                                    TotalAmount = item.InValue,
                                    Notes = $"Nhập trong kỳ từ Excel - Dòng {item.ExcelRowNumber}",
                                    CreatedAt = DateTime.UtcNow
                                };

                                _context.StockTransactions.Add(inTransaction);
                            }

                            // Tạo Stock Transaction cho Xuất trong kỳ
                            if (item.OutQuantity > 0)
                            {
                                var outTransaction = new StockTransaction
                                {
                                    TransactionNumber = GenerateTransactionNumber(StockTransactionType.XuatKho),
                                    PartId = part.Id,
                                    TransactionType = StockTransactionType.XuatKho,
                                    Quantity = (int)item.OutQuantity,
                                    UnitPrice = item.CalculatedUnitPrice,
                                    TotalAmount = item.OutValue,
                                    Notes = $"Xuất trong kỳ từ Excel - Dòng {item.ExcelRowNumber}",
                                    CreatedAt = DateTime.UtcNow
                                };

                                _context.StockTransactions.Add(outTransaction);
                            }

                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Lỗi import dòng {Row}: {PartCode}", item.ExcelRowNumber, item.PartCode);
                            
                            errors.Add(new ExcelImportError
                            {
                                RowNumber = item.ExcelRowNumber,
                                ColumnName = "Database",
                                ErrorMessage = ex.Message,
                                PartCode = item.PartCode,
                                PartName = item.PartName
                            });
                            
                            errorCount++;
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    result.SuccessCount = successCount;
                    result.ErrorCount = errorCount;
                    result.Errors = errors;
                    result.Success = errorCount == 0;

                    if (errorCount > 0)
                    {
                        result.Message = $"Import hoàn thành: {successCount} thành công, {errorCount} lỗi";
                    }

                    _logger.LogInformation("Import Excel hoàn thành: {Success}/{Total}", successCount, data.Count);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi import Excel vào database");
                result.Success = false;
                result.Message = "Lỗi import: " + ex.Message;
                result.ErrorCount = data.Count;
                result.SuccessCount = 0;
            }

            return result;
        }

        /// <summary>
        /// Tạo Excel template
        /// </summary>
        public byte[] GenerateExcelTemplate()
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Tồn Kho");

            // Headers
            worksheet.Cells[1, 1].Value = "Mã Hiệu";
            worksheet.Cells[1, 2].Value = "Tên VTTH";
            worksheet.Cells[1, 3].Value = "Đơn vị";
            worksheet.Cells[1, 4].Value = "Tồn đầu kỳ - S.Lượng";
            worksheet.Cells[1, 5].Value = "Tồn đầu kỳ - G.Trị";
            worksheet.Cells[1, 6].Value = "Nhập trong kỳ - S.Lượng";
            worksheet.Cells[1, 7].Value = "Nhập trong kỳ - G.Trị";
            worksheet.Cells[1, 8].Value = "Xuất trong kỳ - S.Lượng";
            worksheet.Cells[1, 9].Value = "Xuất trong kỳ - G.Trị";
            worksheet.Cells[1, 10].Value = "Tồn cuối kỳ - S.Lượng";
            worksheet.Cells[1, 11].Value = "Tồn cuối kỳ - G.Trị";
            worksheet.Cells[1, 12].Value = "Đơn giá";

            // Sample data
            worksheet.Cells[2, 1].Value = "ACQUY_70";
            worksheet.Cells[2, 2].Value = "BÌNH ẮC QUY GS N570";
            worksheet.Cells[2, 3].Value = "CÁI";
            worksheet.Cells[2, 4].Value = 10;
            worksheet.Cells[2, 5].Value = 15000000;
            worksheet.Cells[2, 6].Value = 5;
            worksheet.Cells[2, 7].Value = 7500000;
            worksheet.Cells[2, 8].Value = 3;
            worksheet.Cells[2, 9].Value = 4500000;
            worksheet.Cells[2, 10].Value = 12;
            worksheet.Cells[2, 11].Value = 18000000;
            worksheet.Cells[2, 12].Value = 1500000;

            // Format headers
            using (var range = worksheet.Cells[1, 1, 1, 12])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            }

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            return package.GetAsByteArray();
        }

        /// <summary>
        /// Lấy giá trị decimal từ Excel cell
        /// </summary>
        private decimal GetDecimalValue(ExcelRange cell)
        {
            if (cell?.Value == null) return 0;
            
            if (decimal.TryParse(cell.Text, out decimal result))
                return result;
                
            return 0;
        }

        /// <summary>
        /// Tạo số giao dịch
        /// </summary>
        private string GenerateTransactionNumber(StockTransactionType transactionType)
        {
            var prefix = transactionType.GetTransactionPrefix();
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            return $"{prefix}{timestamp}";
        }
    }
}
