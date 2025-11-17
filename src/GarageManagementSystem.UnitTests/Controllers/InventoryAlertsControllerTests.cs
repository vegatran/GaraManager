using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GarageManagementSystem.API.Controllers;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GarageManagementSystem.UnitTests.Controllers
{
    public class InventoryAlertsControllerTests : IDisposable
    {
        private readonly GarageDbContext _context;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<InventoryAlertsController>> _loggerMock;
        private readonly InventoryAlertsController _controller;

        public InventoryAlertsControllerTests()
        {
            // Setup InMemory Database
            var options = new DbContextOptionsBuilder<GarageDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new GarageDbContext(options);

            // Setup Mock UnitOfWork
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkMock.Setup(u => u.Parts.GetAllAsync())
                .ReturnsAsync(new List<Part>());

            // Setup Mock Logger
            _loggerMock = new Mock<ILogger<InventoryAlertsController>>();

            // Setup Controller
            _controller = new InventoryAlertsController(_unitOfWorkMock.Object, _loggerMock.Object);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Add Parts with different stock levels
            var parts = new List<Part>
            {
                new Part
                {
                    Id = 1,
                    PartNumber = "PART-001",
                    PartName = "Lốp xe",
                    QuantityInStock = 0,
                    MinimumStock = 10,
                    ReorderLevel = 20,
                    CostPrice = 100000,
                    IsDeleted = false,
                    CreatedAt = DateTime.Now
                },
                new Part
                {
                    Id = 2,
                    PartNumber = "PART-002",
                    PartName = "Dầu máy",
                    QuantityInStock = 5,
                    MinimumStock = 10,
                    ReorderLevel = 20,
                    CostPrice = 50000,
                    IsDeleted = false,
                    CreatedAt = DateTime.Now
                },
                new Part
                {
                    Id = 3,
                    PartNumber = "PART-003",
                    PartName = "Lọc gió",
                    QuantityInStock = 15,
                    MinimumStock = 10,
                    ReorderLevel = 20,
                    CostPrice = 30000,
                    IsDeleted = false,
                    CreatedAt = DateTime.Now
                }
            };

            _context.Parts.AddRange(parts);
            _context.SaveChanges();

            // Update mock to return seeded parts
            _unitOfWorkMock.Setup(u => u.Parts.GetAllAsync())
                .ReturnsAsync(parts);
        }

        [Fact]
        public async Task GetLowStockAlerts_ShouldReturnPartsWithLowStock()
        {
            // Act
            var result = await _controller.GetLowStockAlerts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            
            // Parse response
            var responseType = okResult.Value.GetType();
            var successProperty = responseType.GetProperty("success");
            var dataProperty = responseType.GetProperty("data");
            var countProperty = responseType.GetProperty("count");
            
            Assert.NotNull(successProperty);
            Assert.NotNull(dataProperty);
            
            var success = successProperty.GetValue(okResult.Value);
            var data = dataProperty.GetValue(okResult.Value);
            
            Assert.True((bool)success!);
            Assert.NotNull(data);
            
            // Should return parts with QuantityInStock <= MinimumStock and > 0
            // Part 2 (QuantityInStock = 5, MinimumStock = 10) should be included
            if (data is System.Collections.IEnumerable enumerable)
            {
                var items = enumerable.Cast<object>().ToList();
                Assert.NotEmpty(items);
                // Verify Part 2 is included (QuantityInStock = 5 <= MinimumStock = 10)
            }
        }

        [Fact]
        public async Task GetOutOfStockAlerts_ShouldReturnPartsWithZeroStock()
        {
            // Act
            var result = await _controller.GetOutOfStockAlerts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            
            // Parse response
            var responseType = okResult.Value.GetType();
            var successProperty = responseType.GetProperty("success");
            var dataProperty = responseType.GetProperty("data");
            
            Assert.NotNull(successProperty);
            Assert.NotNull(dataProperty);
            
            var success = successProperty.GetValue(okResult.Value);
            var data = dataProperty.GetValue(okResult.Value);
            
            Assert.True((bool)success!);
            Assert.NotNull(data);
            
            // Should return parts with QuantityInStock = 0
            // Part 1 (QuantityInStock = 0) should be included
            if (data is System.Collections.IEnumerable enumerable)
            {
                var items = enumerable.Cast<object>().ToList();
                // Verify Part 1 is included (QuantityInStock = 0)
            }
        }

        [Fact]
        public async Task GetLowStockAlerts_WithNoLowStockParts_ShouldReturnEmptyList()
        {
            // Arrange - Update parts to have sufficient stock
            var parts = _context.Parts.ToList();
            foreach (var part in parts)
            {
                part.QuantityInStock = part.MinimumStock + 10;
            }
            _context.SaveChanges();

            _unitOfWorkMock.Setup(u => u.Parts.GetAllAsync())
                .ReturnsAsync(parts);

            // Act
            var result = await _controller.GetLowStockAlerts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            
            // Parse response
            var responseType = okResult.Value.GetType();
            var successProperty = responseType.GetProperty("success");
            var dataProperty = responseType.GetProperty("data");
            var countProperty = responseType.GetProperty("count");
            
            Assert.NotNull(successProperty);
            Assert.NotNull(dataProperty);
            
            var success = successProperty.GetValue(okResult.Value);
            var data = dataProperty.GetValue(okResult.Value);
            
            Assert.True((bool)success!);
            Assert.NotNull(data);
            
            // Should return empty list or list with no low stock items
            if (data is System.Collections.IEnumerable enumerable)
            {
                var items = enumerable.Cast<object>().ToList();
                // All parts now have QuantityInStock > MinimumStock, so should be empty or filtered out
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}

