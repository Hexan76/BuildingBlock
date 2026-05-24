using Framework.BuildingBlock.Domain.Shared;
using Microsoft.Extensions.Options;

using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Framework.BuildingBlock.HttpApi;

public class DefaultExceptionLocalizationMapper : IExceptionLocalizationMapper, ITransientDependency
{
    private readonly ExceptionLocalizationOptions _maps;
    public DefaultExceptionLocalizationMapper(IOptions<ExceptionLocalizationOptions> options)
    {
        _maps = options.Value;
    }
    public string? GetLocalizationKey(Exception exception)
    {
        if (exception is BusinessException bEx) return bEx.Message;
        if (exception is UserFriendlyException ufEx) return ufEx.Message;

        return exception.GetType().Name;
    }

    public Type? GetResourceType(Exception exception)
    {
        var typeName = exception.GetType().Name;
        if (_maps.ExceptionResourceMap.TryGetValue(typeName, out var resourceType))
        {
            return resourceType;
        }
        return _maps.DefaultResourceType;
    }
}
