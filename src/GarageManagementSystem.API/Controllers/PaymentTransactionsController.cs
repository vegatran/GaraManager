using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Core.Extensions;
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
    public class PaymentTransactionsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;

        public PaymentTransactionsController(IUnitOfWork unitOfWork, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<PaymentTransactionDto>>> GetPaymentTransactions(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? paymentMethod = null,
            [FromQuery] string? status = null)
        {
            try
            {
                var payments = await _unitOfWork.PaymentTransactions.GetAllAsync();
                var query = payments.AsQueryable();
                
                // Apply search filter if provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(p => 
                        p.TransactionReference.Contains(searchTerm) || 
                        p.Notes.Contains(searchTerm));
                }
                
                // Apply payment method filter if provided
                if (!string.IsNullOrEmpty(paymentMethod))
                {
                    query = query.Where(p => p.PaymentMethod == paymentMethod);
                }
                
                // Apply status filter if provided (removed - PaymentTransaction doesn't have Status property)

                query = query.OrderByDescending(p => p.CreatedAt);

                // Get total count
                var totalCount = await query.GetTotalCountAsync();
                
                // Apply pagination
                var pagedPayments = query.ApplyPagination(pageNumber, pageSize).ToList();
                var paymentDtos = pagedPayments.Select(MapToDto).ToList();
                
                return Ok(PagedResponse<PaymentTransactionDto>.CreateSuccessResult(
                    paymentDtos, pageNumber, pageSize, totalCount, "Payment transactions retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, PagedResponse<PaymentTransactionDto>.CreateErrorResult("Lỗi khi lấy danh sách giao dịch thanh toán"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PaymentTransactionDto>>> GetPaymentTransaction(int id)
        {
            try
            {
                var payment = await _unitOfWork.PaymentTransactions.GetByIdAsync(id);
                if (payment == null)
                {
                    return NotFound(ApiResponse<PaymentTransactionDto>.ErrorResult("Giao dịch thanh toán không tồn tại"));
                }
                return Ok(ApiResponse<PaymentTransactionDto>.SuccessResult(MapToDto(payment)));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaymentTransactionDto>.ErrorResult("Lỗi khi lấy giao dịch thanh toán", ex.Message));
            }
        }

        [HttpGet("serviceorder/{serviceOrderId}")]
        public async Task<ActionResult<ApiResponse<List<PaymentTransactionDto>>>> GetByServiceOrder(int serviceOrderId)
        {
            try
            {
                var payments = await _unitOfWork.PaymentTransactions.GetByServiceOrderIdAsync(serviceOrderId);
                return Ok(ApiResponse<List<PaymentTransactionDto>>.SuccessResult(payments.Select(MapToDto).ToList()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<PaymentTransactionDto>>.ErrorResult("Lỗi khi lấy giao dịch thanh toán theo đơn hàng", ex.Message));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<PaymentTransactionDto>>> CreatePayment(CreatePaymentTransactionDto dto)
        {
            try
            {
                // Validate service order exists
                var order = await _unitOfWork.ServiceOrders.GetByIdAsync(dto.ServiceOrderId);
                if (order == null) return BadRequest(ApiResponse<PaymentTransactionDto>.ErrorResult("Service order not found"));

                var payment = new Core.Entities.PaymentTransaction
                {
                    ReceiptNumber = await _unitOfWork.PaymentTransactions.GenerateReceiptNumberAsync(),
                    ServiceOrderId = dto.ServiceOrderId,
                    PaymentDate = DateTime.Now,
                    Amount = dto.Amount,
                    PaymentMethod = dto.PaymentMethod,
                    TransactionReference = dto.TransactionReference,
                    CardType = dto.CardType,
                    CardLastFourDigits = dto.CardLastFourDigits,
                    ReceivedById = dto.ReceivedById,
                    Notes = dto.Notes,
                    IsRefund = dto.IsRefund,
                    RefundReason = dto.RefundReason
                };

                // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.PaymentTransactions.AddAsync(payment);
                    
                    // Update ServiceOrder payment info
                    order.AmountPaid = await _unitOfWork.PaymentTransactions.GetTotalPaidForOrderAsync(dto.ServiceOrderId) + dto.Amount;
                    order.AmountRemaining = order.FinalAmount - order.AmountPaid;
                    order.PaymentStatus = order.AmountRemaining <= 0 ? "Paid" : 
                                         order.AmountPaid > 0 ? "Partial" : "Pending";
                    
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

                return Ok(ApiResponse<PaymentTransactionDto>.SuccessResult(MapToDto(payment), "Payment recorded"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaymentTransactionDto>.ErrorResult("Lỗi khi tạo giao dịch thanh toán", ex.Message));
            }
        }

        private static PaymentTransactionDto MapToDto(Core.Entities.PaymentTransaction pt) => new()
        {
            Id = pt.Id, ReceiptNumber = pt.ReceiptNumber, ServiceOrderId = pt.ServiceOrderId,
            PaymentDate = pt.PaymentDate, Amount = pt.Amount, PaymentMethod = pt.PaymentMethod,
            TransactionReference = pt.TransactionReference, CardType = pt.CardType,
            CardLastFourDigits = pt.CardLastFourDigits, ReceivedById = pt.ReceivedById,
            Notes = pt.Notes, IsRefund = pt.IsRefund, RefundReason = pt.RefundReason,
            ReceivedBy = pt.ReceivedBy != null ? new EmployeeDto { Id = pt.ReceivedBy.Id, Name = pt.ReceivedBy.Name } : null,
            CreatedAt = pt.CreatedAt, CreatedBy = pt.CreatedBy,
            UpdatedAt = pt.UpdatedAt, UpdatedBy = pt.UpdatedBy
        };
    }
}

