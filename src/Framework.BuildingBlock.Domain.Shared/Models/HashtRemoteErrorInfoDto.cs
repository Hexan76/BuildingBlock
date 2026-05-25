using System.Collections;

namespace Framework.BuildingBlock.Domain.Shared;

public class HashtRemoteErrorInfoDto
{
    public string? Code { get; set; }

    /// <summary>
    /// Error message.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Error details.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Error data.
    /// </summary>
    public IDictionary? Data { get; set; }


    /// <summary>
    /// Creates a new instance of <see cref="RemoteServiceErrorInfo"/>.
    /// </summary>
    public HashtRemoteErrorInfoDto()
    {

    }

    /// <summary>
    /// Creates a new instance of <see cref="RemoteServiceErrorInfo"/>.
    /// </summary>
    /// <param name="code">Error code</param>
    /// <param name="details">Error details</param>
    /// <param name="message">Error message</param>
    /// <param name="data">Error data</param>
    public HashtRemoteErrorInfoDto(string message, string? details = null, string? code = null, IDictionary? data = null)
    {
        Message = message;
        Details = details;
        Code = code;
        Data = data;
    }

}

