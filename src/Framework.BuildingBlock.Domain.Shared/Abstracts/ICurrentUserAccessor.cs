namespace Framework.BuildingBlock.Data;

/// <summary>
/// Provides the identifier of the current user.
/// Used for filling audit properties.
/// </summary>
public interface ICurrentUserAccessor
{
    Guid? UserId { get; }
}


/// <summary>
/// Default no-op implementation.
/// </summary>
public sealed class NullCurrentUserAccessor : ICurrentUserAccessor
{
    public static readonly NullCurrentUserAccessor Instance = new();

    private NullCurrentUserAccessor()
    {
    }

    public Guid? UserId => null;
}
