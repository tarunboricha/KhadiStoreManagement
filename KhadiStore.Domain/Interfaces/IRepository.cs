using System.Linq.Expressions;
using KhadiStore.Domain.Entities;

namespace KhadiStore.Domain.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> CountAsync();
        Task<IEnumerable<T>> GetPagedAsync(int pageIndex, int pageSize);
    }
}