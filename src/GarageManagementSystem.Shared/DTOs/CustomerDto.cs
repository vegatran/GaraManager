using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    public class CustomerDto : BaseDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? AlternativePhone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? ContactPersonName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public List<VehicleDto> Vehicles { get; set; } = new();
    }

    public class CreateCustomerDto
    {
        [Required(ErrorMessage = "Tên khách hàng là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên khách hàng không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        public string? Phone { get; set; }

        [StringLength(20, ErrorMessage = "Số điện thoại phụ không được vượt quá 20 ký tự")]
        public string? AlternativePhone { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(200, ErrorMessage = "Email không được vượt quá 200 ký tự")]
        public string? Email { get; set; }

        [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự")]
        public string? Address { get; set; }

        [StringLength(100, ErrorMessage = "Tên người liên hệ không được vượt quá 100 ký tự")]
        public string? ContactPersonName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(20, ErrorMessage = "Giới tính không được vượt quá 20 ký tự")]
        public string? Gender { get; set; }
    }

    public class UpdateCustomerDto : CreateCustomerDto
    {
        [Required]
        public int Id { get; set; }
    }
}
