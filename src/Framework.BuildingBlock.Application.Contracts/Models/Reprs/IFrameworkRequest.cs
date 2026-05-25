using MediatR;

namespace Framework.BuildingBlock.Application.Contracts;

public interface IFrameworkRequest : IRequest
{
}

public interface IFrameworkRequest<TResponse> : IRequest<MessageContract<TResponse>>
    where TResponse : class
{
}
