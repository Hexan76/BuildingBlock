using Framework.BuildingBlock.Domain.Shared;

namespace Volo.Abp;

public static class ExceptionExtensions
{
    /// <summary>
    /// Attaches a localization resource type to this exception.
    /// </summary>
    public static BusinessException WithResource(
        this BusinessException exception,
        Type resourceType)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(resourceType);

        exception.Data[ExceptionConstants.ResourceType] = resourceType;

        return exception;
    }

    /// <summary>
    /// Attaches a localization resource type to this exception.
    /// </summary>
    public static UserFriendlyException WithResource(
        this UserFriendlyException exception,
        Type resourceType)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(resourceType);

        exception.Data[ExceptionConstants.ResourceType] = resourceType;

        return exception;
    }

    /// <summary>
    /// Attaches localization parameters to this exception.
    /// </summary>
    public static BusinessException WithParameters(
        this BusinessException exception,
        params object[] parameters)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(parameters);

        exception.Data[ExceptionConstants.Parameters] = parameters;

        return exception;
    }

    /// <summary>
    /// Attaches localization parameters to this exception.
    /// </summary>
    public static UserFriendlyException WithParameters(
        this UserFriendlyException exception,
        params object[] parameters)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(parameters);

        exception.Data[ExceptionConstants.Parameters] = parameters;

        return exception;
    }

    /// <summary>
    /// Retrieves the localization resource type attached to this exception.
    /// </summary>
    public static Type? GetResourceType(this BusinessException exception)
    {
        return exception?.Data[ExceptionConstants.ResourceType] as Type;
    }

    /// <summary>
    /// Retrieves the localization resource type attached to this exception.
    /// </summary>
    public static Type? GetResourceType(this UserFriendlyException exception)
    {
        return exception?.Data[ExceptionConstants.ResourceType] as Type;
    }

    /// <summary>
    /// Retrieves the localization parameters attached to this exception.
    /// </summary>
    public static object[] GetParameters(this BusinessException exception)
    {
        return exception?.Data[ExceptionConstants.Parameters] as object[]
               ?? [];
    }

    /// <summary>
    /// Retrieves the localization parameters attached to this exception.
    /// </summary>
    public static object[] GetParameters(this UserFriendlyException exception)
    {
        return exception?.Data[ExceptionConstants.Parameters] as object[]
               ?? [];
    }
}
