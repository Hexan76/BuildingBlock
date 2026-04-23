using System.Text.Json;

namespace Framework.BuildingBlock.Application.Contracts;


/// <summary>
/// Marker interface for all remote handlers (used for DI registration).
/// </summary>
public interface IRemoteModuleHandler
{
    string Name { get; }
    string Module { get; }
    string FullRemoteName => $"{Module}.{Name}";
    Task<object?> HandleAsync(RemoteRequest request, CancellationToken cancellationToken);
}


/// <summary>
/// Strongly typed remote handler interface.
/// Each module implements this for its own commands/queries.
/// </summary>
public interface IRemoteModuleHandler<TRequest, TResponse> : IRemoteModuleHandler
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
    async Task<object?> IRemoteModuleHandler.HandleAsync(RemoteRequest request, CancellationToken cancellationToken)
    {
        JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        // convert RemoteRequest -> TRequest
        var json = JsonSerializer.Serialize(request.Parameters, _jsonOptions);
        var typedReq = JsonSerializer.Deserialize<TRequest>(json, _jsonOptions)!;
        var result = await HandleAsync(typedReq, cancellationToken);
        return result;
    }

}