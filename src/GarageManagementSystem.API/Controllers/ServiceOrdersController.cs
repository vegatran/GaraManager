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

        public ServiceOrdersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
                return StatusCode(500, ApiResponse<List<ServiceOrderDto>>.ErrorResult("Error retrieving service orders", ex.Message));
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
                    return NotFound(ApiResponse<ServiceOrderDto>.ErrorResult("Service order not found"));
                }

                var orderDto = MapToDto(order);
                return Ok(ApiResponse<ServiceOrderDto>.SuccessResult(orderDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceOrderDto>.ErrorResult("Error retrieving service order", ex.Message));
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
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult("Invalid data", errors));
                }

                // Validate customer and vehicle exist
                var customer = await _unitOfWork.Customers.GetByIdAsync(createDto.CustomerId);
                if (customer == null)
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult("Customer not found"));
                }

                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(createDto.VehicleId);
                if (vehicle == null)
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult("Vehicle not found"));
                }

                // Đặt PaymentStatus dựa trên VehicleType
                string paymentStatus = vehicle.VehicleType switch
                {
                    "Insurance" => "InsurancePending", // Chờ phê duyệt bảo hiểm
                    "Company" => "CompanyPending",     // Chờ phê duyệt công ty
                    _ => "Pending"                     // Thanh toán khách hàng
                };

                // Tạo đơn hàng
                var order = new Core.Entities.ServiceOrder
                {
                    OrderNumber = GenerateOrderNumber(),
                    CustomerId = createDto.CustomerId,
                    VehicleId = createDto.VehicleId,
                    OrderDate = DateTime.Now,
                    ScheduledDate = createDto.ScheduledDate,
                    Status = "Pending",
                    Notes = createDto.Notes,
                    DiscountAmount = createDto.DiscountAmount,
                    PaymentStatus = paymentStatus
                };

                // Calculate totals and add service items
                decimal totalAmount = 0;
                foreach (var itemDto in createDto.ServiceOrderItems)
                {
                    var service = await _unitOfWork.Services.GetByIdAsync(itemDto.ServiceId);
                    if (service == null)
                    {
                        return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult($"Service with ID {itemDto.ServiceId} not found"));
                    }

                    var orderItem = new Core.Entities.ServiceOrderItem
                    {
                        ServiceId = itemDto.ServiceId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = service.Price,
                        TotalPrice = service.Price * itemDto.Quantity,
                        Notes = itemDto.Notes
                    };

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

                // Reload with details
                order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(order.Id);
                var orderDto = MapToDto(order!);

                return CreatedAtAction(nameof(GetServiceOrder), new { id = order.Id }, 
                    ApiResponse<ServiceOrderDto>.SuccessResult(orderDto, "Service order created successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceOrderDto>.ErrorResult("Error creating service order", ex.Message));
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
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult("Invalid data", errors));
                }

                var order = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
                if (order == null)
                {
                    return NotFound(ApiResponse<ServiceOrderDto>.ErrorResult("Service order not found"));
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
                    return NotFound(ApiResponse<ServiceOrderDto>.ErrorResult("Service order not found"));
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

        private static ServiceOrderDto MapToDto(Core.Entities.ServiceOrder order)
        {
            return new ServiceOrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerId = order.CustomerId,
                VehicleId = order.VehicleId,
                Customer = order.Customer != null ? new CustomerDto
                {
                    Id = order.Customer.Id,
                    Name = order.Customer.Name,
                    Email = order.Customer.Email,
                    Phone = order.Customer.Phone,
                    Address = order.Customer.Address
                } : null,
                Vehicle = order.Vehicle != null ? new VehicleDto
                {
                    Id = order.Vehicle.Id,
                    LicensePlate = order.Vehicle.LicensePlate,
                    Brand = order.Vehicle.Brand,
                    Model = order.Vehicle.Model,
                    Year = order.Vehicle.Year,
                    Color = order.Vehicle.Color
                } : null,
                OrderDate = order.OrderDate,
                ScheduledDate = order.ScheduledDate,
                CompletedDate = order.CompletedDate,
                Status = order.Status,
                Notes = order.Notes,
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.DiscountAmount,
                FinalAmount = order.FinalAmount,
                PaymentStatus = order.PaymentStatus,
                ServiceOrderItems = order.ServiceOrderItems?.Select(item => new ServiceOrderItemDto
                {
                    Id = item.Id,
                    ServiceOrderId = item.ServiceOrderId,
                    ServiceId = item.ServiceId,
                    Service = item.Service != null ? new ServiceDto
                    {
                        Id = item.Service.Id,
                        Name = item.Service.Name,
                        Description = item.Service.Description,
                        Price = item.Service.Price,
                        Duration = item.Service.Duration,
                        Category = item.Service.Category
                    } : null,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice,
                    Notes = item.Notes,
                    CreatedAt = item.CreatedAt,
                    UpdatedAt = item.UpdatedAt
                }).ToList() ?? new List<ServiceOrderItemDto>(),
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };
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
                    return NotFound(ApiResponse<ServiceOrderDto>.ErrorResult("Service order not found"));
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

