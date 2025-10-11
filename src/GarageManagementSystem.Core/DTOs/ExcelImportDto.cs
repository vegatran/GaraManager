using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.DTOs
{
    /// <summary>
    /// DTO cho việc import dữ liệu tồn kho từ Excel
    /// </summary>
    public class ExcelStockImportDto
    {
        /// <summary>
        /// Mã hiệu/SKU của phụ tùng
        /// </summary>
        [Required(ErrorMessage = "Mã hiệu là bắt buộc")]
        public string PartCode { get; set; } = string.Empty;

        /// <summary>
        /// Tên vật tư thiết bị hàng hóa
        /// </summary>
        [Required(ErrorMessage = "Tên phụ tùng là bắt buộc")]
        public string PartName { get; set; } = string.Empty;

        /// <summary>
        /// Đơn vị tính (CÁI, Chiếc, Bộ, Kg, Chai, Thùng, Lít, Cuộn, Mét)
        /// </summary>
        [Required(ErrorMessage = "Đơn vị tính là bắt buộc")]
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// Số lượng tồn đầu kỳ
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Số lượng tồn đầu kỳ phải >= 0")]
        public decimal OpeningQuantity { get; set; } = 0;

        /// <summary>
        /// Giá trị tồn đầu kỳ
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị tồn đầu kỳ phải >= 0")]
        public decimal OpeningValue { get; set; } = 0;

        /// <summary>
        /// Số lượng nhập trong kỳ
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Số lượng nhập phải >= 0")]
        public decimal InQuantity { get; set; } = 0;

        /// <summary>
        /// Giá trị nhập trong kỳ
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị nhập phải >= 0")]
        public decimal InValue { get; set; } = 0;

        /// <summary>
        /// Số lượng xuất trong kỳ
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Số lượng xuất phải >= 0")]
        public decimal OutQuantity { get; set; } = 0;

        /// <summary>
        /// Giá trị xuất trong kỳ
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị xuất phải >= 0")]
        public decimal OutValue { get; set; } = 0;

        /// <summary>
        /// Đơn giá (VNĐ)
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá phải >= 0")]
        public decimal UnitPrice { get; set; } = 0;

        /// <summary>
        /// Số dòng trong Excel (để báo lỗi chính xác)
        /// </summary>
        public int ExcelRowNumber { get; set; }

        /// <summary>
        /// Tính toán đơn giá từ giá trị và số lượng tồn đầu kỳ
        /// </summary>
        public decimal CalculatedUnitPrice
        {
            get
            {
                if (OpeningQuantity > 0)
                    return OpeningValue / OpeningQuantity;
                if (UnitPrice > 0)
                    return UnitPrice;
                return 0;
            }
        }

        /// <summary>
        /// Tính số lượng tồn cuối kỳ
        /// </summary>
        public decimal EndingQuantity => OpeningQuantity + InQuantity - OutQuantity;

        /// <summary>
        /// Tính giá trị tồn cuối kỳ
        /// </summary>
        public decimal EndingValue => OpeningValue + InValue - OutValue;
    }

    /// <summary>
    /// Kết quả import Excel
    /// </summary>
    public class ExcelImportResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<ExcelImportError> Errors { get; set; } = new List<ExcelImportError>();
        public List<ExcelStockImportDto> ImportedData { get; set; } = new List<ExcelStockImportDto>();
    }

    /// <summary>
    /// Lỗi khi import Excel
    /// </summary>
    public class ExcelImportError
    {
        public int RowNumber { get; set; }
        public string ColumnName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string PartCode { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
    }
}
