using GarageManagementSystem.Shared.Models;
using GarageManagementSystem.Web.Configuration;
using GarageManagementSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller quản lý các báo cáo - Phase 4.3
    /// </summary>
    [Authorize]
    [Route("Reports")]
    public class ReportsController : Controller
    {
        private readonly ApiService _apiService;

        public ReportsController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Báo cáo Doanh Thu
        /// </summary>
        [HttpGet("Revenue")]
        public IActionResult Revenue()
        {
            return View();
        }

        /// <summary>
        /// Báo cáo Lợi Nhuận
        /// </summary>
        [HttpGet("Profit")]
        public IActionResult Profit()
        {
            return View();
        }

        /// <summary>
        /// Báo cáo Kết quả Kinh doanh (Income Statement)
        /// </summary>
        [HttpGet("IncomeStatement")]
        public IActionResult IncomeStatement()
        {
            return View();
        }

        /// <summary>
        /// Báo cáo Dịch Vụ
        /// </summary>
        [HttpGet("Services")]
        public IActionResult Services()
        {
            return View();
        }

        /// <summary>
        /// Báo cáo Tồn Kho
        /// </summary>
        [HttpGet("Inventory")]
        public IActionResult Inventory()
        {
            return View();
        }

        /// <summary>
        /// Báo cáo Khách Hàng
        /// </summary>
        [HttpGet("Customers")]
        public IActionResult Customers()
        {
            return View();
        }

        /// <summary>
        /// Báo cáo Top Khách Hàng
        /// </summary>
        [HttpGet("TopCustomers")]
        public IActionResult TopCustomers()
        {
            return View();
        }

        /// <summary>
        /// Báo cáo Top Dịch Vụ
        /// </summary>
        [HttpGet("TopServices")]
        public IActionResult TopServices()
        {
            return View();
        }

        /// <summary>
        /// Báo cáo Top Phụ Tùng
        /// </summary>
        [HttpGet("TopParts")]
        public IActionResult TopParts()
        {
            return View();
        }

        /// <summary>
        /// Báo cáo Thống kê Đơn Hàng
        /// </summary>
        [HttpGet("ServiceOrderStats")]
        public IActionResult ServiceOrderStats()
        {
            return View();
        }

        /// <summary>
        /// Báo cáo Hiệu suất Nhân viên
        /// </summary>
        [HttpGet("EmployeePerformance")]
        public IActionResult EmployeePerformance()
        {
            return View();
        }

        /// <summary>
        /// Báo cáo Phân tích Sử dụng Phụ tùng
        /// </summary>
        [HttpGet("PartsUsage")]
        public IActionResult PartsUsage()
        {
            return View();
        }

        /// <summary>
        /// Báo cáo Tổng kết Bảo hiểm
        /// </summary>
        [HttpGet("Insurance")]
        public IActionResult Insurance()
        {
            return View();
        }
    }
}

