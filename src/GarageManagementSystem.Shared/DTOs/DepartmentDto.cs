using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    public class DepartmentDto : BaseDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public string? ManagerName { get; set; }
        public string? Location { get; set; }
        public int? EmployeeCount { get; set; }
    }

    public class CreateDepartmentDto
    {
        [Required(ErrorMessage = "Tên bộ phận là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên bộ phận không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(100, ErrorMessage = "Tên quản lý không được vượt quá 100 ký tự")]
        public string? ManagerName { get; set; }

        [StringLength(200, ErrorMessage = "Địa điểm không được vượt quá 200 ký tự")]
        public string? Location { get; set; }
    }

    public class UpdateDepartmentDto : CreateDepartmentDto
    {
        [Required(ErrorMessage = "ID là bắt buộc")]
        public int Id { get; set; }
    }
}