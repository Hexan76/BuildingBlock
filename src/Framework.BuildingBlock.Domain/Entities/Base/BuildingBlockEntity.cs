namespace Framework.BuildingBlock.Entities;

public abstract class BuildingBlockEntity<TKey> : Entity<TKey>, IEntityFramework<TKey>, IHasConcurrencyStamp
{
    public virtual string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString("N");
}

public abstract class BuildingBlockEntity : BuildingBlockEntity<Guid>
{
}
