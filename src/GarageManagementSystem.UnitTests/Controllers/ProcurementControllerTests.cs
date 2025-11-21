using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GarageManagementSystem.API.Controllers;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GarageManagementSystem.UnitTests.Controllers
{
    /// <summary>
    /// Unit tests for ProcurementController - Request Quotation endpoints
    /// Phase 4.2.2 Optional: Request Quotation
    /// </summary>
    public class ProcurementControllerTests : IDisposable
    {
        private readonly GarageDbContext _context;
        private readonly Mock<ILogger<ProcurementController>> _loggerMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly ProcurementController _controller;

        public ProcurementControllerTests()
        {
            // Setup InMemory Database
            var options = new DbContextOptionsBuilder<GarageDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new GarageDbContext(options);

            // Setup Mock Logger
            _loggerMock = new Mock<ILogger<ProcurementController>>();

            // Setup Mock UnitOfWork
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            // Setup Controller
            _controller = new ProcurementController(_unitOfWorkMock.Object, _loggerMock.Object, _context);

            // Setup User Claims (for RequestedById)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "10")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Add Part
            var part = new Part
            {
                Id = 1,
                PartNumber = "PT-001",
                PartName = "Phụ Tùng Test 001",
                QuantityInStock = 50,
                MinimumStock = 10,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            _context.Parts.Add(part);

            // Add Suppliers
            var supplier1 = new Supplier
            {
                Id = 1,
                SupplierName = "Nhà Cung Cấp A",
                SupplierCode = "NCC-A",
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            var supplier2 = new Supplier
            {
                Id = 2,
                SupplierName = "Nhà Cung Cấp B",
                SupplierCode = "NCC-B",
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            _context.Suppliers.AddRange(supplier1, supplier2);

            // Add Employee (for RequestedBy)
            var employee = new Employee
            {
                Id = 10,
                Name = "Nguyễn Văn A",
                Email = "test@example.com",
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            _context.Employees.Add(employee);

            _context.SaveChanges();
        }

        #region Request Quotation Tests

        [Fact]
        public async Task RequestQuotation_WithValidData_ShouldCreateQuotations()
        {
            // Arrange
            var requestDto = new RequestQuotationDto
            {
                PartId = 1,
                SupplierIds = new List<int> { 1, 2 },
                RequestedQuantity = 50,
                RequestNotes = "Test quotation request"
            };

            // Act
            var result = await _controller.RequestQuotation(requestDto);

            // Assert
            var okResult = Assert.IsType<ActionResult<ApiResponse<RequestQuotationResponseDto>>>(result);
            var apiResponse = Assert.IsType<ObjectResult>(okResult.Result);
            Assert.Equal(200, apiResponse.StatusCode);
            var response = Assert.IsType<ApiResponse<RequestQuotationResponseDto>>(apiResponse.Value);
            
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Quotations);
            Assert.Equal(2, response.Data.RequestedCount);
            Assert.Equal(2, response.Data.Quotations.Count);
            
            // Verify quotations were created in database
            var quotations = await _context.Set<SupplierQuotation>()
                .Where(sq => !sq.IsDeleted && sq.PartId == 1)
                .ToListAsync();
            Assert.Equal(2, quotations.Count);
            Assert.All(quotations, q => 
            {
                Assert.Equal("Requested", q.Status);
                Assert.Equal(50, q.RequestedQuantity);
                Assert.Equal(10, q.RequestedById);
                Assert.NotNull(q.QuotationNumber);
                Assert.StartsWith("RQ-", q.QuotationNumber);
            });
        }

        [Fact]
        public async Task RequestQuotation_WithNullRequest_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.RequestQuotation(null!);

            // Assert
            var badRequestResult = Assert.IsType<ActionResult<ApiResponse<RequestQuotationResponseDto>>>(result);
            var apiResponse = Assert.IsType<ObjectResult>(badRequestResult.Result);
            Assert.Equal(400, apiResponse.StatusCode);
            var response = Assert.IsType<ApiResponse<RequestQuotationResponseDto>>(apiResponse.Value);
            Assert.False(response.Success);
            Assert.NotNull(response.Message);
            Assert.Contains("Request data is required", response.Message);
        }

        [Fact]
        public async Task RequestQuotation_WithInvalidPartId_ShouldReturnBadRequest()
        {
            // Arrange
            var requestDto = new RequestQuotationDto
            {
                PartId = 0,
                SupplierIds = new List<int> { 1 },
                RequestedQuantity = 50
            };

            // Act
            var result = await _controller.RequestQuotation(requestDto);

            // Assert
            var badRequestResult = Assert.IsType<ActionResult<ApiResponse<RequestQuotationResponseDto>>>(result);
            var apiResponse = Assert.IsType<ObjectResult>(badRequestResult.Result);
            Assert.Equal(400, apiResponse.StatusCode);
            var response = Assert.IsType<ApiResponse<RequestQuotationResponseDto>>(apiResponse.Value);
            Assert.False(response.Success);
            Assert.NotNull(response.Message);
            Assert.Contains("Part ID is required", response.Message);
        }

        [Fact]
        public async Task RequestQuotation_WithEmptySupplierIds_ShouldReturnBadRequest()
        {
            // Arrange
            var requestDto = new RequestQuotationDto
            {
                PartId = 1,
                SupplierIds = new List<int>(),
                RequestedQuantity = 50
            };

            // Act
            var result = await _controller.RequestQuotation(requestDto);

            // Assert
            var badRequestResult = Assert.IsType<ActionResult<ApiResponse<RequestQuotationResponseDto>>>(result);
            var apiResponse = Assert.IsType<ObjectResult>(badRequestResult.Result);
            Assert.Equal(400, apiResponse.StatusCode);
            var response = Assert.IsType<ApiResponse<RequestQuotationResponseDto>>(apiResponse.Value);
            Assert.False(response.Success);
            Assert.NotNull(response.Message);
            Assert.Contains("At least one supplier is required", response.Message);
        }

        [Fact]
        public async Task RequestQuotation_WithInvalidQuantity_ShouldReturnBadRequest()
        {
            // Arrange
            var requestDto = new RequestQuotationDto
            {
                PartId = 1,
                SupplierIds = new List<int> { 1 },
                RequestedQuantity = 0
            };

            // Act
            var result = await _controller.RequestQuotation(requestDto);

            // Assert
            var badRequestResult = Assert.IsType<ActionResult<ApiResponse<RequestQuotationResponseDto>>>(result);
            var apiResponse = Assert.IsType<ObjectResult>(badRequestResult.Result);
            Assert.Equal(400, apiResponse.StatusCode);
            var response = Assert.IsType<ApiResponse<RequestQuotationResponseDto>>(apiResponse.Value);
            Assert.False(response.Success);
            Assert.NotNull(response.Message);
            Assert.Contains("Requested quantity must be greater than 0", response.Message);
        }

        [Fact]
        public async Task RequestQuotation_WithNonExistentPart_ShouldReturnNotFound()
        {
            // Arrange
            var requestDto = new RequestQuotationDto
            {
                PartId = 999,
                SupplierIds = new List<int> { 1 },
                RequestedQuantity = 50
            };

            // Act
            var result = await _controller.RequestQuotation(requestDto);

            // Assert
            var notFoundResult = Assert.IsType<ActionResult<ApiResponse<RequestQuotationResponseDto>>>(result);
            var apiResponse = Assert.IsType<ObjectResult>(notFoundResult.Result);
            Assert.Equal(404, apiResponse.StatusCode);
            var response = Assert.IsType<ApiResponse<RequestQuotationResponseDto>>(apiResponse.Value);
            Assert.False(response.Success);
            Assert.NotNull(response.Message);
            Assert.Contains("Part not found", response.Message);
        }

        [Fact]
        public async Task RequestQuotation_WithNonExistentSupplier_ShouldReturnBadRequest()
        {
            // Arrange
            var requestDto = new RequestQuotationDto
            {
                PartId = 1,
                SupplierIds = new List<int> { 999 },
                RequestedQuantity = 50
            };

            // Act
            var result = await _controller.RequestQuotation(requestDto);

            // Assert
            var badRequestResult = Assert.IsType<ActionResult<ApiResponse<RequestQuotationResponseDto>>>(result);
            var apiResponse = Assert.IsType<ObjectResult>(badRequestResult.Result);
            Assert.Equal(400, apiResponse.StatusCode);
            var response = Assert.IsType<ApiResponse<RequestQuotationResponseDto>>(apiResponse.Value);
            Assert.False(response.Success);
            Assert.NotNull(response.Message);
            Assert.Contains("One or more suppliers not found", response.Message);
        }

        [Fact]
        public async Task RequestQuotation_WithDuplicateRequest_ShouldSkipDuplicate()
        {
            // Arrange - Create existing quotation
            var existingQuotation = new SupplierQuotation
            {
                PartId = 1,
                SupplierId = 1,
                Status = "Requested",
                QuotationNumber = "RQ-2025-00001",
                RequestedQuantity = 30,
                RequestedDate = DateTime.Now,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            _context.Set<SupplierQuotation>().Add(existingQuotation);
            await _context.SaveChangesAsync();

            var requestDto = new RequestQuotationDto
            {
                PartId = 1,
                SupplierIds = new List<int> { 1, 2 }, // Supplier 1 already has quotation
                RequestedQuantity = 50
            };

            // Act
            var result = await _controller.RequestQuotation(requestDto);

            // Assert
            var okResult = Assert.IsType<ActionResult<ApiResponse<RequestQuotationResponseDto>>>(result);
            var apiResponse = Assert.IsType<ObjectResult>(okResult.Result);
            Assert.Equal(200, apiResponse.StatusCode);
            var response = Assert.IsType<ApiResponse<RequestQuotationResponseDto>>(apiResponse.Value);
            
            // Should only create 1 new quotation (for supplier 2)
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal(1, response.Data.RequestedCount);
        }

        [Fact]
        public async Task RequestQuotation_WithDuplicateIds_ShouldRemoveDuplicates()
        {
            // Arrange
            var requestDto = new RequestQuotationDto
            {
                PartId = 1,
                SupplierIds = new List<int> { 1, 1, 2, 2 }, // Duplicates
                RequestedQuantity = 50
            };

            // Act
            var result = await _controller.RequestQuotation(requestDto);

            // Assert
            var okResult = Assert.IsType<ActionResult<ApiResponse<RequestQuotationResponseDto>>>(result);
            var apiResponse = Assert.IsType<ObjectResult>(okResult.Result);
            Assert.Equal(200, apiResponse.StatusCode);
            var response = Assert.IsType<ApiResponse<RequestQuotationResponseDto>>(apiResponse.Value);
            
            // Should create only 2 quotations (duplicates removed)
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal(2, response.Data.RequestedCount);
        }

        #endregion

        #region Get Quotations Tests

        [Fact]
        public async Task GetQuotations_ShouldReturnPagedList()
        {
            // Arrange - Create test quotations
            var quotations = new List<SupplierQuotation>
            {
                new SupplierQuotation
                {
                    PartId = 1,
                    SupplierId = 1,
                    Status = "Requested",
                    QuotationNumber = "RQ-2025-00001",
                    RequestedQuantity = 50,
                    RequestedDate = DateTime.Now,
                    IsDeleted = false,
                    CreatedAt = DateTime.Now
                },
                new SupplierQuotation
                {
                    PartId = 1,
                    SupplierId = 2,
                    Status = "Pending",
                    QuotationNumber = "RQ-2025-00002",
                    RequestedQuantity = 30,
                    RequestedDate = DateTime.Now,
                    IsDeleted = false,
                    CreatedAt = DateTime.Now
                }
            };
            _context.Set<SupplierQuotation>().AddRange(quotations);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetQuotations();

            // Assert
            var okResult = Assert.IsType<ActionResult<PagedResponse<SupplierQuotationDto>>>(result);
            var apiResponse = Assert.IsType<OkObjectResult>(okResult.Result);
            var response = Assert.IsType<PagedResponse<SupplierQuotationDto>>(apiResponse.Value);
            
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            var dataList = response.Data.ToList();
            Assert.Equal(2, dataList.Count);
        }

        [Fact]
        public async Task GetQuotations_WithPartIdFilter_ShouldFilterCorrectly()
        {
            // Arrange - Create quotations for different parts
            var part2 = new Part
            {
                Id = 2,
                PartNumber = "PT-002",
                PartName = "Phụ Tùng Test 002",
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            _context.Parts.Add(part2);
            await _context.SaveChangesAsync();

            var quotations = new List<SupplierQuotation>
            {
                new SupplierQuotation { PartId = 1, SupplierId = 1, Status = "Requested", QuotationNumber = "RQ-2025-00001", IsDeleted = false, CreatedAt = DateTime.Now },
                new SupplierQuotation { PartId = 2, SupplierId = 1, Status = "Requested", QuotationNumber = "RQ-2025-00002", IsDeleted = false, CreatedAt = DateTime.Now }
            };
            _context.Set<SupplierQuotation>().AddRange(quotations);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetQuotations(partId: 1);

            // Assert
            var okResult = Assert.IsType<ActionResult<PagedResponse<SupplierQuotationDto>>>(result);
            var apiResponse = Assert.IsType<OkObjectResult>(okResult.Result);
            var response = Assert.IsType<PagedResponse<SupplierQuotationDto>>(apiResponse.Value);
            
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            var dataList = response.Data.ToList();
            Assert.Single(dataList);
            Assert.All(dataList, dto => Assert.Equal(1, dto.PartId));
        }

        [Fact]
        public async Task GetQuotations_WithStatusFilter_ShouldFilterCorrectly()
        {
            // Arrange
            var quotations = new List<SupplierQuotation>
            {
                new SupplierQuotation { PartId = 1, SupplierId = 1, Status = "Requested", QuotationNumber = "RQ-2025-00001", IsDeleted = false, CreatedAt = DateTime.Now },
                new SupplierQuotation { PartId = 1, SupplierId = 2, Status = "Pending", QuotationNumber = "RQ-2025-00002", IsDeleted = false, CreatedAt = DateTime.Now }
            };
            _context.Set<SupplierQuotation>().AddRange(quotations);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetQuotations(status: "Pending");

            // Assert
            var okResult = Assert.IsType<ActionResult<PagedResponse<SupplierQuotationDto>>>(result);
            var apiResponse = Assert.IsType<OkObjectResult>(okResult.Result);
            var response = Assert.IsType<PagedResponse<SupplierQuotationDto>>(apiResponse.Value);
            
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            var dataList = response.Data.ToList();
            Assert.Single(dataList);
            Assert.All(dataList, dto => Assert.Equal("Pending", dto.Status));
        }

        [Fact]
        public async Task GetQuotations_WithPagination_ShouldReturnCorrectPage()
        {
            // Arrange - Create 25 quotations
            var quotations = Enumerable.Range(1, 25).Select(i => new SupplierQuotation
            {
                PartId = 1,
                SupplierId = 1,
                Status = "Requested",
                QuotationNumber = $"RQ-2025-{i:D5}",
                IsDeleted = false,
                CreatedAt = DateTime.Now.AddMinutes(-i)
            }).ToList();
            _context.Set<SupplierQuotation>().AddRange(quotations);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetQuotations(pageNumber: 2, pageSize: 10);

            // Assert
            var okResult = Assert.IsType<ActionResult<PagedResponse<SupplierQuotationDto>>>(result);
            var apiResponse = Assert.IsType<OkObjectResult>(okResult.Result);
            var response = Assert.IsType<PagedResponse<SupplierQuotationDto>>(apiResponse.Value);
            
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            var dataList = response.Data.ToList();
            Assert.Equal(10, dataList.Count);
            Assert.Equal(2, response.PageNumber);
            Assert.Equal(25, response.TotalCount);
        }

        #endregion

        #region Get Quotation By ID Tests

        [Fact]
        public async Task GetQuotationById_WithValidId_ShouldReturnQuotation()
        {
            // Arrange
            var quotation = new SupplierQuotation
            {
                PartId = 1,
                SupplierId = 1,
                Status = "Pending",
                QuotationNumber = "RQ-2025-00001",
                UnitPrice = 50000,
                RequestedQuantity = 50,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            _context.Set<SupplierQuotation>().Add(quotation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetQuotationById(quotation.Id);

            // Assert
            var okResult = Assert.IsType<ActionResult<ApiResponse<SupplierQuotationDto>>>(result);
            var apiResponse = Assert.IsType<OkObjectResult>(okResult.Result);
            var response = Assert.IsType<ApiResponse<SupplierQuotationDto>>(apiResponse.Value);
            
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal(quotation.Id, response.Data.Id);
            Assert.Equal("RQ-2025-00001", response.Data.QuotationNumber);
            Assert.Equal(50000, response.Data.UnitPrice);
        }

        [Fact]
        public async Task GetQuotationById_WithInvalidId_ShouldReturnNotFound()
        {
            // Act
            var result = await _controller.GetQuotationById(999);

            // Assert
            var notFoundResult = Assert.IsType<ActionResult<ApiResponse<SupplierQuotationDto>>>(result);
            var apiResponse = Assert.IsType<ObjectResult>(notFoundResult.Result);
            Assert.Equal(404, apiResponse.StatusCode);
            var response = Assert.IsType<ApiResponse<SupplierQuotationDto>>(apiResponse.Value);
            Assert.False(response.Success);
            Assert.NotNull(response.Message);
            Assert.Contains("Quotation not found", response.Message);
        }

        [Fact]
        public async Task GetQuotationById_WithDeletedQuotation_ShouldReturnNotFound()
        {
            // Arrange
            var quotation = new SupplierQuotation
            {
                PartId = 1,
                SupplierId = 1,
                Status = "Requested",
                QuotationNumber = "RQ-2025-00001",
                IsDeleted = true,
                CreatedAt = DateTime.Now
            };
            _context.Set<SupplierQuotation>().Add(quotation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetQuotationById(quotation.Id);

            // Assert
            // API filters IsDeleted, so should return NotFound
            var notFoundResult = Assert.IsType<ActionResult<ApiResponse<SupplierQuotationDto>>>(result);
            var apiResponse = Assert.IsType<ObjectResult>(notFoundResult.Result);
            Assert.Equal(404, apiResponse.StatusCode);
            var response = Assert.IsType<ApiResponse<SupplierQuotationDto>>(apiResponse.Value);
            Assert.False(response.Success);
            Assert.NotNull(response.Message);
            Assert.Contains("Quotation not found", response.Message);
        }

        #endregion

        #region Update Quotation Tests

        [Fact]
        public async Task UpdateQuotation_WithValidData_ShouldUpdateQuotation()
        {
            // Arrange
            var quotation = new SupplierQuotation
            {
                PartId = 1,
                SupplierId = 1,
                Status = "Requested",
                QuotationNumber = "RQ-2025-00001",
                UnitPrice = 0,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            _context.Set<SupplierQuotation>().Add(quotation);
            await _context.SaveChangesAsync();

            var updateDto = new UpdateQuotationDto
            {
                UnitPrice = 50000,
                MinimumOrderQuantity = 10,
                LeadTimeDays = 7,
                ValidUntil = DateTime.Now.AddDays(30),
                WarrantyPeriod = "12 tháng",
                WarrantyTerms = "Bảo hành chính hãng",
                ResponseNotes = "Có sẵn hàng",
                Status = "Pending"
            };

            // Act
            var result = await _controller.UpdateQuotation(quotation.Id, updateDto);

            // Assert
            var okResult = Assert.IsType<ActionResult<ApiResponse<SupplierQuotationDto>>>(result);
            var apiResponse = Assert.IsType<OkObjectResult>(okResult.Result);
            var response = Assert.IsType<ApiResponse<SupplierQuotationDto>>(apiResponse.Value);
            
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal("Pending", response.Data.Status);
            Assert.Equal(50000, response.Data.UnitPrice);
            Assert.Equal(10, response.Data.MinimumOrderQuantity);
            Assert.NotNull(response.Data.ResponseDate);

            // Verify in database
            var updatedQuotation = await _context.Set<SupplierQuotation>().FindAsync(quotation.Id);
            Assert.NotNull(updatedQuotation);
            Assert.Equal("Pending", updatedQuotation.Status);
            Assert.Equal(50000, updatedQuotation.UnitPrice);
        }

        [Fact]
        public async Task UpdateQuotation_WithInvalidStatus_ShouldReturnBadRequest()
        {
            // Arrange
            var quotation = new SupplierQuotation
            {
                PartId = 1,
                SupplierId = 1,
                Status = "Pending", // Not "Requested"
                QuotationNumber = "RQ-2025-00001",
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            _context.Set<SupplierQuotation>().Add(quotation);
            await _context.SaveChangesAsync();

            var updateDto = new UpdateQuotationDto
            {
                UnitPrice = 50000,
                MinimumOrderQuantity = 10,
                Status = "Pending"
            };

            // Act
            var result = await _controller.UpdateQuotation(quotation.Id, updateDto);

            // Assert
            var badRequestResult = Assert.IsType<ActionResult<ApiResponse<SupplierQuotationDto>>>(result);
            var apiResponse = Assert.IsType<ObjectResult>(badRequestResult.Result);
            Assert.Equal(400, apiResponse.StatusCode);
            var response = Assert.IsType<ApiResponse<SupplierQuotationDto>>(apiResponse.Value);
            Assert.False(response.Success);
            Assert.NotNull(response.Message);
            Assert.Contains("Only 'Requested' quotations can be updated", response.Message);
        }

        #endregion

        #region Accept Quotation Tests

        [Fact]
        public async Task AcceptQuotation_WithValidStatus_ShouldAcceptQuotation()
        {
            // Arrange
            var quotation = new SupplierQuotation
            {
                PartId = 1,
                SupplierId = 1,
                Status = "Pending",
                QuotationNumber = "RQ-2025-00001",
                UnitPrice = 50000,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            _context.Set<SupplierQuotation>().Add(quotation);
            await _context.SaveChangesAsync();

            var acceptDto = new AcceptRejectQuotationDto
            {
                Notes = "Chấp nhận báo giá"
            };

            // Act
            var result = await _controller.AcceptQuotation(quotation.Id, acceptDto);

            // Assert
            var okResult = Assert.IsType<ActionResult<ApiResponse<SupplierQuotationDto>>>(result);
            var apiResponse = Assert.IsType<OkObjectResult>(okResult.Result);
            var response = Assert.IsType<ApiResponse<SupplierQuotationDto>>(apiResponse.Value);
            
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal("Accepted", response.Data.Status);

            // Verify in database
            var acceptedQuotation = await _context.Set<SupplierQuotation>().FindAsync(quotation.Id);
            Assert.NotNull(acceptedQuotation);
            Assert.Equal("Accepted", acceptedQuotation.Status);
        }

        [Fact]
        public async Task AcceptQuotation_WithInvalidStatus_ShouldReturnBadRequest()
        {
            // Arrange
            var quotation = new SupplierQuotation
            {
                PartId = 1,
                SupplierId = 1,
                Status = "Requested", // Not "Pending"
                QuotationNumber = "RQ-2025-00001",
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            _context.Set<SupplierQuotation>().Add(quotation);
            await _context.SaveChangesAsync();

            var acceptDto = new AcceptRejectQuotationDto();

            // Act
            var result = await _controller.AcceptQuotation(quotation.Id, acceptDto);

            // Assert
            var badRequestResult = Assert.IsType<ActionResult<ApiResponse<SupplierQuotationDto>>>(result);
            var apiResponse = Assert.IsType<ObjectResult>(badRequestResult.Result);
            Assert.Equal(400, apiResponse.StatusCode);
            var response = Assert.IsType<ApiResponse<SupplierQuotationDto>>(apiResponse.Value);
            Assert.False(response.Success);
            Assert.NotNull(response.Message);
            Assert.Contains("Only 'Pending' quotations can be accepted", response.Message);
        }

        #endregion

        #region Reject Quotation Tests

        [Fact]
        public async Task RejectQuotation_WithValidStatus_ShouldRejectQuotation()
        {
            // Arrange
            var quotation = new SupplierQuotation
            {
                PartId = 1,
                SupplierId = 1,
                Status = "Pending",
                QuotationNumber = "RQ-2025-00001",
                UnitPrice = 50000,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            _context.Set<SupplierQuotation>().Add(quotation);
            await _context.SaveChangesAsync();

            var rejectDto = new AcceptRejectQuotationDto
            {
                Notes = "Giá quá cao"
            };

            // Act
            var result = await _controller.RejectQuotation(quotation.Id, rejectDto);

            // Assert
            var okResult = Assert.IsType<ActionResult<ApiResponse<SupplierQuotationDto>>>(result);
            var apiResponse = Assert.IsType<OkObjectResult>(okResult.Result);
            var response = Assert.IsType<ApiResponse<SupplierQuotationDto>>(apiResponse.Value);
            
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal("Rejected", response.Data.Status);

            // Verify in database
            var rejectedQuotation = await _context.Set<SupplierQuotation>().FindAsync(quotation.Id);
            Assert.NotNull(rejectedQuotation);
            Assert.Equal("Rejected", rejectedQuotation.Status);
        }

        [Fact]
        public async Task RejectQuotation_WithInvalidStatus_ShouldReturnBadRequest()
        {
            // Arrange
            var quotation = new SupplierQuotation
            {
                PartId = 1,
                SupplierId = 1,
                Status = "Requested", // Not "Pending"
                QuotationNumber = "RQ-2025-00001",
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            _context.Set<SupplierQuotation>().Add(quotation);
            await _context.SaveChangesAsync();

            var rejectDto = new AcceptRejectQuotationDto();

            // Act
            var result = await _controller.RejectQuotation(quotation.Id, rejectDto);

            // Assert
            var badRequestResult = Assert.IsType<ActionResult<ApiResponse<SupplierQuotationDto>>>(result);
            var apiResponse = Assert.IsType<ObjectResult>(badRequestResult.Result);
            Assert.Equal(400, apiResponse.StatusCode);
            var response = Assert.IsType<ApiResponse<SupplierQuotationDto>>(apiResponse.Value);
            Assert.False(response.Success);
            Assert.NotNull(response.Message);
            Assert.Contains("Only 'Pending' quotations can be rejected", response.Message);
        }

        #endregion

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}

