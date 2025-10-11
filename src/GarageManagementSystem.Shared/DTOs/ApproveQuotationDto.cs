using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// DTO cho việc phê duyệt báo giá dịch vụ
    /// </summary>
    public class ApproveQuotationDto
    {
        [Required]
        public bool CreateServiceOrder { get; set; } = true;

        public DateTime? ScheduledDate { get; set; }

        [StringLength(5000, ErrorMessage = "Customer notes cannot exceed 5000 characters")]
        public string? CustomerNotes { get; set; }
    }
}
