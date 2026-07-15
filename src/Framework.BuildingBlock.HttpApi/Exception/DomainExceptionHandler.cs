using Framework.BuildingBlock.Domain.Shared;
using Framework.BuildingBlock.Domain.Shared.Exceptions;
using Volo.Abp.DependencyInjection;

namespace Framework.BuildingBlock.HttpApi;

[ExposeServices(typeof(IHashtExceptionHandler))]
public class DomainExceptionHandler : IHashtExceptionHandler, ITransientDependency
{
    public bool CanHandle(Exception ex)
    {
        return ex is ResourceNotFoundException
               || ex is InsufficientStockException
               || ex is ConcurrencyConflictException
               || ex is UpstreamServicesUnavailableException
               || ex is InvalidOrderStateException;
    }

    public FrameworkRemoteErrorInfoDto Handle(
        Exception ex,
        bool sendExceptionsDetailsToClients,
        bool sendStackTraceToClients)
    {
        string code = ex switch
        {
            ResourceNotFoundException => BuildingBlockErrorCodes.ResourceNotFound,
            InsufficientStockException => BuildingBlockErrorCodes.InsufficientStock,
            ConcurrencyConflictException => BuildingBlockErrorCodes.ConcurrencyConflict,
            UpstreamServicesUnavailableException => BuildingBlockErrorCodes.UpstreamServicesUnavailable,
            InvalidOrderStateException => BuildingBlockErrorCodes.InvalidOrderState,
            _ => "DOMAIN_ERROR"
        };

        return new FrameworkRemoteErrorInfoDto(ex.Message, code)
        {
            Details = sendExceptionsDetailsToClients ? ex.ToString() : null
        };
    }
}
