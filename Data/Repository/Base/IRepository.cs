using System.Linq.Expressions;

namespace QuizHub.Data.Repository.Base
{
    public interface IRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync();
        Task<T> GetByIdAsync<TKey>(TKey id);
        Task<T> SelecteOne(Expression<Func<T, bool>> filter);
        Task<T> GetIncludeById(int id, params string[] agers);
        Task<T> GetIncludeById(string id, params string[] agers);
        Task<List<T>> GetAllIncludeAsync(params string[] agers);

        Task AddAsyncEntity(T entity);
        void DeleteEntity(T entity);
        void UpdateEntity(T entity);


    }
}
