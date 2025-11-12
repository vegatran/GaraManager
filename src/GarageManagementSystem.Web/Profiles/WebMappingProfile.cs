using AutoMapper;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Web.Models;
using System.Collections.Generic;

namespace GarageManagementSystem.Web.Profiles
{
    /// <summary>
    /// AutoMapper Profile cho Web Application
    /// </summary>
    public class WebMappingProfile : Profile
    {
        public WebMappingProfile()
        {
            // Customer mappings
            CreateMap<CustomerDto, CustomerDto>();
            CreateMap<CreateCustomerDto, CreateCustomerDto>();
            CreateMap<UpdateCustomerDto, UpdateCustomerDto>();

            // Vehicle mappings
            CreateMap<VehicleDto, VehicleDto>();
            CreateMap<CreateVehicleDto, CreateVehicleDto>();
            CreateMap<UpdateVehicleDto, UpdateVehicleDto>();

            // Employee mappings
            CreateMap<EmployeeDto, EmployeeDto>();
            CreateMap<CreateEmployeeDto, CreateEmployeeDto>();
            CreateMap<UpdateEmployeeDto, UpdateEmployeeDto>();

            // Part mappings
            CreateMap<PartDto, PartDto>();
            CreateMap<CreatePartDto, CreatePartDto>();
            CreateMap<UpdatePartDto, UpdatePartDto>();

            // Supplier mappings
            CreateMap<SupplierDto, SupplierDto>();
            CreateMap<CreateSupplierDto, CreateSupplierDto>();
            CreateMap<UpdateSupplierDto, UpdateSupplierDto>();

            // Purchase Order mappings
            CreateMap<PurchaseOrderDto, PurchaseOrderDto>();
            CreateMap<CreatePurchaseOrderDto, CreatePurchaseOrderDto>();
            CreateMap<UpdatePurchaseOrderDto, UpdatePurchaseOrderDto>();

            // Service Quotation mappings
            CreateMap<ServiceQuotationDto, ServiceQuotationDto>();
            CreateMap<CreateServiceQuotationDto, CreateServiceQuotationDto>();
            CreateMap<UpdateServiceQuotationDto, UpdateServiceQuotationDto>();

            // Service Order mappings
            CreateMap<ServiceOrderDto, ServiceOrderDto>();
            CreateMap<CreateServiceOrderDto, CreateServiceOrderDto>();
            CreateMap<UpdateServiceOrderDto, UpdateServiceOrderDto>();

            // Stock Transaction mappings
            CreateMap<StockTransactionDto, StockTransactionDto>();
            CreateMap<CreateStockTransactionDto, CreateStockTransactionDto>();
            CreateMap<UpdateStockTransactionDto, UpdateStockTransactionDto>();

            // Inventory Alert mappings
            CreateMap<InventoryAlertDto, InventoryAlertDto>();
            CreateMap<CreateInventoryAlertDto, CreateInventoryAlertDto>();
            CreateMap<UpdateInventoryAlertDto, UpdateInventoryAlertDto>();

            // Vehicle Inspection mappings
            CreateMap<VehicleInspectionDto, VehicleInspectionDto>();
            CreateMap<CreateVehicleInspectionDto, CreateVehicleInspectionDto>();
            CreateMap<UpdateVehicleInspectionDto, UpdateVehicleInspectionDto>();

            // Customer Reception mappings
            CreateMap<CustomerReceptionDto, CustomerReceptionDto>();
            CreateMap<CreateCustomerReceptionDto, CreateCustomerReceptionDto>();
            CreateMap<UpdateCustomerReceptionDto, UpdateCustomerReceptionDto>();

            // Appointment mappings
            CreateMap<AppointmentDto, AppointmentDto>();
            CreateMap<CreateAppointmentDto, CreateAppointmentDto>();
            CreateMap<UpdateAppointmentDto, UpdateAppointmentDto>();

            // Service mappings
            CreateMap<ServiceDto, ServiceDto>();
            CreateMap<CreateServiceDto, CreateServiceDto>();
            CreateMap<UpdateServiceDto, UpdateServiceDto>();

            // Payment Transaction mappings
            CreateMap<PaymentTransactionDto, PaymentTransactionDto>();
            CreateMap<CreatePaymentTransactionDto, CreatePaymentTransactionDto>();
            // UpdatePaymentTransactionDto không tồn tại, bỏ qua

                   // Print Template mappings
                   CreateMap<PrintTemplateDto, PrintTemplateDto>();
                   CreateMap<CreatePrintTemplateDto, CreatePrintTemplateDto>();
                   CreateMap<UpdatePrintTemplateDto, UpdatePrintTemplateDto>();

                   // Purchase Order View Models mappings
                   CreateMap<PurchaseOrderDto, PurchaseOrderDetailsViewModel>();
                   CreateMap<PurchaseOrderItemDto, PurchaseOrderItemViewModel>()
                       .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.QuantityOrdered))
                       .ForMember(dest => dest.VATRate, opt => opt.MapFrom(src => src.VATRate))
                       .ForMember(dest => dest.VATAmount, opt => opt.MapFrom(src => src.VATAmount));

                   // Management View Models mappings
                   CreateMap<PartDto, PartDetailsViewModel>()
                       .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? "N/A"))
                       .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category ?? "N/A"))
                       .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand ?? "N/A"))
                       .ForMember(dest => dest.DefaultUnit, opt => opt.MapFrom(src => src.DefaultUnit ?? string.Empty))
                       .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location ?? "N/A"))
                       .ForMember(dest => dest.SourceType, opt => opt.MapFrom(src => src.SourceType ?? "Purchased"))
                       .ForMember(dest => dest.InvoiceType, opt => opt.MapFrom(src => src.InvoiceType ?? "WithInvoice"))
                       .ForMember(dest => dest.Condition, opt => opt.MapFrom(src => src.Condition ?? "New"))
                       .ForMember(dest => dest.SourceReference, opt => opt.MapFrom(src => src.SourceReference ?? ""))
                       .ForMember(dest => dest.OEMNumber, opt => opt.MapFrom(src => src.OEMNumber ?? ""))
                       .ForMember(dest => dest.AftermarketNumber, opt => opt.MapFrom(src => src.AftermarketNumber ?? ""))
                       .ForMember(dest => dest.Manufacturer, opt => opt.MapFrom(src => src.Manufacturer ?? ""))
                       .ForMember(dest => dest.Dimensions, opt => opt.MapFrom(src => src.Dimensions ?? ""))
                       .ForMember(dest => dest.Weight, opt => opt.MapFrom(src => src.Weight.HasValue ? src.Weight.Value.ToString("F2") : ""))
                       .ForMember(dest => dest.Material, opt => opt.MapFrom(src => src.Material ?? ""))
                       .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Color ?? ""))
                       .ForMember(dest => dest.ReorderLevel, opt => opt.MapFrom(src => src.ReorderLevel))
                       .ForMember(dest => dest.Units, opt => opt.MapFrom(src => src.Units ?? new List<PartUnitDto>()));
                   CreateMap<SupplierDto, SupplierDetailsViewModel>();
                   CreateMap<StockTransactionDto, StockTransactionDetailsViewModel>();
                   CreateMap<CustomerDto, CustomerDetailsViewModel>();
                   CreateMap<VehicleDto, VehicleDetailsViewModel>();
                   CreateMap<EmployeeDto, EmployeeDetailsViewModel>();
               }
           }
       }
