using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ConversorDadosXPTO.Services
{
    public interface IEntityService<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task<int> AddAsync(T entity);
        Task AddBatchAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task<IEnumerable<T>> FindByConditionAsync(Expression<Func<T, bool>> expression);
    }
}
