namespace Framework.BuildingBlock.Domain;

public class BackupOptions
{
    /// <summary>
    /// Root folder where backups will be stored.
    /// </summary>
    public string RootPath { get; set; } = @"C:\Backups";

    /// <summary>
    /// Whether to compress backups using SQL Server compression.
    /// </summary>
    public bool UseCompression { get; set; } = true;
}
