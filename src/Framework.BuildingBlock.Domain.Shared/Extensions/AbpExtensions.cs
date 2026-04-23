using Framework.BuildingBlock.Domain.Shared;

namespace Volo.Abp
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Attaches a localization resource type to this exception.
        /// </summary>
        public static BusinessException WithResource(this BusinessException exception, Type resourceType)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            if (resourceType == null)
                throw new ArgumentNullException(nameof(resourceType));

            exception.Data[ExceptionConstants.ResourceType] = resourceType;
            return exception;
        }
        /// <summary>
        /// Attaches a localization resource type to this exception.
        /// </summary>
        public static UserFriendlyException WithResource(this UserFriendlyException exception, Type resourceType)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            if (resourceType == null)
                throw new ArgumentNullException(nameof(resourceType));

            exception.Data[ExceptionConstants.ResourceType] = resourceType;
            return exception;
        }

        /// <summary>
        /// Retrieves the localization resource type attached to this exception, if any.
        /// </summary>
        public static Type? GetResourceType(this BusinessException exception)
        {
            if (exception == null)
                return null;

            if (exception.Data.Contains(ExceptionConstants.ResourceType))
                return exception.Data[ExceptionConstants.ResourceType] as Type;

            return null;
        }
        /// <summary>
        /// Retrieves the localization resource type attached to this exception, if any.
        /// </summary>
        public static Type? GetResourceType(this UserFriendlyException exception)
        {
            if (exception == null)
                return null;

            if (exception.Data.Contains(ExceptionConstants.ResourceType))
                return exception.Data[ExceptionConstants.ResourceType] as Type;

            return null;
        }
    }
}
