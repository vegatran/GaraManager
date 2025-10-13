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
    public class AppointmentsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public AppointmentsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<AppointmentDto>>>> GetAllAppointments()
        {
            try
            {
                var appointments = await _unitOfWork.Appointments.GetAllWithDetailsAsync();
                return Ok(ApiResponse<List<AppointmentDto>>.SuccessResult(appointments.Select(MapToDto).ToList()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AppointmentDto>>.ErrorResult("Error", ex.Message));
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

                var appointment = new Core.Entities.Appointment
                {
                    AppointmentNumber = await _unitOfWork.Appointments.GenerateAppointmentNumberAsync(),
                    CustomerId = dto.CustomerId,
                    VehicleId = dto.VehicleId,
                    ScheduledDateTime = dto.ScheduledDateTime,
                    EstimatedDuration = dto.EstimatedDuration,
                    AppointmentType = dto.AppointmentType,
                    ServiceRequested = dto.ServiceRequested,
                    CustomerNotes = dto.CustomerNotes,
                    AssignedToId = dto.AssignedToId,
                    Status = "Scheduled"
                };

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

        private static AppointmentDto MapToDto(Core.Entities.Appointment a) => new()
        {
            Id = a.Id, AppointmentNumber = a.AppointmentNumber, CustomerId = a.CustomerId, VehicleId = a.VehicleId,
            ScheduledDateTime = a.ScheduledDateTime, EstimatedDuration = a.EstimatedDuration,
            AppointmentType = a.AppointmentType, ServiceRequested = a.ServiceRequested, CustomerNotes = a.CustomerNotes,
            Status = a.Status, ConfirmedDate = a.ConfirmedDate, ArrivalTime = a.ArrivalTime,
            AssignedToId = a.AssignedToId, ReminderSent = a.ReminderSent, CancellationReason = a.CancellationReason,
            Customer = a.Customer != null ? new CustomerDto { Id = a.Customer.Id, Name = a.Customer.Name } : null,
            Vehicle = a.Vehicle != null ? new VehicleDto { Id = a.Vehicle.Id, LicensePlate = a.Vehicle.LicensePlate } : null,
            CreatedAt = a.CreatedAt, CreatedBy = a.CreatedBy, UpdatedAt = a.UpdatedAt, UpdatedBy = a.UpdatedBy
        };
    }
}

