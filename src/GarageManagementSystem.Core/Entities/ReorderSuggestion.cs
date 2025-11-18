using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// ReorderSuggestion - Đề xuất đặt hàng lại
    /// Phase 4.2.1: Phân tích Nhu cầu
    /// </summary>
    public class ReorderSuggestion : BaseEntity
    {
        public int PartId { get; set; }
        
        [Required]
        public int SuggestedQuantity { get; set; }
        
        [Required]
        public decimal EstimatedCost { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Priority { get; set; } = "Medium"; // "High", "Medium", "Low"
        
        [Required]
        [StringLength(50)]
        public string Source { get; set; } = string.Empty; // "InventoryAlert", "ServiceOrder", "Manual"
        
        public int? SourceEntityId { get; set; } // InventoryAlertId hoặc ServiceOrderId
        
        [Required]
        public DateTime SuggestedDate { get; set; } = DateTime.Now;
        
        public DateTime? RequiredByDate { get; set; } // Ngày cần nhận hàng
        
        public bool IsProcessed { get; set; } = false; // Đã tạo PO chưa
        
        public int? PurchaseOrderId { get; set; } // Link đến PO nếu đã tạo
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        // Navigation properties
        public virtual Part Part { get; set; } = null!;
        public virtual PurchaseOrder? PurchaseOrder { get; set; }
    }
}

