namespace Framework.HttpClient.Abstractions.Exceptions;

public sealed class TransportTimeoutException : TransportException
{
    public TransportTimeoutException(string message)
        : base(message)
    {
    }

    public TransportTimeoutException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public TransportTimeoutException(
        string message,
        string? clientName,
        string? correlationId,
        Exception? innerException = null)
        : base(message, clientName, correlationId, innerException)
    {
    }
}
