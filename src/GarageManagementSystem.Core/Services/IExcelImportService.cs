using GarageManagementSystem.Core.DTOs;

namespace GarageManagementSystem.Core.Services
{
    /// <summary>
    /// Interface cho service import Excel
    /// </summary>
    public interface IExcelImportService
    {
        /// <summary>
        /// Đọc và parse dữ liệu từ Excel file
        /// </summary>
        /// <param name="fileStream">Stream của file Excel</param>
        /// <returns>Danh sách dữ liệu đã parse</returns>
        Task<List<ExcelStockImportDto>> ReadExcelDataAsync(Stream fileStream);

        /// <summary>
        /// Validate dữ liệu Excel
        /// </summary>
        /// <param name="data">Dữ liệu cần validate</param>
        /// <returns>Kết quả validation</returns>
        Task<ExcelImportResult> ValidateExcelDataAsync(List<ExcelStockImportDto> data);

        /// <summary>
        /// Import dữ liệu vào database
        /// </summary>
        /// <param name="data">Dữ liệu đã validate</param>
        /// <returns>Kết quả import</returns>
        Task<ExcelImportResult> ImportToDatabaseAsync(List<ExcelStockImportDto> data);

        /// <summary>
        /// Tạo Excel template
        /// </summary>
        /// <returns>Byte array của Excel file</returns>
        byte[] GenerateExcelTemplate();
    }
}
