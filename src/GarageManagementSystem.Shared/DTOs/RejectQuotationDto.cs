using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// DTO cho việc từ chối báo giá dịch vụ
    /// </summary>
    public class RejectQuotationDto
    {
        [Required(ErrorMessage = "Lý do từ chối là bắt buộc")]
        [StringLength(5000, ErrorMessage = "Lý do từ chối không được vượt quá 5000 ký tự")]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Có tính phí kiểm tra hay không
        /// </summary>
        public bool ChargeInspectionFee { get; set; } = false;
    }
}

