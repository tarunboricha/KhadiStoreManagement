using KhadiStore.Domain.Entities;
using KhadiStore.Domain.Interfaces;
using KhadiStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace KhadiStore.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly KhadiStoreDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(KhadiStoreDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.Where(x => !x.IsDeleted).FirstOrDefaultAsync(e => e.Id == id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.Where(x => !x.IsDeleted).ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(x => !x.IsDeleted).Where(predicate).ToListAsync();
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync(); // ← This line is critical
            return entity;
        }

        public virtual Task<T> UpdateAsync(T entity)
        {
            entity.ModifiedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
            _context.SaveChangesAsync();
            return Task.FromResult(entity);
        }

        public virtual async Task<bool> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            await UpdateAsync(entity);
            return true;
        }

        public virtual async Task<bool> ExistsAsync(int id)
        {
            return await _dbSet.Where(x => !x.IsDeleted).AnyAsync(e => e.Id == id);
        }

        public virtual async Task<int> CountAsync()
        {
            return await _dbSet.Where(x => !x.IsDeleted).CountAsync();
        }

        public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageIndex, int pageSize)
        {
            return await _dbSet
                .Where(x => !x.IsDeleted)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}