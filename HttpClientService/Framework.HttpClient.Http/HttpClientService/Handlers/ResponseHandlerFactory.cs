using Framework.HttpClient.Abstractions;
using Microsoft.Extensions.Options;

namespace Framework.HttpClient.Http;

public class ResponseHandlerFactory(
    IOptions<HttpClientServiceOptions> options)
    : IResponseHandlerFactory
{
    private readonly HttpClientServiceOptions _options = options.Value;

    public IResponseHandler GetHandlerFor(
        ResponseType responseType,
        Type responseModelType)
    {
        return responseType switch
        {

            ResponseType.Default =>
                (IResponseHandler)Activator.CreateInstance(
                    typeof(AcceptedResponseHandler<>)
                        .MakeGenericType(responseModelType),
                    _options.JsonSerializerOptions)!,

            ResponseType.Full =>
                new MessageContractHandler(
                    _options.JsonSerializerOptions),

            ResponseType.Custom =>
                new DefaultResponseHandler(
                    _options.JsonSerializerOptions),

            _ => throw new ArgumentOutOfRangeException(
                nameof(responseType),
                responseType,
                null)
        };
    }
}