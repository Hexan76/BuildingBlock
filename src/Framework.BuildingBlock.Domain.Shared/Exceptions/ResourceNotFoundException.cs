using Volo.Abp;

namespace Framework.BuildingBlock.Domain.Shared.Exceptions;

public class ResourceNotFoundException : UserFriendlyException
{
    public ResourceNotFoundException(string resourceName, object resourceId)
        : base(
            BuildingBlockErrorCodes.ResourceNotFound,
            $"{resourceName} '{resourceId}' was not found.")
    {
        ResourceName = resourceName;
        ResourceId = resourceId;
    }

    public ResourceNotFoundException(string message)
        : base(BuildingBlockErrorCodes.ResourceNotFound, message)
    {
        ResourceName = string.Empty;
        ResourceId = string.Empty;
    }

    public string ResourceName { get; }
    public object ResourceId { get; }
}
