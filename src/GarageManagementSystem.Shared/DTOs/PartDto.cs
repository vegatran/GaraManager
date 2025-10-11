using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    public class PartDto : BaseDto
    {
        public string PartNumber { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? Brand { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellPrice { get; set; }
        public int QuantityInStock { get; set; }
        public int MinimumStock { get; set; }
        public int? ReorderLevel { get; set; }
        public string? Unit { get; set; }
        public string? CompatibleVehicles { get; set; }
        public string? Location { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreatePartDto
    {
        [Required] [StringLength(50)] public string PartNumber { get; set; } = string.Empty;
        [Required] [StringLength(200)] public string PartName { get; set; } = string.Empty;
        [StringLength(1000)] public string? Description { get; set; }
        [StringLength(100)] public string? Category { get; set; }
        [StringLength(100)] public string? Brand { get; set; }
        [Required] [Range(0, double.MaxValue)] public decimal CostPrice { get; set; }
        [Required] [Range(0, double.MaxValue)] public decimal SellPrice { get; set; }
        [Required] [Range(0, int.MaxValue)] public int QuantityInStock { get; set; } = 0;
        [Range(0, int.MaxValue)] public int MinimumStock { get; set; } = 0;
        public int? ReorderLevel { get; set; }
        [StringLength(20)] public string? Unit { get; set; }
        [StringLength(500)] public string? CompatibleVehicles { get; set; }
        [StringLength(100)] public string? Location { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdatePartDto : CreatePartDto
    {
        [Required] public int Id { get; set; }
    }
}

