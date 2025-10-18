using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Enums;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Core.Services;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
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

        public ServiceQuotationsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ServiceQuotationDto>>>> GetQuotations()
        {
            try
            {
                var quotations = await _unitOfWork.ServiceQuotations.GetAllWithDetailsAsync();
                var quotationDtos = quotations.Select(MapToDto).ToList();
                
                // ✅ DEBUG: Log totals for QT2025002
                var qt2025002 = quotationDtos.FirstOrDefault(q => q.QuotationNumber == "QT2025002");
                if (qt2025002 != null)
                {
                    Console.WriteLine($"DEBUG: QT2025002 in list - TotalAmount: {qt2025002.TotalAmount}, SubTotal: {qt2025002.SubTotal}, TaxAmount: {qt2025002.TaxAmount}");
                }
                
                return Ok(ApiResponse<List<ServiceQuotationDto>>.SuccessResult(quotationDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ServiceQuotationDto>>.ErrorResult("Lỗi khi lấy danh sách báo giá", ex.Message));
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<ServiceQuotationDto>>> GetQuotation(int id)
        {
            try
            {
                // ✅ DEBUG: Log request
                Console.WriteLine($"DEBUG: GetQuotation called with id = {id}");
                
                var quotation = await _unitOfWork.ServiceQuotations.GetByIdWithDetailsAsync(id);
                if (quotation == null)
                {
                    Console.WriteLine($"DEBUG: Quotation with id {id} not found");
                    return NotFound(ApiResponse<ServiceQuotationDto>.ErrorResult("Không tìm thấy báo giá"));
                }

                // ✅ DEBUG: Log entity data
                Console.WriteLine($"DEBUG: Quotation found - Number: {quotation.QuotationNumber}, Customer: {quotation.Customer?.Name}, Vehicle: {quotation.Vehicle?.LicensePlate}");
                Console.WriteLine($"DEBUG: Entity totals - SubTotal: {quotation.SubTotal}, TaxAmount: {quotation.TaxAmount}, TotalAmount: {quotation.TotalAmount}");
                Console.WriteLine($"DEBUG: Items count: {quotation.Items?.Count ?? 0}");

                var quotationDto = MapToDto(quotation);
                
                // ✅ DEBUG: Log DTO data
                Console.WriteLine($"DEBUG: DTO mapped - Number: {quotationDto.QuotationNumber}, Customer: {quotationDto.Customer?.Name}, Vehicle: {quotationDto.Vehicle?.LicensePlate}");
                Console.WriteLine($"DEBUG: DTO totals - SubTotal: {quotationDto.SubTotal}, TaxAmount: {quotationDto.TaxAmount}, TotalAmount: {quotationDto.TotalAmount}");
                Console.WriteLine($"DEBUG: DTO Items count: {quotationDto.Items?.Count ?? 0}");
                
                return Ok(ApiResponse<ServiceQuotationDto>.SuccessResult(quotationDto));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Exception in GetQuotation: {ex.Message}");
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
                quotation.Status = "Draft";

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
                        // Remove existing items (hard delete để tránh duplicate)
                        var existingItems = await _unitOfWork.ServiceQuotationItems.GetAllAsync();
                        var itemsToDelete = existingItems.Where(i => i.ServiceQuotationId == id).ToList();
                        foreach (var item in itemsToDelete)
                        {
                            // Hard delete thay vì soft delete để tránh duplicate
                            await _unitOfWork.ServiceQuotationItems.DeleteAsync(item);
                        }

                        // Add new items with pricing models
                        foreach (var itemDto in updateDto.Items)
                        {
                            // ✅ DEBUG: Xem data nhận được từ client
                            Console.WriteLine($"🔍 DEBUG API - ItemDto: {itemDto.ItemName}, HasInvoice: {itemDto.HasInvoice}");
                            
                            var newItem = _mapper.Map<QuotationItem>(itemDto);
                            newItem.ServiceQuotationId = id;
                            newItem.CreatedAt = DateTime.Now;
                            newItem.UpdatedAt = DateTime.Now;
                            
                            // ✅ DEBUG: Xem data sau khi map
                            Console.WriteLine($"🔍 DEBUG API - After Map: {newItem.ItemName}, HasInvoice: {newItem.HasInvoice}");

                            // Apply pricing model from Service
                            if (newItem.ServiceId.HasValue)
                            {
                                var service = await _unitOfWork.Services.GetByIdAsync(newItem.ServiceId.Value);
                                if (service != null)
                                {
                                    PricingService.ApplyPricingToQuotationItem(newItem, service);
                                }
                                else
                                {
                                    // Fallback to manual pricing
                                    newItem.TotalPrice = newItem.Quantity * newItem.UnitPrice;
                                }
                            }
                            else
                            {
                                // Manual pricing (no service)
                                newItem.TotalPrice = newItem.Quantity * newItem.UnitPrice;
                            }

                            await _unitOfWork.ServiceQuotationItems.AddAsync(newItem);
                        }
                    }

                    // ✅ SỬA: Recalculate totals từ items mới
                    // Reload quotation với items mới
                    quotation = await _unitOfWork.ServiceQuotations.GetByIdWithDetailsAsync(id);
                    
                    // Cập nhật SubTotal từ items
                    quotation.SubTotal = quotation.Items.Where(x => !x.IsDeleted).Sum(item => item.UnitPrice * item.Quantity);
                    
                    // Chỉ tính VAT trên các items có IsVATApplicable = true
                    var vatApplicableItems = quotation.Items.Where(item => item.IsVATApplicable && !item.IsDeleted).ToList();
                    quotation.TaxAmount = vatApplicableItems.Sum(item => item.UnitPrice * item.Quantity * item.VATRate / 100);
                    quotation.TotalAmount = quotation.SubTotal + quotation.TaxAmount - quotation.DiscountAmount;

                    // Save all changes
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
                quotation = await _unitOfWork.ServiceQuotations.GetByIdWithDetailsAsync(id);
                var quotationDto = MapToDto(quotation!);

                return Ok(ApiResponse<ServiceQuotationDto>.SuccessResult(quotationDto, "Quotation updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceQuotationDto>.ErrorResult("Error updating quotation", ex.Message));
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
                        Status = "Pending",
                        TotalAmount = quotation.TotalAmount,
                        DiscountAmount = quotation.DiscountAmount,
                        FinalAmount = quotation.TotalAmount,
                        PaymentStatus = paymentStatus
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
        public async Task<ActionResult<ApiResponse<ServiceQuotationDto>>> RejectQuotation(int id, [FromBody] string rejectionReason)
        {
            try
            {
                var quotation = await _unitOfWork.ServiceQuotations.GetByIdAsync(id);
                if (quotation == null)
                {
                    return NotFound(ApiResponse<ServiceQuotationDto>.ErrorResult("Không tìm thấy báo giá"));
                }

                quotation.Status = "Rejected";
                quotation.RejectedDate = DateTime.Now;
                quotation.RejectionReason = rejectionReason;

                await _unitOfWork.ServiceQuotations.UpdateAsync(quotation);
                await _unitOfWork.SaveChangesAsync();

                // Reload with details
                quotation = await _unitOfWork.ServiceQuotations.GetByIdWithDetailsAsync(id);
                var quotationDto = MapToDto(quotation!);

                return Ok(ApiResponse<ServiceQuotationDto>.SuccessResult(quotationDto, "Quotation rejected"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceQuotationDto>.ErrorResult("Error rejecting quotation", ex.Message));
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

                if (quotation.Status != "Draft")
                {
                    return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult("Chỉ có thể gửi báo giá ở trạng thái 'Nháp'"));
                }

                quotation.Status = "Sent";
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

                if (quotation.Status == "Approved" && quotation.ServiceOrderId.HasValue)
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

