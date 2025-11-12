using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    public class WarehouseDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? ManagerName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public List<WarehouseZoneDto> Zones { get; set; } = new();
        public List<WarehouseBinDto> Bins { get; set; } = new();
    }

    public class WarehouseZoneDto : BaseDto
    {
        public int WarehouseId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public List<WarehouseBinDto> Bins { get; set; } = new();
    }

    public class WarehouseBinDto : BaseDto
    {
        public int WarehouseId { get; set; }
        public int? WarehouseZoneId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? Capacity { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateWarehouseDto
    {
        [Required, StringLength(50)] public string Code { get; set; } = string.Empty;
        [Required, StringLength(150)] public string Name { get; set; } = string.Empty;
        [StringLength(300)] public string? Description { get; set; }
        [StringLength(300)] public string? Address { get; set; }
        [StringLength(100)] public string? ManagerName { get; set; }
        [StringLength(50)] public string? PhoneNumber { get; set; }
        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }

    public class UpdateWarehouseDto : CreateWarehouseDto
    {
        [Required] public int Id { get; set; }
    }

    public class WarehouseZoneRequestDto
    {
        public int? Id { get; set; }
        [Required, StringLength(50)] public string Code { get; set; } = string.Empty;
        [Required, StringLength(150)] public string Name { get; set; } = string.Empty;
        [StringLength(300)] public string? Description { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }

    public class WarehouseBinRequestDto
    {
        public int? Id { get; set; }
        public int? WarehouseZoneId { get; set; }
        [Required, StringLength(50)] public string Code { get; set; } = string.Empty;
        [Required, StringLength(150)] public string Name { get; set; } = string.Empty;
        [StringLength(300)] public string? Description { get; set; }
        [Range(0, double.MaxValue)] public decimal? Capacity { get; set; }
        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }
}

