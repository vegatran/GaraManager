using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    public class ServiceDto : BaseDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public string? Category { get; set; }
        public bool IsActive { get; set; }
        public int VATRate { get; set; } = 0; // ✅ THÊM: VAT rate từ Service entity
        public bool IsVATApplicable { get; set; } = false; // ✅ THÊM: VAT applicability từ Service entity
    }

    public class CreateServiceDto
    {
        [Required(ErrorMessage = "Tên dịch vụ là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên dịch vụ không được vượt quá 200 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Giá dịch vụ là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá dịch vụ phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Thời gian thực hiện là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Thời gian thực hiện phải lớn hơn 0")]
        public int Duration { get; set; }

        [StringLength(50, ErrorMessage = "Danh mục không được vượt quá 50 ký tự")]
        public string? Category { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateServiceDto : CreateServiceDto
    {
        [Required]
        public int Id { get; set; }
    }
}
