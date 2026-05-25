using System.Net.Http.Json;

using Framework.Security.Abstraction;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

using Volo.Abp.Caching;

namespace Framework.Security;

public class OpenIddictTokenProvider : ITokenProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDistributedCache<TokenCacheItem> _cache;
    private readonly TokenHttpClientOptions _options;

    public OpenIddictTokenProvider(
        IHttpClientFactory httpClientFactory,
        IDistributedCache<TokenCacheItem> cache,
        IOptions<TokenHttpClientOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _options = options.Value;
    }

    public async Task<string> GetTokenAsync(
        string clientId,
        string[] scopes)
    {
        var scopeKey = string.Join("_", scopes);

        var cacheKey =
            $"framework_security_token:{clientId}:{scopeKey}";

        var cached =
            await _cache.GetAsync(cacheKey);

        if (cached != null &&
            cached.Expiration > DateTime.UtcNow.AddMinutes(1))
        {
            return cached.AccessToken;
        }

        var client =
            _httpClientFactory.CreateClient(
                FrameworkSecurityConstants.HttpClientName);

        var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                _options.TokenEndpoint);

        request.Content =
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>(
                    "grant_type",
                    "client_credentials"),

                new KeyValuePair<string, string>(
                    "client_id",
                    clientId),

                new KeyValuePair<string, string>(
                    "scope",
                    string.Join(" ", scopes))
            ]);

        var response =
            await client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var tokenResponse =
            await response.Content
                .ReadFromJsonAsync<
                    OpenIddictTokenResponse>();

        if (tokenResponse == null)
        {
            throw new Exception(
                "OpenIddict token response is null.");
        }

        var expiration =
            DateTime.UtcNow
                .AddSeconds(tokenResponse.ExpiresIn);

        var cacheItem =
            new TokenCacheItem
            {
                AccessToken =
                    tokenResponse.AccessToken,

                Expiration = expiration
            };

        await _cache.SetAsync(
            cacheKey,
            cacheItem,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = expiration
            });

        return tokenResponse.AccessToken;
    }
}
