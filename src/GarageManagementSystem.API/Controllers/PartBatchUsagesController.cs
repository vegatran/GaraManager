using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PartBatchUsagesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PartBatchUsagesController> _logger;

        public PartBatchUsagesController(IUnitOfWork unitOfWork, ILogger<PartBatchUsagesController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet("batch/{batchId}")]
        public async Task<IActionResult> GetByBatch(int batchId)
        {
            try
            {
                var usages = await _unitOfWork.Repository<PartBatchUsage>().GetAllAsync();
                var result = usages.Where(u => u.PartInventoryBatchId == batchId).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting batch usages");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy lịch sử sử dụng" });
            }
        }

        [HttpGet("service-order/{serviceOrderId}")]
        public async Task<IActionResult> GetByServiceOrder(int serviceOrderId)
        {
            try
            {
                var usages = await _unitOfWork.Repository<PartBatchUsage>().GetAllAsync();
                var result = usages.Where(u => u.ServiceOrderId == serviceOrderId).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting batch usages");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy lịch sử sử dụng" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PartBatchUsage usage)
        {
            try
            {
                usage.UsageDate = DateTime.Now;
                usage.CreatedAt = DateTime.Now;
                await _unitOfWork.Repository<PartBatchUsage>().AddAsync(usage);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, data = usage, message = "Ghi nhận sử dụng thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating batch usage");
                return StatusCode(500, new { success = false, message = "Lỗi khi ghi nhận sử dụng" });
            }
        }
    }
}

