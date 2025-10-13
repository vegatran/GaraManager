using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.API.Profiles
{
    public class VehicleProfile : Profile
    {
        public VehicleProfile()
        {
            // Vehicle Entity to DTO mappings
            CreateMap<Vehicle, VehicleDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : null))
                .ForMember(dest => dest.Customer, opt => opt.Ignore()); // Ignore to avoid circular reference

            // Create DTO to Vehicle Entity
            CreateMap<CreateVehicleDto, Vehicle>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceOrders, opt => opt.Ignore())
                .ForMember(dest => dest.Inspections, opt => opt.Ignore())
                .ForMember(dest => dest.Quotations, opt => opt.Ignore())
                .ForMember(dest => dest.Appointments, opt => opt.Ignore());

            // Update DTO to Vehicle Entity
            CreateMap<UpdateVehicleDto, Vehicle>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceOrders, opt => opt.Ignore())
                .ForMember(dest => dest.Inspections, opt => opt.Ignore())
                .ForMember(dest => dest.Quotations, opt => opt.Ignore())
                .ForMember(dest => dest.Appointments, opt => opt.Ignore());
        }
    }
}
