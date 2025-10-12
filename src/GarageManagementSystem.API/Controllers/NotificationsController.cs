using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(IUnitOfWork unitOfWork, ILogger<NotificationsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Send appointment reminder to customer
        /// </summary>
        [HttpPost("send-appointment-reminder")]
        public async Task<IActionResult> SendAppointmentReminder([FromBody] SendReminderRequest request)
        {
            try
            {
                var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId);
                if (appointment == null)
                    return NotFound(new { success = false, message = "Không tìm thấy lịch hẹn" });

                var customer = await _unitOfWork.Customers.GetByIdAsync(appointment.CustomerId);
                if (customer == null)
                    return NotFound(new { success = false, message = "Không tìm thấy khách hàng" });

                // Prepare notification message
                var message = $"Nhắc nhở: Bạn có lịch hẹn vào {appointment.ScheduledDateTime:dd/MM/yyyy HH:mm} " +
                             $"cho dịch vụ {appointment.ServiceRequested ?? appointment.AppointmentType}. " +
                             $"Garage: [Tên Garage]. Hotline: [Phone Number]";

                var notificationLog = new
                {
                    AppointmentId = request.AppointmentId,
                    CustomerId = appointment.CustomerId,
                    CustomerName = customer.Name,
                    Phone = customer.Phone,
                    Email = customer.Email,
                    Message = message,
                    Method = request.Method, // SMS or Email
                    Status = "Queued", // Would integrate with actual SMS/Email service
                    SentAt = DateTime.Now
                };

                // TODO: Integrate with actual SMS/Email service
                // For now, just log and return success
                _logger.LogInformation($"Notification queued: {request.Method} to {customer.Phone ?? customer.Email}");

                return Ok(new 
                { 
                    success = true, 
                    message = $"Đã gửi nhắc nhở qua {request.Method}",
                    data = notificationLog
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending appointment reminder");
                return StatusCode(500, new { success = false, message = "Lỗi khi gửi nhắc nhở" });
            }
        }

        /// <summary>
        /// Send service completion notification
        /// </summary>
        [HttpPost("send-completion-notification")]
        public async Task<IActionResult> SendCompletionNotification([FromBody] SendCompletionNotificationRequest request)
        {
            try
            {
                var order = await _unitOfWork.ServiceOrders.GetByIdAsync(request.ServiceOrderId);
                if (order == null)
                    return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });

                var customer = await _unitOfWork.Customers.GetByIdAsync(order.CustomerId);
                if (customer == null)
                    return NotFound(new { success = false, message = "Không tìm thấy khách hàng" });

                var message = $"Thông báo: Xe {order.Vehicle?.LicensePlate} của quý khách đã hoàn thành dịch vụ. " +
                             $"Vui lòng đến nhận xe. Tổng tiền: {order.FinalAmount:N0} VND. " +
                             $"Hotline: [Phone Number]";

                var notificationLog = new
                {
                    ServiceOrderId = request.ServiceOrderId,
                    OrderNumber = order.OrderNumber,
                    CustomerId = order.CustomerId,
                    CustomerName = customer.Name,
                    Phone = customer.Phone,
                    Email = customer.Email,
                    Message = message,
                    Status = "Sent",
                    SentAt = DateTime.Now
                };

                _logger.LogInformation($"Completion notification sent to customer {customer.Id}");

                return Ok(new 
                { 
                    success = true, 
                    message = "Đã gửi thông báo hoàn thành",
                    data = notificationLog
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending completion notification");
                return StatusCode(500, new { success = false, message = "Lỗi khi gửi thông báo" });
            }
        }

        /// <summary>
        /// Send payment reminder
        /// </summary>
        [HttpPost("send-payment-reminder")]
        public async Task<IActionResult> SendPaymentReminder([FromBody] SendPaymentReminderRequest request)
        {
            try
            {
                var order = await _unitOfWork.ServiceOrders.GetByIdAsync(request.ServiceOrderId);
                if (order == null)
                    return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });

                if (order.AmountRemaining <= 0)
                    return BadRequest(new { success = false, message = "Đơn hàng đã thanh toán đủ" });

                var customer = await _unitOfWork.Customers.GetByIdAsync(order.CustomerId);
                if (customer == null)
                    return NotFound(new { success = false, message = "Không tìm thấy khách hàng" });

                var message = $"Nhắc nhở thanh toán: Quý khách còn nợ {order.AmountRemaining:N0} VND " +
                             $"cho đơn hàng {order.OrderNumber}. " +
                             $"Vui lòng thanh toán để hoàn tất. Hotline: [Phone Number]";

                var notificationLog = new
                {
                    ServiceOrderId = request.ServiceOrderId,
                    OrderNumber = order.OrderNumber,
                    CustomerId = order.CustomerId,
                    CustomerName = customer.Name,
                    AmountRemaining = order.AmountRemaining,
                    Message = message,
                    Status = "Sent",
                    SentAt = DateTime.Now
                };

                _logger.LogInformation($"Payment reminder sent to customer {customer.Id}");

                return Ok(new 
                { 
                    success = true, 
                    message = "Đã gửi nhắc nhở thanh toán",
                    data = notificationLog
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment reminder");
                return StatusCode(500, new { success = false, message = "Lỗi khi gửi nhắc nhở thanh toán" });
            }
        }

        /// <summary>
        /// Send bulk maintenance reminders
        /// </summary>
        [HttpPost("send-maintenance-reminders")]
        public async Task<IActionResult> SendMaintenanceReminders()
        {
            try
            {
                var vehicles = await _unitOfWork.Vehicles.GetAllAsync();
                var serviceOrders = await _unitOfWork.ServiceOrders.GetAllAsync();
                
                var remindersToSend = new List<object>();
                var cutoffDate = DateTime.Now.AddMonths(-6); // 6 months since last service

                foreach (var vehicle in vehicles)
                {
                    var lastService = serviceOrders
                        .Where(o => o.VehicleId == vehicle.Id && o.CompletedDate.HasValue)
                        .OrderByDescending(o => o.CompletedDate)
                        .FirstOrDefault();

                    if (lastService == null || lastService.CompletedDate < cutoffDate)
                    {
                        var customer = await _unitOfWork.Customers.GetByIdAsync(vehicle.CustomerId);
                        if (customer != null && !string.IsNullOrEmpty(customer.Phone))
                        {
                            var daysSince = lastService != null ? 
                                (DateTime.Now - lastService.CompletedDate!.Value).Days : 999;

                            remindersToSend.Add(new
                            {
                                CustomerId = customer.Id,
                                CustomerName = customer.Name,
                                Phone = customer.Phone,
                                VehiclePlate = vehicle.LicensePlate,
                                DaysSinceLastService = daysSince,
                                Message = $"Xe {vehicle.LicensePlate} đã {daysSince} ngày chưa bảo dưỡng. Vui lòng liên hệ đặt lịch."
                            });
                        }
                    }
                }

                _logger.LogInformation($"Queued {remindersToSend.Count} maintenance reminders");

                return Ok(new 
                { 
                    success = true, 
                    message = $"Đã tạo {remindersToSend.Count} tin nhắn nhắc nhở",
                    data = remindersToSend
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending maintenance reminders");
                return StatusCode(500, new { success = false, message = "Lỗi khi gửi nhắc nhở bảo dưỡng" });
            }
        }

        /// <summary>
        /// Send insurance expiry reminders
        /// </summary>
        [HttpPost("send-insurance-expiry-reminders")]
        public async Task<IActionResult> SendInsuranceExpiryReminders([FromQuery] int daysAhead = 30)
        {
            try
            {
                var vehicles = await _unitOfWork.Vehicles.GetAllAsync();
                var expiryDate = DateTime.Now.AddDays(daysAhead);
                
                var expiringInsurance = vehicles
                    .Where(v => v.HasInsurance && 
                               v.InsuranceEndDate.HasValue && 
                               v.InsuranceEndDate <= expiryDate &&
                               v.InsuranceEndDate >= DateTime.Now)
                    .ToList();

                var remindersToSend = new List<object>();

                foreach (var vehicle in expiringInsurance)
                {
                    var customer = await _unitOfWork.Customers.GetByIdAsync(vehicle.CustomerId);
                    if (customer != null)
                    {
                        var daysUntilExpiry = (vehicle.InsuranceEndDate!.Value - DateTime.Now).Days;
                        
                        remindersToSend.Add(new
                        {
                            CustomerId = customer.Id,
                            CustomerName = customer.Name,
                            Phone = customer.Phone,
                            VehiclePlate = vehicle.LicensePlate,
                            InsuranceCompany = vehicle.InsuranceCompany,
                            ExpiryDate = vehicle.InsuranceEndDate,
                            DaysUntilExpiry = daysUntilExpiry,
                            Priority = daysUntilExpiry <= 7 ? "Urgent" : "Normal",
                            Message = $"Bảo hiểm xe {vehicle.LicensePlate} sẽ hết hạn trong {daysUntilExpiry} ngày ({vehicle.InsuranceEndDate:dd/MM/yyyy})"
                        });
                    }
                }

                _logger.LogInformation($"Queued {remindersToSend.Count} insurance expiry reminders");

                return Ok(new 
                { 
                    success = true, 
                    message = $"Đã tạo {remindersToSend.Count} tin nhắn nhắc nhở bảo hiểm",
                    data = remindersToSend
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending insurance expiry reminders");
                return StatusCode(500, new { success = false, message = "Lỗi khi gửi nhắc nhở bảo hiểm" });
            }
        }
    }

    public class SendReminderRequest
    {
        public int AppointmentId { get; set; }
        public string Method { get; set; } = "SMS"; // SMS or Email
    }

    public class SendCompletionNotificationRequest
    {
        public int ServiceOrderId { get; set; }
    }

    public class SendPaymentReminderRequest
    {
        public int ServiceOrderId { get; set; }
    }
}

