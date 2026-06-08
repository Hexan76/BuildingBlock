//using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Framework.HttpClient.Http;

public class FormContentBuilder(IOptions<HttpClientServiceOptions> options) : IFormContentBuilder
{
    private HttpClientServiceOptions _httpClientServiceOptions = options.Value;

    public HttpContent Build(IDictionary<string, object> bodyContent, string contentType)
    {
        if (string.Equals(contentType, "multipart/form-data", StringComparison.OrdinalIgnoreCase))
        {
            var formData = new MultipartFormDataContent();

            foreach (var (key, value) in bodyContent)
            {
                switch (value)
                {
                    case Stream stream:
                        formData.Add(new StreamContent(stream), key, "file");
                        break;

                    case byte[] bytes:
                        formData.Add(new ByteArrayContent(bytes), key, "file");
                        break;

                    default:
                        formData.Add(new StringContent(value.ToString() ?? ""), key);
                        break;
                }
            }

            return formData;
        }

        // Fallback to JSON
        var json = JsonSerializer.Serialize(bodyContent, _httpClientServiceOptions.JsonSerializerOptions);
        return new StringContent(json, Encoding.UTF8, contentType);
    }
}
