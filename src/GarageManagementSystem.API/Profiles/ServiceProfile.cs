using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.API.Profiles
{
    public class ServiceProfile : Profile
    {
        public ServiceProfile()
        {
            // Service Entity to DTO mappings
            CreateMap<Service, ServiceDto>();

            // Create DTO to Service Entity
            CreateMap<CreateServiceDto, Service>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceOrderItems, opt => opt.Ignore())
                .ForMember(dest => dest.RelatedInspectionIssues, opt => opt.Ignore())
                .ForMember(dest => dest.QuotationItems, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceType, opt => opt.Ignore());

            // Update DTO to Service Entity
            CreateMap<UpdateServiceDto, Service>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceOrderItems, opt => opt.Ignore())
                .ForMember(dest => dest.RelatedInspectionIssues, opt => opt.Ignore())
                .ForMember(dest => dest.QuotationItems, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceType, opt => opt.Ignore());
        }
    }
}
