using System.Net.Http.Json;

using Microsoft.Extensions.Configuration;

using Volo.Abp.DependencyInjection;

namespace Framework.Security;

public class PermissionClient : IPermissionClient , ITransientDependency
{
    private readonly HttpClient _httpClient;

    private string BaseUrl;

    public PermissionClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        BaseUrl = configuration["PermissionService:BaseUrl"];
    }

    public async Task<List<string>> GetGrantedPermissionsAsync(
        string providerName,
        string providerKey,
        CancellationToken cancellationToken = default)
    {
        var url =
            $"/v1/api/permissions/granted?providerName={Uri.EscapeDataString(providerName)}&providerKey={Uri.EscapeDataString(providerKey)}";

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

    public async Task<object> SyncDefinitions(
        string serviceName,
        string payload,
        string signature,
        CancellationToken cancellationToken = default)
    {
        var url = $"{BaseUrl}/v1/api/permission-management/sync";

        var request = new DefinitionPermissionsCreate
        {
            ServiceName = serviceName,
            Payload = payload,
            Signature = signature
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                url,
                request,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            return response.Content;
        }
        catch (HttpRequestException)
        {
            throw;
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

public class DefinitionPermissionsCreate
{
    public string ServiceName { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public string Signature { get; set; } = default!;
}
