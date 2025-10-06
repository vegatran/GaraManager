using GarageManagementSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller for managing customers with full CRUD operations via API
    /// </summary>
    [Authorize]
    public class CustomerManagementController : Controller
    {
        private readonly ApiService _apiService;

        public CustomerManagementController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Display customer management dashboard
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Get all customers for DataTable via API
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            var response = await _apiService.GetAsync<List<CustomerDto>>(ApiEndpoints.Customers.GetAll);
            
            if (response.Success)
            {
                var customerList = new List<object>();
                
                if (response.Data != null)
                {
                    customerList = response.Data.Select(c => new
                    {
                        id = c.Id,
                        name = c.Name,
                        email = c.Email ?? "N/A",
                        phone = c.Phone ?? "N/A",
                        address = c.Address ?? "N/A",
                        createdDate = c.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        isActive = true // Assuming active from API
                    }).Cast<object>().ToList();
                }

                return Json(new { data = customerList });
            }
            else
            {
                return Json(new { error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Get customer details by ID via API
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var response = await _apiService.GetAsync<CustomerDto>(ApiEndpoints.Builder.WithId(ApiEndpoints.Customers.GetById, id));
            
            if (response.Success)
            {
                return PartialView("_CustomerDetails", response.Data);
            }
            else
            {
                return Json(new { error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Create new customer via API
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(CreateCustomerDto model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model data" });
            }

            var response = await _apiService.PostAsync<object>(ApiEndpoints.Customers.Create, model);
            
            return Json(new { 
                success = response.Success, 
                message = response.Success ? "Customer created successfully" : response.ErrorMessage 
            });
        }

        /// <summary>
        /// Update customer via API
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Edit(UpdateCustomerDto model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model data" });
            }

            var response = await _apiService.PutAsync<object>(ApiEndpoints.Builder.WithId(ApiEndpoints.Customers.Update, model.Id), model);
            
            return Json(new { 
                success = response.Success, 
                message = response.Success ? "Customer updated successfully" : response.ErrorMessage 
            });
        }

        /// <summary>
        /// Delete customer via API
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _apiService.DeleteAsync<object>(ApiEndpoints.Builder.WithId(ApiEndpoints.Customers.Delete, id));
            
            return Json(new { 
                success = response.Success, 
                message = response.Success ? "Customer deleted successfully" : response.ErrorMessage 
            });
        }
    }
}