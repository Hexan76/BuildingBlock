using System;

namespace Framework.BuildingBlock.Entities;

public abstract class BuildingBlockAggregateRoot<TKey> : BuildingBlockEntity<TKey>, IEntityFramework<TKey>, IHasAuditProperties, IHasConcurrencyStamp
{
    public virtual DateTime CreationTime { get; set; } = DateTime.UtcNow;

    public virtual DateTime? ModificationTime { get; set; }

    public virtual Guid? CreatorId { get; set; }

    public virtual Guid? ModifierId { get; set; }
}

public abstract class BuildingBlockAggregateRoot : BuildingBlockAggregateRoot<Guid>
{
}
