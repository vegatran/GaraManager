using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.IdentityServer.Models
{
    /// <summary>
    /// Entity để quản lý Claims động trong hệ thống
    /// </summary>
    public class Claim
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? DisplayName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsStandard { get; set; } = false; // Standard OIDC claims

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        [StringLength(100)]
        public string? Category { get; set; } // e.g., "Profile", "Contact", "Custom"
    }
}
