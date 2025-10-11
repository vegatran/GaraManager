using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// DTO cho hóa đơn bảo hiểm
    /// </summary>
    public class InsuranceInvoiceDto
    {
        public int Id { get; set; }
        
        [Required]
        public int ServiceOrderId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string InsuranceCompany { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string PolicyNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string ClaimNumber { get; set; } = string.Empty;
        
        [Required]
        public DateTime AccidentDate { get; set; }
        
        [Required]
        [StringLength(500)]
        public string AccidentLocation { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string LicensePlate { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string VehicleModel { get; set; } = string.Empty;
        
        public decimal TotalApprovedAmount { get; set; }
        public decimal CustomerResponsibility { get; set; }
        public decimal InsuranceResponsibility { get; set; }
        public decimal VatAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string? Notes { get; set; }
        
        public List<InsuranceInvoiceItemDto> Items { get; set; } = new();
        
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO cho chi tiết hạng mục hóa đơn bảo hiểm
    /// </summary>
    public class InsuranceInvoiceItemDto
    {
        public int Id { get; set; }
        public int InsuranceInvoiceId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string ItemName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string ItemType { get; set; } = string.Empty; // Thay thế, Sửa chữa, Sơn
        
        public decimal ApprovedPrice { get; set; }
        public decimal CustomerPrice { get; set; }
        public decimal InsurancePrice { get; set; }
        
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
    }
}
