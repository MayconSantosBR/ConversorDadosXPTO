using ConversorDadosXPTO.Context;
using ConversorDadosXPTO.Models;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
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
        private readonly IDbContextFactory<DadosXptoContext> _contextFactory;
        private readonly DadosXptoContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(IDbContextFactory<DadosXptoContext> dbContextFactory, DadosXptoContext context)
        {
            _contextFactory = dbContextFactory;
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var dbSet = context.Set<T>();

            return await dbSet.FindAsync(id);
        }

        public async Task<int> AddAsync(T entity)
        {
            using var context = _contextFactory.CreateDbContext();
            var dbSet = context.Set<T>();

            await dbSet.AddAsync(entity);
            await context.SaveChangesAsync();
            var entry = context.Entry(entity);

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

        public async Task AddBatchAsync(IEnumerable<T> entities)
        {
            switch (typeof(T).Name)
            {
                case "Dado":
                    await _context.BulkInsertOrUpdateAsync(entities, new BulkConfig()
                    {
                        PropertiesToIncludeOnCompare = ["IdufCidade", "Idcidadao", "IdprogramaSocial", "MesAno"]
                    });
                    break;
                case "Cidadao":
                    await _context.BulkInsertOrUpdateAsync(entities, new BulkConfig() 
                    {
                        PropertiesToIncludeOnCompare = ["Cpf"] 
                    });
                    break;
                default:
                    await _context.BulkInsertOrUpdateAsync(entities);
                    break;
            }

            await _context.SaveChangesAsync();
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
            using var context = _contextFactory.CreateDbContext();
            var dbSet = context.Set<T>();

            return await dbSet.Where(expression).AsNoTracking().ToListAsync();
        }
    }
}
