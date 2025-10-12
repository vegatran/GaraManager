using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Core.Services;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InsuranceClaimController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfigurationService _configService;
        private readonly ILogger<InsuranceClaimController> _logger;

        public InsuranceClaimController(
            IUnitOfWork unitOfWork,
            IConfigurationService configService,
            ILogger<InsuranceClaimController> logger)
        {
            _unitOfWork = unitOfWork;
            _configService = configService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách insurance claims
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetClaims(
            [FromQuery] string? status = null,
            [FromQuery] string? insuranceCompany = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var claims = await _unitOfWork.InsuranceClaims.GetAllAsync();

                // Filter by status
                if (!string.IsNullOrEmpty(status))
                {
                    claims = claims.Where(c => c.Status == status);
                }

                // Filter by insurance company
                if (!string.IsNullOrEmpty(insuranceCompany))
                {
                    claims = claims.Where(c => c.InsuranceCompany.Contains(insuranceCompany));
                }

                // Filter by date range
                if (fromDate.HasValue)
                {
                    claims = claims.Where(c => c.ClaimDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    claims = claims.Where(c => c.ClaimDate <= toDate.Value);
                }

                var result = claims.Select(c => new
                {
                    c.Id,
                    c.ClaimNumber,
                    c.ClaimDate,
                    c.CustomerId,
                    c.CustomerName,
                    c.VehicleId,
                    c.VehiclePlate,
                    c.InsuranceCompany,
                    c.PolicyNumber,
                    c.AccidentDate,
                    c.AccidentDescription,
                    EstimatedAmount = c.EstimatedAmount,
                    ApprovedAmount = c.ApprovedAmount,
                    c.Status,
                    c.CreatedAt
                }).OrderByDescending(c => c.CreatedAt).ToList();

                return Ok(new { success = true, data = result, count = result.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting insurance claims");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy danh sách bồi thường" });
            }
        }

        /// <summary>
        /// Lấy chi tiết insurance claim
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClaim(int id)
        {
            try
            {
                var claim = await _unitOfWork.InsuranceClaims.GetByIdAsync(id);
                if (claim == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy hồ sơ bồi thường" });
                }

                // Get documents
                var documents = await _unitOfWork.Repository<InsuranceClaimDocument>().GetAllAsync();
                var claimDocs = documents.Where(d => d.InsuranceClaimId == id).Select(d => new
                {
                    d.Id,
                    d.DocumentType,
                    d.DocumentName,
                    d.FilePath,
                    d.FileSize,
                    d.UploadedAt,
                    d.UploadedBy
                }).ToList();

                var result = new
                {
                    claim.Id,
                    claim.ClaimNumber,
                    claim.ClaimDate,
                    claim.CustomerId,
                    claim.CustomerName,
                    claim.CustomerPhone,
                    claim.CustomerEmail,
                    claim.VehicleId,
                    claim.VehiclePlate,
                    claim.VehicleMake,
                    claim.VehicleModel,
                    claim.VehicleYear,
                    claim.InsuranceCompany,
                    claim.PolicyNumber,
                    claim.PolicyHolderName,
                    claim.AccidentDate,
                    claim.AccidentLocation,
                    claim.AccidentDescription,
                    claim.DamageDescription,
                    claim.ServiceOrderId,
                    claim.InvoiceId,
                    EstimatedAmount = claim.EstimatedAmount,
                    ApprovedAmount = claim.ApprovedAmount,
                    SettlementAmount = claim.SettlementAmount,
                    claim.Status,
                    claim.ApprovalDate,
                    claim.ApprovedBy,
                    claim.SettlementDate,
                    claim.Notes,
                    Documents = claimDocs,
                    claim.CreatedAt,
                    claim.UpdatedAt
                };

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting claim {id}");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thông tin bồi thường" });
            }
        }

        /// <summary>
        /// Tạo insurance claim mới
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateClaim([FromBody] CreateClaimRequest request)
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

                // Generate claim number
                var claimNumber = await GenerateClaimNumber();

                var claim = new InsuranceClaim
                {
                    ClaimNumber = claimNumber,
                    ClaimDate = DateTime.Now,
                    CustomerId = request.CustomerId,
                    CustomerName = customer.Name,
                    CustomerPhone = customer.Phone,
                    CustomerEmail = customer.Email,
                    VehicleId = request.VehicleId,
                    VehiclePlate = vehicle.LicensePlate,
                    VehicleMake = vehicle.Brand,
                    VehicleModel = vehicle.Model,
                    VehicleYear = string.IsNullOrEmpty(vehicle.Year) ? null : int.TryParse(vehicle.Year, out var year) ? (int?)year : null,
                    InsuranceCompany = request.InsuranceCompany,
                    PolicyNumber = request.PolicyNumber,
                    PolicyHolderName = request.PolicyHolderName ?? customer.Name,
                    AccidentDate = request.AccidentDate,
                    AccidentLocation = request.AccidentLocation,
                    AccidentDescription = request.AccidentDescription,
                    DamageDescription = request.DamageDescription,
                    EstimatedAmount = request.EstimatedAmount,
                    Status = "Pending", // Pending, Approved, Rejected, Settled
                    Notes = request.Notes,
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.InsuranceClaims.AddAsync(claim);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Created insurance claim {claimNumber} for customer {customer.Name}");

                return Ok(new
                {
                    success = true,
                    message = "Tạo hồ sơ bồi thường thành công",
                    data = new
                    {
                        claim.Id,
                        claim.ClaimNumber,
                        claim.Status
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating insurance claim");
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo hồ sơ bồi thường" });
            }
        }

        /// <summary>
        /// Cập nhật insurance claim
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClaim(int id, [FromBody] UpdateClaimRequest request)
        {
            try
            {
                var claim = await _unitOfWork.InsuranceClaims.GetByIdAsync(id);
                if (claim == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy hồ sơ bồi thường" });
                }

                // Only allow update if status is Pending
                if (claim.Status != "Pending")
                {
                    return BadRequest(new { success = false, message = "Chỉ được sửa hồ sơ ở trạng thái Pending" });
                }

                // Update fields
                if (!string.IsNullOrEmpty(request.AccidentLocation))
                    claim.AccidentLocation = request.AccidentLocation;
                
                if (!string.IsNullOrEmpty(request.AccidentDescription))
                    claim.AccidentDescription = request.AccidentDescription;
                
                if (!string.IsNullOrEmpty(request.DamageDescription))
                    claim.DamageDescription = request.DamageDescription;
                
                if (request.EstimatedAmount.HasValue)
                    claim.EstimatedAmount = request.EstimatedAmount.Value;
                
                if (!string.IsNullOrEmpty(request.Notes))
                    claim.Notes = request.Notes;

                claim.UpdatedAt = DateTime.Now;

                await _unitOfWork.InsuranceClaims.UpdateAsync(claim);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Updated insurance claim {id}");

                return Ok(new { success = true, message = "Cập nhật hồ sơ thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating claim {id}");
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật hồ sơ" });
            }
        }

        /// <summary>
        /// Duyệt/Từ chối insurance claim
        /// </summary>
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ApproveClaim(int id, [FromBody] ApproveClaimRequest request)
        {
            try
            {
                var claim = await _unitOfWork.InsuranceClaims.GetByIdAsync(id);
                if (claim == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy hồ sơ bồi thường" });
                }

                if (claim.Status != "Pending")
                {
                    return BadRequest(new { success = false, message = "Hồ sơ không ở trạng thái Pending" });
                }

                var username = User.Identity?.Name ?? "System";

                if (request.Approve)
                {
                    claim.Status = "Approved";
                    claim.ApprovedAmount = request.ApprovedAmount ?? claim.EstimatedAmount;
                    claim.ApprovalDate = DateTime.Now;
                    claim.ApprovedBy = username;
                    
                    _logger.LogInformation($"Approved insurance claim {id} with amount {claim.ApprovedAmount} by {username}");
                }
                else
                {
                    claim.Status = "Rejected";
                    claim.ApprovalDate = DateTime.Now;
                    claim.ApprovedBy = username;
                    
                    _logger.LogInformation($"Rejected insurance claim {id} by {username}");
                }

                if (!string.IsNullOrEmpty(request.Notes))
                {
                    claim.Notes = (claim.Notes ?? "") + $"\n[{DateTime.Now:yyyy-MM-dd HH:mm}] {username}: {request.Notes}";
                }

                claim.UpdatedAt = DateTime.Now;

                await _unitOfWork.InsuranceClaims.UpdateAsync(claim);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = request.Approve ? "Duyệt hồ sơ thành công" : "Từ chối hồ sơ thành công",
                    data = new
                    {
                        claim.Status,
                        ApprovedAmount = claim.ApprovedAmount
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error approving/rejecting claim {id}");
                return StatusCode(500, new { success = false, message = "Lỗi khi xử lý duyệt hồ sơ" });
            }
        }

        /// <summary>
        /// Thanh toán/Settle insurance claim
        /// </summary>
        [HttpPost("{id}/settle")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> SettleClaim(int id, [FromBody] SettleClaimRequest request)
        {
            try
            {
                var claim = await _unitOfWork.InsuranceClaims.GetByIdAsync(id);
                if (claim == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy hồ sơ bồi thường" });
                }

                if (claim.Status != "Approved")
                {
                    return BadRequest(new { success = false, message = "Hồ sơ chưa được duyệt" });
                }

                claim.Status = "Settled";
                claim.SettlementAmount = request.SettlementAmount ?? claim.ApprovedAmount;
                claim.SettlementDate = DateTime.Now;
                claim.InvoiceId = request.InvoiceId;

                if (!string.IsNullOrEmpty(request.Notes))
                {
                    var username = User.Identity?.Name ?? "System";
                    claim.Notes = (claim.Notes ?? "") + $"\n[{DateTime.Now:yyyy-MM-dd HH:mm}] {username}: {request.Notes}";
                }

                claim.UpdatedAt = DateTime.Now;

                await _unitOfWork.InsuranceClaims.UpdateAsync(claim);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Settled insurance claim {id} with amount {claim.SettlementAmount}");

                return Ok(new
                {
                    success = true,
                    message = "Thanh toán bồi thường thành công",
                    data = new
                    {
                        claim.Status,
                        SettlementAmount = claim.SettlementAmount,
                        claim.SettlementDate
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error settling claim {id}");
                return StatusCode(500, new { success = false, message = "Lỗi khi thanh toán bồi thường" });
            }
        }

        /// <summary>
        /// Tạo Service Order từ Insurance Claim
        /// </summary>
        [HttpPost("{id}/create-service-order")]
        public async Task<IActionResult> CreateServiceOrder(int id)
        {
            try
            {
                var claim = await _unitOfWork.InsuranceClaims.GetByIdAsync(id);
                if (claim == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy hồ sơ bồi thường" });
                }

                if (claim.Status != "Approved")
                {
                    return BadRequest(new { success = false, message = "Hồ sơ chưa được duyệt" });
                }

                if (claim.ServiceOrderId.HasValue)
                {
                    return BadRequest(new { success = false, message = "Hồ sơ này đã có đơn hàng" });
                }

                // Generate order number
                var orderNumber = await GenerateOrderNumber();

                var serviceOrder = new ServiceOrder
                {
                    OrderNumber = orderNumber,
                    OrderDate = DateTime.Now,
                    CustomerId = claim.CustomerId ?? 0,
                    VehicleId = claim.VehicleId ?? 0,
                    InsuranceClaimId = id,
                    Description = $"Sửa chữa theo claim {claim.ClaimNumber}: {claim.DamageDescription}",
                    EstimatedAmount = claim.ApprovedAmount ?? 0,
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.ServiceOrders.AddAsync(serviceOrder);
                await _unitOfWork.SaveChangesAsync();

                // Update claim with service order id
                claim.ServiceOrderId = serviceOrder.Id;
                claim.UpdatedAt = DateTime.Now;
                await _unitOfWork.InsuranceClaims.UpdateAsync(claim);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Created service order {orderNumber} from claim {id}");

                return Ok(new
                {
                    success = true,
                    message = "Tạo đơn hàng thành công",
                    data = new
                    {
                        serviceOrder.Id,
                        serviceOrder.OrderNumber
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating service order from claim {id}");
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo đơn hàng" });
            }
        }

        /// <summary>
        /// Upload document cho insurance claim
        /// </summary>
        [HttpPost("{id}/documents")]
        public async Task<IActionResult> UploadDocument(int id, [FromForm] UploadDocumentRequest request)
        {
            try
            {
                var claim = await _unitOfWork.InsuranceClaims.GetByIdAsync(id);
                if (claim == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy hồ sơ bồi thường" });
                }

                if (request.File == null || request.File.Length == 0)
                {
                    return BadRequest(new { success = false, message = "File không hợp lệ" });
                }

                // Save file (implement your file storage logic here)
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "insurance-claims", id.ToString());
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{request.File.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream);
                }

                var document = new InsuranceClaimDocument
                {
                    InsuranceClaimId = id,
                    DocumentType = request.DocumentType,
                    DocumentName = request.File.FileName,
                    FilePath = filePath,
                    FileSize = request.File.Length,
                    UploadedAt = DateTime.Now,
                    UploadedBy = null, // Will be set by authentication
                    UploadedByName = User.Identity?.Name ?? "System"
                };

                await _unitOfWork.Repository<InsuranceClaimDocument>().AddAsync(document);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Uploaded document for claim {id}");

                return Ok(new
                {
                    success = true,
                    message = "Upload tài liệu thành công",
                    data = new
                    {
                        document.Id,
                        document.DocumentName,
                        document.FileSize
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading document for claim {id}");
                return StatusCode(500, new { success = false, message = "Lỗi khi upload tài liệu" });
            }
        }

        /// <summary>
        /// Generate claim number: CLM-YYYYMM-XXXX
        /// </summary>
        private async Task<string> GenerateClaimNumber()
        {
            var today = DateTime.Now;
            var prefix = $"CLM-{today:yyyyMM}-";

            var claims = await _unitOfWork.InsuranceClaims.GetAllAsync();
            var maxNumber = claims
                .Where(c => c.ClaimNumber.StartsWith(prefix))
                .Select(c =>
                {
                    var numPart = c.ClaimNumber.Substring(prefix.Length);
                    return int.TryParse(numPart, out int num) ? num : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            return $"{prefix}{(maxNumber + 1):D4}";
        }

        /// <summary>
        /// Generate order number: SO-YYYYMM-XXXX
        /// </summary>
        private async Task<string> GenerateOrderNumber()
        {
            var today = DateTime.Now;
            var prefix = $"SO-{today:yyyyMM}-";

            var orders = await _unitOfWork.ServiceOrders.GetAllAsync();
            var maxNumber = orders
                .Where(o => o.OrderNumber.StartsWith(prefix))
                .Select(o =>
                {
                    var numPart = o.OrderNumber.Substring(prefix.Length);
                    return int.TryParse(numPart, out int num) ? num : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            return $"{prefix}{(maxNumber + 1):D4}";
        }
    }

    #region Request Models

    public class CreateClaimRequest
    {
        public int CustomerId { get; set; }
        public int VehicleId { get; set; }
        public string InsuranceCompany { get; set; } = string.Empty;
        public string PolicyNumber { get; set; } = string.Empty;
        public string? PolicyHolderName { get; set; }
        public DateTime AccidentDate { get; set; }
        public string AccidentLocation { get; set; } = string.Empty;
        public string AccidentDescription { get; set; } = string.Empty;
        public string DamageDescription { get; set; } = string.Empty;
        public decimal EstimatedAmount { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateClaimRequest
    {
        public string? AccidentLocation { get; set; }
        public string? AccidentDescription { get; set; }
        public string? DamageDescription { get; set; }
        public decimal? EstimatedAmount { get; set; }
        public string? Notes { get; set; }
    }

    public class ApproveClaimRequest
    {
        public bool Approve { get; set; }
        public decimal? ApprovedAmount { get; set; }
        public string? Notes { get; set; }
    }

    public class SettleClaimRequest
    {
        public decimal? SettlementAmount { get; set; }
        public int? InvoiceId { get; set; }
        public string? Notes { get; set; }
    }

    public class UploadDocumentRequest
    {
        public IFormFile File { get; set; } = null!;
        public string DocumentType { get; set; } = "Other"; // Photo, Police Report, Estimate, Invoice, Other
    }

    #endregion
}

