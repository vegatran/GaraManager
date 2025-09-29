using Microsoft.AspNetCore.Identity;

namespace GarageManagementSystem.IdentityServer.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName => $"{FirstName} {LastName}".Trim();
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Avatar { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastLoginAt { get; set; }
    }
}
