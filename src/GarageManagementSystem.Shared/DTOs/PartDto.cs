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
        
        // ✅ THÊM: Classification fields
        public string SourceType { get; set; } = "Purchased";
        public string InvoiceType { get; set; } = "WithInvoice";
        public bool HasInvoice { get; set; } = true;
        public bool CanUseForCompany { get; set; } = false;
        public bool CanUseForInsurance { get; set; } = false;
        public bool CanUseForIndividual { get; set; } = true;
        public string Condition { get; set; } = "New";
        public string? SourceReference { get; set; }
        
        // ✅ THÊM: Thông tin thuế VAT
        public decimal VATRate { get; set; } = 10;
        public bool IsVATApplicable { get; set; } = true;
        
        // ✅ THÊM: Technical fields
        public string? OEMNumber { get; set; }
        public string? AftermarketNumber { get; set; }
        public string? Manufacturer { get; set; }
        public string? Dimensions { get; set; }
        public decimal? Weight { get; set; }
        public string? Material { get; set; }
        public string? Color { get; set; }
        public int WarrantyMonths { get; set; } = 12;
        public bool IsOEM { get; set; } = false;
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
        
        // ✅ THÊM: Classification fields
        [StringLength(30)] public string SourceType { get; set; } = "Purchased";
        [StringLength(50)] public string InvoiceType { get; set; } = "WithInvoice";
        public bool HasInvoice { get; set; } = true;
        public bool CanUseForCompany { get; set; } = false;
        public bool CanUseForInsurance { get; set; } = false;
        public bool CanUseForIndividual { get; set; } = true;
        [StringLength(20)] public string Condition { get; set; } = "New";
        [StringLength(100)] public string? SourceReference { get; set; }
        
        // ✅ THÊM: Thông tin thuế VAT
        [Range(0, 20)] public decimal VATRate { get; set; } = 10;
        public bool IsVATApplicable { get; set; } = true;
        
        // ✅ THÊM: Technical fields
        [StringLength(50)] public string? OEMNumber { get; set; }
        [StringLength(50)] public string? AftermarketNumber { get; set; }
        [StringLength(100)] public string? Manufacturer { get; set; }
        [StringLength(100)] public string? Dimensions { get; set; }
        public decimal? Weight { get; set; }
        [StringLength(50)] public string? Material { get; set; }
        [StringLength(50)] public string? Color { get; set; }
        [Range(0, 120)] public int WarrantyMonths { get; set; } = 12;
        public bool IsOEM { get; set; } = false;
    }

    public class UpdatePartDto : CreatePartDto
    {
        [Required] public int Id { get; set; }
    }
}

