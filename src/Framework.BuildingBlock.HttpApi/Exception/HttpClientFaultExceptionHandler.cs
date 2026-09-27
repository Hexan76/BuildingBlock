using Framework.BuildingBlock.Domain.Shared;
using Framework.HttpClient.Abstractions;
using Framework.HttpClient.Abstractions.Exceptions;

using Volo.Abp.DependencyInjection;

namespace Framework.BuildingBlock.HttpApi;

[ExposeServices(typeof(IHashtExceptionHandler))]
public class HttpClientFaultExceptionHandler : IHashtExceptionHandler, ITransientDependency
{
    public bool CanHandle(Exception ex) => ex is TransportException;

    public FrameworkRemoteErrorInfoDto Handle(
        Exception ex,
        bool sendExceptionsDetailsToClients,
        bool sendStackTraceToClients)
    {
        return ex switch
        {
            HttpClientFaultException fault => HandleFault(fault, sendExceptionsDetailsToClients, sendStackTraceToClients),
            TransportTimeoutException timeout => Create(
                timeout.Message,
                BuildingBlockErrorCodes.HttpClientTimeout,
                timeout,
                sendExceptionsDetailsToClients,
                sendStackTraceToClients),
            TransportSerializationException serialization => Create(
                serialization.Message,
                BuildingBlockErrorCodes.HttpClientSerialization,
                serialization,
                sendExceptionsDetailsToClients,
                sendStackTraceToClients),
            _ => Create(
                ex.Message,
                BuildingBlockErrorCodes.HttpClientTransport,
                ex,
                sendExceptionsDetailsToClients,
                sendStackTraceToClients)
        };
    }

    private static FrameworkRemoteErrorInfoDto HandleFault(
        HttpClientFaultException exception,
        bool sendDetails,
        bool sendStackTrace)
    {
        var fault = exception.Fault;
        var upstream = fault.UpstreamResult;
        var messages = CollectMessages(exception);
        var message = messages.FirstOrDefault() ?? exception.Message;
        var extra = messages.Count > 1 ? string.Join("\n", messages.Skip(1)) : null;
        var details = sendDetails
            ? JoinDetails(extra, BuildDebugDetails(fault, exception, sendStackTrace))
            : extra;

        var code = !string.IsNullOrWhiteSpace(upstream?.ApplicationCode) && upstream.ApplicationCode != "500"
            ? upstream.ApplicationCode
            : fault.StatusCode.ToString();

        return new FrameworkRemoteErrorInfoDto(message, details, code);
    }

    private static List<string> CollectMessages(HttpClientFaultException exception)
    {
        var upstream = exception.Fault.UpstreamResult;
        var messages = new List<string>();

        AddIfNotEmpty(messages, upstream?.Error?.Message);
        if (upstream?.Errors is not null)
            messages.AddRange(upstream.Errors.Where(e => !string.IsNullOrWhiteSpace(e)));
        if (upstream?.Messages is not null)
            messages.AddRange(upstream.Messages.Where(e => !string.IsNullOrWhiteSpace(e)));
        if (upstream?.Validations is not null)
        {
            messages.AddRange(upstream.Validations
                .Where(v => !string.IsNullOrWhiteSpace(v.ErrorMessage))
                .Select(v => string.IsNullOrWhiteSpace(v.PropertyName)
                    ? v.ErrorMessage
                    : $"{v.PropertyName}: {v.ErrorMessage}"));
        }

        AddIfNotEmpty(messages, upstream?.Snackbar?.Message);

        if (messages.Count == 0)
            messages.Add(exception.Message);

        return messages
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static void AddIfNotEmpty(List<string> target, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            target.Add(value);
    }

    private static string BuildDebugDetails(HttpClientFault fault, Exception exception, bool sendStackTrace)
    {
        var details =
            $"Client={fault.ClientName}; {fault.Method} {fault.RequestUri} -> {fault.StatusCode}; " +
            $"Correlation={fault.CorrelationId ?? "-"}; Duration={(int)fault.Duration.TotalMilliseconds}ms";

        if (!string.IsNullOrWhiteSpace(fault.ResponseBody))
            details += $"{Environment.NewLine}Body={fault.ResponseBody}";

        if (sendStackTrace && exception.StackTrace is not null)
            details += $"{Environment.NewLine}{exception.StackTrace}";

        return details;
    }

    private static string? JoinDetails(string? first, string? second)
    {
        if (string.IsNullOrWhiteSpace(first))
            return second;
        if (string.IsNullOrWhiteSpace(second))
            return first;
        return first + Environment.NewLine + second;
    }

    private static FrameworkRemoteErrorInfoDto Create(
        string message,
        string code,
        Exception ex,
        bool sendDetails,
        bool sendStackTrace)
    {
        string? details = null;
        if (sendDetails)
        {
            details = sendStackTrace ? ex.ToString() : ex.Message;
            if (ex is TransportException transport)
            {
                details =
                    $"Client={transport.ClientName ?? "-"}; Correlation={transport.CorrelationId ?? "-"}" +
                    Environment.NewLine +
                    details;
            }
        }

        return new FrameworkRemoteErrorInfoDto(message, details, code);
    }
}
