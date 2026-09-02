using Framework.BuildingBlock.Domain.Shared;

using Microsoft.Extensions.Localization;

using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Framework.BuildingBlock.HttpApi;

[ExposeServices(typeof(IHashtExceptionHandler))]
public class LogicalExceptionsHandler : IHashtExceptionHandler, ITransientDependency
{
    private readonly IStringLocalizerFactory _localizerFactory;

    public LogicalExceptionsHandler(IStringLocalizerFactory localizerFactory)
    {
        _localizerFactory = localizerFactory;
    }

    public bool CanHandle(Exception ex)
    {
        return ex is UserFriendlyException || ex is BusinessException;
    }

    public FrameworkRemoteErrorInfoDto Handle(
        Exception ex,
        bool sendExceptionsDetailsToClients,
        bool sendStackTraceToClients)
    {
        var businessEx = ex as BusinessException ?? ex as UserFriendlyException;

        var message = businessEx?.Message ?? "An error occurred.";
        var code = businessEx?.Code ?? "LOGICAL_ERROR";

        if (businessEx?.Data.Contains(ExceptionConstants.ResourceType) == true &&
            businessEx.Data[ExceptionConstants.ResourceType] is Type resourceType)
        {
            var localizer = _localizerFactory.Create(resourceType);

            var key = $"Exceptions.{message}";

            if (businessEx.Data[ExceptionConstants.Parameters] is object[] parameters)
            {
                message = localizer[key, parameters];
            }
            else
            {
                message = localizer[key];
            }
        }

        return CreateError(
            message,
            code,
            ex,
            sendExceptionsDetailsToClients);
    }

    private static FrameworkRemoteErrorInfoDto CreateError(
        string message,
        string code,
        Exception ex,
        bool sendDetails)
    {
        return new FrameworkRemoteErrorInfoDto(message, code)
        {
            Details = sendDetails
                ? ex.StackTrace
                : null
        };
    }
}
