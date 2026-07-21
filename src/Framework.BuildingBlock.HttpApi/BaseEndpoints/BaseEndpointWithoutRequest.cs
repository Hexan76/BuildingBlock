using Framework.BuildingBlock.Application.Contracts;
using MediatR;

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
    public override async Task<MessageContract<TResponse>> ExecuteAsync(CancellationToken ct)
    {
        // Build the TRequest manually (default constructor or overridden)
        var req = await CreateRequestAsync(ct);

        return await Mediator.Send(req, ct);
    }

    /// <summary>
    /// Override this in child classes to construct the request model.
    /// </summary>
    protected virtual Task<TRequest> CreateRequestAsync(CancellationToken ct)
    {
        var request = new TRequest();
        return Task.FromResult(request);
    }
}
