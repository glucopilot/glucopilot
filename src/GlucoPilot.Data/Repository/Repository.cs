using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GlucoPilot.Data.Repository;

[ExcludeFromCodeCoverage] // Maybe one day but EF is a bit of a pain...
public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly GlucoPilotDbContext _glucoPilotDbContext;

    public Repository(GlucoPilotDbContext glucoPilotDbContext)
    {
        _glucoPilotDbContext = glucoPilotDbContext;
    }

    public void Add(TEntity entity)
    {
        _glucoPilotDbContext.Set<TEntity>().Add(entity);
        _glucoPilotDbContext.SaveChanges();
    }

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _glucoPilotDbContext.Set<TEntity>().Add(entity);
        return _glucoPilotDbContext.SaveChangesAsync(cancellationToken);
    }

    public void AddMany(IEnumerable<TEntity> entities)
    {
        _glucoPilotDbContext.Set<TEntity>().AddRange(entities);
        _glucoPilotDbContext.SaveChanges();
    }

    public Task AddManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        _glucoPilotDbContext.Set<TEntity>().AddRange(entities);
        return _glucoPilotDbContext.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _glucoPilotDbContext.Set<TEntity>().Update(entity);
        return _glucoPilotDbContext.SaveChangesAsync(cancellationToken);
    }

    public void Delete(TEntity entity)
    {
        _glucoPilotDbContext.Set<TEntity>().Remove(entity);
        _glucoPilotDbContext.SaveChanges();
    }

    public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _glucoPilotDbContext.Set<TEntity>().Remove(entity);
        return _glucoPilotDbContext.SaveChangesAsync(cancellationToken);
    }

    public void DeleteMany(Expression<Func<TEntity, bool>> predicate)
    {
        var entities = Find(predicate);
        _glucoPilotDbContext.Set<TEntity>().RemoveRange(entities);
        _glucoPilotDbContext.SaveChanges();
    }

    public Task DeleteManyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var entities = Find(predicate);
        _glucoPilotDbContext.Set<TEntity>().RemoveRange(entities);
        return _glucoPilotDbContext.SaveChangesAsync(cancellationToken);
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

    public void Update(TEntity entity)
    {
        _glucoPilotDbContext.Set<TEntity>().Update(entity);
        _glucoPilotDbContext.SaveChanges();
    }

    public bool Any(Expression<Func<TEntity, bool>> predicate)
    {
        return _glucoPilotDbContext.Set<TEntity>().Any(predicate);
    }

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return _glucoPilotDbContext.Set<TEntity>().AnyAsync(predicate, cancellationToken);
    }

    public int Count(Expression<Func<TEntity, bool>> predicate)
    {
        return _glucoPilotDbContext.Set<TEntity>().Count(predicate);
    }

    public Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return _glucoPilotDbContext.Set<TEntity>().CountAsync(predicate, cancellationToken);
    }

    private DbSet<TEntity> Get(FindOptions? findOptions = null)
    {
        findOptions ??= new FindOptions();
        var entity = _glucoPilotDbContext.Set<TEntity>();
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