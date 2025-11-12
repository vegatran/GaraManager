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
                .ForMember(dest => dest.ServiceOrderItems, opt => opt.MapFrom(src => src.ServiceOrderItems))
                .ForMember(dest => dest.Warranties, opt => opt.MapFrom(src => src.Warranties.Where(w => !w.IsDeleted)))
                .ForMember(dest => dest.ServiceOrderFees, opt => opt.MapFrom(src => src.ServiceOrderFees.Where(f => !f.IsDeleted)));

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

            // ServiceOrderItem mappings
            CreateMap<ServiceOrderItem, ServiceOrderItemDto>()
                .ForMember(dest => dest.Service, opt => opt.MapFrom(src => src.Service))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.ServiceName) ? src.ServiceName : (src.Service != null ? src.Service.Name : ""))) // ✅ THÊM: Map ServiceName
                .ForMember(dest => dest.ServiceId, opt => opt.MapFrom(src => src.ServiceId)) // ✅ THÊM: Map ServiceId (nullable)
                .ForMember(dest => dest.AssignedTechnicianName, opt => opt.MapFrom(src => src.AssignedTechnician != null ? src.AssignedTechnician.Name : null))
                // ✅ 2.3.1: Map StartTime, EndTime, ActualHours, CompletedTime
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime))
                .ForMember(dest => dest.ActualHours, opt => opt.MapFrom(src => src.ActualHours))
                .ForMember(dest => dest.CompletedTime, opt => opt.MapFrom(src => src.CompletedTime))
                // ✅ 2.4.3: Map ReworkHours
                .ForMember(dest => dest.ReworkHours, opt => opt.MapFrom(src => src.ReworkHours));

            CreateMap<CreateServiceOrderItemDto, ServiceOrderItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceOrderId, opt => opt.Ignore())
                .ForMember(dest => dest.Service, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
        }
    }
}
