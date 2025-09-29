using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    public class VehicleDto
    {
        public int Id { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string? Year { get; set; }
        public string? Color { get; set; }
        public string? VIN { get; set; }
        public string? EngineNumber { get; set; }
        public int CustomerId { get; set; }
        public CustomerDto? Customer { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateVehicleDto
    {
        [Required(ErrorMessage = "Biển số xe là bắt buộc")]
        [StringLength(20, ErrorMessage = "Biển số xe không được vượt quá 20 ký tự")]
        public string LicensePlate { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hãng xe là bắt buộc")]
        [StringLength(100, ErrorMessage = "Hãng xe không được vượt quá 100 ký tự")]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Dòng xe là bắt buộc")]
        [StringLength(100, ErrorMessage = "Dòng xe không được vượt quá 100 ký tự")]
        public string Model { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Năm sản xuất không được vượt quá 20 ký tự")]
        public string? Year { get; set; }

        [StringLength(50, ErrorMessage = "Màu sắc không được vượt quá 50 ký tự")]
        public string? Color { get; set; }

        [StringLength(17, ErrorMessage = "Số khung không được vượt quá 17 ký tự")]
        public string? VIN { get; set; }

        [StringLength(50, ErrorMessage = "Số máy không được vượt quá 50 ký tự")]
        public string? EngineNumber { get; set; }

        [Required(ErrorMessage = "Khách hàng là bắt buộc")]
        public int CustomerId { get; set; }
    }

    public class UpdateVehicleDto : CreateVehicleDto
    {
        [Required]
        public int Id { get; set; }
    }
}
