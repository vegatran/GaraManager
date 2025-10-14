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
    public class VehicleInspectionsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public VehicleInspectionsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<VehicleInspectionDto>>>> GetInspections()
        {
            try
            {
                var inspections = await _unitOfWork.VehicleInspections.GetAllWithDetailsAsync();
                var inspectionDtos = inspections.Select(MapToDto).ToList();
                
                return Ok(ApiResponse<List<VehicleInspectionDto>>.SuccessResult(inspectionDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<VehicleInspectionDto>>.ErrorResult("Error retrieving inspections", ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<VehicleInspectionDto>>> GetInspection(int id)
        {
            try
            {
                var inspection = await _unitOfWork.VehicleInspections.GetByIdWithDetailsAsync(id);
                if (inspection == null)
                {
                    return NotFound(ApiResponse<VehicleInspectionDto>.ErrorResult("Inspection not found"));
                }

                var inspectionDto = MapToDto(inspection);
                return Ok(ApiResponse<VehicleInspectionDto>.SuccessResult(inspectionDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<VehicleInspectionDto>.ErrorResult("Error retrieving inspection", ex.Message));
            }
        }

        [HttpGet("vehicle/{vehicleId}")]
        public async Task<ActionResult<ApiResponse<List<VehicleInspectionDto>>>> GetInspectionsByVehicle(int vehicleId)
        {
            try
            {
                var inspections = await _unitOfWork.VehicleInspections.GetByVehicleIdAsync(vehicleId);
                var inspectionDtos = inspections.Select(MapToDto).ToList();

                return Ok(ApiResponse<List<VehicleInspectionDto>>.SuccessResult(inspectionDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<VehicleInspectionDto>>.ErrorResult("Error retrieving inspections", ex.Message));
            }
        }

        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<ApiResponse<List<VehicleInspectionDto>>>> GetInspectionsByCustomer(int customerId)
        {
            try
            {
                var inspections = await _unitOfWork.VehicleInspections.GetByCustomerIdAsync(customerId);
                var inspectionDtos = inspections.Select(MapToDto).ToList();

                return Ok(ApiResponse<List<VehicleInspectionDto>>.SuccessResult(inspectionDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<VehicleInspectionDto>>.ErrorResult("Error retrieving inspections", ex.Message));
            }
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<ApiResponse<List<VehicleInspectionDto>>>> GetInspectionsByStatus(string status)
        {
            try
            {
                var inspections = await _unitOfWork.VehicleInspections.GetByStatusAsync(status);
                var inspectionDtos = inspections.Select(MapToDto).ToList();

                return Ok(ApiResponse<List<VehicleInspectionDto>>.SuccessResult(inspectionDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<VehicleInspectionDto>>.ErrorResult("Error retrieving inspections", ex.Message));
            }
        }

        [HttpGet("latest/vehicle/{vehicleId}")]
        public async Task<ActionResult<ApiResponse<VehicleInspectionDto>>> GetLatestInspectionByVehicle(int vehicleId)
        {
            try
            {
                var inspection = await _unitOfWork.VehicleInspections.GetLatestInspectionByVehicleAsync(vehicleId);
                if (inspection == null)
                {
                    return NotFound(ApiResponse<VehicleInspectionDto>.ErrorResult("No inspection found for this vehicle"));
                }

                var inspectionDto = MapToDto(inspection);
                return Ok(ApiResponse<VehicleInspectionDto>.SuccessResult(inspectionDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<VehicleInspectionDto>.ErrorResult("Error retrieving inspection", ex.Message));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<VehicleInspectionDto>>> CreateInspection(CreateVehicleInspectionDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<VehicleInspectionDto>.ErrorResult("Invalid data", errors));
                }

                // Validate vehicle and customer exist
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(createDto.VehicleId);
                if (vehicle == null)
                {
                    return BadRequest(ApiResponse<VehicleInspectionDto>.ErrorResult("Vehicle not found"));
                }

                var customer = await _unitOfWork.Customers.GetByIdAsync(createDto.CustomerId);
                if (customer == null)
                {
                    return BadRequest(ApiResponse<VehicleInspectionDto>.ErrorResult("Customer not found"));
                }

                // Tự động đặt InspectionType dựa trên VehicleType nếu không được chỉ định
                string inspectionType = createDto.InspectionType ?? vehicle.VehicleType switch
                {
                    "Insurance" => "Insurance",
                    "Company" => "Company", 
                    _ => "General"
                };

                // Tạo phiếu kiểm tra
                var inspection = _mapper.Map<Core.Entities.VehicleInspection>(createDto);
                inspection.InspectionNumber = await _unitOfWork.VehicleInspections.GenerateInspectionNumberAsync();
                inspection.InspectionType = inspectionType;
                inspection.Status = "InProgress";

                // Add issues
                foreach (var issueDto in createDto.Issues)
                {
                    inspection.Issues.Add(new Core.Entities.InspectionIssue
                    {
                        Category = issueDto.Category,
                        IssueName = issueDto.IssueName,
                        Description = issueDto.Description,
                        Severity = issueDto.Severity,
                        RequiresImmediateAction = issueDto.RequiresImmediateAction,
                        EstimatedCost = issueDto.EstimatedCost,
                        TechnicianNotes = issueDto.TechnicianNotes,
                        SuggestedServiceId = issueDto.SuggestedServiceId,
                        Status = "Identified"
                    });
                }

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.VehicleInspections.AddAsync(inspection);
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

                // Reload with details
                inspection = await _unitOfWork.VehicleInspections.GetByIdWithDetailsAsync(inspection.Id);
                var inspectionDto = MapToDto(inspection!);

                return CreatedAtAction(nameof(GetInspection), new { id = inspection.Id }, 
                    ApiResponse<VehicleInspectionDto>.SuccessResult(inspectionDto, "Inspection created successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<VehicleInspectionDto>.ErrorResult("Error creating inspection", ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<VehicleInspectionDto>>> UpdateInspection(int id, UpdateVehicleInspectionDto updateDto)
        {
            try
            {
                if (id != updateDto.Id)
                {
                    return BadRequest(ApiResponse<VehicleInspectionDto>.ErrorResult("ID mismatch"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<VehicleInspectionDto>.ErrorResult("Invalid data", errors));
                }

                var inspection = await _unitOfWork.VehicleInspections.GetByIdAsync(id);
                if (inspection == null)
                {
                    return NotFound(ApiResponse<VehicleInspectionDto>.ErrorResult("Inspection not found"));
                }

                // Update inspection properties
                inspection.InspectorId = updateDto.InspectorId; // Missing line!
                inspection.InspectionDate = updateDto.InspectionDate; // Update inspection date
                inspection.InspectionType = updateDto.InspectionType;
                inspection.CurrentMileage = updateDto.CurrentMileage;
                inspection.FuelLevel = updateDto.FuelLevel;
                inspection.GeneralCondition = updateDto.GeneralCondition;
                inspection.ExteriorCondition = updateDto.ExteriorCondition;
                inspection.InteriorCondition = updateDto.InteriorCondition;
                inspection.EngineCondition = updateDto.EngineCondition;
                inspection.BrakeCondition = updateDto.BrakeCondition;
                inspection.SuspensionCondition = updateDto.SuspensionCondition;
                inspection.TireCondition = updateDto.TireCondition;
                inspection.ElectricalCondition = updateDto.ElectricalCondition;
                inspection.LightsCondition = updateDto.LightsCondition;
                inspection.CustomerComplaints = updateDto.CustomerComplaints;
                inspection.Recommendations = updateDto.Recommendations;
                inspection.TechnicianNotes = updateDto.TechnicianNotes;
                
                if (!string.IsNullOrEmpty(updateDto.Status))
                {
                    inspection.Status = updateDto.Status;
                    if (updateDto.Status == "Completed" && !inspection.CompletedDate.HasValue)
                    {
                        inspection.CompletedDate = DateTime.Now;
                    }
                }

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.VehicleInspections.UpdateAsync(inspection);
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

                // Reload with details
                inspection = await _unitOfWork.VehicleInspections.GetByIdWithDetailsAsync(id);
                var inspectionDto = MapToDto(inspection!);

                return Ok(ApiResponse<VehicleInspectionDto>.SuccessResult(inspectionDto, "Inspection updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<VehicleInspectionDto>.ErrorResult("Error updating inspection", ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteInspection(int id)
        {
            try
            {
                var inspection = await _unitOfWork.VehicleInspections.GetByIdAsync(id);
                if (inspection == null)
                {
                    return NotFound(ApiResponse.ErrorResult("Inspection not found"));
                }

                await _unitOfWork.VehicleInspections.DeleteAsync(inspection);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse.SuccessResult("Inspection deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResult("Error deleting inspection", ex.Message));
            }
        }

        [HttpPut("{id}/complete")]
        public async Task<ActionResult<ApiResponse<VehicleInspectionDto>>> CompleteInspection(int id)
        {
            try
            {
                var inspection = await _unitOfWork.VehicleInspections.GetByIdAsync(id);
                if (inspection == null)
                {
                    return NotFound(ApiResponse<VehicleInspectionDto>.ErrorResult("Inspection not found"));
                }

                inspection.Status = "Completed";
                inspection.CompletedDate = DateTime.Now;

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.VehicleInspections.UpdateAsync(inspection);
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

                // Reload with details
                inspection = await _unitOfWork.VehicleInspections.GetByIdWithDetailsAsync(id);
                var inspectionDto = MapToDto(inspection!);

                return Ok(ApiResponse<VehicleInspectionDto>.SuccessResult(inspectionDto, "Inspection completed successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<VehicleInspectionDto>.ErrorResult("Error completing inspection", ex.Message));
            }
        }

        private VehicleInspectionDto MapToDto(Core.Entities.VehicleInspection inspection)
        {
            return _mapper.Map<VehicleInspectionDto>(inspection);
        }

        private static VehicleInspectionDto MapToDtoOld(Core.Entities.VehicleInspection inspection)
        {
            return new VehicleInspectionDto
            {
                Id = inspection.Id,
                InspectionNumber = inspection.InspectionNumber,
                VehicleId = inspection.VehicleId,
                CustomerId = inspection.CustomerId,
                InspectorId = inspection.InspectorId,
                InspectionDate = inspection.InspectionDate,
                InspectionType = inspection.InspectionType,
                CurrentMileage = inspection.CurrentMileage,
                FuelLevel = inspection.FuelLevel,
                GeneralCondition = inspection.GeneralCondition,
                ExteriorCondition = inspection.ExteriorCondition,
                InteriorCondition = inspection.InteriorCondition,
                EngineCondition = inspection.EngineCondition,
                BrakeCondition = inspection.BrakeCondition,
                SuspensionCondition = inspection.SuspensionCondition,
                TireCondition = inspection.TireCondition,
                ElectricalCondition = inspection.ElectricalCondition,
                LightsCondition = inspection.LightsCondition,
                CustomerComplaints = inspection.CustomerComplaints,
                Recommendations = inspection.Recommendations,
                TechnicianNotes = inspection.TechnicianNotes,
                Status = inspection.Status,
                CompletedDate = inspection.CompletedDate,
                QuotationId = inspection.QuotationId,
                Customer = inspection.Customer != null ? new CustomerDto
                {
                    Id = inspection.Customer.Id,
                    Name = inspection.Customer.Name,
                    Phone = inspection.Customer.Phone,
                    Email = inspection.Customer.Email
                } : null,
                Vehicle = inspection.Vehicle != null ? new VehicleDto
                {
                    Id = inspection.Vehicle.Id,
                    LicensePlate = inspection.Vehicle.LicensePlate,
                    Brand = inspection.Vehicle.Brand,
                    Model = inspection.Vehicle.Model,
                    Year = inspection.Vehicle.Year,
                    Color = inspection.Vehicle.Color
                } : null,
                Inspector = inspection.Inspector != null ? new EmployeeDto
                {
                    Id = inspection.Inspector.Id,
                    Name = inspection.Inspector.Name,
                    Position = inspection.Inspector.Position
                } : null,
                Issues = inspection.Issues?.Select(issue => new InspectionIssueDto
                {
                    Id = issue.Id,
                    VehicleInspectionId = issue.VehicleInspectionId,
                    Category = issue.Category,
                    IssueName = issue.IssueName,
                    Description = issue.Description,
                    Severity = issue.Severity,
                    RequiresImmediateAction = issue.RequiresImmediateAction,
                    EstimatedCost = issue.EstimatedCost,
                    TechnicianNotes = issue.TechnicianNotes,
                    SuggestedServiceId = issue.SuggestedServiceId,
                    Status = issue.Status,
                    SuggestedService = issue.SuggestedService != null ? new ServiceDto
                    {
                        Id = issue.SuggestedService.Id,
                        Name = issue.SuggestedService.Name,
                        Price = issue.SuggestedService.Price,
                        Duration = issue.SuggestedService.Duration
                    } : null,
                    CreatedAt = issue.CreatedAt
                }).ToList() ?? new List<InspectionIssueDto>(),
                Photos = inspection.Photos?.Select(photo => new InspectionPhotoDto
                {
                    Id = photo.Id,
                    VehicleInspectionId = photo.VehicleInspectionId,
                    InspectionIssueId = photo.InspectionIssueId,
                    FilePath = photo.FilePath,
                    FileName = photo.FileName,
                    Category = photo.Category,
                    Description = photo.Description,
                    DisplayOrder = photo.DisplayOrder
                }).ToList() ?? new List<InspectionPhotoDto>(),
                CreatedAt = inspection.CreatedAt,
                UpdatedAt = inspection.UpdatedAt
            };
        }

        /// <summary>
        /// Get inspections by vehicle type (Personal, Insurance, Company)
        /// </summary>
        [HttpGet("by-vehicle-type/{vehicleType}")]
        public async Task<ActionResult<ApiResponse<List<VehicleInspectionDto>>>> GetInspectionsByVehicleType(string vehicleType)
        {
            try
            {
                if (!new[] { "Personal", "Insurance", "Company" }.Contains(vehicleType))
                {
                    return BadRequest(ApiResponse<List<VehicleInspectionDto>>.ErrorResult("Invalid vehicle type. Must be Personal, Insurance, or Company"));
                }

                var inspections = await _unitOfWork.VehicleInspections.GetAllWithDetailsAsync();
                var filteredInspections = inspections.Where(i => i.Vehicle.VehicleType == vehicleType).ToList();
                var inspectionDtos = filteredInspections.Select(MapToDto).ToList();

                return Ok(ApiResponse<List<VehicleInspectionDto>>.SuccessResult(inspectionDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<VehicleInspectionDto>>.ErrorResult("Error retrieving inspections by vehicle type", ex.Message));
            }
        }

        /// <summary>
        /// Get inspections by inspection type (General, Insurance, Company, Diagnostic, Pre-service, Post-repair)
        /// </summary>
        [HttpGet("by-inspection-type/{inspectionType}")]
        public async Task<ActionResult<ApiResponse<List<VehicleInspectionDto>>>> GetInspectionsByInspectionType(string inspectionType)
        {
            try
            {
                var inspections = await _unitOfWork.VehicleInspections.GetAllWithDetailsAsync();
                var filteredInspections = inspections.Where(i => i.InspectionType == inspectionType).ToList();
                var inspectionDtos = filteredInspections.Select(MapToDto).ToList();

                return Ok(ApiResponse<List<VehicleInspectionDto>>.SuccessResult(inspectionDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<VehicleInspectionDto>>.ErrorResult("Error retrieving inspections by inspection type", ex.Message));
            }
        }

        /// <summary>
        /// Get inspection workflow recommendations based on vehicle type
        /// </summary>
        [HttpGet("{id}/workflow-recommendations")]
        public async Task<ActionResult<ApiResponse<InspectionWorkflowRecommendationsDto>>> GetInspectionWorkflowRecommendations(int id)
        {
            try
            {
                var inspection = await _unitOfWork.VehicleInspections.GetByIdWithDetailsAsync(id);
                if (inspection == null)
                {
                    return NotFound(ApiResponse<InspectionWorkflowRecommendationsDto>.ErrorResult("Inspection not found"));
                }

                var recommendations = new InspectionWorkflowRecommendationsDto
                {
                    InspectionId = inspection.Id,
                    VehicleType = inspection.Vehicle.VehicleType,
                    InspectionType = inspection.InspectionType,
                    CurrentStatus = inspection.Status,
                    NextRecommendedSteps = GetNextRecommendedSteps(inspection),
                    RequiredApprovals = GetRequiredApprovals(inspection),
                    IsWorkflowBlocked = IsWorkflowBlocked(inspection),
                    BlockingReasons = GetBlockingReasons(inspection)
                };

                return Ok(ApiResponse<InspectionWorkflowRecommendationsDto>.SuccessResult(recommendations));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InspectionWorkflowRecommendationsDto>.ErrorResult("Error retrieving workflow recommendations", ex.Message));
            }
        }

        private static List<string> GetNextRecommendedSteps(Core.Entities.VehicleInspection inspection)
        {
            var steps = new List<string>();

            if (inspection.Status == "Pending")
            {
                steps.Add("Start inspection");
            }
            else if (inspection.Status == "InProgress")
            {
                steps.Add("Complete inspection");
                steps.Add("Document findings");
                steps.Add("Take photos if needed");
            }
            else if (inspection.Status == "Completed")
            {
                steps.Add("Create quotation");
                if (inspection.Vehicle.VehicleType == "Insurance")
                {
                    steps.Add("Submit to insurance company");
                    steps.Add("Wait for insurance approval");
                }
                else if (inspection.Vehicle.VehicleType == "Company")
                {
                    steps.Add("Submit to company manager");
                    steps.Add("Wait for company approval");
                }
                else
                {
                    steps.Add("Present quotation to customer");
                }
            }

            return steps;
        }

        private static List<string> GetRequiredApprovals(Core.Entities.VehicleInspection inspection)
        {
            var approvals = new List<string>();

            if (inspection.Vehicle.VehicleType == "Insurance")
            {
                approvals.Add("Insurance Company Approval");
                approvals.Add("Customer Approval (for deductible)");
            }
            else if (inspection.Vehicle.VehicleType == "Company")
            {
                approvals.Add("Company Manager Approval");
                approvals.Add("Purchase Order Required");
            }
            else
            {
                approvals.Add("Customer Approval");
            }

            return approvals;
        }

        private static bool IsWorkflowBlocked(Core.Entities.VehicleInspection inspection)
        {
            // Check if inspection has critical issues that require immediate attention
            return inspection.Issues.Any(i => i.RequiresImmediateAction && i.Severity == "Critical");
        }

        private static List<string> GetBlockingReasons(Core.Entities.VehicleInspection inspection)
        {
            var reasons = new List<string>();

            if (IsWorkflowBlocked(inspection))
            {
                var criticalIssues = inspection.Issues.Where(i => i.RequiresImmediateAction && i.Severity == "Critical");
                reasons.AddRange(criticalIssues.Select(i => $"Critical issue: {i.IssueName}"));
            }

            return reasons;
        }
    }
}

