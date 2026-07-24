namespace Framework.BuildingBlock.Domain;

public static class CurrentUserContext
{
    private static readonly AsyncLocal<Guid?> _userId = new();

    public static Guid? UserId
    {
        get => _userId.Value;
        set => _userId.Value = value;
    }
}
