using ConversorDadosXPTO.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ConversorDadosXPTO.Services
{
    public class EntityService<T> : IEntityService<T> where T : class
    {
        private readonly IGenericRepository<T> _repository;

        public EntityService(IGenericRepository<T> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> AddAsync(T entity)
        {
            return await _repository.AddAsync(entity);
        }

        public async Task AddBatchAsync(IEnumerable<T> entities)
        {
            await _repository.AddBatchAsync(entities);
        }

        public async Task UpdateAsync(T entity)
        {
            await _repository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<T>> FindByConditionAsync(Expression<Func<T, bool>> expression)
        {
            return await _repository.FindByConditionAsync(expression);
        }
    }
}
