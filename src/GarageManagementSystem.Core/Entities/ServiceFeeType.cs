using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Danh mục loại phí dịch vụ (Labor, Parts, Surcharge, v.v.)
    /// </summary>
    public class ServiceFeeType : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; } = 0;

        public bool IsSystem { get; set; } = false;

        public virtual ICollection<ServiceOrderFee> ServiceOrderFees { get; set; } = new List<ServiceOrderFee>();
    }
}

