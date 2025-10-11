using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    public class OpeningBalanceDto
    {
        [Required]
        [StringLength(50)]
        public string PartNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string PartName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(100)]
        public string? Brand { get; set; }

        [StringLength(20)]
        public string? Unit { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        public decimal TotalValue => Quantity * UnitPrice;

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(500)]
        public string? CompatibleVehicles { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        // For import validation
        public bool IsValid => !string.IsNullOrEmpty(PartNumber) && 
                              !string.IsNullOrEmpty(PartName) && 
                              Quantity > 0 && 
                              UnitPrice > 0;

        public string ValidationMessage
        {
            get
            {
                if (string.IsNullOrEmpty(PartNumber)) return "Part Number is required";
                if (string.IsNullOrEmpty(PartName)) return "Part Name is required";
                if (Quantity <= 0) return "Quantity must be greater than 0";
                if (UnitPrice <= 0) return "Unit Price must be greater than 0";
                return "Valid";
            }
        }
    }

    public class OpeningBalanceImportRequest
    {
        public List<OpeningBalanceDto> Items { get; set; } = new List<OpeningBalanceDto>();
        
        public DateTime OpeningBalanceDate { get; set; } = DateTime.Now;
        
        [StringLength(1000)]
        public string? Notes { get; set; }

        public int TotalItems => Items.Count;
        public int ValidItems => Items.Count(x => x.IsValid);
        public int InvalidItems => Items.Count(x => !x.IsValid);
        public decimal TotalValue => Items.Where(x => x.IsValid).Sum(x => x.TotalValue);
    }

    public class OpeningBalanceImportResult
    {
        public bool Success { get; set; }
        public int TotalProcessed { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public string Message { get; set; } = string.Empty;
    }
}
