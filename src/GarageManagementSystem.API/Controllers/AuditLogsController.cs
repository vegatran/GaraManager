using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Services;
using GarageManagementSystem.Shared.Models;
using GarageManagementSystem.Core.Extensions;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<AuditLogsController> _logger;

        public AuditLogsController(IAuditLogService auditLogService, ILogger<AuditLogsController> logger)
        {
            _auditLogService = auditLogService;
            _logger = logger;
        }

        /// <summary>
        /// Get audit logs with filters
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? entityName = null,
            [FromQuery] string? userId = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var logs = await _auditLogService.GetLogsAsync(fromDate, toDate, entityName, userId);
                
                // Apply search filter if provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    logs = logs.Where(l => 
                        l.EntityName.Contains(searchTerm) || 
                        l.UserName.Contains(searchTerm) || 
                        l.Action.Contains(searchTerm) ||
                        (l.Details != null && l.Details.Contains(searchTerm)));
                }

                var query = logs.AsQueryable();
                var totalCount = query.Count();
                
                var pagedLogs = query
                    .OrderByDescending(l => l.Timestamp)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(l => new
                    {
                        l.Id,
                        l.EntityName,
                        l.EntityId,
                        l.Action,
                        l.UserId,
                        l.UserName,
                        l.Timestamp,
                        l.IpAddress,
                        l.Severity,
                        Details = l.Details != null && l.Details.Length > 200 ? l.Details.Substring(0, 200) + "..." : l.Details
                    })
                    .ToList();

                return Ok(PagedResponse<object>.CreateSuccessResult(
                    pagedLogs, pageNumber, pageSize, totalCount, "Audit logs retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs");
                return StatusCode(500, PagedResponse<object>.CreateErrorResult("Lỗi khi lấy nhật ký audit"));
            }
        }

        /// <summary>
        /// Get audit log details
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuditLogDetails(int id)
        {
            try
            {
                var logs = await _auditLogService.GetLogsAsync();
                var log = logs.FirstOrDefault(l => l.Id == id);

                if (log == null)
                    return NotFound(new { success = false, message = "Không tìm thấy log" });

                return Ok(new { success = true, data = log });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit log {Id}", id);
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy chi tiết log" });
            }
        }

        /// <summary>
        /// Get audit statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetAuditStatistics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var logs = await _auditLogService.GetLogsAsync(fromDate, toDate);

                var stats = new
                {
                    TotalActions = logs.Count(),
                    
                    ByAction = logs.GroupBy(l => l.Action)
                        .Select(g => new { Action = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .ToList(),
                    
                    ByEntity = logs.GroupBy(l => l.EntityName)
                        .Select(g => new { EntityName = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .ToList(),
                    
                    ByUser = logs.Where(l => !string.IsNullOrEmpty(l.UserId))
                        .GroupBy(l => new { l.UserId, l.UserName })
                        .Select(g => new { g.Key.UserId, g.Key.UserName, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .Take(10)
                        .ToList(),
                    
                    BySeverity = logs.Where(l => !string.IsNullOrEmpty(l.Severity))
                        .GroupBy(l => l.Severity)
                        .Select(g => new { Severity = g.Key, Count = g.Count() })
                        .ToList(),
                    
                    RecentActions = logs.OrderByDescending(l => l.Timestamp)
                        .Take(20)
                        .Select(l => new
                        {
                            l.Timestamp,
                            l.EntityName,
                            l.Action,
                            l.UserName,
                            l.IpAddress
                        })
                        .ToList()
                };

                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating audit statistics");
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo thống kê audit" });
            }
        }

        /// <summary>
        /// Get user activity timeline
        /// </summary>
        [HttpGet("user/{userId}/timeline")]
        public async Task<IActionResult> GetUserTimeline(string userId, [FromQuery] int days = 7)
        {
            try
            {
                var fromDate = DateTime.Now.AddDays(-days);
                var logs = await _auditLogService.GetLogsAsync(fromDate, null, null, userId);

                var timeline = logs
                    .OrderByDescending(l => l.Timestamp)
                    .Select(l => new
                    {
                        l.Timestamp,
                        l.EntityName,
                        l.EntityId,
                        l.Action,
                        l.IpAddress,
                        Summary = $"{l.Action} {l.EntityName}" + (l.EntityId.HasValue ? $" #{l.EntityId}" : "")
                    })
                    .ToList();

                return Ok(new { success = true, data = timeline, userId, days });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user timeline for {UserId}", userId);
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy lịch sử hoạt động" });
            }
        }
    }
}

