namespace Framework.BuildingBlock.Domain.Shared;

public static class BuildingBlockErrorCodes
{
    public const string ResourceNotFound = "ResourceNotFound";
    public const string InsufficientStock = "InsufficientStock";
    public const string ConcurrencyConflict = "ConcurrencyConflict";
    public const string UpstreamServicesUnavailable = "UpstreamServicesUnavailable";
    public const string InvalidOrderState = "InvalidOrderState";
    public const string HttpClientFault = "HttpClientFault";
    public const string HttpClientTimeout = "HttpClientTimeout";
    public const string HttpClientTransport = "HttpClientTransport";
    public const string HttpClientSerialization = "HttpClientSerialization";
}
