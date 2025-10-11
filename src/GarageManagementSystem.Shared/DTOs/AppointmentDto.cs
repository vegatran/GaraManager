using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    public class AppointmentDto : BaseDto
    {
        public string AppointmentNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public int? VehicleId { get; set; }
        public DateTime ScheduledDateTime { get; set; }
        public int EstimatedDuration { get; set; }
        public string AppointmentType { get; set; } = string.Empty;
        public string? ServiceRequested { get; set; }
        public string? CustomerNotes { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ConfirmedDate { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }
        public int? AssignedToId { get; set; }
        public bool ReminderSent { get; set; }
        public DateTime? ReminderSentDate { get; set; }
        public string? CancellationReason { get; set; }
        public int? VehicleInspectionId { get; set; }
        public int? ServiceOrderId { get; set; }
        
        public CustomerDto? Customer { get; set; }
        public VehicleDto? Vehicle { get; set; }
        public EmployeeDto? AssignedTo { get; set; }
    }

    public class CreateAppointmentDto
    {
        [Required] public int CustomerId { get; set; }
        public int? VehicleId { get; set; }
        [Required] public DateTime ScheduledDateTime { get; set; }
        [Range(15, 480)] public int EstimatedDuration { get; set; } = 60;
        [Required] [StringLength(50)] public string AppointmentType { get; set; } = "Service";
        [StringLength(500)] public string? ServiceRequested { get; set; }
        [StringLength(1000)] public string? CustomerNotes { get; set; }
        public int? AssignedToId { get; set; }
    }

    public class UpdateAppointmentDto
    {
        [Required] public int Id { get; set; }
        public DateTime? ScheduledDateTime { get; set; }
        public int? EstimatedDuration { get; set; }
        [StringLength(500)] public string? ServiceRequested { get; set; }
        [StringLength(1000)] public string? CustomerNotes { get; set; }
        [StringLength(20)] public string? Status { get; set; }
        public int? AssignedToId { get; set; }
        [StringLength(500)] public string? CancellationReason { get; set; }
    }
}

