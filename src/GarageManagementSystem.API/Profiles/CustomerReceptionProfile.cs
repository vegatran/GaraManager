using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Enums;
using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.API.Profiles
{
    public class CustomerReceptionProfile : Profile
    {
        public CustomerReceptionProfile()
        {
            // CustomerReception -> CustomerReceptionDto
            CreateMap<CustomerReception, CustomerReceptionDto>()
                .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Customer))
                .ForMember(dest => dest.Vehicle, opt => opt.MapFrom(src => src.Vehicle))
                .ForMember(dest => dest.AssignedTechnician, opt => opt.MapFrom(src => src.AssignedTechnician))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : null))
                .ForMember(dest => dest.VehiclePlate, opt => opt.MapFrom(src => src.Vehicle != null ? src.Vehicle.LicensePlate : null))
                .ForMember(dest => dest.VehicleMake, opt => opt.MapFrom(src => src.Vehicle != null ? src.Vehicle.Brand : null))
                .ForMember(dest => dest.VehicleModel, opt => opt.MapFrom(src => src.Vehicle != null ? src.Vehicle.Model : null))
                .ForMember(dest => dest.VehicleYear, opt => opt.MapFrom(src => src.Vehicle != null ? src.Vehicle.Year : null));

            // CreateCustomerReceptionDto -> CustomerReception
            CreateMap<CreateCustomerReceptionDto, CustomerReception>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ReceptionNumber, opt => opt.Ignore())
                .ForMember(dest => dest.ReceptionDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.AssignedTechnicianId.HasValue ? ReceptionStatus.Assigned : ReceptionStatus.Pending))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicle, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedTechnician, opt => opt.Ignore())
                .ForMember(dest => dest.VehicleInspection, opt => opt.Ignore());

            // UpdateCustomerReceptionDto -> CustomerReception
            CreateMap<UpdateCustomerReceptionDto, CustomerReception>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ReceptionNumber, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
                .ForMember(dest => dest.VehicleId, opt => opt.Ignore())
                .ForMember(dest => dest.ReceptionDate, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.AssignedTechnicianId.HasValue ? ReceptionStatus.Assigned : src.Status))
                .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicle, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedTechnician, opt => opt.Ignore())
                .ForMember(dest => dest.VehicleInspection, opt => opt.Ignore());
        }
    }
}
