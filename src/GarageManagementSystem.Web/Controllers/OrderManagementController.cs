using GarageManagementSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller for managing service orders with full CRUD operations via API
    /// </summary>
    [Authorize]
    [Route("OrderManagement")]
    public class OrderManagementController : Controller
    {
        private readonly ApiService _apiService;

        public OrderManagementController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Hiển thị trang quản lý đơn hàng
        /// </summary>
        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Get all service orders for DataTable via API
        /// </summary>
        [HttpGet]
        [Route("GetOrders")]
        public async Task<IActionResult> GetOrders()
        {
            var response = await _apiService.GetAsync<List<ServiceOrderDto>>(ApiEndpoints.ServiceOrders.GetAll);
            
            if (response.Success)
            {
                var orderList = new List<object>();
                
                if (response.Data != null)
                {
                    orderList = response.Data.Select(o => new
                    {
                        id = o.Id,
                        orderNumber = o.OrderNumber,
                        customerName = o.Customer?.Name ?? "N/A",
                        vehiclePlate = o.Vehicle?.LicensePlate ?? "N/A",
                        orderDate = o.OrderDate.ToString("yyyy-MM-dd HH:mm"),
                        scheduledDate = o.ScheduledDate?.ToString("yyyy-MM-dd HH:mm") ?? "N/A",
                        completedDate = o.CompletedDate?.ToString("yyyy-MM-dd HH:mm") ?? "N/A",
                        status = o.Status,
                        finalAmount = o.FinalAmount.ToString("N0"),
                        paymentStatus = o.PaymentStatus ?? "Pending",
                        serviceCount = o.ServiceOrderItems.Count
                    }).Cast<object>().ToList();
                }

                return Json(new { data = orderList });
            }
            else
            {
                return Json(new { error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết đơn hàng theo ID thông qua API
        /// </summary>
        [HttpGet]
        [Route("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var response = await _apiService.GetAsync<ServiceOrderDto>(ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceOrders.GetById, id));
            
            if (response.Success)
            {
                return Json(new { success = true, data = response.Data });
            }
            else
            {
                return Json(new { success = false, error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Get all customers for dropdown
        /// </summary>
        [HttpGet]
        [Route("GetCustomers")]
        public async Task<IActionResult> GetCustomers()
        {
            var response = await _apiService.GetAsync<List<CustomerDto>>(ApiEndpoints.Customers.GetAll);
            
            if (response.Success)
            {
                var customers = response.Data?.Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    phone = c.Phone ?? ""
                }).Cast<object>().ToList() ?? new List<object>();

                return Json(new { success = true, data = customers });
            }
            else
            {
                return Json(new { success = false, error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Get vehicles by customer ID
        /// </summary>
        [HttpGet]
        [Route("GetVehiclesByCustomer/{customerId}")]
        public async Task<IActionResult> GetVehiclesByCustomer(int customerId)
        {
            var response = await _apiService.GetAsync<List<VehicleDto>>(ApiEndpoints.Builder.WithId(ApiEndpoints.Vehicles.GetByCustomerId, customerId));
            
            if (response.Success)
            {
                var vehicles = response.Data?.Select(v => new
                {
                    id = v.Id,
                    licensePlate = v.LicensePlate,
                    brand = v.Brand,
                    model = v.Model,
                    displayText = $"{v.LicensePlate} - {v.Brand} {v.Model}"
                }).Cast<object>().ToList() ?? new List<object>();

                return Json(new { success = true, data = vehicles });
            }
            else
            {
                return Json(new { success = false, error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Get all available services
        /// </summary>
        [HttpGet]
        [Route("GetServices")]
        public async Task<IActionResult> GetServices()
        {
            var response = await _apiService.GetAsync<List<ServiceDto>>(ApiEndpoints.Services.GetAll);
            
            if (response.Success)
            {
                var services = response.Data?
                    .Where(s => s.IsActive)
                    .Select(s => new
                    {
                        id = s.Id,
                        name = s.Name,
                        price = s.Price,
                        duration = s.Duration,
                        category = s.Category ?? "General"
                    }).Cast<object>().ToList() ?? new List<object>();

                return Json(new { success = true, data = services });
            }
            else
            {
                return Json(new { success = false, error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Create new service order via API
        /// </summary>
        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create([FromBody] CreateServiceOrderDto model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model data" });
            }

            var response = await _apiService.PostAsync<object>(ApiEndpoints.ServiceOrders.Create, model);
            
            return Json(new { 
                success = response.Success, 
                message = response.Success ? "Service order created successfully" : response.ErrorMessage 
            });
        }

        /// <summary>
        /// Update service order via API
        /// </summary>
        [HttpPost]
        [Route("Edit")]
        public async Task<IActionResult> Edit([FromBody] UpdateServiceOrderDto model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model data" });
            }

            var response = await _apiService.PutAsync<object>(ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceOrders.Update, model.Id), model);
            
            return Json(new { 
                success = response.Success, 
                message = response.Success ? "Service order updated successfully" : response.ErrorMessage 
            });
        }

        /// <summary>
        /// Delete service order via API
        /// </summary>
        [HttpPost]
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _apiService.DeleteAsync<object>(ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceOrders.Delete, id));
            
            return Json(new { 
                success = response.Success, 
                message = response.Success ? "Service order deleted successfully" : response.ErrorMessage 
            });
        }
    }
}

