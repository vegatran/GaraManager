using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GarageManagementSystem.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly GarageDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(GarageDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.Where(e => !e.IsDeleted).ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(e => !e.IsDeleted).Where(predicate).ToListAsync();
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(e => !e.IsDeleted).FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            // AuditInterceptor sẽ tự động set CreatedAt và CreatedBy
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            // AuditInterceptor sẽ tự động set CreatedAt và CreatedBy cho từng entity
            await _dbSet.AddRangeAsync(entities);
            return entities;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            // AuditInterceptor sẽ tự động set UpdatedAt và UpdatedBy
            _dbSet.Update(entity);
        }

        public virtual async Task DeleteAsync(T entity)
        {
            // AuditInterceptor sẽ tự động set DeletedAt, DeletedBy, UpdatedAt, UpdatedBy
            entity.IsDeleted = true;
            _dbSet.Update(entity);
        }

        public virtual async Task DeleteByIdAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                await DeleteAsync(entity);
            }
        }

        public virtual async Task<int> CountAsync()
        {
            return await _dbSet.Where(e => !e.IsDeleted).CountAsync();
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(e => !e.IsDeleted).CountAsync(predicate);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(e => !e.IsDeleted).AnyAsync(predicate);
        }

        // Methods for managing soft deleted records (for admin use)
        public virtual async Task<IEnumerable<T>> GetDeletedAsync()
        {
            return await _dbSet.Where(e => e.IsDeleted).ToListAsync();
        }

        public virtual async Task<T?> GetDeletedByIdAsync(int id)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && e.IsDeleted);
        }

        public virtual async Task RestoreAsync(T entity)
        {
            entity.IsDeleted = false;
            entity.DeletedAt = null;
            entity.DeletedBy = null;
            // AuditInterceptor sẽ tự động set UpdatedAt và UpdatedBy
            _dbSet.Update(entity);
        }

        public virtual async Task RestoreByIdAsync(int id)
        {
            var entity = await GetDeletedByIdAsync(id);
            if (entity != null)
            {
                await RestoreAsync(entity);
            }
        }

        public virtual async Task<int> CountDeletedAsync()
        {
            return await _dbSet.Where(e => e.IsDeleted).CountAsync();
        }
    }
}
