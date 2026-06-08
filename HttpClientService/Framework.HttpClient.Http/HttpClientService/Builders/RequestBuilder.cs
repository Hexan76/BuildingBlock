using Framework.HttpClient.Abstractions;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Framework.HttpClient.Http;

public class RequestBuilder : IRequestBuilder
{

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public HttpRequestMessage Build<TRequest>(TRequest request, string contentType) where TRequest : IHttpRequest
    {
        var httpRequestResolveContext = ResolveRequestFields(request);
        var requestUri = BuildRequestUri(httpRequestResolveContext.FinalRoute, httpRequestResolveContext.QueryParams);
        var message = new HttpRequestMessage
        {
            Method = request.Method,
            RequestUri = requestUri
        };

        if (message.Method == HttpMethod.Post || message.Method == HttpMethod.Put || message.Method == HttpMethod.Patch)
        {
            if (httpRequestResolveContext.BodyContent.Count > 0)
            {
                var json = JsonSerializer.Serialize(
                    httpRequestResolveContext.BodyContent,
                    _jsonSerializerOptions);

                message.Content = new StringContent(
                    json,
                    Encoding.UTF8,
                    contentType);
            }
        }

        return message;
    }

    private Uri BuildRequestUri(string route, IDictionary<string, string> queryParams)
    {
        if (queryParams == null || queryParams.Count == 0)
            return new Uri(route, UriKind.Relative);

        var queryString = new FormUrlEncodedContent(queryParams).ReadAsStringAsync().Result;
        return new Uri($"{route}?{queryString}", UriKind.Relative);
    }

    public HttpRequestResolverContext ResolveRequestFields<TRequest>(TRequest request) where TRequest : IHttpRequest
    {
        var method = request.Method;
        var routeTemplate = request.Route;

        // Step 1: Exclude IRequest props (interface properties)
        var excluded = typeof(IHttpRequest)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToHashSet();

        // Step 2: Get all properties from the request and exclude IRequest properties
        var props = request.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && !excluded.Contains(p.Name))
            .Select(p => (p.Name, Value: p.GetValue(request)))
            .Where(p => p.Value != null)
            .ToList();

        // Step 3: Create a fast lookup dictionary for properties
        var propDict = props.ToDictionary(p => p.Name, p => p.Value, StringComparer.OrdinalIgnoreCase);

        // Step 4: Replace route placeholders with values from the request properties
        var usedInRoute = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var finalRoute = RouteParamRegex.Replace(routeTemplate, match =>
        {
            var key = match.Groups[1].Value;
            if (propDict.TryGetValue(key, out var val))
            {
                usedInRoute.Add(key);
                return Uri.EscapeDataString(Convert.ToString(val, CultureInfo.InvariantCulture)!);
            }
            return match.Value;
        });

        // Step 5: Get the remaining properties (those not used in the route)
        var remaining = propDict
            .Where(p => !usedInRoute.Contains(p.Key))
            .ToList();

        // Step 6: Apply properties to query parameters for GET method or body for POST/PUT/PATCH
        var queryParams = method == HttpMethod.Get
            ? remaining.ToDictionary(
                p => p.Key,
                p => Convert.ToString(p.Value, CultureInfo.InvariantCulture)!)
            : new Dictionary<string, string>();

        var bodyContent = method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Patch
            ? remaining.ToDictionary(p => p.Key, p => p.Value)
            : new Dictionary<string, object>();

        return new HttpRequestResolverContext(finalRoute, queryParams, bodyContent);
    }

    private static readonly Regex RouteParamRegex = new(@"{(\w+)}", RegexOptions.Compiled);
}
