using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using Framework.HttpClient.Abstractions;

using Microsoft.Extensions.Options;

namespace Framework.HttpClient.Http;

public class RequestBuilder(
    IRequestResolver requestResolver,
    IOptions<HttpClientServiceOptions> options) : IRequestBuilder
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = CreateSerializerOptions(options.Value.JsonSerializerOptions);

    public HttpRequestMessage Build<TRequest>(TRequest request, string contentType)
        where TRequest : IHttpRequest
    {
        var resolved = requestResolver.ResolveRequestFields(request);
        var requestUri = BuildRequestUri(resolved.FinalRoute, resolved.QueryParams);

        var message = new HttpRequestMessage
        {
            Method = request.Method,
            RequestUri = requestUri
        };

        if (message.Method == HttpMethod.Post ||
            message.Method == HttpMethod.Put ||
            message.Method == HttpMethod.Patch)
        {
            if (resolved.BodyContent is not null)
            {
                var json = JsonSerializer.Serialize(resolved.BodyContent, _jsonSerializerOptions);
                message.Content = new StringContent(json, Encoding.UTF8, contentType);
            }
        }

        return message;
    }

    private static Uri BuildRequestUri(string route, IDictionary<string, string> queryParams)
    {
        if (queryParams is null || queryParams.Count == 0)
            return new Uri(route, UriKind.Relative);

        var query = string.Join("&", queryParams.Select(p =>
            $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

        return new Uri($"{route}?{query}", UriKind.Relative);
    }

    private static JsonSerializerOptions CreateSerializerOptions(JsonSerializerOptions? source)
    {
        var json = source is null
            ? new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }
            : new JsonSerializerOptions(source);

        json.TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers = { ExcludeIHttpRequestProperties }
        };

        return json;
    }

    private static void ExcludeIHttpRequestProperties(JsonTypeInfo typeInfo)
    {
        if (!typeof(IHttpRequest).IsAssignableFrom(typeInfo.Type))
            return;

        var ignoredProperties = typeof(IHttpRequest)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var propertiesToRemove = typeInfo.Properties
            .Where(p => ignoredProperties.Contains(p.Name))
            .ToList();

        foreach (var property in propertiesToRemove)
            typeInfo.Properties.Remove(property);
    }
}
