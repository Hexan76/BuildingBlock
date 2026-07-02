using MediatR;

namespace Framework.BuildingBlock.Application.Contracts;

public interface IFrameworkRequest : IRequest
{
}

public interface IFrameworkRequest<TResponse> : IRequest<ApiResult<TResponse>>
    where TResponse : class
{
}
