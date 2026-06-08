using Framework.HttpClient.Abstractions;

namespace Framework.HttpClient.Http;

public class RequestInterceptorHandler(IHttpInterceptorService interceptorService) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            // Invoke before-send event
            interceptorService.OnBeforeSend(request);

            // Send request down the pipeline
            var response = await base.SendAsync(request, cancellationToken);

            // Invoke after-send event
            interceptorService.OnAfterSend(response);

            return response;
        }
        catch (Exception ex)
        {
            // Handle exceptions globally via the service
            interceptorService.OnException(ex);
            throw; // Ensure the exception propagates if needed
        }
    }
}
