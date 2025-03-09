
using System.Linq.Expressions;
using System;
using QuizHub.Data;
using Microsoft.EntityFrameworkCore;
using QuizHub.Data.Repository.Base;
using System.Diagnostics.Contracts;

namespace QuizHub.Data.Repository
{
    public class MainRepository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _dbContext;

        public MainRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddAsyncEntity(T entity)
        {
           

                await _dbContext.Set<T>().AddAsync(entity);
                _dbContext.SaveChanges();
            
           
        }

        public void DeleteEntity(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            _dbContext.SaveChanges();
        }

        public void UpdateEntity(T entity)
        {
            _dbContext.Set<T>().Update(entity);
            _dbContext.SaveChanges();
        }

        public async Task<T> GetByIdAsync<TKey>(TKey id)
        {
            return await _dbContext.Set<T>().FindAsync(id);

        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _dbContext.Set<T>().ToListAsync();
        }
        public async Task<T> GetIncludeById(int id, params string[] agers)
        {

            IQueryable<T> table = _dbContext.Set<T>();
            foreach (var ar in agers)
            {
              table = table.Include(ar);
            }
            var Item = await table.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
            return Item;
        }
        public async Task<T> GetIncludeById(string id, params string[] agers)
        {

            IQueryable<T> table = _dbContext.Set<T>();
            foreach (var ar in agers)
            {
               table = table.Include(ar);
            }
            var Item = await table.FirstOrDefaultAsync(e => EF.Property<string>(e, "Id") == id);
            return Item;
        }

        public async Task<List<T>> GetAllIncludeAsync(params string[] agers)
        {
            IQueryable<T> table = _dbContext.Set<T>();
            foreach (var ar in agers)
            {
                table =  table.Include(ar);
            }
            return await table.ToListAsync();
        }

        public Task<T> SelecteOne(Expression<Func<T, bool>> filter)
        {
            return _dbContext.Set<T>().SingleOrDefaultAsync(filter);
        }

    }
}
