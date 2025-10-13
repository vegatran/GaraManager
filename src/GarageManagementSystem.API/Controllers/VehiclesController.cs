using AutoMapper;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarageManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiScope")]
    public class VehiclesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public VehiclesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<VehicleDto>>>> GetVehicles()
        {
            try
            {
                var vehicles = await _unitOfWork.Vehicles.GetAllWithCustomerAsync();
                var vehicleDtos = vehicles.Select(v => _mapper.Map<VehicleDto>(v)).ToList();
                
                return Ok(ApiResponse<List<VehicleDto>>.SuccessResult(vehicleDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<VehicleDto>>.ErrorResult("Error retrieving vehicles", ex.Message));
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

                var vehicle = new Core.Entities.Vehicle
                {
                    LicensePlate = createDto.LicensePlate,
                    Brand = createDto.Brand,
                    Model = createDto.Model,
                    Year = createDto.Year,
                    Color = createDto.Color,
                    VIN = createDto.VIN,
                    EngineNumber = createDto.EngineNumber,
                    CustomerId = createDto.CustomerId,
                    VehicleType = createDto.VehicleType,
                    // Các trường bảo hiểm
                    InsuranceCompany = createDto.InsuranceCompany,
                    PolicyNumber = createDto.PolicyNumber,
                    CoverageType = createDto.CoverageType,
                    ClaimNumber = createDto.ClaimNumber,
                    AdjusterName = createDto.AdjusterName,
                    AdjusterPhone = createDto.AdjusterPhone,
                    // Các trường công ty
                    CompanyName = createDto.CompanyName,
                    TaxCode = createDto.TaxCode,
                    ContactPerson = createDto.ContactPerson,
                    ContactPhone = createDto.ContactPhone,
                    Department = createDto.Department,
                    CostCenter = createDto.CostCenter
                };

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

                vehicle.LicensePlate = updateDto.LicensePlate;
                vehicle.Brand = updateDto.Brand;
                vehicle.Model = updateDto.Model;
                vehicle.Year = updateDto.Year;
                vehicle.Color = updateDto.Color;
                vehicle.VIN = updateDto.VIN;
                vehicle.EngineNumber = updateDto.EngineNumber;
                vehicle.CustomerId = updateDto.CustomerId;
                vehicle.VehicleType = updateDto.VehicleType;
                // Insurance fields
                vehicle.InsuranceCompany = updateDto.InsuranceCompany;
                vehicle.PolicyNumber = updateDto.PolicyNumber;
                vehicle.CoverageType = updateDto.CoverageType;
                vehicle.ClaimNumber = updateDto.ClaimNumber;
                vehicle.AdjusterName = updateDto.AdjusterName;
                vehicle.AdjusterPhone = updateDto.AdjusterPhone;
                // Company fields
                vehicle.CompanyName = updateDto.CompanyName;
                vehicle.TaxCode = updateDto.TaxCode;
                vehicle.ContactPerson = updateDto.ContactPerson;
                vehicle.ContactPhone = updateDto.ContactPhone;
                vehicle.Department = updateDto.Department;
                vehicle.CostCenter = updateDto.CostCenter;

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

    }
}

