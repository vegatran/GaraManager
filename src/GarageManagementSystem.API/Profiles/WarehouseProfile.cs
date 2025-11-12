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
            CreateMap<Warehouse, WarehouseDto>()
                .ForMember(dest => dest.Zones, opt => opt.MapFrom(src => src.Zones.Where(z => !z.IsDeleted)))
                .ForMember(dest => dest.Bins, opt => opt.MapFrom(src => src.Bins.Where(b => !b.IsDeleted)));

            CreateMap<WarehouseZone, WarehouseZoneDto>()
                .ForMember(dest => dest.Bins, opt => opt.MapFrom(src => src.Bins.Where(b => !b.IsDeleted)));

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

