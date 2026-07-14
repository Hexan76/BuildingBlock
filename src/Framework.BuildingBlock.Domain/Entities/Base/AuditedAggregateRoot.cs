namespace Framework.BuildingBlock.Entities;

public abstract class AuditedAggregateRoot<TKey> : BuildingBlockEntity<TKey>, IEntityFramework<TKey>, IHasAuditProperties, IHasConcurrencyStamp
{
    public DateTime CreationTime { get; set; }
    public DateTime? ModificationTime { get; set; }
    public Guid? CreatorId { get; set; }
    public Guid? ModifierId { get; set; }
}

public abstract class AuditedAggregateRoot : AuditedAggregateRoot<Guid>
{
}
