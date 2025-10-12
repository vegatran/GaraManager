using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InspectionController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<InspectionController> _logger;

        public InspectionController(
            IUnitOfWork unitOfWork,
            ILogger<InspectionController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách inspections
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetInspections(
            [FromQuery] string? status = null,
            [FromQuery] int? customerId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var inspections = await _unitOfWork.Inspections.GetAllAsync();

                // Filters
                if (!string.IsNullOrEmpty(status))
                    inspections = inspections.Where(i => i.Status == status);

                if (customerId.HasValue)
                    inspections = inspections.Where(i => i.CustomerId == customerId.Value);

                if (fromDate.HasValue)
                    inspections = inspections.Where(i => i.InspectionDate >= fromDate.Value);

                if (toDate.HasValue)
                    inspections = inspections.Where(i => i.InspectionDate <= toDate.Value);

                var result = inspections.Select(i => new
                {
                    i.Id,
                    i.InspectionNumber,
                    i.InspectionDate,
                    i.CustomerId,
                    i.CustomerName,
                    i.VehicleId,
                    i.VehiclePlate,
                    i.Mileage,
                    i.InspectorId,
                    i.InspectorName,
                    i.Status,
                    i.CreatedAt
                }).OrderByDescending(i => i.CreatedAt).ToList();

                return Ok(new { success = true, data = result, count = result.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inspections");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy danh sách kiểm tra" });
            }
        }

        /// <summary>
        /// Lấy chi tiết inspection
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInspection(int id)
        {
            try
            {
                var inspection = await _unitOfWork.Inspections.GetByIdAsync(id);
                if (inspection == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy phiếu kiểm tra" });
                }

                var result = new
                {
                    inspection.Id,
                    inspection.InspectionNumber,
                    inspection.InspectionDate,
                    inspection.CustomerId,
                    inspection.CustomerName,
                    inspection.CustomerPhone,
                    inspection.VehicleId,
                    inspection.VehiclePlate,
                    inspection.VehicleMake,
                    inspection.VehicleModel,
                    inspection.VehicleYear,
                    inspection.Mileage,
                    inspection.InspectorId,
                    inspection.InspectorName,
                    inspection.Findings,
                    inspection.Recommendations,
                    inspection.Status,
                    inspection.Notes,
                    inspection.CreatedAt,
                    inspection.UpdatedAt
                };

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting inspection {id}");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thông tin kiểm tra" });
            }
        }

        /// <summary>
        /// Tạo inspection mới
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateInspection([FromBody] CreateInspectionRequest request)
        {
            try
            {
                // Validate customer and vehicle
                var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
                if (customer == null)
                {
                    return BadRequest(new { success = false, message = "Không tìm thấy khách hàng" });
                }

                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.VehicleId);
                if (vehicle == null)
                {
                    return BadRequest(new { success = false, message = "Không tìm thấy xe" });
                }

                // Validate inspector
                Employee? inspector = null;
                if (request.InspectorId.HasValue)
                {
                    inspector = await _unitOfWork.Employees.GetByIdAsync(request.InspectorId.Value);
                    if (inspector == null)
                    {
                        return BadRequest(new { success = false, message = "Không tìm thấy nhân viên kiểm tra" });
                    }
                }

                // Generate inspection number
                var inspectionNumber = await GenerateInspectionNumber();

                var inspection = new VehicleInspection
                {
                    InspectionNumber = inspectionNumber,
                    InspectionDate = request.InspectionDate ?? DateTime.Now,
                    CustomerId = request.CustomerId,
                    CustomerName = customer.Name,
                    CustomerPhone = customer.Phone,
                    VehicleId = request.VehicleId,
                    VehiclePlate = vehicle.LicensePlate,
                    VehicleMake = vehicle.Brand,
                    VehicleModel = vehicle.Model,
                    VehicleYear = string.IsNullOrEmpty(vehicle.Year) ? null : int.TryParse(vehicle.Year, out var vYear) ? (int?)vYear : null,
                    Mileage = request.Mileage,
                    InspectorId = request.InspectorId,
                    InspectorName = inspector?.Name,
                    Findings = request.Findings,
                    Recommendations = request.Recommendations,
                    Status = "Draft",
                    Notes = request.Notes,
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.Inspections.AddAsync(inspection);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Created inspection {inspectionNumber}");

                return Ok(new
                {
                    success = true,
                    message = "Tạo phiếu kiểm tra thành công",
                    data = new
                    {
                        inspection.Id,
                        inspection.InspectionNumber
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating inspection");
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo phiếu kiểm tra" });
            }
        }

        /// <summary>
        /// Cập nhật inspection
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInspection(int id, [FromBody] UpdateInspectionRequest request)
        {
            try
            {
                var inspection = await _unitOfWork.Inspections.GetByIdAsync(id);
                if (inspection == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy phiếu kiểm tra" });
                }

                // Only allow update if status is Draft
                if (inspection.Status != "Draft")
                {
                    return BadRequest(new { success = false, message = "Chỉ được sửa phiếu kiểm tra ở trạng thái Draft" });
                }

                // Update fields
                if (request.Mileage.HasValue)
                    inspection.Mileage = request.Mileage.Value;

                if (request.InspectorId.HasValue)
                {
                    var inspector = await _unitOfWork.Employees.GetByIdAsync(request.InspectorId.Value);
                    if (inspector != null)
                    {
                        inspection.InspectorId = request.InspectorId.Value;
                        inspection.InspectorName = inspector.Name;
                    }
                }

                if (!string.IsNullOrEmpty(request.Findings))
                    inspection.Findings = request.Findings;

                if (!string.IsNullOrEmpty(request.Recommendations))
                    inspection.Recommendations = request.Recommendations;

                if (!string.IsNullOrEmpty(request.Notes))
                    inspection.Notes = request.Notes;

                inspection.UpdatedAt = DateTime.Now;

                await _unitOfWork.Inspections.UpdateAsync(inspection);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Updated inspection {id}");

                return Ok(new { success = true, message = "Cập nhật phiếu kiểm tra thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating inspection {id}");
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật phiếu kiểm tra" });
            }
        }

        /// <summary>
        /// Hoàn thành inspection (chuyển sang Completed)
        /// </summary>
        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompleteInspection(int id, [FromBody] CompleteInspectionRequest request)
        {
            try
            {
                var inspection = await _unitOfWork.Inspections.GetByIdAsync(id);
                if (inspection == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy phiếu kiểm tra" });
                }

                if (inspection.Status != "Draft")
                {
                    return BadRequest(new { success = false, message = "Phiếu kiểm tra không ở trạng thái Draft" });
                }

                // Update findings and recommendations if provided
                if (!string.IsNullOrEmpty(request.Findings))
                    inspection.Findings = request.Findings;

                if (!string.IsNullOrEmpty(request.Recommendations))
                    inspection.Recommendations = request.Recommendations;

                inspection.Status = "Completed";
                inspection.UpdatedAt = DateTime.Now;

                await _unitOfWork.Inspections.UpdateAsync(inspection);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Completed inspection {id}");

                return Ok(new { success = true, message = "Hoàn thành kiểm tra thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error completing inspection {id}");
                return StatusCode(500, new { success = false, message = "Lỗi khi hoàn thành kiểm tra" });
            }
        }

        /// <summary>
        /// Xóa inspection (chỉ xóa Draft)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInspection(int id)
        {
            try
            {
                var inspection = await _unitOfWork.Inspections.GetByIdAsync(id);
                if (inspection == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy phiếu kiểm tra" });
                }

                if (inspection.Status != "Draft")
                {
                    return BadRequest(new { success = false, message = "Chỉ được xóa phiếu kiểm tra ở trạng thái Draft" });
                }

                // Check if has quotation
                var quotations = await _unitOfWork.Quotations.GetAllAsync();
                if (quotations.Any(q => q.InspectionId == id))
                {
                    return BadRequest(new { success = false, message = "Không thể xóa vì đã có báo giá" });
                }

                await _unitOfWork.Inspections.DeleteAsync(inspection);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Deleted inspection {id}");

                return Ok(new { success = true, message = "Xóa phiếu kiểm tra thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting inspection {id}");
                return StatusCode(500, new { success = false, message = "Lỗi khi xóa phiếu kiểm tra" });
            }
        }

        /// <summary>
        /// Generate inspection number: INS-YYYYMM-XXXX
        /// </summary>
        private async Task<string> GenerateInspectionNumber()
        {
            var today = DateTime.Now;
            var prefix = $"INS-{today:yyyyMM}-";

            var inspections = await _unitOfWork.Inspections.GetAllAsync();
            var maxNumber = inspections
                .Where(i => i.InspectionNumber.StartsWith(prefix))
                .Select(i =>
                {
                    var numPart = i.InspectionNumber.Substring(prefix.Length);
                    return int.TryParse(numPart, out int num) ? num : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            return $"{prefix}{(maxNumber + 1):D4}";
        }
    }

    #region Request Models

    public class CreateInspectionRequest
    {
        public int CustomerId { get; set; }
        public int VehicleId { get; set; }
        public DateTime? InspectionDate { get; set; }
        public int Mileage { get; set; }
        public int? InspectorId { get; set; }
        public string? Findings { get; set; }
        public string? Recommendations { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateInspectionRequest
    {
        public int? Mileage { get; set; }
        public int? InspectorId { get; set; }
        public string? Findings { get; set; }
        public string? Recommendations { get; set; }
        public string? Notes { get; set; }
    }

    public class CompleteInspectionRequest
    {
        public string? Findings { get; set; }
        public string? Recommendations { get; set; }
    }

    #endregion
}

