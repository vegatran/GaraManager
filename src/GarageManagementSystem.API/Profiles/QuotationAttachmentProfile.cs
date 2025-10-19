using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.API.Profiles
{
    public class QuotationAttachmentProfile : Profile
    {
        public QuotationAttachmentProfile()
        {
            CreateMap<QuotationAttachment, QuotationAttachmentDto>()
                .ForMember(dest => dest.UploadedBy, opt => opt.MapFrom(src => src.UploadedBy));
            
            CreateMap<CreateQuotationAttachmentDto, QuotationAttachment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.FileName, opt => opt.Ignore())
                .ForMember(dest => dest.FilePath, opt => opt.Ignore())
                .ForMember(dest => dest.FileType, opt => opt.Ignore())
                .ForMember(dest => dest.FileSize, opt => opt.Ignore())
                .ForMember(dest => dest.UploadedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.UploadedById, opt => opt.Ignore())
                .ForMember(dest => dest.UploadedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceQuotation, opt => opt.Ignore());
        }
    }
}
