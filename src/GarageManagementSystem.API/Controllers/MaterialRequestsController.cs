using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Enums;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using GarageManagementSystem.Core.Extensions;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize(Policy = "ApiScope")]
    [ApiController]
    [Route("api/[controller]")]
    public class MaterialRequestsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<MaterialRequestsController> _logger;
        private readonly ICOGSCalculationService _cogsCalculationService;
        private readonly GarageDbContext _context;

        public MaterialRequestsController(
            IUnitOfWork unitOfWork, 
            IMapper mapper, 
            ILogger<MaterialRequestsController> logger,
            ICOGSCalculationService cogsCalculationService,
            GarageDbContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _cogsCalculationService = cogsCalculationService;
            _context = context;
        }

        /// <summary>
        /// Helper method để map authenticated user từ Identity sang EmployeeId
        /// </summary>
        private async Task<int?> GetCurrentEmployeeIdAsync()
        {
            try
            {
                // Lấy email từ claims
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value 
                    ?? User.FindFirst("email")?.Value;
                
                var userName = User.FindFirst(ClaimTypes.Name)?.Value 
                    ?? User.FindFirst("name")?.Value
                    ?? User.Identity?.Name;

                // Try to find Employee by email (ưu tiên)
                if (!string.IsNullOrEmpty(userEmail))
                {
                    var employees = await _unitOfWork.Employees.FindAsync(e => 
                        e.Email != null && 
                        e.Email.ToLower() == userEmail.ToLower());
                    
                    var employeeByEmail = employees.FirstOrDefault();
                    if (employeeByEmail != null)
                    {
                        _logger.LogInformation($"Found employee by email: {employeeByEmail.Id} - {employeeByEmail.Name}");
                        return employeeByEmail.Id;
                    }
                }

                // Try to find Employee by name if email not found
                if (!string.IsNullOrEmpty(userName))
                {
                    var employees = await _unitOfWork.Employees.FindAsync(e => 
                        e.Name != null && 
                        e.Name.ToLower().Contains(userName.ToLower()));
                    
                    var employeeByName = employees.FirstOrDefault();
                    if (employeeByName != null)
                    {
                        _logger.LogInformation($"Found employee by name: {employeeByName.Id} - {employeeByName.Name}");
                        return employeeByName.Id;
                    }
                }

                // Try to get EmployeeId directly from claims (nếu có)
                var employeeIdClaim = User.FindFirst("employee_id")?.Value 
                    ?? User.FindFirst("employeeid")?.Value;
                
                if (!string.IsNullOrEmpty(employeeIdClaim) && int.TryParse(employeeIdClaim, out var empId))
                {
                    var employee = await _unitOfWork.Employees.GetByIdAsync(empId);
                    if (employee != null)
                    {
                        _logger.LogInformation($"Found employee by claim: {employee.Id} - {employee.Name}");
                        return employee.Id;
                    }
                }

                _logger.LogWarning($"Cannot find Employee for authenticated user. email: {userEmail}, name: {userName}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current employee ID from Identity");
                return null;
            }
        }

        /// <summary>
        /// Danh sách MR có phân trang và filter
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResponse<MaterialRequestDto>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? serviceOrderId = null,
            [FromQuery] MaterialRequestStatus? status = null)
        {
            try
            {
                // ✅ OPTIMIZED: Query ở database level thay vì load tất cả vào memory
                var query = _context.MaterialRequests
                    .Where(m => !m.IsDeleted)
                    .AsQueryable();

                if (serviceOrderId.HasValue)
                    query = query.Where(m => m.ServiceOrderId == serviceOrderId.Value);
                if (status.HasValue)
                    query = query.Where(m => m.Status == status.Value);

                query = query.OrderByDescending(m => m.CreatedAt);

                // ✅ OPTIMIZED: Get total count ở database level (trước khi paginate)
                var totalCount = await query.CountAsync();
                
                // ✅ OPTIMIZED: Apply pagination ở database level với Skip/Take
                var data = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Include(m => m.ServiceOrder)
                    .Include(m => m.Items)
                    .ToListAsync();
                
                var dtos = _mapper.Map<List<MaterialRequestDto>>(data);

                return Ok(PagedResponse<MaterialRequestDto>.CreateSuccessResult(
                    dtos, pageNumber, pageSize, totalCount, "Lấy danh sách MR thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting MR paged list");
                return StatusCode(500, PagedResponse<MaterialRequestDto>.CreateErrorResult("Lỗi khi lấy danh sách MR"));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<MaterialRequestDto>>> Create([FromBody] CreateMaterialRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<MaterialRequestDto>.ErrorResult("Dữ liệu không hợp lệ"));

            var serviceOrder = await _unitOfWork.ServiceOrders.GetByIdAsync(dto.ServiceOrderId);
            if (serviceOrder == null) return NotFound(ApiResponse<MaterialRequestDto>.ErrorResult("Không tìm thấy phiếu sửa chữa"));

            // Generate MR number
            var count = await _unitOfWork.Repository<MaterialRequest>().CountAsync();
            var mrNumber = $"MR-{DateTime.Now:yyyyMMdd}-{(count + 1):D4}";

            var mr = _mapper.Map<MaterialRequest>(dto);
            mr.MRNumber = mrNumber;
            mr.Status = MaterialRequestStatus.Draft;
            
            // ✅ Map current user từ Identity sang EmployeeId
            var currentEmployeeId = await GetCurrentEmployeeIdAsync();
            mr.RequestedById = currentEmployeeId ?? 0; // Default to 0 if not found (system user)

            await _unitOfWork.BeginTransactionAsync();
            await _unitOfWork.Repository<MaterialRequest>().AddAsync(mr);

            foreach (var itemDto in dto.Items)
            {
                var part = await _unitOfWork.Repository<Part>().GetByIdAsync(itemDto.PartId);
                if (part == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return BadRequest(ApiResponse<MaterialRequestDto>.ErrorResult($"Không tìm thấy phụ tùng ID {itemDto.PartId}"));
                }

                var item = _mapper.Map<MaterialRequestItem>(itemDto);
                item.MaterialRequest = mr;
                item.PartName = part.PartName;
                await _unitOfWork.Repository<MaterialRequestItem>().AddAsync(item);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            var result = _mapper.Map<MaterialRequestDto>(mr);
            return Ok(ApiResponse<MaterialRequestDto>.SuccessResult(result, "Tạo MR thành công"));
        }

        [HttpPut("{id}/submit")]
        public async Task<ActionResult<ApiResponse<object>>> Submit(int id)
        {
            var mr = await _unitOfWork.Repository<MaterialRequest>().GetByIdAsync(id);
            if (mr == null) return NotFound(ApiResponse<object>.ErrorResult("Không tìm thấy MR"));
            if (mr.Status != MaterialRequestStatus.Draft) return BadRequest(ApiResponse<object>.ErrorResult("Chỉ MR trạng thái Draft mới được gửi duyệt"));

            mr.Status = MaterialRequestStatus.PendingApproval;
            await _unitOfWork.Repository<MaterialRequest>().UpdateAsync(mr);
            await _unitOfWork.SaveChangesAsync();
            return Ok(ApiResponse<object>.SuccessResult(null, "Đã gửi MR chờ duyệt"));
        }

        [HttpPut("{id}/approve")]
        public async Task<ActionResult<ApiResponse<object>>> Approve(int id)
        {
            var mr = await _unitOfWork.Repository<MaterialRequest>().GetByIdAsync(id);
            if (mr == null) return NotFound(ApiResponse<object>.ErrorResult("Không tìm thấy MR"));
            if (mr.Status != MaterialRequestStatus.PendingApproval) return BadRequest(ApiResponse<object>.ErrorResult("Chỉ MR chờ duyệt mới được duyệt"));

            mr.Status = MaterialRequestStatus.Approved;
            mr.ApprovedAt = DateTime.Now;
            
            // ✅ Map current user từ Identity sang EmployeeId
            var currentEmployeeId = await GetCurrentEmployeeIdAsync();
            mr.ApprovedById = currentEmployeeId; // Nullable, có thể null nếu không tìm thấy
            
            await _unitOfWork.Repository<MaterialRequest>().UpdateAsync(mr);
            await _unitOfWork.SaveChangesAsync();
            return Ok(ApiResponse<object>.SuccessResult(null, "Đã duyệt MR"));
        }

        [HttpPut("{id}/reject")]
        public async Task<ActionResult<ApiResponse<object>>> Reject(int id, [FromBody] ChangeMaterialRequestStatusDto dto)
        {
            var mr = await _unitOfWork.Repository<MaterialRequest>().GetByIdAsync(id);
            if (mr == null) return NotFound(ApiResponse<object>.ErrorResult("Không tìm thấy MR"));
            if (mr.Status != MaterialRequestStatus.PendingApproval) return BadRequest(ApiResponse<object>.ErrorResult("Chỉ MR chờ duyệt mới được từ chối"));

            mr.Status = MaterialRequestStatus.Rejected;
            mr.RejectReason = dto.Reason;
            await _unitOfWork.Repository<MaterialRequest>().UpdateAsync(mr);
            await _unitOfWork.SaveChangesAsync();
            return Ok(ApiResponse<object>.SuccessResult(null, "Đã từ chối MR"));
        }

        /// <summary>
        /// ✅ 3.1 Task 2.2: Issue MR - Xuất vật tư cho JO và tự động tính COGS
        /// </summary>
        [HttpPut("{id}/issue")]
        public async Task<ActionResult<ApiResponse<object>>> Issue(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Load MR với items và ServiceOrder
                var mr = await _context.MaterialRequests
                    .Include(m => m.Items)
                    .Include(m => m.ServiceOrder)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (mr == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return NotFound(ApiResponse<object>.ErrorResult("Không tìm thấy MR"));
                }

                if (mr.Status != MaterialRequestStatus.Picked)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return BadRequest(ApiResponse<object>.ErrorResult("Chỉ MR trạng thái 'Đã Lấy' (Picked) mới được xuất"));
                }

                // ✅ FIX: Sử dụng ServiceOrder đã được Include từ MR
                var serviceOrder = mr.ServiceOrder;
                if (serviceOrder == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return BadRequest(ApiResponse<object>.ErrorResult("Không tìm thấy phiếu sửa chữa"));
                }

                // ✅ FIX: Load Customer và Vehicle nếu chưa được Include
                if (serviceOrder.Customer == null || serviceOrder.Vehicle == null)
                {
                    await _context.Entry(serviceOrder)
                        .Reference(so => so.Customer)
                        .LoadAsync();
                    await _context.Entry(serviceOrder)
                        .Reference(so => so.Vehicle)
                        .LoadAsync();
                }

                // ✅ FIX: Validate MR có items
                if (mr.Items == null || !mr.Items.Any())
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return BadRequest(ApiResponse<object>.ErrorResult("MR không có vật tư nào để xuất"));
                }

                // ✅ FIX: Dictionary để track tổng số lượng giảm cho mỗi Part (tránh update nhiều lần)
                // Key: PartId, Value: (Part, totalQuantityToReduce, initialStock, currentStockForTransactions)
                var partsToUpdate = new Dictionary<int, (Part part, int totalQuantityToReduce, int initialStock, int currentStockForTransactions)>();

                // ✅ FIX: Pre-load tất cả Parts để tránh N+1 queries và đảm bảo initialStock chính xác
                var partIds = mr.Items.Select(i => i.PartId).Distinct().ToList();
                var allParts = await _unitOfWork.Repository<Part>()
                    .FindAsync(p => partIds.Contains(p.Id));
                var partsDict = allParts.ToDictionary(p => p.Id);

                // ✅ FIX: Initialize partsToUpdate với giá trị ban đầu cho tất cả Parts
                foreach (var partId in partIds)
                {
                    if (!partsDict.TryGetValue(partId, out var part))
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return BadRequest(ApiResponse<object>.ErrorResult($"Không tìm thấy phụ tùng ID {partId}"));
                    }
                    partsToUpdate[partId] = (part, 0, part.QuantityInStock, part.QuantityInStock);
                }

                // Xử lý từng item trong MR
                foreach (var mrItem in mr.Items)
                {
                    var quantityToIssue = mrItem.QuantityPicked; // Số lượng đã pick
                    if (quantityToIssue <= 0) continue;

                    // ✅ FIX: Sử dụng Part đã được pre-load
                    if (!partsDict.TryGetValue(mrItem.PartId, out var part))
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return BadRequest(ApiResponse<object>.ErrorResult($"Không tìm thấy phụ tùng ID {mrItem.PartId}"));
                    }

                    // ✅ Lấy các batch theo FIFO (cũ nhất trước)
                    var availableBatches = await _context.PartInventoryBatches
                        .Include(b => b.Part)
                        .Where(b => b.PartId == mrItem.PartId
                            && b.IsActive
                            && b.QuantityRemaining > 0)
                        .OrderBy(b => b.ReceiveDate) // FIFO: cũ nhất trước
                        .ThenBy(b => b.Id)
                        .ToListAsync();

                    if (!availableBatches.Any())
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return BadRequest(ApiResponse<object>.ErrorResult(
                            $"Không đủ tồn kho cho phụ tùng: {part?.PartName ?? $"ID {mrItem.PartId}"}"));
                    }

                    int remainingQuantity = quantityToIssue;
                    // ✅ FIX: Lấy currentStockForPart từ dictionary (không reset mỗi lần loop item mới)
                    var partInfo = partsToUpdate[part.Id];
                    int currentStockForPart = partInfo.currentStockForTransactions; // Giá trị tồn kho hiện tại (đã được cập nhật từ các item trước)

                    // Phân bổ theo FIFO
                    foreach (var batch in availableBatches)
                    {
                        if (remainingQuantity <= 0) break;

                        // ✅ FIX: Re-check QuantityRemaining để tránh race condition (defensive check)
                        if (batch.QuantityRemaining <= 0)
                        {
                            continue; // Skip batch này nếu đã hết (có thể do concurrent request)
                        }

                        int quantityFromBatch = Math.Min(remainingQuantity, batch.QuantityRemaining);

                        // ✅ FIX: Skip nếu quantityFromBatch = 0 (defensive check)
                        if (quantityFromBatch <= 0)
                        {
                            continue;
                        }

                        // ✅ FIX: Validate batch.Part không null (defensive check)
                        if (batch.Part == null)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return BadRequest(ApiResponse<object>.ErrorResult(
                                $"Batch '{batch.BatchNumber}' không có thông tin phụ tùng. Vui lòng kiểm tra lại dữ liệu."));
                        }

                        // Tạo PartBatchUsage
                        var batchUsage = new PartBatchUsage
                        {
                            PartInventoryBatchId = batch.Id,
                            ServiceOrderId = serviceOrder.Id,
                            QuantityUsed = quantityFromBatch,
                            UnitCost = batch.UnitCost,
                            UnitPrice = part.SellPrice,
                            TotalCost = quantityFromBatch * batch.UnitCost,
                            TotalPrice = quantityFromBatch * part.SellPrice,
                            UsageDate = DateTime.Now,
                            CustomerId = serviceOrder.CustomerId,
                            CustomerName = serviceOrder.Customer?.Name,
                            CustomerType = serviceOrder.Customer != null 
                                ? (!string.IsNullOrEmpty(serviceOrder.Customer.TaxCode) ? "Company" : "Individual")
                                : null,
                            VehicleId = serviceOrder.VehicleId,
                            VehiclePlate = serviceOrder.Vehicle?.LicensePlate,
                            RequiresInvoice = batch.HasInvoice,
                            OutgoingInvoiceNumber = batch.InvoiceNumber,
                            InvoiceDate = batch.InvoiceDate,
                            Notes = $"Xuất từ MR: {mr.MRNumber}"
                        };

                        await _unitOfWork.Repository<PartBatchUsage>().AddAsync(batchUsage);

                        // Giảm số lượng tồn kho của batch
                        batch.QuantityRemaining -= quantityFromBatch;
                        // ✅ FIX: Validate để đảm bảo QuantityRemaining không bị âm (defensive check)
                        if (batch.QuantityRemaining < 0)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return BadRequest(ApiResponse<object>.ErrorResult(
                                $"Lỗi tính toán tồn kho cho batch '{batch.BatchNumber ?? "N/A"}'. Số lượng còn lại sẽ bị âm. Vui lòng kiểm tra lại dữ liệu."));
                        }
                        if (batch.QuantityRemaining <= 0)
                        {
                            batch.IsActive = false;
                        }
                        await _unitOfWork.Repository<PartInventoryBatch>().UpdateAsync(batch);

                        // ✅ FIX: Tính QuantityBefore và QuantityAfter dựa trên giá trị hiện tại (đã được cập nhật từ các batch trước)
                        int quantityBefore = currentStockForPart;
                        currentStockForPart -= quantityFromBatch; // Cập nhật tồn kho hiện tại
                        // ✅ FIX: Validate để đảm bảo tồn kho không bị âm (defensive check)
                        if (currentStockForPart < 0)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return BadRequest(ApiResponse<object>.ErrorResult(
                                $"Lỗi tính toán tồn kho cho phụ tùng '{part?.PartName ?? $"ID {part.Id}"}'. Tồn kho sẽ bị âm ({quantityBefore} - {quantityFromBatch} = {currentStockForPart}). Vui lòng kiểm tra lại dữ liệu."));
                        }
                        int quantityAfter = currentStockForPart;
                        
                        // ✅ FIX: Cập nhật lại currentStockForTransactions trong dictionary (lấy lại partInfo mới nhất từ dictionary)
                        var updatedPartInfo = partsToUpdate[part.Id];
                        partsToUpdate[part.Id] = (updatedPartInfo.part, updatedPartInfo.totalQuantityToReduce, updatedPartInfo.initialStock, currentStockForPart);

                        // Tạo StockTransaction
                        var stockTx = new StockTransaction
                        {
                            TransactionNumber = $"STK-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
                            TransactionType = StockTransactionType.XuatKho,
                            PartId = part.Id,
                            Quantity = quantityFromBatch,
                            QuantityBefore = quantityBefore,
                            QuantityAfter = quantityAfter,
                            StockAfter = quantityAfter,
                            UnitCost = batch.UnitCost,
                            UnitPrice = part.SellPrice,
                            TotalCost = quantityFromBatch * batch.UnitCost,
                            TotalAmount = quantityFromBatch * part.SellPrice,
                            TransactionDate = DateTime.Now,
                            ServiceOrderId = serviceOrder.Id,
                            RelatedEntity = "MaterialRequest",
                            RelatedEntityId = mr.Id,
                            Notes = $"Xuất vật tư từ MR: {mr.MRNumber}, Batch: {batch.BatchNumber}"
                        };
                        await _unitOfWork.Repository<StockTransaction>().AddAsync(stockTx);

                        remainingQuantity -= quantityFromBatch;
                    }

                    if (remainingQuantity > 0)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return BadRequest(ApiResponse<object>.ErrorResult(
                            $"Không đủ tồn kho cho phụ tùng: {part?.PartName ?? $"ID {mrItem.PartId}"}. Cần thêm {remainingQuantity} đơn vị"));
                    }

                    // ✅ FIX: Cập nhật tổng số lượng giảm cho Part (giữ nguyên currentStockForTransactions đã được cập nhật trong loop batch)
                    var partUpdate = partsToUpdate[part.Id];
                    partsToUpdate[part.Id] = (partUpdate.part, partUpdate.totalQuantityToReduce + quantityToIssue, partUpdate.initialStock, partUpdate.currentStockForTransactions);

                    // Cập nhật QuantityIssued
                    mrItem.QuantityIssued = quantityToIssue;
                    await _unitOfWork.Repository<MaterialRequestItem>().UpdateAsync(mrItem);
                }

                // ✅ FIX: Validate có ít nhất một item được issue
                if (partsToUpdate.Values.All(p => p.totalQuantityToReduce == 0))
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return BadRequest(ApiResponse<object>.ErrorResult("Không có vật tư nào được xuất. Tất cả items đều có QuantityPicked = 0"));
                }

                // ✅ FIX: Update Part một lần cho mỗi Part (thay vì update nhiều lần trong loop)
                foreach (var kvp in partsToUpdate)
                {
                    var (part, totalReduce, initialStock, _) = kvp.Value;
                    if (totalReduce > 0) // Chỉ update nếu có giảm tồn kho
                    {
                        var newStock = initialStock - totalReduce;
                        // ✅ FIX: Validate để đảm bảo tồn kho không bị âm (defensive check)
                        if (newStock < 0)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return BadRequest(ApiResponse<object>.ErrorResult(
                                $"Lỗi tính toán tồn kho cho phụ tùng '{part?.PartName ?? $"ID {part.Id}"}'. Tồn kho sẽ bị âm ({initialStock} - {totalReduce} = {newStock}). Vui lòng kiểm tra lại dữ liệu."));
                        }
                        part.QuantityInStock = newStock;
                        await _unitOfWork.Repository<Part>().UpdateAsync(part);
                    }
                }

                // Cập nhật trạng thái MR
                mr.Status = MaterialRequestStatus.Issued;
                mr.IssuedAt = DateTime.Now;
                
                // ✅ Map current user từ Identity sang EmployeeId
                var currentEmployeeId = await GetCurrentEmployeeIdAsync();
                mr.IssuedById = currentEmployeeId ?? 0; // Default to 0 if not found (system user)
                
                await _unitOfWork.Repository<MaterialRequest>().UpdateAsync(mr);

                await _unitOfWork.SaveChangesAsync();

                // ✅ 3.1 Task 2.2: Tự động tính COGS cho ServiceOrder
                bool cogsCalculated = false;
                decimal cogsValue = serviceOrder.TotalCOGS; // Giữ giá trị hiện tại nếu tính COGS fail
                try
                {
                    var cogsMethod = string.IsNullOrWhiteSpace(serviceOrder.COGSCalculationMethod) 
                        ? "FIFO" 
                        : serviceOrder.COGSCalculationMethod;
                    
                    var cogsResult = await _cogsCalculationService.CalculateCOGSAsync(serviceOrder.Id, cogsMethod);
                    
                    // Lưu kết quả COGS vào ServiceOrder
                    serviceOrder.TotalCOGS = cogsResult.TotalCOGS;
                    serviceOrder.COGSCalculationMethod = cogsResult.CalculationMethod ?? "FIFO";
                    serviceOrder.COGSCalculationDate = cogsResult.CalculationDate;
                    serviceOrder.COGSBreakdown = cogsResult.BreakdownJson;
                    
                    await _unitOfWork.ServiceOrders.UpdateAsync(serviceOrder);
                    // ✅ FIX: Nếu SaveChangesAsync() fail ở đây, exception sẽ được catch và transaction vẫn được commit
                    // vì Issue MR đã thành công ở dòng 426. COGS có thể được tính lại sau.
                    await _unitOfWork.SaveChangesAsync();

                    cogsCalculated = true;
                    cogsValue = cogsResult.TotalCOGS;

                    _logger.LogInformation(
                        "Đã tự động tính COGS cho ServiceOrder {ServiceOrderId} sau khi Issue MR {MRId}. TotalCOGS: {TotalCOGS}",
                        serviceOrder.Id, mr.Id, cogsResult.TotalCOGS);
                }
                catch (Exception cogsEx)
                {
                    // Log lỗi nhưng không rollback transaction vì Issue MR đã thành công ở dòng 426
                    // COGS có thể được tính lại sau bằng cách gọi API calculate-cogs
                    _logger.LogError(cogsEx, 
                        "Lỗi khi tự động tính COGS cho ServiceOrder {ServiceOrderId} sau khi Issue MR {MRId}. Issue MR đã thành công, nhưng COGS chưa được lưu. Có thể tính lại sau.",
                        serviceOrder.Id, mr.Id);
                    // Tiếp tục commit transaction vì Issue MR đã thành công
                }

                await _unitOfWork.CommitTransactionAsync();

                // ✅ FIX: Response message chính xác dựa trên kết quả tính COGS
                var message = cogsCalculated 
                    ? $"Đã xuất vật tư cho JO. COGS đã được tự động tính: {cogsValue:N0} VNĐ"
                    : $"Đã xuất vật tư cho JO. Lưu ý: Không thể tự động tính COGS (giá trị hiện tại: {cogsValue:N0} VNĐ)";

                return Ok(ApiResponse<object>.SuccessResult(null, message));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Lỗi khi xuất vật tư cho MR {MRId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Lỗi khi xuất vật tư", ex.Message));
            }
        }
    }
}


