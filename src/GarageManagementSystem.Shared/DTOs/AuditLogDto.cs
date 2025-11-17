using System;
using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// ✅ Phase 4.1 - Advanced Features: Audit Log DTO
    /// </summary>
    public class AuditLogDto : BaseDto
    {
        public string EntityName { get; set; } = string.Empty;
        public int? EntityId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime Timestamp { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Details { get; set; }
        public string? Severity { get; set; }
    }

    /// <summary>
    /// DTO để lấy audit history cho một entity
    /// </summary>
    public class GetAuditHistoryDto
    {
        public string EntityName { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Action { get; set; }
    }
}

