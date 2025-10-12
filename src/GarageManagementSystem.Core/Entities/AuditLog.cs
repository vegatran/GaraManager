using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Audit Log - Nhật ký audit
    /// </summary>
    public class AuditLog : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string EntityName { get; set; } = string.Empty; // Customer, Vehicle, ServiceOrder, etc.

        public int? EntityId { get; set; } // ID của entity

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty; // Create, Update, Delete, View, Login, etc.

        [StringLength(100)]
        public string? UserId { get; set; } // User thực hiện action

        [StringLength(100)]
        public string? UserName { get; set; } // Tên user

        public DateTime Timestamp { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        public string? Details { get; set; } // JSON hoặc text mô tả chi tiết

        [StringLength(50)]
        public string? Severity { get; set; } // Info, Warning, Error, Critical
    }
}

