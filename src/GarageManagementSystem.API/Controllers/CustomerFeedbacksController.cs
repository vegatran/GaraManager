using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Enums;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiScope")]
    public class CustomerFeedbacksController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly GarageDbContext _context;
        private readonly ILogger<CustomerFeedbacksController> _logger;

        public CustomerFeedbacksController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            GarageDbContext context,
            ILogger<CustomerFeedbacksController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<CustomerFeedbackDto>>> GetFeedbacks([FromQuery] CustomerFeedbackFilterDto filter)
        {
            try
            {
                filter ??= new CustomerFeedbackFilterDto();

                var query = _context.CustomerFeedbacks
                    .AsNoTracking()
                    .Where(f => !f.IsDeleted)
                    .Include(f => f.Customer)
                    .Include(f => f.ServiceOrder)
                    .Include(f => f.FollowUpBy)
                    .Include(f => f.FeedbackChannel)
                    .Include(f => f.Attachments)
                    .AsQueryable();

                if (filter.CustomerId.HasValue)
                {
                    query = query.Where(f => f.CustomerId == filter.CustomerId.Value);
                }

                if (filter.ServiceOrderId.HasValue)
                {
                    query = query.Where(f => f.ServiceOrderId == filter.ServiceOrderId.Value);
                }

                if (!string.IsNullOrWhiteSpace(filter.Status))
                {
                    query = query.Where(f => f.Status == filter.Status);
                }

                if (!string.IsNullOrWhiteSpace(filter.Source))
                {
                    query = query.Where(f => f.Source == filter.Source);
                }

                if (!string.IsNullOrWhiteSpace(filter.Rating))
                {
                    query = query.Where(f => f.Rating == filter.Rating);
                }

                if (filter.StartDate.HasValue)
                {
                    query = query.Where(f => f.CreatedAt >= filter.StartDate.Value);
                }

                if (filter.EndDate.HasValue)
                {
                    var end = filter.EndDate.Value.Date.AddDays(1);
                    query = query.Where(f => f.CreatedAt < end);
                }

                if (!string.IsNullOrWhiteSpace(filter.Keyword))
                {
                    var keyword = filter.Keyword.Trim().ToLower();
                    query = query.Where(f =>
                        (f.Topic != null && f.Topic.ToLower().Contains(keyword)) ||
                        (f.Content.ToLower().Contains(keyword)) ||
                        (f.Notes != null && f.Notes.ToLower().Contains(keyword)));
                }

                query = query.OrderByDescending(f => f.CreatedAt);

                var pageNumber = Math.Max(filter.PageNumber, 1);
                var pageSize = Math.Clamp(filter.PageSize, 1, 100);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var dtoList = _mapper.Map<List<CustomerFeedbackDto>>(items);

                return Ok(PagedResponse<CustomerFeedbackDto>.CreateSuccessResult(
                    dtoList,
                    pageNumber,
                    pageSize,
                    totalCount,
                    "Lấy danh sách phản hồi thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer feedbacks");
                return StatusCode(500, PagedResponse<CustomerFeedbackDto>.CreateErrorResult("Lỗi khi lấy danh sách phản hồi"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CustomerFeedbackDto>>> GetFeedback(int id)
        {
            try
            {
                var feedback = await _context.CustomerFeedbacks
                    .Include(f => f.Customer)
                    .Include(f => f.ServiceOrder)
                    .Include(f => f.FollowUpBy)
                    .Include(f => f.FeedbackChannel)
                    .Include(f => f.Attachments)
                    .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);

                if (feedback == null)
                {
                    return NotFound(ApiResponse<CustomerFeedbackDto>.ErrorResult("Không tìm thấy phản hồi"));
                }

                var dto = _mapper.Map<CustomerFeedbackDto>(feedback);
                return Ok(ApiResponse<CustomerFeedbackDto>.SuccessResult(dto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting feedback {FeedbackId}", id);
                return StatusCode(500, ApiResponse<CustomerFeedbackDto>.ErrorResult("Lỗi khi lấy phản hồi", ex.Message));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CustomerFeedbackDto>>> CreateFeedback(CreateCustomerFeedbackDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<CustomerFeedbackDto>.ErrorResult("Dữ liệu không hợp lệ"));
            }

            try
            {
                var feedback = _mapper.Map<CustomerFeedback>(request);

                if (string.IsNullOrWhiteSpace(feedback.Source))
                {
                    feedback.Source = FeedbackSources.Other;
                }

                if (string.IsNullOrWhiteSpace(feedback.Status))
                {
                    feedback.Status = FeedbackStatuses.New;
                }

                if (feedback.CustomerId.HasValue)
                {
                    var customerExists = await _unitOfWork.Customers.ExistsAsync(c => c.Id == feedback.CustomerId.Value);
                    if (!customerExists)
                    {
                        return BadRequest(ApiResponse<CustomerFeedbackDto>.ErrorResult("Khách hàng không tồn tại"));
                    }
                }

                if (feedback.ServiceOrderId.HasValue)
                {
                    var orderExists = await _unitOfWork.ServiceOrders.ExistsAsync(o => o.Id == feedback.ServiceOrderId.Value);
                    if (!orderExists)
                    {
                        return BadRequest(ApiResponse<CustomerFeedbackDto>.ErrorResult("Phiếu sửa chữa không tồn tại"));
                    }
                }

                if (feedback.FollowUpById.HasValue)
                {
                    var employeeExists = await _unitOfWork.Employees.ExistsAsync(e => e.Id == feedback.FollowUpById.Value);
                    if (!employeeExists)
                    {
                        return BadRequest(ApiResponse<CustomerFeedbackDto>.ErrorResult("Nhân viên follow-up không tồn tại"));
                    }
                }

                if (feedback.FeedbackChannelId.HasValue)
                {
                    var channelExists = await _unitOfWork.FeedbackChannels.ExistsAsync(c => c.Id == feedback.FeedbackChannelId.Value && c.IsActive);
                    if (!channelExists)
                    {
                        return BadRequest(ApiResponse<CustomerFeedbackDto>.ErrorResult("Kênh phản hồi không tồn tại hoặc đã bị vô hiệu"));
                    }
                }

                await _unitOfWork.CustomerFeedbacks.AddAsync(feedback);
                await _unitOfWork.SaveChangesAsync();

                var created = await _context.CustomerFeedbacks
                    .Include(f => f.Customer)
                    .Include(f => f.ServiceOrder)
                    .Include(f => f.FollowUpBy)
                    .Include(f => f.FeedbackChannel)
                    .FirstAsync(f => f.Id == feedback.Id);

                var dto = _mapper.Map<CustomerFeedbackDto>(created);

                return CreatedAtAction(nameof(GetFeedback), new { id = dto.Id },
                    ApiResponse<CustomerFeedbackDto>.SuccessResult(dto, "Đã ghi nhận phản hồi khách hàng"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer feedback");
                return StatusCode(500, ApiResponse<CustomerFeedbackDto>.ErrorResult("Lỗi khi ghi nhận phản hồi", ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<CustomerFeedbackDto>>> UpdateFeedback(int id, UpdateCustomerFeedbackDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<CustomerFeedbackDto>.ErrorResult("Dữ liệu không hợp lệ"));
            }

            try
            {
                var feedback = await _context.CustomerFeedbacks.FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);
                if (feedback == null)
                {
                    return NotFound(ApiResponse<CustomerFeedbackDto>.ErrorResult("Không tìm thấy phản hồi"));
                }

                if (!string.IsNullOrWhiteSpace(request.Rating))
                {
                    feedback.Rating = request.Rating;
                }

                if (!string.IsNullOrWhiteSpace(request.Topic))
                {
                    feedback.Topic = request.Topic;
                }

                if (!string.IsNullOrWhiteSpace(request.Content))
                {
                    feedback.Content = request.Content;
                }

                if (request.ActionTaken != null)
                {
                    feedback.ActionTaken = request.ActionTaken;
                }

                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    feedback.Status = request.Status;
                }

                if (request.FollowUpDate.HasValue)
                {
                    feedback.FollowUpDate = request.FollowUpDate;
                }
                else if (request.FollowUpDate == null && feedback.FollowUpDate != null)
                {
                    feedback.FollowUpDate = null;
                }

                if (request.FollowUpById.HasValue)
                {
                    var employeeExists = await _unitOfWork.Employees.ExistsAsync(e => e.Id == request.FollowUpById.Value);
                    if (!employeeExists)
                    {
                        return BadRequest(ApiResponse<CustomerFeedbackDto>.ErrorResult("Nhân viên follow-up không tồn tại"));
                    }
                    feedback.FollowUpById = request.FollowUpById;
                }

                if (request.Notes != null)
                {
                    feedback.Notes = request.Notes;
                }

                if (request.Score.HasValue)
                {
                    feedback.Score = request.Score;
                }

                if (request.FeedbackChannelId.HasValue)
                {
                    var channelExists = await _unitOfWork.FeedbackChannels.ExistsAsync(c => c.Id == request.FeedbackChannelId.Value && c.IsActive);
                    if (!channelExists)
                    {
                        return BadRequest(ApiResponse<CustomerFeedbackDto>.ErrorResult("Kênh phản hồi không tồn tại hoặc đã bị vô hiệu"));
                    }
                    feedback.FeedbackChannelId = request.FeedbackChannelId;
                }

                await _unitOfWork.CustomerFeedbacks.UpdateAsync(feedback);
                await _unitOfWork.SaveChangesAsync();

                var updated = await _context.CustomerFeedbacks
                    .Include(f => f.Customer)
                    .Include(f => f.ServiceOrder)
                    .Include(f => f.FollowUpBy)
                    .Include(f => f.FeedbackChannel)
                    .Include(f => f.Attachments)
                    .FirstAsync(f => f.Id == feedback.Id);

                var dto = _mapper.Map<CustomerFeedbackDto>(updated);
                return Ok(ApiResponse<CustomerFeedbackDto>.SuccessResult(dto, "Đã cập nhật phản hồi khách hàng"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer feedback {FeedbackId}", id);
                return StatusCode(500, ApiResponse<CustomerFeedbackDto>.ErrorResult("Lỗi khi cập nhật phản hồi", ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteFeedback(int id)
        {
            try
            {
                var feedback = await _unitOfWork.CustomerFeedbacks.GetByIdAsync(id);
                if (feedback == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResult("Không tìm thấy phản hồi"));
                }

                await _unitOfWork.CustomerFeedbacks.DeleteAsync(feedback);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResult(true, "Đã xoá phản hồi"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer feedback {FeedbackId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Lỗi khi xoá phản hồi", ex.Message));
            }
        }
    }
}

