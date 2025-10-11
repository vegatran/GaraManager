using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// DTO for Company approval workflow
    /// </summary>
    public class CompanyApprovalDto
    {
        [Required]
        public int QuotationId { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "PO number cannot exceed 50 characters")]
        public string PONumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "Approved by cannot exceed 100 characters")]
        public string CompanyApprovedBy { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Payment terms cannot exceed 20 characters")]
        public string PaymentTerms { get; set; } = "Net30"; // Cash, Net15, Net30, Net45

        public bool IsTaxExempt { get; set; } = false;

        [StringLength(5000, ErrorMessage = "Approval notes cannot exceed 5000 characters")]
        public string? CompanyApprovalNotes { get; set; }

        [StringLength(100, ErrorMessage = "Contact person cannot exceed 100 characters")]
        public string? CompanyContactPerson { get; set; }

        public bool IsApproved { get; set; } = true;

        [StringLength(5000, ErrorMessage = "Rejection reason cannot exceed 5000 characters")]
        public string? RejectionReason { get; set; }
    }

    /// <summary>
    /// DTO for Company vehicle registration
    /// </summary>
    public class RegisterCompanyVehicleDto
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "Company name cannot exceed 200 characters")]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Tax code cannot exceed 20 characters")]
        public string? TaxCode { get; set; }

        [StringLength(100, ErrorMessage = "Contact person cannot exceed 100 characters")]
        public string? ContactPerson { get; set; }

        [StringLength(20, ErrorMessage = "Contact phone cannot exceed 20 characters")]
        public string? ContactPhone { get; set; }

        [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
        public string? Department { get; set; }

        [StringLength(50, ErrorMessage = "Cost center cannot exceed 50 characters")]
        public string? CostCenter { get; set; }

        // Vehicle information
        [Required]
        [StringLength(20, ErrorMessage = "License plate cannot exceed 20 characters")]
        public string LicensePlate { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "Brand cannot exceed 100 characters")]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "Model cannot exceed 100 characters")]
        public string Model { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Year cannot exceed 20 characters")]
        public string? Year { get; set; }

        [StringLength(50, ErrorMessage = "Color cannot exceed 50 characters")]
        public string? Color { get; set; }

        [StringLength(17, ErrorMessage = "VIN cannot exceed 17 characters")]
        public string? VIN { get; set; }

        [StringLength(50, ErrorMessage = "Engine number cannot exceed 50 characters")]
        public string? EngineNumber { get; set; }
    }
}
