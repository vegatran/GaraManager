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
    public class EmployeesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly GarageDbContext _context;

        public EmployeesController(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService, GarageDbContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
            _context = context;
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
                // ✅ OPTIMIZED: Query ở database level thay vì load tất cả vào memory
                var query = _context.Employees
                    .Where(e => !e.IsDeleted)
                    .AsQueryable();
                
                // Apply search filter if provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(e => 
                        (e.Name != null && e.Name.Contains(searchTerm)) || 
                        (e.Email != null && e.Email.Contains(searchTerm)) || 
                        (e.Phone != null && e.Phone.Contains(searchTerm)));
                }
                
                // Apply department filter if provided
                if (!string.IsNullOrEmpty(department))
                {
                    query = query.Where(e => e.Department == department);
                }

                // ✅ OPTIMIZED: Apply Include before pagination
                query = query
                    .Include(e => e.PositionNavigation)
                    .Include(e => e.DepartmentNavigation);
                
                // ✅ OPTIMIZED: Get paged results with total count - automatically chooses best method
                var (pagedEmployees, totalCount) = await query.ToPagedListWithCountAsync(pageNumber, pageSize, _context);
                
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
                // ✅ FIX: Sử dụng FindAsync thay vì GetAllAsync().Where() để filter ở database level
                var employees = await _unitOfWork.Employees.FindAsync(e => e.Status == "Active");
                var activeEmployees = employees.Select(e => _mapper.Map<EmployeeDto>(e)).ToList();
                
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
                // ✅ FIX: Sử dụng FindAsync thay vì GetAllAsync().Where() để filter ở database level
                var employees = await _unitOfWork.Employees.FindAsync(e => 
                    e.Department != null && e.Department.Equals(department, StringComparison.OrdinalIgnoreCase));
                var deptEmployees = employees
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
        /// ✅ HP2: Cached với TTL 5 phút để giảm số lượng API calls
        /// </summary>
        [HttpGet("{id}/workload")]
        public async Task<ActionResult<ApiResponse<object>>> GetEmployeeWorkload(int id, [FromQuery] DateTime? date = null)
        {
            try
            {
                // ✅ FIX: Load employee với navigation properties để tránh null reference
                var employee = await _unitOfWork.Employees.GetByIdWithNavigationAsync(id);
                if (employee == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Employee not found"));
                }

                // Default to today if not specified
                date ??= DateTime.Today;

                // ✅ HP2: Check cache first
                var cacheKey = $"workload_{id}_{date.Value:yyyyMMdd}";
                var cachedWorkload = await _cacheService.GetAsync<object>(cacheKey);
                if (cachedWorkload != null)
                {
                    // ✅ FIX: Cast về đúng type (anonymous object được preserve trong IMemoryCache)
                    // IMemoryCache lưu trữ object reference trực tiếp, không serialize/deserialize
                    // Nên có thể cast về object mà không mất thông tin
                    return Ok(ApiResponse<object>.SuccessResult(cachedWorkload));
                }

                // ✅ FIX: Load ServiceOrderItems và filter ở database level
                var allAssignedItems = (await _unitOfWork.Repository<Core.Entities.ServiceOrderItem>()
                    .FindAsync(item => item.AssignedTechnicianId == id)).ToList();
                
                // ✅ FIX: Load ServiceOrders để filter và tránh null reference
                var serviceOrderIds = allAssignedItems.Select(i => i.ServiceOrderId).Distinct().ToList();
                var serviceOrdersDict = serviceOrderIds.Any()
                    ? (await _unitOfWork.ServiceOrders
                        .FindAsync(so => serviceOrderIds.Contains(so.Id)))
                        .ToDictionary(so => so.Id)
                    : new Dictionary<int, Core.Entities.ServiceOrder>();
                
                // Filter items by ServiceOrder properties (sử dụng dictionary để tránh null reference)
                var assignedItems = allAssignedItems
                    .Where(item => serviceOrdersDict.TryGetValue(item.ServiceOrderId, out var so) &&
                                   so != null &&
                                   !so.IsDeleted &&
                                   so.Status != "Cancelled" &&
                                   so.Status != "Completed")
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

                // ✅ FIX: Filter ở database level thay vì load tất cả rồi filter trong memory
                // Chỉ load ServiceOrders cần thiết để tránh tốn memory
                var activeOrders = activeOrderIds.Any()
                    ? (await _unitOfWork.ServiceOrders
                        .FindAsync(so => activeOrderIds.Contains(so.Id) && 
                                         !so.IsDeleted && 
                                         so.Status != "Cancelled" && 
                                         so.Status != "Completed")).ToList()
                    : new List<Core.Entities.ServiceOrder>();

                // Get completed items (for historical context) - sử dụng dictionary để tránh null reference
                var completedItems = allAssignedItems.Where(item =>
                    item.AssignedTechnicianId == id &&
                    serviceOrdersDict.TryGetValue(item.ServiceOrderId, out var so) &&
                    so != null &&
                    so.Status == "Completed")
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

                // ✅ HP2: Cache workload data với TTL 5 phút
                await _cacheService.SetAsync(cacheKey, workload, TimeSpan.FromMinutes(5));

                return Ok(ApiResponse<object>.SuccessResult(workload));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult("Error retrieving workload", ex.Message));
            }
        }

    }
}

