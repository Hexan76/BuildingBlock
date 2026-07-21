namespace Framework.BuildingBlock.Data;

/// <summary>
/// Provides the identifier of the current user so that audit properties
/// (CreatorId / ModifierId / DeleterId) can be filled automatically.
/// This abstraction is intentionally free of any ABP dependency; provide an
/// implementation in the host application (or rely on the built-in no-op one).
/// </summary>
public interface ICurrentUserAccessor
{
    Guid? UserId { get; }
}

/// <summary>
/// Default no-op implementation used when the host application does not
/// register a real current-user accessor.
/// </summary>
public sealed class NullCurrentUserAccessor : ICurrentUserAccessor
{
    public static readonly NullCurrentUserAccessor Instance = new();

    public Guid? UserId => null;
}
