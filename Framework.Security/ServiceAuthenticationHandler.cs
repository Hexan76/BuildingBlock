using System.Net.Http.Headers;
using Framework.Security.Abstraction;

namespace Framework.Security;

public class ServiceAuthenticationHandler
    : DelegatingHandler
{
    private readonly ITokenProvider _tokenProvider;

    public ServiceAuthenticationHandler(
        ITokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    protected override async Task<HttpResponseMessage>
        SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
    {
        var token =
            await _tokenProvider.GetTokenAsync(
                "internal-service",
                ["internal-api"]);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        return await base.SendAsync(
            request,
            cancellationToken);
    }
}
