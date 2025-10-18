using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Vehicle Inspection - Phiếu kiểm tra xe
    /// </summary>
    public class VehicleInspection : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string InspectionNumber { get; set; } = string.Empty;

        public int VehicleId { get; set; }
        public int CustomerId { get; set; }
        public int? InspectorId { get; set; } // Employee who performed inspection
        
        [StringLength(100)]
        public string? InspectorName { get; set; } // Cached inspector name

        [Required]
        public DateTime InspectionDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string InspectionType { get; set; } = "General"; // General, Diagnostic, Pre-service, Post-repair

        public int? CurrentMileage { get; set; }
        public int? Mileage { get; set; } // Alias for CurrentMileage

        [StringLength(20)]
        public string? FuelLevel { get; set; } // Empty, 1/4, 1/2, 3/4, Full

        // General findings
        [StringLength(2000)]
        public string? GeneralCondition { get; set; }

        [StringLength(1000)]
        public string? ExteriorCondition { get; set; }

        [StringLength(1000)]
        public string? InteriorCondition { get; set; }

        [StringLength(1000)]
        public string? EngineCondition { get; set; }

        [StringLength(1000)]
        public string? BrakeCondition { get; set; }

        [StringLength(1000)]
        public string? SuspensionCondition { get; set; }

        [StringLength(1000)]
        public string? TireCondition { get; set; }
        
        [StringLength(1000)]
        public string? ElectricalCondition { get; set; } // Tình trạng hệ thống điện
        
        [StringLength(1000)]
        public string? CoolingCondition { get; set; } // Tình trạng hệ thống làm mát
        
        [StringLength(1000)]
        public string? ExhaustCondition { get; set; } // Tình trạng hệ thống xả
        
        [StringLength(1000)]
        public string? BatteryCondition { get; set; } // Tình trạng bình ắc quy
        
        [StringLength(1000)]
        public string? LightsCondition { get; set; } // Tình trạng đèn
        
        [StringLength(2000)]
        public string? Findings { get; set; } // Summary of all findings

        // Customer complaints
        [StringLength(2000)]
        public string? CustomerComplaints { get; set; }

        // Recommendations
        [StringLength(2000)]
        public string? Recommendations { get; set; }

        [StringLength(2000)]
        public string? TechnicianNotes { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Cancelled

        // Reference to Customer Reception
        public int? CustomerReceptionId { get; set; }

        public DateTime? CompletedDate { get; set; }

        // If quotation created from this inspection
        public int? QuotationId { get; set; }
        
        // Cached customer/vehicle info for API
        [StringLength(200)]
        public string? CustomerName { get; set; }
        
        [StringLength(20)]
        public string? CustomerPhone { get; set; }
        
        [StringLength(20)]
        public string? VehiclePlate { get; set; }
        
        [StringLength(50)]
        public string? VehicleMake { get; set; }
        
        [StringLength(50)]
        public string? VehicleModel { get; set; }
        
        public int? VehicleYear { get; set; }
        
        [StringLength(1000)]
        public string? Notes { get; set; } // Additional notes

        // Navigation properties
        public virtual Vehicle Vehicle { get; set; } = null!;
        public virtual Customer Customer { get; set; } = null!;
        public virtual Employee? Inspector { get; set; }
        public virtual CustomerReception? CustomerReception { get; set; }
        public virtual ServiceQuotation? Quotation { get; set; }
        public virtual ICollection<InspectionIssue> Issues { get; set; } = new List<InspectionIssue>();
        public virtual ICollection<InspectionPhoto> Photos { get; set; } = new List<InspectionPhoto>();
    }
}

