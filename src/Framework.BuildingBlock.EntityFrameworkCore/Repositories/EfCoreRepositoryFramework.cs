using System.Linq.Dynamic.Core;

using Framework.BuildingBlock.Domain.Shared;

using Microsoft.EntityFrameworkCore;

using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Framework.BuildingBlock.Repositories;

public abstract class EfCoreRepositoryFramework<TDbContext, TEntity>
    : EfCoreRepository<TDbContext, TEntity>
    where TDbContext : DbContext, IEFCoreDbContextFramework
    where TEntity : class, IEntity
{
    protected EfCoreRepositoryFramework(
        IDbContextProvider<TDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}

public abstract class EfCoreRepositoryFramework<TDbContext, TEntity, TKey>
    : EfCoreRepository<TDbContext, TEntity,TKey>, IRepositoryFramework<TEntity, TKey>
    where TDbContext : DbContext, IEFCoreDbContextFramework
    where TEntity : class, IEntity<TKey>
{
    protected EfCoreRepositoryFramework(
        IDbContextProvider<TDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<PagedResult<TEntity>> PaginationAsync(
        FilterGroup filterGroup,
        IQueryable<TEntity> externalQuery,
        int skip = 0,
        int maxResultCount = 10,
        string sort = "")
    {
        var query = externalQuery ?? (await GetQueryableAsync())
            .ApplyFilter(filterGroup)
            .ApplySort(sort);

        var totalCount = await query.CountAsync();

        var pagedQuery = query
            .Skip(skip)
            .Take(maxResultCount);

        var currentPage = maxResultCount <= 0
            ? 1
            : (skip / maxResultCount) + 1;

        var pageCount = maxResultCount <= 0
            ? 1
            : (int)Math.Ceiling((double)totalCount / maxResultCount);

        return new PagedResult<TEntity>
        {
            Queryable = pagedQuery,
            RowCount = totalCount,
            PageCount = pageCount,
            CurrentPage = currentPage,
            PageSize = maxResultCount
        };
    }
    public virtual async Task<PagedResult<TEntity>> PaginationPagingAsync(
        FilterGroup filterGroup,
        IQueryable<TEntity> externalQuery,
        int page = 1,
        int pageSize = 10,
        string sort = "")
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = externalQuery ?? (await GetQueryableAsync())
            .ApplyFilter(filterGroup)
            .ApplySort(sort);

        var totalCount = await query.CountAsync();

        var skip = (page - 1) * pageSize;

        var pagedQuery = query
            .Skip(skip)
            .Take(pageSize);

        var pageCount = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PagedResult<TEntity>
        {
            Queryable = pagedQuery,
            RowCount = totalCount,
            PageCount = pageCount,
            CurrentPage = page,
            PageSize = pageSize
        };
    }
}
