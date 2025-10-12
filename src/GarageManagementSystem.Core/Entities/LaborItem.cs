using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// LaborItem - Chi tiết công lao động
    /// </summary>
    public class LaborItem : BaseEntity
    {
        public int LaborCategoryId { get; set; }
        public int? CategoryId { get; set; } // Alias for LaborCategoryId
        public int? PartGroupId { get; set; } // Liên kết với nhóm phụ tùng
        
        [StringLength(50)]
        public string LaborCode { get; set; } = string.Empty; // Mã công
        
        [Required]
        [StringLength(200)]
        public string ItemName { get; set; } = string.Empty; // "Công tháo đèn pha", "Công thay nhớt", "Công sơn cản"
        
        [StringLength(200)]
        public string LaborName { get; set; } = string.Empty; // Alias for ItemName
        
        [StringLength(100)]
        public string? PartName { get; set; } // "Đèn pha", "Nhớt động cơ", "Cản xe"
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public decimal StandardHours { get; set; } = 1; // Số giờ công chuẩn
        
        public decimal LaborRate { get; set; } = 0; // Đơn giá công/giờ
        
        public decimal TotalLaborCost { get; set; } = 0; // Tổng tiền công
        
        [StringLength(100)]
        public string? SkillLevel { get; set; } // "Cơ bản", "Trung bình", "Cao cấp"
        
        [StringLength(100)]
        public string? RequiredTools { get; set; } // Công cụ cần thiết
        
        [StringLength(500)]
        public string? WorkSteps { get; set; } // Các bước thực hiện
        
        [StringLength(100)]
        public string? Difficulty { get; set; } // "Dễ", "Trung bình", "Khó"
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual LaborCategory LaborCategory { get; set; } = null!;
        public virtual PartGroup? PartGroup { get; set; }
        public virtual ICollection<ServiceOrderLabor> ServiceOrderLabors { get; set; } = new List<ServiceOrderLabor>();
    }
}
