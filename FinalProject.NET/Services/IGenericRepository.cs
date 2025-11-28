using System.Linq.Expressions;

namespace FinalProject.NET.Services
{
    public interface IGenericRepository<T> where T : class
    {
        IQueryable<T> Query();
        Task<T> CreateAsync(T entity);
        Task<T?> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includes);
        Task<List<T>> GetAsync(Expression<Func<T, bool>>? filter = null, params Expression<Func<T, object>>[] includes);
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(T entity);
        Task<bool> SoftDeleteAsync(T entity);
        Task<int> SaveChangesAsync();
    }
}
