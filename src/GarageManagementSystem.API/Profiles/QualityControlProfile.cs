using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.API.Profiles
{
    /// <summary>
    /// ✅ 2.4: Quality Control AutoMapper Profile
    /// </summary>
    public class QualityControlProfile : Profile
    {
        public QualityControlProfile()
        {
            // QualityControl Entity to DTO
            CreateMap<QualityControl, QualityControlDto>()
                .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.ServiceOrder != null ? src.ServiceOrder.OrderNumber : string.Empty))
                .ForMember(dest => dest.QCInspectorName, opt => opt.MapFrom(src => src.QCInspector != null ? src.QCInspector.Name : null))
                .ForMember(dest => dest.QCChecklistItems, opt => opt.MapFrom(src => src.QCChecklistItems.Where(i => !i.IsDeleted)));

            // CreateQualityControlDto to QualityControl Entity
            CreateMap<CreateQualityControlDto, QualityControl>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceOrder, opt => opt.Ignore())
                .ForMember(dest => dest.QCInspector, opt => opt.Ignore())
                .ForMember(dest => dest.QCChecklistItems, opt => opt.Ignore())
                .ForMember(dest => dest.QCDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.QCResult, opt => opt.MapFrom(src => "Pending"));

            // QCChecklistItem Entity to DTO
            CreateMap<QCChecklistItem, QCChecklistItemDto>();

            // CreateQCChecklistItemDto to QCChecklistItem Entity
            CreateMap<CreateQCChecklistItemDto, QCChecklistItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.QualityControlId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.QualityControl, opt => opt.Ignore());
        }
    }
}

