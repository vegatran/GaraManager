using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.API.Profiles
{
    public class SupplierProfile : Profile
    {
        public SupplierProfile()
        {
            // Supplier Entity to DTO mappings
            CreateMap<Supplier, SupplierDto>()
                .ForMember(dest => dest.SupplierCode, opt => opt.MapFrom(src => src.SupplierCode))
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.SupplierName))
                .ForMember(dest => dest.ContactPerson, opt => opt.MapFrom(src => src.ContactPerson))
                .ForMember(dest => dest.ContactPhone, opt => opt.MapFrom(src => src.ContactPhone ?? src.Phone))
                .ForMember(dest => dest.TaxCode, opt => opt.MapFrom(src => src.TaxCode))
                .ForMember(dest => dest.BankAccount, opt => opt.MapFrom(src => src.BankAccount))
                .ForMember(dest => dest.BankName, opt => opt.MapFrom(src => src.BankName))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating));

            // Create DTO to Supplier Entity
            CreateMap<CreateSupplierDto, Supplier>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.StockTransactions, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseOrders, opt => opt.Ignore())
                .ForMember(dest => dest.PartSuppliers, opt => opt.Ignore())
                .ForMember(dest => dest.City, opt => opt.Ignore())
                .ForMember(dest => dest.Country, opt => opt.Ignore())
                .ForMember(dest => dest.Website, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentTerms, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryTerms, opt => opt.Ignore());

            // Update DTO to Supplier Entity
            CreateMap<UpdateSupplierDto, Supplier>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.StockTransactions, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseOrders, opt => opt.Ignore())
                .ForMember(dest => dest.PartSuppliers, opt => opt.Ignore())
                .ForMember(dest => dest.City, opt => opt.Ignore())
                .ForMember(dest => dest.Country, opt => opt.Ignore())
                .ForMember(dest => dest.Website, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentTerms, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryTerms, opt => opt.Ignore());
        }
    }
}
