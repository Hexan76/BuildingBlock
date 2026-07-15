namespace Framework.BuildingBlock.Domain.Shared;

public static class BuildingBlockErrorCodes
{
    public const string ResourceNotFound = "ResourceNotFound";
    public const string InsufficientStock = "InsufficientStock";
    public const string ConcurrencyConflict = "ConcurrencyConflict";
    public const string UpstreamServicesUnavailable = "UpstreamServicesUnavailable";
}
