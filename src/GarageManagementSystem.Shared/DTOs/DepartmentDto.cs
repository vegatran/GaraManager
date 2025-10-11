namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// DTO for Department data transfer
    /// </summary>
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
