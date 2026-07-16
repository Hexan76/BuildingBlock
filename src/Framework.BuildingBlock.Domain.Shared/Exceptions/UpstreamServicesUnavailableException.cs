using Volo.Abp;

namespace Framework.BuildingBlock.Domain.Shared.Exceptions;

public class UpstreamServicesUnavailableException : UserFriendlyException
{
    public UpstreamServicesUnavailableException()
        : base(
            BuildingBlockErrorCodes.UpstreamServicesUnavailable,
            "Both upstream services failed.")
    {
    }

    public UpstreamServicesUnavailableException(string message)
        : base(BuildingBlockErrorCodes.UpstreamServicesUnavailable, message)
    {
    }
}
