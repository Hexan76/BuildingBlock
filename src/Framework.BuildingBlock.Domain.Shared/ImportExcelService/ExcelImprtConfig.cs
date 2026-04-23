using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace Framework.BuildingBlock.Domain.Shared;

public class ExcelImportConfig
{
    public string Name { get; set; } = string.Empty;
    public List<ColumnMapping> Columns { get; set; } = new();
}

public static class ExcelImportConfigExtensions
{

    public static string SerializeImportConfig(this ExcelImportConfig config)
    {
        var json = JsonSerializer.Serialize(config);

        using var input = new MemoryStream(Encoding.UTF8.GetBytes(json));
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.Optimal))
        {
            input.CopyTo(gzip);
        }

        return Convert.ToBase64String(output.ToArray());
    }
    public static ExcelImportConfig DeserializeImportConfig(this ExcelImportConfig config, string base64)
    {
        using var stream = new MemoryStream(Convert.FromBase64String(base64));
        using var gZipStream = new GZipStream(stream, CompressionMode.Decompress);
        using var memoryStream = new MemoryStream();
        gZipStream.CopyTo(memoryStream);

        var deserialized = JsonSerializer.Deserialize<ExcelImportConfig>(Encoding.UTF8.GetString(memoryStream.ToArray()));
        return deserialized ?? new ExcelImportConfig();
    }


}