using AutoMapper;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarageManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiScope")]
    public class WarrantiesController : ControllerBase
    {
        private readonly IWarrantyService _warrantyService;
        private readonly IMapper _mapper;
        private readonly ILogger<WarrantiesController> _logger;

        public WarrantiesController(
            IWarrantyService warrantyService,
            IMapper mapper,
            ILogger<WarrantiesController> logger)
        {
            _warrantyService = warrantyService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost("service-orders/{serviceOrderId}/generate")]
        public async Task<ActionResult<ApiResponse<WarrantyDto>>> GenerateWarranty(
            int serviceOrderId,
            [FromBody] GenerateWarrantyRequestDto request)
        {
            try
            {
                request ??= new GenerateWarrantyRequestDto();

                var options = new WarrantyGenerationOptions
                {
                    WarrantyStartDate = request.WarrantyStartDate,
                    DefaultWarrantyMonths = request.DefaultWarrantyMonths ?? 3,
                    ForceRegenerate = request.ForceRegenerate,
                    GeneratedBy = User.Identity?.Name,
                    HandoverBy = request.HandoverBy,
                    HandoverLocation = request.HandoverLocation
                };

                var warranty = await _warrantyService.GenerateWarrantyForServiceOrderAsync(serviceOrderId, options);
                var warrantyDto = _mapper.Map<WarrantyDto>(warranty);

                return Ok(ApiResponse<WarrantyDto>.SuccessResult(warrantyDto, "Đã tạo bảo hành cho phiếu sửa chữa"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error while generating warranty for service order {ServiceOrderId}", serviceOrderId);
                return BadRequest(ApiResponse<WarrantyDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating warranty for service order {ServiceOrderId}", serviceOrderId);
                return StatusCode(500, ApiResponse<WarrantyDto>.ErrorResult("Lỗi khi tạo bảo hành cho phiếu sửa chữa", ex.Message));
            }
        }

        [HttpGet("service-order/{serviceOrderId}")]
        public async Task<ActionResult<ApiResponse<WarrantyDto>>> GetWarrantyByServiceOrder(int serviceOrderId)
        {
            try
            {
                var warranty = await _warrantyService.GetWarrantyByServiceOrderAsync(serviceOrderId);
                if (warranty == null)
                {
                    return NotFound(ApiResponse<WarrantyDto>.ErrorResult("Không tìm thấy bảo hành cho phiếu sửa chữa này"));
                }

                var dto = _mapper.Map<WarrantyDto>(warranty);
                return Ok(ApiResponse<WarrantyDto>.SuccessResult(dto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving warranty by service order {ServiceOrderId}", serviceOrderId);
                return StatusCode(500, ApiResponse<WarrantyDto>.ErrorResult("Lỗi khi lấy thông tin bảo hành", ex.Message));
            }
        }

        [HttpGet("{warrantyCode}")]
        public async Task<ActionResult<ApiResponse<WarrantyDto>>> GetByCode(string warrantyCode)
        {
            try
            {
                var warranty = await _warrantyService.GetWarrantyByCodeAsync(warrantyCode);
                if (warranty == null)
                {
                    return NotFound(ApiResponse<WarrantyDto>.ErrorResult("Không tìm thấy bảo hành tương ứng"));
                }

                var dto = _mapper.Map<WarrantyDto>(warranty);
                return Ok(ApiResponse<WarrantyDto>.SuccessResult(dto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving warranty by code {WarrantyCode}", warrantyCode);
                return StatusCode(500, ApiResponse<WarrantyDto>.ErrorResult("Lỗi khi lấy thông tin bảo hành", ex.Message));
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<WarrantySummaryDto>>>> Search([FromQuery] WarrantySearchFilterDto filterDto)
        {
            try
            {
                filterDto ??= new WarrantySearchFilterDto();
                var filter = new WarrantySearchFilter
                {
                    CustomerId = filterDto.CustomerId,
                    VehicleId = filterDto.VehicleId,
                    Status = filterDto.Status,
                    StartDate = filterDto.StartDate,
                    EndDate = filterDto.EndDate,
                    Keyword = filterDto.Keyword
                };

                var warranties = await _warrantyService.SearchWarrantiesAsync(filter);
                var summary = _mapper.Map<List<WarrantySummaryDto>>(warranties);

                return Ok(ApiResponse<List<WarrantySummaryDto>>.SuccessResult(summary));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching warranties");
                return StatusCode(500, ApiResponse<List<WarrantySummaryDto>>.ErrorResult("Lỗi khi tìm kiếm bảo hành", ex.Message));
            }
        }

        [HttpPost("{warrantyId}/claims")]
        public async Task<ActionResult<ApiResponse<WarrantyClaimDto>>> CreateClaim(int warrantyId, [FromBody] CreateWarrantyClaimDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<WarrantyClaimDto>.ErrorResult("Dữ liệu không hợp lệ"));
            }

            try
            {
                var claim = await _warrantyService.CreateWarrantyClaimAsync(warrantyId, new WarrantyClaimCreateRequest
                {
                    ClaimNumber = request.ClaimNumber,
                    ClaimDate = request.ClaimDate,
                    ServiceOrderId = request.ServiceOrderId,
                    CustomerId = request.CustomerId,
                    VehicleId = request.VehicleId,
                    IssueDescription = request.IssueDescription,
                    Notes = request.Notes
                });

                var dto = _mapper.Map<WarrantyClaimDto>(claim);
                return Ok(ApiResponse<WarrantyClaimDto>.SuccessResult(dto, "Đã tạo khiếu nại bảo hành"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<WarrantyClaimDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating warranty claim for warranty {WarrantyId}", warrantyId);
                return StatusCode(500, ApiResponse<WarrantyClaimDto>.ErrorResult("Lỗi khi tạo khiếu nại bảo hành", ex.Message));
            }
        }

        [HttpPut("claims/{claimId}")]
        public async Task<ActionResult<ApiResponse<WarrantyClaimDto>>> UpdateClaimStatus(int claimId, [FromBody] UpdateWarrantyClaimStatusDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<WarrantyClaimDto>.ErrorResult("Dữ liệu không hợp lệ"));
            }

            try
            {
                var claim = await _warrantyService.UpdateWarrantyClaimStatusAsync(claimId, new WarrantyClaimUpdateRequest
                {
                    Status = request.Status,
                    Resolution = request.Resolution,
                    ResolvedDate = request.ResolvedDate,
                    Notes = request.Notes
                });

                if (claim == null)
                {
                    return NotFound(ApiResponse<WarrantyClaimDto>.ErrorResult("Không tìm thấy đơn khiếu nại"));
                }

                var dto = _mapper.Map<WarrantyClaimDto>(claim);
                return Ok(ApiResponse<WarrantyClaimDto>.SuccessResult(dto, "Đã cập nhật khiếu nại bảo hành"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating warranty claim {ClaimId}", claimId);
                return StatusCode(500, ApiResponse<WarrantyClaimDto>.ErrorResult("Lỗi khi cập nhật khiếu nại bảo hành", ex.Message));
            }
        }
    }
}

