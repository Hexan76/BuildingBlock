namespace Framework.BuildingBlock.Entities;

public abstract class FullAuditedEntity<TKey> : SoftDeleteEntity<TKey>
{
}

public abstract class FullAuditedEntity : FullAuditedEntity<Guid>
{
}
