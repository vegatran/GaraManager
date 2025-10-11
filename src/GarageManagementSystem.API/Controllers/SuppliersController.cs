using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarageManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize(Policy = "ApiScope")] // Tạm thời bỏ để test
    
    public class SuppliersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public SuppliersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<SupplierDto>>>> GetSuppliers()
        {
            try
            {
                // Debug: Log user claims
                Console.WriteLine("🔍 User Claims Debug:");
                if (User?.Identity?.IsAuthenticated == true)
                {
                    Console.WriteLine($"✅ User is authenticated: {User.Identity.Name}");
                    foreach (var claim in User.Claims)
                    {
                        Console.WriteLine($"  {claim.Type}: {claim.Value}");
                    }
                }
                else
                {
                    Console.WriteLine("❌ User is NOT authenticated");
                }

                var suppliers = await _unitOfWork.Suppliers.GetAllAsync();
                return Ok(ApiResponse<List<SupplierDto>>.SuccessResult(suppliers.Select(MapToDto).ToList()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<SupplierDto>>.ErrorResult("Error", ex.Message));
            }
        }

        /// <summary>
        /// Debug endpoint để kiểm tra token claims
        /// </summary>
        [HttpGet("debug-token")]
        public IActionResult DebugToken()
        {
            var claims = new List<object>();
            
            if (User?.Identity?.IsAuthenticated == true)
            {
                foreach (var claim in User.Claims)
                {
                    claims.Add(new { Type = claim.Type, Value = claim.Value });
                }
                
                return Ok(new { 
                    IsAuthenticated = true, 
                    UserName = User.Identity.Name,
                    Claims = claims 
                });
            }
            else
            {
                return Ok(new { 
                    IsAuthenticated = false, 
                    Message = "User is not authenticated" 
                });
            }
        }

        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<List<SupplierDto>>>> GetActiveSuppliers()
        {
            try
            {
                var suppliers = await _unitOfWork.Suppliers.GetActiveSuppliersAsync();
                return Ok(ApiResponse<List<SupplierDto>>.SuccessResult(suppliers.Select(MapToDto).ToList()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<SupplierDto>>.ErrorResult("Error", ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<SupplierDto>>> GetSupplier(int id)
        {
            try
            {
                var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
                if (supplier == null) return NotFound(ApiResponse<SupplierDto>.ErrorResult("Not found"));
                return Ok(ApiResponse<SupplierDto>.SuccessResult(MapToDto(supplier)));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<SupplierDto>.ErrorResult("Error", ex.Message));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<SupplierDto>>> CreateSupplier(CreateSupplierDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<SupplierDto>.ErrorResult("Invalid data", errors));
                }

                var supplier = new Core.Entities.Supplier
                {
                    SupplierCode = dto.SupplierCode,
                    SupplierName = dto.SupplierName,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    Address = dto.Address,
                    ContactPerson = dto.ContactPerson,
                    ContactPhone = dto.ContactPhone,
                    TaxCode = dto.TaxCode,
                    BankAccount = dto.BankAccount,
                    BankName = dto.BankName,
                    Notes = dto.Notes,
                    IsActive = dto.IsActive,
                    Rating = dto.Rating
                };

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.Suppliers.AddAsync(supplier);
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
                
                Console.WriteLine($"✅ Supplier created: {supplier.SupplierName} (ID: {supplier.Id})");
                return CreatedAtAction(nameof(GetSupplier), new { id = supplier.Id }, 
                    ApiResponse<SupplierDto>.SuccessResult(MapToDto(supplier), "Supplier created successfully"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating supplier: {ex}");
                return StatusCode(500, ApiResponse<SupplierDto>.ErrorResult("Error creating supplier", ex, includeStackTrace: true));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<SupplierDto>>> UpdateSupplier(int id, UpdateSupplierDto dto)
        {
            try
            {
                if (id != dto.Id) return BadRequest(ApiResponse<SupplierDto>.ErrorResult("ID mismatch"));
                var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
                if (supplier == null) return NotFound(ApiResponse<SupplierDto>.ErrorResult("Not found"));

                supplier.SupplierCode = dto.SupplierCode;
                supplier.SupplierName = dto.SupplierName;
                supplier.Phone = dto.Phone;
                supplier.Email = dto.Email;
                supplier.Address = dto.Address;
                supplier.ContactPerson = dto.ContactPerson;
                supplier.ContactPhone = dto.ContactPhone;
                supplier.TaxCode = dto.TaxCode;
                supplier.BankAccount = dto.BankAccount;
                supplier.BankName = dto.BankName;
                supplier.Notes = dto.Notes;
                supplier.IsActive = dto.IsActive;
                supplier.Rating = dto.Rating;

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.Suppliers.UpdateAsync(supplier);
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
                return Ok(ApiResponse<SupplierDto>.SuccessResult(MapToDto(supplier)));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<SupplierDto>.ErrorResult("Error", ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteSupplier(int id)
        {
            try
            {
                var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
                if (supplier == null) return NotFound(ApiResponse.ErrorResult("Not found"));
                await _unitOfWork.Suppliers.DeleteAsync(supplier);
                await _unitOfWork.SaveChangesAsync();
                return Ok(ApiResponse.SuccessResult("Deleted"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResult("Error", ex.Message));
            }
        }

        private static SupplierDto MapToDto(Core.Entities.Supplier s) => new()
        {
            Id = s.Id, SupplierCode = s.SupplierCode, SupplierName = s.SupplierName,
            Phone = s.Phone, Email = s.Email, Address = s.Address,
            ContactPerson = s.ContactPerson, ContactPhone = s.ContactPhone,
            TaxCode = s.TaxCode, BankAccount = s.BankAccount, BankName = s.BankName,
            Notes = s.Notes, IsActive = s.IsActive, Rating = s.Rating,
            CreatedAt = s.CreatedAt, CreatedBy = s.CreatedBy,
            UpdatedAt = s.UpdatedAt, UpdatedBy = s.UpdatedBy
        };
    }
}

