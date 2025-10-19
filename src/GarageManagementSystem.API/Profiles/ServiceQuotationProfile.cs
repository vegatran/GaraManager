using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.API.Profiles
{
    public class ServiceQuotationProfile : Profile
    {
        public ServiceQuotationProfile()
        {
            // ServiceQuotation Entity to DTO mappings
            CreateMap<ServiceQuotation, ServiceQuotationDto>()
                .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Customer))
                .ForMember(dest => dest.Vehicle, opt => opt.MapFrom(src => src.Vehicle))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

            // Create DTO to ServiceQuotation Entity
            CreateMap<CreateServiceQuotationDto, ServiceQuotation>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicle, opt => opt.Ignore())
                .ForMember(dest => dest.Items, opt => opt.Ignore())
                .ForMember(dest => dest.QuotationNumber, opt => opt.Ignore())
                .ForMember(dest => dest.QuotationDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.ValidUntil, opt => opt.MapFrom(src => src.ValidUntil ?? DateTime.Now.AddDays(7)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Draft"));

            // Update DTO to ServiceQuotation Entity
            CreateMap<UpdateServiceQuotationDto, ServiceQuotation>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicle, opt => opt.Ignore())
                .ForMember(dest => dest.Items, opt => opt.Ignore())
                .ForMember(dest => dest.QuotationNumber, opt => opt.Ignore())
                .ForMember(dest => dest.QuotationDate, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Status)));

            // QuotationItem mappings
            CreateMap<QuotationItem, QuotationItemDto>()
                .ForMember(dest => dest.Service, opt => opt.MapFrom(src => src.Service))
                .ForMember(dest => dest.ServiceType, opt => opt.MapFrom(src => src.ItemType))
                .ForMember(dest => dest.HasInvoice, opt => opt.MapFrom(src => src.HasInvoice))
                .ForMember(dest => dest.IsVATApplicable, opt => opt.MapFrom(src => src.IsVATApplicable))
                .ForMember(dest => dest.VATRate, opt => opt.MapFrom(src => src.VATRate)) // ✅ THÊM: Map VATRate từ entity
                .ForMember(dest => dest.ItemCategory, opt => opt.MapFrom(src => src.ItemCategory));

            CreateMap<CreateQuotationItemDto, QuotationItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceQuotationId, opt => opt.Ignore())
                .ForMember(dest => dest.Service, opt => opt.Ignore())
                .ForMember(dest => dest.ItemType, opt => opt.MapFrom(src => src.ServiceType ?? "Service"))
                .ForMember(dest => dest.HasInvoice, opt => opt.MapFrom(src => src.HasInvoice))
                .ForMember(dest => dest.IsVATApplicable, opt => opt.MapFrom(src => src.IsVATApplicable)) // ✅ THÊM: Map IsVATApplicable
                .ForMember(dest => dest.VATRate, opt => opt.MapFrom(src => src.VATRate)) // ✅ THÊM: Map VATRate
                .ForMember(dest => dest.ItemCategory, opt => opt.MapFrom(src => src.ItemCategory ?? "Material"))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
        }
    }
}
