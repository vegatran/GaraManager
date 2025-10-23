using AutoMapper;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace GarageManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ServicesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ServiceDto>>>> GetServices()
        {
            try
            {
                var services = await _unitOfWork.Services.GetActiveServicesAsync();
                var serviceDtos = services.Select(MapToDto).ToList();
                
                return Ok(ApiResponse<List<ServiceDto>>.SuccessResult(serviceDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ServiceDto>>.ErrorResult("Lỗi khi lấy danh sách dịch vụ", ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ServiceDto>>> GetService(int id)
        {
            try
            {
                var service = await _unitOfWork.Services.GetByIdAsync(id);
                if (service == null)
                {
                    return NotFound(ApiResponse<ServiceDto>.ErrorResult("Không tìm thấy dịch vụ"));
                }

                var serviceDto = MapToDto(service);
                return Ok(ApiResponse<ServiceDto>.SuccessResult(serviceDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceDto>.ErrorResult("Lỗi khi lấy thông tin dịch vụ", ex.Message));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ServiceDto>>> CreateService(CreateServiceDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<ServiceDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
                }

                var service = new Core.Entities.Service
                {
                    Name = createDto.Name,
                    Description = createDto.Description,
                    Price = createDto.Price,
                    Duration = createDto.Duration,
                    Category = createDto.Category,
                    IsActive = createDto.IsActive
                };

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.Services.AddAsync(service);
                    await _unitOfWork.SaveChangesAsync();

                    // Commit transaction nếu thành công
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch
                {
                    // Rollback transaction nếu có lỗi
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }

                var serviceDto = MapToDto(service);
                return CreatedAtAction(nameof(GetService), new { id = service.Id }, 
                    ApiResponse<ServiceDto>.SuccessResult(serviceDto, "Tạo dịch vụ thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceDto>.ErrorResult("Lỗi khi tạo dịch vụ", ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ServiceDto>>> UpdateService(int id, UpdateServiceDto updateDto)
        {
            try
            {
                if (id != updateDto.Id)
                {
                    return BadRequest(ApiResponse<ServiceDto>.ErrorResult("ID không khớp"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<ServiceDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
                }

                var service = await _unitOfWork.Services.GetByIdAsync(id);
                if (service == null)
                {
                    return NotFound(ApiResponse<ServiceDto>.ErrorResult("Không tìm thấy dịch vụ"));
                }

                // ✅ SỬA: Dùng AutoMapper thay vì map tay
                _mapper.Map(updateDto, service);

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.Services.UpdateAsync(service);
                    await _unitOfWork.SaveChangesAsync();

                    // Commit transaction nếu thành công
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch
                {
                    // Rollback transaction nếu có lỗi
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }

                var serviceDto = MapToDto(service);
                return Ok(ApiResponse<ServiceDto>.SuccessResult(serviceDto, "Cập nhật dịch vụ thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceDto>.ErrorResult("Lỗi khi cập nhật dịch vụ", ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteService(int id)
        {
            try
            {
                var service = await _unitOfWork.Services.GetByIdAsync(id);
                if (service == null)
                {
                    return NotFound(ApiResponse.ErrorResult("Không tìm thấy dịch vụ"));
                }

                await _unitOfWork.Services.DeleteAsync(service);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse.SuccessResult("Xóa dịch vụ thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResult("Lỗi khi xóa dịch vụ", ex.Message));
            }
        }

        [HttpGet("category/{category}")]
        public async Task<ActionResult<ApiResponse<List<ServiceDto>>>> GetServicesByCategory(string category)
        {
            try
            {
                var services = await _unitOfWork.Services.GetServicesByCategoryAsync(category);
                var serviceDtos = services.Select(MapToDto).ToList();

                return Ok(ApiResponse<List<ServiceDto>>.SuccessResult(serviceDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ServiceDto>>.ErrorResult("Lỗi khi lấy dịch vụ theo danh mục", ex.Message));
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<List<ServiceDto>>>> SearchServices([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest(ApiResponse<List<ServiceDto>>.ErrorResult("Từ khóa tìm kiếm không được để trống"));
                }

                var services = await _unitOfWork.Services.SearchServicesAsync(searchTerm);
                var serviceDtos = services.Select(MapToDto).ToList();

                return Ok(ApiResponse<List<ServiceDto>>.SuccessResult(serviceDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ServiceDto>>.ErrorResult("Lỗi khi tìm kiếm dịch vụ", ex.Message));
            }
        }

        private ServiceDto MapToDto(Core.Entities.Service service)
        {
            return _mapper.Map<ServiceDto>(service);
        }
    }
}
