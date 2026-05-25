using System.Text.Json;

namespace Framework.BuildingBlock.Application.Contracts;

/// <summary>
/// Strongly typed base class to simplify IRemoteModuleHandler implementation.
/// Handles JSON mapping from RemoteRequest automatically.
/// </summary>
public abstract class RemoteModuleHandlerBase<TRequest, TResponse> : IRemoteModuleHandler<TRequest, TResponse>
{
    public abstract string Name { get; }
    public abstract string Module { get; }
    protected virtual JsonSerializerOptions JsonOptions => new() { PropertyNameCaseInsensitive = true };

    public abstract Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);

    async Task<object?> IRemoteModuleHandler.HandleAsync(RemoteRequest request, CancellationToken cancellationToken)
    {
        // convert RemoteRequest -> TRequest
        var json = JsonSerializer.Serialize(request.Parameters, JsonOptions);
        var typedReq = JsonSerializer.Deserialize<TRequest>(json, JsonOptions)!;
        var result = await HandleAsync(typedReq, cancellationToken);
        return result;
    }
}
