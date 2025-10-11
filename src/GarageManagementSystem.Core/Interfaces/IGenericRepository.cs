using System.Linq.Expressions;

namespace GarageManagementSystem.Core.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task DeleteByIdAsync(int id);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        
        // Methods for managing soft deleted records (for admin use)
        Task<IEnumerable<T>> GetDeletedAsync();
        Task<T?> GetDeletedByIdAsync(int id);
        Task RestoreAsync(T entity);
        Task RestoreByIdAsync(int id);
        Task<int> CountDeletedAsync();
    }
}
