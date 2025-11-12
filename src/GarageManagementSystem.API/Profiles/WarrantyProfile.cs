using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Shared.DTOs;
using System.Linq;

namespace GarageManagementSystem.API.Profiles
{
    public class WarrantyProfile : Profile
    {
        public WarrantyProfile()
        {
            CreateMap<Warranty, WarrantyDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : null))
                .ForMember(dest => dest.VehicleLicensePlate, opt => opt.MapFrom(src => src.Vehicle != null ? src.Vehicle.LicensePlate : null))
                .ForMember(dest => dest.ServiceOrderNumber, opt => opt.MapFrom(src => src.ServiceOrder != null ? src.ServiceOrder.OrderNumber : null));

            CreateMap<Warranty, WarrantySummaryDto>()
                .ForMember(dest => dest.ServiceOrderNumber, opt => opt.MapFrom(src => src.ServiceOrder != null ? src.ServiceOrder.OrderNumber : null))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : null))
                .ForMember(dest => dest.VehicleLicensePlate, opt => opt.MapFrom(src => src.Vehicle != null ? src.Vehicle.LicensePlate : null))
                .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count(i => !i.IsDeleted)))
                .ForMember(dest => dest.ClaimCount, opt => opt.MapFrom(src => src.Claims.Count(c => !c.IsDeleted)));

            CreateMap<WarrantyItem, WarrantyItemDto>()
                .ForMember(dest => dest.PartNumber, opt => opt.MapFrom(src => src.Part != null ? src.Part.PartNumber : src.PartNumber))
                .ForMember(dest => dest.PartName, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.PartName) ? src.PartName : (src.Part != null ? src.Part.PartName : string.Empty)));

            CreateMap<WarrantyClaim, WarrantyClaimDto>();

            CreateMap<CreateWarrantyClaimDto, WarrantyClaim>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"));
        }
    }
}

