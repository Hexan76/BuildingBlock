namespace Framework.BuildingBlock.Application.Contracts;

public abstract class BaseEntityChangeSet<TEntity>
{
    public List<TEntity> Added { get; set; } = new List<TEntity>();
    public List<TEntity> Updated { get; set; } = new List<TEntity>();

}
public class EntityChangeSet<TEntity, TKey> : BaseEntityChangeSet<TEntity>
{
    public virtual List<TKey> Deleted { get; set; } = new List<TKey>();
}

public class FullEntityChangeSet<TEntity> : BaseEntityChangeSet<TEntity>
{
    public virtual List<Guid> Deleted { get; set; } = new List<Guid>();
}
