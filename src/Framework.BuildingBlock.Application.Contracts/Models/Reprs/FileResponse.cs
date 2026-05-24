namespace Framework.BuildingBlock.Application.Contracts;

public class FileResponse
{
    public byte[] Content { get; set; } = null!;
    public string ContentType { get; set; } = "application/octet-stream";
    public string FileName { get; set; } = "download.bin";
}