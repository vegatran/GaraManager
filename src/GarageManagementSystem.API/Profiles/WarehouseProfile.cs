using System.Linq;
using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.API.Profiles
{
    public class WarehouseProfile : Profile
    {
        public WarehouseProfile()
        {
            // ✅ SỬA: Không filter lại trong AutoMapper vì Include đã filter rồi
            // Include(w => w.Zones.Where(z => !z.IsDeleted)) đã filter zones deleted
            // Nên chỉ cần map trực tiếp, không cần filter lại
            CreateMap<Warehouse, WarehouseDto>()
                .ForMember(dest => dest.Zones, opt => opt.MapFrom(src => src.Zones ?? new List<WarehouseZone>()))
                .ForMember(dest => dest.Bins, opt => opt.MapFrom(src => src.Bins ?? new List<WarehouseBin>()));

            CreateMap<WarehouseZone, WarehouseZoneDto>()
                .ForMember(dest => dest.Bins, opt => opt.MapFrom(src => src.Bins ?? new List<WarehouseBin>()));

            CreateMap<WarehouseBin, WarehouseBinDto>();

            CreateMap<CreateWarehouseDto, Warehouse>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Zones, opt => opt.Ignore())
                .ForMember(dest => dest.Bins, opt => opt.Ignore())
                .ForMember(dest => dest.InventoryBatches, opt => opt.Ignore());

            CreateMap<UpdateWarehouseDto, Warehouse>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Zones, opt => opt.Ignore())
                .ForMember(dest => dest.Bins, opt => opt.Ignore())
                .ForMember(dest => dest.InventoryBatches, opt => opt.Ignore());
        }
    }
}

