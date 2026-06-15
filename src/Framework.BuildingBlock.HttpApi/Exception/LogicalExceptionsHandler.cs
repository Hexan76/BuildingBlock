using Framework.BuildingBlock.Domain.Shared;
using Microsoft.Extensions.Localization;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Framework.BuildingBlock.HttpApi;

[ExposeServices(typeof(IHashtExceptionHandler))]
public class LogicalExceptionsHandler : IHashtExceptionHandler, ITransientDependency
{
    private readonly IStringLocalizerFactory _localizerFactory;
    private IStringLocalizer L;

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

        string message = businessEx?.Message ?? "An error occurred.";
        string code = businessEx?.Code ?? "LOGICAL_ERROR";

        if (businessEx?.Data.Contains(ExceptionConstants.ResourceType) == true
            && businessEx.Data[ExceptionConstants.ResourceType] is Type resourceType)
        {
            if (resourceType != null)
            {
                L = _localizerFactory.Create(resourceType);
            }
            message = L[$"Exceptions.{message}"];
        }

        return CreateError(message, code, ex, sendExceptionsDetailsToClients);
    }

    // ---------------- Private ----------------

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
