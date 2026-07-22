using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

using Framework.BuildingBlock.Data;
using Framework.BuildingBlock.Entities;

using Microsoft.EntityFrameworkCore;

namespace Framework.BuildingBlock.EntityFrameworkCore;

/// <summary>
/// Entity Framework Core implementation of <see cref="IGenericRepository{TEntity, TKey}"/>.
/// it depends only on <see cref="DbContext"/> and the
/// framework entity base types.
/// </summary>
public class EfGenericRepository<TEntity, TKey> : IGenericRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    protected DbContext Context { get; }

    protected DbSet<TEntity> Set => Context.Set<TEntity>();

    public EfGenericRepository(DbContext context)
    {
        Context = context;
    }

    public virtual IQueryable<TEntity> GetQueryable()
        => Set.AsQueryable();

    public virtual IQueryable<TEntity> GetQueryableIncludingDeleted()
        => Set.IgnoreQueryFilters();

    public virtual Task<TEntity?> FindAsync(TKey id, CancellationToken cancellationToken = default)
        => Set.FirstOrDefaultAsync(BuildIdPredicate(id), cancellationToken);

    public virtual Task<TEntity?> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        => Set.FirstOrDefaultAsync(predicate, cancellationToken);

    public virtual async Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default)
        => await FindAsync(id, cancellationToken)
           ?? throw new EntityNotFoundException(typeof(TEntity), id);

    public virtual async Task<TEntity> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await FindAsync(predicate, cancellationToken)
           ?? throw new EntityNotFoundException(typeof(TEntity), null);

    public virtual Task<List<TEntity>> GetListAsync(CancellationToken cancellationToken = default)
        => Set.ToListAsync(cancellationToken);

    public virtual Task<List<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        => Set.Where(predicate).ToListAsync(cancellationToken);

    public virtual async Task<PagedResult<TEntity>> GetPagedListAsync(
        int page,
        int pageSize,
        Expression<Func<TEntity, bool>>? predicate = null,
        string? sorting = null,
        CancellationToken cancellationToken = default)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = GetQueryable();

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        var totalCount = await query.LongCountAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(sorting))
        {
            query = query.OrderBy(sorting);
        }

        query = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        var pageCount = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PagedResult<TEntity>
        {
            Queryable = query,
            CurrentPage = page,
            PageSize = pageSize,
            PageCount = pageCount,
            RowCount = (int)totalCount
        };
    }

    public virtual Task<long> CountAsync(CancellationToken cancellationToken = default)
        => Set.LongCountAsync(cancellationToken);

    public virtual Task<long> CountAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        => Set.LongCountAsync(predicate, cancellationToken);

    public virtual Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        => Set.AnyAsync(predicate, cancellationToken);

    public virtual async Task<TEntity> InsertAsync(
        TEntity entity,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        var entry = await Set.AddAsync(entity, cancellationToken);
        await SaveIfRequestedAsync(autoSave, cancellationToken);
        return entry.Entity;
    }

    public virtual async Task InsertManyAsync(
        IEnumerable<TEntity> entities,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        await Set.AddRangeAsync(entities, cancellationToken);
        await SaveIfRequestedAsync(autoSave, cancellationToken);
    }

    public virtual async Task<TEntity> UpdateAsync(
        TEntity entity,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        var entry = Context.Update(entity);
        await SaveIfRequestedAsync(autoSave, cancellationToken);
        return entry.Entity;
    }

    public virtual async Task UpdateManyAsync(
        IEnumerable<TEntity> entities,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        Set.UpdateRange(entities);
        await SaveIfRequestedAsync(autoSave, cancellationToken);
    }

    public virtual async Task DeleteAsync(
        TEntity entity,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        Set.Remove(entity);
        await SaveIfRequestedAsync(autoSave, cancellationToken);
    }

    public virtual async Task DeleteAsync(
        TKey id,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (entity is null)
        {
            return;
        }

        await DeleteAsync(entity, autoSave, cancellationToken);
    }

    public virtual async Task DeleteManyAsync(
        IEnumerable<TEntity> entities,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        Set.RemoveRange(entities);
        await SaveIfRequestedAsync(autoSave, cancellationToken);
    }

    protected virtual Task SaveIfRequestedAsync(bool autoSave, CancellationToken cancellationToken)
        => autoSave ? Context.SaveChangesAsync(cancellationToken) : Task.CompletedTask;

    private static Expression<Func<TEntity, bool>> BuildIdPredicate(TKey id)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "e");
        var idProperty = Expression.Property(parameter, nameof(Entity<TKey>.Id));
        var body = Expression.Equal(idProperty, Expression.Constant(id, typeof(TKey)));
        return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
    }
}

/// <summary>
/// Convenience repository for entities with a <see cref="System.Guid"/> primary key.
/// </summary>
public class EfGenericRepository<TEntity> : EfGenericRepository<TEntity, Guid>, IGenericRepository<TEntity>
    where TEntity : Entity<Guid>
{
    public EfGenericRepository(DbContext context) : base(context)
    {
    }
}
public class EfGenericRepository<TDbContext, TEntity, TKey> : EfGenericRepository<TEntity, TKey>, IGenericRepository<TEntity,TKey>
    where TEntity : Entity<TKey>
    where TDbContext : DbContext
{
    public EfGenericRepository(TDbContext context) : base(context)
    {
    }
}
