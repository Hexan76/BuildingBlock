using System.Net.Http.Json;

namespace Framework.Security;

public class PermissionClient : IPermissionClient
{
    private readonly HttpClient _httpClient;

    public PermissionClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<string>> GetGrantedPermissionsAsync(
        string providerName,
        string providerKey,
        CancellationToken cancellationToken = default)
    {
        var url =
            $"/api/permissions/granted?providerName={Uri.EscapeDataString(providerName)}&providerKey={Uri.EscapeDataString(providerKey)}";

        try
        {
            var result =
                await _httpClient.GetFromJsonAsync<List<string>>(
                    url,
                    cancellationToken);

            return result ?? new List<string>();
        }
        catch (HttpRequestException)
        {
            // fallback safe mode
            return new List<string>();
        }
    }

    public async Task<List<PermissionDefinitionDto>> GetDefinitionsAsync(
        CancellationToken cancellationToken = default)
    {
        var url = "/api/permissions/definitions";

        try
        {
            var result =
                await _httpClient.GetFromJsonAsync<List<PermissionDefinitionDto>>(
                    url,
                    cancellationToken);

            return result ?? new List<PermissionDefinitionDto>();
        }
        catch (HttpRequestException)
        {
            return new List<PermissionDefinitionDto>();
        }
    }
}
