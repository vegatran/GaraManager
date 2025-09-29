using Microsoft.AspNetCore.Identity;
using System;

namespace GarageManagementSystem.IdentityServer.Models
{
    public class ApplicationRole : IdentityRole
    {
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
    }
}
