using Framework.BuildingBlock.Application.Contracts;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Framework.BuildingBlock.HttpApi;

public abstract class BaseEndpointWithoutRequest<TRequest, TResponse> : EndpointWithoutRequest<MessageContract<TResponse>>
    where TRequest : IRequest<MessageContract<TResponse>>, new()
    where TResponse : class
{
    public IMediator Mediator { get; set; } = null!;

    public override void Configure()
    {
        Version(1);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var req = await CreateRequestAsync(ct);
        var result = await Mediator.Send(req, ct);
        await Send.ResponseAsync(result, ResolveStatusCode(result), cancellation: ct);
    }

    /// <summary>
    /// Override this in child classes to construct the request model.
    /// </summary>
    protected virtual Task<TRequest> CreateRequestAsync(CancellationToken ct)
    {
        var request = new TRequest();
        return Task.FromResult(request);
    }

    private static int ResolveStatusCode(MessageContract<TResponse> result)
        => result is ApiResult<TResponse> { Success: false, Error.HttpCode: > 0 } failed
            ? failed.Error.HttpCode
            : StatusCodes.Status200OK;
}
