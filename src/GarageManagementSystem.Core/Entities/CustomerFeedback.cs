using System.ComponentModel.DataAnnotations;
using GarageManagementSystem.Core.Enums;

namespace GarageManagementSystem.Core.Entities
{
    public class CustomerFeedback : BaseEntity
    {
        public int? CustomerId { get; set; }
        public int? ServiceOrderId { get; set; }

        [StringLength(100)]
        public string Source { get; set; } = FeedbackSources.Other;

        [StringLength(50)]
        public string Rating { get; set; } = FeedbackRatings.Neutral;

        [StringLength(200)]
        public string? Topic { get; set; }

        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? ActionTaken { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = FeedbackStatuses.New;

        public DateTime? FollowUpDate { get; set; }

        public int? FollowUpById { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public int? Score { get; set; } // optional numeric scoring e.g. NPS

        public int? FeedbackChannelId { get; set; }

        public virtual Customer? Customer { get; set; }
        public virtual ServiceOrder? ServiceOrder { get; set; }
        public virtual Employee? FollowUpBy { get; set; }
        public virtual FeedbackChannel? FeedbackChannel { get; set; }
        public virtual ICollection<CustomerFeedbackAttachment> Attachments { get; set; } = new List<CustomerFeedbackAttachment>();
    }
}

