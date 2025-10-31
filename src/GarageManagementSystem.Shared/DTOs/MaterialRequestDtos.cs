using System.ComponentModel.DataAnnotations;
using GarageManagementSystem.Core.Enums;

namespace GarageManagementSystem.Shared.DTOs
{
    public class MaterialRequestItemDto
    {
        public int Id { get; set; }
        public int PartId { get; set; }
        public string? PartName { get; set; }
        public int QuantityRequested { get; set; }
        public int QuantityApproved { get; set; }
        public int QuantityPicked { get; set; }
        public int QuantityIssued { get; set; }
        public int QuantityDelivered { get; set; }
        public string? Notes { get; set; }
    }

    public class MaterialRequestDto
    {
        public int Id { get; set; }
        public string MRNumber { get; set; } = string.Empty;
        public int ServiceOrderId { get; set; }
        public MaterialRequestStatus Status { get; set; }
        public string? Notes { get; set; }
        public string? RejectReason { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? IssuedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public List<MaterialRequestItemDto> Items { get; set; } = new();
    }

    public class CreateMaterialRequestDto
    {
        [Required]
        public int ServiceOrderId { get; set; }
        [StringLength(1000)]
        public string? Notes { get; set; }
        public List<CreateMaterialRequestItemDto> Items { get; set; } = new();
    }

    public class CreateMaterialRequestItemDto
    {
        [Required]
        public int PartId { get; set; }
        [Range(1, int.MaxValue)]
        public int QuantityRequested { get; set; }
        public string? Notes { get; set; }
    }

    public class ChangeMaterialRequestStatusDto
    {
        [Required]
        public MaterialRequestStatus Status { get; set; }
        public string? Reason { get; set; }
    }
}


