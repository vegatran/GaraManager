using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Enums;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomerReceptionsController : ControllerBase
    {
        private readonly ICustomerReceptionRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerReceptionsController> _logger;

        public CustomerReceptionsController(
            ICustomerReceptionRepository repository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CustomerReceptionsController> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả phiếu tiếp đón
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<CustomerReceptionDto>>>> GetAll()
        {
            try
            {
                var receptions = await _repository.GetAllWithDetailsAsync();
                var receptionDtos = _mapper.Map<IEnumerable<CustomerReceptionDto>>(receptions);

                return Ok(new ApiResponse<IEnumerable<CustomerReceptionDto>>
                {
                    Success = true,
                    Data = receptionDtos,
                    Message = "Lấy danh sách phiếu tiếp đón thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phiếu tiếp đón");
                return StatusCode(500, new ApiResponse<IEnumerable<CustomerReceptionDto>>
                {
                    Success = false,
                    Message = "Lỗi khi lấy danh sách phiếu tiếp đón",
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy phiếu tiếp đón theo ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<CustomerReceptionDto>>> GetById(int id)
        {
            try
            {
                var reception = await _repository.GetByIdWithDetailsAsync(id);
                if (reception == null)
                {
                    return NotFound(new ApiResponse<CustomerReceptionDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy phiếu tiếp đón"
                    });
                }

                var receptionDto = _mapper.Map<CustomerReceptionDto>(reception);
                return Ok(new ApiResponse<CustomerReceptionDto>
                {
                    Success = true,
                    Data = receptionDto,
                    Message = "Lấy thông tin phiếu tiếp đón thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin phiếu tiếp đón {Id}", id);
                return StatusCode(500, new ApiResponse<CustomerReceptionDto>
                {
                    Success = false,
                    Message = "Lỗi khi lấy thông tin phiếu tiếp đón",
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy danh sách phiếu tiếp đón theo trạng thái
        /// </summary>
        [HttpGet("status/{status}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CustomerReceptionDto>>>> GetByStatus(string status)
        {
            try
            {
                // Parse string to enum
                if (!Enum.TryParse<ReceptionStatus>(status, true, out var receptionStatus))
                {
                    return BadRequest(new ApiResponse<IEnumerable<CustomerReceptionDto>>
                    {
                        Success = false,
                        Message = $"Trạng thái không hợp lệ: {status}"
                    });
                }
                
                var receptions = await _repository.GetByStatusAsync(receptionStatus);
                var receptionDtos = _mapper.Map<IEnumerable<CustomerReceptionDto>>(receptions);

                return Ok(new ApiResponse<IEnumerable<CustomerReceptionDto>>
                {
                    Success = true,
                    Data = receptionDtos,
                    Message = $"Lấy danh sách phiếu tiếp đón trạng thái {status} thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phiếu tiếp đón theo trạng thái {Status}", status);
                return StatusCode(500, new ApiResponse<IEnumerable<CustomerReceptionDto>>
                {
                    Success = false,
                    Message = "Lỗi khi lấy danh sách phiếu tiếp đón theo trạng thái",
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy danh sách phiếu tiếp đón chờ kiểm tra
        /// </summary>
        [HttpGet("pending")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CustomerReceptionDto>>>> GetPending()
        {
            try
            {
                var receptions = await _repository.GetPendingReceptionsAsync();
                var receptionDtos = _mapper.Map<IEnumerable<CustomerReceptionDto>>(receptions);

                return Ok(new ApiResponse<IEnumerable<CustomerReceptionDto>>
                {
                    Success = true,
                    Data = receptionDtos,
                    Message = "Lấy danh sách phiếu tiếp đón chờ kiểm tra thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phiếu tiếp đón chờ kiểm tra");
                return StatusCode(500, new ApiResponse<IEnumerable<CustomerReceptionDto>>
                {
                    Success = false,
                    Message = "Lỗi khi lấy danh sách phiếu tiếp đón chờ kiểm tra",
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Tạo phiếu tiếp đón mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<CustomerReceptionDto>>> Create([FromBody] CreateCustomerReceptionDto createDto)
        {
            try
            {
                // Kiểm tra xe có đang trong quy trình xử lý không
                var isVehicleInProcess = await _repository.IsVehicleInProcessAsync(createDto.VehicleId);
                if (isVehicleInProcess)
                {
                    return BadRequest(new ApiResponse<CustomerReceptionDto>
                    {
                        Success = false,
                        Message = "Xe này đang trong quy trình xử lý, không thể tạo phiếu tiếp đón mới"
                    });
                }

                var reception = _mapper.Map<CustomerReception>(createDto);
                
                // Tạo số phiếu tiếp đón
                reception.ReceptionNumber = await GenerateReceptionNumberAsync();

                // Cache thông tin khách hàng và xe
                await CacheCustomerVehicleInfoAsync(reception);

                await _repository.AddAsync(reception);
                await _unitOfWork.SaveChangesAsync();

                var receptionDto = _mapper.Map<CustomerReceptionDto>(reception);
                return CreatedAtAction(nameof(GetById), new { id = reception.Id }, new ApiResponse<CustomerReceptionDto>
                {
                    Success = true,
                    Data = receptionDto,
                    Message = "Tạo phiếu tiếp đón thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo phiếu tiếp đón");
                return StatusCode(500, new ApiResponse<CustomerReceptionDto>
                {
                    Success = false,
                    Message = "Lỗi khi tạo phiếu tiếp đón",
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Cập nhật phiếu tiếp đón
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<CustomerReceptionDto>>> Update(int id, [FromBody] UpdateCustomerReceptionDto updateDto)
        {
            try
            {
                var reception = await _repository.GetByIdAsync(id);
                if (reception == null)
                {
                    return NotFound(new ApiResponse<CustomerReceptionDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy phiếu tiếp đón"
                    });
                }

                _mapper.Map(updateDto, reception);
                await _repository.UpdateAsync(reception);
                await _unitOfWork.SaveChangesAsync();

                var receptionDto = _mapper.Map<CustomerReceptionDto>(reception);
                return Ok(new ApiResponse<CustomerReceptionDto>
                {
                    Success = true,
                    Data = receptionDto,
                    Message = "Cập nhật phiếu tiếp đón thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật phiếu tiếp đón {Id}", id);
                return StatusCode(500, new ApiResponse<CustomerReceptionDto>
                {
                    Success = false,
                    Message = "Lỗi khi cập nhật phiếu tiếp đón",
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Phân công kỹ thuật viên
        /// </summary>
        [HttpPut("{id:int}/assign")]
        public async Task<ActionResult<ApiResponse<bool>>> AssignTechnician(int id, [FromBody] AssignTechnicianRequest request)
        {
            try
            {
                var success = await _repository.AssignTechnicianAsync(id, request.TechnicianId);
                if (!success)
                {
                    return NotFound(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Không tìm thấy phiếu tiếp đón"
                    });
                }

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Phân công kỹ thuật viên thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi phân công kỹ thuật viên cho phiếu tiếp đón {Id}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Lỗi khi phân công kỹ thuật viên",
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Cập nhật trạng thái phiếu tiếp đón
        /// </summary>
        [HttpPut("{id:int}/status")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                // Parse string to enum
                if (!Enum.TryParse<ReceptionStatus>(request.Status, true, out var receptionStatus))
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = $"Trạng thái không hợp lệ: {request.Status}"
                    });
                }
                
                var success = await _repository.UpdateStatusAsync(id, receptionStatus);
                if (!success)
                {
                    return NotFound(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Không tìm thấy phiếu tiếp đón"
                    });
                }

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Cập nhật trạng thái thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái phiếu tiếp đón {Id}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Lỗi khi cập nhật trạng thái",
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Xóa phiếu tiếp đón
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            try
            {
                var reception = await _repository.GetByIdAsync(id);
                if (reception == null)
                {
                    return NotFound(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Không tìm thấy phiếu tiếp đón"
                    });
                }

                await _repository.DeleteAsync(reception);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Xóa phiếu tiếp đón thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa phiếu tiếp đón {Id}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Lỗi khi xóa phiếu tiếp đón",
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy thống kê trạng thái
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<ApiResponse<Dictionary<string, int>>>> GetStatistics()
        {
            try
            {
                var statistics = await _repository.GetStatusStatisticsAsync();
                
                // Convert enum keys to string for API response
                var stringStatistics = statistics.ToDictionary(
                    kvp => kvp.Key.ToString(), 
                    kvp => kvp.Value
                );
                
                return Ok(new ApiResponse<Dictionary<string, int>>
                {
                    Success = true,
                    Data = stringStatistics,
                    Message = "Lấy thống kê thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê phiếu tiếp đón");
                return StatusCode(500, new ApiResponse<Dictionary<string, int>>
                {
                    Success = false,
                    Message = "Lỗi khi lấy thống kê",
                    ErrorMessage = ex.Message
                });
            }
        }

        #region Private Methods

        private async Task<string> GenerateReceptionNumberAsync()
        {
            var today = DateTime.Now;
            var prefix = $"REC-{today:yyyyMMdd}";
            
            var lastReception = await _repository.GetAllAsync();
            var todayReceptions = lastReception
                .Where(r => r.ReceptionNumber.StartsWith(prefix))
                .OrderByDescending(r => r.ReceptionNumber)
                .FirstOrDefault();

            int nextNumber = 1;
            if (todayReceptions != null)
            {
                var lastNumber = todayReceptions.ReceptionNumber.Split('-').LastOrDefault();
                if (int.TryParse(lastNumber, out int parsedNumber))
                {
                    nextNumber = parsedNumber + 1;
                }
            }

            return $"{prefix}-{nextNumber:D4}";
        }

        private async Task CacheCustomerVehicleInfoAsync(CustomerReception reception)
        {
            // Cache thông tin khách hàng và xe để hiển thị nhanh
            // Implementation sẽ được thêm sau khi có repository cho Customer và Vehicle
        }

        #endregion
    }

    public class AssignTechnicianRequest
    {
        [Required]
        public int TechnicianId { get; set; }
    }

    public class UpdateStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
