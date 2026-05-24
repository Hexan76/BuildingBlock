using Framework.BuildingBlock.Domain.Shared;
using Volo.Abp.DependencyInjection;

namespace Framework.BuildingBlock.HttpApi;

[ExposeServices(typeof(IHashtExceptionHandler))]

public class DefaultExceptionHandler : IHashtExceptionHandler, ITransientDependency
{
    public bool CanHandle(Exception ex) => true; // fallback

    public HashtRemoteErrorInfoDto Handle(Exception ex, bool SendExceptionsDetailsToClients, bool SendStackTraceToClients)
    {
        HashtRemoteErrorInfoDto errorInfoDto = new(ex.Message, ex.InnerException?.Message)
        {
            Details = SendExceptionsDetailsToClients ? ex.ToString() : null,

        };

        return errorInfoDto;
    }

}