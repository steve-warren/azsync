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
    public string Name { get; set; } = "";
    public string LocalFilePath { get; set; } = "";
    public string LocalFilePathHash { get; set; } = "";
    public string? RemoteFilePath { get; set; }
    public DateTime LastModified { get; set; }
    public long FileSizeInBytes { get; set; }
    public string ContentHash { get; set; } = "";
    public string State { get; set; } = "";

    public void SetContentHash(string contentHash) => ContentHash = contentHash;

    public void Upload(string blobContentHash)
    {
        if (string.Equals(blobContentHash, ContentHash, StringComparison.OrdinalIgnoreCase) is false)
            State = "BadHash";

        else        
            State = "Uploaded";
    }

    public void NotFound()
    {
        State = "NotFound";
    }
}