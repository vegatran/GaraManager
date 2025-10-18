using System.ComponentModel.DataAnnotations;
using GarageManagementSystem.Core.Enums;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Customer Reception - Phiếu tiếp đón khách hàng
    /// Đây là bước đầu tiên trong quy trình xử lý xe tại gara
    /// </summary>
    public class CustomerReception : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string ReceptionNumber { get; set; } = string.Empty; // REC-20241006-0001

        public int CustomerId { get; set; }
        public int VehicleId { get; set; }

        [Required]
        public DateTime ReceptionDate { get; set; } = DateTime.Now;

        [StringLength(1000)]
        public string? CustomerRequest { get; set; } // Yêu cầu của khách hàng

        [StringLength(1000)]
        public string? CustomerComplaints { get; set; } // Khiếu nại của khách hàng

        [StringLength(1000)]
        public string? ReceptionNotes { get; set; } // Ghi chú tiếp đón

        // Kỹ thuật viên được phân công kiểm tra
        public int? AssignedTechnicianId { get; set; }

        // Trạng thái tiếp đón
        [Required]
        public ReceptionStatus Status { get; set; } = ReceptionStatus.Pending;

        // Thời gian xử lý
        public DateTime? AssignedDate { get; set; } // Thời gian phân công kỹ thuật
        public DateTime? InspectionStartDate { get; set; } // Thời gian bắt đầu kiểm tra
        public DateTime? InspectionCompletedDate { get; set; } // Thời gian hoàn thành kiểm tra

        // Ưu tiên xử lý
        [StringLength(20)]
        public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent

        // Loại dịch vụ yêu cầu
        [StringLength(50)]
        public string ServiceType { get; set; } = "General"; // General, Emergency, Maintenance, Repair, Inspection

        // Thông tin bảo hiểm (nếu có)
        public bool IsInsuranceClaim { get; set; } = false;
        [StringLength(100)]
        public string? InsuranceCompany { get; set; }
        [StringLength(50)]
        public string? InsurancePolicyNumber { get; set; }

        // Thông tin liên hệ khẩn cấp
        [StringLength(20)]
        public string? EmergencyContact { get; set; }
        [StringLength(200)]
        public string? EmergencyContactName { get; set; }

        // Cached thông tin để hiển thị nhanh
        [StringLength(200)]
        public string? CustomerName { get; set; }
        
        [StringLength(20)]
        public string? CustomerPhone { get; set; }
        
        [StringLength(20)]
        public string? VehiclePlate { get; set; }
        
        [StringLength(50)]
        public string? VehicleMake { get; set; }
        
        [StringLength(50)]
        public string? VehicleModel { get; set; }
        
        public int? VehicleYear { get; set; }

        // BaseEntity properties (inherited from BaseEntity)
        public new DateTime CreatedDate { get; set; } = DateTime.Now;
        public new DateTime UpdatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Customer Customer { get; set; } = null!;
        public virtual Vehicle Vehicle { get; set; } = null!;
        public virtual Employee? AssignedTechnician { get; set; }
        public virtual VehicleInspection? VehicleInspection { get; set; }
    }
}
