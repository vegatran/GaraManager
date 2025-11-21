using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    public class ServiceOrderPartDto : BaseDto
    {
        public int ServiceOrderId { get; set; }
        public int PartId { get; set; }
        public string PartName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; }
        public bool IsWarranty { get; set; }
        public DateTime? WarrantyUntil { get; set; }
        public DateTime? ReturnDate { get; set; }
    }
}

