using AutoMapper;
using GarageManagementSystem.Core.Enums;
using GarageManagementSystem.Core.Extensions;
using GarageManagementSystem.Core.Interfaces;
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
    public class VehicleInspectionsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly GarageDbContext _context;

        public VehicleInspectionsController(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService, GarageDbContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<VehicleInspectionDto>>> GetInspections(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? status = null,
            [FromQuery] int? customerId = null)
        {
            try
            {
                // ✅ OPTIMIZED: Query ở database level thay vì load tất cả vào memory
                var query = _context.VehicleInspections
                    .Where(i => !i.IsDeleted)
                    .AsQueryable();
                
                // Apply search filter if provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(i => 
                        (i.VehiclePlate != null && i.VehiclePlate.Contains(searchTerm)) || 
                        (i.CustomerName != null && i.CustomerName.Contains(searchTerm)) || 
                        (i.InspectionNumber != null && i.InspectionNumber.Contains(searchTerm)));
                }
                
                // Apply status filter if provided
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(i => i.Status == status);
                }
                
                // Apply customer filter if provided
                if (customerId.HasValue)
                {
                    query = query.Where(i => i.CustomerId == customerId.Value);
                }

                query = query.OrderByDescending(i => i.InspectionDate);

                // ✅ OPTIMIZED: Get total count ở database level (trước khi paginate)
                var totalCount = await query.CountAsync();
                
                // ✅ OPTIMIZED: Apply pagination ở database level với Skip/Take
                var pagedInspections = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Include(i => i.Customer)
                    .Include(i => i.Vehicle)
                    .Include(i => i.Inspector)
                    .Include(i => i.Issues)
                    .Include(i => i.Photos)
                    .ToListAsync();
                
                var inspectionDtos = pagedInspections.Select(MapToDto).ToList();
                
                return Ok(PagedResponse<VehicleInspectionDto>.CreateSuccessResult(
                    inspectionDtos, pageNumber, pageSize, totalCount, "Vehicle inspections retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, PagedResponse<VehicleInspectionDto>.CreateErrorResult("Lỗi khi lấy danh sách kiểm tra xe"));
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
                    return NotFound(ApiResponse<VehicleInspectionDto>.ErrorResult("Không tìm thấy kiểm tra xe"));
                }

                var inspectionDto = MapToDto(inspection);
                return Ok(ApiResponse<VehicleInspectionDto>.SuccessResult(inspectionDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<VehicleInspectionDto>.ErrorResult("Lỗi khi lấy thông tin kiểm tra xe", ex.Message));
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
                return StatusCode(500, ApiResponse<List<VehicleInspectionDto>>.ErrorResult("Lỗi khi lấy danh sách kiểm tra xe", ex.Message));
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
                return StatusCode(500, ApiResponse<List<VehicleInspectionDto>>.ErrorResult("Lỗi khi lấy danh sách kiểm tra xe", ex.Message));
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
                return StatusCode(500, ApiResponse<List<VehicleInspectionDto>>.ErrorResult("Lỗi khi lấy danh sách kiểm tra xe", ex.Message));
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
                return StatusCode(500, ApiResponse<VehicleInspectionDto>.ErrorResult("Lỗi khi lấy thông tin kiểm tra xe", ex.Message));
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
                    return BadRequest(ApiResponse<VehicleInspectionDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
                }

                // Business Rule: Kiểm tra xem có CustomerReceptionId không
                if (createDto.CustomerReceptionId.HasValue)
                {
                    var reception = await _unitOfWork.CustomerReceptions.GetByIdAsync(createDto.CustomerReceptionId.Value);
                    if (reception == null)
                    {
                        return BadRequest(ApiResponse<VehicleInspectionDto>.ErrorResult("Không tìm thấy phiếu tiếp đón"));
                    }

                    // Business Rule: Chỉ cho phép tạo kiểm tra từ CustomerReception đã được phân công kỹ thuật viên
                    if (reception.Status != ReceptionStatus.Assigned && reception.Status != ReceptionStatus.InProgress && reception.Status != ReceptionStatus.Pending)
                    {
                        return BadRequest(ApiResponse<VehicleInspectionDto>.ErrorResult(
                            $"Không thể tạo kiểm tra xe. Phiếu tiếp đón phải ở trạng thái 'Chờ Kiểm Tra', 'Đã Phân Công' hoặc 'Đang Kiểm Tra'. Trạng thái hiện tại: {reception.Status}"));
                    }

                    // Business Rule: Kiểm tra xem đã có kiểm tra cho CustomerReception này chưa
                    var existingInspection = await _unitOfWork.VehicleInspections.GetByCustomerReceptionIdAsync(createDto.CustomerReceptionId.Value);
                    if (existingInspection != null)
                    {
                        return BadRequest(ApiResponse<VehicleInspectionDto>.ErrorResult("Đã tồn tại kiểm tra xe cho phiếu tiếp đón này"));
                    }
                }

                // Validate vehicle and customer exist
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(createDto.VehicleId);
                if (vehicle == null)
                {
                    return BadRequest(ApiResponse<VehicleInspectionDto>.ErrorResult("Không tìm thấy xe"));
                }

                var customer = await _unitOfWork.Customers.GetByIdAsync(createDto.CustomerId);
                if (customer == null)
                {
                    return BadRequest(ApiResponse<VehicleInspectionDto>.ErrorResult("Không tìm thấy khách hàng"));
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

                // Business Rule: Cập nhật trạng thái CustomerReception thành "InProgress" nếu có
                if (createDto.CustomerReceptionId.HasValue)
                {
                    var reception = await _unitOfWork.CustomerReceptions.GetByIdAsync(createDto.CustomerReceptionId.Value);
                    if (reception != null)
                    {
                        reception.Status = ReceptionStatus.InProgress;
                        reception.UpdatedAt = DateTime.Now;
                        await _unitOfWork.CustomerReceptions.UpdateAsync(reception);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }

                // Reload with details
                inspection = await _unitOfWork.VehicleInspections.GetByIdWithDetailsAsync(inspection.Id);
                var inspectionDto = MapToDto(inspection!);

                return CreatedAtAction(nameof(GetInspection), new { id = inspection.Id }, 
                    ApiResponse<VehicleInspectionDto>.SuccessResult(inspectionDto, "Tạo kiểm tra xe thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<VehicleInspectionDto>.ErrorResult("Lỗi khi tạo kiểm tra xe", ex.Message));
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
                    return BadRequest(ApiResponse<VehicleInspectionDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
                }

                var inspection = await _unitOfWork.VehicleInspections.GetByIdAsync(id);
                if (inspection == null)
                {
                    return NotFound(ApiResponse<VehicleInspectionDto>.ErrorResult("Không tìm thấy kiểm tra xe"));
                }

                // ✅ SỬA: Dùng AutoMapper thay vì map tay
                _mapper.Map(updateDto, inspection);
                
                // ✅ GIỮ: Logic đặc biệt cho Status và CompletedDate
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
                    return NotFound(ApiResponse<VehicleInspectionDto>.ErrorResult("Không tìm thấy kiểm tra xe"));
                }

                inspection.Status = "Completed";
                inspection.CompletedDate = DateTime.Now;

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.VehicleInspections.UpdateAsync(inspection);
                    await _unitOfWork.SaveChangesAsync();

                    // Business Rule: Cập nhật trạng thái CustomerReception thành "Completed" nếu có
                    if (inspection.CustomerReceptionId.HasValue)
                    {
                        var reception = await _unitOfWork.CustomerReceptions.GetByIdAsync(inspection.CustomerReceptionId.Value);
                        if (reception != null)
                        {
                            reception.Status = ReceptionStatus.Completed;
                            reception.InspectionCompletedDate = DateTime.Now;
                            reception.UpdatedAt = DateTime.Now;
                            await _unitOfWork.CustomerReceptions.UpdateAsync(reception);
                            await _unitOfWork.SaveChangesAsync();
                        }
                    }

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

                // ✅ OPTIMIZED: Query ở database level thay vì load tất cả vào memory
                var inspections = await _context.VehicleInspections
                    .Where(i => !i.IsDeleted && i.Vehicle != null && i.Vehicle.VehicleType == vehicleType)
                    .Include(i => i.Customer)
                    .Include(i => i.Vehicle)
                    .Include(i => i.Issues)
                    .Include(i => i.Photos)
                    .ToListAsync();
                
                var inspectionDtos = inspections.Select(MapToDto).ToList();

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
                // ✅ OPTIMIZED: Query ở database level thay vì load tất cả vào memory
                var inspections = await _context.VehicleInspections
                    .Where(i => !i.IsDeleted && i.InspectionType == inspectionType)
                    .Include(i => i.Customer)
                    .Include(i => i.Vehicle)
                    .Include(i => i.Issues)
                    .Include(i => i.Photos)
                    .ToListAsync();
                
                var inspectionDtos = inspections.Select(MapToDto).ToList();

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

