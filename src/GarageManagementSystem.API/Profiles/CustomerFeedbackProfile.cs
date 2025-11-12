using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.API.Profiles
{
    public class CustomerFeedbackProfile : Profile
    {
        public CustomerFeedbackProfile()
        {
            CreateMap<FeedbackChannel, FeedbackChannelDto>();

            CreateMap<CustomerFeedbackAttachment, CustomerFeedbackAttachmentDto>();

            CreateMap<CustomerFeedback, CustomerFeedbackDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : null))
                .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.ServiceOrder != null ? src.ServiceOrder.OrderNumber : null))
                .ForMember(dest => dest.FollowUpByName, opt => opt.MapFrom(src => src.FollowUpBy != null ? src.FollowUpBy.Name : null))
                .ForMember(dest => dest.FeedbackChannelName, opt => opt.MapFrom(src => src.FeedbackChannel != null ? src.FeedbackChannel.Name : null));

            CreateMap<CreateCustomerFeedbackDto, CustomerFeedback>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Attachments, opt => opt.Ignore())
                .ForMember(dest => dest.FollowUpBy, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceOrder, opt => opt.Ignore())
                .ForMember(dest => dest.FeedbackChannel, opt => opt.Ignore());

        }
    }
}

