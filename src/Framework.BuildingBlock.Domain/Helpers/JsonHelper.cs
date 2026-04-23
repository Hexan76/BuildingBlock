using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Framework.BuildingBlock.Domain;

public static class JsonHelper
{
    /// <summary>
    /// Loads and deserializes a JSON file from disk into the given type.
    /// </summary>
    /// <typeparam name="T">Type to deserialize into.</typeparam>
    /// <param name="path">Relative or absolute path to the JSON file.</param>
    /// <returns>Deserialized object of type T.</returns>
    public static async Task<T> LoadAsync<T>(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path));

        var fullPath = Path.GetFullPath(path);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"JSON file not found: {fullPath}");

        var json = await File.ReadAllTextAsync(fullPath);

        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        }) ?? throw new InvalidOperationException($"Failed to deserialize file '{fullPath}' into {typeof(T).Name}");
    }
}
