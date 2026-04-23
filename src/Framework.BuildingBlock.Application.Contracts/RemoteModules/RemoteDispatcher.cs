using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.BuildingBlock.Application.Contracts;

/// <summary>
/// Default in-memory implementation of IRemoteDispatcher.
/// </summary>
public class RemoteDispatcher : IRemoteDispatcher
{
    private readonly Dictionary<string, IRemoteModuleHandler> _handlers;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public RemoteDispatcher(IEnumerable<IRemoteModuleHandler> handlers)
    {
        // build lookup map (case-insensitive)
        _handlers = handlers.ToDictionary(h => h.FullRemoteName, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<TResult?> DispatchAsync<TResult>(
        string moduleName,
        string handlerName,
        object parameters,
        CancellationToken token = default)
    {
        string fullRemoteName = $"{moduleName}.{handlerName}";

        if (!_handlers.TryGetValue(fullRemoteName, out var handler))
            throw new InvalidOperationException($"Remote handler '{fullRemoteName}' not found.");

        // build RemoteRequest object
        var request = new RemoteRequest();
        foreach (var prop in parameters.GetType().GetProperties())
            request.Parameters[prop.Name] = prop.GetValue(parameters);

        // call handler
        var result = await handler.HandleAsync(request, token);

        if (result == null)
            return default;

        var json = JsonSerializer.Serialize(result, _jsonOptions);
        return JsonSerializer.Deserialize<TResult>(json, _jsonOptions);
    }
}
