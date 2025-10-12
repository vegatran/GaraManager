using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    public class Service : BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Duration { get; set; } // Duration in minutes

        [StringLength(50)]
        public string? Category { get; set; }

        // Thông tin dịch vụ mở rộng
        public int? ServiceTypeId { get; set; } // Loại dịch vụ
        
        [StringLength(50)]
        public string? LaborType { get; set; } // "Tháo", "Lắp", "Kiểm tra", "Điều chỉnh", "Sơn"
        
        [StringLength(100)]
        public string? SkillLevel { get; set; } // "Cơ bản", "Trung bình", "Cao cấp", "Chuyên gia"
        
        public int LaborHours { get; set; } = 1; // Số giờ công (thay vì minutes)
        
        public decimal LaborRate { get; set; } = 0; // Đơn giá công/giờ
        
        public decimal TotalLaborCost { get; set; } = 0; // Tổng tiền công = LaborHours × LaborRate
        
        [StringLength(100)]
        public string? RequiredTools { get; set; } // "Cờ lê 12", "Máy nén khí", "Súng sơn"
        
        [StringLength(100)]
        public string? RequiredSkills { get; set; } // "Thợ điện", "Thợ sơn", "Thợ máy"
        
        [StringLength(500)]
        public string? WorkInstructions { get; set; } // Hướng dẫn thực hiện

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ServiceType? ServiceType { get; set; }
        public virtual ICollection<ServiceOrderItem> ServiceOrderItems { get; set; } = new List<ServiceOrderItem>();
        public virtual ICollection<InspectionIssue> RelatedInspectionIssues { get; set; } = new List<InspectionIssue>();
        public virtual ICollection<QuotationItem> QuotationItems { get; set; } = new List<QuotationItem>();
    }
}
