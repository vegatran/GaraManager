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
    public class EmployeesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public EmployeesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<EmployeeDto>>>> GetEmployees()
        {
            try
            {
                var employees = await _unitOfWork.Employees.GetAllAsync();
                var employeeDtos = await MapToDtoWithNavigation(employees.Where(e => !e.IsDeleted).ToList());
                
                return Ok(ApiResponse<List<EmployeeDto>>.SuccessResult(employeeDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<EmployeeDto>>.ErrorResult("Error retrieving employees", ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<EmployeeDto>>> GetEmployee(int id)
        {
            try
            {
                var employee = await _unitOfWork.Employees.GetByIdAsync(id);
                if (employee == null)
                {
                    return NotFound(ApiResponse<EmployeeDto>.ErrorResult("Employee not found"));
                }

                var employeeDto = MapToDto(employee);
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
                var employees = await _unitOfWork.Employees.GetAllAsync();
                var activeEmployees = employees.Where(e => e.Status == "Active").Select(MapToDto).ToList();
                
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
                    .Select(MapToDto)
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

                var employee = new Core.Entities.Employee
                {
                    Name = createDto.Name,
                    Phone = createDto.Phone,
                    Email = createDto.Email,
                    Address = createDto.Address,
                    Position = createDto.Position,
                    Department = createDto.Department,
                    HireDate = createDto.HireDate ?? DateTime.Now,
                    Salary = createDto.Salary,
                    Status = "Active",
                    Skills = createDto.Skills
                };

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

                var employeeDto = MapToDto(employee);
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

                employee.Name = updateDto.Name;
                employee.Phone = updateDto.Phone;
                employee.Email = updateDto.Email;
                employee.Address = updateDto.Address;
                employee.Position = updateDto.Position;
                employee.Department = updateDto.Department;
                employee.HireDate = updateDto.HireDate;
                employee.Salary = updateDto.Salary;
                employee.Status = updateDto.Status;
                employee.Skills = updateDto.Skills;

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

                var employeeDto = MapToDto(employee);
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

                var employeeDto = MapToDto(employee);
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

        private async Task<List<EmployeeDto>> MapToDtoWithNavigation(List<Core.Entities.Employee> employees)
        {
            var employeeDtos = new List<EmployeeDto>();
            
            foreach (var employee in employees)
            {
                var department = employee.DepartmentId.HasValue 
                    ? await _unitOfWork.Departments.GetByIdAsync(employee.DepartmentId.Value) 
                    : null;
                    
                var position = employee.PositionId.HasValue 
                    ? await _unitOfWork.Positions.GetByIdAsync(employee.PositionId.Value) 
                    : null;

                employeeDtos.Add(new EmployeeDto
                {
                    Id = employee.Id,
                    Name = employee.Name,
                    Phone = employee.Phone,
                    Email = employee.Email,
                    Address = employee.Address,
                    Position = employee.Position,
                    Department = employee.Department,
                    PositionId = employee.PositionId,
                    DepartmentId = employee.DepartmentId,
                    PositionName = position?.Name,
                    DepartmentName = department?.Name,
                    HireDate = employee.HireDate,
                    Salary = employee.Salary,
                    Status = employee.Status,
                    Skills = employee.Skills,
                    CreatedAt = employee.CreatedAt,
                    CreatedBy = employee.CreatedBy,
                    UpdatedAt = employee.UpdatedAt,
                    UpdatedBy = employee.UpdatedBy
                });
            }
            
            return employeeDtos;
        }

        private static EmployeeDto MapToDto(Core.Entities.Employee employee)
        {
            return new EmployeeDto
            {
                Id = employee.Id,
                Name = employee.Name,
                Phone = employee.Phone,
                Email = employee.Email,
                Address = employee.Address,
                Position = employee.Position,
                Department = employee.Department,
                PositionId = employee.PositionId,
                DepartmentId = employee.DepartmentId,
                PositionName = employee.PositionNavigation?.Name,
                DepartmentName = employee.DepartmentNavigation?.Name,
                HireDate = employee.HireDate,
                Salary = employee.Salary,
                Status = employee.Status,
                Skills = employee.Skills,
                CreatedAt = employee.CreatedAt,
                CreatedBy = employee.CreatedBy,
                UpdatedAt = employee.UpdatedAt,
                UpdatedBy = employee.UpdatedBy
            };
        }
    }
}

