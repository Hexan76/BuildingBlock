using MediatR;

namespace Framework.BuildingBlock.Application.Contracts;

public interface IFrameworkRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, MessageContract<TResponse>>
    where TRequest : IFrameworkRequest<TResponse>
    where TResponse : class
{
}
