using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.API.Profiles
{
    public class PositionProfile : Profile
    {
        public PositionProfile()
        {
            // Position Entity to DTO mappings
            CreateMap<Position, PositionDto>();
            CreateMap<Position, DepartmentDto>(); // For dropdown compatibility

            // Create DTO to Position Entity
            CreateMap<CreatePositionDto, Position>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Employees, opt => opt.Ignore());

            // Update DTO to Position Entity
            CreateMap<UpdatePositionDto, Position>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Employees, opt => opt.Ignore());
        }
    }
}
