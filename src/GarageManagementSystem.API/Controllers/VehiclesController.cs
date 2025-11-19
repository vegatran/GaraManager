using AutoMapper;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Core.Extensions;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using GarageManagementSystem.API.Services;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiScope")]
    public class VehiclesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly GarageDbContext _context;

        public VehiclesController(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService, GarageDbContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<VehicleDto>>> GetVehicles(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? brand = null,
            [FromQuery] int? customerId = null)
        {
            try
            {
                var vehicles = await _unitOfWork.Vehicles.GetAllWithCustomerAsync();
                var query = vehicles.AsQueryable();
                
                // Apply search filter if provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(v => 
                        v.LicensePlate.Contains(searchTerm) || 
                        v.VIN.Contains(searchTerm) || 
                        v.Model.Contains(searchTerm));
                }
                
                // Apply brand filter if provided
                if (!string.IsNullOrEmpty(brand))
                {
                    query = query.Where(v => v.Brand == brand);
                }
                
                // Apply customer filter if provided
                if (customerId.HasValue)
                {
                    query = query.Where(v => v.CustomerId == customerId.Value);
                }

                // ✅ OPTIMIZED: Get paged results with total count - automatically chooses best method
                var (pagedVehicles, totalCount) = await query.ToPagedListWithCountAsync(pageNumber, pageSize, _context);
                var vehicleDtos = pagedVehicles.Select(v => _mapper.Map<VehicleDto>(v)).ToList();
                
                return Ok(PagedResponse<VehicleDto>.CreateSuccessResult(
                    vehicleDtos, pageNumber, pageSize, totalCount, "Vehicles retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, PagedResponse<VehicleDto>.CreateErrorResult("Error retrieving vehicles"));
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả xe cho dropdown (không phân trang)
        /// </summary>
        [HttpGet("dropdown")]
        public async Task<ActionResult<List<VehicleDto>>> GetAllForDropdown()
        {
            try
            {
                var vehicles = await _unitOfWork.Vehicles.GetAllWithCustomerAsync();
                var vehicleDtos = vehicles.Select(v => _mapper.Map<VehicleDto>(v)).ToList();
                
                return Ok(vehicleDtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting vehicles for dropdown: {ex}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<VehicleDto>>> GetVehicle(int id)
        {
            try
            {
                var vehicle = await _unitOfWork.Vehicles.GetByIdWithCustomerAsync(id);
                if (vehicle == null)
                {
                    return NotFound(ApiResponse<VehicleDto>.ErrorResult("Vehicle not found"));
                }

                var vehicleDto = _mapper.Map<VehicleDto>(vehicle);
                return Ok(ApiResponse<VehicleDto>.SuccessResult(vehicleDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<VehicleDto>.ErrorResult("Error retrieving vehicle", ex.Message));
            }
        }

        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<ApiResponse<List<VehicleDto>>>> GetVehiclesByCustomer(int customerId)
        {
            try
            {
                var vehicles = await _unitOfWork.Vehicles.GetByCustomerIdAsync(customerId);
                var vehicleDtos = vehicles.Select(v => _mapper.Map<VehicleDto>(v)).ToList();

                return Ok(ApiResponse<List<VehicleDto>>.SuccessResult(vehicleDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<VehicleDto>>.ErrorResult("Error retrieving vehicles", ex.Message));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<VehicleDto>>> CreateVehicle(CreateVehicleDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<VehicleDto>.ErrorResult("Invalid data", errors));
                }

                // Check if customer exists
                var customer = await _unitOfWork.Customers.GetByIdAsync(createDto.CustomerId);
                if (customer == null)
                {
                    return BadRequest(ApiResponse<VehicleDto>.ErrorResult("Customer not found"));
                }

                var vehicle = _mapper.Map<Core.Entities.Vehicle>(createDto);

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.Vehicles.AddAsync(vehicle);
                    await _unitOfWork.SaveChangesAsync();

                    // Commit transaction nếu thành công
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch
                {
                    // Rollback transaction nếu có lỗi
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }

                // Tải thông tin khách hàng cho DTO
                vehicle = await _unitOfWork.Vehicles.GetByIdWithCustomerAsync(vehicle.Id);
                var vehicleDto = _mapper.Map<VehicleDto>(vehicle!);

                return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id }, 
                    ApiResponse<VehicleDto>.SuccessResult(vehicleDto, "Vehicle created successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<VehicleDto>.ErrorResult("Error creating vehicle", ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<VehicleDto>>> UpdateVehicle(int id, UpdateVehicleDto updateDto)
        {
            try
            {
                if (id != updateDto.Id)
                {
                    return BadRequest(ApiResponse<VehicleDto>.ErrorResult("ID mismatch"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<VehicleDto>.ErrorResult("Invalid data", errors));
                }

                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(id);
                if (vehicle == null)
                {
                    return NotFound(ApiResponse<VehicleDto>.ErrorResult("Vehicle not found"));
                }

                // Check if customer exists
                var customer = await _unitOfWork.Customers.GetByIdAsync(updateDto.CustomerId);
                if (customer == null)
                {
                    return BadRequest(ApiResponse<VehicleDto>.ErrorResult("Customer not found"));
                }

                _mapper.Map(updateDto, vehicle);

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.Vehicles.UpdateAsync(vehicle);
                    await _unitOfWork.SaveChangesAsync();

                    // Commit transaction nếu thành công
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch
                {
                    // Rollback transaction nếu có lỗi
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }

                // Reload with customer
                vehicle = await _unitOfWork.Vehicles.GetByIdWithCustomerAsync(id);
                var vehicleDto = _mapper.Map<VehicleDto>(vehicle!);

                return Ok(ApiResponse<VehicleDto>.SuccessResult(vehicleDto, "Vehicle updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<VehicleDto>.ErrorResult("Error updating vehicle", ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteVehicle(int id)
        {
            try
            {
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(id);
                if (vehicle == null)
                {
                    return NotFound(ApiResponse.ErrorResult("Vehicle not found"));
                }

                await _unitOfWork.Vehicles.DeleteAsync(vehicle);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse.SuccessResult("Vehicle deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResult("Error deleting vehicle", ex.Message));
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<List<VehicleDto>>>> SearchVehicles([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest(ApiResponse<List<VehicleDto>>.ErrorResult("Search term cannot be empty"));
                }

                var vehicles = await _unitOfWork.Vehicles.SearchAsync(searchTerm);
                var vehicleDtos = vehicles.Select(v => _mapper.Map<VehicleDto>(v)).ToList();

                return Ok(ApiResponse<List<VehicleDto>>.SuccessResult(vehicleDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<VehicleDto>>.ErrorResult("Error searching vehicles", ex.Message));
            }
        }

        /// <summary>
        /// Get vehicles by type (Personal, Insurance, Company)
        /// </summary>
        [HttpGet("by-type/{vehicleType}")]
        public async Task<ActionResult<ApiResponse<List<VehicleDto>>>> GetVehiclesByType(string vehicleType)
        {
            try
            {
                if (!new[] { "Personal", "Insurance", "Company" }.Contains(vehicleType))
                {
                    return BadRequest(ApiResponse<List<VehicleDto>>.ErrorResult("Invalid vehicle type. Must be Personal, Insurance, or Company"));
                }

                var vehicles = await _unitOfWork.Vehicles.GetAllWithCustomerAsync();
                var filteredVehicles = vehicles.Where(v => v.VehicleType == vehicleType).ToList();
                var vehicleDtos = filteredVehicles.Select(v => _mapper.Map<VehicleDto>(v)).ToList();

                return Ok(ApiResponse<List<VehicleDto>>.SuccessResult(vehicleDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<VehicleDto>>.ErrorResult("Error retrieving vehicles by type", ex.Message));
            }
        }

        /// <summary>
        /// Get insurance vehicles
        /// </summary>
        [HttpGet("insurance")]
        public async Task<ActionResult<ApiResponse<List<VehicleDto>>>> GetInsuranceVehicles()
        {
            try
            {
                var vehicles = await _unitOfWork.Vehicles.GetAllWithCustomerAsync();
                var insuranceVehicles = vehicles.Where(v => v.VehicleType == "Insurance").ToList();
                var vehicleDtos = insuranceVehicles.Select(v => _mapper.Map<VehicleDto>(v)).ToList();

                return Ok(ApiResponse<List<VehicleDto>>.SuccessResult(vehicleDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<VehicleDto>>.ErrorResult("Error retrieving insurance vehicles", ex.Message));
            }
        }

        /// <summary>
        /// Get company vehicles
        /// </summary>
        [HttpGet("company")]
        public async Task<ActionResult<ApiResponse<List<VehicleDto>>>> GetCompanyVehicles()
        {
            try
            {
                var vehicles = await _unitOfWork.Vehicles.GetAllWithCustomerAsync();
                var companyVehicles = vehicles.Where(v => v.VehicleType == "Company").ToList();
                var vehicleDtos = companyVehicles.Select(v => _mapper.Map<VehicleDto>(v)).ToList();

                return Ok(ApiResponse<List<VehicleDto>>.SuccessResult(vehicleDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<VehicleDto>>.ErrorResult("Error retrieving company vehicles", ex.Message));
            }
        }

        /// <summary>
        /// Change vehicle type (Personal, Insurance, Company)
        /// </summary>
        [HttpPut("{id}/change-type")]
        public async Task<ActionResult<ApiResponse<VehicleDto>>> ChangeVehicleType(int id, [FromBody] ChangeVehicleTypeDto changeTypeDto)
        {
            try
            {
                if (!new[] { "Personal", "Insurance", "Company" }.Contains(changeTypeDto.VehicleType))
                {
                    return BadRequest(ApiResponse<VehicleDto>.ErrorResult("Invalid vehicle type. Must be Personal, Insurance, or Company"));
                }

                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(id);
                if (vehicle == null)
                {
                    return NotFound(ApiResponse<VehicleDto>.ErrorResult("Vehicle not found"));
                }

                // Update vehicle type and clear related fields if changing type
                if (vehicle.VehicleType != changeTypeDto.VehicleType)
                {
                    vehicle.VehicleType = changeTypeDto.VehicleType;

                    // Clear fields that don't apply to new type
                    if (changeTypeDto.VehicleType == "Personal")
                    {
                        // Clear insurance and company fields
                        vehicle.InsuranceCompany = null;
                        vehicle.PolicyNumber = null;
                        vehicle.CoverageType = null;
                        vehicle.ClaimNumber = null;
                        vehicle.AdjusterName = null;
                        vehicle.AdjusterPhone = null;
                        vehicle.CompanyName = null;
                        vehicle.TaxCode = null;
                        vehicle.ContactPerson = null;
                        vehicle.ContactPhone = null;
                        vehicle.Department = null;
                        vehicle.CostCenter = null;
                    }
                    else if (changeTypeDto.VehicleType == "Insurance")
                    {
                        // Clear company fields
                        vehicle.CompanyName = null;
                        vehicle.TaxCode = null;
                        vehicle.ContactPerson = null;
                        vehicle.ContactPhone = null;
                        vehicle.Department = null;
                        vehicle.CostCenter = null;
                    }
                    else if (changeTypeDto.VehicleType == "Company")
                    {
                        // Clear insurance fields
                        vehicle.InsuranceCompany = null;
                        vehicle.PolicyNumber = null;
                        vehicle.CoverageType = null;
                        vehicle.ClaimNumber = null;
                        vehicle.AdjusterName = null;
                        vehicle.AdjusterPhone = null;
                    }
                }

                await _unitOfWork.Vehicles.UpdateAsync(vehicle);
                await _unitOfWork.SaveChangesAsync();

                var updatedVehicle = await _unitOfWork.Vehicles.GetByIdWithCustomerAsync(id);
                var vehicleDto = _mapper.Map<VehicleDto>(updatedVehicle!);

                return Ok(ApiResponse<VehicleDto>.SuccessResult(vehicleDto, "Vehicle type changed successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<VehicleDto>.ErrorResult("Error changing vehicle type", ex.Message));
            }
        }

        /// <summary>
        /// Get workflow status for a vehicle
        /// </summary>
        [HttpGet("{id}/workflow-status")]
        public async Task<ActionResult<ApiResponse<VehicleWorkflowStatusDto>>> GetVehicleWorkflowStatus(int id)
        {
            try
            {
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(id);
                if (vehicle == null)
                {
                    return NotFound(ApiResponse<VehicleWorkflowStatusDto>.ErrorResult("Vehicle not found"));
                }

                // Get related data
                var inspections = await _unitOfWork.VehicleInspections.GetByVehicleIdAsync(id);
                var quotations = await _unitOfWork.ServiceQuotations.GetByVehicleIdAsync(id);
                var serviceOrders = await _unitOfWork.ServiceOrders.GetByVehicleIdAsync(id);

                var status = new VehicleWorkflowStatusDto
                {
                    VehicleId = vehicle.Id,
                    VehicleType = vehicle.VehicleType,
                    LicensePlate = vehicle.LicensePlate,
                    HasActiveInspection = inspections.Any(i => i.Status == "InProgress"),
                    HasPendingQuotation = quotations.Any(q => q.Status == "Sent"),
                    HasApprovedQuotation = quotations.Any(q => q.Status == "Approved"),
                    HasActiveServiceOrder = serviceOrders.Any(so => so.Status == "InProgress"),
                    LastInspectionDate = inspections.OrderByDescending(i => i.InspectionDate).FirstOrDefault()?.InspectionDate,
                    LastQuotationDate = quotations.OrderByDescending(q => q.QuotationDate).FirstOrDefault()?.QuotationDate,
                    LastServiceOrderDate = serviceOrders.OrderByDescending(so => so.OrderDate).FirstOrDefault()?.OrderDate
                };

                return Ok(ApiResponse<VehicleWorkflowStatusDto>.SuccessResult(status));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<VehicleWorkflowStatusDto>.ErrorResult("Error retrieving workflow status", ex.Message));
            }
        }

        [HttpGet("available")]
        public async Task<ActionResult<ApiResponse<List<VehicleDto>>>> GetAvailableVehicles()
        {
            try
            {
                var vehicles = await _unitOfWork.Vehicles.GetAvailableVehiclesAsync();
                var vehicleDtos = vehicles.Select(v => _mapper.Map<VehicleDto>(v)).ToList();
                
                return Ok(ApiResponse<List<VehicleDto>>.SuccessResult(vehicleDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<VehicleDto>>.ErrorResult("Error retrieving available vehicles", ex.Message));
            }
        }

        [HttpGet("{id}/availability")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckVehicleAvailability(int id)
        {
            try
            {
                var isAvailable = await _unitOfWork.Vehicles.IsVehicleAvailableAsync(id);
                return Ok(ApiResponse<bool>.SuccessResult(isAvailable));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Error checking vehicle availability", ex.Message));
            }
        }

    }
}

