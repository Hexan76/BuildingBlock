using Framework.BuildingBlock.Application.Contracts;

using MediatR;

using Microsoft.AspNetCore.Http;

using static MassTransit.ValidationResultExtensions;

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

    //public override async Task HandleAsync(TRequest req, CancellationToken ct)
    //{
    //    var result = await Mediator.Send(req, ct);
    //    await Send.ResponseAsync(result, ResolveStatusCode(result), cancellation: ct);
    //}

    //private static int ResolveStatusCode(MessageContract<TResponse> result)
    //    => result is ApiResult<TResponse> { Success: false, Error.HttpCode: > 0 } failed
    //        ? failed.Error.HttpCode
    //        : StatusCodes.Status200OK;
}
