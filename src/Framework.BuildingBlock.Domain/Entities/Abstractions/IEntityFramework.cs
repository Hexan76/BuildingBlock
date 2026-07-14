using Volo.Abp.Domain.Entities;

namespace Framework.BuildingBlock.Entities;

public interface IEntityFramework : IEntity
{
}
public interface IEntityFramework<TKey> : IEntity<TKey>
{
}
