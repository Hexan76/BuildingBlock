using Framework.BuildingBlock.Entities;

using Microsoft.EntityFrameworkCore;

using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Framework.BuildingBlock.Repositories;

public abstract class EfCoreRepositoryFramework<TDbContext, TEntity>
    : EfCoreRepository<TDbContext, TEntity>
    where TDbContext : DbContext,IEFCoreDbContextFramework
    where TEntity : class, IEntityFramework
{
    protected EfCoreRepositoryFramework(IDbContextProvider<TDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }
}
