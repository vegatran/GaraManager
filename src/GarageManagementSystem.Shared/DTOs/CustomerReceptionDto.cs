using System.ComponentModel.DataAnnotations;
using GarageManagementSystem.Core.Enums;

namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// DTO cho Customer Reception
    /// </summary>
    public class CustomerReceptionDto
    {
        public int Id { get; set; }
        public string ReceptionNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public int VehicleId { get; set; }
        public DateTime ReceptionDate { get; set; }
        public string? CustomerRequest { get; set; }
        public string? CustomerComplaints { get; set; }
        public string? ReceptionNotes { get; set; }
        public int? AssignedTechnicianId { get; set; }
        public ReceptionStatus Status { get; set; } = ReceptionStatus.Pending;
        public DateTime? AssignedDate { get; set; }
        public DateTime? InspectionStartDate { get; set; }
        public DateTime? InspectionCompletedDate { get; set; }
        public string Priority { get; set; } = "Normal";
        public string ServiceType { get; set; } = "General";
        public bool IsInsuranceClaim { get; set; } = false;
        public string? InsuranceCompany { get; set; }
        public string? InsurancePolicyNumber { get; set; }
        public string? EmergencyContact { get; set; }
        public string? EmergencyContactName { get; set; }

        // Cached information for display
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? VehiclePlate { get; set; }
        public string? VehicleMake { get; set; }
        public string? VehicleModel { get; set; }
        public int? VehicleYear { get; set; }

        // Navigation properties
        public CustomerDto? Customer { get; set; }
        public VehicleDto? Vehicle { get; set; }
        public EmployeeDto? AssignedTechnician { get; set; }
    }

    /// <summary>
    /// DTO cho tạo mới Customer Reception
    /// </summary>
    public class CreateCustomerReceptionDto
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        public string? CustomerRequest { get; set; }
        public string? CustomerComplaints { get; set; }
        public string? ReceptionNotes { get; set; }
        public int? AssignedTechnicianId { get; set; }
        public string Priority { get; set; } = "Normal";
        public string ServiceType { get; set; } = "General";
        public bool IsInsuranceClaim { get; set; } = false;
        public string? InsuranceCompany { get; set; }
        public string? InsurancePolicyNumber { get; set; }
        public string? EmergencyContact { get; set; }
        public string? EmergencyContactName { get; set; }
    }

    /// <summary>
    /// DTO cho cập nhật Customer Reception
    /// </summary>
    public class UpdateCustomerReceptionDto
    {
        public string? CustomerRequest { get; set; }
        public string? CustomerComplaints { get; set; }
        public string? ReceptionNotes { get; set; }
        public int? AssignedTechnicianId { get; set; }
        public ReceptionStatus Status { get; set; } = ReceptionStatus.Pending;
        public string Priority { get; set; } = "Normal";
        public string ServiceType { get; set; } = "General";
        public bool IsInsuranceClaim { get; set; } = false;
        public string? InsuranceCompany { get; set; }
        public string? InsurancePolicyNumber { get; set; }
        public string? EmergencyContact { get; set; }
        public string? EmergencyContactName { get; set; }
    }
}
