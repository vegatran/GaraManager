using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.API.Profiles
{
    public class PartProfile : Profile
    {
        public PartProfile()
        {
            // Part Entity to DTO mappings
            CreateMap<Part, PartDto>();

            // Create DTO to Part Entity
            CreateMap<CreatePartDto, Part>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.StockTransactions, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceOrderParts, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceOrderParts, opt => opt.Ignore())
                .ForMember(dest => dest.QuotationItems, opt => opt.Ignore())
                .ForMember(dest => dest.StockTransactions, opt => opt.Ignore())
                .ForMember(dest => dest.PartGroup, opt => opt.Ignore());

            // Update DTO to Part Entity
            CreateMap<UpdatePartDto, Part>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.StockTransactions, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceOrderParts, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceOrderParts, opt => opt.Ignore())
                .ForMember(dest => dest.QuotationItems, opt => opt.Ignore())
                .ForMember(dest => dest.StockTransactions, opt => opt.Ignore())
                .ForMember(dest => dest.PartGroup, opt => opt.Ignore());
        }
    }
}
