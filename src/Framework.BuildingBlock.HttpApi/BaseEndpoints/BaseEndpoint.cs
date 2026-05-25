using Framework.BuildingBlock.Application.Contracts;
using MediatR;

namespace Framework.BuildingBlock.HttpApi;

public abstract class BaseEndpoint<TRequest, TResponse> : Endpoint<TRequest, MessageContract<TResponse>>
where TRequest : class, IRequest<MessageContract<TResponse>>
where TResponse : class
{
    public IMediator Mediator { get; set; } = null!;

    public override void Configure()
    {
        Version(1);
    }

    public override async Task<MessageContract<TResponse>> ExecuteAsync(TRequest req, CancellationToken ct)
    {
        return await Mediator.Send(req, ct);
    }
}
