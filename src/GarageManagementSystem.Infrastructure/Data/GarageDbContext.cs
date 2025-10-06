using GarageManagementSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.Infrastructure.Data
{
    public class GarageDbContext : DbContext
    {
        public GarageDbContext(DbContextOptions<GarageDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceOrder> ServiceOrders { get; set; }
        public DbSet<ServiceOrderItem> ServiceOrderItems { get; set; }
        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Customer configuration
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(200);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.Gender).HasMaxLength(20);
                entity.HasIndex(e => e.Phone).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Vehicle configuration
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LicensePlate).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Brand).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Model).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Year).HasMaxLength(20);
                entity.Property(e => e.Color).HasMaxLength(50);
                entity.Property(e => e.VIN).HasMaxLength(17);
                entity.Property(e => e.EngineNumber).HasMaxLength(50);
                entity.HasIndex(e => e.LicensePlate).IsUnique();
                entity.HasIndex(e => e.VIN).IsUnique();

                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.Vehicles)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Service configuration
            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Category).HasMaxLength(50);
            });

            // ServiceOrder configuration
            modelBuilder.Entity<ServiceOrder>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.FinalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PaymentStatus).HasMaxLength(50);
                entity.HasIndex(e => e.OrderNumber).IsUnique();

                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.ServiceOrders)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Vehicle)
                    .WithMany(v => v.ServiceOrders)
                    .HasForeignKey(e => e.VehicleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ServiceOrderItem configuration
            modelBuilder.Entity<ServiceOrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasOne(e => e.ServiceOrder)
                    .WithMany(so => so.ServiceOrderItems)
                    .HasForeignKey(e => e.ServiceOrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Service)
                    .WithMany(s => s.ServiceOrderItems)
                    .HasForeignKey(e => e.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Employee configuration
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(200);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.Position).HasMaxLength(50);
                entity.Property(e => e.Department).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.Skills).HasMaxLength(1000);
                entity.Property(e => e.Salary).HasColumnType("decimal(18,2)");
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Services
            modelBuilder.Entity<Service>().HasData(
                new Service
                {
                    Id = 1,
                    Name = "Thay dầu động cơ",
                    Description = "Thay dầu động cơ và lọc dầu",
                    Price = 200000,
                    Duration = 30,
                    Category = "Bảo dưỡng",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Service
                {
                    Id = 2,
                    Name = "Kiểm tra phanh",
                    Description = "Kiểm tra và bảo dưỡng hệ thống phanh",
                    Price = 150000,
                    Duration = 45,
                    Category = "An toàn",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Service
                {
                    Id = 3,
                    Name = "Thay lốp",
                    Description = "Thay lốp xe và cân bằng",
                    Price = 300000,
                    Duration = 60,
                    Category = "Lốp xe",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Service
                {
                    Id = 4,
                    Name = "Sửa chữa động cơ",
                    Description = "Chẩn đoán và sửa chữa động cơ",
                    Price = 500000,
                    Duration = 120,
                    Category = "Sửa chữa",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }
            );

            // Seed Employees
            modelBuilder.Entity<Employee>().HasData(
                new Employee
                {
                    Id = 1,
                    Name = "Nguyễn Văn A",
                    Phone = "0123456789",
                    Email = "nguyenvana@garage.com",
                    Position = "Thợ sửa chữa",
                    Department = "Kỹ thuật",
                    HireDate = DateTime.Now.AddYears(-2),
                    Salary = 8000000,
                    Status = "Active",
                    Skills = "Sửa chữa động cơ, Thay dầu, Kiểm tra phanh",
                    CreatedAt = DateTime.Now
                },
                new Employee
                {
                    Id = 2,
                    Name = "Trần Thị B",
                    Phone = "0987654321",
                    Email = "tranthib@garage.com",
                    Position = "Thợ lốp",
                    Department = "Kỹ thuật",
                    HireDate = DateTime.Now.AddYears(-1),
                    Salary = 7000000,
                    Status = "Active",
                    Skills = "Thay lốp, Cân bằng, Sửa chữa lốp",
                    CreatedAt = DateTime.Now
                }
            );
        }
    }
}
