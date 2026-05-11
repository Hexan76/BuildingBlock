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
        string providerKey)
    {
        var url =
            $"/api/permissions/granted?providerName={providerName}&providerKey={providerKey}";

        var result =
            await _httpClient.GetFromJsonAsync<List<string>>(url);

        return result ?? new List<string>();
    }
}
