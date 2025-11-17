using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GarageManagementSystem.API.Controllers;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace GarageManagementSystem.UnitTests.Controllers
{
    public class WarehousesControllerTests : IDisposable
    {
        private readonly GarageDbContext _context;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly IMapper _mapper;
        private readonly WarehousesController _controller;

        public WarehousesControllerTests()
        {
            // Setup InMemory Database
            var options = new DbContextOptionsBuilder<GarageDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new GarageDbContext(options);

            // Setup AutoMapper
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<GarageManagementSystem.API.Profiles.WarehouseProfile>();
            });
            _mapper = mapperConfig.CreateMapper();

            // Setup Mock UnitOfWork
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            // Setup Controller
            _controller = new WarehousesController(_unitOfWorkMock.Object, _mapper, _context);

            // Seed test data
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

            _context.SaveChanges();
        }

        [Fact]
        public async Task GetWarehouses_ShouldReturnListOfWarehouses()
        {
            // Act
            var result = await _controller.GetWarehouses();

            // Assert
            var okResult = Assert.IsType<ActionResult<ApiResponse<List<WarehouseDto>>>>(result);
            var apiResponse = Assert.IsType<OkObjectResult>(okResult.Result);
            var response = Assert.IsType<ApiResponse<List<WarehouseDto>>>(apiResponse.Value);
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.NotEmpty(response.Data);
            Assert.Equal("WH-001", response.Data[0].Code);
        }

        [Fact]
        public async Task GetWarehouse_WithValidId_ShouldReturnWarehouse()
        {
            // Act
            var result = await _controller.GetWarehouse(1);

            // Assert
            var okResult = Assert.IsType<ActionResult<ApiResponse<WarehouseDto>>>(result);
            var apiResponse = Assert.IsType<OkObjectResult>(okResult.Result);
            var response = Assert.IsType<ApiResponse<WarehouseDto>>(apiResponse.Value);
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal("WH-001", response.Data!.Code);
        }

        [Fact]
        public async Task GetWarehouse_WithInvalidId_ShouldReturnNotFound()
        {
            // Act
            var result = await _controller.GetWarehouse(999);

            // Assert
            var notFoundResult = Assert.IsType<ActionResult<ApiResponse<WarehouseDto>>>(result);
            var apiResponse = Assert.IsType<NotFoundObjectResult>(notFoundResult.Result);
            var response = Assert.IsType<ApiResponse<WarehouseDto>>(apiResponse.Value);
            Assert.False(response.Success);
        }

        [Fact]
        public async Task CreateWarehouse_WithValidData_ShouldCreateWarehouse()
        {
            // Arrange
            var dto = new CreateWarehouseDto
            {
                Code = "WH-002",
                Name = "Kho Phụ",
                Address = "123 Test Street",
                IsDefault = false,
                IsActive = true
            };

            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.Warehouses.AddAsync(It.IsAny<Warehouse>()))
                .ReturnsAsync((Warehouse w) => 
                {
                    if (w.Id == 0) w.Id = 2;
                    return w;
                });
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);
            _unitOfWorkMock.Setup(u => u.CommitTransactionAsync())
                .Returns(Task.CompletedTask);

            // Add created entity to context for reload
            var createdWarehouse = new Warehouse
            {
                Id = 2,
                Code = dto.Code,
                Name = dto.Name,
                Address = dto.Address,
                IsDefault = dto.IsDefault,
                IsActive = dto.IsActive,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            _context.Warehouses.Add(createdWarehouse);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.CreateWarehouse(dto);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<ApiResponse<WarehouseDto>>>(result);
            
            if (actionResult.Result is CreatedAtActionResult createdAtResult)
            {
                var response = Assert.IsType<ApiResponse<WarehouseDto>>(createdAtResult.Value);
                Assert.True(response.Success);
                Assert.NotNull(response.Data);
            }
        }

        [Fact]
        public async Task CreateWarehouse_WithDuplicateCode_ShouldReturnBadRequest()
        {
            // Arrange
            var dto = new CreateWarehouseDto
            {
                Code = "WH-001", // Duplicate code
                Name = "Kho Trùng",
                IsActive = true
            };

            // Act
            var result = await _controller.CreateWarehouse(dto);

            // Assert
            var badRequestResult = Assert.IsType<ActionResult<ApiResponse<WarehouseDto>>>(result);
            var apiResponse = Assert.IsType<BadRequestObjectResult>(badRequestResult.Result);
            var response = Assert.IsType<ApiResponse<WarehouseDto>>(apiResponse.Value);
            Assert.False(response.Success);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}

