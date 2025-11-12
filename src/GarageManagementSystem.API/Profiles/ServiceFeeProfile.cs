using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.API.Profiles
{
    public class ServiceFeeProfile : Profile
    {
        public ServiceFeeProfile()
        {
            CreateMap<ServiceFeeType, ServiceFeeTypeDto>();

            CreateMap<ServiceOrderFee, ServiceOrderFeeDto>()
                .ForMember(dest => dest.ServiceFeeTypeName, opt => opt.MapFrom(src => src.ServiceFeeType != null ? src.ServiceFeeType.Name : string.Empty));

            CreateMap<UpsertServiceOrderFeeDto, ServiceOrderFee>()
                .ForMember(dest => dest.Id, opt => opt.Condition(src => src.Id.HasValue))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id ?? 0))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceFeeType, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceOrder, opt => opt.Ignore());
        }
    }
}

