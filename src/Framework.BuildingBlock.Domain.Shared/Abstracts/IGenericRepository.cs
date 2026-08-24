using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

using Framework.BuildingBlock.Domain.Shared;

namespace Framework.BuildingBlock.Data;

public interface IGenericRepository<TEntity, TKey>
    where TEntity : class
{
    IQueryable<TEntity> GetQueryable();

    IQueryable<TEntity> GetQueryableIncludingDeleted();

    Task<TEntity?> FindAsync(TKey id, CancellationToken cancellationToken = default);

    Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default);

    Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<List<TEntity>> GetListAsync(CancellationToken cancellationToken = default);

    Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<PagedResult<TEntity>> GetPagedListAsync(
        int page,
        int pageSize,
        Expression<Func<TEntity, bool>>? predicate = null,
        string? sorting = null,
        CancellationToken cancellationToken = default);

    Task<PagedResult<TEntity>> PaginationAsync(FilterGroup filterGroup, IQueryable<TEntity>? externalQuery, int skip = 0, int maxResultCount = 10, string sort = "");
    Task<PagedResult<TEntity>> PaginationPagingAsync(FilterGroup filterGroup, IQueryable<TEntity>? externalQuery, int page = 1, int pageSize = 10, string sort = "");

    Task<long> CountAsync(CancellationToken cancellationToken = default);

    Task<long> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<TEntity> InsertAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default);

    Task InsertManyAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default);

    Task<TEntity> UpdateAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default);

    Task UpdateManyAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default);

    Task DeleteAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default);

    Task DeleteAsync(TKey id, bool autoSave = false, CancellationToken cancellationToken = default);

    Task DeleteManyAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default);
}

public interface IGenericRepository<TEntity> : IGenericRepository<TEntity, Guid>
    where TEntity : class
{
}
