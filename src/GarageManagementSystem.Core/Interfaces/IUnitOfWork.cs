using GarageManagementSystem.Core.Entities;

namespace GarageManagementSystem.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ICustomerRepository Customers { get; }
        IVehicleRepository Vehicles { get; }
        IServiceRepository Services { get; }
        IServiceOrderRepository ServiceOrders { get; }
        IEmployeeRepository Employees { get; }
        IGenericRepository<Department> Departments { get; }
        IGenericRepository<Position> Positions { get; }
        IVehicleInspectionRepository VehicleInspections { get; }
        IServiceQuotationRepository ServiceQuotations { get; }
        IGenericRepository<QuotationItem> ServiceQuotationItems { get; }
        IPartRepository Parts { get; }
        IStockTransactionRepository StockTransactions { get; }
        ISupplierRepository Suppliers { get; }
        IPaymentTransactionRepository PaymentTransactions { get; }
        IAppointmentRepository Appointments { get; }
        IPrintTemplateRepository PrintTemplates { get; }
        ICustomerReceptionRepository CustomerReceptions { get; }
        IQuotationAttachmentRepository QuotationAttachments { get; }
        
        // Phase 1 - New repositories
        IGenericRepository<Invoice> Invoices { get; }
        IGenericRepository<Payment> Payments { get; }
        IGenericRepository<InsuranceClaim> InsuranceClaims { get; }
        IGenericRepository<ServiceQuotation> Quotations { get; } // Alias for ServiceQuotations
        IVehicleInspectionRepository Inspections { get; } // Alias for VehicleInspections
        IWarrantyRepository Warranties { get; }
        IGenericRepository<WarrantyItem> WarrantyItems { get; }
        IGenericRepository<WarrantyClaim> WarrantyClaims { get; }
        IGenericRepository<ServiceFeeType> ServiceFeeTypes { get; }
        IGenericRepository<ServiceOrderFee> ServiceOrderFees { get; }
        IGenericRepository<CustomerFeedback> CustomerFeedbacks { get; }
        IGenericRepository<CustomerFeedbackAttachment> CustomerFeedbackAttachments { get; }
        IGenericRepository<FeedbackChannel> FeedbackChannels { get; }
        IGenericRepository<PartUnit> PartUnits { get; }
        IGenericRepository<Warehouse> Warehouses { get; }
        IGenericRepository<WarehouseZone> WarehouseZones { get; }
        IGenericRepository<WarehouseBin> WarehouseBins { get; }
        
        // ✅ Phase 4.1 - Sprint 2: Periodic Inventory Checks
        IGenericRepository<InventoryCheck> InventoryChecks { get; }
        IGenericRepository<InventoryCheckItem> InventoryCheckItems { get; }
        IGenericRepository<InventoryAdjustment> InventoryAdjustments { get; }
        IGenericRepository<InventoryAdjustmentItem> InventoryAdjustmentItems { get; }
        
        IGenericRepository<T> Repository<T>() where T : BaseEntity;
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
