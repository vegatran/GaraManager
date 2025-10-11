using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    public class EmployeeDto : BaseDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Position { get; set; }
        public string? Department { get; set; }
        
        // Foreign key IDs
        public int? PositionId { get; set; }
        public int? DepartmentId { get; set; }
        
        // Navigation properties for display
        public string? PositionName { get; set; }
        public string? DepartmentName { get; set; }
        public DateTime? HireDate { get; set; }
        public decimal? Salary { get; set; }
        public string? Status { get; set; }
        public string? Skills { get; set; }
    }

    public class CreateEmployeeDto
    {
        [Required(ErrorMessage = "Tên nhân viên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên nhân viên không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(200, ErrorMessage = "Email không được vượt quá 200 ký tự")]
        public string? Email { get; set; }

        [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự")]
        public string? Address { get; set; }

        [StringLength(50, ErrorMessage = "Chức vụ không được vượt quá 50 ký tự")]
        public string? Position { get; set; }

        [StringLength(50, ErrorMessage = "Phòng ban không được vượt quá 50 ký tự")]
        public string? Department { get; set; }
        
        // Foreign key IDs
        public int? PositionId { get; set; }
        public int? DepartmentId { get; set; }

        public DateTime? HireDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Lương phải lớn hơn hoặc bằng 0")]
        public decimal? Salary { get; set; }

        [StringLength(20, ErrorMessage = "Trạng thái không được vượt quá 20 ký tự")]
        public string? Status { get; set; } = "Active";

        [StringLength(1000, ErrorMessage = "Kỹ năng không được vượt quá 1000 ký tự")]
        public string? Skills { get; set; }
    }

    public class UpdateEmployeeDto : CreateEmployeeDto
    {
        [Required]
        public int Id { get; set; }
    }
}
