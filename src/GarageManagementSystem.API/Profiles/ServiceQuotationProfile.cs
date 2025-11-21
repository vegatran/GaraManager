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
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.QuotationType, opt => opt.MapFrom(src => src.QuotationType));

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
                .ForMember(dest => dest.Items, opt => opt.Ignore()) // ✅ SỬA: Ignore Items vì sẽ xử lý riêng trong controller
                .ForMember(dest => dest.QuotationNumber, opt => opt.Ignore())
                .ForMember(dest => dest.QuotationDate, opt => opt.Ignore())
                .ForMember(dest => dest.QuotationType, opt => opt.Condition(src => !string.IsNullOrEmpty(src.QuotationType)))
                .ForMember(dest => dest.DiscountAmount, opt => opt.MapFrom(src => src.DiscountAmount))
                .ForMember(dest => dest.Status, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Status)));

            // QuotationItem mappings
            CreateMap<QuotationItem, QuotationItemDto>()
                .ForMember(dest => dest.Service, opt => opt.MapFrom(src => src.Service))
                // ✅ FIX: Map ItemType sang ServiceType với format đúng (parts, repair, paint)
                // ItemType = "Part" -> ServiceType = "parts"
                // ItemType = "Service" -> ServiceType = "repair" (mặc định) hoặc "paint" nếu Service.LaborType = "Sơn"
                .ForMember(dest => dest.ServiceType, opt => opt.MapFrom(src => 
                    src.ItemType == "Part" ? "parts" :
                    src.ItemType == "Service" && src.Service != null && src.Service.LaborType != null && src.Service.LaborType.ToLower().Contains("sơn") ? "paint" :
                    src.ItemType == "Service" ? "repair" :
                    string.IsNullOrEmpty(src.ItemType) ? "repair" : src.ItemType.ToLower()))
                .ForMember(dest => dest.HasInvoice, opt => opt.MapFrom(src => src.HasInvoice))
                .ForMember(dest => dest.IsVATApplicable, opt => opt.MapFrom(src => src.IsVATApplicable))
                .ForMember(dest => dest.VATRate, opt => opt.MapFrom(src => src.VATRate)) // ✅ THÊM: Map VATRate từ entity
                .ForMember(dest => dest.OverrideVATRate, opt => opt.MapFrom(src => src.OverrideVATRate)) // ✅ THÊM: Map OverrideVATRate
                .ForMember(dest => dest.OverrideIsVATApplicable, opt => opt.MapFrom(src => src.OverrideIsVATApplicable)) // ✅ THÊM: Map OverrideIsVATApplicable
                .ForMember(dest => dest.PartVATRate, opt => opt.MapFrom(src => src.Part != null ? src.Part.VATRate : (decimal?)null)) // ✅ THÊM: Map VATRate từ Part
                .ForMember(dest => dest.PartIsVATApplicable, opt => opt.MapFrom(src => src.Part != null ? src.Part.IsVATApplicable : (bool?)null)) // ✅ THÊM: Map IsVATApplicable từ Part
                .ForMember(dest => dest.ItemCategory, opt => opt.MapFrom(src => src.ItemCategory))
                .ForMember(dest => dest.PartId, opt => opt.MapFrom(src => src.PartId)); // ✅ THÊM: Map PartId

            CreateMap<CreateQuotationItemDto, QuotationItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceQuotationId, opt => opt.Ignore())
                .ForMember(dest => dest.Service, opt => opt.Ignore())
                // ✅ FIX: Map ServiceType (parts, repair, paint) sang ItemType (Part, Service)
                .ForMember(dest => dest.ItemType, opt => opt.MapFrom(src => 
                    src.ServiceType == "parts" ? "Part" :
                    src.ServiceType == "repair" || src.ServiceType == "paint" ? "Service" :
                    string.IsNullOrEmpty(src.ServiceType) ? "Service" : src.ServiceType))
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
