using AutoMapper;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Core.Extensions;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarageManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Bật lại authentication
    public class CustomersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CustomersController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<CustomerDto>>> GetCustomers(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var customers = await _unitOfWork.Customers.GetAllAsync();
                var query = customers.AsQueryable();
                
                // Apply search filter if provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(c => 
                        c.Name.Contains(searchTerm) || 
                        c.Email.Contains(searchTerm) || 
                        c.Phone.Contains(searchTerm));
                }

                // Get total count
                var totalCount = await query.GetTotalCountAsync();
                
                // Apply pagination
                var pagedCustomers = query.ApplyPagination(pageNumber, pageSize).ToList();
                var customerDtos = pagedCustomers.Select(c => _mapper.Map<CustomerDto>(c)).ToList();
                
                return Ok(PagedResponse<CustomerDto>.CreateSuccessResult(
                    customerDtos, pageNumber, pageSize, totalCount, "Customers retrieved successfully"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error retrieving customers: {ex}");
                return StatusCode(500, PagedResponse<CustomerDto>.CreateErrorResult("Error retrieving customers"));
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả khách hàng cho dropdown (không phân trang)
        /// </summary>
        [HttpGet("dropdown")]
        public async Task<ActionResult<List<CustomerDto>>> GetAllForDropdown()
        {
            try
            {
                var customers = await _unitOfWork.Customers.GetAllAsync();
                var customerDtos = customers.Select(c => _mapper.Map<CustomerDto>(c)).ToList();
                
                return Ok(customerDtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting customers for dropdown: {ex}");
                return StatusCode(500, new { message = "Internal server error" });
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
                    return NotFound(ApiResponse<CustomerDto>.ErrorResult("Customer not found"));
                }

                var customerDto = _mapper.Map<CustomerDto>(customer);
                return Ok(ApiResponse<CustomerDto>.SuccessResult(customerDto));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error retrieving customer {id}: {ex}");
                return StatusCode(500, ApiResponse<CustomerDto>.ErrorResult("Error retrieving customer", ex, includeStackTrace: true));
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
                    return BadRequest(ApiResponse<CustomerDto>.ErrorResult("Invalid data", errors));
                }

                // ✅ Kiểm tra xem có khách hàng đã bị xóa với cùng email/phone không
                var existingDeletedCustomer = await _unitOfWork.Customers.FindAsync(c => 
                    c.IsDeleted && 
                    ((!string.IsNullOrEmpty(createDto.Email) && c.Email == createDto.Email) || 
                     (!string.IsNullOrEmpty(createDto.Phone) && c.Phone == createDto.Phone)));
                
                if (existingDeletedCustomer.Any())
                {
                    var deletedCustomer = existingDeletedCustomer.First();
                    
                    // Trả về thông báo có khách hàng cũ đã bị xóa
                    return BadRequest(ApiResponse<CustomerDto>.ErrorResult(
                        $"Đã tồn tại khách hàng '{deletedCustomer.Name}' (đã xóa) với email/phone này. " +
                        $"Vui lòng sử dụng API /customers/reactivate/{deletedCustomer.Id} để khôi phục hoặc dùng thông tin liên lạc khác.",
                        new List<string> 
                        { 
                            $"Deleted Customer ID: {deletedCustomer.Id}",
                            $"Deleted Customer Name: {deletedCustomer.Name}",
                            $"Deleted At: {deletedCustomer.DeletedAt}",
                            $"Use POST /api/customers/reactivate/{deletedCustomer.Id} to restore"
                        }));
                }

                var customer = new Core.Entities.Customer
                {
                    Name = createDto.Name,
                    Email = createDto.Email,
                    Phone = createDto.Phone,
                    AlternativePhone = createDto.AlternativePhone,
                    Address = createDto.Address,
                    ContactPersonName = createDto.ContactPersonName
                };

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.Customers.AddAsync(customer);
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

                var customerDto = _mapper.Map<CustomerDto>(customer);
                return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, 
                    ApiResponse<CustomerDto>.SuccessResult(customerDto, "Customer created successfully"));
            }
            catch (Exception ex)
            {
                // Log chi tiết lỗi ra console
                Console.WriteLine($"❌ Error creating customer: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                
                return StatusCode(500, ApiResponse<CustomerDto>.ErrorResult("Error creating customer", ex, includeStackTrace: true));
            }
        }

        /// <summary>
        /// Khôi phục khách hàng đã bị xóa (soft delete)
        /// </summary>
        [HttpPost("reactivate/{id}")]
        public async Task<ActionResult<ApiResponse<CustomerDto>>> ReactivateCustomer(int id, [FromBody] UpdateCustomerDto? updateDto = null)
        {
            try
            {
                // Lấy customer kể cả đã bị xóa
                var customer = await _unitOfWork.Customers.GetDeletedByIdAsync(id);
                
                // Nếu không tìm thấy trong deleted, kiểm tra active
                if (customer == null)
                {
                    customer = await _unitOfWork.Customers.GetByIdAsync(id);
                }
                
                if (customer == null)
                {
                    return NotFound(ApiResponse<CustomerDto>.ErrorResult("Customer not found"));
                }

                if (!customer.IsDeleted)
                {
                    return BadRequest(ApiResponse<CustomerDto>.ErrorResult("Customer is already active"));
                }

                // Khôi phục khách hàng
                customer.IsDeleted = false;
                customer.DeletedAt = null;
                customer.DeletedBy = null;

                // Cập nhật thông tin mới nếu có
                if (updateDto != null)
                {
                    // ✅ SỬA: Dùng AutoMapper thay vì map tay
                    _mapper.Map(updateDto, customer);
                }

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.Customers.UpdateAsync(customer);
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

                return Ok(ApiResponse<CustomerDto>.SuccessResult(_mapper.Map<CustomerDto>(customer), "Customer reactivated successfully"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error reactivating customer {id}: {ex}");
                return StatusCode(500, ApiResponse<CustomerDto>.ErrorResult("Error reactivating customer", ex, includeStackTrace: true));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<CustomerDto>>> UpdateCustomer(int id, UpdateCustomerDto updateDto)
        {
            try
            {
                if (id != updateDto.Id)
                {
                    return BadRequest(ApiResponse<CustomerDto>.ErrorResult("ID mismatch"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<CustomerDto>.ErrorResult("Invalid data", errors));
                }

                var customer = await _unitOfWork.Customers.GetByIdAsync(id);
                if (customer == null)
                {
                    return NotFound(ApiResponse<CustomerDto>.ErrorResult("Customer not found"));
                }

                // ✅ SỬA: Dùng AutoMapper thay vì map tay
                _mapper.Map(updateDto, customer);

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.Customers.UpdateAsync(customer);
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

                var customerDto = _mapper.Map<CustomerDto>(customer);
                return Ok(ApiResponse<CustomerDto>.SuccessResult(customerDto, "Customer updated successfully"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error updating customer {id}: {ex}");
                return StatusCode(500, ApiResponse<CustomerDto>.ErrorResult("Error updating customer", ex, includeStackTrace: true));
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
                    return NotFound(ApiResponse.ErrorResult("Customer not found"));
                }

                await _unitOfWork.Customers.DeleteAsync(customer);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse.SuccessResult("Customer deleted successfully"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error deleting customer {id}: {ex}");
                return StatusCode(500, ApiResponse.ErrorResult("Error deleting customer", ex, includeStackTrace: true));
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<List<CustomerDto>>>> SearchCustomers([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest(ApiResponse<List<CustomerDto>>.ErrorResult("Search term cannot be empty"));
                }

                var customers = await _unitOfWork.Customers.SearchAsync(searchTerm);
                var customerDtos = customers.Select(c => _mapper.Map<CustomerDto>(c)).ToList();

                return Ok(ApiResponse<List<CustomerDto>>.SuccessResult(customerDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<CustomerDto>>.ErrorResult("Error searching customers", ex.Message));
            }
        }

        /// <summary>
        /// Get customer history including vehicles, orders, appointments, quotations, invoices, and payments
        /// </summary>
        [HttpGet("{id}/history")]
        public async Task<ActionResult<ApiResponse<object>>> GetCustomerHistory(int id)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(id);
                if (customer == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Customer not found"));
                }

                // Get all related data
                var vehicles = await _unitOfWork.Vehicles.FindAsync(v => v.CustomerId == id && !v.IsDeleted);
                var serviceOrders = await _unitOfWork.ServiceOrders.FindAsync(so => so.CustomerId == id && !so.IsDeleted);
                var appointments = await _unitOfWork.Appointments.FindAsync(a => a.CustomerId == id && !a.IsDeleted);
                var quotations = await _unitOfWork.ServiceQuotations.FindAsync(q => q.CustomerId == id && !q.IsDeleted);
                var inspections = await _unitOfWork.VehicleInspections.FindAsync(vi => vi.CustomerId == id && !vi.IsDeleted);

                // Get invoices and payments through service orders
                var serviceOrderIds = serviceOrders.Select(so => so.Id).ToList();
                var invoices = await _unitOfWork.Invoices.FindAsync(inv => inv.ServiceOrderId.HasValue && serviceOrderIds.Contains(inv.ServiceOrderId.Value) && !inv.IsDeleted);
                var invoiceIds = invoices.Select(inv => inv.Id).ToList();
                var payments = await _unitOfWork.Payments.FindAsync(p => p.InvoiceId.HasValue && invoiceIds.Contains(p.InvoiceId.Value) && !p.IsDeleted);

                // Calculate statistics
                var totalSpent = invoices.Sum(i => i.TotalAmount);
                var totalPaid = payments.Sum(p => p.Amount);
                var outstanding = totalSpent - totalPaid;

                var history = new
                {
                    Customer = _mapper.Map<CustomerDto>(customer),
                    Statistics = new
                    {
                        TotalVehicles = vehicles.Count(),
                        TotalServiceOrders = serviceOrders.Count(),
                        TotalAppointments = appointments.Count(),
                        TotalQuotations = quotations.Count(),
                        TotalInspections = inspections.Count(),
                        TotalInvoices = invoices.Count(),
                        TotalSpent = totalSpent,
                        TotalPaid = totalPaid,
                        OutstandingBalance = outstanding
                    },
                    Vehicles = vehicles.Select(v => new
                    {
                        v.Id,
                        v.LicensePlate,
                        v.Brand,
                        v.Model,
                        v.Year,
                        v.VIN,
                        v.CreatedAt
                    }).OrderByDescending(v => v.CreatedAt).ToList(),
                    RecentServiceOrders = serviceOrders.OrderByDescending(so => so.OrderDate).Take(10).Select(so => new
                    {
                        so.Id,
                        so.OrderNumber,
                        so.OrderDate,
                        so.Status,
                        so.TotalAmount,
                        so.CompletedDate
                    }).ToList(),
                    RecentAppointments = appointments.OrderByDescending(a => a.ScheduledDateTime).Take(10).Select(a => new
                    {
                        a.Id,
                        a.ScheduledDateTime,
                        a.AppointmentType,
                        a.Status,
                        a.CustomerNotes
                    }).ToList(),
                    RecentQuotations = quotations.OrderByDescending(q => q.QuotationDate).Take(10).Select(q => new
                    {
                        q.Id,
                        q.QuotationNumber,
                        q.QuotationDate,
                        q.Status,
                        q.TotalAmount
                    }).ToList(),
                    RecentInvoices = invoices.OrderByDescending(i => i.InvoiceDate).Take(10).Select(i => new
                    {
                        i.Id,
                        i.InvoiceNumber,
                        i.InvoiceDate,
                        i.TotalAmount,
                        i.Status // Using Status instead of PaymentStatus
                    }).ToList(),
                    RecentPayments = payments.OrderByDescending(p => p.PaymentDate).Take(10).Select(p => new
                    {
                        p.Id,
                        p.PaymentDate,
                        p.Amount,
                        p.PaymentMethod,
                        p.Status
                    }).ToList()
                };

                return Ok(ApiResponse<object>.SuccessResult(history));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult("Error retrieving customer history", ex.Message));
            }
        }

        /// <summary>
        /// Get customers with active vehicles (for reception)
        /// </summary>
        [HttpGet("with-active-vehicles")]
        public async Task<ActionResult<ApiResponse<List<CustomerDto>>>> GetCustomersWithActiveVehicles()
        {
            try
            {
                var customers = await _unitOfWork.Customers.GetAllAsync();
                
                // Filter customers who have active service orders or appointments
                var activeCustomers = new List<CustomerDto>();
                
                foreach (var customer in customers)
                {
                    // Check if customer has active service orders
                    var hasActiveOrders = await _unitOfWork.ServiceOrders.GetServiceOrdersByCustomerIdAsync(customer.Id);
                    var activeOrders = hasActiveOrders.Where(o => o.Status != "Completed" && o.Status != "Cancelled").Any();
                    
                    // Check if customer has active appointments
                    var hasActiveAppointments = await _unitOfWork.Appointments.GetByCustomerIdAsync(customer.Id);
                    var activeAppointments = hasActiveAppointments.Where(a => a.Status != "Completed" && a.Status != "Cancelled").Any();
                    
                    if (activeOrders || activeAppointments)
                    {
                        activeCustomers.Add(_mapper.Map<CustomerDto>(customer));
                    }
                }
                
                return Ok(ApiResponse<List<CustomerDto>>.SuccessResult(activeCustomers));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error retrieving customers with active vehicles: {ex}");
                return StatusCode(500, ApiResponse<List<CustomerDto>>.ErrorResult("Error retrieving customers with active vehicles", ex.Message));
            }
        }

    }
}

