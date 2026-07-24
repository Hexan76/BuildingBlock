namespace Framework.BuildingBlock.Data;

public class EntityNotFoundException : Exception
{
    public Type EntityType { get; }

    public object? Id { get; }

    public EntityNotFoundException(Type entityType, object? id)
        : base(id is null
            ? $"There is no entity of type '{entityType.FullName}' matching the given criteria."
            : $"There is no entity of type '{entityType.FullName}' with id '{id}'.")
    {
        EntityType = entityType;
        Id = id;
    }
}
