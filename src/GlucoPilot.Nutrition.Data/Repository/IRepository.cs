using System.Linq.Expressions;

namespace GlucoPilot.Nutrition.Data.Repository;

public interface IRepository<TEntity> where TEntity : class
{
    IQueryable<TEntity> GetAll(FindOptions? findOptions = null);
    TEntity? FindOne(Expression<Func<TEntity, bool>> predicate, FindOptions? findOptions = null);
    Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> predicate, FindOptions? findOptions = null, CancellationToken cancellationToken = default);
    IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate, FindOptions? findOptions = null);
    IQueryable<TEntity> FromSqlRaw(string sql, FindOptions? findOptions = null, params object[] parameters);

    IQueryable<TModel> FromSqlRaw<TModel>(string sql, FindOptions? findOptions = null, params object[] parameters) where TModel : class;
    void Add(TEntity entity);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void AddMany(IEnumerable<TEntity> entities);
    Task AddManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Delete(TEntity entity);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    void DeleteMany(Expression<Func<TEntity, bool>> predicate);
    Task DeleteManyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    bool Any(Expression<Func<TEntity, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    int Count(Expression<Func<TEntity, bool>> predicate);
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
}