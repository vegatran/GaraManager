using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace GarageManagementSystem.Core.Enums
{
    /// <summary>
    /// Loại giao dịch kho
    /// </summary>
    public enum StockTransactionType
    {
        /// <summary>
        /// Nhập kho
        /// </summary>
        [Display(Name = "Nhập kho")]
        NhapKho = 1,

        /// <summary>
        /// Xuất kho
        /// </summary>
        [Display(Name = "Xuất kho")]
        XuatKho = 2,

        /// <summary>
        /// Điều chỉnh tồn kho
        /// </summary>
        [Display(Name = "Điều chỉnh")]
        DieuChinh = 3,

        /// <summary>
        /// Tồn đầu kỳ (khi chuyển từ Excel sang hệ thống)
        /// </summary>
        [Display(Name = "Tồn đầu kỳ")]
        TonDauKy = 4
    }

    /// <summary>
    /// Extension methods cho StockTransactionType
    /// </summary>
    public static class StockTransactionTypeExtensions
    {
        /// <summary>
        /// Lấy tên hiển thị của loại giao dịch
        /// </summary>
        public static string GetDisplayName(this StockTransactionType type)
        {
            var field = type.GetType().GetField(type.ToString());
            var attribute = field?.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
            return attribute?.Name ?? type.ToString();
        }

        /// <summary>
        /// Lấy mã ngắn của loại giao dịch
        /// </summary>
        public static string GetShortCode(this StockTransactionType type)
        {
            return type switch
            {
                StockTransactionType.NhapKho => "NK", // Nhập kho
                StockTransactionType.XuatKho => "XK", // Xuất kho
                StockTransactionType.DieuChinh => "DC", // Điều chỉnh
                StockTransactionType.TonDauKy => "TDK", // Tồn đầu kỳ
                _ => type.ToString()
            };
        }

        /// <summary>
        /// Lấy prefix cho TransactionNumber
        /// </summary>
        public static string GetTransactionPrefix(this StockTransactionType type)
        {
            return type switch
            {
                StockTransactionType.NhapKho => "NK",
                StockTransactionType.XuatKho => "XK", 
                StockTransactionType.DieuChinh => "DC",
                StockTransactionType.TonDauKy => "TDK",
                _ => type.ToString()
            };
        }

        /// <summary>
        /// Kiểm tra xem có phải giao dịch làm tăng tồn kho không
        /// </summary>
        public static bool IsIncrease(this StockTransactionType type)
        {
            return type == StockTransactionType.NhapKho || type == StockTransactionType.TonDauKy;
        }

        /// <summary>
        /// Kiểm tra xem có phải giao dịch làm giảm tồn kho không
        /// </summary>
        public static bool IsDecrease(this StockTransactionType type)
        {
            return type == StockTransactionType.XuatKho;
        }

        /// <summary>
        /// Kiểm tra xem có phải giao dịch điều chỉnh không
        /// </summary>
        public static bool IsAdjustment(this StockTransactionType type)
        {
            return type == StockTransactionType.DieuChinh;
        }
    }
}
