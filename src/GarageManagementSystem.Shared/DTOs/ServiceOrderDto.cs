using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    public class ServiceOrderDto : BaseDto
    {
        public string OrderNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public int VehicleId { get; set; }
        public CustomerDto? Customer { get; set; }
        public VehicleDto? Vehicle { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string? PaymentStatus { get; set; }
        public List<ServiceOrderItemDto> ServiceOrderItems { get; set; } = new();
    }

    public class CreateServiceOrderDto
    {
        [Required(ErrorMessage = "Khách hàng là bắt buộc")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Xe là bắt buộc")]
        public int VehicleId { get; set; }

        public DateTime? ScheduledDate { get; set; }

        [StringLength(5000, ErrorMessage = "Ghi chú không được vượt quá 5000 ký tự")]
        public string? Notes { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Số tiền giảm giá phải lớn hơn hoặc bằng 0")]
        public decimal DiscountAmount { get; set; } = 0;

        [Required(ErrorMessage = "Danh sách dịch vụ là bắt buộc")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất một dịch vụ")]
        public List<CreateServiceOrderItemDto> ServiceOrderItems { get; set; } = new();
    }

    public class UpdateServiceOrderDto
    {
        [Required]
        public int Id { get; set; }

        public DateTime? ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        [StringLength(20, ErrorMessage = "Trạng thái không được vượt quá 20 ký tự")]
        public string? Status { get; set; }

        [StringLength(5000, ErrorMessage = "Ghi chú không được vượt quá 5000 ký tự")]
        public string? Notes { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Số tiền giảm giá phải lớn hơn hoặc bằng 0")]
        public decimal DiscountAmount { get; set; }

        [StringLength(50, ErrorMessage = "Trạng thái thanh toán không được vượt quá 50 ký tự")]
        public string? PaymentStatus { get; set; }
    }
}
