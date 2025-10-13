using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.API.Profiles
{
    public class DepartmentProfile : Profile
    {
        public DepartmentProfile()
        {
            // Department Entity to DTO mappings
            CreateMap<Department, DepartmentDto>();

            // Create DTO to Department Entity
            CreateMap<CreateDepartmentDto, Department>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Employees, opt => opt.Ignore());

            // Update DTO to Department Entity
            CreateMap<UpdateDepartmentDto, Department>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Employees, opt => opt.Ignore());
        }
    }
}
