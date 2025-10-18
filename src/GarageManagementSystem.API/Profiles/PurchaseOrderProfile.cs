using AutoMapper;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.API.Profiles
{
    /// <summary>
    /// AutoMapper Profile cho PurchaseOrder
    /// </summary>
    public class PurchaseOrderProfile : Profile
    {
        public PurchaseOrderProfile()
        {
            // PurchaseOrder -> PurchaseOrderDto
            CreateMap<PurchaseOrder, PurchaseOrderDto>()
                .ForMember(dest => dest.SupplierName, opt => opt.Ignore()) // Will be set manually
                .ForMember(dest => dest.ItemCount, opt => opt.Ignore()) // Will be set manually
                .ForMember(dest => dest.Items, opt => opt.Ignore()); // Will be set manually
            
            // PurchaseOrderItem -> PurchaseOrderItemDto
            CreateMap<PurchaseOrderItem, PurchaseOrderItemDto>();
            
            // CreatePurchaseOrderDto -> PurchaseOrder
            CreateMap<CreatePurchaseOrderDto, PurchaseOrder>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrderNumber, opt => opt.Ignore()) // Will be generated
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.ActualDeliveryDate, opt => opt.Ignore())
                .ForMember(dest => dest.SupplierOrderNumber, opt => opt.Ignore())
                .ForMember(dest => dest.ContactPerson, opt => opt.Ignore())
                .ForMember(dest => dest.ContactPhone, opt => opt.Ignore())
                .ForMember(dest => dest.ContactEmail, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryAddress, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentTerms, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryTerms, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => "VND"))
                .ForMember(dest => dest.SubTotal, opt => opt.Ignore())
                .ForMember(dest => dest.TaxAmount, opt => opt.Ignore())
                .ForMember(dest => dest.ShippingCost, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsApproved, opt => opt.MapFrom(src => false));
            
            // CreatePurchaseOrderItemDto -> PurchaseOrderItem
            CreateMap<CreatePurchaseOrderItemDto, PurchaseOrderItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseOrderId, opt => opt.Ignore())
                .ForMember(dest => dest.PartName, opt => opt.Ignore())
                .ForMember(dest => dest.QuantityReceived, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.QuantityOrdered * src.UnitPrice))
                .ForMember(dest => dest.SupplierPartNumber, opt => opt.Ignore())
                .ForMember(dest => dest.PartDescription, opt => opt.Ignore())
                .ForMember(dest => dest.Unit, opt => opt.Ignore())
                .ForMember(dest => dest.ExpectedDeliveryDate, opt => opt.Ignore())
                .ForMember(dest => dest.Notes, opt => opt.Ignore())
                .ForMember(dest => dest.IsReceived, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.ReceivedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());
            
            // UpdatePurchaseOrderDto -> PurchaseOrder
            CreateMap<UpdatePurchaseOrderDto, PurchaseOrder>()
                .ForMember(dest => dest.OrderNumber, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.SupplierOrderNumber, opt => opt.Ignore())
                .ForMember(dest => dest.ContactPerson, opt => opt.Ignore())
                .ForMember(dest => dest.ContactPhone, opt => opt.Ignore())
                .ForMember(dest => dest.ContactEmail, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryAddress, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentTerms, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryTerms, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => "VND"))
                .ForMember(dest => dest.SubTotal, opt => opt.Ignore())
                .ForMember(dest => dest.TaxAmount, opt => opt.Ignore())
                .ForMember(dest => dest.ShippingCost, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsApproved, opt => opt.Ignore());
        }
    }
}
