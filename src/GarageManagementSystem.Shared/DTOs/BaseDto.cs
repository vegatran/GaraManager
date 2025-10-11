namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// Base DTO with common properties and audit trail
    /// All DTOs should inherit from this
    /// </summary>
    public abstract class BaseDto
    {
        public int Id { get; set; }
        
        // Audit trail fields
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}

