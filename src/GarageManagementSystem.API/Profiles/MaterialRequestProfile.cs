using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.API.Profiles
{
    public class MaterialRequestProfile : Profile
    {
        public MaterialRequestProfile()
        {
            CreateMap<MaterialRequest, MaterialRequestDto>();
            CreateMap<MaterialRequestItem, MaterialRequestItemDto>();

            CreateMap<CreateMaterialRequestDto, MaterialRequest>();
            CreateMap<CreateMaterialRequestItemDto, MaterialRequestItem>();
        }
    }
}


