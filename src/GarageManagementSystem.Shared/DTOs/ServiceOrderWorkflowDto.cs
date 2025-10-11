using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// DTO cho việc cập nhật trạng thái thanh toán
    /// </summary>
    public class UpdatePaymentStatusDto
    {
        [Required]
        [StringLength(50, ErrorMessage = "Payment status cannot exceed 50 characters")]
        public string PaymentStatus { get; set; } = string.Empty;

        [StringLength(5000, ErrorMessage = "Payment notes cannot exceed 5000 characters")]
        public string? PaymentNotes { get; set; }
    }

    /// <summary>
    /// DTO cho trạng thái quy trình đơn hàng dịch vụ
    /// </summary>
    public class ServiceOrderWorkflowStatusDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public List<string> RequiredApprovals { get; set; } = new();
        public List<string> NextRecommendedSteps { get; set; } = new();
        public bool IsWorkflowBlocked { get; set; }
        public List<string> BlockingReasons { get; set; } = new();
    }

    /// <summary>
    /// DTO cho thống kê trạng thái thanh toán đơn hàng dịch vụ
    /// </summary>
    public class ServiceOrderPaymentStatsDto
    {
        public int PendingCount { get; set; }
        public int InsurancePendingCount { get; set; }
        public int CompanyPendingCount { get; set; }
        public int PaidCount { get; set; }
        public int CancelledCount { get; set; }
        public int TotalCount { get; set; }
        public Dictionary<string, decimal> StatusPercentages { get; set; } = new();
    }
}
