using System.Linq.Dynamic.Core;

using Framework.BuildingBlock.Domain.Shared;

using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace Framework.BuildingBlock.Repositories;

public interface IRepositoryFramework<TEntity> : IRepository<TEntity>
    where TEntity : class, IEntity
{
    Task<PagedResult<TEntity>> PaginationAsync(FilterGroup filterGroup, IQueryable<TEntity>? externalQuery, int skip = 0, int maxResultCount = 10, string sort = "");
    Task<PagedResult<TEntity>> PaginationPagingAsync(FilterGroup filterGroup, IQueryable<TEntity>? externalQuery, int page = 1, int pageSize = 10, string sort = "");
}
public interface IRepositoryFramework<TEntity,TKey> : IRepository<TEntity,TKey>
    where TEntity : class, IEntity<TKey>
{
    Task<PagedResult<TEntity>> PaginationAsync(FilterGroup filterGroup, IQueryable<TEntity>? externalQuery, int skip = 0, int maxResultCount = 10, string sort = "");
    Task<PagedResult<TEntity>> PaginationPagingAsync(FilterGroup filterGroup, IQueryable<TEntity>? externalQuery, int page = 1, int pageSize = 10, string sort = "");

}

