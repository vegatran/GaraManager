using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    public class StockTransactionDto : BaseDto
    {
        public string TransactionNumber { get; set; } = string.Empty;
        public int PartId { get; set; }
        public string TransactionType { get; set; } = "NhapKho";
        
        /// <summary>
        /// Tên hiển thị của loại giao dịch (cho UI)
        /// </summary>
        public string TransactionTypeDisplay => TransactionTypeHelper.GetTransactionTypeDisplayName(TransactionType);
        public int Quantity { get; set; }
        public int QuantityBefore { get; set; }
        public int QuantityAfter { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        public int? ServiceOrderId { get; set; }
        public int? SupplierId { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public int? ProcessedById { get; set; }
        
        public PartDto? Part { get; set; }
        public SupplierDto? Supplier { get; set; }
        public EmployeeDto? ProcessedBy { get; set; }
    }

    public class CreateStockTransactionDto
    {
        [Required] public int PartId { get; set; }
        [Required] public string TransactionType { get; set; } = "NhapKho";
        [Required] [Range(1, int.MaxValue)] public int Quantity { get; set; }
        [Required] [Range(0, double.MaxValue)] public decimal UnitPrice { get; set; }
        public int? ServiceOrderId { get; set; }
        public int? SupplierId { get; set; }
        [StringLength(100)] public string? ReferenceNumber { get; set; }
        [StringLength(1000)] public string? Notes { get; set; }
        public int? ProcessedById { get; set; }
    }

    /// <summary>
    /// Helper methods cho TransactionType
    /// </summary>
    public static class TransactionTypeHelper
    {
        public static string GetTransactionTypeDisplayName(string transactionType)
        {
            return transactionType switch
            {
                "NhapKho" => "Nhập kho",
                "XuatKho" => "Xuất kho",
                "DieuChinh" => "Điều chỉnh",
                "TonDauKy" => "Tồn đầu kỳ",
                _ => transactionType
            };
        }

        public static string GetTransactionPrefix(string transactionType)
        {
            return transactionType switch
            {
                "NhapKho" => "NK",
                "XuatKho" => "XK",
                "DieuChinh" => "DC", 
                "TonDauKy" => "TDK",
                _ => transactionType
            };
        }

        public static bool IsIncrease(string transactionType)
        {
            return transactionType == "NhapKho" || transactionType == "TonDauKy";
        }

        public static bool IsDecrease(string transactionType)
        {
            return transactionType == "XuatKho";
        }
    }
}

