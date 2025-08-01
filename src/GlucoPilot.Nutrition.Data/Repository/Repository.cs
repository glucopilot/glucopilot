using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace GlucoPilot.Nutrition.Data.Repository;

[ExcludeFromCodeCoverage] // Maybe one day but EF is a bit of a pain...
public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly GlucoPilotNutritionDbContext _glucoPilotNutritionDbContext;

    public Repository(GlucoPilotNutritionDbContext glucoPilotNutritionDbContext)
    {
        _glucoPilotNutritionDbContext = glucoPilotNutritionDbContext;
    }

    public void Add(TEntity entity)
    {
        _glucoPilotNutritionDbContext.Set<TEntity>().Add(entity);
        _glucoPilotNutritionDbContext.SaveChanges();
    }

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _glucoPilotNutritionDbContext.Set<TEntity>().Add(entity);
        return _glucoPilotNutritionDbContext.SaveChangesAsync(cancellationToken);
    }

    public void AddMany(IEnumerable<TEntity> entities)
    {
        _glucoPilotNutritionDbContext.Set<TEntity>().AddRange(entities);
        _glucoPilotNutritionDbContext.SaveChanges();
    }

    public Task AddManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        _glucoPilotNutritionDbContext.Set<TEntity>().AddRange(entities);
        return _glucoPilotNutritionDbContext.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _glucoPilotNutritionDbContext.Set<TEntity>().Update(entity);
        return _glucoPilotNutritionDbContext.SaveChangesAsync(cancellationToken);
    }

    public void Delete(TEntity entity)
    {
        _glucoPilotNutritionDbContext.Set<TEntity>().Remove(entity);
        _glucoPilotNutritionDbContext.SaveChanges();
    }

    public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _glucoPilotNutritionDbContext.Set<TEntity>().Remove(entity);
        return _glucoPilotNutritionDbContext.SaveChangesAsync(cancellationToken);
    }

    public void DeleteMany(Expression<Func<TEntity, bool>> predicate)
    {
        var entities = Find(predicate);
        _glucoPilotNutritionDbContext.Set<TEntity>().RemoveRange(entities);
        _glucoPilotNutritionDbContext.SaveChanges();
    }

    public Task DeleteManyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var entities = Find(predicate);
        _glucoPilotNutritionDbContext.Set<TEntity>().RemoveRange(entities);
        return _glucoPilotNutritionDbContext.SaveChangesAsync(cancellationToken);
    }

    public TEntity? FindOne(Expression<Func<TEntity, bool>> predicate, FindOptions? findOptions = null)
    {
        return Get(findOptions).FirstOrDefault(predicate);
    }

    public Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> predicate, FindOptions? findOptions = null, CancellationToken cancellationToken = default)
    {
        return Get(findOptions).FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate, FindOptions? findOptions = null)
    {
        return Get(findOptions).Where(predicate);
    }

    public IQueryable<TEntity> GetAll(FindOptions? findOptions = null)
    {
        return Get(findOptions);
    }

    public IQueryable<TEntity> FromSqlRaw(string sql, FindOptions? findOptions = null, params object[] parameters)
    {
        return Get(findOptions).FromSqlRaw(sql, parameters);
    }

    public IQueryable<TModel> FromSqlRaw<TModel>(string sql, FindOptions? findOptions = null, params object[] parameters)
        where TModel : class
    {
        return _glucoPilotNutritionDbContext.Database.SqlQueryRaw<TModel>(sql, parameters);
    }

    public void Update(TEntity entity)
    {
        _glucoPilotNutritionDbContext.Set<TEntity>().Update(entity);
        _glucoPilotNutritionDbContext.SaveChanges();
    }

    public bool Any(Expression<Func<TEntity, bool>> predicate)
    {
        return _glucoPilotNutritionDbContext.Set<TEntity>().Any(predicate);
    }

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return _glucoPilotNutritionDbContext.Set<TEntity>().AnyAsync(predicate, cancellationToken);
    }

    public int Count(Expression<Func<TEntity, bool>> predicate)
    {
        return _glucoPilotNutritionDbContext.Set<TEntity>().Count(predicate);
    }

    public Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return _glucoPilotNutritionDbContext.Set<TEntity>().CountAsync(predicate, cancellationToken);
    }

    private DbSet<TEntity> Get(FindOptions? findOptions = null)
    {
        findOptions ??= new FindOptions();
        var entity = _glucoPilotNutritionDbContext.Set<TEntity>();
        if (findOptions.IsAsNoTracking && findOptions.IsIgnoreAutoIncludes)
        {
            entity.IgnoreAutoIncludes().AsNoTracking();
        }
        else if (findOptions.IsIgnoreAutoIncludes)
        {
            entity.IgnoreAutoIncludes();
        }
        else if (findOptions.IsAsNoTracking)
        {
            entity.AsNoTracking();
        }

        return entity;
    }
}