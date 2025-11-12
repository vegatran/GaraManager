using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiScope")]
    public class WarehousesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly GarageDbContext _context;

        public WarehousesController(IUnitOfWork unitOfWork, IMapper mapper, GarageDbContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<WarehouseDto>>>> GetWarehouses()
        {
            var warehouses = await _context.Warehouses
                .Where(w => !w.IsDeleted)
                .Include(w => w.Zones.Where(z => !z.IsDeleted))
                    .ThenInclude(z => z.Bins.Where(b => !b.IsDeleted))
                .Include(w => w.Bins.Where(b => !b.IsDeleted && b.WarehouseZoneId == null))
                .ToListAsync();

            var dto = _mapper.Map<List<WarehouseDto>>(warehouses);
            return Ok(ApiResponse<List<WarehouseDto>>.SuccessResult(dto));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<WarehouseDto>>> GetWarehouse(int id)
        {
            var warehouse = await _context.Warehouses
                .Where(w => !w.IsDeleted && w.Id == id)
                .Include(w => w.Zones.Where(z => !z.IsDeleted))
                    .ThenInclude(z => z.Bins.Where(b => !b.IsDeleted))
                .Include(w => w.Bins.Where(b => !b.IsDeleted && b.WarehouseZoneId == null))
                .FirstOrDefaultAsync();

            if (warehouse == null)
            {
                return NotFound(ApiResponse<WarehouseDto>.ErrorResult("Warehouse not found"));
            }

            return Ok(ApiResponse<WarehouseDto>.SuccessResult(_mapper.Map<WarehouseDto>(warehouse)));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<WarehouseDto>>> CreateWarehouse(CreateWarehouseDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<WarehouseDto>.ErrorResult("Invalid data"));
            }

            var entity = _mapper.Map<Warehouse>(dto);
            await _unitOfWork.Warehouses.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetWarehouse), new { id = entity.Id }, ApiResponse<WarehouseDto>.SuccessResult(_mapper.Map<WarehouseDto>(entity)));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<WarehouseDto>>> UpdateWarehouse(int id, UpdateWarehouseDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(ApiResponse<WarehouseDto>.ErrorResult("ID mismatch"));
            }

            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id);
            if (warehouse == null)
            {
                return NotFound(ApiResponse<WarehouseDto>.ErrorResult("Warehouse not found"));
            }

            _mapper.Map(dto, warehouse);
            await _unitOfWork.Warehouses.UpdateAsync(warehouse);
            await _unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse<WarehouseDto>.SuccessResult(_mapper.Map<WarehouseDto>(warehouse)));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteWarehouse(int id)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id);
            if (warehouse == null)
            {
                return NotFound(ApiResponse.ErrorResult("Warehouse not found"));
            }

            await _unitOfWork.Warehouses.DeleteAsync(warehouse);
            await _unitOfWork.SaveChangesAsync();
            return Ok(ApiResponse.SuccessResult("Warehouse deleted"));
        }

        [HttpPost("{warehouseId}/zones")]
        public async Task<ActionResult<ApiResponse<WarehouseZoneDto>>> CreateZone(int warehouseId, WarehouseZoneRequestDto dto)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(warehouseId);
            if (warehouse == null)
            {
                return NotFound(ApiResponse<WarehouseZoneDto>.ErrorResult("Warehouse not found"));
            }

            var zone = new WarehouseZone
            {
                WarehouseId = warehouseId,
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description,
                DisplayOrder = dto.DisplayOrder,
                IsActive = dto.IsActive
            };

            await _unitOfWork.WarehouseZones.AddAsync(zone);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetWarehouse), new { id = warehouseId }, ApiResponse<WarehouseZoneDto>.SuccessResult(_mapper.Map<WarehouseZoneDto>(zone)));
        }

        [HttpPut("{warehouseId}/zones/{zoneId}")]
        public async Task<ActionResult<ApiResponse<WarehouseZoneDto>>> UpdateZone(int warehouseId, int zoneId, WarehouseZoneRequestDto dto)
        {
            var zone = await _unitOfWork.WarehouseZones.GetByIdAsync(zoneId);
            if (zone == null || zone.WarehouseId != warehouseId)
            {
                return NotFound(ApiResponse<WarehouseZoneDto>.ErrorResult("Zone not found"));
            }

            zone.Code = dto.Code;
            zone.Name = dto.Name;
            zone.Description = dto.Description;
            zone.DisplayOrder = dto.DisplayOrder;
            zone.IsActive = dto.IsActive;

            await _unitOfWork.WarehouseZones.UpdateAsync(zone);
            await _unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse<WarehouseZoneDto>.SuccessResult(_mapper.Map<WarehouseZoneDto>(zone)));
        }

        [HttpDelete("{warehouseId}/zones/{zoneId}")]
        public async Task<ActionResult<ApiResponse>> DeleteZone(int warehouseId, int zoneId)
        {
            var zone = await _unitOfWork.WarehouseZones.GetByIdAsync(zoneId);
            if (zone == null || zone.WarehouseId != warehouseId)
            {
                return NotFound(ApiResponse.ErrorResult("Zone not found"));
            }

            await _unitOfWork.WarehouseZones.DeleteAsync(zone);
            await _unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse.SuccessResult("Zone deleted"));
        }

        [HttpPost("{warehouseId}/bins")]
        public async Task<ActionResult<ApiResponse<WarehouseBinDto>>> CreateBin(int warehouseId, WarehouseBinRequestDto dto)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(warehouseId);
            if (warehouse == null)
            {
                return NotFound(ApiResponse<WarehouseBinDto>.ErrorResult("Warehouse not found"));
            }

            if (dto.WarehouseZoneId.HasValue)
            {
                var zone = await _unitOfWork.WarehouseZones.GetByIdAsync(dto.WarehouseZoneId.Value);
                if (zone == null || zone.WarehouseId != warehouseId)
                {
                    return BadRequest(ApiResponse<WarehouseBinDto>.ErrorResult("Zone does not belong to warehouse"));
                }
            }

            var bin = new WarehouseBin
            {
                WarehouseId = warehouseId,
                WarehouseZoneId = dto.WarehouseZoneId,
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description,
                Capacity = dto.Capacity,
                IsDefault = dto.IsDefault,
                IsActive = dto.IsActive
            };

            await _unitOfWork.WarehouseBins.AddAsync(bin);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetWarehouse), new { id = warehouseId }, ApiResponse<WarehouseBinDto>.SuccessResult(_mapper.Map<WarehouseBinDto>(bin)));
        }

        [HttpPut("{warehouseId}/bins/{binId}")]
        public async Task<ActionResult<ApiResponse<WarehouseBinDto>>> UpdateBin(int warehouseId, int binId, WarehouseBinRequestDto dto)
        {
            var bin = await _unitOfWork.WarehouseBins.GetByIdAsync(binId);
            if (bin == null || bin.WarehouseId != warehouseId)
            {
                return NotFound(ApiResponse<WarehouseBinDto>.ErrorResult("Bin not found"));
            }

            if (dto.WarehouseZoneId.HasValue)
            {
                var zone = await _unitOfWork.WarehouseZones.GetByIdAsync(dto.WarehouseZoneId.Value);
                if (zone == null || zone.WarehouseId != warehouseId)
                {
                    return BadRequest(ApiResponse<WarehouseBinDto>.ErrorResult("Zone does not belong to warehouse"));
                }
            }

            bin.WarehouseZoneId = dto.WarehouseZoneId;
            bin.Code = dto.Code;
            bin.Name = dto.Name;
            bin.Description = dto.Description;
            bin.Capacity = dto.Capacity;
            bin.IsDefault = dto.IsDefault;
            bin.IsActive = dto.IsActive;

            await _unitOfWork.WarehouseBins.UpdateAsync(bin);
            await _unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse<WarehouseBinDto>.SuccessResult(_mapper.Map<WarehouseBinDto>(bin)));
        }

        [HttpDelete("{warehouseId}/bins/{binId}")]
        public async Task<ActionResult<ApiResponse>> DeleteBin(int warehouseId, int binId)
        {
            var bin = await _unitOfWork.WarehouseBins.GetByIdAsync(binId);
            if (bin == null || bin.WarehouseId != warehouseId)
            {
                return NotFound(ApiResponse.ErrorResult("Bin not found"));
            }

            await _unitOfWork.WarehouseBins.DeleteAsync(bin);
            await _unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse.SuccessResult("Bin deleted"));
        }
    }
}

