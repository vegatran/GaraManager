using GarageManagementSystem.Core.Entities;

namespace GarageManagementSystem.Core.Interfaces
{
    public interface IWarrantyService
    {
        Task<Warranty> GenerateWarrantyForServiceOrderAsync(int serviceOrderId, WarrantyGenerationOptions options, CancellationToken cancellationToken = default);
        Task<Warranty?> GetWarrantyByServiceOrderAsync(int serviceOrderId, CancellationToken cancellationToken = default);
        Task<Warranty?> GetWarrantyByCodeAsync(string warrantyCode, CancellationToken cancellationToken = default);
        Task<IEnumerable<Warranty>> SearchWarrantiesAsync(WarrantySearchFilter filter, CancellationToken cancellationToken = default);
        Task<WarrantyClaim> CreateWarrantyClaimAsync(int warrantyId, WarrantyClaimCreateRequest request, CancellationToken cancellationToken = default);
        Task<WarrantyClaim?> UpdateWarrantyClaimStatusAsync(int claimId, WarrantyClaimUpdateRequest request, CancellationToken cancellationToken = default);
    }

    public class WarrantyGenerationOptions
    {
        public DateTime? WarrantyStartDate { get; set; }
        public int DefaultWarrantyMonths { get; set; } = 3;
        public bool ForceRegenerate { get; set; } = false;
        public string? GeneratedBy { get; set; }
        public string? HandoverBy { get; set; }
        public string? HandoverLocation { get; set; }
    }

    public class WarrantySearchFilter
    {
        public int? CustomerId { get; set; }
        public int? VehicleId { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Keyword { get; set; }
    }

    public class WarrantyClaimCreateRequest
    {
        public string? ClaimNumber { get; set; }
        public DateTime? ClaimDate { get; set; }
        public int? ServiceOrderId { get; set; }
        public int? CustomerId { get; set; }
        public int? VehicleId { get; set; }
        public string IssueDescription { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class WarrantyClaimUpdateRequest
    {
        public string Status { get; set; } = "Pending";
        public string? Resolution { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string? Notes { get; set; }
    }
}

