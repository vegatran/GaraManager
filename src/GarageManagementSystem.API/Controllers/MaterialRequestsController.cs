using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Enums;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using GarageManagementSystem.Core.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize(Policy = "ApiScope")]
    [ApiController]
    [Route("api/[controller]")]
    public class MaterialRequestsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<MaterialRequestsController> _logger;

        public MaterialRequestsController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<MaterialRequestsController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Danh sách MR có phân trang và filter
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResponse<MaterialRequestDto>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? serviceOrderId = null,
            [FromQuery] MaterialRequestStatus? status = null)
        {
            try
            {
                var all = await _unitOfWork.Repository<MaterialRequest>().GetAllAsync();
                var query = all.AsQueryable();

                if (serviceOrderId.HasValue)
                    query = query.Where(m => m.ServiceOrderId == serviceOrderId.Value);
                if (status.HasValue)
                    query = query.Where(m => m.Status == status.Value);

                query = query.OrderByDescending(m => m.CreatedAt);

                var totalCount = await query.GetTotalCountAsync();
                var data = query.ApplyPagination(pageNumber, pageSize).ToList();
                var dtos = _mapper.Map<List<MaterialRequestDto>>(data);

                return Ok(PagedResponse<MaterialRequestDto>.CreateSuccessResult(
                    dtos, pageNumber, pageSize, totalCount, "Lấy danh sách MR thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting MR paged list");
                return StatusCode(500, PagedResponse<MaterialRequestDto>.CreateErrorResult("Lỗi khi lấy danh sách MR"));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<MaterialRequestDto>>> Create([FromBody] CreateMaterialRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<MaterialRequestDto>.ErrorResult("Dữ liệu không hợp lệ"));

            var serviceOrder = await _unitOfWork.ServiceOrders.GetByIdAsync(dto.ServiceOrderId);
            if (serviceOrder == null) return NotFound(ApiResponse<MaterialRequestDto>.ErrorResult("Không tìm thấy phiếu sửa chữa"));

            // Generate MR number
            var count = await _unitOfWork.Repository<MaterialRequest>().CountAsync();
            var mrNumber = $"MR-{DateTime.Now:yyyyMMdd}-{(count + 1):D4}";

            var mr = _mapper.Map<MaterialRequest>(dto);
            mr.MRNumber = mrNumber;
            mr.Status = MaterialRequestStatus.Draft;
            mr.RequestedById = 0; // TODO: map from current user when identity ready

            await _unitOfWork.BeginTransactionAsync();
            await _unitOfWork.Repository<MaterialRequest>().AddAsync(mr);

            foreach (var itemDto in dto.Items)
            {
                var part = await _unitOfWork.Repository<Part>().GetByIdAsync(itemDto.PartId);
                if (part == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return BadRequest(ApiResponse<MaterialRequestDto>.ErrorResult($"Không tìm thấy phụ tùng ID {itemDto.PartId}"));
                }

                var item = _mapper.Map<MaterialRequestItem>(itemDto);
                item.MaterialRequest = mr;
                item.PartName = part.PartName;
                await _unitOfWork.Repository<MaterialRequestItem>().AddAsync(item);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            var result = _mapper.Map<MaterialRequestDto>(mr);
            return Ok(ApiResponse<MaterialRequestDto>.SuccessResult(result, "Tạo MR thành công"));
        }

        [HttpPut("{id}/submit")]
        public async Task<ActionResult<ApiResponse<object>>> Submit(int id)
        {
            var mr = await _unitOfWork.Repository<MaterialRequest>().GetByIdAsync(id);
            if (mr == null) return NotFound(ApiResponse<object>.ErrorResult("Không tìm thấy MR"));
            if (mr.Status != MaterialRequestStatus.Draft) return BadRequest(ApiResponse<object>.ErrorResult("Chỉ MR trạng thái Draft mới được gửi duyệt"));

            mr.Status = MaterialRequestStatus.PendingApproval;
            await _unitOfWork.Repository<MaterialRequest>().UpdateAsync(mr);
            await _unitOfWork.SaveChangesAsync();
            return Ok(ApiResponse<object>.SuccessResult(null, "Đã gửi MR chờ duyệt"));
        }

        [HttpPut("{id}/approve")]
        public async Task<ActionResult<ApiResponse<object>>> Approve(int id)
        {
            var mr = await _unitOfWork.Repository<MaterialRequest>().GetByIdAsync(id);
            if (mr == null) return NotFound(ApiResponse<object>.ErrorResult("Không tìm thấy MR"));
            if (mr.Status != MaterialRequestStatus.PendingApproval) return BadRequest(ApiResponse<object>.ErrorResult("Chỉ MR chờ duyệt mới được duyệt"));

            mr.Status = MaterialRequestStatus.Approved;
            mr.ApprovedAt = DateTime.Now;
            await _unitOfWork.Repository<MaterialRequest>().UpdateAsync(mr);
            await _unitOfWork.SaveChangesAsync();
            return Ok(ApiResponse<object>.SuccessResult(null, "Đã duyệt MR"));
        }

        [HttpPut("{id}/reject")]
        public async Task<ActionResult<ApiResponse<object>>> Reject(int id, [FromBody] ChangeMaterialRequestStatusDto dto)
        {
            var mr = await _unitOfWork.Repository<MaterialRequest>().GetByIdAsync(id);
            if (mr == null) return NotFound(ApiResponse<object>.ErrorResult("Không tìm thấy MR"));
            if (mr.Status != MaterialRequestStatus.PendingApproval) return BadRequest(ApiResponse<object>.ErrorResult("Chỉ MR chờ duyệt mới được từ chối"));

            mr.Status = MaterialRequestStatus.Rejected;
            mr.RejectReason = dto.Reason;
            await _unitOfWork.Repository<MaterialRequest>().UpdateAsync(mr);
            await _unitOfWork.SaveChangesAsync();
            return Ok(ApiResponse<object>.SuccessResult(null, "Đã từ chối MR"));
        }
    }
}


