using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// ServiceType - Loại dịch vụ
    /// </summary>
    public class ServiceType : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string TypeName { get; set; } = string.Empty; // "Thay thế", "Sửa chữa", "Bảo dưỡng", "Sơn"
        
        [StringLength(20)]
        public string TypeCode { get; set; } = string.Empty; // "REPLACE", "REPAIR", "MAINTENANCE", "PAINT"
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(50)]
        public string? Category { get; set; } // "Mechanical", "Electrical", "Body", "Paint"
        
        public int EstimatedDuration { get; set; } = 60; // Thời gian ước lượng (phút)
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<Service> Services { get; set; } = new List<Service>();
    }
}
