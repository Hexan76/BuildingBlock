using System.Collections;
using System.Reflection;
using System.Text;
using System.Text.Json;

using Framework.HttpClient.Abstractions;

using Microsoft.Extensions.Options;

namespace Framework.HttpClient.Http;

public class FormContentBuilder(IOptions<HttpClientServiceOptions> options) : IFormContentBuilder
{
    private readonly HttpClientServiceOptions _httpClientServiceOptions = options.Value;

    public HttpContent Build(object bodyContent, string contentType)
    {
        if (string.Equals(contentType, "multipart/form-data", StringComparison.OrdinalIgnoreCase))
            return BuildMultipartContent(bodyContent);

        var json = JsonSerializer.Serialize(bodyContent, _httpClientServiceOptions.JsonSerializerOptions);
        return new StringContent(json, Encoding.UTF8, contentType);
    }

    private static MultipartFormDataContent BuildMultipartContent(object bodyContent)
    {
        var formData = new MultipartFormDataContent();

        foreach (var (key, value) in EnumerateFields(bodyContent))
        {
            if (value is null)
                continue;

            switch (value)
            {
                case Stream stream:
                    formData.Add(new StreamContent(stream), key, "file");
                    break;
                case byte[] bytes:
                    formData.Add(new ByteArrayContent(bytes), key, "file");
                    break;
                default:
                    formData.Add(new StringContent(value.ToString() ?? string.Empty), key);
                    break;
            }
        }

        return formData;
    }

    private static IEnumerable<(string Key, object? Value)> EnumerateFields(object bodyContent)
    {
        if (bodyContent is IDictionary<string, object?> genericNullable)
        {
            foreach (var pair in genericNullable)
                yield return (pair.Key, pair.Value);
            yield break;
        }

        if (bodyContent is IDictionary<string, object> generic)
        {
            foreach (var pair in generic)
                yield return (pair.Key, pair.Value);
            yield break;
        }

        if (bodyContent is IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                if (entry.Key is string key)
                    yield return (key, entry.Value);
            }

            yield break;
        }

        var excluded = typeof(IHttpRequest)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var properties = bodyContent
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && !excluded.Contains(p.Name));

        foreach (var property in properties)
            yield return (property.Name, property.GetValue(bodyContent));
    }
}
