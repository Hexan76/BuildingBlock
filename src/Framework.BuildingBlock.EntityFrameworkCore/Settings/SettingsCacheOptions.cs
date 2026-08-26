namespace Framework.BuildingBlock.Settings;

public sealed class SettingsCacheOptions
{
    public string AppKey { get; set; } = default!;
    public int ExpirationMinutes { get; set; } = 30;
}
