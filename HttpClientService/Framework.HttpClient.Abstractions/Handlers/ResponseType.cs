namespace Framework.HttpClient.Abstractions;

public enum ResponseType
{
    /// <summary>Deserialize an envelope and return its Result payload.</summary>
    Default = 0,
    UnwrapEnvelope = 0,

    /// <summary>Deserialize the full response body into TResponse (typically an envelope).</summary>
    Full = 1,
    Envelope = 1,

    /// <summary>Deserialize the response body directly as TResponse.</summary>
    Custom = 2,
    Raw = 2
}
