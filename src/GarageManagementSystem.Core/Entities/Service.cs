using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    public class Service : BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Duration { get; set; } // Duration in minutes

        [StringLength(50)]
        public string? Category { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<ServiceOrderItem> ServiceOrderItems { get; set; } = new List<ServiceOrderItem>();
        public virtual ICollection<InspectionIssue> RelatedInspectionIssues { get; set; } = new List<InspectionIssue>();
        public virtual ICollection<QuotationItem> QuotationItems { get; set; } = new List<QuotationItem>();
    }
}
