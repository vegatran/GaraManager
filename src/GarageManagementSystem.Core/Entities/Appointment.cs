using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Appointment - Lịch hẹn
    /// </summary>
    public class Appointment : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string AppointmentNumber { get; set; } = string.Empty; // APT-20241006-0001

        public int CustomerId { get; set; }
        public int? VehicleId { get; set; }

        [Required]
        public DateTime ScheduledDateTime { get; set; }

        public int EstimatedDuration { get; set; } = 60; // Thời gian dự kiến (phút)

        [Required]
        [StringLength(50)]
        public string AppointmentType { get; set; } = "Service"; // Inspection, Service, Pickup, Delivery

        [StringLength(500)]
        public string? ServiceRequested { get; set; } // Dịch vụ khách yêu cầu

        [StringLength(1000)]
        public string? CustomerNotes { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Confirmed, Arrived, InProgress, Completed, Cancelled, NoShow

        public DateTime? ConfirmedDate { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }

        public int? AssignedToId { get; set; } // Employee được phân công

        public bool ReminderSent { get; set; } = false;
        public DateTime? ReminderSentDate { get; set; }

        [StringLength(500)]
        public string? CancellationReason { get; set; }

        // Converted to actual records
        public int? VehicleInspectionId { get; set; }
        public int? ServiceOrderId { get; set; }

        // Navigation properties
        public virtual Customer Customer { get; set; } = null!;
        public virtual Vehicle? Vehicle { get; set; }
        public virtual Employee? AssignedTo { get; set; }
        public virtual VehicleInspection? VehicleInspection { get; set; }
        public virtual ServiceOrder? ServiceOrder { get; set; }
    }
}

