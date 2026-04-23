using Framework.BuildingBlock.Application.Contracts;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Framework.BuildingBlock.HttpApi;

public abstract class BaseEndpointLocalizer<TRequest, TResponse, TResource> : Endpoint<TRequest, MessageContract<TResponse>>
where TRequest : class, IRequest<MessageContract<TResponse>>
where TResponse : class
{
    public IMediator Mediator { get; set; } = null!;
    public IStringLocalizer<TResource> L { get; set; } = null!;

    public override async Task<MessageContract<TResponse>> ExecuteAsync(TRequest req, CancellationToken ct)
    {
        return await Mediator.Send(req, ct);
    }


    public override void OnValidationFailed()
    {

        foreach (var failure in ValidationFailures)
        {
            if (failure.CustomState is ValidationMessageInfo info)
            {
                failure.ErrorMessage = L[info.MessageKey, info.Args];
            }
            else
            {
                failure.ErrorMessage = L[failure.ErrorMessage];
            }
        }
    }
}