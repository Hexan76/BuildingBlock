namespace Framework.BuildingBlock.Application.Contracts;

public interface IRemoteDispatcher
{
    Task<TResult?> DispatchAsync<TResult>(
        string moduleName,
        string handlerName,
        object parameters,
        CancellationToken token = default);
}
