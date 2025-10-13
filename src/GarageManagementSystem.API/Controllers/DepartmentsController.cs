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
    public class DepartmentsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DepartmentsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<DepartmentDto>>>> GetDepartments()
        {
            try
            {
                var departments = await _unitOfWork.Departments.GetAllAsync();
                var departmentDtos = departments.Where(d => !d.IsDeleted).Select(d => _mapper.Map<DepartmentDto>(d)).ToList();
                
                return Ok(ApiResponse<List<DepartmentDto>>.SuccessResult(departmentDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<DepartmentDto>>.ErrorResult("Error retrieving departments", ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<DepartmentDto>>> GetDepartment(int id)
        {
            try
            {
                var department = await _unitOfWork.Departments.GetByIdAsync(id);
                if (department == null)
                {
                    return NotFound(ApiResponse<DepartmentDto>.ErrorResult("Department not found"));
                }

                var departmentDto = _mapper.Map<DepartmentDto>(department);
                return Ok(ApiResponse<DepartmentDto>.SuccessResult(departmentDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<DepartmentDto>.ErrorResult("Error retrieving department", ex.Message));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<DepartmentDto>>> CreateDepartment(CreateDepartmentDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<DepartmentDto>.ErrorResult("Invalid data", errors));
                }

                // Use AutoMapper to map DTO to Entity
                var department = _mapper.Map<Core.Entities.Department>(createDto);

                // Begin transaction
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.Departments.AddAsync(department);
                    await _unitOfWork.SaveChangesAsync();

                    // Commit transaction
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch
                {
                    // Rollback transaction
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }

                var departmentDto = _mapper.Map<DepartmentDto>(department);
                return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, 
                    ApiResponse<DepartmentDto>.SuccessResult(departmentDto, "Department created successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<DepartmentDto>.ErrorResult("Error creating department", ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<DepartmentDto>>> UpdateDepartment(int id, UpdateDepartmentDto updateDto)
        {
            try
            {
                if (id != updateDto.Id)
                {
                    return BadRequest(ApiResponse<DepartmentDto>.ErrorResult("ID mismatch"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<DepartmentDto>.ErrorResult("Invalid data", errors));
                }

                var department = await _unitOfWork.Departments.GetByIdAsync(id);
                if (department == null)
                {
                    return NotFound(ApiResponse<DepartmentDto>.ErrorResult("Department not found"));
                }

                // Use AutoMapper to map DTO to existing Entity
                _mapper.Map(updateDto, department);

                // Begin transaction
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.Departments.UpdateAsync(department);
                    await _unitOfWork.SaveChangesAsync();

                    // Commit transaction
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch
                {
                    // Rollback transaction
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }

                var departmentDto = _mapper.Map<DepartmentDto>(department);
                return Ok(ApiResponse<DepartmentDto>.SuccessResult(departmentDto, "Department updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<DepartmentDto>.ErrorResult("Error updating department", ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteDepartment(int id)
        {
            try
            {
                var department = await _unitOfWork.Departments.GetByIdAsync(id);
                if (department == null)
                {
                    return NotFound(ApiResponse.ErrorResult("Department not found"));
                }

                await _unitOfWork.Departments.DeleteAsync(department);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse.SuccessResult("Department deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResult("Error deleting department", ex.Message));
            }
        }
    }
}