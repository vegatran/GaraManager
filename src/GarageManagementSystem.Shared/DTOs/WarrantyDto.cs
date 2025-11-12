namespace GarageManagementSystem.Shared.DTOs
{
    public class WarrantyDto : BaseDto
    {
        public string WarrantyCode { get; set; } = string.Empty;
        public int ServiceOrderId { get; set; }
        public string? ServiceOrderNumber { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int VehicleId { get; set; }
        public string? VehicleLicensePlate { get; set; }
        public DateTime WarrantyStartDate { get; set; }
        public DateTime WarrantyEndDate { get; set; }
        public string Status { get; set; } = "Active";
        public string? Notes { get; set; }
        public string? HandoverBy { get; set; }
        public string? HandoverLocation { get; set; }

        public List<WarrantyItemDto> Items { get; set; } = new();
        public List<WarrantyClaimDto> Claims { get; set; } = new();
    }

    public class WarrantyItemDto : BaseDto
    {
        public int WarrantyId { get; set; }
        public int? ServiceOrderPartId { get; set; }
        public int? PartId { get; set; }
        public string PartName { get; set; } = string.Empty;
        public string? PartNumber { get; set; }
        public string? SerialNumber { get; set; }
        public int WarrantyMonths { get; set; }
        public DateTime WarrantyStartDate { get; set; }
        public DateTime WarrantyEndDate { get; set; }
        public string Status { get; set; } = "Active";
        public string? Notes { get; set; }
    }

    public class WarrantyClaimDto : BaseDto
    {
        public string ClaimNumber { get; set; } = string.Empty;
        public int WarrantyId { get; set; }
        public int? ServiceOrderId { get; set; }
        public DateTime ClaimDate { get; set; }
        public string Status { get; set; } = "Pending";
        public string IssueDescription { get; set; } = string.Empty;
        public string? Resolution { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string? Notes { get; set; }
    }

    public class WarrantySummaryDto : BaseDto
    {
        public string WarrantyCode { get; set; } = string.Empty;
        public int ServiceOrderId { get; set; }
        public string? ServiceOrderNumber { get; set; }
        public string? CustomerName { get; set; }
        public string? VehicleLicensePlate { get; set; }
        public DateTime WarrantyStartDate { get; set; }
        public DateTime WarrantyEndDate { get; set; }
        public string Status { get; set; } = "Active";
        public int ItemCount { get; set; }
        public int ClaimCount { get; set; }
    }

    public class CreateWarrantyClaimDto
    {
        public string? ClaimNumber { get; set; }
        public DateTime? ClaimDate { get; set; }
        public int? ServiceOrderId { get; set; }
        public int? CustomerId { get; set; }
        public int? VehicleId { get; set; }
        public string IssueDescription { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class UpdateWarrantyClaimStatusDto
    {
        public string Status { get; set; } = "Pending";
        public string? Resolution { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string? Notes { get; set; }
    }

    public class WarrantySearchFilterDto
    {
        public int? CustomerId { get; set; }
        public int? VehicleId { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Keyword { get; set; }
    }

    public class GenerateWarrantyRequestDto
    {
        public DateTime? WarrantyStartDate { get; set; }
        public int? DefaultWarrantyMonths { get; set; }
        public bool ForceRegenerate { get; set; } = false;
        public string? HandoverBy { get; set; }
        public string? HandoverLocation { get; set; }
    }
}

