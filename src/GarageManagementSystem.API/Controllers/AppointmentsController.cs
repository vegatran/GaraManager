using AutoMapper;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Core.Extensions;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using GarageManagementSystem.API.Services;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiScope")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly GarageDbContext _context;

        public AppointmentsController(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService, GarageDbContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<AppointmentDto>>> GetAllAppointments(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? status = null,
            [FromQuery] int? customerId = null)
        {
            try
            {
                var appointments = await _unitOfWork.Appointments.GetAllWithDetailsAsync();
                var query = appointments.AsQueryable();
                
                // Apply search filter if provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(a => 
                        a.CustomerNotes.Contains(searchTerm));
                }
                
                // Apply status filter if provided
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(a => a.Status == status);
                }
                
                // Apply customer filter if provided
                if (customerId.HasValue)
                {
                    query = query.Where(a => a.CustomerId == customerId.Value);
                }

                query = query.OrderByDescending(a => a.ScheduledDateTime);

                // ✅ OPTIMIZED: Get paged results with total count - automatically chooses best method
                var (pagedAppointments, totalCount) = await query.ToPagedListWithCountAsync(pageNumber, pageSize, _context);
                var appointmentDtos = pagedAppointments.Select(MapToDto).ToList();
                
                return Ok(PagedResponse<AppointmentDto>.CreateSuccessResult(
                    appointmentDtos, pageNumber, pageSize, totalCount, "Appointments retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, PagedResponse<AppointmentDto>.CreateErrorResult("Error retrieving appointments"));
            }
        }

        [HttpGet("today")]
        public async Task<ActionResult<ApiResponse<List<AppointmentDto>>>> GetTodayAppointments()
        {
            try
            {
                var appointments = await _unitOfWork.Appointments.GetTodayAppointmentsAsync();
                return Ok(ApiResponse<List<AppointmentDto>>.SuccessResult(appointments.Select(MapToDto).ToList()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AppointmentDto>>.ErrorResult("Error", ex.Message));
            }
        }

        [HttpGet("upcoming")]
        public async Task<ActionResult<ApiResponse<List<AppointmentDto>>>> GetUpcoming()
        {
            try
            {
                var appointments = await _unitOfWork.Appointments.GetUpcomingAppointmentsAsync();
                return Ok(ApiResponse<List<AppointmentDto>>.SuccessResult(appointments.Select(MapToDto).ToList()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AppointmentDto>>.ErrorResult("Error", ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<AppointmentDto>>> GetAppointment(int id)
        {
            try
            {
                var appointment = await _unitOfWork.Appointments.GetByIdWithDetailsAsync(id);
                if (appointment == null) return NotFound(ApiResponse<AppointmentDto>.ErrorResult("Not found"));
                return Ok(ApiResponse<AppointmentDto>.SuccessResult(MapToDto(appointment)));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AppointmentDto>.ErrorResult("Error", ex.Message));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<AppointmentDto>>> CreateAppointment(CreateAppointmentDto dto)
        {
            try
            {
                // Check time slot availability
                var isAvailable = await _unitOfWork.Appointments.IsTimeSlotAvailableAsync(dto.ScheduledDateTime, dto.EstimatedDuration);
                if (!isAvailable) return BadRequest(ApiResponse<AppointmentDto>.ErrorResult("Time slot not available"));

                var appointment = _mapper.Map<Core.Entities.Appointment>(dto);
                appointment.AppointmentNumber = await _unitOfWork.Appointments.GenerateAppointmentNumberAsync();
                appointment.Status = "Scheduled";

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.Appointments.AddAsync(appointment);
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
                return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, ApiResponse<AppointmentDto>.SuccessResult(MapToDto(appointment)));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AppointmentDto>.ErrorResult("Error", ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<AppointmentDto>>> UpdateAppointment(int id, UpdateAppointmentDto dto)
        {
            try
            {
                if (id != dto.Id) return BadRequest(ApiResponse<AppointmentDto>.ErrorResult("ID mismatch"));
                var appointment = await _unitOfWork.Appointments.GetByIdAsync(id);
                if (appointment == null) return NotFound(ApiResponse<AppointmentDto>.ErrorResult("Not found"));

                if (dto.ScheduledDateTime.HasValue) appointment.ScheduledDateTime = dto.ScheduledDateTime.Value;
                if (dto.EstimatedDuration.HasValue) appointment.EstimatedDuration = dto.EstimatedDuration.Value;
                if (dto.ServiceRequested != null) appointment.ServiceRequested = dto.ServiceRequested;
                if (dto.CustomerNotes != null) appointment.CustomerNotes = dto.CustomerNotes;
                if (dto.Status != null) appointment.Status = dto.Status;
                if (dto.AssignedToId.HasValue) appointment.AssignedToId = dto.AssignedToId.Value;
                if (dto.CancellationReason != null) appointment.CancellationReason = dto.CancellationReason;

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.Appointments.UpdateAsync(appointment);
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
                return Ok(ApiResponse<AppointmentDto>.SuccessResult(MapToDto(appointment)));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AppointmentDto>.ErrorResult("Error", ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteAppointment(int id)
        {
            try
            {
                var appointment = await _unitOfWork.Appointments.GetByIdAsync(id);
                if (appointment == null) return NotFound(ApiResponse.ErrorResult("Not found"));
                await _unitOfWork.Appointments.DeleteAsync(appointment);
                await _unitOfWork.SaveChangesAsync();
                return Ok(ApiResponse.SuccessResult("Deleted"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResult("Error", ex.Message));
            }
        }

        [HttpGet("types")]
        public ActionResult<ApiResponse<List<object>>> GetAppointmentTypes()
        {
            try
            {
                var types = new[]
                {
                    new { id = "Service", text = "Bảo Dưỡng/Sửa Chữa" },
                    new { id = "Inspection", text = "Kiểm Tra" },
                    new { id = "Consultation", text = "Tư Vấn" },
                    new { id = "Pickup", text = "Nhận Xe" },
                    new { id = "Delivery", text = "Giao Xe" }
                }.ToList<object>();

                return Ok(ApiResponse<List<object>>.SuccessResult(types));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<object>>.ErrorResult("Error retrieving appointment types", ex.Message));
            }
        }

        private AppointmentDto MapToDto(Core.Entities.Appointment a)
        {
            return _mapper.Map<AppointmentDto>(a);
        }
    }
}

