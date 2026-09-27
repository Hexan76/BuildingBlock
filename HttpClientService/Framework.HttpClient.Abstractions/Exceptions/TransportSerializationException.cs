namespace Framework.HttpClient.Abstractions.Exceptions;

public sealed class TransportSerializationException : TransportException
{
    public TransportSerializationException(string message)
        : base(message)
    {
    }

    public TransportSerializationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public TransportSerializationException(
        string message,
        string? clientName,
        string? correlationId,
        Exception? innerException = null)
        : base(message, clientName, correlationId, innerException)
    {
    }
}
