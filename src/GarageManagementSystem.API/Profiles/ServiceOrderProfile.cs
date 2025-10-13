using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.API.Profiles
{
    public class ServiceOrderProfile : Profile
    {
        public ServiceOrderProfile()
        {
            // ServiceOrder Entity to DTO mappings
            CreateMap<ServiceOrder, ServiceOrderDto>()
                .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Customer))
                .ForMember(dest => dest.Vehicle, opt => opt.MapFrom(src => src.Vehicle))
                .ForMember(dest => dest.ServiceOrderItems, opt => opt.MapFrom(src => src.ServiceOrderItems));

            // Create DTO to ServiceOrder Entity
            CreateMap<CreateServiceOrderDto, ServiceOrder>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicle, opt => opt.Ignore())
                .ForMember(dest => dest.PrimaryTechnician, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceOrderItems, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceOrderParts, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentTransactions, opt => opt.Ignore())
                .ForMember(dest => dest.VehicleInspection, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceQuotation, opt => opt.Ignore())
                .ForMember(dest => dest.OrderNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"))
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => DateTime.Now));

            // Update DTO to ServiceOrder Entity
            CreateMap<UpdateServiceOrderDto, ServiceOrder>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicle, opt => opt.Ignore())
                .ForMember(dest => dest.PrimaryTechnician, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceOrderItems, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceOrderParts, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentTransactions, opt => opt.Ignore())
                .ForMember(dest => dest.VehicleInspection, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceQuotation, opt => opt.Ignore())
                .ForMember(dest => dest.OrderNumber, opt => opt.Ignore())
                .ForMember(dest => dest.OrderDate, opt => opt.Ignore());
        }
    }
}
