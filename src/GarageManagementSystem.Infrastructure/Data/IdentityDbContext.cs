using GarageManagementSystem.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.Infrastructure.Data
{
    public class IdentityDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure ApplicationUser
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.Avatar).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
                entity.HasIndex(e => e.IsDeleted);
            });

            // Configure ApplicationRole
            builder.Entity<ApplicationRole>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
                entity.HasIndex(e => e.IsDeleted);
            });

            // Seed default roles
            builder.Entity<ApplicationRole>().HasData(
                new ApplicationRole
                {
                    Id = "1",
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    Description = "Administrator with full access",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                },
                new ApplicationRole
                {
                    Id = "2",
                    Name = "Manager",
                    NormalizedName = "MANAGER",
                    Description = "Manager with limited access",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                },
                new ApplicationRole
                {
                    Id = "3",
                    Name = "Employee",
                    NormalizedName = "EMPLOYEE",
                    Description = "Employee with basic access",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                }
            );

            // Seed default admin user
            var hasher = new PasswordHasher<ApplicationUser>();
            var adminUser = new ApplicationUser
            {
                Id = "1",
                UserName = "admin@gara.com",
                NormalizedUserName = "ADMIN@GARA.COM",
                Email = "admin@gara.com",
                NormalizedEmail = "ADMIN@GARA.COM",
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "User",
                IsActive = true,
                CreatedAt = DateTime.Now,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123");

            builder.Entity<ApplicationUser>().HasData(adminUser);

            // Assign admin role to admin user
            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    UserId = "1",
                    RoleId = "1"
                }
            );
        }
    }
}
