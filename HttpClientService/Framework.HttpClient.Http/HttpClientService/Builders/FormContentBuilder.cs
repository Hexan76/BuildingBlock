using Microsoft.Extensions.Options;

using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Framework.HttpClient.Http;

public class FormContentBuilder(IOptions<HttpClientServiceOptions> options) : IFormContentBuilder
{
    private readonly HttpClientServiceOptions _httpClientServiceOptions = options.Value;

    public HttpContent Build(object bodyContent, string contentType)
    {
        if (string.Equals(
                contentType,
                "multipart/form-data",
                StringComparison.OrdinalIgnoreCase))
        {
            return BuildMultipartContent(bodyContent);
        }

        var json = JsonSerializer.Serialize(
            bodyContent,
            _httpClientServiceOptions.JsonSerializerOptions);

        return new StringContent(
            json,
            Encoding.UTF8,
            contentType);
    }


    private static MultipartFormDataContent BuildMultipartContent(object bodyContent)
    {
        var formData = new MultipartFormDataContent();

        var properties = bodyContent
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead);


        foreach (var property in properties)
        {
            var value = property.GetValue(bodyContent);

            if (value == null)
                continue;

            var key = property.Name;

            switch (value)
            {
                case Stream stream:
                    formData.Add(
                        new StreamContent(stream),
                        key,
                        "file");
                    break;


                case byte[] bytes:
                    formData.Add(
                        new ByteArrayContent(bytes),
                        key,
                        "file");
                    break;


                default:
                    formData.Add(
                        new StringContent(value.ToString() ?? string.Empty),
                        key);
                    break;
            }
        }

        return formData;
    }
}
