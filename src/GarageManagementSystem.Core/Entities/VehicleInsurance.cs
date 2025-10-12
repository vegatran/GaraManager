using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// VehicleInsurance - Quản lý bảo hiểm xe chi tiết
    /// </summary>
    public class VehicleInsurance : BaseEntity
    {
        public int VehicleId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string PolicyNumber { get; set; } = string.Empty; // Số hợp đồng bảo hiểm
        
        [Required]
        [StringLength(100)]
        public string InsuranceCompany { get; set; } = string.Empty; // Công ty bảo hiểm
        
        [StringLength(50)]
        public string CoverageType { get; set; } = string.Empty; // Loại bảo hiểm: Full, Third Party, Comprehensive, Commercial
        
        [Required]
        public DateTime StartDate { get; set; } // Ngày bắt đầu bảo hiểm
        
        [Required]
        public DateTime EndDate { get; set; } // Ngày kết thúc bảo hiểm
        
        [Required]
        public decimal PremiumAmount { get; set; } // Phí bảo hiểm
        
        [StringLength(3)]
        public string Currency { get; set; } = "VND"; // Đơn vị tiền tệ
        
        [StringLength(20)]
        public string PaymentMethod { get; set; } = string.Empty; // Phương thức thanh toán: Cash, Bank Transfer, Installment
        
        [StringLength(100)]
        public string? AgentName { get; set; } // Tên đại lý bảo hiểm
        
        [StringLength(20)]
        public string? AgentPhone { get; set; } // SĐT đại lý
        
        [StringLength(100)]
        public string? AgentEmail { get; set; } // Email đại lý
        
        [StringLength(500)]
        public string? CoverageDetails { get; set; } // Chi tiết bảo hiểm
        
        [StringLength(100)]
        public string? DeductibleAmount { get; set; } // Số tiền khấu trừ
        
        [StringLength(500)]
        public string? Exclusions { get; set; } // Các trường hợp loại trừ
        
        [StringLength(100)]
        public string? EmergencyContact { get; set; } // Liên hệ khẩn cấp
        
        [StringLength(20)]
        public string? EmergencyPhone { get; set; } // SĐT liên hệ khẩn cấp
        
        [StringLength(500)]
        public string? Notes { get; set; } // Ghi chú
        
        public bool IsActive { get; set; } = true; // Bảo hiểm còn hiệu lực
        
        public bool IsRenewed { get; set; } = false; // Đã gia hạn
        
        public DateTime? RenewalDate { get; set; } // Ngày gia hạn
        
        public int? PreviousPolicyId { get; set; } // ID bảo hiểm trước đó (để theo dõi lịch sử)
        
        // Navigation properties
        public virtual Vehicle Vehicle { get; set; } = null!;
        public virtual VehicleInsurance? PreviousPolicy { get; set; }
        public virtual ICollection<VehicleInsurance> Renewals { get; set; } = new List<VehicleInsurance>();
        public virtual ICollection<InsuranceClaim> Claims { get; set; } = new List<InsuranceClaim>();
    }
}
