using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// ServiceOrderPart - Phụ tùng đã sử dụng trong phiếu sửa chữa
    /// </summary>
    public class ServiceOrderPart : BaseEntity
    {
        public int ServiceOrderId { get; set; }
        public int PartId { get; set; }
        public int? ServiceOrderItemId { get; set; } // Liên kết với dịch vụ nào (optional)
        
        [StringLength(200)]
        public string PartName { get; set; } = string.Empty; // Cached part name

        [Required]
        public int Quantity { get; set; } = 1;

        [Required]
        public decimal UnitCost { get; set; } // Giá nhập (for profit calculation)

        [Required]
        public decimal UnitPrice { get; set; } // Giá bán cho khách

        [Required]
        public decimal TotalPrice { get; set; } // Quantity × UnitPrice

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(20)]
        public string? Status { get; set; } = "Pending"; // "Pending", "Used", "Returned", "Warranty"

        public bool IsWarranty { get; set; } = false; // Phụ tùng bảo hành

        public DateTime? WarrantyUntil { get; set; }
        
        public DateTime? ReturnDate { get; set; } // Ngày trả lại (nếu bảo hành)

        // Navigation properties
        public virtual ServiceOrder ServiceOrder { get; set; } = null!;
        public virtual Part Part { get; set; } = null!;
        public virtual ServiceOrderItem? ServiceOrderItem { get; set; }
    }
}

