using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.API.Profiles
{
    /// <summary>
    /// AutoMapper Profile cho PaymentTransaction
    /// </summary>
    public class PaymentTransactionProfile : Profile
    {
        public PaymentTransactionProfile()
        {
            // PaymentTransaction -> PaymentTransactionDto
            CreateMap<PaymentTransaction, PaymentTransactionDto>()
                .ForMember(dest => dest.ReceivedBy, opt => opt.MapFrom(src => src.ReceivedBy));

            // CreatePaymentTransactionDto -> PaymentTransaction
            CreateMap<CreatePaymentTransactionDto, PaymentTransaction>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ReceiptNumber, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ReceivedBy, opt => opt.Ignore());
        }
    }
}

