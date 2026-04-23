using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Framework.BuildingBlock.Domain;

public static class CompressionHelper
{
    public static byte[] CompressToZip(byte[] input, string entryName = null)
    {
        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            var entry = archive.CreateEntry(entryName, CompressionLevel.SmallestSize);
            using var es = entry.Open();
            es.Write(input, 0, input.Length);
        }
        ms.Position = 0;
        return ms.ToArray();
    }

    public static byte[] DecompressFromZip(byte[] zipData, string? entryName = null)
    {
        using var ms = new MemoryStream(zipData);
        using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
        ZipArchiveEntry? entry;
        if (string.IsNullOrEmpty(entryName))
            entry = archive.Entries.FirstOrDefault();
        else
            entry = archive.GetEntry(entryName);

        if (entry == null) return Array.Empty<byte>();

        using var es = entry.Open();
        using var outMs = new MemoryStream();
        es.CopyTo(outMs);
        return outMs.ToArray();
    }

    public static byte[] CompressGzip(byte[] data)
    {
        using var ms = new MemoryStream();
        using (var gz = new GZipStream(ms, CompressionLevel.SmallestSize, true))
        {
            gz.Write(data, 0, data.Length);
        }
        ms.Position = 0;
        return ms.ToArray();
    }

    public static byte[] DecompressGzip(byte[] gzipped)
    {
        using var inMs = new MemoryStream(gzipped);
        using var gz = new GZipStream(inMs, CompressionMode.Decompress);
        using var outMs = new MemoryStream();
        gz.CopyTo(outMs);
        return outMs.ToArray();
    }
}
