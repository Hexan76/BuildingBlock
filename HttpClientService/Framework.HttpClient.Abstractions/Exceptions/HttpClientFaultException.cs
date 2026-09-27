using System.Text;

using Framework.HttpClient.Abstractions;

namespace Framework.HttpClient.Abstractions.Exceptions;

public sealed class HttpClientFaultException : TransportException
{
    public HttpClientFault Fault { get; }

    public HttpClientFaultException(HttpClientFault fault)
        : base(BuildMessage(fault), fault.ClientName, fault.CorrelationId)
    {
        Fault = fault;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append(GetType().FullName);
        builder.Append(": ");
        builder.AppendLine(Message);
        builder.Append("Client=").Append(Fault.ClientName);
        builder.Append("; ").Append(Fault.Method).Append(' ').Append(Fault.RequestUri);
        builder.Append(" -> ").Append(Fault.StatusCode);
        builder.Append(" (public ").Append(Fault.PublicStatusCode).Append(')');
        builder.Append("; Correlation=").Append(Fault.CorrelationId ?? "-");
        builder.Append("; Duration=").Append(Fault.Duration.TotalMilliseconds.ToString("0")).Append("ms");
        builder.AppendLine();

        if (!string.IsNullOrWhiteSpace(Fault.ResponseBody))
        {
            builder.Append("Body=").AppendLine(Fault.ResponseBody);
        }

        if (InnerException is not null)
        {
            builder.Append(" ---> ").AppendLine(InnerException.ToString());
        }

        if (StackTrace is not null)
        {
            builder.AppendLine(StackTrace);
        }

        return builder.ToString();
    }

    private static string BuildMessage(HttpClientFault fault)
    {
        var upstream = fault.UpstreamResult;
        return upstream?.Error?.Message
            ?? FirstNonEmpty(upstream?.Errors)
            ?? upstream?.Snackbar?.Message
            ?? FirstNonEmpty(upstream?.Messages)
            ?? $"HTTP {fault.StatusCode} from '{fault.ClientName}'.";
    }

    private static string? FirstNonEmpty(string[]? values)
        => values?.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
}
