namespace Framework.HttpClient.Abstractions;

public sealed class HttpClientErrorPolicy
{
    private readonly List<StatusMapRule> _rules = [];
    private int _defaultPublicStatus = 502;

    public static HttpClientErrorPolicy CreateDefault()
        => new HttpClientErrorPolicy()
            .MapRange(400, 499)
            .MapRange(500, 599, 503)
            .DefaultTo(502);

    /// <summary>Map an upstream status to the same public status.</summary>
    public HttpClientErrorPolicy Propagate(params int[] statusCodes)
    {
        foreach (var status in statusCodes)
            Map(status, status);

        return this;
    }

    public HttpClientErrorPolicy Map(int upstreamStatus, int publicStatus)
    {
        _rules.Add(new StatusMapRule(upstreamStatus, upstreamStatus, publicStatus));
        return this;
    }

    /// <param name="publicStatus">
    /// Public status to return. When omitted, the upstream status is propagated.
    /// </param>
    public HttpClientErrorPolicy MapRange(int fromInclusive, int toInclusive, int? publicStatus = null)
    {
        _rules.Add(new StatusMapRule(fromInclusive, toInclusive, publicStatus));
        return this;
    }

    public HttpClientErrorPolicy DefaultTo(int publicStatus)
    {
        _defaultPublicStatus = publicStatus;
        return this;
    }

    public int ResolvePublicStatus(int upstreamStatus)
    {
        for (var i = _rules.Count - 1; i >= 0; i--)
        {
            var rule = _rules[i];
            if (upstreamStatus >= rule.From && upstreamStatus <= rule.To)
                return rule.PublicStatus ?? upstreamStatus;
        }

        return _defaultPublicStatus;
    }

    private readonly record struct StatusMapRule(int From, int To, int? PublicStatus);
}
