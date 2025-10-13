using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    public class PositionDto : BaseDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public string? Requirements { get; set; }
    }

    public class CreatePositionDto
    {
        [Required(ErrorMessage = "Tên chức vụ là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên chức vụ không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        [Range(0, double.MaxValue, ErrorMessage = "Lương tối thiểu phải >= 0")]
        public decimal? MinSalary { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Lương tối đa phải >= 0")]
        public decimal? MaxSalary { get; set; }

        [StringLength(1000, ErrorMessage = "Yêu cầu không được vượt quá 1000 ký tự")]
        public string? Requirements { get; set; }
    }

    public class UpdatePositionDto : CreatePositionDto
    {
        [Required(ErrorMessage = "ID là bắt buộc")]
        public int Id { get; set; }
    }
}