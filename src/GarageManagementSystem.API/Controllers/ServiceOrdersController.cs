using AutoMapper;
using GarageManagementSystem.Core.Enums;
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
    public class ServiceOrdersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ServiceOrdersController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ServiceOrderDto>>>> GetServiceOrders()
        {
            try
            {
                var orders = await _unitOfWork.ServiceOrders.GetAllWithDetailsAsync();
                var orderDtos = orders.Select(MapToDto).ToList();
                
                return Ok(ApiResponse<List<ServiceOrderDto>>.SuccessResult(orderDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ServiceOrderDto>>.ErrorResult("Lỗi khi lấy danh sách phiếu sửa chữa", ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ServiceOrderDto>>> GetServiceOrder(int id)
        {
            try
            {
                var order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                if (order == null)
                {
                    return NotFound(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                var orderDto = MapToDto(order);
                return Ok(ApiResponse<ServiceOrderDto>.SuccessResult(orderDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceOrderDto>.ErrorResult("Lỗi khi lấy thông tin phiếu sửa chữa", ex.Message));
            }
        }

        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<ApiResponse<List<ServiceOrderDto>>>> GetOrdersByCustomer(int customerId)
        {
            try
            {
                var orders = await _unitOfWork.ServiceOrders.GetByCustomerIdAsync(customerId);
                var orderDtos = orders.Select(MapToDto).ToList();

                return Ok(ApiResponse<List<ServiceOrderDto>>.SuccessResult(orderDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ServiceOrderDto>>.ErrorResult("Error retrieving orders", ex.Message));
            }
        }

        [HttpGet("vehicle/{vehicleId}")]
        public async Task<ActionResult<ApiResponse<List<ServiceOrderDto>>>> GetOrdersByVehicle(int vehicleId)
        {
            try
            {
                var orders = await _unitOfWork.ServiceOrders.GetByVehicleIdAsync(vehicleId);
                var orderDtos = orders.Select(MapToDto).ToList();

                return Ok(ApiResponse<List<ServiceOrderDto>>.SuccessResult(orderDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ServiceOrderDto>>.ErrorResult("Error retrieving orders", ex.Message));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ServiceOrderDto>>> CreateServiceOrder(CreateServiceOrderDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
                }

                // Business Rule: Kiểm tra xem có ServiceQuotationId không
                if (createDto.ServiceQuotationId.HasValue)
                {
                    var quotation = await _unitOfWork.ServiceQuotations.GetByIdAsync(createDto.ServiceQuotationId.Value);
                    if (quotation == null)
                    {
                        return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy báo giá"));
                    }

                    // Business Rule: Chỉ cho phép tạo phiếu sửa chữa từ báo giá đã được phê duyệt
                    if (quotation.Status != "Approved")
                    {
                        return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult(
                            $"Không thể tạo phiếu sửa chữa. Báo giá phải ở trạng thái 'Đã Phê Duyệt'. Trạng thái hiện tại: {quotation.Status}"));
                    }

                    // Business Rule: Kiểm tra xem đã có phiếu sửa chữa cho báo giá này chưa
                    var existingOrder = await _unitOfWork.ServiceOrders.GetByServiceQuotationIdAsync(createDto.ServiceQuotationId.Value);
                    if (existingOrder != null)
                    {
                        return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult("Đã tồn tại phiếu sửa chữa cho báo giá này"));
                    }
                }

                // Business Rule: Kiểm tra xem có CustomerReceptionId không
                if (createDto.CustomerReceptionId.HasValue)
                {
                    var reception = await _unitOfWork.CustomerReceptions.GetByIdAsync(createDto.CustomerReceptionId.Value);
                    if (reception == null)
                    {
                        return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy phiếu tiếp đón"));
                    }

                    // Business Rule: Chỉ cho phép tạo phiếu sửa chữa từ CustomerReception đã hoàn thành kiểm tra
                    if (reception.Status != ReceptionStatus.Completed)
                    {
                        return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult(
                            $"Không thể tạo phiếu sửa chữa. Phiếu tiếp đón phải ở trạng thái 'Đã Hoàn Thành'. Trạng thái hiện tại: {reception.Status}"));
                    }
                }

                // Validate customer and vehicle exist
                var customer = await _unitOfWork.Customers.GetByIdAsync(createDto.CustomerId);
                if (customer == null)
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy khách hàng"));
                }

                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(createDto.VehicleId);
                if (vehicle == null)
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy xe"));
                }

                // Đặt PaymentStatus dựa trên VehicleType
                string paymentStatus = vehicle.VehicleType switch
                {
                    "Insurance" => "InsurancePending", // Chờ phê duyệt bảo hiểm
                    "Company" => "CompanyPending",     // Chờ phê duyệt công ty
                    _ => "Pending"                     // Thanh toán khách hàng
                };

                // Tạo đơn hàng bằng AutoMapper
                var order = _mapper.Map<Core.Entities.ServiceOrder>(createDto);
                order.OrderNumber = GenerateOrderNumber();
                order.PaymentStatus = paymentStatus;

                // Calculate totals and add service items
                decimal totalAmount = 0;
                foreach (var itemDto in createDto.ServiceOrderItems)
                {
                    var service = await _unitOfWork.Services.GetByIdAsync(itemDto.ServiceId);
                    if (service == null)
                    {
                        return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult($"Service with ID {itemDto.ServiceId} not found"));
                    }

                    var orderItem = _mapper.Map<Core.Entities.ServiceOrderItem>(itemDto);
                    orderItem.UnitPrice = service.Price;
                    orderItem.TotalPrice = service.Price * itemDto.Quantity;

                    order.ServiceOrderItems.Add(orderItem);
                    totalAmount += orderItem.TotalPrice;
                }

                order.TotalAmount = totalAmount;
                order.FinalAmount = totalAmount - order.DiscountAmount;

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.ServiceOrders.AddAsync(order);
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

                // Business Rule: CustomerReception status remains "Completed" after service order creation
                // No need to update status as it's already in final state
                if (createDto.CustomerReceptionId.HasValue)
                {
                    // CustomerReception status is already "Completed"
                    // ServiceOrder is a separate workflow step
                }

                // Reload with details
                order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(order.Id);
                var orderDto = MapToDto(order!);

                return CreatedAtAction(nameof(GetServiceOrder), new { id = order.Id }, 
                    ApiResponse<ServiceOrderDto>.SuccessResult(orderDto, "Tạo phiếu sửa chữa thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceOrderDto>.ErrorResult("Lỗi khi tạo phiếu sửa chữa", ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ServiceOrderDto>>> UpdateServiceOrder(int id, UpdateServiceOrderDto updateDto)
        {
            try
            {
                if (id != updateDto.Id)
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult("ID mismatch"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
                }

                var order = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
                if (order == null)
                {
                    return NotFound(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                // Update order properties
                order.ScheduledDate = updateDto.ScheduledDate;
                order.CompletedDate = updateDto.CompletedDate;
                order.Status = updateDto.Status ?? order.Status;
                order.Notes = updateDto.Notes;
                order.DiscountAmount = updateDto.DiscountAmount;
                order.PaymentStatus = updateDto.PaymentStatus ?? order.PaymentStatus;
                
                // Recalculate final amount if discount changed
                order.FinalAmount = order.TotalAmount - order.DiscountAmount;

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.ServiceOrders.UpdateAsync(order);
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
                order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                var orderDto = MapToDto(order!);

                return Ok(ApiResponse<ServiceOrderDto>.SuccessResult(orderDto, "Service order updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceOrderDto>.ErrorResult("Error updating service order", ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteServiceOrder(int id)
        {
            try
            {
                var order = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
                if (order == null)
                {
                    return NotFound(ApiResponse.ErrorResult("Service order not found"));
                }

                await _unitOfWork.ServiceOrders.DeleteAsync(order);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse.SuccessResult("Service order deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResult("Error deleting service order", ex.Message));
            }
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<ApiResponse<List<ServiceOrderDto>>>> GetOrdersByStatus(string status)
        {
            try
            {
                var orders = await _unitOfWork.ServiceOrders.GetByStatusAsync(status);
                var orderDtos = orders.Select(MapToDto).ToList();

                return Ok(ApiResponse<List<ServiceOrderDto>>.SuccessResult(orderDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ServiceOrderDto>>.ErrorResult("Error retrieving orders", ex.Message));
            }
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult<ApiResponse<ServiceOrderDto>>> UpdateOrderStatus(int id, [FromBody] string status)
        {
            try
            {
                var order = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
                if (order == null)
                {
                    return NotFound(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                order.Status = status;
                
                // Auto-set completed date if status is Completed
                if (status == "Completed" && !order.CompletedDate.HasValue)
                {
                    order.CompletedDate = DateTime.Now;
                }

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.ServiceOrders.UpdateAsync(order);
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
                order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                var orderDto = MapToDto(order!);

                return Ok(ApiResponse<ServiceOrderDto>.SuccessResult(orderDto, "Order status updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceOrderDto>.ErrorResult("Error updating order status", ex.Message));
            }
        }


        private static string GenerateOrderNumber()
        {
            return $"SO-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }

        private ServiceOrderDto MapToDto(Core.Entities.ServiceOrder order)
        {
            return _mapper.Map<ServiceOrderDto>(order);
        }

        /// <summary>
        /// Lấy đơn hàng dịch vụ theo trạng thái thanh toán
        /// </summary>
        [HttpGet("by-payment-status/{paymentStatus}")]
        public async Task<ActionResult<ApiResponse<List<ServiceOrderDto>>>> GetOrdersByPaymentStatus(string paymentStatus)
        {
            try
            {
                var orders = await _unitOfWork.ServiceOrders.GetAllWithDetailsAsync();
                var filteredOrders = orders.Where(o => o.PaymentStatus == paymentStatus).ToList();
                var orderDtos = filteredOrders.Select(MapToDto).ToList();

                return Ok(ApiResponse<List<ServiceOrderDto>>.SuccessResult(orderDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ServiceOrderDto>>.ErrorResult("Error retrieving orders by payment status", ex.Message));
            }
        }

        /// <summary>
        /// Lấy đơn hàng dịch vụ theo loại xe
        /// </summary>
        [HttpGet("by-vehicle-type/{vehicleType}")]
        public async Task<ActionResult<ApiResponse<List<ServiceOrderDto>>>> GetOrdersByVehicleType(string vehicleType)
        {
            try
            {
                if (!new[] { "Personal", "Insurance", "Company" }.Contains(vehicleType))
                {
                    return BadRequest(ApiResponse<List<ServiceOrderDto>>.ErrorResult("Invalid vehicle type. Must be Personal, Insurance, or Company"));
                }

                var orders = await _unitOfWork.ServiceOrders.GetAllWithDetailsAsync();
                var filteredOrders = orders.Where(o => o.Vehicle.VehicleType == vehicleType).ToList();
                var orderDtos = filteredOrders.Select(MapToDto).ToList();

                return Ok(ApiResponse<List<ServiceOrderDto>>.SuccessResult(orderDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ServiceOrderDto>>.ErrorResult("Error retrieving orders by vehicle type", ex.Message));
            }
        }

        /// <summary>
        /// Cập nhật trạng thái thanh toán cho đơn hàng dịch vụ
        /// </summary>
        [HttpPut("{id}/payment-status")]
        public async Task<ActionResult<ApiResponse<ServiceOrderDto>>> UpdatePaymentStatus(int id, [FromBody] UpdatePaymentStatusDto updateDto)
        {
            try
            {
                var order = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
                if (order == null)
                {
                    return NotFound(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                // Validate payment status transition
                if (!IsValidPaymentStatusTransition(order.PaymentStatus, updateDto.PaymentStatus))
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult($"Invalid payment status transition from {order.PaymentStatus} to {updateDto.PaymentStatus}"));
                }

                order.PaymentStatus = updateDto.PaymentStatus;
                if (!string.IsNullOrEmpty(updateDto.PaymentNotes))
                {
                    order.Notes = (order.Notes ?? "") + $"\nPayment Status Update: {updateDto.PaymentNotes}";
                }

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.ServiceOrders.UpdateAsync(order);
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

                var updatedOrder = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                var orderDto = MapToDto(updatedOrder!);

                return Ok(ApiResponse<ServiceOrderDto>.SuccessResult(orderDto, "Payment status updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceOrderDto>.ErrorResult("Error updating payment status", ex.Message));
            }
        }

        /// <summary>
        /// Lấy trạng thái quy trình đơn hàng dịch vụ
        /// </summary>
        [HttpGet("{id}/workflow-status")]
        public async Task<ActionResult<ApiResponse<ServiceOrderWorkflowStatusDto>>> GetServiceOrderWorkflowStatus(int id)
        {
            try
            {
                var order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                if (order == null)
                {
                    return NotFound(ApiResponse<ServiceOrderWorkflowStatusDto>.ErrorResult("Service order not found"));
                }

                var status = new ServiceOrderWorkflowStatusDto
                {
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    VehicleType = order.Vehicle.VehicleType,
                    CurrentStatus = order.Status,
                    PaymentStatus = order.PaymentStatus,
                    RequiredApprovals = GetRequiredApprovalsForOrder(order),
                    NextRecommendedSteps = GetNextRecommendedStepsForOrder(order),
                    IsWorkflowBlocked = IsOrderWorkflowBlocked(order),
                    BlockingReasons = GetOrderBlockingReasons(order)
                };

                return Ok(ApiResponse<ServiceOrderWorkflowStatusDto>.SuccessResult(status));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceOrderWorkflowStatusDto>.ErrorResult("Error retrieving workflow status", ex.Message));
            }
        }

        private static bool IsValidPaymentStatusTransition(string currentStatus, string newStatus)
        {
            // Define valid payment status transitions
            var validTransitions = new Dictionary<string, List<string>>
            {
                ["Pending"] = new List<string> { "Paid", "Cancelled", "InsurancePending", "CompanyPending" },
                ["InsurancePending"] = new List<string> { "InsuranceApproved", "InsuranceRejected", "Cancelled" },
                ["CompanyPending"] = new List<string> { "CompanyApproved", "CompanyRejected", "Cancelled" },
                ["InsuranceApproved"] = new List<string> { "Paid", "Cancelled" },
                ["CompanyApproved"] = new List<string> { "Paid", "Cancelled" },
                ["Paid"] = new List<string> { "Refunded" },
                ["Cancelled"] = new List<string>(), // No transitions from cancelled
                ["Refunded"] = new List<string>()   // No transitions from refunded
            };

            return validTransitions.ContainsKey(currentStatus) && validTransitions[currentStatus].Contains(newStatus);
        }

        private static List<string> GetRequiredApprovalsForOrder(Core.Entities.ServiceOrder order)
        {
            var approvals = new List<string>();

            if (order.Vehicle.VehicleType == "Insurance")
            {
                approvals.Add("Insurance Company Approval");
                approvals.Add("Customer Payment (Deductible)");
            }
            else if (order.Vehicle.VehicleType == "Company")
            {
                approvals.Add("Company Manager Approval");
                approvals.Add("Purchase Order");
            }
            else
            {
                approvals.Add("Customer Payment");
            }

            return approvals;
        }

        private static List<string> GetNextRecommendedStepsForOrder(Core.Entities.ServiceOrder order)
        {
            var steps = new List<string>();

            // Based on current payment status and vehicle type
            switch (order.PaymentStatus)
            {
                case "Pending":
                    if (order.Vehicle.VehicleType == "Insurance")
                    {
                        steps.Add("Submit quotation to insurance company");
                        steps.Add("Wait for insurance approval");
                    }
                    else if (order.Vehicle.VehicleType == "Company")
                    {
                        steps.Add("Submit quotation to company manager");
                        steps.Add("Wait for company approval");
                    }
                    else
                    {
                        steps.Add("Collect payment from customer");
                    }
                    break;

                case "InsurancePending":
                    steps.Add("Wait for insurance company response");
                    steps.Add("Follow up with insurance adjuster");
                    break;

                case "CompanyPending":
                    steps.Add("Wait for company manager approval");
                    steps.Add("Follow up with company contact");
                    break;

                case "InsuranceApproved":
                case "CompanyApproved":
                    steps.Add("Collect remaining payment (deductible/balance)");
                    steps.Add("Process payment");
                    break;

                case "Paid":
                    if (order.Status != "Completed")
                    {
                        steps.Add("Start service work");
                        steps.Add("Complete all services");
                        steps.Add("Update order status to Completed");
                    }
                    else
                    {
                        steps.Add("Deliver vehicle to customer");
                    }
                    break;

                case "Cancelled":
                    steps.Add("No further action required");
                    break;
            }

            return steps;
        }

        private static bool IsOrderWorkflowBlocked(Core.Entities.ServiceOrder order)
        {
            // Check if order is blocked due to payment status
            return order.PaymentStatus == "InsuranceRejected" || order.PaymentStatus == "CompanyRejected";
        }

        private static List<string> GetOrderBlockingReasons(Core.Entities.ServiceOrder order)
        {
            var reasons = new List<string>();

            if (order.PaymentStatus == "InsuranceRejected")
            {
                reasons.Add("Insurance company rejected the claim");
            }
            else if (order.PaymentStatus == "CompanyRejected")
            {
                reasons.Add("Company rejected the service order");
            }

            return reasons;
        }
    }
}

