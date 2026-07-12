using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Framework.Observability;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(ObservabilityOptions.SectionName)
            .Get<ObservabilityOptions>() ?? new();

        services.Configure<ObservabilityOptions>(
            configuration.GetSection(ObservabilityOptions.SectionName));

        if (!options.Enabled)
            return services;

        ConfigureSerilog(configuration, options);

        services.AddLogging(logging =>
        {
            logging.ClearProviders();

            if (options.Logging.Enabled)
            {
                logging.AddSerilog(Log.Logger);
            }

            logging.AddOpenTelemetry(otel =>
            {
                otel.SetResourceBuilder(CreateResourceBuilder(options));

                otel.AddOtlpExporter(exporter =>
                {
                    exporter.Endpoint = new Uri(options.OtlpEndpoint);
                });

                if (options.EnableConsoleExporter)
                {
                    otel.AddConsoleExporter();
                }
            });
        });

        var resourceBuilder = CreateResourceBuilder(options);

        services.AddOpenTelemetry()

            .ConfigureResource(resource =>
            {
                resource.AddService(
                        serviceName: options.ServiceName,
                        serviceNamespace: options.ServiceNamespace,
                        serviceVersion: options.ServiceVersion,
                        serviceInstanceId: Environment.MachineName)
                    .AddAttributes(new Dictionary<string, object>
                    {
                        ["deployment.environment"] = options.Environment,
                        ["host.name"] = Environment.MachineName
                    });
            })

            .WithTracing(tracing =>
            {
                tracing.SetSampler(
                    options.SamplingRatio >= 1
                        ? new AlwaysOnSampler()
                        : new TraceIdRatioBasedSampler(options.SamplingRatio));

                if (options.Instrumentation.AspNetCore)
                    tracing.AddAspNetCoreInstrumentation();

                if (options.Instrumentation.HttpClient)
                    tracing.AddHttpClientInstrumentation();

                if (options.Instrumentation.SqlClient)
                    tracing.AddSqlClientInstrumentation();

                tracing.AddSource(options.ServiceName);

                tracing.AddOtlpExporter(exporter =>
                {
                    exporter.Endpoint = new Uri(options.OtlpEndpoint);
                });

                if (options.EnableConsoleExporter)
                {
                    tracing.AddConsoleExporter();
                }
            })

            .WithMetrics(metrics =>
            {
                if (options.Instrumentation.AspNetCore)
                    metrics.AddAspNetCoreInstrumentation();

                if (options.Instrumentation.HttpClient)
                    metrics.AddHttpClientInstrumentation();

                if (options.Instrumentation.Runtime)
                    metrics.AddRuntimeInstrumentation();

                metrics.AddMeter(options.ServiceName);

                metrics.AddOtlpExporter(exporter =>
                {
                    exporter.Endpoint = new Uri(options.OtlpEndpoint);
                });

                if (options.EnableConsoleExporter)
                {
                    metrics.AddConsoleExporter();
                }
            });

        return services;
    }

    private static ResourceBuilder CreateResourceBuilder(
        ObservabilityOptions options)
    {
        return ResourceBuilder.CreateDefault()
            .AddService(
                serviceName: options.ServiceName,
                serviceNamespace: options.ServiceNamespace,
                serviceVersion: options.ServiceVersion,
                serviceInstanceId: Environment.MachineName)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = options.Environment,
                ["host.name"] = Environment.MachineName
            });
    }

    private static void ConfigureSerilog(
        IConfiguration configuration,
        ObservabilityOptions options)
    {
        if (!options.Logging.Enabled)
            return;

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .MinimumLevel.Is(ParseLevel(options.Logging.MinimumLevel));

        if (options.Logging.Console)
        {
            if (options.Logging.JsonConsole)
            {
                logger.WriteTo.Console(new RenderedCompactJsonFormatter());
            }
            else
            {
                logger.WriteTo.Console();
            }
        }

        Log.Logger = logger.CreateLogger();
    }

    private static LogEventLevel ParseLevel(string level)
    {
        return Enum.TryParse(level, true, out LogEventLevel result)
            ? result
            : LogEventLevel.Information;
    }
}
