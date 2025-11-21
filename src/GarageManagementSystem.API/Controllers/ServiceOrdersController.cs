using AutoMapper;
using GarageManagementSystem.Core.Enums;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Core.Extensions;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using GarageManagementSystem.API.Services;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace GarageManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiScope")]
    public class ServiceOrdersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly GarageDbContext _context;
        private readonly ICOGSCalculationService _cogsCalculationService;
        private readonly ILogger<ServiceOrdersController> _logger;

        public ServiceOrdersController(
            IUnitOfWork unitOfWork, 
            IMapper mapper, 
            ICacheService cacheService, 
            GarageDbContext context,
            ICOGSCalculationService cogsCalculationService,
            ILogger<ServiceOrdersController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
            _context = context;
            _cogsCalculationService = cogsCalculationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<ServiceOrderDto>>> GetServiceOrders(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? status = null,
            [FromQuery] int? customerId = null)
        {
            try
            {
                // ✅ OPTIMIZED: Query ở database level thay vì load tất cả vào memory
                // Build IQueryable từ DbContext để filter và paginate ở database level
                var query = _context.ServiceOrders
                    .Where(so => !so.IsDeleted)
                    .AsQueryable();
                
                // Apply search filter if provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(so => 
                        so.OrderNumber != null && so.OrderNumber.Contains(searchTerm));
                }
                
                // Apply status filter if provided
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(so => so.Status == status);
                }
                
                // Apply customer filter if provided
                if (customerId.HasValue)
                {
                    query = query.Where(so => so.CustomerId == customerId.Value);
                }

                // Order by OrderDate descending
                query = query.OrderByDescending(so => so.OrderDate);

                // ✅ OPTIMIZED: Apply Include before pagination
                query = query
                    .Include(so => so.Customer)
                    .Include(so => so.Vehicle)
                    .Include(so => so.ServiceOrderItems.Where(item => !item.IsDeleted))
                        .ThenInclude(item => item.Service)
                    .Include(so => so.ServiceOrderItems.Where(item => !item.IsDeleted))
                        .ThenInclude(item => item.AssignedTechnician);
                
                // ✅ OPTIMIZED: Get paged results with total count - automatically chooses best method
                var (orders, totalCount) = await query.ToPagedListWithCountAsync(pageNumber, pageSize, _context);
                
                var orderDtos = new List<ServiceOrderDto>();
                foreach (var order in orders)
                {
                    orderDtos.Add(await MapToDtoAsync(order));
                }
                
                return Ok(PagedResponse<ServiceOrderDto>.CreateSuccessResult(
                    orderDtos, pageNumber, pageSize, totalCount, "Service orders retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service orders");
                return StatusCode(500, PagedResponse<ServiceOrderDto>.CreateErrorResult("Lỗi khi lấy danh sách phiếu sửa chữa"));
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

                var orderDto = await MapToDtoAsync(order);
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
                var orderDtos = new List<ServiceOrderDto>();
                foreach (var order in orders)
                {
                    orderDtos.Add(await MapToDtoAsync(order));
                }

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
                var orderDtos = new List<ServiceOrderDto>();
                foreach (var order in orders)
                {
                    orderDtos.Add(await MapToDtoAsync(order));
                }

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
                    // Cho phép thiếu ServiceOrderItems nếu có ServiceQuotationId (server sẽ tự copy từ báo giá)
                    var itemsError = ModelState.ContainsKey(nameof(CreateServiceOrderDto.ServiceOrderItems))
                        ? ModelState[nameof(CreateServiceOrderDto.ServiceOrderItems)]!.Errors.Count
                        : 0;
                    var totalErrors = ModelState.Values.Sum(v => v.Errors.Count);
                    var allowByQuotation = createDto.ServiceQuotationId.HasValue && totalErrors == itemsError && itemsError > 0;

                    if (!allowByQuotation)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult("Dữ liệu không hợp lệ", errors));
                    }
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

                    // ✅ SỬA: Kiểm tra xem đã có phiếu sửa chữa cho báo giá này chưa
                    // Nếu có ServiceOrder chưa xóa (IsDeleted = false) thì không cho phép tạo mới
                    var existingActiveOrder = await _unitOfWork.ServiceOrders.GetByServiceQuotationIdAsync(createDto.ServiceQuotationId.Value);
                    if (existingActiveOrder != null)
                    {
                        return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult("Đã tồn tại phiếu sửa chữa cho báo giá này"));
                    }
                    
                    // ✅ THÊM: Nếu có ServiceOrder đã bị xóa (IsDeleted = true), clear ServiceQuotationId của record cũ để tránh unique constraint
                    // Sử dụng IgnoreQueryFilters() để query cả các record với IsDeleted = true
                    var existingDeletedOrders = await _context.ServiceOrders
                        .IgnoreQueryFilters() // Bỏ qua filter mặc định (!IsDeleted)
                        .Where(so => so.ServiceQuotationId == createDto.ServiceQuotationId.Value && so.IsDeleted)
                        .ToListAsync();
                    
                    if (existingDeletedOrders != null && existingDeletedOrders.Any())
                    {
                        foreach (var deletedOrder in existingDeletedOrders)
                        {
                            deletedOrder.ServiceQuotationId = null; // Clear để tránh unique constraint
                        }
                        await _context.SaveChangesAsync(); // Save ngay để clear ServiceQuotationId trước khi tạo mới
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

                Core.Entities.ServiceQuotation? sourceQuotation = null;
                if (createDto.ServiceQuotationId.HasValue)
                {
                    sourceQuotation = await _unitOfWork.ServiceQuotations.GetByIdWithDetailsAsync(createDto.ServiceQuotationId.Value);
                    if (sourceQuotation == null)
                    {
                        return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy báo giá gốc"));
                    }
                }

                // Tạo đơn hàng bằng AutoMapper
                var order = _mapper.Map<Core.Entities.ServiceOrder>(createDto);
                order.OrderNumber = GenerateOrderNumber();
                order.PaymentStatus = paymentStatus;
                // ✅ Đảm bảo Status được set mặc định là "Pending" nếu chưa có
                if (string.IsNullOrEmpty(order.Status))
                {
                    order.Status = "Pending";
                }

                // Calculate totals and add service items
                decimal totalAmount = 0;
                if (sourceQuotation != null && (createDto.ServiceOrderItems == null || !createDto.ServiceOrderItems.Any()))
                {
                    // Tự động copy items từ báo giá nếu client không gửi service items
                    var quotationItems = sourceQuotation.Items
                        .Where(i => !i.IsDeleted)
                        .ToList();
                    foreach (var qItem in quotationItems)
                    {
                        // ✅ SỬA: Xử lý labor items (tiền công) không có ServiceId/PartId
                        if (qItem.ItemCategory == "Labor" || (!qItem.ServiceId.HasValue && !qItem.PartId.HasValue))
                        {
                            // Labor items (tiền công) - ServiceId = null, dùng ItemName làm ServiceName
                            var orderItem = new Core.Entities.ServiceOrderItem
                            {
                                ServiceId = null, // Labor items không có ServiceId
                                ServiceName = qItem.ItemName, // Dùng ItemName (ví dụ: "Công sửa chữa động cơ")
                                Quantity = qItem.Quantity,
                                UnitPrice = qItem.UnitPrice,
                                TotalPrice = qItem.Quantity * qItem.UnitPrice,
                                Notes = qItem.Notes ?? qItem.ItemName,
                                Description = qItem.Description
                            };
                            order.ServiceOrderItems.Add(orderItem);
                            totalAmount += orderItem.TotalPrice;
                            order.ServiceTotal += orderItem.TotalPrice;
                        }
                        else if (qItem.ServiceId.HasValue)
                        {
                            var service = await _unitOfWork.Services.GetByIdAsync(qItem.ServiceId.Value);
                            if (service == null)
                            {
                                return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult($"Không tìm thấy dịch vụ ID {qItem.ServiceId} từ báo giá"));
                            }
                            var orderItem = new Core.Entities.ServiceOrderItem
                            {
                                ServiceId = qItem.ServiceId.Value,
                                ServiceName = service.Name,
                                Quantity = qItem.Quantity,
                                UnitPrice = qItem.UnitPrice,
                                TotalPrice = qItem.Quantity * qItem.UnitPrice,
                                Notes = qItem.ItemName
                            };
                            order.ServiceOrderItems.Add(orderItem);
                            totalAmount += orderItem.TotalPrice;
                            order.ServiceTotal += orderItem.TotalPrice;
                        }
                        else if (qItem.PartId.HasValue)
                        {
                            var part = await _unitOfWork.Parts.GetByIdAsync(qItem.PartId.Value);
                            if (part == null)
                            {
                                return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult($"Không tìm thấy phụ tùng ID {qItem.PartId} từ báo giá"));
                            }
                            var orderPart = new Core.Entities.ServiceOrderPart
                            {
                                PartId = part.Id,
                            PartName = !string.IsNullOrEmpty(qItem.ItemName) ? qItem.ItemName : part.PartName,
                                Quantity = qItem.Quantity,
                                UnitCost = part.AverageCostPrice,
                                UnitPrice = qItem.UnitPrice,
                                TotalPrice = qItem.Quantity * qItem.UnitPrice,
                                Notes = qItem.ItemName
                            };
                            order.ServiceOrderParts.Add(orderPart);
                            totalAmount += orderPart.TotalPrice;
                            order.PartsTotal += orderPart.TotalPrice;
                        }
                        else
                        {
                            // Trường hợp này không nên xảy ra, nhưng để an toàn vẫn log
                            return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult($"Item báo giá không hợp lệ: {qItem.ItemName}"));
                        }
                    }
                }
                else
                {
                    foreach (var itemDto in createDto.ServiceOrderItems)
                    {
                        // ✅ SỬA: Kiểm tra ServiceId có giá trị không (có thể null cho labor items)
                        if (!itemDto.ServiceId.HasValue)
                        {
                            // Labor items không có ServiceId, dùng ServiceName
                            if (string.IsNullOrEmpty(itemDto.ServiceName))
                            {
                                return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult("Labor items phải có ServiceName"));
                            }
                            var laborItem = new Core.Entities.ServiceOrderItem
                            {
                                ServiceId = null,
                                ServiceName = itemDto.ServiceName,
                                Quantity = itemDto.Quantity,
                                UnitPrice = 0, // Sẽ được tính sau
                                TotalPrice = 0,
                                Notes = itemDto.ServiceName
                            };
                            order.ServiceOrderItems.Add(laborItem);
                            totalAmount += 0; // Sẽ được tính sau
                            order.ServiceTotal += 0;
                            continue;
                        }
                        
                        var service = await _unitOfWork.Services.GetByIdAsync(itemDto.ServiceId.Value);
                    if (service == null)
                    {
                            return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult($"Service with ID {itemDto.ServiceId.Value} not found"));
                        }

                        var serviceOrderItem = _mapper.Map<Core.Entities.ServiceOrderItem>(itemDto);
                        serviceOrderItem.UnitPrice = service.Price;
                        serviceOrderItem.TotalPrice = service.Price * itemDto.Quantity;

                        order.ServiceOrderItems.Add(serviceOrderItem);
                        totalAmount += serviceOrderItem.TotalPrice;
                    }
                }

                var serviceTotal = order.ServiceOrderItems.Sum(i => i.TotalPrice);
                var partsTotal = order.ServiceOrderParts.Sum(p => p.TotalPrice);

                order.ServiceTotal = serviceTotal;
                order.PartsTotal = partsTotal;

                if (sourceQuotation != null)
                {
                    order.SubTotal = sourceQuotation.SubTotal;
                    order.VATAmount = sourceQuotation.VATAmount;
                    order.DiscountAmount = sourceQuotation.DiscountAmount;
                    order.TotalAmount = sourceQuotation.TotalAmount;
                    order.FinalAmount = sourceQuotation.TotalAmount;
                }
                else
                {
                    order.SubTotal = totalAmount;
                    order.VATAmount = 0;
                    order.TotalAmount = totalAmount;
                    order.FinalAmount = totalAmount - order.DiscountAmount;
                }

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.ServiceOrders.AddAsync(order);
                    await _unitOfWork.SaveChangesAsync();

                    // Commit transaction nếu thành công
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch(Exception ex)
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
                var orderDto = await MapToDtoAsync(order!);

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

                // ✅ Business Rule: Không cho phép edit khi đã ReadyToWork, InProgress, Completed
                // Phải dùng endpoint /change-status để chuyển trạng thái theo workflow
                var restrictedStatuses = new[] { "ReadyToWork", "InProgress", "Completed" };
                if (restrictedStatuses.Contains(order.Status))
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult(
                        $"Không thể chỉnh sửa phiếu sửa chữa ở trạng thái '{order.Status}'. " +
                        $"Để thay đổi trạng thái, vui lòng sử dụng chức năng 'Chuyển Trạng Thái' theo workflow."));
                }

                // ✅ Business Rule: Không cho phép edit Status trực tiếp trong UpdateServiceOrder
                // Status phải được thay đổi qua endpoint /change-status để đảm bảo workflow validation
                if (!string.IsNullOrEmpty(updateDto.Status) && updateDto.Status != order.Status)
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult(
                        "Không thể thay đổi trạng thái trực tiếp. Vui lòng sử dụng endpoint '/change-status' để chuyển trạng thái theo workflow."));
                }

                // Update order properties (không cho phép update Status)
                order.ScheduledDate = updateDto.ScheduledDate;
                order.CompletedDate = updateDto.CompletedDate;
                // order.Status = updateDto.Status ?? order.Status; // ❌ REMOVE: Không cho phép edit Status
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
                var orderDto = await MapToDtoAsync(order!);

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
                var order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                if (order == null)
                {
                    return NotFound(ApiResponse.ErrorResult("Service order not found"));
                }

                // ✅ HP1: Validate khi hủy Service Order có items đang "InProgress"
                if (order.ServiceOrderItems != null && order.ServiceOrderItems.Any())
                {
                    var inProgressItems = order.ServiceOrderItems
                        .Where(item => item.Status == "InProgress" || item.Status == "In Progress")
                        .ToList();
                    
                    if (inProgressItems.Count > 0)
                    {
                        var itemNames = inProgressItems
                            .Select(item => item.Service?.Name ?? item.ServiceName ?? $"Hạng mục #{item.Id}")
                            .ToList();
                        
                        var errorMessage = $"Không thể hủy phiếu sửa chữa vì có {inProgressItems.Count} hạng mục đang làm việc: " +
                            string.Join(", ", itemNames.Take(5)) + 
                            (itemNames.Count > 5 ? $" và {itemNames.Count - 5} hạng mục khác" : "") +
                            ". Vui lòng hoàn thành hoặc dừng các hạng mục này trước khi hủy.";
                        
                        return BadRequest(ApiResponse.ErrorResult(errorMessage));
                    }
                    
                    // ✅ HP1: Check items có giờ công thực tế (đã bắt đầu làm việc)
                    var startedItems = order.ServiceOrderItems
                        .Where(item => item.StartTime.HasValue && item.Status != "Completed" && item.Status != "Cancelled")
                        .ToList();
                    
                    if (startedItems.Count > 0)
                    {
                        var itemNames = startedItems
                            .Select(item => item.Service?.Name ?? item.ServiceName ?? $"Hạng mục #{item.Id}")
                            .ToList();
                        
                        var errorMessage = $"Không thể hủy phiếu sửa chữa vì có {startedItems.Count} hạng mục đã bắt đầu làm việc: " +
                            string.Join(", ", itemNames.Take(5)) + 
                            (itemNames.Count > 5 ? $" và {itemNames.Count - 5} hạng mục khác" : "") +
                            ". Vui lòng hoàn thành hoặc dừng các hạng mục này trước khi hủy.";
                        
                        return BadRequest(ApiResponse.ErrorResult(errorMessage));
                    }
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
                var orderDtos = new List<ServiceOrderDto>();
                foreach (var order in orders)
                {
                    orderDtos.Add(await MapToDtoAsync(order));
                }

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
                var orderDto = await MapToDtoAsync(order!);

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

        private async Task<ServiceOrderDto> MapToDtoAsync(Core.Entities.ServiceOrder order)
        {
            var dto = _mapper.Map<ServiceOrderDto>(order);
            
            // ✅ THÊM: Map ServiceQuotationId
            dto.ServiceQuotationId = order.ServiceQuotationId;
            
            // ✅ 2.3.3: Tính tổng tiền LSC Bổ sung nếu là JO gốc
            if (!order.IsAdditionalOrder && order.Id > 0)
            {
                // ✅ OPTIMIZED: Query tất cả LSC Bổ sung liên kết với JO gốc này ở database level
                var additionalOrders = (await _unitOfWork.Repository<Core.Entities.ServiceOrder>()
                    .FindAsync(o => o.ParentServiceOrderId == order.Id && !o.IsDeleted))
                    .ToList();
                
                dto.AdditionalOrdersTotalAmount = additionalOrders.Sum(o => o.TotalAmount);
                dto.GrandTotalAmount = order.TotalAmount + dto.AdditionalOrdersTotalAmount;
            }
            else
            {
                // Nếu là LSC Bổ sung, không tính tổng
                dto.AdditionalOrdersTotalAmount = 0;
                dto.GrandTotalAmount = order.TotalAmount;
            }
            
            // ✅ THÊM: Map ServiceName, AssignedTechnicianName, EstimatedHours, AssignedTechnicianId cho từng item
            if (dto.ServiceOrderItems != null)
            {
                foreach (var itemDto in dto.ServiceOrderItems)
                {
                    var item = order.ServiceOrderItems.FirstOrDefault(i => i.Id == itemDto.Id);
                    if (item != null)
                    {
                        // ✅ THÊM: Map ServiceName (ưu tiên từ ServiceName entity, fallback từ Service.Name)
                        if (!string.IsNullOrEmpty(item.ServiceName))
                        {
                            itemDto.ServiceName = item.ServiceName;
                        }
                        else if (item.Service != null && !string.IsNullOrEmpty(item.Service.Name))
                        {
                            itemDto.ServiceName = item.Service.Name;
                        }
                        
                        // ✅ SỬA: Map ServiceId (có thể null cho labor items)
                        itemDto.ServiceId = item.ServiceId;
                        
                        // Map AssignedTechnicianName
                        if (item.AssignedTechnician != null)
                        {
                            itemDto.AssignedTechnicianName = item.AssignedTechnician.Name;
                        }
                        // Map EstimatedHours
                        if (item.EstimatedHours.HasValue)
                        {
                            itemDto.EstimatedHours = item.EstimatedHours;
                        }
                        // Map AssignedTechnicianId
                        if (item.AssignedTechnicianId.HasValue)
                        {
                            itemDto.AssignedTechnicianId = item.AssignedTechnicianId;
                        }
                        
                        // ✅ 2.3.1: Map StartTime, EndTime, ActualHours, CompletedTime
                        itemDto.StartTime = item.StartTime;
                        itemDto.EndTime = item.EndTime;
                        itemDto.ActualHours = item.ActualHours;
                        itemDto.CompletedTime = item.CompletedTime;
                        
                        // ✅ 2.4.3: Map ReworkHours
                        itemDto.ReworkHours = item.ReworkHours;
                    }
                }
            }
            
            // ✅ 2.4: Map QC và Bàn giao fields
            dto.TotalActualHours = order.TotalActualHours;
            dto.QCFailedCount = order.QCFailedCount;
            dto.HandoverDate = order.HandoverDate;
            dto.HandoverLocation = order.HandoverLocation;

            if (order.ServiceOrderFees != null && order.ServiceOrderFees.Count > 0)
            {
                dto.ServiceOrderFees = _mapper.Map<List<ServiceOrderFeeDto>>(order.ServiceOrderFees.Where(f => !f.IsDeleted));
            }

            if (order.ServiceOrderParts != null && order.ServiceOrderParts.Count > 0)
            {
                dto.ServiceOrderParts = _mapper.Map<List<ServiceOrderPartDto>>(order.ServiceOrderParts.Where(p => !p.IsDeleted));
            }
            
            return dto;
        }

        /// <summary>
        /// Lấy đơn hàng dịch vụ theo trạng thái thanh toán
        /// </summary>
        [HttpGet("by-payment-status/{paymentStatus}")]
        public async Task<ActionResult<ApiResponse<List<ServiceOrderDto>>>> GetOrdersByPaymentStatus(string paymentStatus)
        {
            try
            {
                // ✅ OPTIMIZED: Query ở database level thay vì load tất cả vào memory
                var orders = await _context.ServiceOrders
                    .Where(so => !so.IsDeleted && so.PaymentStatus == paymentStatus)
                    .Include(so => so.Customer)
                    .Include(so => so.Vehicle)
                    .Include(so => so.ServiceOrderItems)
                        .ThenInclude(item => item.Service)
                    .Include(so => so.ServiceOrderItems)
                        .ThenInclude(item => item.AssignedTechnician)
                    .ToListAsync();
                
                var orderDtos = new List<ServiceOrderDto>();
                foreach (var order in orders)
                {
                    orderDtos.Add(await MapToDtoAsync(order));
                }

                return Ok(ApiResponse<List<ServiceOrderDto>>.SuccessResult(orderDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders by payment status");
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

                // ✅ OPTIMIZED: Query ở database level thay vì load tất cả vào memory
                var orders = await _context.ServiceOrders
                    .Where(so => !so.IsDeleted && so.Vehicle != null && so.Vehicle.VehicleType == vehicleType)
                    .Include(so => so.Customer)
                    .Include(so => so.Vehicle)
                    .Include(so => so.ServiceOrderItems)
                        .ThenInclude(item => item.Service)
                    .Include(so => so.ServiceOrderItems)
                        .ThenInclude(item => item.AssignedTechnician)
                    .ToListAsync();
                
                var orderDtos = new List<ServiceOrderDto>();
                foreach (var order in orders)
                {
                    orderDtos.Add(await MapToDtoAsync(order));
                }

                return Ok(ApiResponse<List<ServiceOrderDto>>.SuccessResult(orderDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders by vehicle type");
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
                var orderDto = await MapToDtoAsync(updatedOrder!);

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

        /// <summary>
        /// ✅ 2.1.1: Chuyển trạng thái ServiceOrder
        /// </summary>
        [HttpPut("{id}/change-status")]
        public async Task<ActionResult<ApiResponse<ServiceOrderDto>>> ChangeOrderStatus(int id, ChangeServiceOrderStatusDto statusDto)
        {
            try
            {
                var order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                if (order == null)
                {
                    return NotFound(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                // Validate status transition
                var currentStatus = order.Status;
                var newStatus = statusDto.Status;

                // ✅ HP1: Validate khi chuyển sang "Cancelled" - Check items đang làm việc
                if (newStatus == "Cancelled")
                {
                    if (order.ServiceOrderItems != null && order.ServiceOrderItems.Any())
                    {
                        var inProgressItems = order.ServiceOrderItems
                            .Where(item => item.Status == "InProgress" || item.Status == "In Progress")
                            .ToList();
                        
                        if (inProgressItems.Count > 0)
                        {
                            var itemNames = inProgressItems
                                .Select(item => item.Service?.Name ?? item.ServiceName ?? $"Hạng mục #{item.Id}")
                                .ToList();
                            
                            var errorMessage = $"Không thể hủy phiếu sửa chữa vì có {inProgressItems.Count} hạng mục đang làm việc: " +
                                string.Join(", ", itemNames.Take(5)) + 
                                (itemNames.Count > 5 ? $" và {itemNames.Count - 5} hạng mục khác" : "") +
                                ". Vui lòng hoàn thành hoặc dừng các hạng mục này trước khi hủy.";
                            
                            return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult(errorMessage));
                        }
                        
                        // Check items có giờ công thực tế (đã bắt đầu làm việc)
                        var startedItems = order.ServiceOrderItems
                            .Where(item => item.StartTime.HasValue && item.Status != "Completed" && item.Status != "Cancelled")
                            .ToList();
                        
                        if (startedItems.Count > 0)
                        {
                            var itemNames = startedItems
                                .Select(item => item.Service?.Name ?? item.ServiceName ?? $"Hạng mục #{item.Id}")
                                .ToList();
                            
                            var errorMessage = $"Không thể hủy phiếu sửa chữa vì có {startedItems.Count} hạng mục đã bắt đầu làm việc: " +
                                string.Join(", ", itemNames.Take(5)) + 
                                (itemNames.Count > 5 ? $" và {itemNames.Count - 5} hạng mục khác" : "") +
                                ". Vui lòng hoàn thành hoặc dừng các hạng mục này trước khi hủy.";
                            
                            return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult(errorMessage));
                        }
                    }
                }

                // Allowed transitions:
                // "Pending" -> "PendingAssignment" -> "WaitingForParts" / "ReadyToWork" -> "InProgress" -> "Completed"
                bool isValidTransition = (currentStatus == "Pending" && newStatus == "PendingAssignment") ||
                                         (currentStatus == "PendingAssignment" && (newStatus == "WaitingForParts" || newStatus == "ReadyToWork")) ||
                                         ((currentStatus == "WaitingForParts" || currentStatus == "ReadyToWork") && newStatus == "InProgress") ||
                                         (currentStatus == "InProgress" && newStatus == "Completed") ||
                                         // ✅ HP1: Allow cancel from any status except Completed
                                         (newStatus == "Cancelled" && currentStatus != "Completed");

                if (!isValidTransition && currentStatus != newStatus)
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult(
                        $"Không thể chuyển từ trạng thái '{currentStatus}' sang '{newStatus}'. Chuyển đổi không hợp lệ."));
                }

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    order.Status = newStatus;
                    if (!string.IsNullOrEmpty(statusDto.Notes))
                    {
                        order.Notes = string.IsNullOrEmpty(order.Notes) 
                            ? statusDto.Notes 
                            : $"{order.Notes}\n{statusDto.Notes}";
                    }

                    // Nếu chuyển sang "PendingAssignment", khóa Quotation editing
                    if (newStatus == "PendingAssignment" && order.ServiceQuotationId.HasValue)
                    {
                        var quotation = await _unitOfWork.ServiceQuotations.GetByIdAsync(order.ServiceQuotationId.Value);
                        if (quotation != null)
                        {
                            // Set flag để lock (có thể thêm field IsLocked nếu cần)
                            // Tạm thời check qua ServiceOrderId
                        }
                    }

                    await _unitOfWork.ServiceOrders.UpdateAsync(order);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    // Reload with details
                    order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                    var orderDto = await MapToDtoAsync(order!);

                    return Ok(ApiResponse<ServiceOrderDto>.SuccessResult(orderDto, $"Đã chuyển trạng thái thành '{newStatus}'"));
                }
                catch (Exception innerEx)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceOrderDto>.ErrorResult("Lỗi khi chuyển trạng thái", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 2.1.2: Phân công KTV cho một item cụ thể
        /// </summary>
        [HttpPut("{id}/items/{itemId}/assign-technician")]
        public async Task<ActionResult<ApiResponse<ServiceOrderDto>>> AssignTechnicianToItem(
            int id, int itemId, AssignTechnicianDto assignDto)
        {
            try
            {
                // ✅ BỔ SUNG: Kiểm tra quyền - Chỉ Quản đốc/Tổ trưởng mới có quyền phân công
                var currentUserId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(currentUserId) && int.TryParse(currentUserId, out var userId))
                {
                    var currentEmployee = await _unitOfWork.Employees.GetByIdAsync(userId);
                    if (currentEmployee != null)
                    {
                        var position = (currentEmployee.Position ?? "").ToLower();
                        var positionName = currentEmployee.PositionNavigation?.Name?.ToLower() ?? "";
                        
                        // Kiểm tra Position: Quản đốc, Tổ trưởng, Quản lý, Manager, Supervisor
                        bool isAuthorized = position.Contains("quản đốc") || position.Contains("tổ trưởng") || 
                                           position.Contains("quản lý") || position.Contains("manager") ||
                                           position.Contains("supervisor") || position.Contains("supervise") ||
                                           positionName.Contains("quản đốc") || positionName.Contains("tổ trưởng") ||
                                           positionName.Contains("quản lý") || positionName.Contains("manager") ||
                                           positionName.Contains("supervisor");
                        
                        // Kiểm tra roles từ claims
                        var userRoles = User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role || c.Type == "role")
                                                   .Select(c => c.Value.ToLower()).ToList();
                        isAuthorized = isAuthorized || userRoles.Contains("manager") || userRoles.Contains("supervisor") || 
                                      userRoles.Contains("admin") || userRoles.Contains("superadmin");
                        
                        if (!isAuthorized)
                        {
                            return Forbid("Chỉ Quản đốc, Tổ trưởng hoặc Quản lý mới có quyền phân công KTV");
                        }
                    }
                }

                var order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                if (order == null)
                {
                    return NotFound(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                var item = order.ServiceOrderItems.FirstOrDefault(i => i.Id == itemId);
                if (item == null)
                {
                    return NotFound(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy hạng mục"));
                }

                // Validate technician exists
                var technician = await _unitOfWork.Employees.GetByIdAsync(assignDto.TechnicianId);
                if (technician == null)
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy KTV"));
                }

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    // ✅ THÊM: Ghi log khi đổi KTV (reassign)
                    var oldTechnicianId = item.AssignedTechnicianId;
                    var isReassign = oldTechnicianId.HasValue && oldTechnicianId.Value != assignDto.TechnicianId;
                    
                    if (isReassign)
                    {
                        var oldTechnician = await _unitOfWork.Employees.GetByIdAsync(oldTechnicianId.Value);
                        var oldTechnicianName = oldTechnician?.Name ?? $"KTV ID {oldTechnicianId.Value}";
                        
                        // Ghi log vào Notes
                        var reassignNote = $"\n[Đổi KTV] {DateTime.Now:dd/MM/yyyy HH:mm} - Từ: {oldTechnicianName} → Đến: {technician.Name}";
                        item.Notes = string.IsNullOrEmpty(item.Notes) 
                            ? reassignNote.Trim() 
                            : $"{item.Notes}{reassignNote}";
                    }
                    
                    item.AssignedTechnicianId = assignDto.TechnicianId;
                    item.EstimatedHours = assignDto.EstimatedHours;
                    
                    if (!string.IsNullOrEmpty(assignDto.Notes) && !isReassign)
                    {
                        item.Notes = string.IsNullOrEmpty(item.Notes) 
                            ? assignDto.Notes 
                            : $"{item.Notes}\nPhân công: {assignDto.Notes}";
                    }
                    else if (!string.IsNullOrEmpty(assignDto.Notes) && isReassign)
                    {
                        // Nếu là reassign và có Notes, thêm vào sau log reassign
                        item.Notes = $"{item.Notes}\nLý do: {assignDto.Notes}";
                    }

                    await _unitOfWork.Repository<Core.Entities.ServiceOrderItem>().UpdateAsync(item);
                    await _unitOfWork.SaveChangesAsync();

                    // ✅ OPTIMIZED: Find Appointment ở database level
                    var appointment = await _unitOfWork.Appointments
                        .FirstOrDefaultAsync(a => a.ServiceOrderId == id && !a.IsDeleted);
                    
                    if (appointment != null)
                    {
                        // Nếu chưa có AssignedToId, set KTV đầu tiên được phân công
                        if (!appointment.AssignedToId.HasValue)
                        {
                            appointment.AssignedToId = assignDto.TechnicianId;
                            await _unitOfWork.Appointments.UpdateAsync(appointment);
                        }
                        // Hoặc nếu muốn update EstimatedDuration dựa trên tổng EstimatedHours
                        if (assignDto.EstimatedHours.HasValue && order.ServiceOrderItems.Any())
                        {
                            var totalEstimatedHours = order.ServiceOrderItems
                                .Where(i => i.EstimatedHours.HasValue)
                                .Sum(i => i.EstimatedHours!.Value);
                            // Convert hours to minutes
                            appointment.EstimatedDuration = (int)(totalEstimatedHours * 60);
                            await _unitOfWork.Appointments.UpdateAsync(appointment);
                        }
                        await _unitOfWork.SaveChangesAsync();
                    }
                    else if (order.ScheduledDate.HasValue)
                    {
                        // ✅ Tạo Appointment mới nếu chưa có (tùy chọn)
                        // Chỉ tạo nếu có ScheduledDate và CustomerId
                        if (order.CustomerId > 0 && order.VehicleId > 0)
                        {
                            // ✅ OPTIMIZED: Count appointments ở database level
                            var todayPrefix = $"APT-{DateTime.Now:yyyyMMdd}";
                            var appointments = (await _unitOfWork.Appointments
                                .FindAsync(a => !a.IsDeleted && a.AppointmentNumber.StartsWith(todayPrefix))).ToList();
                            var count = appointments.Count;
                            var appointmentNumber = $"{todayPrefix}-{(count + 1):D4}";
                            
                            var newAppointment = new Core.Entities.Appointment
                            {
                                AppointmentNumber = appointmentNumber,
                                CustomerId = order.CustomerId,
                                VehicleId = order.VehicleId,
                                ScheduledDateTime = order.ScheduledDate.Value,
                                EstimatedDuration = assignDto.EstimatedHours.HasValue 
                                    ? (int)(assignDto.EstimatedHours.Value * 60) 
                                    : 60,
                                AppointmentType = "Service",
                                ServiceRequested = $"Sửa chữa theo JO: {order.OrderNumber}",
                                Status = "Scheduled",
                                AssignedToId = assignDto.TechnicianId,
                                ServiceOrderId = id,
                                CreatedAt = DateTime.Now
                            };
                            
                            await _unitOfWork.Appointments.AddAsync(newAppointment);
                            await _unitOfWork.SaveChangesAsync();
                        }
                    }

                    // Update order status if needed
                    if (order.Status == "PendingAssignment" && order.ServiceOrderItems.All(i => i.AssignedTechnicianId.HasValue))
                    {
                        // Tất cả items đã được phân công -> chuyển sang "ReadyToWork"
                        order.Status = "ReadyToWork";
                        await _unitOfWork.ServiceOrders.UpdateAsync(order);
                        await _unitOfWork.SaveChangesAsync();
                    }

                    await _unitOfWork.CommitTransactionAsync();

                    // ✅ HP2: Invalidate workload cache cho cả KTV cũ và mới
                    if (oldTechnicianId.HasValue)
                    {
                        await _cacheService.RemoveByPrefixAsync($"workload_{oldTechnicianId.Value}_");
                    }
                    await _cacheService.RemoveByPrefixAsync($"workload_{assignDto.TechnicianId}_");

                    // Reload with details
                    order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                    var orderDto = await MapToDtoAsync(order!);

                    return Ok(ApiResponse<ServiceOrderDto>.SuccessResult(orderDto, isReassign ? "Đã đổi KTV thành công" : "Đã phân công KTV thành công"));
                }
                catch (Exception innerEx)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceOrderDto>.ErrorResult("Lỗi khi phân công KTV", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 2.1.2: Phân công hàng loạt cho nhiều items
        /// </summary>
        [HttpPut("{id}/bulk-assign-technician")]
        public async Task<ActionResult<ApiResponse<ServiceOrderDto>>> BulkAssignTechnician(
            int id, BulkAssignTechnicianDto bulkDto)
        {
            try
            {
                // ✅ BỔ SUNG: Kiểm tra quyền - Chỉ Quản đốc/Tổ trưởng mới có quyền phân công
                var currentUserId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(currentUserId) && int.TryParse(currentUserId, out var userId))
                {
                    var currentEmployee = await _unitOfWork.Employees.GetByIdAsync(userId);
                    if (currentEmployee != null)
                    {
                        var position = (currentEmployee.Position ?? "").ToLower();
                        var positionName = currentEmployee.PositionNavigation?.Name?.ToLower() ?? "";
                        
                        bool isAuthorized = position.Contains("quản đốc") || position.Contains("tổ trưởng") || 
                                           position.Contains("quản lý") || position.Contains("manager") ||
                                           position.Contains("supervisor") || position.Contains("supervise") ||
                                           positionName.Contains("quản đốc") || positionName.Contains("tổ trưởng") ||
                                           positionName.Contains("quản lý") || positionName.Contains("manager") ||
                                           positionName.Contains("supervisor");
                        
                        var userRoles = User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role || c.Type == "role")
                                                   .Select(c => c.Value.ToLower()).ToList();
                        isAuthorized = isAuthorized || userRoles.Contains("manager") || userRoles.Contains("supervisor") || 
                                      userRoles.Contains("admin") || userRoles.Contains("superadmin");
                        
                        if (!isAuthorized)
                        {
                            return Forbid("Chỉ Quản đốc, Tổ trưởng hoặc Quản lý mới có quyền phân công KTV");
                        }
                    }
                }

                var order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                if (order == null)
                {
                    return NotFound(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                // Validate technician exists
                var technician = await _unitOfWork.Employees.GetByIdAsync(bulkDto.TechnicianId);
                if (technician == null)
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy KTV"));
                }

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var itemsToUpdate = bulkDto.ItemIds == null || !bulkDto.ItemIds.Any()
                        ? order.ServiceOrderItems.ToList()
                        : order.ServiceOrderItems.Where(i => bulkDto.ItemIds.Contains(i.Id)).ToList();

                    foreach (var item in itemsToUpdate)
                    {
                        item.AssignedTechnicianId = bulkDto.TechnicianId;
                        if (bulkDto.EstimatedHours.HasValue)
                        {
                            item.EstimatedHours = bulkDto.EstimatedHours;
                        }
                    }

                    foreach (var item in itemsToUpdate)
                    {
                        await _unitOfWork.Repository<Core.Entities.ServiceOrderItem>().UpdateAsync(item);
                    }
                    
                    await _unitOfWork.SaveChangesAsync();

                    // Update order status if needed
                    if (order.Status == "PendingAssignment" && order.ServiceOrderItems.All(i => i.AssignedTechnicianId.HasValue))
                    {
                        order.Status = "ReadyToWork";
                        await _unitOfWork.ServiceOrders.UpdateAsync(order);
                        await _unitOfWork.SaveChangesAsync();
                    }

                    await _unitOfWork.CommitTransactionAsync();

                    // ✅ HP2: Invalidate workload cache cho KTV được phân công
                    await _cacheService.RemoveByPrefixAsync($"workload_{bulkDto.TechnicianId}_");

                    // Reload with details
                    order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                    var orderDto = await MapToDtoAsync(order!);

                    return Ok(ApiResponse<ServiceOrderDto>.SuccessResult(orderDto, $"Đã phân công KTV cho {itemsToUpdate.Count} hạng mục"));
                }
                catch (Exception innerEx)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceOrderDto>.ErrorResult("Lỗi khi phân công KTV", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 2.1.2: Cập nhật giờ công dự kiến cho một item
        /// </summary>
        [HttpPut("{id}/items/{itemId}/set-estimated-hours")]
        public async Task<ActionResult<ApiResponse<ServiceOrderDto>>> SetEstimatedHours(
            int id, int itemId, [FromBody] decimal estimatedHours)
        {
            try
            {
                if (estimatedHours <= 0 || estimatedHours > 24)
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult("Giờ công dự kiến phải từ 0.1 đến 24 giờ"));
                }

                var order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                if (order == null)
                {
                    return NotFound(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                var item = order.ServiceOrderItems.FirstOrDefault(i => i.Id == itemId);
                if (item == null)
                {
                    return NotFound(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy hạng mục"));
                }

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    item.EstimatedHours = estimatedHours;
                    await _unitOfWork.Repository<Core.Entities.ServiceOrderItem>().UpdateAsync(item);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    // ✅ HP2: Invalidate workload cache cho KTV được phân công (nếu có)
                    if (item.AssignedTechnicianId.HasValue)
                    {
                        await _cacheService.RemoveByPrefixAsync($"workload_{item.AssignedTechnicianId.Value}_");
                    }

                    // Reload with details
                    order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                    var orderDto = await MapToDtoAsync(order!);

                    return Ok(ApiResponse<ServiceOrderDto>.SuccessResult(orderDto, "Đã cập nhật giờ công dự kiến"));
                }
                catch (Exception innerEx)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceOrderDto>.ErrorResult("Lỗi khi cập nhật giờ công", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 2.3.1: KTV bắt đầu làm việc cho một item
        /// </summary>
        [HttpPost("{id}/items/{itemId}/start-work")]
        public async Task<ActionResult<ApiResponse<ServiceOrderDto>>> StartItemWork(int id, int itemId)
        {
            try
            {
                var order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                if (order == null)
                {
                    return NotFound(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                var item = order.ServiceOrderItems.FirstOrDefault(i => i.Id == itemId);
                if (item == null)
                {
                    return NotFound(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy hạng mục"));
                }

                // Validate: Item phải có KTV được phân công và ở trạng thái Pending hoặc ReadyToWork
                if (!item.AssignedTechnicianId.HasValue)
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult("Hạng mục chưa được phân công KTV"));
                }

                // ✅ SỬA: Cho phép bắt đầu lại nếu đã dừng (status = Pending nhưng có StartTime và ActualHours)
                if (item.Status != "Pending" && item.Status != "ReadyToWork" && item.Status != "Ready To Work")
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult($"Không thể bắt đầu. Trạng thái hiện tại: {item.Status}"));
                }
                
                // ✅ Nếu đã có StartTime (đã làm trước đó), reset StartTime để bắt đầu lại từ đầu
                // Nhưng giữ ActualHours đã tính (cộng dồn)

                // Validate: KTV hiện tại có phải là KTV được phân công không (nếu có thông tin user)
                var currentUserId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(currentUserId) && int.TryParse(currentUserId, out var userId))
                {
                    if (item.AssignedTechnicianId.Value != userId)
                    {
                        // Cho phép nếu user là Quản đốc/Tổ trưởng hoặc Admin
                        var currentEmployee = await _unitOfWork.Employees.GetByIdAsync(userId);
                        if (currentEmployee != null)
                        {
                            var position = (currentEmployee.Position ?? "").ToLower();
                            var isAuthorized = position.Contains("quản đốc") || position.Contains("tổ trưởng") || 
                                              position.Contains("quản lý") || position.Contains("manager") ||
                                              position.Contains("supervisor") || position.Contains("admin");
                            
                            if (!isAuthorized)
                            {
                                return Forbid("Chỉ KTV được phân công mới có thể bắt đầu làm việc");
                            }
                        }
                    }
                }

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    // ✅ SỬA: Set StartTime mới (có thể đã có StartTime cũ từ lần làm trước)
                    // Reset EndTime về null khi bắt đầu lại
                    item.StartTime = DateTime.Now;
                    item.EndTime = null; // Reset EndTime khi bắt đầu lại
                    item.Status = "InProgress";
                    item.UpdatedAt = DateTime.Now;

                    await _unitOfWork.Repository<Core.Entities.ServiceOrderItem>().UpdateAsync(item);
                    await _unitOfWork.SaveChangesAsync();

                    // ✅ Nếu đây là item đầu tiên bắt đầu, cập nhật ServiceOrder.StartDate
                    if (!order.StartDate.HasValue)
                    {
                        order.StartDate = DateTime.Now;
                        if (order.Status == "ReadyToWork")
                        {
                            order.Status = "InProgress";
                        }
                        await _unitOfWork.ServiceOrders.UpdateAsync(order);
                        await _unitOfWork.SaveChangesAsync();
                    }

                    await _unitOfWork.CommitTransactionAsync();

                    // Reload with details
                    order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                    var orderDto = await MapToDtoAsync(order!);

                    return Ok(ApiResponse<ServiceOrderDto>.SuccessResult(orderDto, "Đã bắt đầu làm việc"));
                }
                catch (Exception innerEx)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceOrderDto>.ErrorResult("Lỗi khi bắt đầu làm việc", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 2.3.1: KTV dừng/tạm dừng làm việc cho một item (chưa hoàn thành)
        /// </summary>
        [HttpPost("{id}/items/{itemId}/stop-work")]
        public async Task<ActionResult<ApiResponse<ServiceOrderDto>>> StopItemWork(int id, int itemId, [FromBody] decimal? actualHours = null)
        {
            try
            {
                var order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                if (order == null)
                {
                    return NotFound(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                var item = order.ServiceOrderItems.FirstOrDefault(i => i.Id == itemId);
                if (item == null)
                {
                    return NotFound(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy hạng mục"));
                }

                if (item.Status != "InProgress")
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult($"Chỉ có thể dừng khi đang làm việc. Trạng thái hiện tại: {item.Status}"));
                }

                if (!item.StartTime.HasValue)
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult("Hạng mục chưa bắt đầu làm việc"));
                }

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    item.EndTime = DateTime.Now;
                    
                    // Tính ActualHours từ StartTime và EndTime nếu không được cung cấp
                    if (actualHours.HasValue)
                    {
                        item.ActualHours = actualHours.Value;
                    }
                    else if (item.StartTime.HasValue && item.EndTime.HasValue)
                    {
                        var timeSpan = item.EndTime.Value - item.StartTime.Value;
                        // ✅ SỬA: Cộng dồn ActualHours nếu đã có giờ công trước đó
                        var newActualHours = (decimal)timeSpan.TotalHours;
                        item.ActualHours = (item.ActualHours ?? 0) + newActualHours;
                    }

                    // ✅ SỬA: Khi dừng làm việc, chuyển status về "Pending" để có thể bắt đầu lại sau
                    // Giữ StartTime (không reset) và ActualHours đã tính để KTV có thể tiếp tục công việc
                    // StartTime sẽ được reset khi "Tiếp tục" (StartItemWork)
                    item.Status = "Pending";
                    // KHÔNG reset StartTime khi dừng - giữ nguyên để có thể tính ActualHours từ StartTime đến EndTime
                    
                    item.UpdatedAt = DateTime.Now;

                    await _unitOfWork.Repository<Core.Entities.ServiceOrderItem>().UpdateAsync(item);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    // Reload with details
                    order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                    var orderDto = await MapToDtoAsync(order!);

                    return Ok(ApiResponse<ServiceOrderDto>.SuccessResult(orderDto, "Đã dừng làm việc"));
                }
                catch (Exception innerEx)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceOrderDto>.ErrorResult("Lỗi khi dừng làm việc", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 2.3.1 và 2.3.4: KTV hoàn thành một item
        /// </summary>
        [HttpPost("{id}/items/{itemId}/complete")]
        public async Task<ActionResult<ApiResponse<ServiceOrderDto>>> CompleteItem(
            int id, int itemId, [FromBody] decimal? actualHours = null)
        {
            try
            {
                var order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                if (order == null)
                {
                    return NotFound(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                var item = order.ServiceOrderItems.FirstOrDefault(i => i.Id == itemId);
                if (item == null)
                {
                    return NotFound(ApiResponse<ServiceOrderDto>.ErrorResult("Không tìm thấy hạng mục"));
                }

                if (item.Status == "Completed")
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult("Hạng mục đã hoàn thành"));
                }

                if (item.Status != "InProgress" && item.Status != "Pending")
                {
                    return BadRequest(ApiResponse<ServiceOrderDto>.ErrorResult($"Không thể hoàn thành. Trạng thái hiện tại: {item.Status}"));
                }

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var now = DateTime.Now;
                    
                    // ✅ Logic tính ActualHours khi hoàn thành (đảm bảo cộng dồn đúng)
                    var currentActualHours = item.ActualHours ?? 0; // Giữ lại ActualHours đã có (từ lần dừng trước)
                    
                    if (actualHours.HasValue)
                    {
                        // Case 1: Có actualHours từ input → dùng giá trị đó (tổng cuối cùng, không cộng dồn)
                        item.ActualHours = actualHours.Value;
                    }
                    else if (item.Status == "InProgress" && item.StartTime.HasValue)
                    {
                        // Case 2: Đang làm việc (InProgress) → Tính từ StartTime hiện tại đến now và cộng dồn
                        var timeSpan = now - item.StartTime.Value;
                        var newActualHours = (decimal)timeSpan.TotalHours;
                        item.ActualHours = currentActualHours + newActualHours; // Cộng dồn
                    }
                    else if (item.Status == "Pending" && item.EndTime.HasValue)
                    {
                        // Case 3: Đã dừng (Pending) và có EndTime
                        // ActualHours đã được tính ở lần dừng trước (EndTime - StartTime) và cộng dồn
                        // Theo nghiệp vụ: Nếu hoàn thành từ trạng thái "Pending" (đã dừng), 
                        // thì không có thời gian làm việc thêm từ EndTime đến now
                        // → Giữ nguyên ActualHours đã có (không cần tính thêm)
                        // item.ActualHours = currentActualHours; // Không cần gán lại
                    }
                    else if (item.Status == "Pending" && !item.EndTime.HasValue && item.StartTime.HasValue)
                    {
                        // Case 4: Status = Pending nhưng không có EndTime
                        // Có thể xảy ra nếu:
                        // - Item chưa bao giờ dừng (trường hợp này không nên xảy ra vì status = Pending)
                        // - Item đã dừng và tiếp tục (StartTime đã reset, EndTime = null)
                        // → Tính từ StartTime hiện tại đến now và cộng dồn
                        var timeSpan = now - item.StartTime.Value;
                        var newActualHours = (decimal)timeSpan.TotalHours;
                        item.ActualHours = currentActualHours + newActualHours;
                    }
                    else if (!item.StartTime.HasValue && !item.ActualHours.HasValue && item.EstimatedHours.HasValue)
                    {
                        // Case 5: Chưa bao giờ bắt đầu → Fallback: dùng EstimatedHours
                        item.ActualHours = item.EstimatedHours.Value;
                    }
                    else if (item.ActualHours.HasValue)
                    {
                        // Case 6: Đã có ActualHours (từ lần dừng trước), giữ nguyên
                        // item.ActualHours = item.ActualHours; // Không cần gán lại
                    }
                    else
                    {
                        // Case 7: Không có gì cả → set = 0
                        item.ActualHours = 0;
                    }
                    
                    // ✅ Set StartTime nếu chưa có (để có thông tin đầy đủ)
                    if (!item.StartTime.HasValue)
                    {
                        item.StartTime = now;
                    }

                    item.EndTime = now;
                    item.CompletedTime = now;
                    item.Status = "Completed";

                    item.UpdatedAt = now;

                    await _unitOfWork.Repository<Core.Entities.ServiceOrderItem>().UpdateAsync(item);
                    await _unitOfWork.SaveChangesAsync();

                    // ✅ Kiểm tra: Nếu tất cả items đã hoàn thành, tự động cập nhật ServiceOrder status
                    var allItemsCompleted = order.ServiceOrderItems.All(i => i.Status == "Completed" || i.Status == "Cancelled");
                    if (allItemsCompleted && order.Status != "Completed")
                    {
                        order.Status = "Completed";
                        order.CompletedDate = now;
                        await _unitOfWork.ServiceOrders.UpdateAsync(order);
                        await _unitOfWork.SaveChangesAsync();
                    }

                    await _unitOfWork.CommitTransactionAsync();

                    // Reload with details
                    order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                    var orderDto = await MapToDtoAsync(order!);

                    return Ok(ApiResponse<ServiceOrderDto>.SuccessResult(orderDto, "Đã hoàn thành hạng mục"));
                }
                catch (Exception innerEx)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceOrderDto>.ErrorResult("Lỗi khi hoàn thành hạng mục", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 2.3.4: Lấy tiến độ chi tiết của Service Order
        /// </summary>
        [HttpGet("{id}/progress")]
        public async Task<ActionResult<ApiResponse<ServiceOrderProgressDto>>> GetServiceOrderProgress(int id)
        {
            try
            {
                var order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                if (order == null)
                {
                    return NotFound(ApiResponse<ServiceOrderProgressDto>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                // ✅ OPTIMIZED: Query items ở database level
                var items = (await _unitOfWork.Repository<Core.Entities.ServiceOrderItem>()
                    .FindAsync(i => i.ServiceOrderId == id && !i.IsDeleted))
                    .ToList();

                var totalItems = items.Count;
                var pendingItems = items.Count(i => i.Status == "Pending");
                var inProgressItems = items.Count(i => i.Status == "InProgress");
                var completedItems = items.Count(i => i.Status == "Completed");
                var onHoldItems = items.Count(i => i.Status == "OnHold");
                var cancelledItems = items.Count(i => i.Status == "Cancelled");

                // Tính progress percentage
                var progressPercentage = totalItems > 0 
                    ? (decimal)(completedItems * 100.0 / totalItems) 
                    : 0;

                // Tính tổng giờ công
                var totalEstimatedHours = items.Sum(i => i.EstimatedHours ?? 0);
                var totalActualHours = items.Sum(i => i.ActualHours ?? 0);
                
                // ✅ FIX: Giờ Công Còn Lại = Tổng Dự Kiến - Tổng Thực Tế (hoặc chỉ tính cho items chưa hoàn thành)
                // Logic 1: Tổng Dự Kiến - Tổng Thực Tế (tổng quát hơn)
                // var remainingEstimatedHours = totalEstimatedHours - totalActualHours;
                
                // Logic 2: Chỉ tính cho items chưa hoàn thành và trừ đi giờ công thực tế đã làm
                var incompleteItems = items.Where(i => i.Status != "Completed" && i.Status != "Cancelled").ToList();
                var remainingEstimatedHoursForIncomplete = incompleteItems.Sum(i => i.EstimatedHours ?? 0);
                var actualHoursForIncomplete = incompleteItems.Sum(i => i.ActualHours ?? 0);
                var remainingEstimatedHours = Math.Max(0, remainingEstimatedHoursForIncomplete - actualHoursForIncomplete);

                // Map items với progress
                var itemProgressDtos = items.Select(item => new ServiceOrderItemProgressDto
                {
                    ItemId = item.Id,
                    ItemName = item.ServiceName ?? item.Service?.Name ?? "N/A",
                    Status = item.Status ?? "Pending",
                    StartTime = item.StartTime,
                    EndTime = item.EndTime,
                    CompletedTime = item.CompletedTime,
                    EstimatedHours = item.EstimatedHours,
                    ActualHours = item.ActualHours,
                    ProgressPercentage = item.Status == "Completed" ? 100 : (item.Status == "InProgress" ? 50 : 0),
                    AssignedTechnicianId = item.AssignedTechnicianId,
                    AssignedTechnicianName = item.AssignedTechnician?.Name
                }).ToList();

                var progressDto = new ServiceOrderProgressDto
                {
                    ServiceOrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    TotalItems = totalItems,
                    PendingItems = pendingItems,
                    InProgressItems = inProgressItems,
                    CompletedItems = completedItems,
                    OnHoldItems = onHoldItems,
                    CancelledItems = cancelledItems,
                    ProgressPercentage = progressPercentage,
                    TotalEstimatedHours = totalEstimatedHours,
                    TotalActualHours = totalActualHours,
                    RemainingEstimatedHours = remainingEstimatedHours,
                    OrderDate = order.OrderDate,
                    StartDate = order.StartDate,
                    ExpectedCompletionDate = order.ScheduledDate,
                    ActualCompletionDate = order.CompletedDate,
                    Items = itemProgressDtos
                };

                return Ok(ApiResponse<ServiceOrderProgressDto>.SuccessResult(progressDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceOrderProgressDto>.ErrorResult("Lỗi khi lấy tiến độ", ex.Message));
            }
        }

        #region ✅ 3.1: COGS Calculation Endpoints

        /// <summary>
        /// ✅ 3.1: Tính COGS cho Service Order và lưu vào database
        /// </summary>
        [HttpPost("{id}/calculate-cogs")]
        public async Task<ActionResult<ApiResponse<COGSCalculationDto>>> CalculateCOGS(int id, [FromBody] COGSMethodDto? methodDto = null)
        {
            try
            {
                var serviceOrder = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
                if (serviceOrder == null)
                {
                    return NotFound(ApiResponse<COGSCalculationDto>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                // Lấy phương pháp tính (từ request hoặc từ ServiceOrder)
                var method = methodDto?.Method ?? serviceOrder.COGSCalculationMethod;
                if (string.IsNullOrWhiteSpace(method))
                {
                    method = "FIFO"; // Default
                }

                // Tính COGS
                var cogsResult = await _cogsCalculationService.CalculateCOGSAsync(id, method);

                // Lưu kết quả vào ServiceOrder
                // ✅ FIX: Null safety cho các properties
                serviceOrder.TotalCOGS = cogsResult.TotalCOGS;
                serviceOrder.COGSCalculationMethod = cogsResult.CalculationMethod ?? "FIFO";
                serviceOrder.COGSCalculationDate = cogsResult.CalculationDate;
                serviceOrder.COGSBreakdown = cogsResult.BreakdownJson; // Có thể null, OK

                await _unitOfWork.ServiceOrders.UpdateAsync(serviceOrder);
                await _unitOfWork.SaveChangesAsync();

                // Map sang DTO
                // ✅ FIX: Null safety cho ItemDetails và từng item
                var cogsDto = new COGSCalculationDto
                {
                    ServiceOrderId = cogsResult.ServiceOrderId,
                    CalculationMethod = cogsResult.CalculationMethod ?? string.Empty,
                    TotalCOGS = cogsResult.TotalCOGS,
                    CalculationDate = cogsResult.CalculationDate,
                    ItemDetails = (cogsResult.ItemDetails ?? new List<COGSItemDetail>())
                        .Where(item => item != null)
                        .Select(item => new COGSItemDetailDto
                        {
                            PartId = item.PartId,
                            PartName = item.PartName ?? string.Empty,
                            PartNumber = item.PartNumber ?? string.Empty,
                            QuantityUsed = item.QuantityUsed,
                            UnitCost = item.UnitCost,
                            TotalCost = item.TotalCost,
                            BatchNumber = item.BatchNumber,
                            BatchReceiveDate = item.BatchReceiveDate,
                            CalculationMethod = item.CalculationMethod ?? string.Empty
                        }).ToList()
                };

                return Ok(ApiResponse<COGSCalculationDto>.SuccessResult(cogsDto, $"Tính COGS thành công: {cogsResult.TotalCOGS:N0} VNĐ"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<COGSCalculationDto>.ErrorResult("Lỗi khi tính COGS", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 3.1: Lấy chi tiết breakdown COGS cho Service Order
        /// </summary>
        [HttpGet("{id}/cogs-details")]
        public async Task<ActionResult<ApiResponse<COGSBreakdownDto>>> GetCOGSDetails(int id)
        {
            try
            {
                var serviceOrder = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
                if (serviceOrder == null)
                {
                    return NotFound(ApiResponse<COGSBreakdownDto>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                // Lấy breakdown từ service
                var breakdownResult = await _cogsCalculationService.GetCOGSBreakdownAsync(id);

                // Map sang DTO
                // ✅ FIX: Null safety cho ItemDetails và từng item
                var breakdownDto = new COGSBreakdownDto
                {
                    ServiceOrderId = breakdownResult.ServiceOrderId,
                    CalculationMethod = breakdownResult.CalculationMethod ?? string.Empty,
                    TotalCOGS = breakdownResult.TotalCOGS,
                    CalculationDate = breakdownResult.CalculationDate,
                    ItemDetails = (breakdownResult.ItemDetails ?? new List<COGSItemDetail>())
                        .Where(item => item != null)
                        .Select(item => new COGSItemDetailDto
                        {
                            PartId = item.PartId,
                            PartName = item.PartName ?? string.Empty,
                            PartNumber = item.PartNumber ?? string.Empty,
                            QuantityUsed = item.QuantityUsed,
                            UnitCost = item.UnitCost,
                            TotalCost = item.TotalCost,
                            BatchNumber = item.BatchNumber,
                            BatchReceiveDate = item.BatchReceiveDate,
                            CalculationMethod = item.CalculationMethod ?? string.Empty
                        }).ToList()
                };

                return Ok(ApiResponse<COGSBreakdownDto>.SuccessResult(breakdownDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<COGSBreakdownDto>.ErrorResult("Lỗi khi lấy chi tiết COGS", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 3.1: Thiết lập phương pháp tính COGS cho Service Order
        /// </summary>
        [HttpPut("{id}/set-cogs-method")]
        public async Task<ActionResult<ApiResponse<object>>> SetCOGSMethod(int id, [FromBody] COGSMethodDto methodDto)
        {
            try
            {
                if (methodDto == null || string.IsNullOrWhiteSpace(methodDto.Method))
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Phương pháp tính COGS không được để trống"));
                }

                var method = methodDto.Method.ToUpper();
                if (method != "FIFO" && method != "WEIGHTEDAVERAGE" && method != "WEIGHTED_AVERAGE" && method != "AVERAGE")
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Phương pháp tính COGS không hợp lệ. Chỉ hỗ trợ 'FIFO' hoặc 'WeightedAverage'"));
                }

                var serviceOrder = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
                if (serviceOrder == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                // Chuẩn hóa method về FIFO hoặc WeightedAverage
                var normalizedMethod = method == "FIFO" ? "FIFO" : "WeightedAverage";
                
                // ✅ FIX: Nếu đã tính COGS trước đó với method khác, reset để tính lại
                if (serviceOrder.COGSCalculationDate != null && serviceOrder.COGSCalculationMethod != normalizedMethod)
                {
                    serviceOrder.COGSCalculationDate = null;
                    serviceOrder.COGSBreakdown = null;
                    serviceOrder.TotalCOGS = 0; // Reset TotalCOGS để đảm bảo tính lại chính xác
                }

                // Set method
                serviceOrder.COGSCalculationMethod = normalizedMethod;

                await _unitOfWork.ServiceOrders.UpdateAsync(serviceOrder);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResult(null, $"Đã thiết lập phương pháp tính COGS: {serviceOrder.COGSCalculationMethod}"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult("Lỗi khi thiết lập phương pháp tính COGS", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 3.1: Tính lợi nhuận gộp cho Service Order
        /// </summary>
        [HttpGet("{id}/gross-profit")]
        public async Task<ActionResult<ApiResponse<GrossProfitDto>>> GetGrossProfit(int id)
        {
            try
            {
                var serviceOrder = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
                if (serviceOrder == null)
                {
                    return NotFound(ApiResponse<GrossProfitDto>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                // Tính lợi nhuận gộp
                var grossProfitResult = await _cogsCalculationService.CalculateGrossProfitAsync(id);

                // Map sang DTO
                var grossProfitDto = new GrossProfitDto
                {
                    ServiceOrderId = grossProfitResult.ServiceOrderId,
                    TotalRevenue = grossProfitResult.TotalRevenue,
                    TotalCOGS = grossProfitResult.TotalCOGS,
                    GrossProfit = grossProfitResult.GrossProfit,
                    GrossProfitMargin = grossProfitResult.GrossProfitMargin
                };

                return Ok(ApiResponse<GrossProfitDto>.SuccessResult(grossProfitDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<GrossProfitDto>.ErrorResult("Lỗi khi tính lợi nhuận gộp", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 3.1: Báo cáo tổng hợp COGS cho phiếu sửa chữa
        /// </summary>
        [HttpGet("cogs-report")]
        public async Task<ActionResult<ApiResponse<ServiceOrderCogsReportDto>>> GetCogsReport(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? method)
        {
            try
            {
                var report = await BuildCogsReportAsync(startDate, endDate, method);
                return Ok(ApiResponse<ServiceOrderCogsReportDto>.SuccessResult(report, "Lấy báo cáo COGS thành công"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<ServiceOrderCogsReportDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating COGS report");
                return StatusCode(500, ApiResponse<ServiceOrderCogsReportDto>.ErrorResult("Lỗi khi tạo báo cáo COGS", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 3.1: Export báo cáo COGS ra CSV
        /// </summary>
        [HttpGet("cogs-report/export")]
        public async Task<IActionResult> ExportCogsReport(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? method)
        {
            try
            {
                var report = await BuildCogsReportAsync(startDate, endDate, method);

                var builder = new StringBuilder();
                builder.AppendLine("Số phiếu,Số tiền,Giá vốn,Lợi nhuận,Tỷ lệ (%),Ngày hoàn tất,Phương pháp,Ngày tính");

                foreach (var order in report.Orders)
                {
                    var completedDate = order.CompletedDate?.ToString("dd/MM/yyyy") ?? string.Empty;
                    var cogsDate = order.CogsCalculationDate?.ToString("dd/MM/yyyy HH:mm") ?? string.Empty;
                    var methodDisplay = string.IsNullOrWhiteSpace(order.CogsCalculationMethod) ? string.Empty : order.CogsCalculationMethod;

                    builder.AppendLine(string.Join(',', new[]
                    {
                        EscapeCsv(order.OrderNumber),
                        order.TotalRevenue.ToString("0.00"),
                        order.TotalCogs.ToString("0.00"),
                        order.GrossProfit.ToString("0.00"),
                        order.GrossMargin.ToString("0.00"),
                        EscapeCsv(completedDate),
                        EscapeCsv(methodDisplay),
                        EscapeCsv(cogsDate)
                    }));
                }

                builder.AppendLine();
                builder.AppendLine("Tổng cộng");
                builder.AppendLine($"Tổng doanh thu,{report.TotalRevenue:0.00}");
                builder.AppendLine($"Tổng giá vốn,{report.TotalCogs:0.00}");
                builder.AppendLine($"Tổng lợi nhuận,{report.TotalGrossProfit:0.00}");
                builder.AppendLine($"Tỷ lệ lợi nhuận gộp TB (%),{report.AverageGrossMargin:0.00}");
                builder.AppendLine($"Tổng số phiếu,{report.TotalOrders}");

                var bytes = Encoding.UTF8.GetBytes(builder.ToString());
                var fileName = $"cogs-report-{DateTime.Now:yyyyMMddHHmmss}.csv";
                return File(bytes, "text/csv", fileName);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<ServiceOrderCogsReportDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting COGS report");
                return StatusCode(500, ApiResponse<ServiceOrderCogsReportDto>.ErrorResult("Lỗi khi xuất báo cáo COGS", ex.Message));
            }
        }

        [HttpGet("cogs-report/export-excel")]
        public async Task<IActionResult> ExportCogsReportExcel(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? method)
        {
            try
            {
                var report = await BuildCogsReportAsync(startDate, endDate, method);

#pragma warning disable CS0618
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
#pragma warning restore CS0618
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("COGS Report");

                BuildCogsExcelWorksheet(worksheet, report);

                var fileName = $"cogs-report-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                var bytes = package.GetAsByteArray();
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<ServiceOrderCogsReportDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting COGS report excel");
                return StatusCode(500, ApiResponse<ServiceOrderCogsReportDto>.ErrorResult("Lỗi khi xuất báo cáo COGS", ex.Message));
            }
        }

        private async Task<ServiceOrderCogsReportDto> BuildCogsReportAsync(DateTime? startDate, DateTime? endDate, string? method)
        {
            var query = _context.ServiceOrders
                .AsNoTracking()
                .Where(so => !so.IsDeleted);

            if (startDate.HasValue)
            {
                query = query.Where(so => so.CompletedDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1);
                query = query.Where(so => so.CompletedDate < end);
            }

            if (!string.IsNullOrWhiteSpace(method))
            {
                var normalizedMethod = NormalizeCogsMethod(method);
                query = query.Where(so => so.COGSCalculationMethod == normalizedMethod);
            }

            var orders = await query
                .OrderByDescending(so => so.CompletedDate)
                .Select(so => new ServiceOrderCogsReportItemDto
                {
                    ServiceOrderId = so.Id,
                    OrderNumber = so.OrderNumber,
                    CustomerName = so.Customer != null ? so.Customer.Name : null,
                    CompletedDate = so.CompletedDate,
                    TotalRevenue = so.FinalAmount,
                    TotalCogs = so.TotalCOGS,
                    GrossProfit = so.FinalAmount - so.TotalCOGS,
                    GrossMargin = so.FinalAmount != 0 ? ((so.FinalAmount - so.TotalCOGS) / so.FinalAmount) * 100 : 0,
                    CogsCalculationMethod = so.COGSCalculationMethod,
                    CogsCalculationDate = so.COGSCalculationDate
                })
                .ToListAsync();

            var totalRevenue = orders.Sum(o => o.TotalRevenue);
            var totalCogs = orders.Sum(o => o.TotalCogs);
            var totalGrossProfit = totalRevenue - totalCogs;
            var averageGrossMargin = totalRevenue != 0 ? (totalGrossProfit / totalRevenue) * 100 : 0;

            return new ServiceOrderCogsReportDto
            {
                TotalRevenue = totalRevenue,
                TotalCogs = totalCogs,
                TotalGrossProfit = totalGrossProfit,
                AverageGrossMargin = averageGrossMargin,
                TotalOrders = orders.Count,
                Orders = orders
            };
        }

        private static string NormalizeCogsMethod(string? method)
        {
            if (string.IsNullOrWhiteSpace(method))
            {
                return "FIFO";
            }

            var normalized = method.Trim().ToUpperInvariant();
            return normalized switch
            {
                "FIFO" => "FIFO",
                "WEIGHTEDAVERAGE" => "WeightedAverage",
                "WEIGHTED_AVERAGE" => "WeightedAverage",
                "AVERAGE" => "WeightedAverage",
                _ => throw new ArgumentException("Phương pháp tính COGS không hợp lệ. Hỗ trợ 'FIFO' hoặc 'WeightedAverage'.")
            };
        }

        private static string EscapeCsv(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (value.Contains('"') || value.Contains(',') || value.Contains('\n'))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }

        /// <summary>
        /// ✅ 3.3: Lấy danh sách phí dịch vụ cho Service Order
        /// </summary>
        [HttpGet("{id}/fees")]
        public async Task<ActionResult<ApiResponse<ServiceOrderFeeSummaryDto>>> GetServiceOrderFees(int id)
        {
            try
            {
                var order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                if (order == null)
                {
                    return NotFound(ApiResponse<ServiceOrderFeeSummaryDto>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                var fees = order.ServiceOrderFees.Where(f => !f.IsDeleted).ToList();
                var feeTypes = (await _unitOfWork.ServiceFeeTypes.FindAsync(t => t.IsActive && !t.IsDeleted))
                    .OrderBy(t => t.DisplayOrder)
                    .ThenBy(t => t.Name)
                    .ToList();
                var summary = BuildFeeSummary(order.Id, fees, feeTypes);
                return Ok(ApiResponse<ServiceOrderFeeSummaryDto>.SuccessResult(summary));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service order fees for {ServiceOrderId}", id);
                return StatusCode(500, ApiResponse<ServiceOrderFeeSummaryDto>.ErrorResult("Lỗi khi lấy danh sách phí dịch vụ", ex.Message));
            }
        }

        /// <summary>
        /// ✅ 3.3: Cập nhật danh sách phí dịch vụ cho Service Order
        /// </summary>
        [HttpPut("{id}/fees")]
        public async Task<ActionResult<ApiResponse<ServiceOrderFeeSummaryDto>>> UpdateServiceOrderFees(int id, [FromBody] UpdateServiceOrderFeesRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(ApiResponse<ServiceOrderFeeSummaryDto>.ErrorResult("Dữ liệu không hợp lệ"));
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id);
                if (order == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return NotFound(ApiResponse<ServiceOrderFeeSummaryDto>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                var existingFees = (await _unitOfWork.ServiceOrderFees.FindAsync(f => f.ServiceOrderId == id && !f.IsDeleted))
                    .ToDictionary(f => f.Id);

                var typeIds = request.Fees.Select(f => f.ServiceFeeTypeId).Distinct().ToList();
                Dictionary<int, Core.Entities.ServiceFeeType> feeTypes = new();
                if (typeIds.Count > 0)
                {
                    feeTypes = (await _unitOfWork.ServiceFeeTypes.FindAsync(t => typeIds.Contains(t.Id) && !t.IsDeleted))
                        .ToDictionary(t => t.Id);

                    foreach (var feeDto in request.Fees)
                    {
                        if (!feeTypes.ContainsKey(feeDto.ServiceFeeTypeId))
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return BadRequest(ApiResponse<ServiceOrderFeeSummaryDto>.ErrorResult($"Loại phí (ID: {feeDto.ServiceFeeTypeId}) không hợp lệ hoặc đã bị vô hiệu"));
                        }

                        if (feeDto.Amount < 0 || feeDto.VatAmount < 0 || feeDto.DiscountAmount < 0)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return BadRequest(ApiResponse<ServiceOrderFeeSummaryDto>.ErrorResult("Số tiền không được nhỏ hơn 0"));
                        }
                    }
                }

                var handledIds = new HashSet<int>();
                foreach (var feeDto in request.Fees)
                {
                    if (feeDto.Id.HasValue && existingFees.TryGetValue(feeDto.Id.Value, out var existingFee))
                    {
                        existingFee.ServiceFeeTypeId = feeDto.ServiceFeeTypeId;
                        existingFee.Amount = feeDto.Amount;
                        existingFee.VatAmount = feeDto.VatAmount;
                        existingFee.DiscountAmount = feeDto.DiscountAmount;
                        existingFee.ReferenceSource = feeDto.ReferenceSource;
                        existingFee.Notes = feeDto.Notes;
                        existingFee.IsManual = feeDto.IsManual;
                        existingFee.UpdatedAt = DateTime.Now;

                        await _unitOfWork.ServiceOrderFees.UpdateAsync(existingFee);
                        handledIds.Add(existingFee.Id);
                    }
                    else
                    {
                        var newFee = new Core.Entities.ServiceOrderFee
                        {
                            ServiceOrderId = id,
                            ServiceFeeTypeId = feeDto.ServiceFeeTypeId,
                            Amount = feeDto.Amount,
                            VatAmount = feeDto.VatAmount,
                            DiscountAmount = feeDto.DiscountAmount,
                            ReferenceSource = feeDto.ReferenceSource,
                            Notes = feeDto.Notes,
                            IsManual = feeDto.IsManual,
                            CreatedAt = DateTime.Now
                        };

                        await _unitOfWork.ServiceOrderFees.AddAsync(newFee);
                    }
                }

                var feesToRemove = existingFees.Values.Where(f => !handledIds.Contains(f.Id)).ToList();
                foreach (var fee in feesToRemove)
                {
                    await _unitOfWork.ServiceOrderFees.DeleteAsync(fee);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(id) ?? order;
                var updatedFees = order.ServiceOrderFees.Where(f => !f.IsDeleted).ToList();
                var activeTypes = (await _unitOfWork.ServiceFeeTypes.FindAsync(t => t.IsActive && !t.IsDeleted))
                    .OrderBy(t => t.DisplayOrder)
                    .ThenBy(t => t.Name)
                    .ToList();
                var summary = BuildFeeSummary(order.Id, updatedFees, activeTypes);

                return Ok(ApiResponse<ServiceOrderFeeSummaryDto>.SuccessResult(summary, "Cập nhật phí dịch vụ thành công"));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error updating service order fees for {ServiceOrderId}", id);
                return StatusCode(500, ApiResponse<ServiceOrderFeeSummaryDto>.ErrorResult("Lỗi khi cập nhật phí dịch vụ", ex.Message));
            }
        }

        private ServiceOrderFeeSummaryDto BuildFeeSummary(int serviceOrderId, IEnumerable<Core.Entities.ServiceOrderFee> fees, IEnumerable<Core.Entities.ServiceFeeType> feeTypes)
        {
            var feeList = fees?.ToList() ?? new List<Core.Entities.ServiceOrderFee>();
            var dto = new ServiceOrderFeeSummaryDto
            {
                ServiceOrderId = serviceOrderId,
                TotalAmount = feeList.Sum(f => f.Amount),
                TotalVat = feeList.Sum(f => f.VatAmount),
                TotalDiscount = feeList.Sum(f => f.DiscountAmount),
                Fees = _mapper.Map<List<ServiceOrderFeeDto>>(feeList),
                FeeTypes = _mapper.Map<List<ServiceFeeTypeDto>>(feeTypes?.ToList() ?? new List<Core.Entities.ServiceFeeType>())
            };

            return dto;
        }

        private void BuildCogsExcelWorksheet(ExcelWorksheet worksheet, ServiceOrderCogsReportDto report)
        {
            var currentRow = 1;
            var titleRange = worksheet.Cells[currentRow, 1, currentRow, 8];
            titleRange.Merge = true;
            titleRange.Value = "BÁO CÁO GIÁ VỐN (COGS)";
            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.Size = 16;
            titleRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            currentRow += 2;

            worksheet.Cells[currentRow, 1].Value = "Tổng doanh thu";
            worksheet.Cells[currentRow, 2].Value = report.TotalRevenue;
            worksheet.Cells[currentRow, 2].Style.Numberformat.Format = "#,##0.00";
            currentRow++;

            worksheet.Cells[currentRow, 1].Value = "Tổng giá vốn";
            worksheet.Cells[currentRow, 2].Value = report.TotalCogs;
            worksheet.Cells[currentRow, 2].Style.Numberformat.Format = "#,##0.00";
            currentRow++;

            worksheet.Cells[currentRow, 1].Value = "Tổng lợi nhuận";
            worksheet.Cells[currentRow, 2].Value = report.TotalGrossProfit;
            worksheet.Cells[currentRow, 2].Style.Numberformat.Format = "#,##0.00";
            currentRow++;

            worksheet.Cells[currentRow, 1].Value = "Tỷ lệ lợi nhuận gộp TB";
            worksheet.Cells[currentRow, 2].Value = report.AverageGrossMargin / 100;
            worksheet.Cells[currentRow, 2].Style.Numberformat.Format = "0.00%";
            currentRow++;

            worksheet.Cells[currentRow, 1].Value = "Tổng số phiếu";
            worksheet.Cells[currentRow, 2].Value = report.TotalOrders;
            worksheet.Cells[currentRow, 2].Style.Numberformat.Format = "0";
            currentRow += 2;

            var headerRow = currentRow;
            var headers = new[] { "Số phiếu", "Khách hàng", "Ngày hoàn tất", "Doanh thu", "Giá vốn", "Lợi nhuận", "Tỷ lệ", "Phương pháp", "Ngày tính COGS" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[headerRow, i + 1].Value = headers[i];
                worksheet.Cells[headerRow, i + 1].Style.Font.Bold = true;
                worksheet.Cells[headerRow, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[headerRow, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));
                worksheet.Cells[headerRow, i + 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
                worksheet.Cells[headerRow, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            currentRow++;
            foreach (var order in report.Orders)
            {
                worksheet.Cells[currentRow, 1].Value = order.OrderNumber;
                worksheet.Cells[currentRow, 2].Value = order.CustomerName;
                worksheet.Cells[currentRow, 3].Value = order.CompletedDate;
                worksheet.Cells[currentRow, 3].Style.Numberformat.Format = "dd/MM/yyyy";
                worksheet.Cells[currentRow, 4].Value = order.TotalRevenue;
                worksheet.Cells[currentRow, 4].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[currentRow, 5].Value = order.TotalCogs;
                worksheet.Cells[currentRow, 5].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[currentRow, 6].Value = order.GrossProfit;
                worksheet.Cells[currentRow, 6].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[currentRow, 7].Value = order.GrossMargin / 100;
                worksheet.Cells[currentRow, 7].Style.Numberformat.Format = "0.00%";
                worksheet.Cells[currentRow, 8].Value = order.CogsCalculationMethod;
                worksheet.Cells[currentRow, 9].Value = order.CogsCalculationDate;
                worksheet.Cells[currentRow, 9].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";
                currentRow++;
            }

            worksheet.Cells[headerRow, 1, currentRow - 1, 9].AutoFitColumns();
            worksheet.Cells[headerRow, 4, currentRow - 1, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            worksheet.Cells[headerRow, 7, currentRow - 1, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            worksheet.Cells[headerRow, 1, currentRow - 1, 9].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[headerRow, 1, currentRow - 1, 9].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[headerRow, 1, currentRow - 1, 9].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[headerRow, 1, currentRow - 1, 9].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[headerRow, 1, currentRow - 1, 9].Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }

        #endregion
    }
}

