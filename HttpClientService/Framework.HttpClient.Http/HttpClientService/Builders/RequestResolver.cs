using Framework.HttpClient.Abstractions;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Framework.HttpClient.Http;

public class RequestResolver : IRequestResolver
{
    public HttpRequestResolverContext ResolveRequestFields<TRequest>(TRequest request) where TRequest : IHttpRequest
    {
        var routeTemplate = request.Route;
        var method = request.Method;
        var excluded = typeof(IHttpRequest).GetProperties().Select(p => p.Name).ToHashSet();

        var props = request.GetType()
            .GetProperties()
            .Where(p => p.CanRead && !excluded.Contains(p.Name))
            .Select(p => (p.Name, Value: p.GetValue(request)))
            .Where(p => p.Value != null)
            .ToList();

        var propDict = props.ToDictionary(p => p.Name, p => p.Value, StringComparer.OrdinalIgnoreCase);

        var usedInRoute = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var finalRoute = Regex.Replace(routeTemplate, @"{(\w+)}", match =>
        {
            var key = match.Groups[1].Value;
            if (propDict.TryGetValue(key, out var val))
            {
                usedInRoute.Add(key);
                return Uri.EscapeDataString(Convert.ToString(val, CultureInfo.InvariantCulture)!);
            }
            return match.Value;
        });

        var remaining = propDict.Where(p => !usedInRoute.Contains(p.Key)).ToList();

        var queryParams = method == HttpMethod.Get
            ? remaining.ToDictionary(p => p.Key, p => Convert.ToString(p.Value, CultureInfo.InvariantCulture)!)
            : new Dictionary<string, string>();

        var bodyContent = method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Patch
            ? remaining.ToDictionary(p => p.Key, p => p.Value)
            : new Dictionary<string, object>();

        return new (finalRoute, queryParams, bodyContent);
    }
}
