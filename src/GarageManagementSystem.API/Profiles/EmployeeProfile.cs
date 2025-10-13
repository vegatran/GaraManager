using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.API.Profiles
{
    public class EmployeeProfile : Profile
    {
        public EmployeeProfile()
        {
            // Employee Entity to DTO mappings
            CreateMap<Employee, EmployeeDto>()
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.PositionNavigation != null ? src.PositionNavigation.Name : src.Position))
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.DepartmentNavigation != null ? src.DepartmentNavigation.Name : src.Department))
                .ForMember(dest => dest.PositionName, opt => opt.MapFrom(src => src.PositionNavigation != null ? src.PositionNavigation.Name : src.Position))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.DepartmentNavigation != null ? src.DepartmentNavigation.Name : src.Department));

            // Create DTO to Employee Entity
            CreateMap<CreateEmployeeDto, Employee>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Position, opt => opt.Ignore())
                .ForMember(dest => dest.Department, opt => opt.Ignore());

            // Update DTO to Employee Entity
            CreateMap<UpdateEmployeeDto, Employee>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Position, opt => opt.Ignore())
                .ForMember(dest => dest.Department, opt => opt.Ignore());
        }
    }
}
