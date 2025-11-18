using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace GarageManagementSystem.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly GarageDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(GarageDbContext context)
        {
            _context = context;
            Customers = new CustomerRepository(_context);
            Vehicles = new VehicleRepository(_context);
            Services = new ServiceRepository(_context);
            ServiceOrders = new ServiceOrderRepository(_context);
            Employees = new EmployeeRepository(_context);
            Departments = new GenericRepository<Department>(_context);
            Positions = new GenericRepository<Position>(_context);
            VehicleInspections = new VehicleInspectionRepository(_context);
            ServiceQuotations = new ServiceQuotationRepository(_context);
            ServiceQuotationItems = new GenericRepository<QuotationItem>(_context);
            Parts = new PartRepository(_context);
            StockTransactions = new StockTransactionRepository(_context);
            Suppliers = new SupplierRepository(_context);
            PaymentTransactions = new PaymentTransactionRepository(_context);
            Appointments = new AppointmentRepository(_context);
            PrintTemplates = new PrintTemplateRepository(_context);
            CustomerReceptions = new CustomerReceptionRepository(_context);
            QuotationAttachments = new QuotationAttachmentRepository(_context);
            
            // Phase 1 - New repositories
            Invoices = new GenericRepository<Invoice>(_context);
            Payments = new GenericRepository<Payment>(_context);
            InsuranceClaims = new GenericRepository<InsuranceClaim>(_context);
            Quotations = ServiceQuotations; // Alias
            Inspections = VehicleInspections; // Alias
            Warranties = new WarrantyRepository(_context);
            WarrantyItems = new GenericRepository<WarrantyItem>(_context);
            WarrantyClaims = new GenericRepository<WarrantyClaim>(_context);
            ServiceFeeTypes = new GenericRepository<ServiceFeeType>(_context);
            ServiceOrderFees = new GenericRepository<ServiceOrderFee>(_context);
            CustomerFeedbacks = new GenericRepository<CustomerFeedback>(_context);
            CustomerFeedbackAttachments = new GenericRepository<CustomerFeedbackAttachment>(_context);
            FeedbackChannels = new GenericRepository<FeedbackChannel>(_context);
            PartUnits = new GenericRepository<PartUnit>(_context);
            Warehouses = new GenericRepository<Warehouse>(_context);
            WarehouseZones = new GenericRepository<WarehouseZone>(_context);
            WarehouseBins = new GenericRepository<WarehouseBin>(_context);
            
            // ✅ Phase 4.1 - Sprint 2: Periodic Inventory Checks
            InventoryChecks = new GenericRepository<InventoryCheck>(_context);
            InventoryCheckItems = new GenericRepository<InventoryCheckItem>(_context);
            InventoryAdjustments = new GenericRepository<InventoryAdjustment>(_context);
            InventoryAdjustmentItems = new GenericRepository<InventoryAdjustmentItem>(_context);
            // ✅ Phase 4.1 - Advanced Features: Comments/Notes Timeline
            InventoryCheckComments = new GenericRepository<InventoryCheckComment>(_context);
            InventoryAdjustmentComments = new GenericRepository<InventoryAdjustmentComment>(_context);
            
            // ✅ Phase 4.2 - Procurement Management
            ReorderSuggestions = new GenericRepository<ReorderSuggestion>(_context);
            SupplierQuotations = new GenericRepository<SupplierQuotation>(_context);
            SupplierRatings = new GenericRepository<SupplierRating>(_context);
            SupplierPerformances = new GenericRepository<SupplierPerformance>(_context);
            SupplierPerformanceHistories = new GenericRepository<SupplierPerformanceHistory>(_context);
            PurchaseOrderStatusHistories = new GenericRepository<PurchaseOrderStatusHistory>(_context);
        }

        public ICustomerRepository Customers { get; private set; }
        public IVehicleRepository Vehicles { get; private set; }
        public IServiceRepository Services { get; private set; }
        public IServiceOrderRepository ServiceOrders { get; private set; }
        public IEmployeeRepository Employees { get; private set; }
        public IGenericRepository<Department> Departments { get; private set; }
        public IGenericRepository<Position> Positions { get; private set; }
        public IVehicleInspectionRepository VehicleInspections { get; private set; }
        public IServiceQuotationRepository ServiceQuotations { get; private set; }
        public IGenericRepository<QuotationItem> ServiceQuotationItems { get; private set; }
        public IPartRepository Parts { get; private set; }
        public IStockTransactionRepository StockTransactions { get; private set; }
        public ISupplierRepository Suppliers { get; private set; }
        public IPaymentTransactionRepository PaymentTransactions { get; private set; }
        public IAppointmentRepository Appointments { get; private set; }
        public IPrintTemplateRepository PrintTemplates { get; private set; }
        public ICustomerReceptionRepository CustomerReceptions { get; private set; }
        public IQuotationAttachmentRepository QuotationAttachments { get; private set; }
        
        // Phase 1 - New repositories
        public IGenericRepository<Invoice> Invoices { get; private set; }
        public IGenericRepository<Payment> Payments { get; private set; }
        public IGenericRepository<InsuranceClaim> InsuranceClaims { get; private set; }
        public IGenericRepository<ServiceQuotation> Quotations { get; private set; }
        public IVehicleInspectionRepository Inspections { get; private set; }
        public IWarrantyRepository Warranties { get; private set; }
        public IGenericRepository<WarrantyItem> WarrantyItems { get; private set; }
        public IGenericRepository<WarrantyClaim> WarrantyClaims { get; private set; }
        public IGenericRepository<ServiceFeeType> ServiceFeeTypes { get; private set; }
        public IGenericRepository<ServiceOrderFee> ServiceOrderFees { get; private set; }
        public IGenericRepository<CustomerFeedback> CustomerFeedbacks { get; private set; }
        public IGenericRepository<CustomerFeedbackAttachment> CustomerFeedbackAttachments { get; private set; }
        public IGenericRepository<FeedbackChannel> FeedbackChannels { get; private set; }
        public IGenericRepository<PartUnit> PartUnits { get; private set; }
        public IGenericRepository<Warehouse> Warehouses { get; private set; }
        public IGenericRepository<WarehouseZone> WarehouseZones { get; private set; }
        public IGenericRepository<WarehouseBin> WarehouseBins { get; private set; }
        
        // ✅ Phase 4.1 - Sprint 2: Periodic Inventory Checks
        public IGenericRepository<InventoryCheck> InventoryChecks { get; private set; }
        public IGenericRepository<InventoryCheckItem> InventoryCheckItems { get; private set; }
        public IGenericRepository<InventoryAdjustment> InventoryAdjustments { get; private set; }
        public IGenericRepository<InventoryAdjustmentItem> InventoryAdjustmentItems { get; private set; }
        // ✅ Phase 4.1 - Advanced Features: Comments/Notes Timeline
        public IGenericRepository<InventoryCheckComment> InventoryCheckComments { get; private set; }
        public IGenericRepository<InventoryAdjustmentComment> InventoryAdjustmentComments { get; private set; }
        
        // ✅ Phase 4.2 - Procurement Management
        public IGenericRepository<ReorderSuggestion> ReorderSuggestions { get; private set; }
        public IGenericRepository<SupplierQuotation> SupplierQuotations { get; private set; }
        public IGenericRepository<SupplierRating> SupplierRatings { get; private set; }
        public IGenericRepository<SupplierPerformance> SupplierPerformances { get; private set; }
        public IGenericRepository<SupplierPerformanceHistory> SupplierPerformanceHistories { get; private set; }
        public IGenericRepository<PurchaseOrderStatusHistory> PurchaseOrderStatusHistories { get; private set; }

        public IGenericRepository<T> Repository<T>() where T : BaseEntity
        {
            return new GenericRepository<T>(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
