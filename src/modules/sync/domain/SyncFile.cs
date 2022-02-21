namespace azsync;

public class SyncFile
{
    private SyncFile() { }
    public SyncFile(string name, string localFilePath, string localFilePathHash, DateTime lastModified, long fileSizeInBytes, int containerId, int localPathId)
    {
        Name = name;
        LocalFilePath = localFilePath;
        LocalFilePathHash = localFilePathHash;
        LastModified = lastModified;
        FileSizeInBytes = fileSizeInBytes;
        ContainerId = containerId;
        LocalPathId = localPathId;
        State = "Tracked";
    }

    public int Id { get; set; }
    public int ContainerId { get; set; }
    public int LocalPathId { get; set; }
    public string Name { get; set; } = "";
    public string LocalFilePath { get; set; } = "";
    public string LocalFilePathHash { get; set; } = "";
    public string? RemoteFilePath { get; set; }
    public DateTime LastModified { get; set; }
    public DateTimeOffset? LastUpload { get; set; }
    public long FileSizeInBytes { get; set; }
    public string ContentHash { get; set; } = "";
    public string State { get; set; } = "";

    public void SetContentHash(string contentHash) => ContentHash = contentHash;

    public void Upload(string blobContentHash, DateTimeOffset now)
    {
        if (string.Equals(blobContentHash, ContentHash, StringComparison.OrdinalIgnoreCase) is false)
            State = "Error";

        else
        {
            State = "Uploaded";
            LastUpload = now;
        }
    }

    public void Error()
    {
        State = "Error";
    }

    public void NotFound()
    {
        State = "NotFound";
    }
}