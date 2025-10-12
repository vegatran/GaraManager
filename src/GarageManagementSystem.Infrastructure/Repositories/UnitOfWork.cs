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
            Parts = new PartRepository(_context);
            StockTransactions = new StockTransactionRepository(_context);
            Suppliers = new SupplierRepository(_context);
            PaymentTransactions = new PaymentTransactionRepository(_context);
            Appointments = new AppointmentRepository(_context);
            
            // Phase 1 - New repositories
            Invoices = new GenericRepository<Invoice>(_context);
            Payments = new GenericRepository<Payment>(_context);
            InsuranceClaims = new GenericRepository<InsuranceClaim>(_context);
            Quotations = ServiceQuotations; // Alias
            Inspections = VehicleInspections; // Alias
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
        public IPartRepository Parts { get; private set; }
        public IStockTransactionRepository StockTransactions { get; private set; }
        public ISupplierRepository Suppliers { get; private set; }
        public IPaymentTransactionRepository PaymentTransactions { get; private set; }
        public IAppointmentRepository Appointments { get; private set; }
        
        // Phase 1 - New repositories
        public IGenericRepository<Invoice> Invoices { get; private set; }
        public IGenericRepository<Payment> Payments { get; private set; }
        public IGenericRepository<InsuranceClaim> InsuranceClaims { get; private set; }
        public IGenericRepository<ServiceQuotation> Quotations { get; private set; }
        public IVehicleInspectionRepository Inspections { get; private set; }

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
