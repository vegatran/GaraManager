namespace GarageManagementSystem.Shared.DTOs
{
    public class ServiceFeeTypeDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsSystem { get; set; }
    }

    public class ServiceOrderFeeDto : BaseDto
    {
        public int ServiceOrderId { get; set; }
        public int ServiceFeeTypeId { get; set; }
        public string ServiceFeeTypeName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? ReferenceSource { get; set; }
        public string? Notes { get; set; }
        public bool IsManual { get; set; }
    }

    public class ServiceOrderFeeSummaryDto
    {
        public int ServiceOrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalVat { get; set; }
        public decimal TotalDiscount { get; set; }
        public List<ServiceOrderFeeDto> Fees { get; set; } = new();
        public List<ServiceFeeTypeDto> FeeTypes { get; set; } = new();
    }

    public class UpsertServiceOrderFeeDto
    {
        public int? Id { get; set; }
        public int ServiceFeeTypeId { get; set; }
        public decimal Amount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? ReferenceSource { get; set; }
        public string? Notes { get; set; }
        public bool IsManual { get; set; } = true;
    }

    public class UpdateServiceOrderFeesRequestDto
    {
        public List<UpsertServiceOrderFeeDto> Fees { get; set; } = new();
    }

    public class FeedbackChannelDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class CustomerFeedbackAttachmentDto : BaseDto
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? ContentType { get; set; }
        public long FileSize { get; set; }
    }

    public class CustomerFeedbackDto : BaseDto
    {
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? ServiceOrderId { get; set; }
        public string? OrderNumber { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Rating { get; set; } = "Neutral";
        public string? Topic { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? ActionTaken { get; set; }
        public string Status { get; set; } = "New";
        public DateTime? FollowUpDate { get; set; }
        public int? FollowUpById { get; set; }
        public string? FollowUpByName { get; set; }
        public string? Notes { get; set; }
        public int? Score { get; set; }
        public int? FeedbackChannelId { get; set; }
        public string? FeedbackChannelName { get; set; }
        public List<CustomerFeedbackAttachmentDto> Attachments { get; set; } = new();
    }

    public class CreateCustomerFeedbackDto
    {
        public int? CustomerId { get; set; }
        public int? ServiceOrderId { get; set; }
        public string Source { get; set; } = "Other";
        public string Rating { get; set; } = "Neutral";
        public string? Topic { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Status { get; set; } = "New";
        public DateTime? FollowUpDate { get; set; }
        public int? FollowUpById { get; set; }
        public string? Notes { get; set; }
        public int? Score { get; set; }
        public int? FeedbackChannelId { get; set; }
    }

    public class UpdateCustomerFeedbackDto
    {
        public string? Rating { get; set; }
        public string? Topic { get; set; }
        public string? Content { get; set; }
        public string? ActionTaken { get; set; }
        public string? Status { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public int? FollowUpById { get; set; }
        public string? Notes { get; set; }
        public int? Score { get; set; }
        public int? FeedbackChannelId { get; set; }
    }

    public class CustomerFeedbackFilterDto
    {
        public int? CustomerId { get; set; }
        public int? ServiceOrderId { get; set; }
        public string? Status { get; set; }
        public string? Source { get; set; }
        public string? Rating { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Keyword { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}

