using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

using Framework.HttpClient.Abstractions;

namespace Framework.HttpClient.Http;

public class RequestResolver : IRequestResolver
{
    private static readonly Regex RouteParamRegex = new(@"{(\w+)}", RegexOptions.Compiled);

    public HttpRequestResolverContext ResolveRequestFields<TRequest>(TRequest request)
        where TRequest : IHttpRequest
    {
        var method = request.Method;
        var routeTemplate = request.Route;

        var excluded = typeof(IHttpRequest)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var props = request.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && !excluded.Contains(p.Name))
            .Select(p => (p.Name, Value: p.GetValue(request)))
            .Where(p => p.Value != null)
            .ToList();

        var propDict = props.ToDictionary(
            p => p.Name,
            p => p.Value,
            StringComparer.OrdinalIgnoreCase);

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

        var remaining = propDict
            .Where(p => !usedInRoute.Contains(p.Key))
            .ToList();

        var queryParams = method == HttpMethod.Get
            ? remaining.ToDictionary(
                p => p.Key,
                p => Convert.ToString(p.Value, CultureInfo.InvariantCulture)!)
            : new Dictionary<string, string>();

        object? bodyContent = method == HttpMethod.Post ||
                              method == HttpMethod.Put ||
                              method == HttpMethod.Patch
            ? request
            : null;

        return new HttpRequestResolverContext(finalRoute, queryParams, bodyContent);
    }
}
