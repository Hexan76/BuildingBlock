namespace Framework.HttpClient.Options;

public class HttpServicesOptions
{
    public const string SectionName = "ExternalServices";

    public Dictionary<string, HttpClientItemServiceOptions> Services { get; set; } = new();
}

public class HttpClientItemServiceOptions
{
    public string BaseUrl { get; set; } = default!;

    public TimeSpan? Timeout { get; set; }

    public List<HttpHeaderOption> DefaultHeaders { get; set; } = [];
}

public class HttpHeaderOption
{
    public string Name { get; set; } = default!;
    public string Value { get; set; } = default!;
}
