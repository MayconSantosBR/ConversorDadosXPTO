using ConversorDadosXPTO.Context;
using ConversorDadosXPTO.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ConversorDadosXPTO.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly DadosXptoContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(DadosXptoContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<int> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            var entry = _context.Entry(entity);

            switch (typeof(T).Name)
            {
                case "ProgramaSocial":
                    return (int)entry.Property("IdprogramaSocial").CurrentValue;
                case "Dado":
                    return (int)entry.Property("Iddados").CurrentValue;
                case "Cidadao":
                    return (int)entry.Property("Idcidadao").CurrentValue;
                case "UfCidade":
                    return (int)entry.Property("IdufCidade").CurrentValue;
                default:
                    return 0;
            }
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<T>> FindByConditionAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbSet.Where(expression).ToListAsync();
        }
    }
}
