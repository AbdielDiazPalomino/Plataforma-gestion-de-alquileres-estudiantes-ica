// Repositories/IGenericRepository.cs
using System.Linq.Expressions;

namespace Final.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        
        // Métodos adicionales
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize);
        Task<IEnumerable<T>> GetPagedAsync(
            int page, 
            int pageSize, 
            Expression<Func<T, object>> orderBy, 
            bool ascending = true);
    }
}