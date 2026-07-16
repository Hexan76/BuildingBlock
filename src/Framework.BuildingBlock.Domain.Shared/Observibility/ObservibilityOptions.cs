namespace Framework.Observability;

public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    public bool Enabled { get; set; } = true;

    public string ServiceName { get; set; } = "";

    public string ServiceNamespace { get; set; } = "DefaultNameSpace";

    public string ServiceVersion { get; set; } = "1.0.0";

    public string Environment { get; set; } = "Development";

    public string OtlpEndpoint { get; set; } = "http://otel-collector:4317";

    public bool UseGrpc { get; set; } = true;

    public bool EnableConsoleExporter { get; set; }

    public double SamplingRatio { get; set; } = 1;

    public InstrumentationOptions Instrumentation { get; set; } = new();

    public LoggingOptions Logging { get; set; } = new();
}

public sealed class InstrumentationOptions
{
    public bool AspNetCore { get; set; } = true;

    public bool HttpClient { get; set; } = true;

    public bool SqlClient { get; set; } = true;

    public bool Runtime { get; set; } = true;

    public bool Process { get; set; } = true;

    public bool EfCore { get; set; } = true;
}

public sealed class LoggingOptions
{
    public bool Enabled { get; set; } = true;

    public string MinimumLevel { get; set; } = "Information";

    public bool Console { get; set; } = true;

    public bool JsonConsole { get; set; } = true;

    /// <summary>
    /// When true, Serilog exports logs via OTLP to <see cref="ObservabilityOptions.OtlpEndpoint"/>
    /// (Elasticsearch / collector log index).
    /// </summary>
    public bool ExportToOtlp { get; set; } = true;
}
