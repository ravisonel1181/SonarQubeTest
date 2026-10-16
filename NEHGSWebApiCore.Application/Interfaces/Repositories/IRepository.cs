using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NEHGSWebApiCore.Application.Interfaces.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(object id);

        Task<List<T>> GetAllAsync();

        Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);

        Task AddAsync(T entity);

        void Update(T entity);

        void Delete(T entity);

        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    }
}
