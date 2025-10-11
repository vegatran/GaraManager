using GarageManagementSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller quản lý lịch hẹn with full CRUD operations via API
    /// </summary>
    [Authorize]
    [Route("AppointmentManagement")]
    public class AppointmentManagementController : Controller
    {
        private readonly ApiService _apiService;

        public AppointmentManagementController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Hiển thị trang quản lý lịch hẹn
        /// </summary>
        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Get today's appointments for DataTable via API
        /// </summary>
        [HttpGet]
        [Route("GetTodayAppointments")]
        public async Task<IActionResult> GetTodayAppointments()
        {
            var response = await _apiService.GetAsync<List<AppointmentDto>>(ApiEndpoints.Appointments.GetToday);
            
            if (response.Success)
            {
                var appointmentList = new List<object>();
                
                if (response.Data != null)
                {
                    appointmentList = response.Data.Select(a => new
                    {
                        id = a.Id,
                        appointmentNumber = a.AppointmentNumber,
                        customerName = a.Customer?.Name ?? "N/A",
                        vehiclePlate = a.Vehicle?.LicensePlate ?? "N/A",
                        scheduledDateTime = a.ScheduledDateTime.ToString("dd/MM/yyyy HH:mm"),
                        appointmentType = a.AppointmentType,
                        assignedTo = a.AssignedTo?.Name ?? "Chưa phân công",
                        status = a.Status
                    }).Cast<object>().ToList();
                }

                return Json(new { data = appointmentList });
            }
            else
            {
                return Json(new { error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Get upcoming appointments for DataTable via API
        /// </summary>
        [HttpGet]
        [Route("GetUpcomingAppointments")]
        public async Task<IActionResult> GetUpcomingAppointments()
        {
            var response = await _apiService.GetAsync<List<AppointmentDto>>(ApiEndpoints.Appointments.GetUpcoming);
            
            if (response.Success)
            {
                var appointmentList = new List<object>();
                
                if (response.Data != null)
                {
                    appointmentList = response.Data.Select(a => new
                    {
                        id = a.Id,
                        appointmentNumber = a.AppointmentNumber,
                        customerName = a.Customer?.Name ?? "N/A",
                        vehiclePlate = a.Vehicle?.LicensePlate ?? "N/A",
                        scheduledDateTime = a.ScheduledDateTime.ToString("dd/MM/yyyy HH:mm"),
                        appointmentType = a.AppointmentType,
                        assignedTo = a.AssignedTo?.Name ?? "Chưa phân công",
                        status = a.Status
                    }).Cast<object>().ToList();
                }

                return Json(new { data = appointmentList });
            }
            else
            {
                return Json(new { error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết lịch hẹn theo ID thông qua API
        /// </summary>
        [HttpGet]
        [Route("GetAppointment/{id}")]
        public async Task<IActionResult> GetAppointment(int id)
        {
            var response = await _apiService.GetAsync<AppointmentDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.Appointments.GetById, id)
            );
            
            return Json(response);
        }

        /// <summary>
        /// Tạo lịch hẹn mới thông qua API
        /// </summary>
        [HttpPost]
        [Route("CreateAppointment")]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto appointmentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errorMessage = "Dữ liệu không hợp lệ" });
            }

            var response = await _apiService.PostAsync<AppointmentDto>(
                ApiEndpoints.Appointments.Create,
                appointmentDto
            );

            return Json(response);
        }

        /// <summary>
        /// Cập nhật thông tin lịch hẹn thông qua API
        /// </summary>
        [HttpPut]
        [Route("UpdateAppointment/{id}")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] UpdateAppointmentDto appointmentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errorMessage = "Dữ liệu không hợp lệ" });
            }

            if (id != appointmentDto.Id)
            {
                return BadRequest(new { success = false, errorMessage = "ID không khớp" });
            }

            var response = await _apiService.PutAsync<AppointmentDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.Appointments.Update, id),
                appointmentDto
            );

            return Json(response);
        }

        /// <summary>
        /// Xóa lịch hẹn thông qua API
        /// </summary>
        [HttpDelete]
        [Route("DeleteAppointment/{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var response = await _apiService.DeleteAsync<AppointmentDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.Appointments.Delete, id)
            );

            return Json(response);
        }

        /// <summary>
        /// Get customers for dropdown via API
        /// </summary>
        [HttpGet]
        [Route("GetCustomers")]
        public async Task<IActionResult> GetCustomers()
        {
            var response = await _apiService.GetAsync<List<CustomerDto>>(ApiEndpoints.Customers.GetAll);
            
            if (response.Success && response.Data != null)
            {
                var customerList = response.Data.Select(c => new
                {
                    id = c.Id,
                    text = c.Name + (c.Phone != null ? " - " + c.Phone : "")
                }).Cast<object>().ToList();

                return Json(customerList);
            }

            return Json(new List<object>());
        }

        /// <summary>
        /// Get vehicles by customer for dropdown via API
        /// </summary>
        [HttpGet]
        [Route("GetVehiclesByCustomer/{customerId}")]
        public async Task<IActionResult> GetVehiclesByCustomer(int customerId)
        {
            var response = await _apiService.GetAsync<List<VehicleDto>>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.Vehicles.GetByCustomerId, customerId)
            );
            
            if (response.Success && response.Data != null)
            {
                var vehicleList = response.Data.Select(v => new
                {
                    id = v.Id,
                    text = v.LicensePlate + " - " + v.Brand + " " + v.Model
                }).Cast<object>().ToList();

                return Json(vehicleList);
            }

            return Json(new List<object>());
        }

        /// <summary>
        /// Get appointment types for dropdown via API
        /// </summary>
        [HttpGet]
        [Route("GetAppointmentTypes")]
        public async Task<IActionResult> GetAppointmentTypes()
        {
            var response = await _apiService.GetAsync<List<object>>(ApiEndpoints.Appointments.GetTypes);
            
            if (response.Success && response.Data != null)
            {
                return Json(response.Data);
            }

            return Json(new List<object>());
        }
    }
}

