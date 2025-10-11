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
    public class ServiceQuotationsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ServiceQuotationsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ServiceQuotationDto>>>> GetQuotations()
        {
            try
            {
                var quotations = await _unitOfWork.ServiceQuotations.GetAllWithDetailsAsync();
                var quotationDtos = quotations.Select(MapToDto).ToList();
                
                return Ok(ApiResponse<List<ServiceQuotationDto>>.SuccessResult(quotationDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ServiceQuotationDto>>.ErrorResult("Error retrieving quotations", ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ServiceQuotationDto>>> GetQuotation(int id)
        {
            try
            {
                var quotation = await _unitOfWork.ServiceQuotations.GetByIdWithDetailsAsync(id);
                if (quotation == null)
                {
                    return NotFound(ApiResponse<ServiceQuotationDto>.ErrorResult("Quotation not found"));
                }

                var quotationDto = MapToDto(quotation);
                return Ok(ApiResponse<ServiceQuotationDto>.SuccessResult(quotationDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceQuotationDto>.ErrorResult("Error retrieving quotation", ex.Message));
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
                return StatusCode(500, ApiResponse<List<ServiceQuotationDto>>.ErrorResult("Error retrieving quotations", ex.Message));
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
                return StatusCode(500, ApiResponse<List<ServiceQuotationDto>>.ErrorResult("Error retrieving quotations", ex.Message));
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
                return StatusCode(500, ApiResponse<List<ServiceQuotationDto>>.ErrorResult("Error retrieving quotations", ex.Message));
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
                return StatusCode(500, ApiResponse<List<ServiceQuotationDto>>.ErrorResult("Error retrieving quotations", ex.Message));
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
                    return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult("Invalid data", errors));
                }

                // Validate vehicle and customer exist
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(createDto.VehicleId);
                if (vehicle == null)
                {
                    return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult("Vehicle not found"));
                }

                var customer = await _unitOfWork.Customers.GetByIdAsync(createDto.CustomerId);
                if (customer == null)
                {
                    return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult("Customer not found"));
                }

                // Create quotation
                var quotation = new Core.Entities.ServiceQuotation
                {
                    QuotationNumber = await _unitOfWork.ServiceQuotations.GenerateQuotationNumberAsync(),
                    VehicleInspectionId = createDto.VehicleInspectionId,
                    CustomerId = createDto.CustomerId,
                    VehicleId = createDto.VehicleId,
                    PreparedById = createDto.PreparedById,
                    QuotationDate = DateTime.Now,
                    ValidUntil = createDto.ValidUntil ?? DateTime.Now.AddDays(7), // Default 7 days
                    Description = createDto.Description,
                    Terms = createDto.Terms,
                    TaxRate = createDto.TaxRate,
                    DiscountAmount = createDto.DiscountAmount,
                    Status = "Draft"
                };

                // Add items and calculate totals
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
                        TotalPrice = itemDto.UnitPrice * itemDto.Quantity,
                        IsOptional = itemDto.IsOptional,
                        IsApproved = false,
                        Notes = itemDto.Notes
                    };

                    quotation.Items.Add(item);
                    subTotal += item.TotalPrice;
                }

                quotation.SubTotal = subTotal;
                quotation.TaxAmount = subTotal * (quotation.TaxRate / 100);
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

                // Reload with details
                quotation = await _unitOfWork.ServiceQuotations.GetByIdWithDetailsAsync(quotation.Id);
                var quotationDto = MapToDto(quotation!);

                return CreatedAtAction(nameof(GetQuotation), new { id = quotation.Id }, 
                    ApiResponse<ServiceQuotationDto>.SuccessResult(quotationDto, "Quotation created successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceQuotationDto>.ErrorResult("Error creating quotation", ex.Message));
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
                    return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult("Invalid data", errors));
                }

                var quotation = await _unitOfWork.ServiceQuotations.GetByIdAsync(id);
                if (quotation == null)
                {
                    return NotFound(ApiResponse<ServiceQuotationDto>.ErrorResult("Quotation not found"));
                }

                // Update quotation
                quotation.ValidUntil = updateDto.ValidUntil;
                quotation.Description = updateDto.Description;
                quotation.Terms = updateDto.Terms;
                quotation.TaxRate = updateDto.TaxRate;
                quotation.DiscountAmount = updateDto.DiscountAmount;
                quotation.CustomerNotes = updateDto.CustomerNotes;
                quotation.RejectionReason = updateDto.RejectionReason;

                if (!string.IsNullOrEmpty(updateDto.Status))
                {
                    quotation.Status = updateDto.Status;
                }

                // Recalculate totals
                quotation.TaxAmount = quotation.SubTotal * (quotation.TaxRate / 100);
                quotation.TotalAmount = quotation.SubTotal + quotation.TaxAmount - quotation.DiscountAmount;

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
                    return NotFound(ApiResponse<ServiceQuotationDto>.ErrorResult("Quotation not found"));
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
                    return NotFound(ApiResponse<ServiceQuotationDto>.ErrorResult("Quotation not found"));
                }

                if (quotation.Status != "Draft")
                {
                    return BadRequest(ApiResponse<ServiceQuotationDto>.ErrorResult("Only draft quotations can be sent"));
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

        private static ServiceQuotationDto MapToDto(Core.Entities.ServiceQuotation quotation)
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
                    Notes = item.Notes,
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
                    return NotFound(ApiResponse<ServiceQuotationDto>.ErrorResult("Quotation not found"));
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
                    return NotFound(ApiResponse<ServiceQuotationDto>.ErrorResult("Quotation not found"));
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

