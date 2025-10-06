using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.Api.Controllers
{
    /// <summary>
    /// Dashboard statistics controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiScope")]
    public class DashboardController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Get dashboard statistics
        /// </summary>
        /// <returns>Dashboard statistics</returns>
        [HttpGet("statistics")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var customerCount = await _unitOfWork.Customers.CountAsync();
                var vehicleCount = await _unitOfWork.Vehicles.CountAsync();
                var serviceCount = await _unitOfWork.Services.CountAsync();
                var orderCount = await _unitOfWork.ServiceOrders.CountAsync();

                var statistics = new
                {
                    customerCount,
                    vehicleCount,
                    serviceCount,
                    orderCount,
                    timestamp = DateTime.UtcNow
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
