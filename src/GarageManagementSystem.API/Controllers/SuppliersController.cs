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
    // [Authorize(Policy = "ApiScope")] // Tạm thời bỏ để test
    
    public class SuppliersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly GarageDbContext _context;

        public SuppliersController(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService, GarageDbContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<SupplierDto>>> GetSuppliers(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? category = null)
        {
            try
            {
                // ✅ OPTIMIZED: Query ở database level thay vì load tất cả vào memory
                var query = _context.Suppliers
                    .Where(s => !s.IsDeleted)
                    .AsQueryable();
                
                // Apply search filter if provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(s => 
                        (s.SupplierName != null && s.SupplierName.Contains(searchTerm)) || 
                        (s.ContactPerson != null && s.ContactPerson.Contains(searchTerm)) || 
                        (s.Email != null && s.Email.Contains(searchTerm)) ||
                        (s.Phone != null && s.Phone.Contains(searchTerm)));
                }
                
                // Apply category filter if provided (removed - Supplier doesn't have Category property)

                query = query.OrderBy(s => s.SupplierName);

                // ✅ OPTIMIZED: Get total count ở database level (trước khi paginate)
                var totalCount = await query.CountAsync();
                
                // ✅ OPTIMIZED: Apply pagination ở database level với Skip/Take
                var pagedSuppliers = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                
                var supplierDtos = pagedSuppliers.Select(MapToDto).ToList();
                
                return Ok(PagedResponse<SupplierDto>.CreateSuccessResult(
                    supplierDtos, pageNumber, pageSize, totalCount, "Suppliers retrieved successfully"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error retrieving suppliers: {ex}");
                return StatusCode(500, PagedResponse<SupplierDto>.CreateErrorResult("Error retrieving suppliers"));
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
                    var errors = new List<string>();
                    foreach (var key in ModelState.Keys)
                    {
                        var modelErrors = ModelState[key].Errors.Select(e => $"{key}: {e.ErrorMessage}");
                        errors.AddRange(modelErrors);
                    }
                    return BadRequest(ApiResponse<SupplierDto>.ErrorResult("Validation failed", errors));
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
                    Rating = dto.Rating.HasValue ? (decimal)dto.Rating.Value : 5.0m
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
                if (!ModelState.IsValid)
                {
                    var errors = new List<string>();
                    foreach (var key in ModelState.Keys)
                    {
                        var modelErrors = ModelState[key].Errors.Select(e => $"{key}: {e.ErrorMessage}");
                        errors.AddRange(modelErrors);
                    }
                    return BadRequest(ApiResponse<SupplierDto>.ErrorResult("Validation failed", errors));
                }

                if (id != dto.Id) return BadRequest(ApiResponse<SupplierDto>.ErrorResult("ID mismatch"));
                var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
                if (supplier == null) return NotFound(ApiResponse<SupplierDto>.ErrorResult("Not found"));

                // ✅ SỬA: Dùng AutoMapper thay vì map tay
                _mapper.Map(dto, supplier);
                
                // ✅ GIỮ: Logic đặc biệt cho Rating
                supplier.Rating = dto.Rating.HasValue ? (decimal)dto.Rating.Value : 5.0m;

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

        /// <summary>
        /// Update supplier rating (1-5 stars)
        /// </summary>
        [HttpPut("{id}/rating")]
        public async Task<ActionResult<ApiResponse<SupplierDto>>> UpdateRating(int id, [FromBody] decimal rating)
        {
            try
            {
                if (rating < 1 || rating > 5)
                {
                    return BadRequest(ApiResponse<SupplierDto>.ErrorResult("Rating must be between 1 and 5"));
                }

                var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
                if (supplier == null)
                {
                    return NotFound(ApiResponse<SupplierDto>.ErrorResult("Supplier not found"));
                }

                supplier.Rating = rating;
                await _unitOfWork.Suppliers.UpdateAsync(supplier);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse<SupplierDto>.SuccessResult(MapToDto(supplier), "Rating updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<SupplierDto>.ErrorResult("Error updating rating", ex.Message));
            }
        }

        /// <summary>
        /// Get supplier performance and statistics
        /// </summary>
        [HttpGet("{id}/performance")]
        public async Task<ActionResult<ApiResponse<object>>> GetSupplierPerformance(int id, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
                if (supplier == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Supplier not found"));
                }

                // Default date range: last 90 days
                startDate ??= DateTime.Now.AddDays(-90);
                endDate ??= DateTime.Now;

                // Get purchase orders from this supplier
                // Note: PurchaseOrders repository not in IUnitOfWork yet, using empty list
                var purchaseOrders = new List<Core.Entities.PurchaseOrder>();

                // Get stock transactions from this supplier
                var stockTransactions = await _unitOfWork.StockTransactions.FindAsync(st =>
                    st.SupplierId == id &&
                    !st.IsDeleted &&
                    st.TransactionDate >= startDate &&
                    st.TransactionDate <= endDate);

                // Calculate metrics
                var totalPurchaseOrders = purchaseOrders.Count();
                var completedOrders = purchaseOrders.Count(po => po.Status == "Completed");
                var pendingOrders = purchaseOrders.Count(po => po.Status == "Pending" || po.Status == "Ordered");
                var totalAmount = purchaseOrders.Sum(po => po.TotalAmount);
                var totalItems = stockTransactions.Where(st => st.TransactionType == Core.Enums.StockTransactionType.NhapKho)
                    .Sum(st => st.Quantity);

                // Calculate delivery performance
                var ordersWithDelivery = purchaseOrders
                    .Where(po => po.ExpectedDeliveryDate.HasValue && po.ActualDeliveryDate.HasValue)
                    .ToList();

                var onTimeDeliveries = ordersWithDelivery.Count(po => po.ActualDeliveryDate <= po.ExpectedDeliveryDate);
                var lateDeliveries = ordersWithDelivery.Count - onTimeDeliveries;
                var deliveryOnTimeRate = ordersWithDelivery.Count > 0 ? (decimal)onTimeDeliveries / ordersWithDelivery.Count * 100 : 0;

                var performance = new
                {
                    Supplier = MapToDto(supplier),
                    DateRange = new
                    {
                        StartDate = startDate,
                        EndDate = endDate
                    },
                    Metrics = new
                    {
                        TotalPurchaseOrders = totalPurchaseOrders,
                        CompletedOrders = completedOrders,
                        PendingOrders = pendingOrders,
                        TotalAmount = totalAmount,
                        TotalItemsPurchased = totalItems,
                        AverageOrderValue = totalPurchaseOrders > 0 ? totalAmount / totalPurchaseOrders : 0,
                        DeliveryOnTimeRate = deliveryOnTimeRate,
                        OnTimeDeliveries = onTimeDeliveries,
                        LateDeliveries = lateDeliveries,
                        CurrentRating = supplier.Rating
                    },
                    RecentPurchaseOrders = purchaseOrders.OrderByDescending(po => po.OrderDate).Take(10).Select(po => new
                    {
                        po.Id,
                        po.OrderNumber,
                        po.OrderDate,
                        po.Status,
                        po.TotalAmount,
                        po.ExpectedDeliveryDate,
                        po.ActualDeliveryDate
                    }).ToList()
                };

                return Ok(ApiResponse<object>.SuccessResult(performance));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult("Error retrieving supplier performance", ex.Message));
            }
        }

        private static SupplierDto MapToDto(Core.Entities.Supplier s) => new()
        {
            Id = s.Id, SupplierCode = s.SupplierCode, SupplierName = s.SupplierName,
            Phone = s.Phone, Email = s.Email, Address = s.Address,
            ContactPerson = s.ContactPerson, ContactPhone = s.ContactPhone,
            TaxCode = s.TaxCode, BankAccount = s.BankAccount, BankName = s.BankName,
            Notes = s.Notes, IsActive = s.IsActive, Rating = (int?)s.Rating,
            CreatedAt = s.CreatedAt, CreatedBy = s.CreatedBy,
            UpdatedAt = s.UpdatedAt, UpdatedBy = s.UpdatedBy
        };
    }
}

