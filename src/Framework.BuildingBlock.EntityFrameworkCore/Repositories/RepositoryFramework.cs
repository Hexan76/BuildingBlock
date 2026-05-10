using Framework.BuildingBlock.Entities;

using Volo.Abp.Domain.Repositories;

namespace Framework.BuildingBlock.Repositories;

public abstract class RepositoryFramework<TEntity>
    : RepositoryBase<TEntity>
    where TEntity : class, IEntityFramework
{
    protected RepositoryFramework(string providerName) : base(providerName)
    {
    }
}
