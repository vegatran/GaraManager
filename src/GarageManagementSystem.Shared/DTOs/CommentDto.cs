using System;
using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// ✅ Phase 4.1 - Advanced Features: Comment DTO
    /// </summary>
    public class InventoryCheckCommentDto : BaseDto
    {
        public int InventoryCheckId { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public int? CreatedByEmployeeId { get; set; }
        public string? CreatedByEmployeeName { get; set; }
        public string? CreatedByUserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class InventoryAdjustmentCommentDto : BaseDto
    {
        public int InventoryAdjustmentId { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public int? CreatedByEmployeeId { get; set; }
        public string? CreatedByEmployeeName { get; set; }
        public string? CreatedByUserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateInventoryCheckCommentDto
    {
        [Required]
        public int InventoryCheckId { get; set; }

        [Required]
        [StringLength(2000, ErrorMessage = "Comment không được vượt quá 2000 ký tự")]
        public string CommentText { get; set; } = string.Empty;
    }

    public class CreateInventoryAdjustmentCommentDto
    {
        [Required]
        public int InventoryAdjustmentId { get; set; }

        [Required]
        [StringLength(2000, ErrorMessage = "Comment không được vượt quá 2000 ký tự")]
        public string CommentText { get; set; } = string.Empty;
    }
}

