using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace GarageManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<CustomerDto>>>> GetCustomers()
        {
            try
            {
                var customers = await _unitOfWork.Customers.GetAllAsync();
                var customerDtos = customers.Select(MapToDto).ToList();
                
                return Ok(ApiResponse<List<CustomerDto>>.SuccessResult(customerDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<CustomerDto>>.ErrorResult("Lỗi khi lấy danh sách khách hàng", ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CustomerDto>>> GetCustomer(int id)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(id);
                if (customer == null)
                {
                    return NotFound(ApiResponse<CustomerDto>.ErrorResult("Không tìm thấy khách hàng"));
                }

                var customerDto = MapToDto(customer);
                return Ok(ApiResponse<CustomerDto>.SuccessResult(customerDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CustomerDto>.ErrorResult("Lỗi khi lấy thông tin khách hàng", ex.Message));
            }
        }

        [HttpGet("{id}/vehicles")]
        public async Task<ActionResult<ApiResponse<List<VehicleDto>>>> GetCustomerVehicles(int id)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetCustomerWithVehiclesAsync(id);
                if (customer == null)
                {
                    return NotFound(ApiResponse<List<VehicleDto>>.ErrorResult("Không tìm thấy khách hàng"));
                }

                var vehicleDtos = customer.Vehicles.Select(MapVehicleToDto).ToList();
                return Ok(ApiResponse<List<VehicleDto>>.SuccessResult(vehicleDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<VehicleDto>>.ErrorResult("Lỗi khi lấy danh sách xe của khách hàng", ex.Message));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CustomerDto>>> CreateCustomer(CreateCustomerDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<CustomerDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
                }

                // Check if phone number already exists
                if (!string.IsNullOrEmpty(createDto.PhoneNumber) && 
                    await _unitOfWork.Customers.IsPhoneNumberExistsAsync(createDto.PhoneNumber))
                {
                    return BadRequest(ApiResponse<CustomerDto>.ErrorResult("Số điện thoại đã tồn tại"));
                }

                // Check if email already exists
                if (!string.IsNullOrEmpty(createDto.Email) && 
                    await _unitOfWork.Customers.IsEmailExistsAsync(createDto.Email))
                {
                    return BadRequest(ApiResponse<CustomerDto>.ErrorResult("Email đã tồn tại"));
                }

                var customer = new Core.Entities.Customer
                {
                    Name = createDto.Name,
                    PhoneNumber = createDto.PhoneNumber,
                    Email = createDto.Email,
                    Address = createDto.Address,
                    DateOfBirth = createDto.DateOfBirth,
                    Gender = createDto.Gender
                };

                await _unitOfWork.Customers.AddAsync(customer);
                await _unitOfWork.SaveChangesAsync();

                var customerDto = MapToDto(customer);
                return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, 
                    ApiResponse<CustomerDto>.SuccessResult(customerDto, "Tạo khách hàng thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CustomerDto>.ErrorResult("Lỗi khi tạo khách hàng", ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<CustomerDto>>> UpdateCustomer(int id, UpdateCustomerDto updateDto)
        {
            try
            {
                if (id != updateDto.Id)
                {
                    return BadRequest(ApiResponse<CustomerDto>.ErrorResult("ID không khớp"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<CustomerDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
                }

                var customer = await _unitOfWork.Customers.GetByIdAsync(id);
                if (customer == null)
                {
                    return NotFound(ApiResponse<CustomerDto>.ErrorResult("Không tìm thấy khách hàng"));
                }

                // Check if phone number already exists (excluding current customer)
                if (!string.IsNullOrEmpty(updateDto.PhoneNumber) && 
                    await _unitOfWork.Customers.IsPhoneNumberExistsAsync(updateDto.PhoneNumber, id))
                {
                    return BadRequest(ApiResponse<CustomerDto>.ErrorResult("Số điện thoại đã tồn tại"));
                }

                // Check if email already exists (excluding current customer)
                if (!string.IsNullOrEmpty(updateDto.Email) && 
                    await _unitOfWork.Customers.IsEmailExistsAsync(updateDto.Email, id))
                {
                    return BadRequest(ApiResponse<CustomerDto>.ErrorResult("Email đã tồn tại"));
                }

                customer.Name = updateDto.Name;
                customer.PhoneNumber = updateDto.PhoneNumber;
                customer.Email = updateDto.Email;
                customer.Address = updateDto.Address;
                customer.DateOfBirth = updateDto.DateOfBirth;
                customer.Gender = updateDto.Gender;

                await _unitOfWork.Customers.UpdateAsync(customer);
                await _unitOfWork.SaveChangesAsync();

                var customerDto = MapToDto(customer);
                return Ok(ApiResponse<CustomerDto>.SuccessResult(customerDto, "Cập nhật khách hàng thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CustomerDto>.ErrorResult("Lỗi khi cập nhật khách hàng", ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteCustomer(int id)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(id);
                if (customer == null)
                {
                    return NotFound(ApiResponse.ErrorResult("Không tìm thấy khách hàng"));
                }

                await _unitOfWork.Customers.DeleteAsync(customer);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse.SuccessResult("Xóa khách hàng thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResult("Lỗi khi xóa khách hàng", ex.Message));
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<List<CustomerDto>>>> SearchCustomers([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest(ApiResponse<List<CustomerDto>>.ErrorResult("Từ khóa tìm kiếm không được để trống"));
                }

                var customers = await _unitOfWork.Customers.SearchCustomersAsync(searchTerm);
                var customerDtos = customers.Select(MapToDto).ToList();

                return Ok(ApiResponse<List<CustomerDto>>.SuccessResult(customerDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<CustomerDto>>.ErrorResult("Lỗi khi tìm kiếm khách hàng", ex.Message));
            }
        }

        private static CustomerDto MapToDto(Core.Entities.Customer customer)
        {
            return new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                PhoneNumber = customer.PhoneNumber,
                Email = customer.Email,
                Address = customer.Address,
                DateOfBirth = customer.DateOfBirth,
                Gender = customer.Gender,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt
            };
        }

        private static VehicleDto MapVehicleToDto(Core.Entities.Vehicle vehicle)
        {
            return new VehicleDto
            {
                Id = vehicle.Id,
                LicensePlate = vehicle.LicensePlate,
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Color = vehicle.Color,
                VIN = vehicle.VIN,
                EngineNumber = vehicle.EngineNumber,
                CustomerId = vehicle.CustomerId,
                CreatedAt = vehicle.CreatedAt,
                UpdatedAt = vehicle.UpdatedAt
            };
        }
    }
}
