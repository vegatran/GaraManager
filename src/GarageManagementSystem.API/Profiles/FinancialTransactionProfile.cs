using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.API.Profiles
{
    /// <summary>
    /// AutoMapper Profile cho FinancialTransaction
    /// </summary>
    public class FinancialTransactionProfile : Profile
    {
        public FinancialTransactionProfile()
        {
            // FinancialTransaction -> FinancialTransactionDto
            CreateMap<FinancialTransaction, FinancialTransactionDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.Ignore())
                .ReverseMap();
            
            // CreateFinancialTransactionDto -> FinancialTransaction
            CreateMap<CreateFinancialTransactionDto, FinancialTransaction>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TransactionNumber, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());
        }
    }
}

