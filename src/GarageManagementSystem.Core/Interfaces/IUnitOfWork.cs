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
        IPartRepository Parts { get; }
        IStockTransactionRepository StockTransactions { get; }
        ISupplierRepository Suppliers { get; }
        IPaymentTransactionRepository PaymentTransactions { get; }
        IAppointmentRepository Appointments { get; }
        IGenericRepository<T> Repository<T>() where T : BaseEntity;
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
