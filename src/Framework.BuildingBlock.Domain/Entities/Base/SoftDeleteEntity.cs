using System;

namespace Framework.BuildingBlock.Entities;

public abstract class SoftDeleteEntity<TKey> : BuildingBlockEntity<TKey>, IHasSoftDelete
{
    public virtual bool IsDeleted { get; set; }

    public virtual DateTime? DeletionTime { get; set; }

    public virtual Guid? DeleterId { get; set; }
}

public abstract class SoftDeleteEntity : SoftDeleteEntity<Guid>
{
}
