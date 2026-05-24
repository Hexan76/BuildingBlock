namespace Framework.BuildingBlock.Domain;

public static class BuildingBlockDbProperties
{
    public static string DbTablePrefix { get; set; } = "BuildingBlock";

    public static string? DbSchema { get; set; } = null;

    public const string ConnectionStringName = "BuildingBlock";
}
