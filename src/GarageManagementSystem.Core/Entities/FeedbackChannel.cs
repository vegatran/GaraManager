using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    public class FeedbackChannel : BaseEntity
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

        public virtual ICollection<CustomerFeedback> Feedbacks { get; set; } = new List<CustomerFeedback>();
    }
}

