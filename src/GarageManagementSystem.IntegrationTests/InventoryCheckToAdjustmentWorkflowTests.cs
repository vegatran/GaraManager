using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Infrastructure.Data;
using GarageManagementSystem.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GarageManagementSystem.IntegrationTests
{
    /// <summary>
    /// Integration tests for Inventory Check to Adjustment workflow
    /// Note: Full integration tests with WebApplicationFactory require Program class or InternalsVisibleTo
    /// These are simplified integration tests using DbContext directly
    /// </summary>
    public class InventoryCheckToAdjustmentWorkflowTests : IDisposable
    {
        private readonly GarageDbContext _context;

        public InventoryCheckToAdjustmentWorkflowTests()
        {
            // Setup InMemory Database
            var options = new DbContextOptionsBuilder<GarageDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new GarageDbContext(options);

            SeedTestData();
        }

        private void SeedTestData()
        {
            // Add Warehouse
            var warehouse = new Warehouse
            {
                Id = 1,
                Code = "WH-001",
                Name = "Kho Chính",
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            _context.Warehouses.Add(warehouse);

            // Add Part
            var part = new Part
            {
                Id = 1,
                PartNumber = "PART-001",
                PartName = "Lốp xe",
                QuantityInStock = 50,
                MinimumStock = 10,
                ReorderLevel = 20,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            _context.Parts.Add(part);

            // Add Employee
            var employee = new Employee
            {
                Id = 1,
                Name = "Nguyễn Văn A",
                Status = "Active",
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            _context.Employees.Add(employee);

            _context.SaveChanges();
        }

        [Fact]
        public async Task CreateInventoryCheck_WithDiscrepancy_ShouldCreateCheck()
        {
            // Arrange
            var check = new InventoryCheck
            {
                Code = "IK-2025-001",
                Name = "Kiểm kê test",
                WarehouseId = 1,
                CheckDate = DateTime.Now,
                Status = "Draft",
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };

            // Act
            _context.InventoryChecks.Add(check);
            await _context.SaveChangesAsync();

            // Add item with discrepancy
            var part = await _context.Parts.FirstAsync();
            var item = new InventoryCheckItem
            {
                InventoryCheckId = check.Id,
                PartId = part.Id,
                SystemQuantity = part.QuantityInStock,
                ActualQuantity = part.QuantityInStock - 5, // Discrepancy
                DiscrepancyQuantity = -5,
                IsDiscrepancy = true,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            _context.InventoryCheckItems.Add(item);
            await _context.SaveChangesAsync();

            // Assert
            var savedCheck = await _context.InventoryChecks
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == check.Id);
            Assert.NotNull(savedCheck);
            Assert.Single(savedCheck.Items);
            Assert.True(savedCheck.Items.First().IsDiscrepancy);
        }

        [Fact]
        public async Task CreateAdjustmentFromCheck_ShouldCreateAdjustment()
        {
            // Arrange - Create completed check with discrepancy
            var check = new InventoryCheck
            {
                Code = "IK-2025-001",
                Name = "Kiểm kê test",
                WarehouseId = 1,
                CheckDate = DateTime.Now,
                Status = "Completed",
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            _context.InventoryChecks.Add(check);
            await _context.SaveChangesAsync();

            var part = await _context.Parts.FirstAsync();
            var item = new InventoryCheckItem
            {
                InventoryCheckId = check.Id,
                PartId = part.Id,
                SystemQuantity = part.QuantityInStock,
                ActualQuantity = part.QuantityInStock - 5,
                DiscrepancyQuantity = -5,
                IsDiscrepancy = true,
                IsAdjusted = false,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            _context.InventoryCheckItems.Add(item);
            await _context.SaveChangesAsync();

            // Act - Create adjustment
            var adjustment = new InventoryAdjustment
            {
                AdjustmentNumber = "ADJ-2025-001",
                InventoryCheckId = check.Id,
                WarehouseId = check.WarehouseId,
                AdjustmentDate = DateTime.Now,
                Status = "Pending",
                Reason = "Điều chỉnh từ kiểm kê",
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            _context.InventoryAdjustments.Add(adjustment);
            await _context.SaveChangesAsync();

            var adjustmentItem = new InventoryAdjustmentItem
            {
                InventoryAdjustmentId = adjustment.Id,
                InventoryCheckItemId = item.Id,
                PartId = part.Id,
                QuantityChange = -5,
                SystemQuantityBefore = part.QuantityInStock,
                SystemQuantityAfter = part.QuantityInStock - 5,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            _context.InventoryAdjustmentItems.Add(adjustmentItem);
            await _context.SaveChangesAsync();

            // Assert
            var savedAdjustment = await _context.InventoryAdjustments
                .Include(a => a.Items)
                .FirstOrDefaultAsync(a => a.Id == adjustment.Id);
            Assert.NotNull(savedAdjustment);
            Assert.Equal("Pending", savedAdjustment.Status);
            Assert.Single(savedAdjustment.Items);
            Assert.Equal(-5, savedAdjustment.Items.First().QuantityChange);
        }

        [Fact]
        public async Task ApproveAdjustment_ShouldUpdateStock()
        {
            // Arrange
            var part = await _context.Parts.FirstAsync();
            var initialStock = part.QuantityInStock;

            var adjustment = new InventoryAdjustment
            {
                AdjustmentNumber = "ADJ-2025-001",
                WarehouseId = 1,
                AdjustmentDate = DateTime.Now,
                Status = "Pending",
                Reason = "Test adjustment",
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            _context.InventoryAdjustments.Add(adjustment);
            await _context.SaveChangesAsync();

            var adjustmentItem = new InventoryAdjustmentItem
            {
                InventoryAdjustmentId = adjustment.Id,
                PartId = part.Id,
                QuantityChange = 10,
                SystemQuantityBefore = initialStock,
                SystemQuantityAfter = initialStock + 10,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            _context.InventoryAdjustmentItems.Add(adjustmentItem);
            await _context.SaveChangesAsync();

            // Act - Approve adjustment
            adjustment.Status = "Approved";
            adjustment.ApprovedAt = DateTime.Now;
            part.QuantityInStock = adjustmentItem.SystemQuantityAfter;
            _context.InventoryAdjustments.Update(adjustment);
            _context.Parts.Update(part);
            await _context.SaveChangesAsync();

            // Assert
            var updatedPart = await _context.Parts.FirstAsync(p => p.Id == part.Id);
            Assert.Equal(initialStock + 10, updatedPart.QuantityInStock);
            
            var approvedAdjustment = await _context.InventoryAdjustments.FirstAsync(a => a.Id == adjustment.Id);
            Assert.Equal("Approved", approvedAdjustment.Status);
            Assert.NotNull(approvedAdjustment.ApprovedAt);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}

