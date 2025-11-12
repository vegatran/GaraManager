using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace GarageManagementSystem.Infrastructure.Services
{
    public class WarrantyService : IWarrantyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly GarageDbContext _context;
        private readonly ILogger<WarrantyService> _logger;

        public WarrantyService(
            IUnitOfWork unitOfWork,
            GarageDbContext context,
            ILogger<WarrantyService> logger)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _logger = logger;
        }

        public async Task<Warranty> GenerateWarrantyForServiceOrderAsync(int serviceOrderId, WarrantyGenerationOptions options, CancellationToken cancellationToken = default)
        {
            options ??= new WarrantyGenerationOptions();
            try
            {
                var existingWarranty = await _unitOfWork.Warranties.GetByServiceOrderIdAsync(serviceOrderId);
                if (existingWarranty != null && !options.ForceRegenerate)
                {
                    return existingWarranty;
                }

                var order = await _unitOfWork.ServiceOrders.GetByIdWithDetailsAsync(serviceOrderId);
                if (order == null)
                {
                    throw new InvalidOperationException($"Không tìm thấy JO với ID {serviceOrderId}");
                }

                var customer = await _unitOfWork.Customers.GetByIdAsync(order.CustomerId)
                    ?? throw new InvalidOperationException("Không tìm thấy khách hàng của JO");
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(order.VehicleId)
                    ?? throw new InvalidOperationException("Không tìm thấy xe của JO");

                var warrantyStart = options.WarrantyStartDate ?? order.HandoverDate ?? DateTime.Now;
                var warrantyCode = existingWarranty?.WarrantyCode;
                if (string.IsNullOrWhiteSpace(warrantyCode) || options.ForceRegenerate)
                {
                    warrantyCode = await GenerateWarrantyCodeAsync(serviceOrderId, cancellationToken);
                }

                Warranty warranty;
                if (existingWarranty == null)
                {
                    warranty = new Warranty
                    {
                        WarrantyCode = warrantyCode,
                        ServiceOrderId = order.Id,
                        CustomerId = customer.Id,
                        VehicleId = vehicle.Id,
                        WarrantyStartDate = warrantyStart,
                        WarrantyEndDate = warrantyStart,
                        Status = "Active",
                        HandoverBy = options.HandoverBy,
                        HandoverLocation = options.HandoverLocation
                    };

                    await _unitOfWork.Warranties.AddAsync(warranty);
                }
                else
                {
                    warranty = existingWarranty;
                    warranty.WarrantyCode = warrantyCode;
                    warranty.CustomerId = customer.Id;
                    warranty.VehicleId = vehicle.Id;
                    warranty.WarrantyStartDate = warrantyStart;
                    warranty.HandoverBy = options.HandoverBy ?? warranty.HandoverBy;
                    warranty.HandoverLocation = options.HandoverLocation ?? warranty.HandoverLocation;

                    // Xóa các warranty item cũ (soft delete)
                    var existingItems = await _unitOfWork.WarrantyItems.FindAsync(i => i.WarrantyId == warranty.Id);
                    foreach (var item in existingItems)
                    {
                        await _unitOfWork.WarrantyItems.DeleteAsync(item);
                    }
                }

                var warrantyItems = new List<WarrantyItem>();
                var maxEndDate = warrantyStart;

                // Chuẩn bị dictionary PartId -> Part để tránh query nhiều lần
                var partIds = order.ServiceOrderParts
                    .Where(p => !p.IsDeleted)
                    .Select(p => p.PartId)
                    .Distinct()
                    .ToList();

                var partDictionary = await _context.Parts
                    .Where(p => partIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id, cancellationToken);

                foreach (var partUsage in order.ServiceOrderParts.Where(p => !p.IsDeleted))
                {
                    Part? part = null;
                    partDictionary.TryGetValue(partUsage.PartId, out part);

                    int warrantyMonths = 0;
                    if (partUsage.WarrantyUntil.HasValue)
                    {
                        warrantyMonths = Math.Max(0, (int)Math.Round((partUsage.WarrantyUntil.Value - warrantyStart).TotalDays / 30.0));
                    }

                    if (warrantyMonths <= 0 && part != null)
                    {
                        warrantyMonths = part.WarrantyMonths;
                    }

                    if (warrantyMonths <= 0)
                    {
                        warrantyMonths = options.DefaultWarrantyMonths;
                    }

                    if (warrantyMonths <= 0)
                    {
                        continue;
                    }

                    var itemStart = warrantyStart;
                    var itemEnd = itemStart.AddMonths(warrantyMonths);
                    if (itemEnd > maxEndDate)
                    {
                        maxEndDate = itemEnd;
                    }

                    partUsage.IsWarranty = true;
                    partUsage.WarrantyUntil = itemEnd;

                    var warrantyItem = new WarrantyItem
                    {
                        Warranty = warranty,
                        ServiceOrderPartId = partUsage.Id,
                        PartId = part?.Id,
                        PartName = partUsage.PartName ?? part?.PartName ?? string.Empty,
                        PartNumber = part?.PartNumber,
                        WarrantyMonths = warrantyMonths,
                        WarrantyStartDate = itemStart,
                        WarrantyEndDate = itemEnd,
                        Status = "Active"
                    };

                    warrantyItems.Add(warrantyItem);
                }

                if (warrantyItems.Count == 0)
                {
                    // Nếu không có vật tư nào có bảo hành, dùng default cho toàn bộ JO
                    var months = options.DefaultWarrantyMonths > 0 ? options.DefaultWarrantyMonths : 0;
                    if (months > 0)
                    {
                        maxEndDate = warrantyStart.AddMonths(months);
                    }
                    else
                    {
                        maxEndDate = warrantyStart;
                    }
                }

                warranty.WarrantyEndDate = maxEndDate;
                warranty.Items = warrantyItems;

                order.WarrantyCode = warranty.WarrantyCode;
                order.WarrantyExpiryDate = warranty.WarrantyEndDate;

                await _unitOfWork.ServiceOrders.UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync();

                return warranty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating warranty for service order {ServiceOrderId}", serviceOrderId);
                throw;
            }
        }

        public async Task<Warranty?> GetWarrantyByServiceOrderAsync(int serviceOrderId, CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.Warranties.GetByServiceOrderIdAsync(serviceOrderId);
        }

        public async Task<Warranty?> GetWarrantyByCodeAsync(string warrantyCode, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(warrantyCode))
            {
                return null;
            }

            return await _unitOfWork.Warranties.GetByCodeAsync(warrantyCode);
        }

        public async Task<IEnumerable<Warranty>> SearchWarrantiesAsync(WarrantySearchFilter filter, CancellationToken cancellationToken = default)
        {
            filter ??= new WarrantySearchFilter();

            var query = _context.Warranties.AsQueryable();

            if (filter.CustomerId.HasValue)
            {
                query = query.Where(w => w.CustomerId == filter.CustomerId.Value);
            }

            if (filter.VehicleId.HasValue)
            {
                query = query.Where(w => w.VehicleId == filter.VehicleId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(w => w.Status == filter.Status);
            }

            if (filter.StartDate.HasValue)
            {
                query = query.Where(w => w.WarrantyStartDate >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(w => w.WarrantyEndDate <= filter.EndDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.ToLower();
                query = query.Where(w =>
                    w.WarrantyCode.ToLower().Contains(keyword) ||
                    w.Customer.Name.ToLower().Contains(keyword) ||
                    w.Vehicle.LicensePlate.ToLower().Contains(keyword));
            }

            return await query
                .Where(w => !w.IsDeleted)
                .Include(w => w.Customer)
                .Include(w => w.Vehicle)
                .Include(w => w.ServiceOrder)
                .OrderByDescending(w => w.WarrantyStartDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<WarrantyClaim> CreateWarrantyClaimAsync(int warrantyId, WarrantyClaimCreateRequest request, CancellationToken cancellationToken = default)
        {
            var warranty = await _unitOfWork.Warranties.GetByIdAsync(warrantyId);
            if (warranty == null)
            {
                throw new InvalidOperationException("Không tìm thấy thông tin bảo hành");
            }

            var claimNumber = request.ClaimNumber;
            if (string.IsNullOrWhiteSpace(claimNumber))
            {
                claimNumber = await GenerateWarrantyClaimNumberAsync(warrantyId, cancellationToken);
            }

            var claim = new WarrantyClaim
            {
                WarrantyId = warrantyId,
                ClaimNumber = claimNumber,
                ClaimDate = request.ClaimDate ?? DateTime.Now,
                ServiceOrderId = request.ServiceOrderId,
                CustomerId = request.CustomerId ?? warranty.CustomerId,
                VehicleId = request.VehicleId ?? warranty.VehicleId,
                IssueDescription = request.IssueDescription,
                Status = "Pending",
                Notes = request.Notes
            };

            await _unitOfWork.WarrantyClaims.AddAsync(claim);
            await _unitOfWork.SaveChangesAsync();

            return claim;
        }

        public async Task<WarrantyClaim?> UpdateWarrantyClaimStatusAsync(int claimId, WarrantyClaimUpdateRequest request, CancellationToken cancellationToken = default)
        {
            var claim = await _unitOfWork.WarrantyClaims.GetByIdAsync(claimId);
            if (claim == null)
            {
                return null;
            }

            claim.Status = request.Status;
            claim.Resolution = request.Resolution;
            claim.ResolvedDate = request.ResolvedDate;
            claim.Notes = request.Notes ?? claim.Notes;

            await _unitOfWork.WarrantyClaims.UpdateAsync(claim);
            await _unitOfWork.SaveChangesAsync();

            return claim;
        }

        private async Task<string> GenerateWarrantyCodeAsync(int serviceOrderId, CancellationToken cancellationToken)
        {
            var prefix = $"WAR{DateTime.UtcNow:yyyyMMdd}";
            var sequence = 1;

            while (true)
            {
                var code = $"{prefix}-{serviceOrderId:D6}-{sequence:D2}";
                var exists = await _context.Warranties.AnyAsync(w => w.WarrantyCode == code, cancellationToken);
                if (!exists)
                {
                    return code;
                }
                sequence++;
            }
        }

        private async Task<string> GenerateWarrantyClaimNumberAsync(int warrantyId, CancellationToken cancellationToken)
        {
            var prefix = $"CLM{DateTime.UtcNow:yyyyMMdd}";
            var sequence = 1;

            while (true)
            {
                var code = $"{prefix}-{warrantyId:D6}-{sequence:D2}";
                var exists = await _context.WarrantyClaims.AnyAsync(c => c.ClaimNumber == code, cancellationToken);
                if (!exists)
                {
                    return code;
                }
                sequence++;
            }
        }
    }
}

