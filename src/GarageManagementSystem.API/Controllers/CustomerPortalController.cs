using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerPortalController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CustomerPortalController> _logger;

        public CustomerPortalController(IUnitOfWork unitOfWork, ILogger<CustomerPortalController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Get customer's vehicles
        /// </summary>
        [HttpGet("my-vehicles")]
        public async Task<IActionResult> GetMyVehicles([FromQuery] int customerId)
        {
            try
            {
                var vehicles = await _unitOfWork.Vehicles.GetAllAsync();
                var myVehicles = vehicles.Where(v => v.CustomerId == customerId).Select(v => new
                {
                    v.Id,
                    v.LicensePlate,
                    v.Brand,
                    v.Model,
                    v.Year,
                    v.Color,
                    v.VIN,
                    InsuranceStatus = v.HasInsurance ? (v.IsInsuranceActive ? "Active" : "Expired") : "None",
                    v.InsuranceCompany,
                    v.InsuranceEndDate
                }).ToList();

                return Ok(new { success = true, data = myVehicles });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer vehicles");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy danh sách xe" });
            }
        }

        /// <summary>
        /// Get customer's service history
        /// </summary>
        [HttpGet("service-history")]
        public async Task<IActionResult> GetServiceHistory(
            [FromQuery] int customerId,
            [FromQuery] int? vehicleId = null)
        {
            try
            {
                var orders = await _unitOfWork.ServiceOrders.GetAllAsync();
                var myOrders = orders.Where(o => o.CustomerId == customerId);

                if (vehicleId.HasValue)
                    myOrders = myOrders.Where(o => o.VehicleId == vehicleId.Value);

                var history = myOrders.Select(o => new
                {
                    o.Id,
                    o.OrderNumber,
                    o.OrderDate,
                    o.CompletedDate,
                    VehiclePlate = o.Vehicle?.LicensePlate,
                    o.Status,
                    Services = o.ServiceOrderItems.Select(i => i.ServiceName).ToList(),
                    TotalAmount = o.FinalAmount,
                    o.PaymentStatus,
                    AmountPaid = o.AmountPaid,
                    AmountRemaining = o.AmountRemaining
                }).OrderByDescending(o => o.OrderDate).ToList();

                return Ok(new { success = true, data = history });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service history");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy lịch sử dịch vụ" });
            }
        }

        /// <summary>
        /// Get customer's upcoming appointments
        /// </summary>
        [HttpGet("appointments")]
        public async Task<IActionResult> GetMyAppointments([FromQuery] int customerId)
        {
            try
            {
                var appointments = await _unitOfWork.Appointments.GetAllAsync();
                var myAppointments = appointments
                    .Where(a => a.CustomerId == customerId && a.ScheduledDateTime >= DateTime.Now)
                    .Select(a => new
                    {
                        a.Id,
                        AppointmentDate = a.ScheduledDateTime,
                        VehiclePlate = a.Vehicle?.LicensePlate,
                        ServiceType = a.AppointmentType,
                        Description = a.ServiceRequested,
                        a.Status,
                        Notes = a.CustomerNotes
                    })
                    .OrderBy(a => a.AppointmentDate)
                    .ToList();

                return Ok(new { success = true, data = myAppointments });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting appointments");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy lịch hẹn" });
            }
        }

        /// <summary>
        /// Book new appointment
        /// </summary>
        [HttpPost("book-appointment")]
        public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentRequest request)
        {
            try
            {
                // Validate customer and vehicle
                var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
                if (customer == null)
                    return BadRequest(new { success = false, message = "Khách hàng không tồn tại" });

                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.VehicleId);
                if (vehicle == null || vehicle.CustomerId != request.CustomerId)
                    return BadRequest(new { success = false, message = "Xe không hợp lệ" });

                var appointment = new Appointment
                {
                    CustomerId = request.CustomerId,
                    VehicleId = request.VehicleId,
                    ScheduledDateTime = request.AppointmentDate,
                    AppointmentType = request.ServiceType,
                    ServiceRequested = request.Description,
                    Status = "Scheduled"
                };

                await _unitOfWork.Appointments.AddAsync(appointment);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = appointment, message = "Đặt lịch hẹn thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error booking appointment");
                return StatusCode(500, new { success = false, message = "Lỗi khi đặt lịch hẹn" });
            }
        }

        /// <summary>
        /// Get customer's invoices
        /// </summary>
        [HttpGet("invoices")]
        public async Task<IActionResult> GetMyInvoices(
            [FromQuery] int customerId,
            [FromQuery] string? status = null)
        {
            try
            {
                var invoices = await _unitOfWork.Invoices.GetAllAsync();
                var myInvoices = invoices.Where(i => i.CustomerId == customerId);

                if (!string.IsNullOrEmpty(status))
                    myInvoices = myInvoices.Where(i => i.Status == status);

                var result = myInvoices.Select(i => new
                {
                    i.Id,
                    i.InvoiceNumber,
                    i.InvoiceDate,
                    i.VehiclePlate,
                    i.SubTotal,
                    i.VATAmount,
                    i.TotalAmount,
                    i.Status,
                    i.PaymentMethod,
                    CanDownload = i.Status == "Issued"
                }).OrderByDescending(i => i.InvoiceDate).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoices");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy hóa đơn" });
            }
        }

        /// <summary>
        /// Track service order status
        /// </summary>
        [HttpGet("track-order/{orderNumber}")]
        public async Task<IActionResult> TrackOrder(string orderNumber)
        {
            try
            {
                var orders = await _unitOfWork.ServiceOrders.GetAllAsync();
                var order = orders.FirstOrDefault(o => o.OrderNumber == orderNumber);

                if (order == null)
                    return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });

                var trackingInfo = new
                {
                    order.OrderNumber,
                    order.OrderDate,
                    order.Status,
                    VehiclePlate = order.Vehicle?.LicensePlate,
                    EstimatedCompletion = order.ScheduledDate,
                    ActualCompletion = order.CompletedDate,
                    TotalAmount = order.FinalAmount,
                    PaymentStatus = order.PaymentStatus,
                    AmountPaid = order.AmountPaid,
                    AmountRemaining = order.AmountRemaining,
                    
                    Timeline = new[]
                    {
                        new { Step = "Tiếp nhận", Date = (DateTime?)order.OrderDate, Status = "Completed" },
                        new { Step = "Đang sửa chữa", Date = order.StartDate, Status = order.Status == "InProgress" || order.Status == "Completed" ? "Completed" : "Pending" },
                        new { Step = "Hoàn thành", Date = order.CompletedDate, Status = order.Status == "Completed" ? "Completed" : "Pending" },
                        new { Step = "Thanh toán", Date = order.CompletedDate, Status = order.PaymentStatus == "Paid" ? "Completed" : "Pending" }
                    },
                    
                    Services = order.ServiceOrderItems.Select(i => new
                    {
                        i.ServiceName,
                        i.Quantity,
                        i.UnitPrice,
                        i.TotalPrice
                    }).ToList(),
                    
                    Parts = order.ServiceOrderParts.Select(p => new
                    {
                        p.PartName,
                        p.Quantity,
                        p.UnitPrice,
                        p.TotalPrice
                    }).ToList()
                };

                return Ok(new { success = true, data = trackingInfo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking order");
                return StatusCode(500, new { success = false, message = "Lỗi khi theo dõi đơn hàng" });
            }
        }

        /// <summary>
        /// Get maintenance reminders for customer's vehicles
        /// </summary>
        [HttpGet("maintenance-reminders")]
        public async Task<IActionResult> GetMaintenanceReminders([FromQuery] int customerId)
        {
            try
            {
                var vehicles = await _unitOfWork.Vehicles.GetAllAsync();
                var myVehicles = vehicles.Where(v => v.CustomerId == customerId).ToList();

                var reminders = new List<object>();

                foreach (var vehicle in myVehicles)
                {
                    var serviceOrders = await _unitOfWork.ServiceOrders.GetAllAsync();
                    var lastService = serviceOrders
                        .Where(o => o.VehicleId == vehicle.Id && o.CompletedDate.HasValue)
                        .OrderByDescending(o => o.CompletedDate)
                        .FirstOrDefault();

                    if (lastService != null)
                    {
                        var daysSinceService = (DateTime.Now - lastService.CompletedDate!.Value).Days;
                        
                        // Simple reminder logic (would be more sophisticated in production)
                        if (daysSinceService >= 180) // 6 months
                        {
                            reminders.Add(new
                            {
                                VehicleId = vehicle.Id,
                                VehiclePlate = vehicle.LicensePlate,
                                ReminderType = "Regular Maintenance",
                                Message = "Đã hơn 6 tháng kể từ lần bảo dưỡng cuối",
                                LastServiceDate = lastService.CompletedDate,
                                DaysSinceService = daysSinceService,
                                Priority = daysSinceService >= 365 ? "High" : "Medium"
                            });
                        }
                    }
                    else
                    {
                        reminders.Add(new
                        {
                            VehicleId = vehicle.Id,
                            VehiclePlate = vehicle.LicensePlate,
                            ReminderType = "First Service",
                            Message = "Xe chưa từng được bảo dưỡng tại garage",
                            Priority = "Medium"
                        });
                    }

                    // Insurance expiry reminder
                    if (vehicle.HasInsurance && vehicle.InsuranceEndDate.HasValue)
                    {
                        var daysUntilExpiry = (vehicle.InsuranceEndDate.Value - DateTime.Now).Days;
                        if (daysUntilExpiry <= 30 && daysUntilExpiry >= 0)
                        {
                            reminders.Add(new
                            {
                                VehicleId = vehicle.Id,
                                VehiclePlate = vehicle.LicensePlate,
                                ReminderType = "Insurance Expiry",
                                Message = $"Bảo hiểm sẽ hết hạn trong {daysUntilExpiry} ngày",
                                ExpiryDate = vehicle.InsuranceEndDate,
                                Priority = daysUntilExpiry <= 7 ? "High" : "Medium"
                            });
                        }
                    }
                }

                return Ok(new { success = true, data = reminders, count = reminders.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting maintenance reminders");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy nhắc nhở bảo dưỡng" });
            }
        }
    }

    public class BookAppointmentRequest
    {
        public int CustomerId { get; set; }
        public int VehicleId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}

