using System.ComponentModel.DataAnnotations;
using GarageManagementSystem.Core.Enums;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// StockTransaction - Giao dịch nhập/xuất kho
    /// </summary>
    public class StockTransaction : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string TransactionNumber { get; set; } = string.Empty; // ST-20241006-0001

        public int PartId { get; set; }

        [Required]
        public StockTransactionType TransactionType { get; set; } = StockTransactionType.NhapKho;

        [Required]
        public int Quantity { get; set; }

        public int QuantityBefore { get; set; } // Tồn kho trước giao dịch
        public int QuantityAfter { get; set; }  // Tồn kho sau giao dịch

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        public int? ServiceOrderId { get; set; } // Nếu xuất kho cho order
        public int? SupplierId { get; set; }     // Nếu nhập kho từ supplier

        [StringLength(100)]
        public string? ReferenceNumber { get; set; } // Số hóa đơn nhập, phiếu xuất...

        [StringLength(1000)]
        public string? Notes { get; set; }

        public int? ProcessedById { get; set; } // Employee xử lý

        // Navigation properties
        public virtual Part Part { get; set; } = null!;
        public virtual ServiceOrder? ServiceOrder { get; set; }
        public virtual Supplier? Supplier { get; set; }
        public virtual Employee? ProcessedBy { get; set; }
    }
}

