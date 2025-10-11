using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    public class SupplierDto : BaseDto
    {
        public string SupplierCode { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactPhone { get; set; }
        public string? TaxCode { get; set; }
        public string? BankAccount { get; set; }
        public string? BankName { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public int? Rating { get; set; }
    }

    public class CreateSupplierDto
    {
        [Required] [StringLength(50)] public string SupplierCode { get; set; } = string.Empty;
        [Required] [StringLength(200)] public string SupplierName { get; set; } = string.Empty;
        [StringLength(20)] public string? Phone { get; set; }
        [EmailAddress] [StringLength(200)] public string? Email { get; set; }
        [StringLength(500)] public string? Address { get; set; }
        [StringLength(100)] public string? ContactPerson { get; set; }
        [StringLength(20)] public string? ContactPhone { get; set; }
        [StringLength(50)] public string? TaxCode { get; set; }
        [StringLength(100)] public string? BankAccount { get; set; }
        [StringLength(200)] public string? BankName { get; set; }
        [StringLength(1000)] public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        [Range(1, 5)] public int? Rating { get; set; }
    }

    public class UpdateSupplierDto : CreateSupplierDto
    {
        [Required] public int Id { get; set; }
    }
}

