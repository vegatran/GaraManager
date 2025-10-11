using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// DTO for Insurance Company approval workflow
    /// </summary>
    public class InsuranceApprovalDto
    {
        [Required]
        public int QuotationId { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Approved amount must be positive")]
        public decimal InsuranceApprovedAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Deductible must be positive")]
        public decimal? Deductible { get; set; }

        [StringLength(5000, ErrorMessage = "Notes cannot exceed 5000 characters")]
        public string? InsuranceApprovalNotes { get; set; }

        [StringLength(200, ErrorMessage = "Adjuster contact cannot exceed 200 characters")]
        public string? InsuranceAdjusterContact { get; set; }

        public bool IsApproved { get; set; } = true;

        [StringLength(5000, ErrorMessage = "Rejection reason cannot exceed 5000 characters")]
        public string? RejectionReason { get; set; }
    }

    /// <summary>
    /// DTO for Insurance claim submission
    /// </summary>
    public class SubmitInsuranceClaimDto
    {
        [Required]
        public int VehicleId { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Claim number cannot exceed 50 characters")]
        public string ClaimNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "Insurance company cannot exceed 100 characters")]
        public string InsuranceCompany { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Policy number cannot exceed 50 characters")]
        public string? PolicyNumber { get; set; }

        [StringLength(50, ErrorMessage = "Coverage type cannot exceed 50 characters")]
        public string? CoverageType { get; set; }

        [StringLength(100, ErrorMessage = "Adjuster name cannot exceed 100 characters")]
        public string? AdjusterName { get; set; }

        [StringLength(20, ErrorMessage = "Adjuster phone cannot exceed 20 characters")]
        public string? AdjusterPhone { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Max insurance amount must be positive")]
        public decimal? MaxInsuranceAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Deductible must be positive")]
        public decimal? Deductible { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }
    }
}
