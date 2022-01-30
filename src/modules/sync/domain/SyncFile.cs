namespace azsync;

public class SyncFile
{
    private SyncFile() { }
    public SyncFile(string name, string localFilePath, string localFilePathHash, DateTime lastModified, long fileSizeInBytes)
    {
        Name = name;
        LocalFilePath = localFilePath;
        LocalFilePathHash = localFilePathHash;
        LastModified = lastModified;
        FileSizeInBytes = fileSizeInBytes;
        State = "Tracked";
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public string LocalFilePath { get; set; }
    public string LocalFilePathHash { get; set; }
    public string? RemoteFilePath { get; set; }
    public DateTime LastModified { get; set; }
    public long FileSizeInBytes { get; set; }
    public string State { get; set; }
}