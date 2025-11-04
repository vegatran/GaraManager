using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Enums;
using GarageManagementSystem.Core.Extensions;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Core.Services;
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
    public class ServiceQuotationsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly GarageManagementSystem.API.Services.ICacheService _cacheService;

        public ServiceQuotationsController(IUnitOfWork unitOfWork, IMapper mapper, GarageManagementSystem.API.Services.ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<ServiceQuotationDto>>> GetQuotations(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? status = null,
            [FromQuery] int? customerId = null)
        {
            try
            {
                var quotations = await _unitOfWork.ServiceQuotations.GetAllWithDetailsAsync();
                var query = quotations.AsQueryable();
                
                // Apply search filter if provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(q => 
                        q.QuotationNumber.Contains(searchTerm) || 
                        q.CustomerName.Contains(searchTerm) || 
                        q.VehiclePlate.Contains(searchTerm));
                }
                
                // Apply status filter if provided
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(q => q.Status == status);
                }
                
                // Apply customer filter if provided
                if (customerId.HasValue)
                {
                    query = query.Where(q => q.CustomerId == customerId.Value);
                }

                query = query.OrderByDescending(q => q.QuotationDate);

                // Get total count
                var totalCount = await query.GetTotalCountAsync();
                
                // Apply pagination
                var pagedQuotations = query.ApplyPagination(pageNumber, pageSize).ToList();
                var quotationDtos = pagedQuotations.Select(MapToDto).ToList();
                
                return Ok(PagedResponse<ServiceQuotationDto>.CreateSuccessResult(
                    quotationDtos, pageNumber, pageSize, totalCount, "Service quotations retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, PagedResponse<ServiceQuotationDto>.CreateErrorResult("Lỗi khi lấy danh sách báo giá"));
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<ServiceQuotationDto>>> GetQuotation(int id)
        {
            try
            {
                
                var quotation = await _unitOfWork.ServiceQuotations.GetByIdWithDetailsAsync(id);
                if (quotation == null)
                {
                    return NotFound(ApiResponse<ServiceQuotationDto>.ErrorResult("Không tìm thấy báo giá"));
                }

                var quotationDto = MapToDto(quotation);
                
                
                return Ok(ApiResponse<ServiceQuotationDto>.SuccessResult(quotationDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceQuotationDto>.ErrorResult("Lỗi khi lấy thông tin báo giá", ex.Message));
            }
        }

        [HttpGet("vehicle/{vehicleId}")]
        public async Task<ActionResult<ApiResponse<List<ServiceQuotationDto>>>> GetQuotationsByVehicle(int vehicleId)
        {
            try
            {
                var quotations = await _unitOfWork.ServiceQuotations.GetByVehicleIdAsync(vehicleId);
                var quotationDtos = quotations.Select(MapToDto).ToList();

                return Ok(ApiResponse<List<ServiceQuotationDto>>.SuccessResult(quotationDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ServiceQuotationDto>>.ErrorResult("Lỗi khi lấy danh sách báo giá", ex.Message));
            }
        }


        [HttpGet("vehicle/{vehicleId}/can-create")]
        public async Task<ActionResult<ApiResponse<object>>> CanCreateQuotationForVehicle(int vehicleId)
        {
            try
            {
                // Kiểm tra xe có đang trong quy trình sửa chữa không
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(vehicleId);
                if (vehicle == null)
                {
                    return Ok(ApiResponse<object>.ErrorResult("Không tìm thấy xe"));
                }

                // Kiểm tra có báo giá "Approved" hoặc "Sent" chưa được chuyển thành Service Order không
                var quotations = await _unitOfWork.ServiceQuotations.GetByVehicleIdAsync(vehicleId);
                var activeQuotations = quotations.Where(q => 
                    (q.Status == "Approved" || q.Status == "Sent") && 
                    q.ValidUntil >= DateTime.Now).ToList();

                if (activeQuotations.Any())
                {
                    var quotationNumbers = string.Join(", ", activeQuotations.Select(q => q.QuotationNumber));
                    return Ok(ApiResponse<object>.ErrorResult($"Xe đã có báo giá đang hiệu lực: {quotationNumbers}. Không thể tạo báo giá mới"));
                }

                return Ok(ApiResponse<object>.SuccessResult(null, "Có thể tạo báo giá mới"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult("Lỗi khi kiểm tra khả năng tạo báo giá", ex.Message));
            }
        }

        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<ApiResponse<List<ServiceQuotationDto>>>> GetQuotationsByCustomer(int customerId)
        {
            try
            {
                var quotations = await _unitOfWork.ServiceQuotations.GetByCustomerIdAsync(customerId);
                var quotationDtos = quotations.Select(MapToDto).ToList();

                return Ok(ApiResponse<List<ServiceQuotationDto>>.SuccessResult(quotationDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ServiceQuotationDto>>.ErrorResult("Lỗi khi lấy danh sách báo giá", ex.Message));
            }
        }

        [HttpGet("inspection/{inspectionId}")]
        public async Task<ActionResult<ApiResponse<List<ServiceQuotationDto>>>> GetQuotationsByInspection(int inspectionId)
        {
            try
            {
                var quotations = await _unitOfWork.ServiceQuotations.GetByInspectionIdAsync(inspectionId);
                var quotationDtos = quotations.Select(MapToDto).ToList();

                return Ok(ApiResponse<List<ServiceQuotationDto>>.SuccessResult(quotationDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ServiceQuotationDto>>.ErrorResult("Lỗi khi lấy danh sách báo giá", ex.Message));
            }
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<ApiResponse<List<ServiceQuotationDto>>>> GetQuotationsByStatus(string status)
        {
            try
            {
                var quotations = await _unitOfWork.ServiceQuotations.GetByStatusAsync(status);
                var quotationDtos = quotations.Select(MapToDto).ToList();

                return Ok(ApiResponse<List<ServiceQuotationDto>>.SuccessResult(quotationDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ServiceQuotationDto>>.ErrorResult("Lỗi khi lấy danh sách báo giá", ex.Message));
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả báo giá cho dropdown (không phân trang)
        /// </summary>
        [HttpGet("dropdown")]
        public async Task<ActionResult<List<ServiceQuotationDto>>> GetAllForDropdown()
        {
            try
            {
                var quotations = await _unitOfWork.ServiceQuotations.GetAllWithDetailsAsync();
                var quotationDtos = quotations.Select(MapToDto).ToList();
                
                return Ok(quotationDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ServiceQuotationDto>>> CreateQuotation(CreateServiceQuotationDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
                }

                // Business Rule: Kiểm tra xem có VehicleInspectionId không
                if (createDto.VehicleInspectionId.HasValue)
                {
                    var inspection = await _unitOfWork.VehicleInspections.GetByIdAsync(createDto.VehicleInspectionId.Value);
                    if (inspection == null)
                    {
                        return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult("Không tìm thấy kiểm tra xe"));
                    }

                    // Business Rule: Chỉ cho phép tạo báo giá từ VehicleInspection đã hoàn thành
                    if (inspection.Status != "Completed")
                    {
                        return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult(
                            $"Không thể tạo báo giá. Kiểm tra xe phải ở trạng thái 'Đã Hoàn Thành'. Trạng thái hiện tại: {inspection.Status}"));
                    }

                    // Business Rule: Kiểm tra xem đã có báo giá cho VehicleInspection này chưa
                    var existingQuotation = await _unitOfWork.ServiceQuotations.GetByVehicleInspectionIdAsync(createDto.VehicleInspectionId.Value);
                    if (existingQuotation != null)
                    {
                        return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult("Đã tồn tại báo giá cho kiểm tra xe này"));
                    }
                }

                // Business Rule: Kiểm tra xem có CustomerReceptionId không
                if (createDto.CustomerReceptionId.HasValue)
                {
                    var reception = await _unitOfWork.CustomerReceptions.GetByIdAsync(createDto.CustomerReceptionId.Value);
                    if (reception == null)
                    {
                        return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult("Không tìm thấy phiếu tiếp đón"));
                    }

                    // Business Rule: Chỉ cho phép tạo báo giá từ CustomerReception đã hoàn thành kiểm tra
                    if (reception.Status != ReceptionStatus.Completed)
                    {
                        return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult(
                            $"Không thể tạo báo giá. Phiếu tiếp đón phải ở trạng thái 'Đã Hoàn Thành'. Trạng thái hiện tại: {reception.Status}"));
                    }
                }

                // Validate vehicle and customer exist
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(createDto.VehicleId);
                if (vehicle == null)
                {
                    return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult("Không tìm thấy xe"));
                }

                var customer = await _unitOfWork.Customers.GetByIdAsync(createDto.CustomerId);
                if (customer == null)
                {
                    return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult("Không tìm thấy khách hàng"));
                }

                // Create quotation bằng AutoMapper
                var quotation = _mapper.Map<Core.Entities.ServiceQuotation>(createDto);
                quotation.QuotationNumber = await _unitOfWork.ServiceQuotations.GenerateQuotationNumberAsync();
                quotation.QuotationDate = DateTime.Now;
                quotation.ValidUntil = createDto.ValidUntil ?? DateTime.Now.AddDays(7); // Default 7 days
                quotation.Status = QuotationStatus.Draft.ToString();

                // Add items and calculate totals with pricing models
                decimal subTotal = 0;
                foreach (var itemDto in createDto.Items)
                {
                    var item = new Core.Entities.QuotationItem
                    {
                        ServiceId = itemDto.ServiceId,
                        InspectionIssueId = itemDto.InspectionIssueId,
                        ItemName = itemDto.ItemName,
                        Description = itemDto.Description,
                        Quantity = itemDto.Quantity,
                        UnitPrice = itemDto.UnitPrice,
                        IsOptional = itemDto.IsOptional,
                        IsApproved = false,
                        Notes = itemDto.Notes,
                        ItemType = itemDto.ServiceType ?? "Service",
                        ItemCategory = itemDto.ItemCategory ?? "Material"  // ✅ THÊM ItemCategory
                    };

                    // Apply pricing model from Service
                    if (itemDto.ServiceId.HasValue)
                    {
                        var service = await _unitOfWork.Services.GetByIdAsync(itemDto.ServiceId.Value);
                        if (service != null)
                        {
                            PricingService.ApplyPricingToQuotationItem(item, service);
                        }
                        else
                        {
                            // Fallback to manual pricing
                            item.TotalPrice = itemDto.UnitPrice * itemDto.Quantity;
                        }
                    }
                    else
                    {
                        // Manual pricing (no service)
                        item.TotalPrice = itemDto.UnitPrice * itemDto.Quantity;
                    }

                    quotation.Items.Add(item);
                    subTotal += item.UnitPrice * item.Quantity; // ✅ SỬA: SubTotal = UnitPrice × Quantity (không bao gồm VAT)
                }

                quotation.SubTotal = subTotal;
                
                // ✅ SỬA: Chỉ tính VAT trên các items có IsVATApplicable = true
                var vatApplicableItems = quotation.Items.Where(item => item.IsVATApplicable).ToList();
                quotation.TaxAmount = vatApplicableItems.Sum(item => item.UnitPrice * item.Quantity * item.VATRate / 100);
                
                quotation.TotalAmount = quotation.SubTotal + quotation.TaxAmount - quotation.DiscountAmount;

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.ServiceQuotations.AddAsync(quotation);
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

                // Business Rule: CustomerReception status remains "Completed" after quotation creation
                // No need to update status as it's already in final state after inspection
                if (createDto.CustomerReceptionId.HasValue)
                {
                    // CustomerReception status is already "Completed" after inspection
                    // Quotation is a separate workflow step
                }

                // Reload with details
                quotation = await _unitOfWork.ServiceQuotations.GetByIdWithDetailsAsync(quotation.Id);
                var quotationDto = MapToDto(quotation!);

                return CreatedAtAction(nameof(GetQuotation), new { id = quotation.Id }, 
                    ApiResponse<ServiceQuotationDto>.SuccessResult(quotationDto, "Tạo báo giá thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceQuotationDto>.ErrorResult("Lỗi khi tạo báo giá", ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ServiceQuotationDto>>> UpdateQuotation(int id, UpdateServiceQuotationDto updateDto)
        {
            try
            {
                if (id != updateDto.Id)
                {
                    return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult("ID mismatch"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
                }

                var quotation = await _unitOfWork.ServiceQuotations.GetByIdAsync(id);
                if (quotation == null)
                {
                    return NotFound(ApiResponse<ServiceQuotationDto>.ErrorResult("Không tìm thấy báo giá"));
                }

                // ✅ 2.1.1: Business Rule: Khóa chỉnh sửa nếu đã có ServiceOrder
                if (quotation.ServiceOrderId.HasValue)
                {
                    var serviceOrder = await _unitOfWork.ServiceOrders.GetByIdAsync(quotation.ServiceOrderId.Value);
                    if (serviceOrder != null && serviceOrder.Status != "Cancelled")
                    {
                        return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult(
                            $"Không thể chỉnh sửa báo giá. Báo giá đã được chuyển thành phiếu sửa chữa (JO: {serviceOrder.OrderNumber}). " +
                            "Vui lòng chỉnh sửa trong phiếu sửa chữa thay vì báo giá."));
                    }
                }

                // Update quotation using AutoMapper
                _mapper.Map(updateDto, quotation);

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Update quotation first
                    await _unitOfWork.ServiceQuotations.UpdateAsync(quotation);

                    // Update Items if provided
                    if (updateDto.Items != null && updateDto.Items.Any())
                    {
                        // ✅ OPTIMIZED: Filter existing items ở database level
                        var itemsToDelete = (await _unitOfWork.ServiceQuotationItems
                            .FindAsync(i => i.ServiceQuotationId == id)).ToList();
                        foreach (var item in itemsToDelete)
                        {
                            // Hard delete thay vì soft delete để tránh duplicate
                            await _unitOfWork.ServiceQuotationItems.DeleteAsync(item);
                        }

                        // Add new items - dùng AutoMapper để map tất cả properties
                        foreach (var itemDto in updateDto.Items)
                        {
                            // ✅ SỬA: Dùng AutoMapper để map tất cả properties từ DTO sang Entity
                            var newItem = _mapper.Map<QuotationItem>(itemDto);
                            newItem.ServiceQuotationId = id;
                            newItem.QuotationId = id; // ✅ THÊM: Set QuotationId (alias của ServiceQuotationId)
                            newItem.CreatedAt = DateTime.Now;
                            newItem.UpdatedAt = DateTime.Now;
                            
                            // ✅ SỬA: Validate và fix foreign keys để tránh constraint errors
                            // Validate ServiceId nếu có - phải tồn tại trong database
                            if (itemDto.ServiceId.HasValue && itemDto.ServiceId.Value > 0)
                            {
                                var serviceExists = await _unitOfWork.Services.GetByIdAsync(itemDto.ServiceId.Value);
                                if (serviceExists == null)
                                {
                                    // Service không tồn tại, set về null để tránh foreign key constraint error
                                    newItem.ServiceId = null;
                                }
                            }
                            else
                            {
                                // Không có ServiceId hoặc = 0, set về null
                                newItem.ServiceId = null;
                            }
                            
                            // Nếu là Parts item (ServiceType = "parts"), đảm bảo không có ServiceId
                            if (!string.IsNullOrEmpty(itemDto.ServiceType) && itemDto.ServiceType.ToLower() == "parts")
                            {
                                if (newItem.ServiceId.HasValue)
                                {
                                    newItem.ServiceId = null; // Parts không cần ServiceId
                                }
                            }
                            
                            // ✅ SỬA: Validate required fields
                            if (string.IsNullOrWhiteSpace(newItem.ItemName))
                            {
                                throw new Exception($"ItemName là bắt buộc cho item {itemDto.ItemName ?? "không xác định"}");
                            }
                            
                            if (newItem.Quantity <= 0)
                            {
                                throw new Exception($"Số lượng phải lớn hơn 0 cho item {newItem.ItemName}");
                            }
                            
                            if (newItem.UnitPrice < 0)
                            {
                                throw new Exception($"Đơn giá không được âm cho item {newItem.ItemName}");
                            }
                            
                            // ✅ SỬA: Chỉ tính toán lại những field cần thiết, giữ nguyên VATRate từ user
                            newItem.SubTotal = newItem.Quantity * newItem.UnitPrice;
                            
                            // Tính VATAmount dựa trên VATRate từ user input
                            // VATRate có thể là decimal (0.10) hoặc phần trăm (10)
                            if (newItem.IsVATApplicable && newItem.VATRate > 0)
                            {
                                if (newItem.VATRate > 1)
                                {
                                    // Phần trăm: chia cho 100
                                    newItem.VATAmount = newItem.SubTotal * (newItem.VATRate / 100);
                                }
                                else
                                {
                                    // Decimal: dùng trực tiếp
                                    newItem.VATAmount = newItem.SubTotal * newItem.VATRate;
                                }
                            }
                            else
                            {
                                newItem.VATAmount = 0;
                            }
                            
                            // Tính TotalPrice (bao gồm VAT) - Required field
                            newItem.TotalPrice = newItem.SubTotal + newItem.VATAmount;
                            newItem.TotalAmount = newItem.TotalPrice - newItem.DiscountAmount;

                            await _unitOfWork.ServiceQuotationItems.AddAsync(newItem);
                        }
                        
                        // ✅ SỬA: Tính toán tổng từ items đã add vào context (không cần reload)
                        if (updateDto.Items != null && updateDto.Items.Any())
                        {
                            // Lấy items đã được add từ context hoặc tính trực tiếp từ DTO
                            quotation.SubTotal = updateDto.Items.Sum(item => item.Quantity * item.UnitPrice);
                            quotation.TaxAmount = updateDto.Items
                                .Where(item => item.IsVATApplicable && item.VATRate > 0)
                                .Sum(item => (item.Quantity * item.UnitPrice) * (item.VATRate / 100));
                            quotation.TotalAmount = quotation.SubTotal + quotation.TaxAmount - quotation.DiscountAmount;
                        }
                    }

                    // Save all changes
                    await _unitOfWork.SaveChangesAsync();

                    // Commit transaction nếu thành công
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch (Exception innerEx)
                {
                    // Rollback transaction nếu có lỗi
                    await _unitOfWork.RollbackTransactionAsync();
                    // Log chi tiết exception để debug
                    var innerMessage = innerEx.Message;
                    if (innerEx.InnerException != null)
                    {
                        innerMessage += $" | Inner: {innerEx.InnerException.Message}";
                        // Thêm stack trace nếu là Entity Framework exception
                        if (innerEx.InnerException.InnerException != null)
                        {
                            innerMessage += $" | Deep: {innerEx.InnerException.InnerException.Message}";
                        }
                    }
                    // Re-throw với thông điệp chi tiết hơn
                    throw new Exception($"Lỗi khi cập nhật items hoặc tính toán tổng tiền: {innerMessage}", innerEx);
                }

                // Reload with details
                quotation = await _unitOfWork.ServiceQuotations.GetByIdWithDetailsAsync(id);
                var quotationDto = MapToDto(quotation!);

                return Ok(ApiResponse<ServiceQuotationDto>.SuccessResult(quotationDto, "Cập nhật báo giá thành công"));
            }
            catch (Exception ex)
            {
                // Log chi tiết exception để debug
                var errorDetails = ex.Message;
                if (ex.InnerException != null)
                {
                    errorDetails += $" | Chi tiết: {ex.InnerException.Message}";
                }
                // Chỉ trả về error message chi tiết, không trả về stack trace
                return StatusCode(500, ApiResponse<ServiceQuotationDto>.ErrorResult("Lỗi khi cập nhật báo giá", errorDetails));
            }
        }

        [HttpPost("{id}/approve")]
        public async Task<ActionResult<ApiResponse<ServiceOrderDto>>> ApproveQuotation(int id, [FromBody] ApproveQuotationDto approveDto)
        {
            try
            {
                var quotation = await _unitOfWork.ServiceQuotations.GetByIdWithDetailsAsync(id);
                if (quotation == null)
                {
                    return NotFound(ApiResponse<ServiceOrderDto>.ErrorResult("Quotation not found"));
                }

                if (quotation.Status == "Approved")
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult("Quotation already approved"));
                }

                // Update quotation status
                quotation.Status = "Approved";
                quotation.ApprovedDate = DateTime.Now;
                quotation.CustomerNotes = approveDto.CustomerNotes;

                await _unitOfWork.ServiceQuotations.UpdateAsync(quotation);

                // Create ServiceOrder if requested
                Core.Entities.ServiceOrder? serviceOrder = null;
                if (approveDto.CreateServiceOrder)
                {
                    // Get vehicle to determine payment status based on vehicle type
                    var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(quotation.VehicleId);
                    string paymentStatus = vehicle?.VehicleType switch
                    {
                        "Insurance" => "InsurancePending", // Waiting for insurance approval
                        "Company" => "CompanyPending",     // Waiting for company approval
                        _ => "Pending"                     // Customer payment
                    };

                    serviceOrder = new Core.Entities.ServiceOrder
                    {
                        OrderNumber = $"SO-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
                        CustomerId = quotation.CustomerId,
                        VehicleId = quotation.VehicleId,
                        VehicleInspectionId = quotation.VehicleInspectionId,
                        ServiceQuotationId = quotation.Id,
                        OrderDate = DateTime.Now,
                        ScheduledDate = approveDto.ScheduledDate,
                        Status = ServiceOrderStatus.Pending.ToString(),
                        TotalAmount = quotation.TotalAmount,
                        DiscountAmount = quotation.DiscountAmount,
                        FinalAmount = quotation.TotalAmount,
                        PaymentStatus = paymentStatus,
                        
                        // ✅ 2.3.3: Set fields for additional order if quotation is additional
                        ParentServiceOrderId = quotation.IsAdditionalQuotation ? quotation.RelatedToServiceOrderId : null,
                        IsAdditionalOrder = quotation.IsAdditionalQuotation
                    };

                    // Copy items from quotation to order
                    foreach (var quotationItem in quotation.Items.Where(i => i.IsApproved || !i.IsOptional))
                    {
                        if (quotationItem.ServiceId.HasValue)
                        {
                            serviceOrder.ServiceOrderItems.Add(new Core.Entities.ServiceOrderItem
                            {
                                ServiceId = quotationItem.ServiceId.Value,
                                Quantity = quotationItem.Quantity,
                                UnitPrice = quotationItem.UnitPrice,
                                TotalPrice = quotationItem.TotalPrice,
                                Notes = quotationItem.Notes
                            });
                        }
                    }

                    // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                    await _unitOfWork.BeginTransactionAsync();

                    try
                    {
                        await _unitOfWork.ServiceOrders.AddAsync(serviceOrder);
                        quotation.ServiceOrderId = serviceOrder.Id;

                        await _unitOfWork.ServiceQuotations.UpdateAsync(quotation);
                        await _unitOfWork.SaveChangesAsync();
                        
                        // ✅ 2.3.3: Update AdditionalIssue if this is an additional quotation
                        if (quotation.IsAdditionalQuotation && quotation.Id > 0)
                        {
                            var additionalIssues = await _unitOfWork.Repository<Core.Entities.AdditionalIssue>()
                                .FindAsync(ai => ai.AdditionalQuotationId == quotation.Id);
                            
                            foreach (var issue in additionalIssues)
                            {
                                issue.AdditionalServiceOrderId = serviceOrder.Id;
                                issue.Status = "Approved";
                                await _unitOfWork.Repository<Core.Entities.AdditionalIssue>().UpdateAsync(issue);
                            }
                            
                            await _unitOfWork.SaveChangesAsync();
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
                }
                else
                {
                    // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                    await _unitOfWork.BeginTransactionAsync();

                    try
                    {
                        await _unitOfWork.ServiceQuotations.UpdateAsync(quotation);
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
                }

                // Return created service order
                if (serviceOrder != null)
                {
                    serviceOrder = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(serviceOrder.Id);
                    var orderDto = MapServiceOrderToDto(serviceOrder!);
                    return Ok(ApiResponse<ServiceOrderDto>.SuccessResult(orderDto, "Quotation approved and service order created"));
                }

                return Ok(ApiResponse<ServiceOrderDto>.SuccessResult(null, "Quotation approved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceOrderDto>.ErrorResult("Error approving quotation", ex.Message));
            }
        }

        [HttpPost("{id}/reject")]
        public async Task<ActionResult<ApiResponse<ServiceQuotationDto>>> RejectQuotation(int id, [FromBody] RejectQuotationDto rejectDto)
        {
            try
            {
                var quotation = await _unitOfWork.ServiceQuotations.GetByIdWithDetailsAsync(id);
                if (quotation == null)
                {
                    return NotFound(ApiResponse<ServiceQuotationDto>.ErrorResult("Không tìm thấy báo giá"));
                }

                if (quotation.Status == "Rejected")
                {
                    return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult("Báo giá đã được từ chối trước đó"));
                }

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    quotation.Status = "Rejected";
                    quotation.RejectedDate = DateTime.Now;
                    quotation.RejectionReason = rejectDto.Reason;

                    // ✅ THÊM: Tính phí kiểm tra nếu được yêu cầu
                    if (rejectDto.ChargeInspectionFee)
                    {
                        // Lấy phí kiểm tra từ configuration hoặc mặc định
                        // Có thể lấy từ bảng VehicleInspection nếu có
                        decimal inspectionFee = 0;
                        
                        if (quotation.VehicleInspectionId.HasValue)
                        {
                            var inspection = await _unitOfWork.VehicleInspections.GetByIdAsync(quotation.VehicleInspectionId.Value);
                            if (inspection != null)
                            {
                                // Có thể lưu inspection fee trong inspection entity hoặc lấy từ config
                                // Tạm thời dùng giá trị mặc định hoặc từ config
                                inspectionFee = 500000; // 500.000 VNĐ - có thể lấy từ config
                            }
                        }
                        else
                        {
                            // Nếu không có inspection, vẫn tính phí mặc định
                            inspectionFee = 500000; // 500.000 VNĐ - có thể lấy từ config
                        }

                        // ✅ THÊM: Tạo Financial Transaction cho phí kiểm tra
                        if (inspectionFee > 0)
                        {
                            // ✅ OPTIMIZED: Use CountAsync thay vì GetAllAsync().Count()
                            var count = await _unitOfWork.Repository<Core.Entities.FinancialTransaction>().CountAsync();
                            var transactionNumber = $"FT-{DateTime.Now:yyyyMMdd}-{(count + 1):D4}";
                            
                            var financialTransaction = new Core.Entities.FinancialTransaction
                            {
                                TransactionNumber = transactionNumber,
                                TransactionDate = DateTime.Now,
                                TransactionType = "Income", // Revenue = Income
                                Category = "Inspection",
                                Amount = inspectionFee,
                                Description = $"Phí kiểm tra xe cho báo giá {quotation.QuotationNumber}",
                                RelatedEntity = "Quotation",
                                RelatedEntityId = quotation.Id,
                                Status = "Completed",
                                PaymentMethod = "Cash",
                                Notes = $"Phí kiểm tra do khách hàng từ chối báo giá. Lý do: {rejectDto.Reason}",
                                CreatedAt = DateTime.Now
                            };

                            await _unitOfWork.Repository<Core.Entities.FinancialTransaction>().AddAsync(financialTransaction);
                        }
                    }

                    await _unitOfWork.ServiceQuotations.UpdateAsync(quotation);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    // Reload with details
                    quotation = await _unitOfWork.ServiceQuotations.GetByIdWithDetailsAsync(id);
                    var quotationDto = MapToDto(quotation!);

                    var message = rejectDto.ChargeInspectionFee 
                        ? "Đã từ chối báo giá và tính phí kiểm tra" 
                        : "Đã từ chối báo giá";

                    return Ok(ApiResponse<ServiceQuotationDto>.SuccessResult(quotationDto, message));
                }
                catch (Exception innerEx)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceQuotationDto>.ErrorResult("Lỗi khi từ chối báo giá", ex.Message));
            }
        }

        /// <summary>
        /// Dropdown: Danh sách báo giá đã duyệt, có thể tạo phiếu sửa chữa.
        /// Bao gồm: Approved và (chưa có ServiceOrder) hoặc ServiceOrder đã Cancelled.
        /// </summary>
        [HttpGet("approved-available")]
        public async Task<ActionResult<ApiResponse<List<object>>>> GetApprovedAvailableForOrder()
        {
            try
            {
                // Lấy Approved và chưa có SO (lọc ngay ở DB)
                var approvedNoSO = await _unitOfWork.ServiceQuotations
                    .FindAsync(q => q.Status == "Approved" && !q.ServiceOrderId.HasValue);

                // Lấy Approved đã có SO để kiểm tra SO Cancelled / SO đã bị xóa (IsDeleted)
                var approvedHasSO = await _unitOfWork.ServiceQuotations
                    .FindAsync(q => q.Status == "Approved" && q.ServiceOrderId.HasValue);
                var soIds = approvedHasSO.Select(q => q.ServiceOrderId!.Value).Distinct().ToList();
                // Lấy các SO còn tồn tại (không bị soft-delete) theo danh sách id
                var activeSOs = soIds.Any()
                    ? await _unitOfWork.ServiceOrders.FindAsync(so => soIds.Contains(so.Id))
                    : new List<Core.Entities.ServiceOrder>();
                var activeSoIdSet = new HashSet<int>(activeSOs.Select(so => so.Id));
                var cancelledIdSet = new HashSet<int>(activeSOs.Where(so => so.Status == "Cancelled").Select(so => so.Id));

                var result = new List<object>();

                // Eager-load Vehicle/Customer info for composing dropdown text
                var allQuotations = approvedNoSO.Concat(approvedHasSO).ToList();
                var vehicleIdsAll = allQuotations.Select(q => q.VehicleId).Distinct().ToList();
                var customerIdsAll = allQuotations.Select(q => q.CustomerId).Distinct().ToList();

                var vehicles = vehicleIdsAll.Any()
                    ? await _unitOfWork.Vehicles.FindAsync(v => vehicleIdsAll.Contains(v.Id))
                    : new List<Core.Entities.Vehicle>();
                var customers = customerIdsAll.Any()
                    ? await _unitOfWork.Customers.FindAsync(c => customerIdsAll.Contains(c.Id))
                    : new List<Core.Entities.Customer>();

                var vehicleDict = vehicles.ToDictionary(v => v.Id, v => v);
                var customerDict = customers.ToDictionary(c => c.Id, c => c);

                foreach (var q in approvedNoSO)
                {
                    vehicleDict.TryGetValue(q.VehicleId, out var v);
                    customerDict.TryGetValue(q.CustomerId, out var c);
                    var vehicleText = v != null ? $"{v.Brand} {v.Model} ({v.LicensePlate})" : "";
                    var customerText = c?.Name ?? "";
                    var text = string.IsNullOrEmpty(vehicleText) && string.IsNullOrEmpty(customerText)
                        ? q.QuotationNumber
                        : $"{q.QuotationNumber} - {vehicleText} - {customerText}";
                    result.Add(new
                    {
                        value = q.Id,
                        text = text,
                        vehicleId = q.VehicleId,
                        customerId = q.CustomerId,
                        vehicleInfo = vehicleText,
                        customerName = customerText,
                        totalAmount = q.TotalAmount,
                        quotationDate = q.CreatedAt
                    });
                }

                foreach (var q in approvedHasSO)
                {
                    if (!q.ServiceOrderId.HasValue) continue;
                    var soId = q.ServiceOrderId.Value;
                    // Không có trong activeSoIdSet => đã bị xóa (IsDeleted = true)
                    var isDeleted = !activeSoIdSet.Contains(soId);
                    var isCancelled = cancelledIdSet.Contains(soId);
                    if (isDeleted || isCancelled)
                    {
                        vehicleDict.TryGetValue(q.VehicleId, out var v);
                        customerDict.TryGetValue(q.CustomerId, out var c);
                        var vehicleText = v != null ? $"{v.Brand} {v.Model} ({v.LicensePlate})" : "";
                        var customerText = c?.Name ?? "";
                        var text = string.IsNullOrEmpty(vehicleText) && string.IsNullOrEmpty(customerText)
                            ? q.QuotationNumber
                            : $"{q.QuotationNumber} - {vehicleText} - {customerText}";
                        result.Add(new
                        {
                            value = q.Id,
                            text = text,
                            vehicleId = q.VehicleId,
                            customerId = q.CustomerId,
                            vehicleInfo = vehicleText,
                            customerName = customerText,
                            totalAmount = q.TotalAmount,
                            quotationDate = q.CreatedAt
                        });
                    }
                }

                return Ok(ApiResponse<List<object>>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<object>>.ErrorResult("Lỗi khi lấy danh sách báo giá khả dụng", ex.Message));
            }
        }

        [HttpPost("{id}/send")]
        public async Task<ActionResult<ApiResponse<ServiceQuotationDto>>> SendQuotation(int id)
        {
            try
            {
                var quotation = await _unitOfWork.ServiceQuotations.GetByIdAsync(id);
                if (quotation == null)
                {
                    return NotFound(ApiResponse<ServiceQuotationDto>.ErrorResult("Không tìm thấy báo giá"));
                }

                if (quotation.Status != QuotationStatus.Draft.ToString())
                {
                    return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult("Chỉ có thể gửi báo giá ở trạng thái 'Nháp'"));
                }

                quotation.Status = QuotationStatus.Sent.ToString();
                quotation.SentDate = DateTime.Now;

                await _unitOfWork.ServiceQuotations.UpdateAsync(quotation);
                await _unitOfWork.SaveChangesAsync();

                // Reload with details
                quotation = await _unitOfWork.ServiceQuotations.GetByIdWithDetailsAsync(id);
                var quotationDto = MapToDto(quotation!);

                return Ok(ApiResponse<ServiceQuotationDto>.SuccessResult(quotationDto, "Quotation sent to customer"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceQuotationDto>.ErrorResult("Error sending quotation", ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteQuotation(int id)
        {
            try
            {
                var quotation = await _unitOfWork.ServiceQuotations.GetByIdAsync(id);
                if (quotation == null)
                {
                    return NotFound(ApiResponse.ErrorResult("Quotation not found"));
                }

                if (quotation.Status == "Approved" && quotation.ServiceOrderId.HasValue && quotation.ServiceOrderId.Value !=0  && quotation.ServiceOrder?.IsDeleted == false)
                {
                    return BadRequest(ApiResponse.ErrorResult("Cannot delete approved quotation with service order"));
                }

                await _unitOfWork.ServiceQuotations.DeleteAsync(quotation);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse.SuccessResult("Quotation deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResult("Error deleting quotation", ex.Message));
            }
        }

        private ServiceQuotationDto MapToDto(Core.Entities.ServiceQuotation quotation)
        {
            // Use AutoMapper for consistent mapping
            return _mapper.Map<ServiceQuotationDto>(quotation);
        }

        private static ServiceQuotationDto MapToDtoOld(Core.Entities.ServiceQuotation quotation)
        {
            return new ServiceQuotationDto
            {
                Id = quotation.Id,
                QuotationNumber = quotation.QuotationNumber,
                VehicleInspectionId = quotation.VehicleInspectionId,
                CustomerId = quotation.CustomerId,
                VehicleId = quotation.VehicleId,
                PreparedById = quotation.PreparedById,
                QuotationDate = quotation.QuotationDate,
                ValidUntil = quotation.ValidUntil,
                Description = quotation.Description,
                Terms = quotation.Terms,
                QuotationType = quotation.QuotationType,
                SubTotal = quotation.SubTotal,
                TaxAmount = quotation.TaxAmount,
                TaxRate = quotation.TaxRate,
                DiscountAmount = quotation.DiscountAmount,
                TotalAmount = quotation.TotalAmount,
                // Insurance specific fields
                MaxInsuranceAmount = quotation.MaxInsuranceAmount,
                Deductible = quotation.Deductible,
                InsuranceApprovalDate = quotation.InsuranceApprovalDate,
                InsuranceApprovedAmount = quotation.InsuranceApprovedAmount,
                InsuranceApprovalNotes = quotation.InsuranceApprovalNotes,
                InsuranceAdjusterContact = quotation.InsuranceAdjusterContact,
                // Company specific fields
                PONumber = quotation.PONumber,
                PaymentTerms = quotation.PaymentTerms,
                IsTaxExempt = quotation.IsTaxExempt,
                CompanyApprovalDate = quotation.CompanyApprovalDate,
                CompanyApprovedBy = quotation.CompanyApprovedBy,
                CompanyApprovalNotes = quotation.CompanyApprovalNotes,
                CompanyContactPerson = quotation.CompanyContactPerson,
                Status = quotation.Status,
                SentDate = quotation.SentDate,
                ApprovedDate = quotation.ApprovedDate,
                RejectedDate = quotation.RejectedDate,
                CustomerNotes = quotation.CustomerNotes,
                RejectionReason = quotation.RejectionReason,
                ServiceOrderId = quotation.ServiceOrderId,
                Customer = quotation.Customer != null ? new CustomerDto
                {
                    Id = quotation.Customer.Id,
                    Name = quotation.Customer.Name,
                    Phone = quotation.Customer.Phone,
                    Email = quotation.Customer.Email
                } : null,
                Vehicle = quotation.Vehicle != null ? new VehicleDto
                {
                    Id = quotation.Vehicle.Id,
                    LicensePlate = quotation.Vehicle.LicensePlate,
                    Brand = quotation.Vehicle.Brand,
                    Model = quotation.Vehicle.Model
                } : null,
                PreparedBy = quotation.PreparedBy != null ? new EmployeeDto
                {
                    Id = quotation.PreparedBy.Id,
                    Name = quotation.PreparedBy.Name,
                    Position = quotation.PreparedBy.Position
                } : null,
                Items = quotation.Items?.Select(item => new QuotationItemDto
                {
                    Id = item.Id,
                    ServiceQuotationId = item.ServiceQuotationId,
                    ServiceId = item.ServiceId,
                    InspectionIssueId = item.InspectionIssueId,
                    ItemName = item.ItemName,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice,
                    IsOptional = item.IsOptional,
                    IsApproved = item.IsApproved,
                    HasInvoice = item.HasInvoice,
                    IsVATApplicable = item.IsVATApplicable,
                    Notes = item.Notes,
                    ServiceType = item.ItemType,
                    Service = item.Service != null ? new ServiceDto
                    {
                        Id = item.Service.Id,
                        Name = item.Service.Name,
                        Price = item.Service.Price,
                        Duration = item.Service.Duration
                    } : null
                }).ToList() ?? new List<QuotationItemDto>(),
                CreatedAt = quotation.CreatedAt,
                CreatedBy = quotation.CreatedBy,
                UpdatedAt = quotation.UpdatedAt,
                UpdatedBy = quotation.UpdatedBy
            };
        }

        private static ServiceOrderDto MapServiceOrderToDto(Core.Entities.ServiceOrder order)
        {
            return new ServiceOrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerId = order.CustomerId,
                VehicleId = order.VehicleId,
                OrderDate = order.OrderDate,
                ScheduledDate = order.ScheduledDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.DiscountAmount,
                FinalAmount = order.FinalAmount,
                PaymentStatus = order.PaymentStatus,
                CreatedAt = order.CreatedAt,
                CreatedBy = order.CreatedBy
            };
        }

        /// <summary>
        /// Quy trình phê duyệt bảo hiểm
        /// </summary>
        [HttpPost("{id}/insurance-approve")]
        public async Task<ActionResult<ApiResponse<ServiceQuotationDto>>> InsuranceApprove(int id, [FromBody] InsuranceApprovalDto approvalDto)
        {
            try
            {
                var quotation = await _unitOfWork.ServiceQuotations.GetByIdAsync(id);
                if (quotation == null)
                {
                    return NotFound(ApiResponse<ServiceQuotationDto>.ErrorResult("Không tìm thấy báo giá"));
                }

                if (quotation.QuotationType != "Insurance")
                {
                    return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult("This quotation is not for insurance"));
                }

                if (quotation.Status != "Sent")
                {
                    return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult("Quotation must be in 'Sent' status for insurance approval"));
                }

                // Cập nhật thông tin phê duyệt bảo hiểm
                quotation.InsuranceApprovalDate = DateTime.Now;
                quotation.InsuranceApprovedAmount = approvalDto.InsuranceApprovedAmount;
                quotation.InsuranceApprovalNotes = approvalDto.InsuranceApprovalNotes;
                quotation.InsuranceAdjusterContact = approvalDto.InsuranceAdjusterContact;

                if (approvalDto.Deductible.HasValue)
                {
                    quotation.Deductible = approvalDto.Deductible.Value;
                }

                if (approvalDto.IsApproved)
                {
                    quotation.Status = "Approved";
                    quotation.ApprovedDate = DateTime.Now;
                }
                else
                {
                    quotation.Status = "Rejected";
                    quotation.RejectedDate = DateTime.Now;
                    quotation.RejectionReason = approvalDto.RejectionReason ?? "Insurance company rejected";
                }

                await _unitOfWork.ServiceQuotations.UpdateAsync(quotation);
                
                // Cập nhật trạng thái thanh toán ServiceOrder liên quan nếu có
                if (quotation.ServiceOrderId.HasValue)
                {
                    var serviceOrder = await _unitOfWork.ServiceOrders.GetByIdAsync(quotation.ServiceOrderId.Value);
                    if (serviceOrder != null)
                    {
                        if (approvalDto.IsApproved)
                        {
                            serviceOrder.PaymentStatus = "InsuranceApproved";
                        }
                        else
                        {
                            serviceOrder.PaymentStatus = "InsuranceRejected";
                        }
                        await _unitOfWork.ServiceOrders.UpdateAsync(serviceOrder);
                    }
                }
                
                await _unitOfWork.SaveChangesAsync();

                var updatedQuotation = await _unitOfWork.ServiceQuotations.GetByIdWithDetailsAsync(id);
                var quotationDto = MapToDto(updatedQuotation!);

                return Ok(ApiResponse<ServiceQuotationDto>.SuccessResult(quotationDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceQuotationDto>.ErrorResult("Error processing insurance approval", ex.Message));
            }
        }

        /// <summary>
        /// Quy trình phê duyệt công ty
        /// </summary>
        [HttpPost("{id}/company-approve")]
        public async Task<ActionResult<ApiResponse<ServiceQuotationDto>>> CompanyApprove(int id, [FromBody] CompanyApprovalDto approvalDto)
        {
            try
            {
                var quotation = await _unitOfWork.ServiceQuotations.GetByIdAsync(id);
                if (quotation == null)
                {
                    return NotFound(ApiResponse<ServiceQuotationDto>.ErrorResult("Không tìm thấy báo giá"));
                }

                if (quotation.QuotationType != "Company")
                {
                    return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult("This quotation is not for company"));
                }

                if (quotation.Status != "Sent")
                {
                    return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult("Quotation must be in 'Sent' status for company approval"));
                }

                // Cập nhật thông tin phê duyệt công ty
                quotation.CompanyApprovalDate = DateTime.Now;
                quotation.PONumber = approvalDto.PONumber;
                quotation.CompanyApprovedBy = approvalDto.CompanyApprovedBy;
                quotation.PaymentTerms = approvalDto.PaymentTerms;
                quotation.IsTaxExempt = approvalDto.IsTaxExempt;
                quotation.CompanyApprovalNotes = approvalDto.CompanyApprovalNotes;
                quotation.CompanyContactPerson = approvalDto.CompanyContactPerson;

                if (approvalDto.IsApproved)
                {
                    quotation.Status = "Approved";
                    quotation.ApprovedDate = DateTime.Now;
                }
                else
                {
                    quotation.Status = "Rejected";
                    quotation.RejectedDate = DateTime.Now;
                    quotation.RejectionReason = approvalDto.RejectionReason ?? "Company rejected";
                }

                await _unitOfWork.ServiceQuotations.UpdateAsync(quotation);
                
                // Cập nhật trạng thái thanh toán ServiceOrder liên quan nếu có
                if (quotation.ServiceOrderId.HasValue)
                {
                    var serviceOrder = await _unitOfWork.ServiceOrders.GetByIdAsync(quotation.ServiceOrderId.Value);
                    if (serviceOrder != null)
                    {
                        if (approvalDto.IsApproved)
                        {
                            serviceOrder.PaymentStatus = "CompanyApproved";
                        }
                        else
                        {
                            serviceOrder.PaymentStatus = "CompanyRejected";
                        }
                        await _unitOfWork.ServiceOrders.UpdateAsync(serviceOrder);
                    }
                }
                
                await _unitOfWork.SaveChangesAsync();

                var updatedQuotation = await _unitOfWork.ServiceQuotations.GetByIdWithDetailsAsync(id);
                var quotationDto = MapToDto(updatedQuotation!);

                return Ok(ApiResponse<ServiceQuotationDto>.SuccessResult(quotationDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceQuotationDto>.ErrorResult("Error processing company approval", ex.Message));
            }
        }

        /// <summary>
        /// Submit insurance claim for a vehicle
        /// </summary>
        [HttpPost("submit-insurance-claim")]
        public async Task<ActionResult<ApiResponse<VehicleDto>>> SubmitInsuranceClaim([FromBody] SubmitInsuranceClaimDto claimDto)
        {
            try
            {
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(claimDto.VehicleId);
                if (vehicle == null)
                {
                    return NotFound(ApiResponse<VehicleDto>.ErrorResult("Vehicle not found"));
                }

                // Update vehicle with insurance information
                vehicle.VehicleType = "Insurance";
                vehicle.InsuranceCompany = claimDto.InsuranceCompany;
                vehicle.PolicyNumber = claimDto.PolicyNumber;
                vehicle.CoverageType = claimDto.CoverageType;
                vehicle.ClaimNumber = claimDto.ClaimNumber;
                vehicle.AdjusterName = claimDto.AdjusterName;
                vehicle.AdjusterPhone = claimDto.AdjusterPhone;

                await _unitOfWork.Vehicles.UpdateAsync(vehicle);
                await _unitOfWork.SaveChangesAsync();

                var updatedVehicle = await _unitOfWork.Vehicles.GetByIdWithCustomerAsync(claimDto.VehicleId);
                var vehicleDto = MapVehicleToDto(updatedVehicle!);

                return Ok(ApiResponse<VehicleDto>.SuccessResult(vehicleDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<VehicleDto>.ErrorResult("Error submitting insurance claim", ex.Message));
            }
        }

        /// <summary>
        /// Register company vehicle
        /// </summary>
        [HttpPost("register-company-vehicle")]
        public async Task<ActionResult<ApiResponse<VehicleDto>>> RegisterCompanyVehicle([FromBody] RegisterCompanyVehicleDto companyVehicleDto)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(companyVehicleDto.CustomerId);
                if (customer == null)
                {
                    return NotFound(ApiResponse<VehicleDto>.ErrorResult("Customer not found"));
                }

                // Check if license plate already exists
                var existingVehicle = await _unitOfWork.Vehicles.GetByLicensePlateAsync(companyVehicleDto.LicensePlate);
                if (existingVehicle != null)
                {
                    return BadRequest(ApiResponse<VehicleDto>.ErrorResult("License plate already exists"));
                }

                // Create new company vehicle
                var vehicle = new Core.Entities.Vehicle
                {
                    LicensePlate = companyVehicleDto.LicensePlate,
                    Brand = companyVehicleDto.Brand,
                    Model = companyVehicleDto.Model,
                    Year = companyVehicleDto.Year,
                    Color = companyVehicleDto.Color,
                    VIN = companyVehicleDto.VIN,
                    EngineNumber = companyVehicleDto.EngineNumber,
                    CustomerId = companyVehicleDto.CustomerId,
                    VehicleType = "Company",
                    CompanyName = companyVehicleDto.CompanyName,
                    TaxCode = companyVehicleDto.TaxCode,
                    ContactPerson = companyVehicleDto.ContactPerson,
                    ContactPhone = companyVehicleDto.ContactPhone,
                    Department = companyVehicleDto.Department,
                    CostCenter = companyVehicleDto.CostCenter
                };

                await _unitOfWork.Vehicles.AddAsync(vehicle);
                await _unitOfWork.SaveChangesAsync();

                var createdVehicle = await _unitOfWork.Vehicles.GetByIdWithCustomerAsync(vehicle.Id);
                var vehicleDto = MapVehicleToDto(createdVehicle!);

                return Ok(ApiResponse<VehicleDto>.SuccessResult(vehicleDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<VehicleDto>.ErrorResult("Error registering company vehicle", ex.Message));
            }
        }

        /// <summary>
        /// Get quotations by type (Personal, Insurance, Company)
        /// </summary>
        [HttpGet("by-type/{quotationType}")]
        public async Task<ActionResult<ApiResponse<List<ServiceQuotationDto>>>> GetQuotationsByType(string quotationType)
        {
            try
            {
                if (!new[] { "Personal", "Insurance", "Company" }.Contains(quotationType))
                {
                    return BadRequest(ApiResponse<List<ServiceQuotationDto>>.ErrorResult("Invalid quotation type. Must be Personal, Insurance, or Company"));
                }

                var quotations = await _unitOfWork.ServiceQuotations.GetAllWithDetailsAsync();
                var filteredQuotations = quotations.Where(q => q.QuotationType == quotationType).ToList();
                var quotationDtos = filteredQuotations.Select(MapToDto).ToList();

                return Ok(ApiResponse<List<ServiceQuotationDto>>.SuccessResult(quotationDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ServiceQuotationDto>>.ErrorResult("Error retrieving quotations by type", ex.Message));
            }
        }

        [HttpPost("{id}/insurance-approved-pricing")]
        [Authorize(Policy = "ApiScope")]
        public async Task<ActionResult<ApiResponse<ServiceQuotationDto>>> UpdateInsuranceApprovedPricing(int id, [FromBody] InsuranceApprovedPricingDto pricingData)
        {
            try
            {
                var quotation = await _unitOfWork.ServiceQuotations.GetByIdAsync(id);
                if (quotation == null)
                {
                    return NotFound(ApiResponse<ServiceQuotationDto>.ErrorResult("Quotation not found"));
                }

                // Update quotation with insurance pricing data
                quotation.InsuranceApprovalDate = pricingData.ApprovalDate;
                quotation.InsuranceApprovedAmount = pricingData.ApprovedAmount;
                quotation.InsuranceApprovalNotes = pricingData.ApprovalNotes;
                // Save uploaded insurance verification file path (if provided)
                quotation.InsuranceFilePath = pricingData.InsuranceFilePath;
                quotation.UpdatedAt = DateTime.Now;

                // ✅ THÊM: Tự động chuyển trạng thái thành "Approved" khi cập nhật bảng giá bảo hiểm
                if (quotation.QuotationType == "Insurance" && quotation.Status == "Pending")
                {
                    quotation.Status = "Approved";
                }

                await _unitOfWork.ServiceQuotations.UpdateAsync(quotation);

                // Update individual items if provided
                if (pricingData.ApprovedItems != null && pricingData.ApprovedItems.Any())
                {
                    foreach (var approvedItem in pricingData.ApprovedItems)
                    {
                        var item = await _unitOfWork.ServiceQuotationItems.GetByIdAsync(approvedItem.QuotationItemId);
                        if (item != null)
                        {
                            item.InsuranceApprovedUnitPrice = approvedItem.ApprovedPrice;
                            item.InsuranceApprovedSubTotal = approvedItem.ApprovedPrice * item.Quantity;
                            item.InsuranceApprovalNotes = approvedItem.ApprovalNotes;
                            item.UpdatedAt = DateTime.Now;

                            await _unitOfWork.ServiceQuotationItems.UpdateAsync(item);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                var result = await _unitOfWork.ServiceQuotations.GetByIdWithDetailsAsync(id);
                var quotationDto = _mapper.Map<ServiceQuotationDto>(result);

                return Ok(ApiResponse<ServiceQuotationDto>.SuccessResult(quotationDto));
            }
            catch (Exception ex)
            {
                // Log error
                return StatusCode(500, ApiResponse<ServiceQuotationDto>.ErrorResult("Error updating insurance approved pricing", ex.Message));
            }
        }

        [HttpGet("{id}/insurance-approved-pricing")]
        [Authorize(Policy = "ApiScope")]
        public async Task<ActionResult<ApiResponse<InsuranceApprovedPricingDto>>> GetInsuranceApprovedPricing(int id)
        {
            try
            {
                var quotation = await _unitOfWork.ServiceQuotations.GetByIdWithDetailsAsync(id);
                if (quotation == null)
                {
                    return NotFound(ApiResponse<InsuranceApprovedPricingDto>.ErrorResult("Quotation not found"));
                }

                var pricingData = new InsuranceApprovedPricingDto
                {
                    QuotationId = id,
                    InsuranceCompany = "", // Not available in entity
                    TaxCode = "", // Not available in entity
                    PolicyNumber = "", // Not available in entity
                    ApprovalDate = quotation.InsuranceApprovalDate,
                    ApprovedAmount = quotation.InsuranceApprovedAmount ?? 0,
                    CustomerCoPayment = 0, // Not available in entity
                    ApprovalNotes = quotation.InsuranceApprovalNotes ?? "",
                    InsuranceFilePath = quotation.InsuranceFilePath, // ✅ THÊM: Đường dẫn file bảo hiểm
                    ApprovedItems = quotation.Items?.Where(x => !x.IsDeleted).Select(item => new InsuranceApprovedItemDto
                    {
                        QuotationItemId = item.Id,
                        ItemName = item.ItemName,
                        Quantity = item.Quantity,
                        OriginalPrice = item.UnitPrice,
                        ApprovedPrice = item.InsuranceApprovedUnitPrice ?? item.UnitPrice,
                        CustomerCoPayment = 0, // Not available in entity
                        IsApproved = true, // Default to true
                        ApprovalNotes = item.InsuranceApprovalNotes ?? ""
                    }).ToList() ?? new List<InsuranceApprovedItemDto>()
                };

                return Ok(ApiResponse<InsuranceApprovedPricingDto>.SuccessResult(pricingData));
            }
            catch (Exception ex)
            {
                // Log error
                return StatusCode(500, ApiResponse<InsuranceApprovedPricingDto>.ErrorResult("Error getting insurance approved pricing", ex.Message));
            }
        }

        [HttpPost("{id}/corporate-approved-pricing")]
        [Authorize(Policy = "ApiScope")]
        public async Task<ActionResult<ApiResponse<ServiceQuotationDto>>> UpdateCorporateApprovedPricing(int id, [FromBody] CorporateApprovedPricingDto pricingData)
        {
            try
            {
                var quotation = await _unitOfWork.ServiceQuotations.GetByIdAsync(id);
                if (quotation == null)
                {
                    return NotFound(ApiResponse<ServiceQuotationDto>.ErrorResult("Quotation not found"));
                }

                // Update quotation with corporate pricing data
                quotation.CorporateApprovalDate = pricingData.ApprovalDate;
                quotation.CorporateApprovedAmount = pricingData.ApprovedAmount;
                quotation.CorporateApprovalNotes = pricingData.ApprovalNotes;
                quotation.UpdatedAt = DateTime.Now;

                // ✅ THÊM: Tự động chuyển trạng thái thành "Approved" khi cập nhật bảng giá công ty
                if (quotation.QuotationType == "Corporate" && quotation.Status == "Pending")
                {
                    quotation.Status = "Approved";
                }

                await _unitOfWork.ServiceQuotations.UpdateAsync(quotation);

                // Update individual items if provided
                if (pricingData.ApprovedItems != null && pricingData.ApprovedItems.Any())
                {
                    foreach (var approvedItem in pricingData.ApprovedItems)
                    {
                        var quotationItem = await _unitOfWork.Repository<QuotationItem>().GetByIdAsync(approvedItem.QuotationItemId);
                        if (quotationItem != null)
                        {
                            quotationItem.CorporateApprovedUnitPrice = approvedItem.ApprovedPrice;
                            quotationItem.CorporateApprovedSubTotal = approvedItem.ApprovedPrice * quotationItem.Quantity;
                            quotationItem.CorporateApprovalNotes = approvedItem.ApprovalNotes;
                            quotationItem.UpdatedAt = DateTime.Now;

                            await _unitOfWork.Repository<QuotationItem>().UpdateAsync(quotationItem);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                var result = await _unitOfWork.ServiceQuotations.GetByIdWithDetailsAsync(id);
                var quotationDto = _mapper.Map<ServiceQuotationDto>(result);

                return Ok(ApiResponse<ServiceQuotationDto>.SuccessResult(quotationDto));
            }
            catch (Exception ex)
            {
                // Log error
                return StatusCode(500, ApiResponse<ServiceQuotationDto>.ErrorResult("Error updating corporate approved pricing", ex.Message));
            }
        }

        [HttpGet("{id}/corporate-approved-pricing")]
        [Authorize(Policy = "ApiScope")]
        public async Task<ActionResult<ApiResponse<CorporateApprovedPricingDto>>> GetCorporateApprovedPricing(int id)
        {
            try
            {
                var quotation = await _unitOfWork.ServiceQuotations.GetByIdWithDetailsAsync(id);
                if (quotation == null)
                {
                    return NotFound(ApiResponse<CorporateApprovedPricingDto>.ErrorResult("Quotation not found"));
                }

                var pricing = new CorporateApprovedPricingDto
                {
                    QuotationId = quotation.Id,
                    CompanyName = quotation.CorporateApprovalNotes ?? "",
                    TaxCode = "",
                    ContractNumber = quotation.ContractNumber ?? "",
                    ApprovalDate = quotation.CorporateApprovalDate,
                    ApprovedAmount = quotation.CorporateApprovedAmount ?? 0,
                    CustomerCoPayment = 0,
                    ApprovalNotes = quotation.CorporateApprovalNotes ?? "",
                    ApprovedItems = quotation.Items?.Select(item => new CorporateApprovedItemDto
                    {
                        QuotationItemId = item.Id,
                        ItemName = item.ItemName,
                        Quantity = item.Quantity,
                        OriginalPrice = item.UnitPrice,
                        ApprovedPrice = item.CorporateApprovedUnitPrice ?? item.UnitPrice,
                        CustomerCoPayment = 0,
                        IsApproved = true,
                        ApprovalNotes = item.CorporateApprovalNotes
                    }).ToList() ?? new List<CorporateApprovedItemDto>()
                };

                return Ok(ApiResponse<CorporateApprovedPricingDto>.SuccessResult(pricing));
            }
            catch (Exception ex)
            {
                // Log error
                return StatusCode(500, ApiResponse<CorporateApprovedPricingDto>.ErrorResult("Error getting corporate approved pricing", ex.Message));
            }
        }

        [HttpPost("demo-seed")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<string>>> CreateDemoQuotations()
        {
            try
            {
                // Try to find any existing customer and vehicle; if none, bail out with message
                var customers = await _unitOfWork.Customers.GetAllAsync();
                var vehicles = await _unitOfWork.Vehicles.GetAllAsync();
                var anyCustomer = customers.FirstOrDefault();
                var anyVehicle = vehicles.FirstOrDefault();
                if (anyCustomer == null || anyVehicle == null)
                {
                    return BadRequest(ApiResponse<string>.ErrorResult("Thiếu dữ liệu khách hàng/xe để tạo demo. Hãy tạo ít nhất 1 khách hàng và 1 xe."));
                }

                async Task<int> CreateOne(string quotationType, string numberSuffix)
                {
                    var now = DateTime.Now;
                    var quotation = new ServiceQuotation
                    {
                        QuotationNumber = $"QT{now:yyyyMMddHHmmss}{numberSuffix}",
                        CustomerId = anyCustomer.Id,
                        VehicleId = anyVehicle.Id,
                        QuotationDate = now,
                        ValidUntil = now.AddDays(30),
                        QuotationType = quotationType,
                        Status = "Pending",
                        SubTotal = 0,
                        VATAmount = 0,
                        TotalAmount = 0,
                        CreatedAt = now,
                        UpdatedAt = now
                    };

                    await _unitOfWork.ServiceQuotations.AddAsync(quotation);
                    await _unitOfWork.SaveChangesAsync();

                    // Add one demo item
                    var item = new QuotationItem
                    {
                        ServiceQuotationId = quotation.Id,
                        ItemName = quotationType == "Personal" ? "Thay nhớt" : quotationType == "Insurance" ? "Sơn cản sau" : "Bảo dưỡng định kỳ",
                        Quantity = 1,
                        UnitPrice = quotationType == "Insurance" ? 1500000 : quotationType == "Corporate" ? 1200000 : 500000,
                        SubTotal = quotationType == "Insurance" ? 1500000 : quotationType == "Corporate" ? 1200000 : 500000,
                        VATRate = 0.10m,
                        VATAmount = (quotationType == "Insurance" ? 1500000 : quotationType == "Corporate" ? 1200000 : 500000) * 0.10m,
                        TotalPrice = (quotationType == "Insurance" ? 1500000 : quotationType == "Corporate" ? 1200000 : 500000) * 1.10m,
                        TotalAmount = (quotationType == "Insurance" ? 1500000 : quotationType == "Corporate" ? 1200000 : 500000) * 1.10m,
                        IsVATApplicable = true,
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    await _unitOfWork.Repository<QuotationItem>().AddAsync(item);

                    // Update totals
                    quotation.SubTotal = item.SubTotal;
                    quotation.VATAmount = item.VATAmount;
                    quotation.TotalAmount = item.TotalAmount;

                    await _unitOfWork.ServiceQuotations.UpdateAsync(quotation);
                    await _unitOfWork.SaveChangesAsync();
                    return quotation.Id;
                }

                var id1 = await CreateOne("Personal", "01");
                var id2 = await CreateOne("Insurance", "02");
                var id3 = await CreateOne("Corporate", "03");

                return Ok(ApiResponse<string>.SuccessResult($"Created demo quotations: {id1}, {id2}, {id3}"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResult("Lỗi khi tạo dữ liệu demo báo giá", ex));
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Policy = "ApiScope")]
        public async Task<ActionResult<ApiResponse<ServiceQuotationDto>>> UpdateQuotationStatus(int id, [FromBody] UpdateQuotationStatusDto statusDto)
        {
            try
            {
                var quotation = await _unitOfWork.ServiceQuotations.GetByIdAsync(id);
                if (quotation == null)
                {
                    return NotFound(ApiResponse<ServiceQuotationDto>.ErrorResult("Quotation not found"));
                }

                // ✅ THÊM: Chỉ cho phép chuyển trạng thái xe cá nhân từ Pending sang Approved
                if (quotation.QuotationType == "Personal" && quotation.Status == "Pending" && statusDto.Status == "Approved")
                {
                    quotation.Status = statusDto.Status;
                    quotation.UpdatedAt = DateTime.Now;
                    
                    await _unitOfWork.ServiceQuotations.UpdateAsync(quotation);
                    await _unitOfWork.SaveChangesAsync();

                    var result = await _unitOfWork.ServiceQuotations.GetByIdWithDetailsAsync(id);
                    var quotationDto = _mapper.Map<ServiceQuotationDto>(result);

                    return Ok(ApiResponse<ServiceQuotationDto>.SuccessResult(quotationDto));
                }
                else
                {
                    return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult("Không thể chuyển trạng thái này"));
                }
            }
            catch (Exception ex)
            {
                // Log error
                return StatusCode(500, ApiResponse<ServiceQuotationDto>.ErrorResult("Error updating quotation status", ex.Message));
            }
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
                VehicleType = vehicle.VehicleType,
                InsuranceCompany = vehicle.InsuranceCompany,
                PolicyNumber = vehicle.PolicyNumber,
                CoverageType = vehicle.CoverageType,
                ClaimNumber = vehicle.ClaimNumber,
                AdjusterName = vehicle.AdjusterName,
                AdjusterPhone = vehicle.AdjusterPhone,
                CompanyName = vehicle.CompanyName,
                TaxCode = vehicle.TaxCode,
                ContactPerson = vehicle.ContactPerson,
                ContactPhone = vehicle.ContactPhone,
                Department = vehicle.Department,
                CostCenter = vehicle.CostCenter,
                Customer = vehicle.Customer != null ? new CustomerDto
                {
                    Id = vehicle.Customer.Id,
                    Name = vehicle.Customer.Name,
                    Phone = vehicle.Customer.Phone,
                    Email = vehicle.Customer.Email
                } : null
            };
        }
    }
}

