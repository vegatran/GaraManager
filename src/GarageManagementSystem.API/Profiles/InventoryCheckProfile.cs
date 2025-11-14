using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.API.Profiles
{
    public class InventoryCheckProfile : Profile
    {
        public InventoryCheckProfile()
        {
            // ✅ SỬA: Không filter lại trong AutoMapper vì Include đã filter rồi
            // Include(w => w.Items.Where(i => !i.IsDeleted)) đã filter items deleted
            // Nên chỉ cần map trực tiếp, không cần filter lại

            // InventoryCheck -> InventoryCheckDto
            CreateMap<InventoryCheck, InventoryCheckDto>()
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : null))
                .ForMember(dest => dest.WarehouseZoneName, opt => opt.MapFrom(src => src.WarehouseZone != null ? src.WarehouseZone.Name : null))
                .ForMember(dest => dest.WarehouseBinName, opt => opt.MapFrom(src => src.WarehouseBin != null ? src.WarehouseBin.Name : null))
                .ForMember(dest => dest.StartedByEmployeeName, opt => opt.MapFrom(src => src.StartedByEmployee != null ? src.StartedByEmployee.Name : null))
                .ForMember(dest => dest.CompletedByEmployeeName, opt => opt.MapFrom(src => src.CompletedByEmployee != null ? src.CompletedByEmployee.Name : null))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items ?? new List<InventoryCheckItem>()));

            // InventoryCheckItem -> InventoryCheckItemDto
            CreateMap<InventoryCheckItem, InventoryCheckItemDto>()
                .ForMember(dest => dest.PartNumber, opt => opt.MapFrom(src => src.Part != null ? src.Part.PartNumber : null))
                .ForMember(dest => dest.PartName, opt => opt.MapFrom(src => src.Part != null ? src.Part.PartName : null))
                .ForMember(dest => dest.PartSku, opt => opt.MapFrom(src => src.Part != null ? src.Part.Sku : null))
                .ForMember(dest => dest.PartBarcode, opt => opt.MapFrom(src => src.Part != null ? src.Part.Barcode : null));

            // CreateInventoryCheckDto -> InventoryCheck
            CreateMap<CreateInventoryCheckDto, InventoryCheck>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Warehouse, opt => opt.Ignore())
                .ForMember(dest => dest.WarehouseZone, opt => opt.Ignore())
                .ForMember(dest => dest.WarehouseBin, opt => opt.Ignore())
                .ForMember(dest => dest.StartedByEmployee, opt => opt.Ignore())
                .ForMember(dest => dest.CompletedByEmployee, opt => opt.Ignore())
                .ForMember(dest => dest.CompletedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CompletedByEmployeeId, opt => opt.Ignore())
                .ForMember(dest => dest.Items, opt => opt.Ignore()); // Items sẽ được xử lý riêng

            // UpdateInventoryCheckDto -> InventoryCheck
            CreateMap<UpdateInventoryCheckDto, InventoryCheck>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Warehouse, opt => opt.Ignore())
                .ForMember(dest => dest.WarehouseZone, opt => opt.Ignore())
                .ForMember(dest => dest.WarehouseBin, opt => opt.Ignore())
                .ForMember(dest => dest.StartedByEmployee, opt => opt.Ignore())
                .ForMember(dest => dest.CompletedByEmployee, opt => opt.Ignore())
                .ForMember(dest => dest.Items, opt => opt.Ignore()); // Items sẽ được xử lý riêng

            // CreateInventoryCheckItemDto -> InventoryCheckItem
            CreateMap<CreateInventoryCheckItemDto, InventoryCheckItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.InventoryCheckId, opt => opt.Ignore())
                .ForMember(dest => dest.DiscrepancyQuantity, opt => opt.MapFrom(src => src.ActualQuantity - src.SystemQuantity))
                .ForMember(dest => dest.IsDiscrepancy, opt => opt.MapFrom(src => src.ActualQuantity != src.SystemQuantity))
                .ForMember(dest => dest.IsAdjusted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.InventoryCheck, opt => opt.Ignore())
                .ForMember(dest => dest.Part, opt => opt.Ignore());

            // UpdateInventoryCheckItemDto -> InventoryCheckItem
            CreateMap<UpdateInventoryCheckItemDto, InventoryCheckItem>()
                .ForMember(dest => dest.InventoryCheckId, opt => opt.Ignore())
                .ForMember(dest => dest.DiscrepancyQuantity, opt => opt.MapFrom(src => src.ActualQuantity - src.SystemQuantity))
                .ForMember(dest => dest.IsDiscrepancy, opt => opt.MapFrom(src => src.ActualQuantity != src.SystemQuantity))
                .ForMember(dest => dest.IsAdjusted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.InventoryCheck, opt => opt.Ignore())
                .ForMember(dest => dest.Part, opt => opt.Ignore());
        }
    }
}

