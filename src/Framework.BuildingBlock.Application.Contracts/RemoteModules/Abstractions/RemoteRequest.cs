using System.Text.Json;

namespace Framework.BuildingBlock.Application.Contracts;


/// <summary>
/// Default wrapper used for dynamic parameter passing.
/// </summary>
public class RemoteRequest
{
    public Dictionary<string, object?> Parameters { get; set; } = new();
    public T Get<T>(string key)
    {
        if (Parameters.TryGetValue(key, out var value))
        {
            var json = JsonSerializer.Serialize(value);
            return JsonSerializer.Deserialize<T>(json)!;
        }
        throw new KeyNotFoundException($"Parameter '{key}' not found in RemoteRequest.");
    }
}
