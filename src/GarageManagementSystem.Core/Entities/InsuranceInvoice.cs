using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Hóa đơn bảo hiểm
    /// </summary>
    public class InsuranceInvoice : BaseEntity
    {
        [Required]
        public int ServiceOrderId { get; set; }
        public virtual ServiceOrder ServiceOrder { get; set; } = null!;
        
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
        
        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;
        
        public virtual ICollection<InsuranceInvoiceItem> Items { get; set; } = new List<InsuranceInvoiceItem>();
    }

    /// <summary>
    /// Chi tiết hạng mục hóa đơn bảo hiểm
    /// </summary>
    public class InsuranceInvoiceItem : BaseEntity
    {
        [Required]
        public int InsuranceInvoiceId { get; set; }
        public virtual InsuranceInvoice InsuranceInvoice { get; set; } = null!;
        
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
