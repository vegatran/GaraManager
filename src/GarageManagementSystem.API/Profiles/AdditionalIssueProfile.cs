using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.API.Profiles
{
    public class AdditionalIssueProfile : Profile
    {
        public AdditionalIssueProfile()
        {
            // AdditionalIssue Entity to DTO mappings
            CreateMap<AdditionalIssue, AdditionalIssueDto>()
                .ForMember(dest => dest.ServiceOrder, opt => opt.MapFrom(src => src.ServiceOrder))
                .ForMember(dest => dest.ServiceOrderItem, opt => opt.MapFrom(src => src.ServiceOrderItem))
                .ForMember(dest => dest.ReportedByEmployee, opt => opt.MapFrom(src => src.ReportedByEmployee))
                .ForMember(dest => dest.AdditionalQuotation, opt => opt.MapFrom(src => src.AdditionalQuotation))
                .ForMember(dest => dest.Photos, opt => opt.MapFrom(src => src.Photos ?? new List<AdditionalIssuePhoto>()));

            // Create DTO to AdditionalIssue Entity
            CreateMap<CreateAdditionalIssueDto, AdditionalIssue>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceOrderId, opt => opt.MapFrom(src => src.ServiceOrderId))
                .ForMember(dest => dest.ServiceOrderItemId, opt => opt.MapFrom(src => src.ServiceOrderItemId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Identified"))
                .ForMember(dest => dest.ReportedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceOrder, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceOrderItem, opt => opt.Ignore())
                .ForMember(dest => dest.ReportedByEmployee, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalQuotation, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalServiceOrder, opt => opt.Ignore())
                .ForMember(dest => dest.Photos, opt => opt.Ignore());

            // Update DTO to AdditionalIssue Entity
            CreateMap<UpdateAdditionalIssueDto, AdditionalIssue>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ServiceOrderId, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceOrderItemId, opt => opt.Ignore())
                .ForMember(dest => dest.ReportedByEmployeeId, opt => opt.Ignore())
                .ForMember(dest => dest.ReportedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceOrder, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceOrderItem, opt => opt.Ignore())
                .ForMember(dest => dest.ReportedByEmployee, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalQuotation, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalServiceOrder, opt => opt.Ignore())
                .ForMember(dest => dest.Photos, opt => opt.Ignore());

            // AdditionalIssuePhoto mappings
            CreateMap<AdditionalIssuePhoto, AdditionalIssuePhotoDto>();
        }
    }
}

