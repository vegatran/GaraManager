using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// InsuranceClaim - Quản lý claim bảo hiểm
    /// </summary>
    public class InsuranceClaim : BaseEntity
    {
        public int VehicleInsuranceId { get; set; }
        
        public int? ServiceOrderId { get; set; } // Liên quan đến đơn hàng sửa chữa
        public int? CustomerId { get; set; } // Link to customer
        public int? VehicleId { get; set; } // Link to vehicle
        public int? InvoiceId { get; set; } // Link to invoice
        
        [Required]
        [StringLength(50)]
        public string ClaimNumber { get; set; } = string.Empty; // Số claim
        
        [Required]
        public DateTime ClaimDate { get; set; } = DateTime.Now; // Ngày khởi tạo claim
        
        [StringLength(20)]
        public string ClaimStatus { get; set; } = "Pending"; // Pending, Submitted, Under Review, Approved, Rejected, Settled
        
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Alias for ClaimStatus
        
        [StringLength(100)]
        public string? IncidentType { get; set; } // Loại sự cố: Accident, Theft, Fire, Natural Disaster, Vandalism
        
        [StringLength(500)]
        public string? IncidentDescription { get; set; } // Mô tả sự cố
        
        public DateTime? IncidentDate { get; set; } // Ngày xảy ra sự cố
        public DateTime? AccidentDate { get; set; } // Alias for IncidentDate
        
        [StringLength(200)]
        public string? IncidentLocation { get; set; } // Địa điểm xảy ra sự cố
        
        [StringLength(200)]
        public string? AccidentLocation { get; set; } // Alias for IncidentLocation
        
        [StringLength(500)]
        public string? AccidentDescription { get; set; } // Alias for IncidentDescription
        
        [StringLength(500)]
        public string? DamageDescription { get; set; } // Description of damage
        
        [StringLength(100)]
        public string? PoliceReportNumber { get; set; } // Số biên bản công an
        
        [StringLength(100)]
        public string? AdjusterName { get; set; } // Tên điều chỉnh viên
        
        [StringLength(20)]
        public string? AdjusterPhone { get; set; } // SĐT điều chỉnh viên
        
        [StringLength(100)]
        public string? AdjusterEmail { get; set; } // Email điều chỉnh viên
        
        public decimal? EstimatedDamage { get; set; } // Thiệt hại ước tính
        public decimal? EstimatedAmount { get; set; } // Alias for EstimatedDamage
        
        public decimal? ApprovedAmount { get; set; } // Số tiền được phê duyệt
        public decimal? SettlementAmount { get; set; } // Final settlement amount
        
        [StringLength(100)]
        public string? ApprovedBy { get; set; } // Who approved the claim
        
        public decimal? PaidAmount { get; set; } // Số tiền đã thanh toán
        
        public DateTime? ApprovalDate { get; set; } // Ngày phê duyệt
        
        public DateTime? SettlementDate { get; set; } // Ngày giải quyết
        
        [StringLength(500)]
        public string? AdjusterNotes { get; set; } // Ghi chú của điều chỉnh viên
        
        [StringLength(500)]
        public string? CustomerNotes { get; set; } // Ghi chú của khách hàng
        
        [StringLength(100)]
        public string? RepairShopName { get; set; } // Tên garage sửa chữa
        
        [StringLength(200)]
        public string? RepairShopAddress { get; set; } // Địa chỉ garage
        
        [StringLength(20)]
        public string? RepairShopPhone { get; set; } // SĐT garage
        
        public bool RequiresInspection { get; set; } = false; // Cần kiểm tra
        
        public DateTime? InspectionDate { get; set; } // Ngày kiểm tra
        
        [StringLength(100)]
        public string? InspectorName { get; set; } // Tên người kiểm tra
        
        [StringLength(500)]
        public string? InspectionReport { get; set; } // Báo cáo kiểm tra
        
        public bool IsRepairCompleted { get; set; } = false; // Đã hoàn thành sửa chữa
        
        public DateTime? RepairCompletionDate { get; set; } // Ngày hoàn thành sửa chữa
        
        [StringLength(500)]
        public string? Notes { get; set; } // Ghi chú tổng quát
        
        // Additional fields for API compatibility
        [StringLength(200)]
        public string? CustomerName { get; set; }
        
        [StringLength(20)]
        public string? CustomerPhone { get; set; }
        
        [StringLength(100)]
        public string? CustomerEmail { get; set; }
        
        [StringLength(20)]
        public string? VehiclePlate { get; set; }
        
        [StringLength(50)]
        public string? VehicleMake { get; set; }
        
        [StringLength(50)]
        public string? VehicleModel { get; set; }
        
        public int? VehicleYear { get; set; }
        
        [StringLength(100)]
        public string? InsuranceCompany { get; set; }
        
        [StringLength(50)]
        public string? PolicyNumber { get; set; }
        
        [StringLength(100)]
        public string? PolicyHolderName { get; set; }
        
        // Navigation properties
        public virtual VehicleInsurance VehicleInsurance { get; set; } = null!;
        public virtual ServiceOrder? ServiceOrder { get; set; }
        public virtual Customer? Customer { get; set; }
        public virtual Vehicle? Vehicle { get; set; }
        public virtual Invoice? Invoice { get; set; }
        public virtual ICollection<InsuranceClaimDocument> Documents { get; set; } = new List<InsuranceClaimDocument>();
    }
}
