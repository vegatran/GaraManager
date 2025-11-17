using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GarageManagementSystem.API.Controllers;
using GarageManagementSystem.API.Services;
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
    public class PartsControllerTests : IDisposable
    {
        private readonly GarageDbContext _context;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly IMapper _mapper;
        private readonly PartsController _controller;

        public PartsControllerTests()
        {
            // Setup InMemory Database
            var options = new DbContextOptionsBuilder<GarageDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new GarageDbContext(options);

            // Setup AutoMapper
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<GarageManagementSystem.API.Profiles.PartProfile>();
            });
            _mapper = mapperConfig.CreateMapper();

            // Setup Mock UnitOfWork
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            // Setup Mock CacheService
            _cacheServiceMock = new Mock<ICacheService>();
            _cacheServiceMock.Setup(c => c.RemoveByPrefixAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Setup Controller
            _controller = new PartsController(_unitOfWorkMock.Object, _mapper, _cacheServiceMock.Object, _context);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Add Part
            var part = new Part
            {
                Id = 1,
                PartNumber = "PART-001",
                PartName = "Lốp xe",
                QuantityInStock = 50,
                MinimumStock = 10,
                ReorderLevel = 20,
                Sku = "SKU-001",
                Barcode = "1234567890123",
                DefaultUnit = "Cái",
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            _context.Parts.Add(part);

            _context.SaveChanges();
        }

        [Fact]
        public async Task GetParts_ShouldReturnPagedListOfParts()
        {
            // Act
            var result = await _controller.GetParts(pageNumber: 1, pageSize: 10);

            // Assert
            var okResult = Assert.IsType<ActionResult<PagedResponse<PartDto>>>(result);
            var apiResponse = Assert.IsType<OkObjectResult>(okResult.Result);
            var response = Assert.IsType<PagedResponse<PartDto>>(apiResponse.Value);
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            var dataList = response.Data.ToList();
            Assert.NotEmpty(dataList);
            Assert.Equal("PART-001", dataList[0].PartNumber);
        }

        [Fact]
        public async Task GetPart_WithValidId_ShouldReturnPart()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.Parts.GetWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => _context.Parts.FirstOrDefault(p => p.Id == id));

            // Act
            var result = await _controller.GetPart(1);

            // Assert
            var okResult = Assert.IsType<ActionResult<ApiResponse<PartDto>>>(result);
            var apiResponse = Assert.IsType<OkObjectResult>(okResult.Result);
            var response = Assert.IsType<ApiResponse<PartDto>>(apiResponse.Value);
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal("PART-001", response.Data!.PartNumber);
        }

        [Fact]
        public async Task GetPart_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.Parts.GetWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync((Part?)null);

            // Act
            var result = await _controller.GetPart(999);

            // Assert
            var notFoundResult = Assert.IsType<ActionResult<ApiResponse<PartDto>>>(result);
            var apiResponse = Assert.IsType<NotFoundObjectResult>(notFoundResult.Result);
            var response = Assert.IsType<ApiResponse<PartDto>>(apiResponse.Value);
            Assert.False(response.Success);
        }

        [Fact]
        public async Task GetParts_WithSearchTerm_ShouldFilterParts()
        {
            // Arrange
            var part2 = new Part
            {
                Id = 2,
                PartNumber = "PART-002",
                PartName = "Dầu máy",
                QuantityInStock = 30,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            _context.Parts.Add(part2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetParts(pageNumber: 1, pageSize: 10, searchTerm: "Lốp");

            // Assert
            var okResult = Assert.IsType<ActionResult<PagedResponse<PartDto>>>(result);
            var apiResponse = Assert.IsType<OkObjectResult>(okResult.Result);
            var response = Assert.IsType<PagedResponse<PartDto>>(apiResponse.Value);
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            // Should only return parts matching "Lốp"
            var dataList = response.Data.ToList();
            Assert.All(dataList, p => Assert.Contains("Lốp", p.PartName, StringComparison.OrdinalIgnoreCase));
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}

