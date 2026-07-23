namespace Framework.HttpClient.Options;

public class HttpServicesOptions
{
    public const string SectionName = "ExternalServices";

    public Dictionary<string, HttpClientItemServiceOptions> Services { get; set; } = new();
}


public class HttpClientItemServiceOptions
{
    public string BaseUrl { get; set; } = default!;
}
