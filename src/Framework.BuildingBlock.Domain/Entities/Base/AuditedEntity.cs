namespace Framework.BuildingBlock.Entities;

public abstract class AuditedEntity<TKey> : BuildingBlockEntity<TKey>
{
}

public abstract class AuditedEntity : AuditedEntity<Guid>
{
}
