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
        }

        public ICustomerRepository Customers { get; private set; }
        public IVehicleRepository Vehicles { get; private set; }
        public IServiceRepository Services { get; private set; }
        public IServiceOrderRepository ServiceOrders { get; private set; }
        public IEmployeeRepository Employees { get; private set; }

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
