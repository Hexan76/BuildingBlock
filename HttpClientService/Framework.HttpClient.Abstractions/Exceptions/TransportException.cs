namespace Framework.HttpClient.Abstractions.Exceptions;

public class TransportException : Exception
{
    public string? ClientName { get; }

    public string? CorrelationId { get; }

    public TransportException(string message)
        : base(message)
    {
    }

    public TransportException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public TransportException(
        string message,
        string? clientName,
        string? correlationId,
        Exception? innerException = null)
        : base(message, innerException)
    {
        ClientName = clientName;
        CorrelationId = correlationId;
    }
}
