using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.API.Profiles
{
    public class VehicleInspectionProfile : Profile
    {
        public VehicleInspectionProfile()
        {
            // VehicleInspection Entity to DTO mappings
            CreateMap<VehicleInspection, VehicleInspectionDto>()
                .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Customer))
                .ForMember(dest => dest.Vehicle, opt => opt.MapFrom(src => src.Vehicle))
                .ForMember(dest => dest.Inspector, opt => opt.MapFrom(src => src.Inspector))
                .ForMember(dest => dest.Issues, opt => opt.MapFrom(src => src.Issues))
                .ForMember(dest => dest.Photos, opt => opt.MapFrom(src => src.Photos));

            // Create DTO to VehicleInspection Entity
            CreateMap<CreateVehicleInspectionDto, VehicleInspection>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicle, opt => opt.Ignore())
                .ForMember(dest => dest.Inspector, opt => opt.Ignore())
                .ForMember(dest => dest.Issues, opt => opt.Ignore())
                .ForMember(dest => dest.Photos, opt => opt.Ignore())
                .ForMember(dest => dest.InspectionNumber, opt => opt.Ignore())
                .ForMember(dest => dest.InspectionDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "InProgress"));

            // Update DTO to VehicleInspection Entity
            CreateMap<UpdateVehicleInspectionDto, VehicleInspection>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicle, opt => opt.Ignore())
                .ForMember(dest => dest.Inspector, opt => opt.Ignore())
                .ForMember(dest => dest.Issues, opt => opt.Ignore())
                .ForMember(dest => dest.Photos, opt => opt.Ignore())
                .ForMember(dest => dest.InspectionNumber, opt => opt.Ignore())
                .ForMember(dest => dest.InspectionDate, opt => opt.Ignore());

            // InspectionIssue mappings
            CreateMap<InspectionIssue, InspectionIssueDto>()
                .ForMember(dest => dest.SuggestedService, opt => opt.MapFrom(src => src.SuggestedService));

            CreateMap<CreateInspectionIssueDto, InspectionIssue>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.VehicleInspectionId, opt => opt.Ignore())
                .ForMember(dest => dest.SuggestedService, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // InspectionPhoto mappings
            CreateMap<InspectionPhoto, InspectionPhotoDto>();
        }
    }
}
