using AutoMapper;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Core.Extensions;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using GarageManagementSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarageManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiScope")]
    public class EmployeesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public EmployeesController(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<EmployeeDto>>> GetEmployees(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? department = null)
        {
            try
            {
                var employees = await _unitOfWork.Employees.GetAllWithNavigationAsync();
                var query = employees.AsQueryable();
                
                // Apply search filter if provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(e => 
                        e.Name.Contains(searchTerm) || 
                        e.Email.Contains(searchTerm) || 
                        e.Phone.Contains(searchTerm));
                }
                
                // Apply department filter if provided
                if (!string.IsNullOrEmpty(department))
                {
                    query = query.Where(e => e.Department == department);
                }

                // Get total count
                var totalCount = await query.GetTotalCountAsync();
                
                // Apply pagination
                var pagedEmployees = query.ApplyPagination(pageNumber, pageSize).ToList();
                var employeeDtos = pagedEmployees.Select(e => _mapper.Map<EmployeeDto>(e)).ToList();
                
                return Ok(PagedResponse<EmployeeDto>.CreateSuccessResult(
                    employeeDtos, pageNumber, pageSize, totalCount, "Employees retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, PagedResponse<EmployeeDto>.CreateErrorResult("Error retrieving employees"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<EmployeeDto>>> GetEmployee(int id)
        {
            try
            {
                var employee = await _unitOfWork.Employees.GetByIdWithNavigationAsync(id);
                if (employee == null)
                {
                    return NotFound(ApiResponse<EmployeeDto>.ErrorResult("Employee not found"));
                }

                var employeeDto = _mapper.Map<EmployeeDto>(employee);
                return Ok(ApiResponse<EmployeeDto>.SuccessResult(employeeDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<EmployeeDto>.ErrorResult("Error retrieving employee", ex.Message));
            }
        }

        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<List<EmployeeDto>>>> GetActiveEmployees()
        {
            try
            {
                var employees = await _unitOfWork.Employees.GetAllWithNavigationAsync();
                var activeEmployees = employees.Where(e => e.Status == "Active").Select(e => _mapper.Map<EmployeeDto>(e)).ToList();
                
                return Ok(ApiResponse<List<EmployeeDto>>.SuccessResult(activeEmployees));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<EmployeeDto>>.ErrorResult("Error retrieving employees", ex.Message));
            }
        }

        [HttpGet("department/{department}")]
        public async Task<ActionResult<ApiResponse<List<EmployeeDto>>>> GetEmployeesByDepartment(string department)
        {
            try
            {
                var employees = await _unitOfWork.Employees.GetAllAsync();
                var deptEmployees = employees
                    .Where(e => e.Department != null && e.Department.Equals(department, StringComparison.OrdinalIgnoreCase))
                    .Select(e => _mapper.Map<EmployeeDto>(e))
                    .ToList();
                
                return Ok(ApiResponse<List<EmployeeDto>>.SuccessResult(deptEmployees));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<EmployeeDto>>.ErrorResult("Error retrieving employees", ex.Message));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<EmployeeDto>>> CreateEmployee(CreateEmployeeDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<EmployeeDto>.ErrorResult("Invalid data", errors));
                }

                // Use AutoMapper to map DTO to Entity
                var employee = _mapper.Map<Core.Entities.Employee>(createDto);
                employee.Status = "Active";
                employee.HireDate = createDto.HireDate ?? DateTime.Now;

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.Employees.AddAsync(employee);
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

                var employeeDto = _mapper.Map<EmployeeDto>(employee);
                return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, 
                    ApiResponse<EmployeeDto>.SuccessResult(employeeDto, "Employee created successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<EmployeeDto>.ErrorResult("Error creating employee", ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<EmployeeDto>>> UpdateEmployee(int id, UpdateEmployeeDto updateDto)
        {
            try
            {
                if (id != updateDto.Id)
                {
                    return BadRequest(ApiResponse<EmployeeDto>.ErrorResult("ID mismatch"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<EmployeeDto>.ErrorResult("Invalid data", errors));
                }

                var employee = await _unitOfWork.Employees.GetByIdAsync(id);
                if (employee == null)
                {
                    return NotFound(ApiResponse<EmployeeDto>.ErrorResult("Employee not found"));
                }

                // Use AutoMapper to map DTO to existing Entity
                _mapper.Map(updateDto, employee);

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.Employees.UpdateAsync(employee);
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

                var employeeDto = _mapper.Map<EmployeeDto>(employee);
                return Ok(ApiResponse<EmployeeDto>.SuccessResult(employeeDto, "Employee updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<EmployeeDto>.ErrorResult("Error updating employee", ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteEmployee(int id)
        {
            try
            {
                var employee = await _unitOfWork.Employees.GetByIdAsync(id);
                if (employee == null)
                {
                    return NotFound(ApiResponse.ErrorResult("Employee not found"));
                }

                await _unitOfWork.Employees.DeleteAsync(employee);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse.SuccessResult("Employee deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResult("Error deleting employee", ex.Message));
            }
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult<ApiResponse<EmployeeDto>>> UpdateEmployeeStatus(int id, [FromBody] string status)
        {
            try
            {
                var employee = await _unitOfWork.Employees.GetByIdAsync(id);
                if (employee == null)
                {
                    return NotFound(ApiResponse<EmployeeDto>.ErrorResult("Employee not found"));
                }

                employee.Status = status;

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.Employees.UpdateAsync(employee);
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

                var employeeDto = _mapper.Map<EmployeeDto>(employee);
                return Ok(ApiResponse<EmployeeDto>.SuccessResult(employeeDto, "Employee status updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<EmployeeDto>.ErrorResult("Error updating employee status", ex.Message));
            }
        }

        [HttpGet("departments")]
        public ActionResult<ApiResponse<List<object>>> GetDepartments()
        {
            try
            {
                var departments = new[]
                {
                    new { id = "Kỹ Thuật", text = "Kỹ Thuật" },
                    new { id = "Dịch Vụ", text = "Dịch Vụ" },
                    new { id = "Kho", text = "Kho" },
                    new { id = "Kế Toán", text = "Kế Toán" },
                    new { id = "Hành Chính", text = "Hành Chính" }
                }.ToList<object>();

                return Ok(ApiResponse<List<object>>.SuccessResult(departments));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<object>>.ErrorResult("Error retrieving departments", ex.Message));
            }
        }

        [HttpGet("positions")]
        public ActionResult<ApiResponse<List<object>>> GetPositions()
        {
            try
            {
                var positions = new[]
                {
                    new { id = "Thợ Chính", text = "Thợ Chính" },
                    new { id = "Thợ Phụ", text = "Thợ Phụ" },
                    new { id = "Tư Vấn Dịch Vụ", text = "Tư Vấn Dịch Vụ" },
                    new { id = "Kiểm Tra Viên", text = "Kiểm Tra Viên" },
                    new { id = "Thủ Kho", text = "Thủ Kho" },
                    new { id = "Kế Toán", text = "Kế Toán" },
                    new { id = "Quản Lý", text = "Quản Lý" }
                }.ToList<object>();

                return Ok(ApiResponse<List<object>>.SuccessResult(positions));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<object>>.ErrorResult("Error retrieving positions", ex.Message));
            }
        }

        /// <summary>
        /// Get employee performance metrics
        /// </summary>
        [HttpGet("{id}/performance")]
        public async Task<ActionResult<ApiResponse<object>>> GetEmployeePerformance(int id, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var employee = await _unitOfWork.Employees.GetByIdAsync(id);
                if (employee == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Employee not found"));
                }

                // Default date range: last 30 days
                startDate ??= DateTime.Now.AddDays(-30);
                endDate ??= DateTime.Now;

                // Get all service orders handled by this employee
                var serviceOrders = await _unitOfWork.ServiceOrders.FindAsync(so =>
                    !so.IsDeleted &&
                    so.OrderDate >= startDate &&
                    so.OrderDate <= endDate);

                // For now, we'll use all service orders in the date range
                // TODO: Add ServiceOrderLabors repository to IUnitOfWork to filter by employee
                var employeeServiceOrders = serviceOrders.ToList();
                var totalLaborHours = 0m;
                var totalLaborCost = 0m;

                // Get invoices for these service orders
                var serviceOrderIds = employeeServiceOrders.Select(so => so.Id).ToList();
                var invoices = await _unitOfWork.Invoices.FindAsync(inv =>
                    inv.ServiceOrderId.HasValue &&
                    serviceOrderIds.Contains(inv.ServiceOrderId.Value) &&
                    !inv.IsDeleted);

                // Calculate metrics
                var totalOrders = employeeServiceOrders.Count;
                var completedOrders = employeeServiceOrders.Count(so => so.Status == "Completed");
                var totalRevenue = invoices.Sum(i => i.TotalAmount);

                // Calculate average completion time for completed orders
                var completedOrdersWithDates = employeeServiceOrders
                    .Where(so => so.Status == "Completed" && so.CompletedDate.HasValue && so.StartDate.HasValue)
                    .ToList();

                double? avgCompletionDays = null;
                if (completedOrdersWithDates.Any())
                {
                    avgCompletionDays = completedOrdersWithDates
                        .Average(so => (so.CompletedDate!.Value - so.StartDate!.Value).TotalDays);
                }

                // Get recent orders
                var recentOrders = employeeServiceOrders
                    .OrderByDescending(so => so.OrderDate)
                    .Take(10)
                    .Select(so => new
                    {
                        so.Id,
                        so.OrderNumber,
                        so.OrderDate,
                        so.Status,
                        so.TotalAmount,
                        so.CompletedDate
                    })
                    .ToList();

                var performance = new
                {
                    Employee = new
                    {
                        employee.Id,
                        employee.Name,
                        employee.Email,
                        employee.Phone,
                        employee.DepartmentId,
                        employee.PositionId
                    },
                    DateRange = new
                    {
                        StartDate = startDate,
                        EndDate = endDate
                    },
                    Metrics = new
                    {
                        TotalServiceOrders = totalOrders,
                        CompletedOrders = completedOrders,
                        PendingOrders = totalOrders - completedOrders,
                        CompletionRate = totalOrders > 0 ? (decimal)completedOrders / totalOrders * 100 : 0,
                        TotalRevenue = totalRevenue,
                        TotalLaborHours = totalLaborHours,
                        TotalLaborCost = totalLaborCost,
                        AverageCompletionDays = avgCompletionDays,
                        RevenuePerOrder = totalOrders > 0 ? totalRevenue / totalOrders : 0
                    },
                    RecentOrders = recentOrders
                };

                return Ok(ApiResponse<object>.SuccessResult(performance));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult("Error retrieving employee performance", ex.Message));
            }
        }

        /// <summary>
        /// ✅ BỔ SUNG: Get employee workload (total assigned hours, active orders)
        /// </summary>
        [HttpGet("{id}/workload")]
        public async Task<ActionResult<ApiResponse<object>>> GetEmployeeWorkload(int id, [FromQuery] DateTime? date = null)
        {
            try
            {
                var employee = await _unitOfWork.Employees.GetByIdAsync(id);
                if (employee == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Employee not found"));
                }

                // Default to today if not specified
                date ??= DateTime.Today;

                // ✅ OPTIMIZED: Filter assigned items ở database level
                // Note: Filter theo AssignedTechnicianId first, then filter by ServiceOrder properties in memory
                // (Vì navigation properties không thể filter trực tiếp trong FindAsync)
                var allAssignedItems = (await _unitOfWork.Repository<Core.Entities.ServiceOrderItem>()
                    .FindAsync(item => item.AssignedTechnicianId == id)).ToList();
                
                // Filter by ServiceOrder properties after loading (for navigation properties)
                var assignedItems = allAssignedItems
                    .Where(item => item.ServiceOrder != null &&
                                   !item.ServiceOrder.IsDeleted &&
                                   item.ServiceOrder.Status != "Cancelled" &&
                                   item.ServiceOrder.Status != "Completed")
                    .ToList();

                // Calculate total estimated hours
                var totalEstimatedHours = assignedItems
                    .Where(i => i.EstimatedHours.HasValue)
                    .Sum(i => i.EstimatedHours!.Value);

                // Get items assigned today
                var todayItems = assignedItems.Where(item =>
                    item.UpdatedAt.HasValue && item.UpdatedAt.Value.Date == date.Value.Date)
                    .ToList();
                var todayEstimatedHours = todayItems
                    .Where(i => i.EstimatedHours.HasValue)
                    .Sum(i => i.EstimatedHours!.Value);

                // Get active service orders (unique order IDs)
                var activeOrderIds = assignedItems
                    .Select(i => i.ServiceOrderId)
                    .Distinct()
                    .ToList();

                // Get ServiceOrders for these IDs
                var activeOrders = (await _unitOfWork.ServiceOrders.GetAllWithDetailsAsync())
                    .Where(so => activeOrderIds.Contains(so.Id) && 
                                 !so.IsDeleted && 
                                 so.Status != "Cancelled" && 
                                 so.Status != "Completed")
                    .ToList();

                // Get completed items (for historical context)
                var completedItems = allAssignedItems.Where(item =>
                    item.AssignedTechnicianId == id &&
                    item.ServiceOrder != null &&
                    item.ServiceOrder.Status == "Completed")
                    .ToList();
                var completedOrdersCount = completedItems
                    .Select(i => i.ServiceOrderId)
                    .Distinct()
                    .Count();

                var workload = new
                {
                    Employee = new
                    {
                        employee.Id,
                        employee.Name,
                        employee.Position,
                        PositionName = employee.PositionNavigation?.Name
                    },
                    Date = date,
                    ActiveOrders = new
                    {
                        Count = activeOrders.Count,
                        TotalEstimatedHours = totalEstimatedHours,
                        Items = activeOrders.Select(so => new
                        {
                            so.Id,
                            so.OrderNumber,
                            so.Status,
                            so.ScheduledDate,
                            ItemsCount = assignedItems.Count(i => i.ServiceOrderId == so.Id),
                            ItemsEstimatedHours = assignedItems
                                .Where(i => i.ServiceOrderId == so.Id && i.EstimatedHours.HasValue)
                                .Sum(i => i.EstimatedHours!.Value)
                        }).ToList()
                    },
                    Today = new
                    {
                        AssignedItemsCount = todayItems.Count,
                        EstimatedHours = todayEstimatedHours
                    },
                    Statistics = new
                    {
                        TotalActiveItems = assignedItems.Count,
                        TotalCompletedOrders = completedOrdersCount,
                        CapacityUsed = totalEstimatedHours > 0 ? Math.Min(100, (totalEstimatedHours / 8.0m) * 100) : 0 // Assuming 8 hours/day capacity
                    }
                };

                return Ok(ApiResponse<object>.SuccessResult(workload));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult("Error retrieving workload", ex.Message));
            }
        }

    }
}

